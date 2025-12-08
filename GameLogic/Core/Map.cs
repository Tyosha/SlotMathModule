using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
 

namespace SlotMathModule.GameLogic.Core
{
    public class MapAddParam
    {
        public string Name;
        public string Value;
    }

    public abstract class MapBonusData
    {
        public string Desc;

        public abstract int Win();

        public string Description()
        {
            return Desc;
        }
    }
    

    public class Map
    {
//        static Random r = new Random();

        public List<Symbol> map = new List<Symbol>();
        public List<WinLine> winline = new List<WinLine>();
        
        public bool Bonus;
        public MapBonusData BonusData;
        public int BonusWin;
        public int BonusGameAdd = 0;
        public bool FixWin = false;
        public int AddWild; // дополнительный вайлд
        public int WinSymCount = 0; // количество видов символов которые составляют выигрышные линии

        public ScatterWinLine ScatterLine;

        private int multiplier = 1;
        public int Multiplier
        {
            get
            {
                return multiplier;
            }
            set
            {
                multiplier = value;
                DefineWinLine();
            }
        }

        public Game Game;

        public void RemoveExcessWinLines(int lineCount)
        {
            for (int i = winline.Count - 1; i >= 0; i--)
                if (winline[i].line.Index > lineCount) winline.Remove(winline[i]);            
        }

        public int Win(int lineCount, int bet = 1)
        {
            int result = 0;

            if (ScatterLine != null)
                ScatterLine.win = (ScatterLine.symbol.Wins[ScatterLine.symbolMatchedCount] + Game.PrivateScatterCalc(this)) * lineCount * Multiplier;

            foreach (WinLine w in winline)
            {
                if (w.line.Index > lineCount) continue;

                w.win *= bet;
                result += w.win;
            }
            
            return result + (BonusWin * bet);
        }

        public Map(Game game_, int[] symbols)
        {
            Game = game_;

            FillSym(symbols);

            DefineWinLine();
        }

        void FillSym(int[] syms)
        {
            map.Clear();            
            foreach (var symbol in syms)
            {
                map.Add(Game.getSymbol(symbol));
            }            
        }

        // заполняет до конца, частично заполненную матрицу, не меняя ее выигрыша
        public Map FillMap(int lineCount_, int badSym = 0)
        {
            #region [Debug]
            /*
            int i = 0;
            foreach (var symbol in map)
            {
                i++;
                Debug.Write(symbol.Index.ToString()+"  ");
                if (i % 5 == 0) Debug.WriteLine("");
            }
            Debug.WriteLine("--------------------------");
             */
            #endregion

            Game.PreOnFillMap(this, lineCount_);

            DefineWinLine();
            int curWin = Win(lineCount_);

            List<WinLine> wlsave =  new List<WinLine>();
            foreach (var winLine in winline){wlsave.Add(winLine); }
            List<Symbol> mapsave = new List<Symbol>();
            foreach (var sym in map) { mapsave.Add(sym); }


            for (int j = 0; j < map.Count(); j++)
            {
                int n = j;
                if (Game.FillMapSequence != null) n = Game.FillMapSequence[j];

                Symbol s = map[n];

                if (s.Index == 0)
                {
                    bool cont = false;
                    int cycle = 0;
                    do
                    {
                        int maxSym = Game.Symbol.Count();
                        //  if (  r.Next(1, 5) > 1 || badSym > 0) maxSym--; // если уже есть два бонусных символа, то убираем его из возможных вариантов

                        Symbol sym = Game.getSymbol(Game.rnd.Next(1, maxSym));
                        map[n] = sym;

                        // проверяем возможно ли использовать данный символ в данной ячейке
                        if (
                                (sym.Index == badSym) || (sym.NotUsedForFIll) ||
                                (sym.Index == Game.Scatter && (map.Where(p => p.Index == Game.Scatter).Count() > 1)) || // не добавляем больше одного скаттера
                                (IsWild(sym.Index) && //== Game.Wild && 
                                    (
                                       (Game.WildOnFirstReel && (n == 0 || n == 5 || n == 10)) ||
                                        (n == 1 || n == 2 || n == 6 || n == 11)
                                     )
                                ) || // не ставим дикий символ в позиции, которые могут привести к коллизии
                                (Game.WinlineTwoDirection && sym.Index == Game.Wild && (n == 4 || n == 9 || n == 14 || n == 3 || n == 8 || n == 13)) || // не ставим дикий символ в позиции, которые могут привести к коллизии при TwoDirection
                                (!Game.IsValidSymbolPlaceOnFillMap(this, sym, n))   // индивидуальная проверка согласно условиям конкретной игры
                            )
                        {
                            cont = true;
                            continue;
                        } // если символ запрещен, ищем след.вариант
                        int ind = n % 5;

                        // не допускаем появления одинаковых символов в одной колонке
                        if (!Game.OneSymAtColumn && map.Count() > 9)
                        {
                            cont = (map[ind] == map[ind + 5] && (ind == n || ind + 5 == n)) ||
                                (map[ind] == map[ind + 10] && (ind == n || ind + 10 == n)) ||
                                (map[ind + 5] == map[ind + 10] && (ind + 5 == n || ind + 10 == n));

                            /*
                            cont = (map[ind] == map[ind + 5] && map[ind + 5].Index > 0) || 
                                (map[ind] == map[ind + 10] && map[ind + 10].Index > 0) || 
                                (map[ind + 5] == map[ind + 10] && map[ind + 10].Index > 0);
                                */
                        }

                        if (Game.MapSize < 15) cont = false;

                        DefineWinLine();
                       cycle++;

                        //  if (cycle > 2000) break; // debug
                    } while (Win(lineCount_) != curWin || cont); //  (Win(Game.MaxLineIndex) != curWin || cont);
                }
            }

            return this;
        }

        public virtual void DefineWinLine()
        {
            winline.Clear();
            foreach (Line line in Game.lines)
            {
                line.AddToMapIfWin(this);
            }
        }

        public bool[] GetBonusRows(int bonusSym)
        {
            bool[] result = new bool[5];
            for (int i = 0; i < 5; i++)
            {
                result[i] = (map[i].Index == bonusSym || map[i + 5].Index == bonusSym || map[i + 10].Index == bonusSym);
            }
            int bonusRowCount = result.Where(p => p == true).Count();
            bonusRowCount = Math.Max(1,Math.Min(bonusRowCount, 5));

            if (Game.getSymbol(bonusSym).Wins[bonusRowCount - 1] > 0) 
                return result;
                else return null;
        }

        public int[] ToArray()
        {
            int[] result = new int[map.Count()];
            int i = 0;
            foreach (var symbol in map)
            {
                result[i] = symbol.Index;
                i++;
            }
            return result;
        }

        // меняет символ не пересчитывая выигрышные линии
        public void ChangeSym(int index, int newSym)
        {
            int[] temp = ToArray();
            temp[index] = newSym;
            FillSym(temp);
        }

        public bool IsSymPlaceAt(int sym, int[] places)
        {
            if (sym == 0) return false;
            for (int i = 0; i < places.Length; i++)
            {
                if (map[places[i]].Index == sym) return true;
            }
            return false;
        }

        public bool IsWild(int sym)
        {
            return sym > 0 && (sym == Game.Wild || sym == AddWild);
        }
    }
}

