using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
 

namespace SlotMathModule.GameLogic.Core
{
    public class FeedStatistic
    {
        public int Variant;

        public int PercentWin;
        public int AllWin;
        public int WinCount;

        public int BonusWinCount;
        public int BonusWin;
        public int BonusPercent;

        public int MaxWin;
        public int MaxBonusWin;
    }

    public partial class Feed
    {
        public List<FeedStatistic> GetStatistic()
        {
            List<FeedStatistic> res = new List<FeedStatistic>();

            for (int l = 0; l < Game.Variants.Length; l++)
            {
                FeedStatistic s = new FeedStatistic();

                s.Variant = l;
                s.AllWin = FeedItem[l].Sum(x => x.Win);
                s.WinCount = FeedItem[l].Where(x => x.Win > 0).Count();
                s.PercentWin = s.AllWin * 100 / (CellCount * Game.Variants[l]);
                s.BonusWin = FeedItem[l].Where(x => x.Bonus > 0).Sum(x => x.Win);
                s.BonusWinCount = FeedItem[l].Where(x => x.Bonus > 0).Count();
                s.BonusPercent = s.AllWin > 0 ? s.BonusWin * 100 / s.AllWin : 0;
                if (s.BonusWinCount > 0) s.MaxBonusWin = FeedItem[l].Where(x => x.Bonus > 0).Max(x => x.Win);
                s.MaxWin = FeedItem[l].Where(x => x.Bonus == 0).Max(x => x.Win);

                res.Add(s);
            } 

            return res;
        }

        public void OutputStatistic()
        {
            var l = GetStatistic();

            foreach (var s in l)
            {
                //Debug.WriteLine("============ Var {0}", s.Variant);
                //Debug.WriteLine("Allwin {0}", s.AllWin);
                //Debug.WriteLine("Percent win {0}", s.PercentWin);
                //Debug.WriteLine("Win count {0}", s.WinCount);
                //Debug.WriteLine("Bonus win {0}", s.BonusWin);
                //Debug.WriteLine("Bonus win count {0}", s.BonusWinCount);
                //Debug.WriteLine("Bonus percent {0}", s.BonusPercent);
                //Debug.WriteLine("");
            }
        }
    }
}

