using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using SlotMathModule.Common.Models;
using SlotMathModule.Common;

namespace SlotMathModule.SuperBall
{
    public class SuperballMath
    {
        private const short MinPercent = 80; // минимальный процент возврата
        private const short MaxPercent = 95; // максимальный процент возврата
        private const int CellCount = 100000;

        #region Win Table

        public int[][] _winLists = new int[][]
        {
            new int[] {0}, //1
            new int[] //2
            {
                1, 6,
                4, 24
            },
            new int[] //3
            {
                2, 27,
                8, 108
            },
            new int[] //4
            {
                1, 6, 50,
                4, 24, 200
            },
            new int[] //5
            {
                1, 2, 10, 100,
                4, 8, 40, 400
            },
            new int[] //6
            {
                2, 7, 31, 150,
                4, 28, 124, 600
            },
            new int[] //7
            {
                1, 4, 12, 60, 200,
                4, 16, 48, 240, 800
            },
            new int[] //8
            {
                1, 2, 6, 21, 100, 500,
                4, 8, 24, 84, 400, 2000
            },
            new int[] //9
            {
                2, 4, 16, 90, 200, 1000,
                8, 16, 64, 360, 800, 4000
            },
            new int[] //10
            {
                1, 4, 8, 25, 150, 500, 1000,
                4, 16, 32, 100, 600, 2000, 4000
            },
        };

        public int[][] _winCountsLight = new int[][]
        {
            new int[] {0}, //1
            new int[] //2
            {
                800, 4,
                200, 1
            },// 100, 600,
              // 400, 2400
            new int[] //3
            {
                10000, 100,
                500, 1
            },// 200, 2700,
              // 800, 10800
            new int[] //4
            {
                10000, 5000, 5,
                1000, 50, 1
            },// 100, 600, 5000,
              // 400, 2400, 20000
            new int[] //5
            {
                7500, 5000, 500, 2,
                5000, 1000, 6, 1
            },// 100, 200, 1000, 10000,
              // 400, 800, 4000, 40000
            new int[] //6
            {
                10000, 500, 25, 2,
                5000, 6, 2, 1
            },// 200, 700, 3100, 15000,
              // 400, 2800, 12400, 60000
            new int[] //7
            {
                20000, 5000, 1000, 50, 1,
                2500, 500, 50, 5, 1
            },// 100, 400, 1200, 6000, 20000,
              // 400, 1600, 4800, 24000, 80000
            new int[] //8
            {
                20000, 7500, 500, 100, 5, 1,
                5000, 500, 100, 50, 3, 1
            },// 100, 200, 600, 2100, 10000, 50000,
              // 400, 800, 2400, 8400, 40000, 200000
            new int[] //9
            {
                25000, 2500, 50, 10, 1, 1,
                500, 50, 10, 2, 1, 1
            },// 200, 400, 1600, 9000, 20000, 100000,
              // 800, 1600, 6400, 36000, 80000, 400000
            new int[] //10
            {
                20000, 5000, 2500, 250, 50, 1, 1,
                200, 50, 25, 10, 2, 1, 1
            },// 100, 400, 800, 2500, 12500, 50000, 100000,
              // 400, 1600, 3200, 10000, 50000, 200000, 400000
        };

        public int[][] _winCountsHard = new int[][]
        {
            new int[] {0}, //1
            new int[] //2
            {
                800, 4,
                200, 1
            },// 100, 600,
              // 400, 2400
            new int[] //3
            {
                5000, 100,
                250, 1
            },// 200, 2700,
              // 800, 10800
            new int[] //4
            {
                5000, 2500, 10,
                500, 25, 1
            },// 100, 600, 5000,
              // 400, 2400, 20000
            new int[] //5
            {
                3750, 2500, 250, 4,
                2500, 500, 8, 1
            },// 100, 200, 1000, 10000,
              // 400, 800, 4000, 40000
            new int[] //6
            {
                4000, 300, 12, 2,
                2000, 100, 2, 1
            },// 200, 700, 3100, 15000,
              // 400, 2800, 12400, 60000
            new int[] //7
            {
                7500, 1750, 350, 20, 3,
                750, 175, 20, 5, 1
            },// 100, 400, 1200, 6000, 20000,
              // 400, 1600, 4800, 24000, 80000
            new int[] //8
            {
                5000, 3750, 250, 50, 55, 2,
                1250, 250, 125, 50, 5, 1
            },// 100, 200, 600, 2100, 10000, 50000,
              // 400, 800, 2400, 8400, 40000, 200000
            new int[] //9
            {
                10000, 1000, 25, 10, 2, 2,
                125, 25, 10, 3, 2, 1
            },// 200, 400, 1600, 9000, 20000, 100000,
              // 800, 1600, 6400, 36000, 80000, 400000
            new int[] //10
            {
                20000, 5000, 2500, 250, 100, 10, 1,
                200, 100, 75, 50, 4, 1, 1
            },// 100, 400, 800, 2500, 12500, 50000, 100000,
              // 400, 1600, 3200, 10000, 50000, 200000, 400000
        };

        public int[][] _winCounts;
        #endregion

        private int _percent = 90;

        private int[] _balls;

        private Random _rand;
        private int[] _cells;

        private int _maxWinLimit;

        private long _balance = 0; // баланс игры
        private long _maxOverdraft = -500000; // баланс игры

        private List<int> _wins = new List<int>();

        private long _initiator = 0;

        private WriteLogDelegate OnLog; // делегат для логирования

        private int _minKoefToBigWin = int.MaxValue; // минимальный кофэффициент, с которого считается крупный выигрыш
        private int _firstPositionBigWin = 0; // с какого номера начинаются крупные выигрыши
        private int _minPositionFirstBigWin = 0; // с какого номера должен прийти первый крупный выигрыш
        private int _maxPositionFirstBigWin = CellCount; // до какого номера должен прийти первый крупный выигрыш

        // конструктор
        public SuperballMath(GamePropertiesModel param, long gameBalance, WriteLogDelegate onLog = null)
        {
            _balance = gameBalance;
            _maxOverdraft = param.MaxOverdraft < 0 ? param.MaxOverdraft : 0;

            _maxWinLimit = param.MaxWinLimit <= 0 ? int.MaxValue : param.MaxWinLimit;

            param.Percent = param.Percent > MaxPercent ? MaxPercent : param.Percent;
            param.Percent = param.Percent < MinPercent ? MinPercent : param.Percent;
            _percent = param.Percent;

            _winCounts = _winCountsLight;

            if (param.AdditionGameModel != null)
            {
                var addParam = JsonConvert.DeserializeObject<GameParams>(param.AdditionGameModel);

                _firstPositionBigWin = addParam.FirstPositionBigWin;
                _minPositionFirstBigWin = addParam.MinPositionFirstBigWin;
                _maxPositionFirstBigWin = addParam.MaxPositionFirstBigWin;
                _minKoefToBigWin = addParam.MinKoefToBigWin;

                _winCounts = addParam.IsLightTape ? _winCountsLight : _winCountsHard;
            }

            _firstPositionBigWin = _firstPositionBigWin >= CellCount || _firstPositionBigWin < 0 ? CellCount / 3 : _firstPositionBigWin;
            _minPositionFirstBigWin = _minPositionFirstBigWin >= _firstPositionBigWin || _minPositionFirstBigWin < 0 ? 1000 : _minPositionFirstBigWin;
            _maxPositionFirstBigWin = _maxPositionFirstBigWin == 0 ? CellCount : _maxPositionFirstBigWin;
            _minKoefToBigWin = _minKoefToBigWin == 0 || _minKoefToBigWin > 4000 ? 200 : _minKoefToBigWin;

            _cells = new int[CellCount];

            _balls = new int[80];

            for (int i = 0; i < 80; i++)
            {
                _balls[i] = i + 1;
            }

            OnLog = onLog;
            Log(string.Format("Percent: {0}, MaxWin: {1}", _percent, _maxWinLimit));
        }

        // спин (прокрут)
        public TiketGameModel Spin(long fullTicket, int ballCount, int nominal, bool isMayBeJackPot = false)
        {
            _balance += 100 * nominal;

            var ticket = (int)(fullTicket % CellCount);

            var newInitiator = (fullTicket / CellCount);

            if (_initiator != newInitiator)
            {
                _initiator = newInitiator;
                MakeNewRandom(ballCount);
            }

            int ticketWin = _cells[ticket];
            int realTicketWin = 100 * nominal * _cells[ticket];

            if (realTicketWin > _maxWinLimit || _balance - realTicketWin < _maxOverdraft)
            {
                ticketWin = realTicketWin = 0;
            }

            bool jackPot = ballCount == 7 && (ticketWin == 200 || ticketWin == 800) ||
                           ballCount == 8 && (ticketWin == 500 || ticketWin == 2000) || 
                           ballCount == 9 && (ticketWin == 200 || ticketWin == 800 || ticketWin == 1000 || ticketWin == 4000) || 
                           ballCount == 10 && (ticketWin == 500 || ticketWin == 2000 || ticketWin == 1000 || ticketWin == 4000);

            if (nominal >= 10 && !isMayBeJackPot && jackPot)
            {
                jackPot = false;
                realTicketWin = 0;
            }

            _balance -= realTicketWin;

            Log(string.Format("Ticket: {0}. Bet: {1}, Win: {2}, Balance: {3}\n",
                               fullTicket, 100 * nominal, realTicketWin, _balance));

            return new TiketGameModel()
            {
                TotalWin = realTicketWin,
                Balance = _balance,
                IsJackpot = jackPot
            };
        }

        // получение нового рандома
        private void MakeNewRandom(int betCount)
        {
            var r1 = new Random((int)(_initiator / CellCount));
            var r2 = new Random((int)(_initiator % CellCount));

            var key1 = r1.Next(-2147483648, -1);
            var key2 = r2.Next(0, 2147483647);

            var key = key1 + key2;
            _rand = new Random(key);
            _cells = new int[CellCount];
            ExtractWinCells(betCount);
        }

        private int[] MakeWinCounts(int[] winList, int index)
        {
            var count = winList.Length;

            var winCounts = new int[count];

            _winCounts[index].CopyTo(winCounts, 0);

            return winCounts;
        }

        // заполнение ленты
        private void ExtractWinCells(int betCount)
        {
            var index = betCount - 1;

            int[] winList = new int[_winLists[index].Length];
            _winLists[index].CopyTo(winList, 0);

            int[] winCount = MakeWinCounts(winList, index);

            for (int i = 0; i < winList.Count(); i++)
            {
                for (int j = 0; j < winCount[i]; j++)
                {
                    _wins.Add(winList[i]);
                }
            }

            //int allWin = CellCount * _percent / 100;
            int allWin = CellCount;
            int win = 0;

            var wins = new List<int>();

            while (allWin > win)
            {
                var item = _wins[_rand.Next(0, _wins.Count)];

                if (item + win > allWin) break;

                wins.Add(item);
                win += item;
            }

            var sortWins = wins.OrderBy(t => t).ToList();

            int smallWinCount = sortWins.Count(t => t < _minKoefToBigWin);
            int allWinCount = sortWins.Count();

            // мелкие выигрыши
            for (int i = 0; i < smallWinCount; i++)
            {
                int ind = _rand.Next(0, CellCount);

                while (_cells[ind] != 0)
                {
                    ind = _rand.Next(0, CellCount);
                }

                _cells[ind] = sortWins.ElementAt(i);
            }

            int bigWinCount = allWinCount - smallWinCount;
            int bigWinNumber = _rand.Next(0, bigWinCount);
            int firstInd = 0;

            // крупные выигрыши
            for (int i = smallWinCount; i < allWinCount; i++)
            {
                if (i - smallWinCount == bigWinNumber)
                {
                    firstInd = _rand.Next(_minPositionFirstBigWin, _maxPositionFirstBigWin);

                    while (_cells[firstInd] != 0)
                    {
                        firstInd = _rand.Next(_minPositionFirstBigWin, _maxPositionFirstBigWin);
                    }

                    _cells[firstInd] = sortWins.ElementAt(i);
                }
                else
                {
                    int ind = _rand.Next(_firstPositionBigWin, CellCount);

                    while (_cells[ind] != 0)
                    {
                        ind = _rand.Next(_firstPositionBigWin, CellCount);
                    }

                    _cells[ind] = sortWins.ElementAt(i);
                }
            }

            int mustDelete = allWin * (100 - _percent) / 100; // нужно убрать выигрышей с ленты

            while (mustDelete >= 0)
            {
                int ind = _rand.Next(0, CellCount);

                while (_cells[ind] == 0 || ind == firstInd)
                {
                    ind = _rand.Next(0, CellCount);
                }

                mustDelete -= _cells[ind];
                _cells[ind] = 0;
            }
        }

        // возврат ленты
        public int[] GetCells()
        {
            var mas = new List<int>();
            mas.AddRange(_cells);

            return _cells.ToArray();
        }

        // Логирование
        private void Log(string text)
        {
            if (OnLog != null)
            {
                OnLog("SuperBall. " + text);
            }
        }
    }
}

