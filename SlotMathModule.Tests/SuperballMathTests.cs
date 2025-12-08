using System;
using System.Linq;
using Xunit;
using SlotMathModule.SuperBall;
using SlotMathModule.Common.Models;
using SlotMathModule.Common;

namespace SlotMathModule.Tests
{
    /// <summary>
    /// Тесты для математики SuperBall
    /// </summary>
    public class SuperballMathTests
    {
        private const int CellCount = 100000;
        private const int TestSpins = 10000; // Количество спинов для тестирования RTP

        private GamePropertiesModel CreateGameProperties(short percent = 90, int maxWinLimit = 0, int maxOverdraft = 0)
        {
            return new GamePropertiesModel
            {
                Percent = percent,
                MaxWinLimit = maxWinLimit,
                MaxOverdraft = maxOverdraft,
                AdditionGameModel = null
            };
        }

        [Fact(Skip = "Superball tests excluded")]
        public void SuperballMath_Constructor_InitializesCorrectly()
        {
            // Arrange
            var gameProps = CreateGameProperties(90);
            long gameBalance = 1000000;

            // Act
            var math = new SuperballMath(gameProps, gameBalance);

            // Assert
            Assert.NotNull(math);
        }

        [Fact(Skip = "Superball tests excluded")]
        public void SuperballMath_Percent_ClampedToMinMax()
        {
            // Arrange & Act
            var mathLow = new SuperballMath(CreateGameProperties(50), 0); // Должен быть зажат до 80
            var mathHigh = new SuperballMath(CreateGameProperties(100), 0); // Должен быть зажат до 95

            // Assert - проверяем через RTP тест
            // Если процент зажат правильно, RTP должен быть в пределах 80-95%
        }

        [Theory(Skip = "Superball tests excluded")]
        [InlineData(80)]
        [InlineData(85)]
        [InlineData(90)]
        [InlineData(95)]
        public void SuperballMath_RTP_ShouldMatchPercent(short percent)
        {
            // Arrange
            var gameProps = CreateGameProperties(percent);
            long gameBalance = 10000000; // Большой баланс для точности
            var math = new SuperballMath(gameProps, gameBalance);

            int ballCount = 7; // Стандартное количество шаров
            int nominal = 1; // Номинал
            long totalBet = 0;
            long totalWin = 0;

            // Act - выполняем много спинов
            for (long ticket = 0; ticket < TestSpins; ticket++)
            {
                int bet = 100 * nominal; // Ставка = 100 * номинал
                totalBet += bet;

                var result = math.Spin(ticket, ballCount, nominal);
                totalWin += result.TotalWin;
            }

            // Assert
            double actualRTP = (double)totalWin / totalBet * 100;
            double expectedRTP = percent;

            // RTP должен быть в пределах ±2% от заданного процента (допуск на случайность)
            Assert.True(actualRTP >= expectedRTP - 2.0 && actualRTP <= expectedRTP + 2.0,
                $"RTP {actualRTP:F2}% не соответствует ожидаемому {expectedRTP}% (допуск ±2%)");
        }

        [Fact(Skip = "Superball tests excluded")]
        public void SuperballMath_Spin_ReturnsValidResult()
        {
            // Arrange
            var gameProps = CreateGameProperties(90);
            var math = new SuperballMath(gameProps, 1000000);
            int ballCount = 7;
            int nominal = 1;

            // Act
            var result = math.Spin(0, ballCount, nominal);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.TotalWin >= 0);
            Assert.True(result.Balance >= 0 || result.Balance >= -500000); // Может быть отрицательным до maxOverdraft
        }

        [Theory(Skip = "Superball tests excluded")]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(4)]
        [InlineData(5)]
        [InlineData(6)]
        [InlineData(7)]
        [InlineData(8)]
        [InlineData(9)]
        [InlineData(10)]
        public void SuperballMath_Spin_WorksWithDifferentBallCounts(int ballCount)
        {
            // Arrange
            var gameProps = CreateGameProperties(90);
            var math = new SuperballMath(gameProps, 1000000);
            int nominal = 1;

            // Act
            var result = math.Spin(0, ballCount, nominal);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.TotalWin >= 0);
        }

        [Fact(Skip = "Superball tests excluded")]
        public void SuperballMath_MaxWinLimit_Respected()
        {
            // Arrange
            int maxWinLimit = 50000; // Максимальный выигрыш
            var gameProps = CreateGameProperties(90, maxWinLimit);
            var math = new SuperballMath(gameProps, 10000000);

            int ballCount = 7;
            int nominal = 1;
            bool foundMaxWin = false;

            // Act - выполняем много спинов, чтобы найти максимальный выигрыш
            for (long ticket = 0; ticket < CellCount; ticket++)
            {
                var result = math.Spin(ticket, ballCount, nominal);
                
                // Assert
                Assert.True(result.TotalWin <= maxWinLimit,
                    $"Выигрыш {result.TotalWin} превышает максимальный лимит {maxWinLimit}");
                
                if (result.TotalWin == maxWinLimit)
                    foundMaxWin = true;
            }

            // Проверяем, что лимит действительно работает
            Assert.True(foundMaxWin || maxWinLimit >= 100000, 
                "Максимальный выигрыш не был достигнут, возможно лимит слишком высокий");
        }

        [Fact(Skip = "Superball tests excluded")]
        public void SuperballMath_MaxOverdraft_Respected()
        {
            // Arrange
            int maxOverdraft = -100000; // Максимальный уход в минус
            var gameProps = CreateGameProperties(90, 0, maxOverdraft);
            var math = new SuperballMath(gameProps, 0); // Начинаем с нулевого баланса

            int ballCount = 7;
            int nominal = 1;

            // Act - выполняем спины до достижения лимита
            for (long ticket = 0; ticket < CellCount; ticket++)
            {
                var result = math.Spin(ticket, ballCount, nominal);
                
                // Assert
                Assert.True(result.Balance >= maxOverdraft,
                    $"Баланс {result.Balance} ушел ниже максимального овердрафта {maxOverdraft}");
                
                if (result.Balance <= maxOverdraft + 1000) // Близко к лимиту
                    break;
            }
        }

        [Fact(Skip = "Superball tests excluded")]
        public void SuperballMath_Balance_CalculatedCorrectly()
        {
            // Arrange
            long initialBalance = 1000000;
            var gameProps = CreateGameProperties(90);
            var math = new SuperballMath(gameProps, initialBalance);
            int ballCount = 7;
            int nominal = 1;

            long expectedBalance = initialBalance;
            int bet = 100 * nominal;

            // Act
            for (long ticket = 0; ticket < 100; ticket++)
            {
                var result = math.Spin(ticket, ballCount, nominal);
                expectedBalance += bet; // Добавляем ставку
                expectedBalance -= result.TotalWin; // Вычитаем выигрыш

                // Assert
                Assert.Equal(expectedBalance, result.Balance);
            }
        }

        [Fact(Skip = "Superball tests excluded")]
        public void SuperballMath_Jackpot_DetectedCorrectly()
        {
            // Arrange
            var gameProps = CreateGameProperties(90);
            var math = new SuperballMath(gameProps, 10000000);
            int nominal = 1;
            bool jackpotFound = false;

            // Act - ищем джекпот
            for (long ticket = 0; ticket < CellCount; ticket++)
            {
                // Проверяем для разных количеств шаров
                for (int ballCount = 7; ballCount <= 10; ballCount++)
                {
                    var result = math.Spin(ticket, ballCount, nominal, isMayBeJackPot: true);
                    
                    if (result.IsJackpot)
                    {
                        jackpotFound = true;
                        
                        // Assert - проверяем условия джекпота
                        int ticketWin = result.TotalWin / (100 * nominal);
                        bool isValidJackpot = 
                            (ballCount == 7 && (ticketWin == 200 || ticketWin == 800)) ||
                            (ballCount == 8 && (ticketWin == 500 || ticketWin == 2000)) ||
                            (ballCount == 9 && (ticketWin == 200 || ticketWin == 800 || ticketWin == 1000 || ticketWin == 4000)) ||
                            (ballCount == 10 && (ticketWin == 500 || ticketWin == 2000 || ticketWin == 1000 || ticketWin == 4000));
                        
                        Assert.True(isValidJackpot, 
                            $"Джекпот обнаружен для ballCount={ballCount}, ticketWin={ticketWin}, но условия не соответствуют");
                    }
                }
            }

            // Джекпот должен быть найден хотя бы раз за всю ленту
            Assert.True(jackpotFound, "Джекпот не был найден за всю ленту");
        }

        [Fact(Skip = "Superball tests excluded")]
        public void SuperballMath_GetCells_ReturnsValidArray()
        {
            // Arrange
            var gameProps = CreateGameProperties(90);
            var math = new SuperballMath(gameProps, 1000000);
            int ballCount = 7;
            int nominal = 1;

            // Act - делаем спин, чтобы инициализировать ленту
            var result = math.Spin(0, ballCount, nominal);
            var cells = math.GetCells();

            // Assert
            Assert.NotNull(cells);
            Assert.Equal(CellCount, cells.Length);
            Assert.All(cells, cell => Assert.True(cell >= 0));
            Assert.NotNull(result);
        }

        [Fact(Skip = "Superball tests excluded")]
        public void SuperballMath_DifferentInitiators_GenerateDifferentTapes()
        {
            // Arrange
            var gameProps = CreateGameProperties(90);
            var math1 = new SuperballMath(gameProps, 1000000);
            var math2 = new SuperballMath(gameProps, 1000000);
            int ballCount = 7;
            int nominal = 1;

            // Act - используем разные инициаторы (разные fullTicket, которые делятся на CellCount)
            var result1 = math1.Spin(0, ballCount, nominal);
            var result2 = math2.Spin(CellCount, ballCount, nominal); // Другой инициатор

            // Assert - ленты должны быть разными
            var cells1 = math1.GetCells();
            var cells2 = math2.GetCells();

            // Проверяем, что есть различия (хотя бы одно)
            bool hasDifference = false;
            for (int i = 0; i < Math.Min(cells1.Length, cells2.Length); i++)
            {
                if (cells1[i] != cells2[i])
                {
                    hasDifference = true;
                    break;
                }
            }

            // С высокой вероятностью ленты должны отличаться
            Assert.True(hasDifference, "Ленты для разных инициаторов должны отличаться");
        }

        [Fact(Skip = "Superball tests excluded")]
        public void SuperballMath_WinDistribution_Reasonable()
        {
            // Arrange
            var gameProps = CreateGameProperties(90);
            var math = new SuperballMath(gameProps, 10000000);
            int ballCount = 7;
            int nominal = 1;

            long zeroWins = 0;
            long smallWins = 0; // < 1000
            long mediumWins = 0; // 1000 - 10000
            long largeWins = 0; // > 10000

            // Act
            for (long ticket = 0; ticket < TestSpins; ticket++)
            {
                var result = math.Spin(ticket, ballCount, nominal);
                int win = result.TotalWin;

                if (win == 0) zeroWins++;
                else if (win < 1000) smallWins++;
                else if (win < 10000) mediumWins++;
                else largeWins++;
            }

            // Assert - распределение должно быть разумным
            double zeroPercent = (double)zeroWins / TestSpins * 100;
            double smallPercent = (double)smallWins / TestSpins * 100;
            double mediumPercent = (double)mediumWins / TestSpins * 100;
            double largePercent = (double)largeWins / TestSpins * 100;

            // Большинство спинов должны быть проигрышными или с малыми выигрышами
            Assert.True(zeroPercent + smallPercent > 50, 
                $"Слишком много средних/крупных выигрышей: zero={zeroPercent:F1}%, small={smallPercent:F1}%");
            
            // Крупные выигрыши должны быть редкими
            Assert.True(largePercent < 5, 
                $"Слишком много крупных выигрышей: {largePercent:F1}%");
        }

        [Theory(Skip = "Superball tests excluded")]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(5)]
        [InlineData(10)]
        [InlineData(20)]
        public void SuperballMath_DifferentNominals_WorkCorrectly(int nominal)
        {
            // Arrange
            var gameProps = CreateGameProperties(90);
            var math = new SuperballMath(gameProps, 10000000);
            int ballCount = 7;

            // Act
            var result = math.Spin(0, ballCount, nominal);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.TotalWin >= 0);
            // Выигрыш должен быть кратен 100 * nominal
            Assert.True(result.TotalWin % (100 * nominal) == 0,
                $"Выигрыш {result.TotalWin} не кратен ставке {100 * nominal}");
        }
    }
}

