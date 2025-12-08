using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SlotMathModule.GameLogic.Core
{
    public class WinLine
    {
        public Line line;
        public Symbol symbol;
        public int win;
        public int symbolMatchedCount;
        public Map map;

        private int muliplier = 1;


        public WinLine(Map map_, Line _line, Symbol _symbol, int win_, int symbolMatchedCount_, int multiplier)
        {
            line = _line;
            symbol = _symbol;
            win = win_ * multiplier;
            symbolMatchedCount = symbolMatchedCount_;
            map = map_;

            map.Game.PostProcessWinLine(this);
        }
    }

    public class ScatterWinLine : WinLine
    {
        public ScatterWinLine(Map map, Line line, int scatterCount) 
            : base( map, line, map.Game.Symbol[map.Game.Scatter], 0,scatterCount,0)
        {
        }
    }
}

