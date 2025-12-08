using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SlotMathModule.GameLogic.Core
{
    public class Line
    {
        public int[] Cells;
        public int Index;

        public Line(int[] cells_)
        {
            Cells = cells_;
        }

        public virtual void  AddToMapIfWin(Map map)
        {
            int symCount = 0;
            WinLine win_line = null;

            Symbol first = map.map[Cells[0]];
            if (first.Index != 0 && first.Index == map.Game.Scatter && !map.IsWild(first.Index)) return;

            Symbol s = null;
            for (int i = 0; i < Cells.Length; i++)
            {
                s = map.map[Cells[i]];
                if (!map.IsWild(s.Index) || s.Index == 0) break;
//                if (s.Index != map.Game.Wild || s.Index == 0) break;
            }

            // слева-направо
            for (int n = 1; n < Cells.Length; n++)
            {
                if (map.map[Cells[n]] == s || (map.Game.CheckSymbolInWinline(map, this, n))) symCount++; else break;
            }

            // одиночный вайлд
            //if (s.Wins[symCount] == 0 && map.map[Cells[0]].Index == map.Game.Wild) s = map.Game.getSymbol(map.Game.Wild);

            if (s.Wins[symCount] > 0)
            {
                win_line = new WinLine(map, this, s, s.Wins[symCount], symCount + 1, map.Multiplier);
            }            

            // справа-налево
            if (map.Game.WinlineTwoDirection)
            {
                symCount = 0;
                s = map.map[Cells[Cells.Count() - 1]];
                for (int n = 2; n <= Cells.Length; n++)
                {
                    if (map.map[Cells[Cells.Count() - n]] == s ||
                        (map.Game.Wild > 0 && map.map[Cells[Cells.Count() - n]].Index == map.Game.Wild))
                        symCount++;
                    else break;
                }

                if (s.Wins[symCount] > 0 && (win_line == null || win_line.win < s.Wins[symCount]))
                {
                    win_line = new WinLine(map, this, s, s.Wins[symCount], symCount + 1, map.Multiplier);
                    //map.winline.Add(new WinLine(map, this, s, s.Wins[symCount], symCount + 1, map.Multiplier));
                }
            }

            // Добавляем выигрышную линию
            if (win_line != null) map.winline.Add(win_line);
        }

        public bool IsEmpty(Map map)
        {        
            for (int n = 0; n < Cells.Length; n++)
            {
                if (map.map[Cells[n]].Index != 0) return false;
            }
            return true;
        }
    }

    public class ScatterLine : Line
    {
        public ScatterLine() : base ( new int[5]{0, 0, 0, 0, 0} )
        {
            Index = 0;
        }

        public override void AddToMapIfWin(Map map)
        {
            Symbol sym = map.Game.getSymbol(map.Game.Scatter);
            int scatCount = Math.Min(4, Math.Max(0, map.map.Where(p => p == sym).Count() - 1));
            if ((scatCount > 0 && map.Game.getSymbol(map.Game.Scatter).Wins[scatCount] > 0)
                || map.Game.PrivateScatterCalc(map) > 0)
            {
                map.ScatterLine = new ScatterWinLine(map, this, scatCount);
                map.winline.Add(map.ScatterLine);
            }            
        }
    }

}

