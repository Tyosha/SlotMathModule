using SlotMathModule.GameLogic.Games;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
 

namespace SlotMathModule.GameLogic.Core
{
    public class RangeWin
    {
        public int Line;
        public int From;
        public int To;
        public int Percent;
    }

    public class MapStruct
    {
        public int[] Map;
        public int Win;
        public int Line;

        public long CountUsed = 0;

        public MapStruct(Map map, List<WinStruct> winStructList)
        {
            Map = map.ToArray();
            Win = map.Win(map.Game.MaxLine);
            if (Win == 0) return;

            Line = map.winline.Max(n => n.line.Index);

            // добавляем возможные выигрыши по каждой линии
            foreach (var lines in map.Game.Variants)
            {
                Win = map.Win(lines);

                WinStruct w;
                var f = winStructList.Where(x => x.Win == Win && x.MaxLineIndex == lines);
                if (f.Count() != 0) w = f.First();
                else
                {
                    w = new WinStruct();
                    w.Win = Win;
                    w.MaxLineIndex = lines;
                    winStructList.Add(w);
                }
                w.Count++;
                w.MapStructs.Add(this);
                w.WinSymCount = Math.Max(w.WinSymCount, map.winline.Select(x => x.symbol).Distinct().Count());
            }
        }

        public Map CreateMap(Game game)
        {
            return new Map(game, Map);
        }
    }

    public class WinStruct
    {
        public int Win;
        public int WinSymCount;
        public int MaxLineIndex;
        public int Count;
        public List<MapStruct> MapStructs = new List<MapStruct>();
    }

    public abstract class Game
    {
        #region [Properties]

        public int Id { get; set; }

        public string Name
        {
            get { return this.GetType().Name; }
        }

        public List<Line> lines = new List<Line>();
        public List<Symbol> Symbol = new List<Symbol>();
        public List<RangeWin> RangeWin = new List<RangeWin>();
        public int[,] Baraban = new int[5,20];
        public int Scatter;
        public int Wild;
        public bool WildOnFirstReel = true;
        public bool UseBonusSym;
        public int PercentBonus = 30;
        public int BonusGameMin = 50;
        public int BonusGameMax = 300;

        public int[] Variants = new int[5] {1, 3, 5, 7, 9};

        public int[] FillMapSequence;

        public int MaxLine;
        public int MapSize = 15;

        public bool WinlineTwoDirection = false; // совпадения линий с обеих сторон
        public bool Together123 = false; // возможность совпадения одновременно двух или трех линий из 1, 2 и 3
        public bool OneSymAtColumn = false; // возможность выпадения более одного символа в одной столбце

        public int MaxWinSymbolInMap = 3;
                   // максимальное кол-во выигрышных символов, не более кол-во символов на барабане (3)

        public List<MapStruct> WinMapList = new List<MapStruct>();

        public int[,] WinTable = new int[9,50];

        public List<WinStruct> AllowWins = new List<WinStruct>();

        public static List<Game> Games = new List<Game>();

        public Random rnd = new Random();

        #endregion

        #region [Create game]

        public static bool LoadGames()
        {
            try
            {
                if (Games.Count == 0)
                {
                    Assembly asm = Assembly.GetExecutingAssembly();
                    foreach (Type type in asm.GetTypes())
                    {
                        if (type.GetCustomAttributes(typeof(GameIdentity), true).Length > 0)
                        {
                            if (Games.Find(x => x.GetType() == type) == null)
                            {
                                var game = Activator.CreateInstance(type) as Game;
                                Games.Add(game);
                            }
                        }
                    }
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static Game GameById(int id)
        {
            return Games.FirstOrDefault(x => x.Id == id);
        }

        public static Game GameByName(string gameName)
        {
            //  return new UltraHot();
            Game result = Games.FirstOrDefault(x => x.Name.ToLower().Equals(gameName.ToLower()));

            if (result == null)
            {
                string s = gameName.Replace(" ", string.Empty);
                var t = Type.GetType("SlotMathModule.GameLogic.Games." + s, true, true);
                result = Activator.CreateInstance(t) as Game;
                // result = new BananasGoBahamas();
            }
            return result;
        }

        #endregion

        public Game()
        {
            CreateLines();

            AddSymbol(new Symbol("-", new int[5] {0, 0, 0, 0, 0}, true));

            CreateSymbol();
            GenWinMaps();

            RangeWin.Add(new RangeWin {Line = 0, From = 0, To = 5, Percent = 40});
            RangeWin.Add(new RangeWin {Line = 0, From = 5, To = 50, Percent = 40});
            RangeWin.Add(new RangeWin {Line = 0, From = 50, To = 10000, Percent = 20});
        }

        protected virtual void CreateLines()
        {
        }

        protected virtual void CreateSymbol()
        {
        }

        protected void AddLine(Line line_)
        {
            lines.Add(line_);
            line_.Index = lines.Count();
        }

        protected void AddSymbol(Symbol symbol_)
        {
            Symbol.Add(symbol_);
            symbol_.Index = Symbol.Count() - 1;
        }

        public virtual Map CreateMap(int[] symbols_)
        {
            return new Map(this, symbols_);
        }

        public Symbol getSymbol(int index_)
        {
            if (index_ >= 0 && index_ < Symbol.Count)
            {
                return Symbol[index_];
            }
            // Если индекс невалидный, возвращаем пустой символ (индекс 0), если он существует
            if (Symbol.Count > 0)
            {
                return Symbol[0];
            }
            throw new ArgumentOutOfRangeException(nameof(index_), $"Invalid symbol index: {index_}, Symbol count: {Symbol.Count}");
        }

        public Symbol getSymbol(string name)
        {
            return Symbol.Where(n => n.Name == name).First();
        }


        #region [Логические игровые фунцкции]

        // проверка валидности расположения символа на матрице
        public virtual bool IsValidSymbolPlaceOnFillMap(Map map, Symbol sym, int place)
        {
            return true;
        }

        // проверка того, может ли символ продолжать выигрышную линиию
        public virtual bool CheckSymbolInWinline(Map map, Line l, int index)
        {
            return map.IsWild(map.map[l.Cells[index]].Index);
            //return Wild > 0 && map.map[l.Cells[index]].Index == Wild;
        }

        public virtual FeedItem GenBonusFeedItem(Random random, int from, int to, int l)
        {
            FeedItem res = new FeedItem();
            res.Win = random.Next(from/5, to/5)*(l + 1)*5;
            return res;
        }


        public virtual void PostProcessWinLine(WinLine wline)
        {

        }

        // подсчет особого скаттер
        public virtual int PrivateScatterCalc(Map m)
        {
            return 0;
        }

        // преобработка матрицы при FillMap
        public virtual void PreOnFillMap(Map m, int lineCount)
        {

        }

        #endregion

        #region [Genarate win maps]

        // проверка матрицы на валидность
        protected virtual bool CheckMapValidOn_GenWinMap(Map m)
        {
            return true; // m.Win(MaxLine) < 800;
        }

    // добавление матриц в массив
        private void AddMapToWinList(int[] map, int wrongWin )
        {
            Map m = new Map(this, map);

            int win = m.Win(MaxLine);           

            if (CheckMapValidOn_GenWinMap(m) && (win != wrongWin)) 
                WinMapList.Add(new MapStruct(m, AllowWins));
        }

        private void _genWinMaps_AddSym(int[] map1, Line line, int winTempMap, int deep, bool no123_, Symbol s, int index, int count = 0)
        {
            if (count == 0) count = index;

            // генерируем с диким символом (только для второй и третьей клетки среднего ряда)
            if (Wild != 0 && map1.Where(p => p == Wild).Count() == 0 && 
                (
                    line.Cells[index] == 6 || line.Cells[index] == 7 ||
                    line.Cells[index] == 1 || line.Cells[index] == 2 ||
                    line.Cells[index] == 11 || line.Cells[index] == 12
                ))
            {
                map1[line.Cells[index]] = Wild;
                if (s.Wins[count] > 0)
                {
                    AddMapToWinList(map1, winTempMap);

                    if (deep < MaxWinSymbolInMap - 1) _genWinMaps(map1, line.Index, deep + 1, no123_);
                }
            }

            // генерируем с обычным символом
            map1[line.Cells[index]] = s.Index;
            if (s.Wins[count] > 0)
            {
                AddMapToWinList(map1, winTempMap);

                if (deep < MaxWinSymbolInMap - 1) _genWinMaps(map1, line.Index, deep + 1, no123_);
            }
            
        }

        // генерируем выигрышные комбинации
        void _genWinMaps(int[]? map =null, int lineNo=0, int deep=0, bool no123 = false)
        {
            if (Together123) no123 = false; // возможность совпадения одновременно 1, 2 и 3 линий

            Map tempMap = null; // исходная матрица на входе в функции
            int winTempMap = 0; // выигрыш по исходной матрице
            if (map != null)
            {
                tempMap = new Map(this, map);
                winTempMap = tempMap.Win(9);
            }
            // перебираем символы
            foreach (var s in Symbol)
            {
                if (s.Index == 0 ||s.Index == Wild || s.Index == Scatter || s.NotUsedForFIll) continue;

                //if (s.Index == 0) continue; // если пустой символ - пропускаем

                if (tempMap != null && !OneSymAtColumn) // проверяем не использован ли символ в других выигрышных линиях
                {
                    bool cont=false;
                    foreach (WinLine l in tempMap.winline)
                    {
                        if (s == l.symbol) cont = true;
                    }
                    if (cont) continue;
                }

                // перебираем линии
                foreach (var line in lines)
                {
                    if (line.Index < lineNo || line.Index == 0) continue;

                    if (no123 && (line.Index == 1 || line.Index == 2 || line.Index == 3)) continue; // не делаем матриц где одновременно могути совпасть две линии из 1, 2, 3

                    if (deep == 0) map = new int[MapSize]; // создаем матрицу только при первой итерации

                    if (deep != 0) // если итерация не первая - проверяем, пуста ли строка
                    {
                        if (!line.IsEmpty(tempMap)) continue;
                    }

                    // слева направо
                    int[] map1 = new int[MapSize];
                    Array.Copy(map, map1, map.Length);

                    map1[line.Cells[0]] = s.Index;
                    if (map1.Distinct().Count() > 3) return; // не допускаем комбинаций более, чем по двум символам
                    AddMapToWinList(map1, winTempMap); // комбинация из одного символа

                    bool no123_ = (line.Index == 1 || line.Index == 2 || line.Index == 3);

                    for (int i = 1; i < 5; i++)
                    {
                        if (line.Cells.Count() < i + 1) continue;                        
                        
                        _genWinMaps_AddSym(map1, line, winTempMap, deep, no123_, s, i);
                    }               
     
                    // справа-налево
                    if (WinlineTwoDirection)
                    {
                        map1 = new int[MapSize];
                        Array.Copy(map, map1, map.Length);

                        map1[line.Cells[line.Cells.Count()-1]] = s.Index;

                        no123_ = (line.Index == 1 || line.Index == 2 || line.Index == 3);

                        for (int i = 2; i < line.Cells.Count(); i++)
                        {
                            if (line.Cells.Count() < i + 1) continue;

                            _genWinMaps_AddSym(map1, line, winTempMap, deep, no123_, s, line.Cells.Count()-i, i - 1);

                            if (map1[8]==map1[13] && map1[8]!=0)
                                break;
                        }                                       
                    }
                }                
            }
        }

        void GenWinMaps()
        {
            WinTable = new int[MaxLine,50];

            int tick = Environment.TickCount;
            _genWinMaps();

            // формируем таблицы выигрышей по линиям
            for (int l = 0; l < MaxLine; l++)
            {
                // Where(n => n.Line <= l + 1)
                var s = WinMapList.Where(x => x.Line <= l+1 ).OrderBy(n => n.Line).Select(n => n.Win).Distinct().OrderBy(n => n).Take(40);

                int j = 0;
                foreach (var i in s)
                {
                    WinTable[l, j] = i;
                    j++;
                }
            }
        }

        #endregion

        #region [Функции получения матриц]

        // получение матрицы по размеру выигрыша
            #region [old version GetMapByWin]
        /*
        public Map GetMapByWin(int lineCount, int win, int badSym = -1)
        {
            var mapSel = WinMapList.Where(n => n.Line <= lineCount && n.Win == win && !n.Map.Contains(badSym));
            if (!mapSel.Any()) mapSel = WinMapList.Where(n => n.Win == win && !n.Map.Contains(badSym));

            var l = mapSel.Select(n => n.WinStruct.WinSymCount).Distinct(); // список возможных линий
            
            int li = l.ElementAt(rnd.Next(0, l.Count())); // случайный выбор линии

            long countUsed = mapSel.Where(x => x.WinStruct.WinSymCount == li).Select(x => x.CountUsed).Min(); // для уменьшения повторения подряд одной и той же комбинации

            mapSel = mapSel.Where(n => n.WinStruct.WinSymCount == li && n.CountUsed == countUsed); // выборка матриц по выбранной линии

            MapStruct mStruct = mapSel.ElementAt(rnd.Next(0, mapSel.Count()));
            mStruct.CountUsed++;

            Map m = mStruct.CreateMap(this);

            return m;
        }
            */
            #endregion

        public Map GetMapByWin(int lineCount, int win, int badSym = -1)
        {
            var mapSel = AllowWins.Where(x => x.Win == win && x.MaxLineIndex == lineCount).First().MapStructs.Where(x => !x.Map.Contains(badSym));

          //  var l = mapSel.Select(n => n.WinSymCount).Distinct(); // список возможных линий

          //  int li = l.ElementAt(rnd.Next(0, l.Count())); // случайный выбор линии

            long countUsed = mapSel.Select(x => x.CountUsed).Min(); // для уменьшения повторения подряд одной и той же комбинации

            mapSel = mapSel.Where(n => n.CountUsed == countUsed); // выборка матриц по выбранной линии

            MapStruct mStruct = mapSel.ElementAt(rnd.Next(0, mapSel.Count()));
            mStruct.CountUsed++;

            Map m = mStruct.CreateMap(this);

            return m;
        }

        // получение матрицы по ячейке билета
        public abstract List<Map> GetMapByFeedItem(FeedItem feedItem_, int lineCount_, int bet_);

        // получение без выигрышной случайно матрицы
        public Map GenSimpleMap(bool noFill = false)
        {
            Map m = new Map(this, new int[MapSize]);
            if (!noFill) m.FillMap(MaxLine);
            return m;
        }

        #endregion
    }
}

