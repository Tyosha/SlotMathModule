using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SlotMathModule.GameLogic.Games;
using SlotMathModule.GameLogic.Core;

namespace SlotMathModule.GameLogic.Core
{
    public class FeedExctractParam
    {
        public int FeedKey;
        public long FeedNumber;
        public long TicketNumber;
        public int SegmentNumber;
        public int GameIndex;
        public int TicketCount;
        public int Nominal;

        public int CellCount
        {
            get { return TicketCount * 100; }
        }
        public int Percent;
        public int PercentBonus;
        public int BonusGameMin;
        public int BonusGameMax;

        public FeedExctractParam()
        {
            FeedKey = 1000;
            FeedNumber = 1000000;
            TicketNumber = FeedNumber;
            GameIndex = 0;
            TicketCount = 1100;
            Nominal = 20;

            Percent = 95;
            PercentBonus = 30;
            BonusGameMin = 30;
            BonusGameMax = 500;
        }
    }

    #region [Slot interface]

    public interface ISlot
    {
        int Bet { get; set; }             // ставка
        int Lines { get; set; }           // кол-во линий игры
        int BetLine { get; }
        bool BonusGame { get; set; }     // признак выпадения бонусной игры
        int BonusGameRemain { get; set; }  // кол-во оставшихся бонусных игр
        int BonusGameAll { get; set; }  // кол-во бонусных игр
        int BonusSym { get; set; }

        int CurSegment { get; set; } // текущий сегмент

        int CurWin { get; set; }       // текущий выигрыш
        int BonusWin { get; set; } // текущий выигрыш по бонусной игре       
        int AllBonusWin { get; set; } // общий выигрыш по бонусной игре

        int Credits { get; set; }      // текущее кол-во кредитов

        int CashIn { get; set; }       // общий вход денег
        int CashOut { get; set; }      // общий выход денег
        int CreditIn { get; set; }     // общее кол-во потраченных кредитов
        int CreditOut { get; set; }    // общее кол-во выигранных кредитов

        int SpinCount { get; set; }    // кол-во прокрутов

        Map Map { get; set; }          // текущая матрица

        void DoCashIn(int amount);    // пополнение кредитов
        void DoCashOut();   // снятие кредитов
        int BuyTicket(long ticketNumber);
        void ExtractFeed(FeedExctractParam p);
        //int feedKey, long feedNumber, long ticketNumber, int segmentNumber, int gameIndex, int ticketCount, int nominal); // установка ленты
        void Reset();
        int Spin(int bet, int line, bool scratch = false);  // прокрут
    }

    #endregion

    public enum GameState
    {
        MainGame, RiskGame, BonusGame
    }

    public class Slot : ISlot
    {
        #region Properties

        public int Bet { get; set; }             // ставка
        public int Lines { get; set; }           // кол-во линий игры
        public int BetLine { get { return Bet * Lines; } }

        public bool BonusGame { get; set; }      // признак выпадения бонусной игры
        public int BonusGameRemain { get; set; } // кол-во оставшихся бонусных игр
        public int BonusGameAll { get; set; }    // кол-во бонусных игр
        public int BonusSym { get; set; }
        public bool[] BonusRows;

        public int CurWin { get; set; }          // текущий выигрыш
        public int BonusWin { get; set; }        // текущий выигрыш по бонусной игре       
        public int AllBonusWin { get; set; } // общий выигрыш по бонусной игре

        public int Credits { get; set; }         // текущее кол-во кредитов

        public bool IsScratch { get; set; }
        public int CurSegment
        {
            get { return Feed.CurSeqment; }
            set { Feed.CurSeqment = value; }
        } // текущий сегмент

        public int CashIn { get; set; }          // общий вход денег
        public int CashOut { get; set; }         // общий выход денег
        public int CreditIn { get; set; }        // общее кол-во потраченных кредитов
        public int CreditOut { get; set; }       // общее кол-во выигранных кредитов

        public int SpinCount { get; set; }       // кол-во прокрутов

        public GameState GameState; // состояние терминала

        public FeedItem feedItem;
        public Map Map { get; set; }             // текущая матрица
        public List<Map> Maps;                   // список матриц для прокрута

        public Game Game;
        public Feed Feed;                        // установленная лента

        #endregion

        public Slot()
        {
            Lines = 1;
            Bet = 1;
            Game.LoadGames();
        }
        ~Slot()
        {
            //Game = null;
            //feedItem = null;
            //Map = null;
            //Feed = null;
            //if(Maps != null) Maps.Clear();
            //Maps = null;
            System.Diagnostics.Debug.Print("Destroy slot");
        }

        #region [Operation]

        public void DoCashIn(int amount)
        {
            CashIn += amount;
            Credits += amount;
        }

        public void DoCashOut()
        {
            CashOut += Credits;
            Credits = 0;
        }

        public void NextBet()
        {
            switch (Bet)
            {
                case 1: Bet = 2; break;
                case 2: Bet = 5; break;
                case 5: Bet = 10; break;
                case 10: Bet = 20; break;
                case 20: Bet = 50; break;
                case 50: Bet = 100; break;
                case 100: Bet = 1; break;
            }
        }

        public void NextLine()
        {
            int ind = Array.IndexOf(Game.Variants, Lines);
            ind++;
            if (ind == Game.Variants.Length) ind = 0;
            Lines = Game.Variants[ind];
        }

        public void Reset()
        {
            CashIn = 0;
            CashOut = 0;
            CreditIn = 0;
            CreditOut = 0;
            SpinCount = 0;

            Bet = 1;
            Lines = 1;
        }

        #endregion

        public List<FeedItem> ExtractResult(int segment)
        {
            return Feed.GetNextFeedItemNoLines(segment);
        }

        public List<FeedStatistic> Statistic()
        {
            return Feed.GetStatistic();
        }

        public int Spin(int bet = 0, int line = 0, bool scratch = false)
        {
            IsScratch = scratch;
            if (bet > 0) Bet = bet;
            if (line > 0) Lines = line;

            if (Feed.CurSeqment == 100 && Maps == null) return 301; // закончились сегменты
            if (Credits < BetLine && !BonusGame && BonusGameRemain == 0) return 302; // недостаточно кредитов

            #region [Scratch]
            // скрейч режим
            if (scratch)
            {
                if (Game.Variants.Distinct().Count() == 1) Lines = Game.Variants[0];

                Credits -= BetLine;
                CreditIn += BetLine;
                feedItem = Feed.GetNextFeedItem(Lines, Bet);
                if (feedItem != null)
                {
                    CurWin = feedItem.Win * Bet; // текущий выигрыш по ячейке
                    Credits += CurWin; // увеличиваем кол-во кредитов на сумму выигрыша
                    CreditOut += CurWin; // увеличиваем CreditOut на сумму выигрыша
                }
                else
                {
                    CurWin = 0;
                }
                return 0;
            }
            #endregion

            SpinCount++;

            BonusGame = false;
            if (BonusGameRemain <= 0)
            {
                BonusWin = 0;
                BonusGameRemain = 0;
            }

            // если нет матриц для розыгрша, извлекаем следующий сегмент
            if (Maps == null)
            {
                feedItem = Feed.GetNextFeedItem(Lines, Bet);
                Maps = Game.GetMapByFeedItem(feedItem, Lines, Bet);

                BonusGame = (feedItem != null) && (feedItem.Bonus > 0);
                if (BonusGame) AllBonusWin = feedItem.Win * bet;
                  else AllBonusWin = 0;
            }

            Map = Maps[0]; // выбираем текущую матрицу
            Maps.Remove(Maps[0]); // удаляем ее из массива

            BonusGame = BonusGame || Map.BonusGameAdd > 0; // бонус в бонусе

            if (Maps.Count() == 0) Maps = null;

            CurWin = Map.Win(Lines, Bet); // вычисляем текущий выигрыш
            Map.RemoveExcessWinLines(Lines); // удаление выигрышных линий первышающих линию игрока

            if (BonusGameRemain > 0) BonusRows = Map.GetBonusRows(BonusSym);
            else
            {
                BonusSym = 0;
                BonusRows = null;
                BonusGameAll = 0;
            }

            // окончание бонусной игры
            if (BonusGameRemain == 0) // && !BonusGame)
            {
                Credits -= BetLine;
                CreditIn += BetLine;
                if (!BonusGame || Map.BonusGameAdd == 0)
                {
                    Credits += CurWin; // увеличиваем кол-во кредитов на сумму выигрыша}
                    CreditOut += CurWin; // увеличиваем CreditOut на сумму выигрыша
                }
            }
            else
            {
                BonusGameRemain--;   // уменьшаем счетчик бонусных игр
                BonusWin += CurWin; // +Map.BonusWin; // увеличиваем бонусный выигрыш

                //  if (BonusGame && BonusGameRemain == 0) BonusWin += Map.BonusWin;

                if (BonusGameRemain <= 0)
                {
                    Credits += BonusWin;
                    CreditOut += BonusWin;         // увеличиваем CreditOut на сумму выигрыша
                }
            }

            BonusGameRemain += Map.BonusGameAdd;
            BonusGameAll += Map.BonusGameAdd;

            // начало бонусной игры
            if (BonusGame && BonusGameAll == BonusGameRemain)
            {
                // BonusSymIndex = feedItem.Bonus;
                //if (!Game.UseBonusSym) BonusSymIndex = 0;
                BonusWin += CurWin;
            }

            return 0;
        }

        public bool Risk()
        {
            Random r = new Random();
            bool result = r.Next(1, 3) == 1;

            int value = CurWin;
            if (BonusWin > 0) value = BonusWin;
            if (!result) value = -value;

            if (result)
            {
                //Credits += CurWin;
                //CreditOut += CurWin;
                if (BonusWin == 0)
                    CurWin = CurWin * 2;
                else BonusWin = BonusWin * 2;
            }
            else
            {
                //    Credits -= CurWin;
                //    CreditOut += CurWin;
                CurWin = 0;
                BonusWin = 0;
            }

            Credits += value;
            CreditOut += Math.Abs(value);

            return result;
        }

        public void ExtractFeed(FeedExctractParam p)
        //int feedKey, long feedNumber, long ticketNumber, int segmentNumber, int gameIndex, int ticketCount, int nominal) // установка ленты
        {
            Maps = null;
            Feed f = new Feed(Game) { FeedNumber = p.FeedNumber, TicketCount = p.TicketCount };

            f.CellCount = p.CellCount;
            f.Percent = p.Percent;
            f.PercentBonus = Game.PercentBonus; // p.PercentBonus;
            f.BonusGameMin = Game.BonusGameMin; // p.BonusGameMin;
            f.BonusGameMax = Game.BonusGameMax; // p.BonusGameMax;

            f.Extract(p.FeedKey);
            f.CurTicket = p.TicketNumber;
            f.CurSeqment = p.SegmentNumber;
            f.TicketCount = p.TicketCount;

            Feed = f;

            Map = Game.GenSimpleMap();
        }

        public void ResetBonusGame()
        {
            BonusGame = false;
            Maps = null;
            BonusSym = 0;
            BonusRows = null;
            BonusGameAll = 0;
            BonusGameRemain = 0;
        }

        public int BuyTicket(long ticketNumber)
        {
            return Feed.BuyTicket(ticketNumber);
        }

        public void OutSummary()
        {
            Console.WriteLine("============= Slot summary ============");
            if (CashIn > 0) Console.WriteLine("SpinPercent={0} CashIn:CashOut:Percent {1}:{2}:{3}      {4}", (CashIn - CashOut) * 100 / SpinCount, CashIn, CashOut, CashOut * 100 / CashIn, CashIn - CashOut);
            if (CreditIn > 0) Console.WriteLine("SpinCount={0} CreditIn:CreditOut:Percent {1}:{2}:{3}", SpinCount, CreditIn, CreditOut, CreditOut * 100 / CreditIn);
            Console.WriteLine("---------------------------------------");
        }


        public int LoadGame(string game)
        {
            Game = Game.GameByName(game);
            return Game.Id;
        }


        public string BonusSymbol { get { return Game.getSymbol(BonusSym).Name; } }



        public long CurrentTicket { get { return Feed.CurTicket; } }

        public int LoadGame(int currentGame)
        {
            Game = Game.GameById(currentGame);
            return Game.Id;
        }
    }
}

