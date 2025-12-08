using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management.Instrumentation;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace SlotMathModule.GameLogic.Games
{
    public class HappySeven
    {
        private int CellCount = 100000;
        private int[] Cells;

        private Random Rnd;

        private long _initiator;
        
        public int[] WinList =   {1,    2,    3,    4,    5,    7,   10,  20,  30,  50, 70, 100, 200, 300, 500, 1000};
        public int[] WinCounts = {5000, 4000, 3000, 2000, 1000, 500, 300, 200, 100, 50, 30, 9,   5,   2,   1,   1};

        public List<int> Wins = new List<int>();

        public HappySeven()
        {
            Cells = new int[CellCount];
        }
        

        public int[] Spin(int TicketWin, int mult)
        {
            var _leftSymbol = -1;
            var _centerSymbol = -1;
            var _rightSymbol = -1;

            TicketWin /= mult;

            TicketWin /= 100;

            var rand = new Random();
            if (TicketWin > 0)
            {
                switch (TicketWin)
                {
                    case 1:
                        _leftSymbol = rand.Next(0, 100) < 50 ? 0 : 1;
                        _centerSymbol = rand.Next(2, 11);
                        _rightSymbol = rand.Next(2, 11);
                        break;
                    case 2:
                        _leftSymbol = rand.Next(0, 100) < 50 ? 0 : 1;
                        _centerSymbol = rand.Next(0, 100) < 50 ? 0 : 1;
                        _rightSymbol = rand.Next(2, 11);
                        break;
                    case 3:
                        _leftSymbol = rand.Next(0, 100) < 50 ? 0 : 1;
                        _centerSymbol = rand.Next(0, 100) < 50 ? 0 : 1;
                        _rightSymbol = rand.Next(0, 100) < 50 ? 0 : 1;
                        while (_centerSymbol == _rightSymbol)
                        {
                            _centerSymbol = rand.Next(0, 100) < 50 ? 0 : 1;
                        }
                        break;
                    case 4:
                        _leftSymbol = 10;
                        _centerSymbol = rand.Next(0, 10);
                        _rightSymbol = rand.Next(0, 10);
                        break;
                    case 5:
                        _leftSymbol = 10;
                        _centerSymbol = 10;
                        _rightSymbol = rand.Next(0, 10);
                        break;
                    case 7:
                        _leftSymbol = 10;
                        _centerSymbol = 10;
                        _rightSymbol = 10;
                        break;
                    case 10:
                        _leftSymbol = 9;
                        _centerSymbol = 9;
                        _rightSymbol = 9;
                        break;
                    case 20:
                        _leftSymbol = 8;
                        _centerSymbol = 8;
                        _rightSymbol = 8;
                        break;
                    case 30:
                        _leftSymbol = 7;
                        _centerSymbol = 7;
                        _rightSymbol = 7;
                        break;
                    case 50:
                        _leftSymbol = 6;
                        _centerSymbol = 6;
                        _rightSymbol = 6;
                        break;
                    case 70:
                        _leftSymbol = 5;
                        _centerSymbol = 5;
                        _rightSymbol = 5;
                        break;
                    case 100:
                        _leftSymbol = 4;
                        _centerSymbol = 4;
                        _rightSymbol = 4;
                        break;
                    case 200:
                        _leftSymbol = 3;
                        _centerSymbol = 3;
                        _rightSymbol = 3;
                        break;
                    case 300:
                        _leftSymbol = 2;
                        _centerSymbol = 2;
                        _rightSymbol = 2;
                        break;
                    case 500:
                        _leftSymbol = 1;
                        _centerSymbol = 1;
                        _rightSymbol = 1;
                        break;
                    case 1000:
                        _leftSymbol = 0;
                        _centerSymbol = 0;
                        _rightSymbol = 0;
                        break;
                }
            }
            else
            {
                _leftSymbol = rand.Next(2, 10);
                _centerSymbol = rand.Next(0, 11);
                _rightSymbol = rand.Next(0, 11);

                while (_leftSymbol == _centerSymbol)
                {
                    _centerSymbol = rand.Next(0, 11);
                }
            }
            return new int[] { _leftSymbol, _centerSymbol, _rightSymbol };
        }

        public enum HappySevenSymbols
        {
            TrippleSeven = 0,
            Seven = 1,
            Star = 2,
            Bar = 3,
            Watermelon = 4,
            Bell = 5,
            Tomato = 6,
            Plum = 7,
            Lemon = 8,
            Orange = 9,
            Cherries = 10
        }
    }
}

