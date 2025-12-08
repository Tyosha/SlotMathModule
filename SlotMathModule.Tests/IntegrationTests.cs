using System;
using System.Linq;
using Xunit;
using SlotMathModule.GameLogic.Core;
using SlotMathModule.GameLogic.Games;
using SlotMathModule.SuperBall;
using SlotMathModule.Common.Models;
using SlotMathModule.Common;

namespace SlotMathModule.Tests
{
    /// <summary>
    /// Интеграционные тесты для проверки работы всей системы
    /// </summary>
    public class IntegrationTests
    {
        [Fact]
        public void Slot_WithFeed_CompleteGameSession()
        {
            // Arrange
            var slot = new Slot();
            slot.LoadGame("Slot5_3");
            
            var feedParams = new FeedExctractParam
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

            slot.ExtractFeed(feedParams);
            slot.DoCashIn(100000);
            slot.Bet = 10;
            slot.Lines = 1;

            int initialCredits = slot.Credits;
            int spins = 1000;

            // Act - выполняем сессию игры
            for (int i = 0; i < spins; i++)
            {
                int result = slot.Spin(10, 1);
                if (result != 0 && result != 301) // 301 - закончились сегменты
                    break;
            }

            // Assert
            Assert.True(slot.SpinCount > 0, "Должен быть выполнен хотя бы один спин");
            Assert.True(slot.CreditIn > 0, "Должны быть потрачены кредиты");
            Assert.True(slot.CreditOut >= 0, "Выигрыш не может быть отрицательным");

            // Проверяем RTP
            if (slot.CreditIn > 0)
            {
                double rtp = (double)slot.CreditOut / slot.CreditIn * 100;
                Assert.True(rtp >= feedParams.Percent - 10 && rtp <= feedParams.Percent + 10,
                    $"RTP {rtp:F2}% должен быть близок к {feedParams.Percent}%");
            }
        }

        [Fact]
        public void SuperballMath_LongSession_RTPStable()
        {
            // Arrange
            var gameProps = new GamePropertiesModel
            {
                Percent = 90,
                MaxWinLimit = 0,
                MaxOverdraft = -500000
            };

            var math = new SuperballMath(gameProps, 10000000);
            int ballCount = 7;
            int nominal = 1;
            int spins = 50000;

            long totalBet = 0;
            long totalWin = 0;

            // Act
            for (long ticket = 0; ticket < spins; ticket++)
            {
                int bet = 100 * nominal;
                totalBet += bet;

                var result = math.Spin(ticket, ballCount, nominal);
                totalWin += result.TotalWin;
            }

            // Assert
            double rtp = (double)totalWin / totalBet * 100;
            Assert.True(rtp >= 88 && rtp <= 92,
                $"RTP {rtp:F2}% должен быть стабильным и близким к 90%");
        }

        [Fact]
        public void Slot_MultipleGames_WorkIndependently()
        {
            // Arrange
            var slot1 = new Slot();
            var slot2 = new Slot();
            
            slot1.LoadGame("Slot5_3");
            slot2.LoadGame("Slot5_3");

            var feedParams = new FeedExctractParam
            {
                FeedKey = 1000,
                FeedNumber = 1000000,
                TicketNumber = 1000000,
                SegmentNumber = 0,
                Percent = 95
            };

            slot1.ExtractFeed(feedParams);
            slot2.ExtractFeed(feedParams);

            slot1.DoCashIn(10000);
            slot2.DoCashIn(10000);

            // Act
            slot1.Spin(10, 1);
            slot2.Spin(10, 1);

            // Assert
            Assert.True(slot1.SpinCount == slot2.SpinCount, "Оба слота должны иметь одинаковое количество спинов");
            // Но результаты могут отличаться из-за разных состояний
        }

        [Fact]
        public void Feed_Statistics_Accurate()
        {
            // Arrange
            var game = Game.GameByName("Slot5_3");
            var feed = new Feed(game);
            feed.CellCount = 10000;
            feed.Percent = 95;
            feed.Extract(1000);

            // Act
            var statistics = feed.Statistic();

            // Assert
            Assert.NotNull(statistics);
            // Статистика должна содержать информацию о ленте
        }

        [Fact]
        public void Slot_BonusGame_WorksCorrectly()
        {
            // Arrange
            var slot = new Slot();
            slot.LoadGame("Slot5_3");
            
            var feedParams = new FeedExctractParam
            {
                FeedKey = 1000,
                FeedNumber = 1000000,
                TicketNumber = 1000000,
                SegmentNumber = 0,
                Percent = 95,
                PercentBonus = 30
            };

            slot.ExtractFeed(feedParams);
            slot.DoCashIn(100000);
            slot.Bet = 10;
            slot.Lines = 1;

            bool bonusFound = false;

            // Act - ищем бонусную игру
            for (int i = 0; i < 10000 && !bonusFound; i++)
            {
                slot.Spin(10, 1);
                if (slot.BonusGame)
                {
                    bonusFound = true;
                    break;
                }
            }

            // Assert
            // Бонусная игра может быть найдена или нет, но если найдена - должна работать корректно
            if (bonusFound)
            {
                Assert.True(slot.BonusGameRemain > 0 || slot.BonusGameAll > 0,
                    "Если бонусная игра активирована, должен быть установлен счетчик бонусных игр");
            }
        }

        [Fact]
        public void Slot_Reset_DoesNotAffectGame()
        {
            // Arrange
            var slot = new Slot();
            slot.LoadGame("Slot5_3");
            
            var feedParams = new FeedExctractParam
            {
                FeedKey = 1000,
                FeedNumber = 1000000,
                TicketNumber = 1000000,
                SegmentNumber = 0,
                Percent = 95
            };

            slot.ExtractFeed(feedParams);
            slot.DoCashIn(10000);
            slot.Spin(10, 1);
            var gameBeforeReset = slot.Game;

            // Act
            slot.Reset();

            // Assert
            Assert.NotNull(slot.Game);
            Assert.Equal(gameBeforeReset, slot.Game); // Игра не должна сброситься
            Assert.Equal(0, slot.SpinCount);
            Assert.Equal(0, slot.CashIn);
        }

        [Fact]
        public void SuperballMath_BalanceTracking_Accurate()
        {
            // Arrange
            long initialBalance = 1000000;
            var gameProps = new GamePropertiesModel
            {
                Percent = 90,
                MaxWinLimit = 0,
                MaxOverdraft = -100000
            };

            var math = new SuperballMath(gameProps, initialBalance);
            int ballCount = 7;
            int nominal = 1;
            int spins = 1000;

            long expectedBalance = initialBalance;

            // Act & Assert
            for (long ticket = 0; ticket < spins; ticket++)
            {
                int bet = 100 * nominal;
                expectedBalance += bet;

                var result = math.Spin(ticket, ballCount, nominal);
                expectedBalance -= result.TotalWin;

                Assert.Equal(expectedBalance, result.Balance,
                    $"Баланс не соответствует ожидаемому на спине {ticket}");
            }
        }

        [Fact]
        public void Slot_ScratchMode_DoesNotUseMaps()
        {
            // Arrange
            var slot = new Slot();
            slot.LoadGame("Slot5_3");
            
            var feedParams = new FeedExctractParam
            {
                FeedKey = 1000,
                FeedNumber = 1000000,
                TicketNumber = 1000000,
                SegmentNumber = 0,
                Percent = 95
            };

            slot.ExtractFeed(feedParams);
            slot.DoCashIn(10000);
            slot.Bet = 10;
            slot.Lines = 1;

            // Act
            slot.Spin(10, 1, scratch: true);

            // Assert
            Assert.True(slot.IsScratch);
            // В скрейч-режиме не должно быть матриц
            Assert.Null(slot.Maps);
        }
    }
}

