using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
 
using SlotMathModule.GameLogic.Games;

namespace SlotMathModule.GameLogic.Core
{
    public class FeedItem
    {
        public int Index;
        public int Win;
        public int Bonus;
        public object BonusDet;
    }

    public class FeedItemDebug : FeedItem
    {
        public List<Map> Maps = new List<Map>();
    }

    public partial class Feed
    {

        // In params
        public Game Game;

        public int Percent = 95;
        public int PercentBonus = 30;
        public int BonusGameMin;
        public int BonusGameMax;

        public int CellCount = 110000;
        public int FeedKey;
        public long FeedNumber;
        public long TicketCount = 1100;
        public long CurTicket;

        private int curSegment;
        public int CurSeqment
        {
            get { return curSegment;  }
            set 
            { 
                curSegment = value;
                CurCell = (int)((CurTicket - FeedNumber)*100) + curSegment;
            }
        }
        
        public int CurCell = 0;
        public int BonusGame = 0;

        public FeedFake FeedFake; //new FeedFakeBonus( new BookOfRa() ); //new FeedFake55Bonus();

        public List<FeedItem>[] FeedItem; // = new List<FeedItem>[9];

        public Feed(Game game_)
        {
            Game = game_;
            
            // FeedFake = new FeedFake_NoFree(Game); 
            // FeedFake = new FeedFake_Var_1(Game);
            // FeedFake = new FeedFakeSeq(Game);
        }
        
        public int BuyTicket(long ticketNumber)
        {
            if (ticketNumber >= FeedNumber && ticketNumber < FeedNumber + TicketCount)
            {
                CurTicket = ticketNumber;
                CurSeqment = 0;
                return 0;
            }

            return 201;
        }

        public FeedItem GetNextFeedItem(int lineCount_, int bet)
        {
            int varInd = Array.IndexOf(Game.Variants, lineCount_);
            
            if (varInd < 0 || varInd >= FeedItem.Length || FeedItem[varInd] == null)
            {
                return null; // Неверный вариант или лента не инициализирована
            }

            FeedItem result = new FeedItem();

            CurCell++;
            CurSeqment++;

            #region test
            //*

            if (FeedFake != null)
            {
                result = FeedFake.GetNextItem(this, lineCount_);

                if (result.Win == 0) result = null;

                return result;
            }

            //*/

            #endregion

            //var fwin = FeedItem[lineCount_ - 1].Where(p => p.Index == CurCell).OrderByDescending( p => p.Bonus );
            var fwin = FeedItem[varInd].Where(p => p.Index == CurCell).OrderByDescending(p => p.Bonus);

            if (fwin.Count() > 0)
            {
                result = fwin.First();
                if (ConstraintFilter.IsValidWin(result.Win, bet, lineCount_)) return result;
            } 

            return null;
        }

        #region get result
        public List<FeedItem> GetNextFeedItemNoLines(int segment)
        {
            List<FeedItem> results = new List<FeedItem>();
            CurSeqment = segment;

            for (int i = 0; i < FeedItem.Count(); i++)
            {
                if (FeedItem[i] == null) results.Add(new FeedItem());
                else
                {
                    var fwin = FeedItem[i].Where(p => p.Index == CurCell).OrderByDescending(p => p.Bonus);
                    if (fwin.Count() > 0)
                    {
                        results.Add(fwin.FirstOrDefault());
                    }
                    else
                    {
                        results.Add(new FeedItem());
                    }
                }
            }

            return results;
        }

        #endregion

        public void MoveToNextSegment()
        {
            CurSeqment++;
        }

        public void Extract(int feedKey)
        {
            FeedItem = new List<FeedItem>[Game.Variants.Length];

            PercentBonus = Game.PercentBonus;

            FeedKey = feedKey + Game.Id;
            Random rnd = new Random(FeedKey);

            for (int varInd = 0; varInd < Game.Variants.Length; varInd++ )
            {
                int l = Game.Variants[varInd];

                int allWin = CellCount * l * Percent / 100;
                int bonusWin = allWin * PercentBonus / 100;
                int mainWin = allWin - bonusWin;

                int win = 0;

                // лента учета свободных ячеек
                FeedItem[varInd] = new List<FeedItem>();

                // таблица диапазонов по данной линии
                var rangeTable = Game.RangeWin.Where(n => n.Line == l);
                if (rangeTable.Count() == 0) rangeTable = Game.RangeWin.Where(n => n.Line == 0);

                int sumWin = 0;
                foreach (var r in rangeTable.OrderByDescending(x => x.To))
                {
                    int rangeWin = mainWin * r.Percent / 100;

                    // выбираем выигрыши по диапазону
                    List<int> wins = new List<int>();             // x.MaxLineIndex <= l D:01152014
                    foreach (var w in Game.AllowWins.Where(x => x.MaxLineIndex == l && x.Win > r.From && x.Win <= r.To))
                    {
                        int cnt = 1;
                        if (w.WinSymCount == 1) cnt = 20; // w.Count * 2;
                        if (w.WinSymCount >= 2) cnt = 3; // w.Count;
                        for (int i = 0; i < cnt; i++) wins.Add(w.Win);
                    }

                    // формируем последовательность выигрышей
                    win = 0;
                    while (win < rangeWin && wins.Count > 0 && sumWin < mainWin)
                    {
                        // выбираем выигрыш
                        int curWin = wins.Skip(rnd.Next(0, wins.Count())).First();

                        // формируем элемент
                        FeedItem[varInd].Add(new FeedItem { Win = curWin });
                        win += curWin;
                        sumWin += curWin;
                    }
                }

                // формируем бонусные розыгрыши
                sumWin = 0;
                rangeTable = Game.RangeWin.Where(n => n.Line == 999);
                foreach (var r in rangeTable.OrderByDescending(x => x.To))
                {
                    int rangeWin = bonusWin * r.Percent / 100;

                    win = 0;
                    // формируем бонусные розыгрыши
                    while (win < rangeWin && sumWin < bonusWin) 
                    {
                        // генерируем выигрыш
                        FeedItem fitem = Game.GenBonusFeedItem(rnd, r.From, r.To, l);
                        fitem.Bonus = fitem.Bonus > 0 ? fitem.Bonus : 1;

                        // добавляем элемент в список
                        FeedItem[varInd].Add(fitem);
                        win += fitem.Win;
                        sumWin += fitem.Win;
                    }
                }

                // распределяем выигрышные элементы
                int[] indexes = new int[CellCount];
                for (int i = 0; i < CellCount; i++) indexes[i] = i;

                // Получаем список доступных индексов для более эффективного распределения
                var availableIndexes = indexes.Where(x => x >= 0).ToList();
                
                int num = 0;
                foreach (var feedItem in FeedItem[varInd])
                {
                    if (availableIndexes.Count == 0) break; // Нет доступных индексов
                    
                    int randomIndex = rnd.Next(0, availableIndexes.Count);
                    int ind = availableIndexes[randomIndex];
                    availableIndexes.RemoveAt(randomIndex); // Удаляем использованный индекс
                    
                    feedItem.Index = ind;
                    indexes[ind] = -1; // Помечаем как использованный

                    num++;
                    if (num > CellCount - 100) break;
                }
            }
        }

        public double Dispersion()
        {
            double matOj = FeedItem[0].Sum(n => n.Win)/CellCount;

            double result = 0;
            foreach (var feedItem in FeedItem[0])
            {
                result += Math.Pow(feedItem.Win - matOj, 2);
            }

            return result/CellCount;

/*            for (int i = 0; i < 200; i++)
                MatOj = mas[i] + MatOj;
            MatOj = MatOj / 200; //Математическое ожидание

            for (int i = 0; i < 200; i++)
                Disp = pow((mas[i] - MatOj), 2) + Disp;
            Disp = Disp / 199; //Дисперсия           
 */
        }

        public void OutFeedSummary()
        {
            Console.WriteLine("-- Feed summary -------------");
            Console.WriteLine("FeedItem sum[3] = "+FeedItem[2].Sum( n => n.Win ));
            Console.WriteLine("-----------------------------");
        }
    }
}

