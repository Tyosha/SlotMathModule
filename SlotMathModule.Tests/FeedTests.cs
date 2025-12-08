using System;
using System.Linq;
using Xunit;
using SlotMathModule.GameLogic.Core;
using SlotMathModule.GameLogic.Games;

namespace SlotMathModule.Tests
{
    /// <summary>
    /// Тесты для генерации ленты выигрышей (Feed)
    /// </summary>
    public class FeedTests
    {
        private Game GetGame()
        {
            Game.LoadGames();
            return Game.GameByName("Slot5_3");
        }

        [Fact]
        public void Feed_Constructor_InitializesCorrectly()
        {
            // Arrange
            var game = GetGame();

            // Act
            var feed = new Feed(game);

            // Assert
            Assert.NotNull(feed);
            Assert.Equal(game, feed.Game);
        }

        [Fact]
        public void Feed_Extract_GeneratesFeedItems()
        {
            // Arrange
            var game = GetGame();
            var feed = new Feed(game);
            feed.CellCount = 10000;
            feed.Percent = 95;
            feed.PercentBonus = 30;
            int feedKey = 1000;

            // Act
            feed.Extract(feedKey);

            // Assert
            Assert.NotNull(feed.FeedItem);
            Assert.True(feed.FeedItem.Length > 0);
            
            // Проверяем, что для каждого варианта линий есть элементы
            foreach (var feedItems in feed.FeedItem)
            {
                if (feedItems != null)
                {
                    Assert.True(feedItems.Count > 0, "FeedItem должен содержать элементы");
                    Assert.All(feedItems, item => Assert.True(item.Index >= 0 && item.Index < feed.CellCount));
                }
            }
        }

        [Fact]
        public void Feed_Extract_RespectsPercent()
        {
            // Arrange
            var game = GetGame();
            var feed = new Feed(game);
            feed.CellCount = 10000;
            feed.Percent = 90;
            feed.PercentBonus = 30;
            int feedKey = 1000;

            // Act
            feed.Extract(feedKey);

            // Assert
            if (feed.FeedItem[0] != null && feed.FeedItem[0].Count > 0)
            {
                int totalWin = feed.FeedItem[0].Sum(item => item.Win);
                int expectedWin = feed.CellCount * feed.Game.Variants[0] * feed.Percent / 100;
                
                // Сумма выигрышей должна быть близка к ожидаемой (с допуском ±5%)
                double tolerance = expectedWin * 0.05;
                Assert.True(Math.Abs(totalWin - expectedWin) <= tolerance,
                    $"Сумма выигрышей {totalWin} не соответствует ожидаемой {expectedWin} (допуск ±{tolerance:F0})");
            }
        }

        [Fact]
        public void Feed_GetNextFeedItem_ReturnsValidItem()
        {
            // Arrange
            var game = GetGame();
            var feed = new Feed(game);
            feed.CellCount = 10000;
            feed.Percent = 95;
            feed.Extract(1000);
            feed.CurTicket = feed.FeedNumber;
            feed.CurSeqment = 0;
            int lineCount = feed.Game.Variants[0];
            int bet = 1;

            // Act
            var feedItem = feed.GetNextFeedItem(lineCount, bet);

            // Assert
            // FeedItem может быть null (если нет выигрыша на этой позиции)
            if (feedItem != null)
            {
                Assert.True(feedItem.Win >= 0);
                Assert.True(feedItem.Index >= 0);
            }
        }

        [Fact]
        public void Feed_GetNextFeedItem_AdvancesSegment()
        {
            // Arrange
            var game = GetGame();
            var feed = new Feed(game);
            feed.CellCount = 10000;
            feed.Percent = 95;
            feed.Extract(1000);
            feed.CurTicket = feed.FeedNumber;
            feed.CurSeqment = 0;
            int lineCount = feed.Game.Variants[0];
            int bet = 1;
            int initialSegment = feed.CurSeqment;

            // Act
            feed.GetNextFeedItem(lineCount, bet);

            // Assert
            Assert.True(feed.CurSeqment > initialSegment || feed.CurSeqment == 100);
        }

        [Fact]
        public void Feed_GetNextFeedItemNoLines_ReturnsAllVariants()
        {
            // Arrange
            var game = GetGame();
            var feed = new Feed(game);
            feed.CellCount = 10000;
            feed.Percent = 95;
            feed.Extract(1000);
            feed.CurTicket = feed.FeedNumber;
            int segment = 0;

            // Act
            var results = feed.GetNextFeedItemNoLines(segment);

            // Assert
            Assert.NotNull(results);
            Assert.Equal(feed.Game.Variants.Length, results.Count);
        }

        [Fact]
        public void Feed_BuyTicket_SetsTicket()
        {
            // Arrange
            var game = GetGame();
            var feed = new Feed(game);
            feed.FeedNumber = 1000000;
            feed.TicketCount = 1100;
            long ticketNumber = 1000100;

            // Act
            int result = feed.BuyTicket(ticketNumber);

            // Assert
            Assert.Equal(0, result);
            Assert.Equal(ticketNumber, feed.CurTicket);
            Assert.Equal(0, feed.CurSeqment);
        }

        [Fact]
        public void Feed_BuyTicket_RejectsInvalidTicket()
        {
            // Arrange
            var game = GetGame();
            var feed = new Feed(game);
            feed.FeedNumber = 1000000;
            feed.TicketCount = 1100;
            long invalidTicket = feed.FeedNumber + feed.TicketCount + 100;

            // Act
            int result = feed.BuyTicket(invalidTicket);

            // Assert
            Assert.Equal(201, result);
        }

        [Fact]
        public void Feed_Dispersion_Calculated()
        {
            // Arrange
            var game = GetGame();
            var feed = new Feed(game);
            feed.CellCount = 10000;
            feed.Percent = 95;
            feed.Extract(1000);

            // Act
            double dispersion = feed.Dispersion();

            // Assert
            Assert.True(dispersion >= 0, "Дисперсия должна быть неотрицательной");
        }

        [Fact]
        public void Feed_Extract_DistributesWinsEvenly()
        {
            // Arrange
            var game = GetGame();
            var feed = new Feed(game);
            feed.CellCount = 10000;
            feed.Percent = 95;
            feed.Extract(1000);

            // Act & Assert
            if (feed.FeedItem[0] != null)
            {
                // Проверяем, что выигрыши распределены по всей ленте
                var indices = feed.FeedItem[0].Select(item => item.Index).ToList();
                
                int minIndex = indices.Min();
                int maxIndex = indices.Max();
                
                // Выигрыши должны быть распределены по всей длине ленты
                Assert.True(maxIndex - minIndex > feed.CellCount * 0.5,
                    "Выигрыши должны быть распределены по всей ленте");
            }
        }

        [Fact]
        public void Feed_Extract_NoDuplicateIndices()
        {
            // Arrange
            var game = GetGame();
            var feed = new Feed(game);
            feed.CellCount = 10000;
            feed.Percent = 95;
            feed.Extract(1000);

            // Act & Assert
            foreach (var feedItems in feed.FeedItem)
            {
                if (feedItems != null && feedItems.Count > 0)
                {
                    var indices = feedItems.Select(item => item.Index).ToList();
                    var uniqueIndices = indices.Distinct().ToList();
                    
                    // Все индексы должны быть уникальными
                    Assert.Equal(indices.Count, uniqueIndices.Count//, "Индексы выигрышей должны быть уникальными"
                                                                   );
                }
            }
        }

        [Theory]
        [InlineData(80)]
        [InlineData(85)]
        [InlineData(90)]
        [InlineData(95)]
        public void Feed_Extract_RespectsDifferentPercents(int percent)
        {
            // Arrange
            var game = GetGame();
            var feed = new Feed(game);
            feed.CellCount = 10000;
            feed.Percent = percent;
            feed.Extract(1000);

            // Act & Assert
            if (feed.FeedItem[0] != null && feed.FeedItem[0].Count > 0)
            {
                int totalWin = feed.FeedItem[0].Sum(item => item.Win);
                int expectedWin = feed.CellCount * feed.Game.Variants[0] * percent / 100;
                
                double tolerance = expectedWin * 0.1; // 10% допуск
                Assert.True(Math.Abs(totalWin - expectedWin) <= tolerance,
                    $"Для процента {percent}% сумма выигрышей {totalWin} не соответствует ожидаемой {expectedWin}");
            }
        }
    }
}

