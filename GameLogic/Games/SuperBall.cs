using System;
using System.Collections.Generic;
//using System.Diagnostics;
using System.Linq;
using System.Text;

namespace SlotMathModule.GameLogic.Games
{
    public class SuperBall
    {
        public int[][] WinLists = new int[][]
        {
            new int[] {0}, //1
            new int[] //2
            {
                100, 600,
                400, 2400
            },
            new int[] //3
            {
                200, 2700,
                800, 10800
            },
            new int[] //4
            {
                100, 600, 5000,
                400, 2400, 20000
            },
            new int[] //5
            {
                100, 200, 1000, 10000,
                400, 800, 4000, 40000
            },
            new int[] //6
            {
                200, 700, 3100, 15000,
                400, 2800, 12400, 60000
            },
            new int[] //7
            {
                100, 400, 1200, 6000, 20000,
                400, 1600, 4800, 24000, 80000
            },
            new int[] //8
            {
                100, 200, 600, 2100, 10000, 50000,
                400, 800, 2400, 8400, 40000, 200000
            },
            new int[] //9
            {
                200, 400, 1600, 9000, 20000, 100000,
                800, 1600, 6400, 36000, 80000, 400000
            },
            new int[] //10
            {
                100, 400, 800, 2500, 15000, 50000, 100000,
                400, 1600, 3200, 10000, 60000, 200000, 400000
            },
        };
        
        private int[] Balls;

        public int[] Cells;
        
        public int BallHits;
        public bool IsSuperball;

        public SuperBall()
        {
            Balls = new int[80];

            for (int i = 0; i < 80; i++)
            {
                Balls[i] = i + 1;
            }
        }
        
        private int[] GetBallDraw(int[] betData, int winAmount)
        {
            var betCount = betData.Length;

            BallHits = 0;

            var res = new int[20];

            var r = new Random();

            if (winAmount == 0)
            {
                var winBallsCount = betCount - (WinLists[betCount - 1].Length / 2);

                winBallsCount = r.Next(0, winBallsCount + 1);

                if (winBallsCount > 0)
                {
                    for (int i = 0; i < winBallsCount; i++)
                    {
                        var rN = r.Next(0, betCount);
                        while (res.Contains(betData[rN]))
                        {
                            rN = r.Next(0, betCount);
                        }
                        var iN = r.Next(0, 20);
                        while (res[iN] != 0)
                        {
                            iN = r.Next(0, 20);
                        }
                        res[iN] = betData[rN];
                    }
                }

                for (int i = 0; i < 20; i++)
                {
                    var ballIndex = r.Next(0, 80 - betCount - res.Length);
                    if (res[i] == 0) res[i] = Balls.Where(x => !betData.Contains(x) && !res.Contains(x)).ToArray()[ballIndex];
                }

                return res;
            }
            else
            {
                int[] winList = new int[WinLists[betCount - 1].Length];
                WinLists[betCount - 1].CopyTo(winList, 0);

                var indexOfWin = 0;

                if (winList.Count(x => x == winAmount) > 1)
                {
                    var beASuperBall = r.Next(0, 2) == 1;
                    var calcWinList = winList.Skip(beASuperBall ? (winList.Length / 2) : 0).ToArray();
                    for (int i = 0; i < calcWinList.Length; i++)
                    {
                        if (calcWinList[i] == winAmount)
                        {
                            indexOfWin = i + (beASuperBall ? (winList.Length / 2) : 0);
                            break;
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < winList.Length; i++)
                    {
                        if (winList[i] == winAmount)
                        {
                            indexOfWin = i;
                            break;
                        }
                    }
                }

                IsSuperball = (int)(winList.Length / 2) < indexOfWin + 1;

                var winBallCount = betCount - (winList.Length / 2) + (IsSuperball ? indexOfWin + 1 - (winList.Length / 2) : indexOfWin + 1);

                BallHits = winBallCount;

                var betBalls = betData.ToList();

                for (int i = 0; i < winBallCount; i++)
                {
                    var ball = betBalls[r.Next(0, betBalls.Count)];
                    betBalls.Remove(ball);

                    if (i == 0 && IsSuperball)
                    {
                        res[19] = ball;
                    }
                    else
                    {
                        var resI = r.Next(0, 19);
                        while (res[resI] > 0)
                        {
                            resI = r.Next(0, 19);
                        }
                        res[resI] = ball;
                    }
                }

                for (int i = 0; i < 20; i++)
                {
                    if (res[i] != 0) continue;
                    var ballIndex = r.Next(0, 80 - betData.Length - res.Length);
                    res[i] = Balls.Where(x => !betData.Contains(x) && !res.Contains(x)).ToArray()[ballIndex];
                }

                return res;
            }
        }

        public int[] Spin(int ticketWin, int[] betData)
        {
            return GetBallDraw(betData, ticketWin);
        }
    }
}

