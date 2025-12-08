using System;
using System.Linq;
using Xunit;
using SlotMathModule.GameLogic.Core;
using SlotMathModule.GameLogic.Games;

namespace SlotMathModule.Tests
{
    /// <summary>
    /// Тесты для базового класса Slot
    /// </summary>
    public class SlotTests
    {
        private Slot CreateSlot()
        {
            var slot = new Slot();
            slot.LoadGame("Slot5_3");
            return slot;
        }

        private FeedExctractParam CreateFeedParams()
        {
            return new FeedExctractParam
            {
                FeedKey = 1000,
                FeedNumber = 1000000,
                TicketNumber = 1000000,
                SegmentNumber = 0,
                GameIndex = 0,
                TicketCount = 1100,
                Nominal = 20,
                Percent = 95,
                PercentBonus = 30,
                BonusGameMin = 30,
                BonusGameMax = 500
            };
        }

        [Fact]
        public void Slot_Constructor_InitializesCorrectly()
        {
            // Arrange & Act
            var slot = new Slot();

            // Assert
            Assert.NotNull(slot);
            Assert.Equal(1, slot.Bet);
            Assert.Equal(1, slot.Lines);
            Assert.Equal(0, slot.Credits);
            Assert.Equal(0, slot.SpinCount);
        }

        [Fact]
        public void Slot_LoadGame_LoadsGameCorrectly()
        {
            // Arrange
            var slot = new Slot();

            // Act
            int gameId = slot.LoadGame("Slot5_3");

            // Assert
            Assert.NotNull(slot.Game);
            Assert.Equal("Slot5_3", slot.Game.Name);
            Assert.True(gameId > 0);
        }

        [Fact]
        public void Slot_ExtractFeed_InitializesFeed()
        {
            // Arrange
            var slot = CreateSlot();
            var feedParams = CreateFeedParams();

            // Act
            slot.ExtractFeed(feedParams);

            // Assert
            Assert.NotNull(slot.Feed);
            Assert.Equal(feedParams.FeedNumber, slot.Feed.FeedNumber);
            Assert.Equal(feedParams.Percent, slot.Feed.Percent);
        }

        [Fact]
        public void Slot_DoCashIn_IncreasesCredits()
        {
            // Arrange
            var slot = CreateSlot();
            int amount = 1000;

            // Act
            slot.DoCashIn(amount);

            // Assert
            Assert.Equal(amount, slot.Credits);
            Assert.Equal(amount, slot.CashIn);
        }

        [Fact]
        public void Slot_DoCashOut_ResetsCredits()
        {
            // Arrange
            var slot = CreateSlot();
            slot.DoCashIn(1000);

            // Act
            slot.DoCashOut();

            // Assert
            Assert.Equal(0, slot.Credits);
            Assert.Equal(1000, slot.CashOut);
        }

        [Fact]
        public void Slot_Spin_RequiresCredits()
        {
            // Arrange
            var slot = CreateSlot();
            var feedParams = CreateFeedParams();
            slot.ExtractFeed(feedParams);
            slot.Bet = 10;
            slot.Lines = 1;

            // Act
            int result = slot.Spin(10, 1);

            // Assert
            Assert.Equal(302, result); // Недостаточно кредитов
        }

        [Fact]
        public void Slot_Spin_WorksWithCredits()
        {
            // Arrange
            var slot = CreateSlot();
            var feedParams = CreateFeedParams();
            slot.ExtractFeed(feedParams);
            slot.DoCashIn(10000);
            slot.Bet = 10;
            slot.Lines = 1;

            // Act
            int result = slot.Spin(10, 1);

            // Assert
            Assert.Equal(0, result); // Успешный спин
            Assert.True(slot.SpinCount > 0);
            Assert.NotNull(slot.Map);
        }

        [Fact]
        public void Slot_Spin_DeductsBetLine()
        {
            // Arrange
            var slot = CreateSlot();
            var feedParams = CreateFeedParams();
            slot.ExtractFeed(feedParams);
            slot.DoCashIn(10000);
            slot.Bet = 10;
            slot.Lines = 3;
            int initialCredits = slot.Credits;
            int expectedBet = slot.BetLine; // 10 * 3 = 30

            // Act
            slot.Spin(10, 3);

            // Assert
            // Кредиты должны уменьшиться на ставку (если не бонусная игра)
            if (!slot.BonusGame || slot.BonusGameRemain == 0)
            {
                Assert.True(slot.Credits <= initialCredits - expectedBet + slot.CurWin,
                    $"Кредиты должны уменьшиться на ставку. Было: {initialCredits}, Стало: {slot.Credits}, Ставка: {expectedBet}");
            }
        }

        [Fact]
        public void Slot_Spin_IncreasesSpinCount()
        {
            // Arrange
            var slot = CreateSlot();
            var feedParams = CreateFeedParams();
            slot.ExtractFeed(feedParams);
            slot.DoCashIn(100000);
            int initialSpinCount = slot.SpinCount;

            // Act
            slot.Spin(10, 1);

            // Assert
            Assert.Equal(initialSpinCount + 1, slot.SpinCount);
        }

        [Fact]
        public void Slot_NextBet_ChangesBet()
        {
            // Arrange
            var slot = CreateSlot();
            slot.Bet = 1;

            // Act & Assert
            slot.NextBet();
            Assert.Equal(2, slot.Bet);

            slot.NextBet();
            Assert.Equal(5, slot.Bet);

            slot.NextBet();
            Assert.Equal(10, slot.Bet);

            slot.NextBet();
            Assert.Equal(20, slot.Bet);

            slot.NextBet();
            Assert.Equal(50, slot.Bet);

            slot.NextBet();
            Assert.Equal(100, slot.Bet);

            slot.NextBet();
            Assert.Equal(1, slot.Bet); // Циклически возвращается к 1
        }

        [Fact]
        public void Slot_Reset_ResetsStatistics()
        {
            // Arrange
            var slot = CreateSlot();
            slot.DoCashIn(1000);
            slot.SpinCount = 100;
            slot.CashIn = 5000;
            slot.CashOut = 3000;

            // Act
            slot.Reset();

            // Assert
            Assert.Equal(0, slot.CashIn);
            Assert.Equal(0, slot.CashOut);
            Assert.Equal(0, slot.CreditIn);
            Assert.Equal(0, slot.CreditOut);
            Assert.Equal(0, slot.SpinCount);
            Assert.Equal(1, slot.Bet);
            Assert.Equal(1, slot.Lines);
        }

        [Fact]
        public void Slot_BetLine_CalculatedCorrectly()
        {
            // Arrange
            var slot = CreateSlot();
            slot.Bet = 10;
            slot.Lines = 5;

            // Act
            int betLine = slot.BetLine;

            // Assert
            Assert.Equal(50, betLine); // 10 * 5
        }

        [Fact]
        public void Slot_MultipleSpins_AccumulateStatistics()
        {
            // Arrange
            var slot = CreateSlot();
            var feedParams = CreateFeedParams();
            slot.ExtractFeed(feedParams);
            slot.DoCashIn(100000);
            slot.Bet = 10;
            slot.Lines = 1;
            int spinCount = 100;

            // Act
            for (int i = 0; i < spinCount; i++)
            {
                slot.Spin(10, 1);
            }

            // Assert
            Assert.Equal(spinCount, slot.SpinCount);
            Assert.True(slot.CreditIn > 0);
            Assert.True(slot.CreditOut >= 0);
        }

        [Fact]
        public void Slot_RTP_CalculatedCorrectly()
        {
            // Arrange
            var slot = CreateSlot();
            var feedParams = CreateFeedParams();
            slot.ExtractFeed(feedParams);
            slot.DoCashIn(1000000); // Большой баланс для точности
            slot.Bet = 10;
            slot.Lines = 1;
            int spinCount = 10000;

            // Act
            for (int i = 0; i < spinCount; i++)
            {
                slot.Spin(10, 1);
            }

            // Assert
            double rtp = 0;
            if (slot.CreditIn > 0)
            {
                rtp = (double)slot.CreditOut / slot.CreditIn * 100;
            }

            // RTP должен быть близок к заданному проценту (95% в feedParams)
            Assert.True(rtp >= feedParams.Percent - 5 && rtp <= feedParams.Percent + 5,
                $"RTP {rtp:F2}% не соответствует ожидаемому {feedParams.Percent}% (допуск ±5%)");
        }

        [Fact]
        public void Slot_ScratchMode_Works()
        {
            // Arrange
            var slot = CreateSlot();
            var feedParams = CreateFeedParams();
            slot.ExtractFeed(feedParams);
            slot.DoCashIn(10000);
            slot.Bet = 10;
            slot.Lines = 1;

            // Act
            int result = slot.Spin(10, 1, scratch: true);

            // Assert
            Assert.Equal(0, result);
            Assert.True(slot.IsScratch);
        }

        [Fact]
        public void Slot_BuyTicket_Works()
        {
            // Arrange
            var slot = CreateSlot();
            var feedParams = CreateFeedParams();
            slot.ExtractFeed(feedParams);
            long ticketNumber = feedParams.FeedNumber + 100;

            // Act
            int result = slot.BuyTicket(ticketNumber);

            // Assert
            Assert.Equal(0, result);
            Assert.Equal(ticketNumber, slot.CurrentTicket);
        }

        [Fact]
        public void Slot_BuyTicket_RejectsInvalidTicket()
        {
            // Arrange
            var slot = CreateSlot();
            var feedParams = CreateFeedParams();
            slot.ExtractFeed(feedParams);
            long invalidTicket = feedParams.FeedNumber + feedParams.TicketCount + 100; // Вне диапазона

            // Act
            int result = slot.BuyTicket(invalidTicket);

            // Assert
            Assert.Equal(201, result); // Ошибка
        }
    }
}

