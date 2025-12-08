using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
 
using SlotMathModule.GameLogic.Core;
using SlotMathModule.GameLogic.Games;

namespace SlotMathModule.GameLogic.Core
{
    public abstract class FeedFake
    {
        public FeedFake(Game g)
        {        
        }

        public abstract FeedItem GetNextItem(Feed feed, int lineCount);
    }

    public class FeedFake55Bonus : FeedFake
    {
        public FeedFake55Bonus(Game g) : base (g)
        {
        }

        public override FeedItem GetNextItem(Feed feed, int lineCount)
        {
            FeedItem res;

            res = new FeedItem();

            int ind = feed.CurCell % 5; switch (ind)
            {
                case 1:
                    res.Win = 5;
                    break;
                case 3:
                    res.Win = 5;
                    break;
                case 4:
                    if (feed.Game.GetType().Name == "UltraHot" || feed.Game.GetType().Name == "SizzlingHot") res.Win = 25;
                    else
                    {
                        res.Win = lineCount * 100;
                        res.Bonus = 5;
                    }

                    break;
            }

            return res;
        }        
    }

    public class FeedFakeSeq : FeedFake
    {
        public FeedFakeSeq(Game g)
            : base(g)
        {
        }

        private int num = 0;

        public override FeedItem GetNextItem(Feed feed, int lineCount)
        {
            FeedItem res;

            res = new FeedItem();

            res.Win = 5; // feed.Game.WinTable[lineCount - 1, num];

            num++;
            if (num == 50 || res.Win == 0) num = 0;

            return res;
        }
    }

    public class FeedFakeBonus : FeedFake
    {
        private int num = 0;

        private  Random rnd = new Random();

        public FeedFakeBonus(Game g)
            : base(g)
        {
        }

        public override FeedItem GetNextItem(Feed feed, int lineCount)
        {
            FeedItem res = new FeedItem();

            int[] wins = new int[7] { 0, 10, 0, 20, 0, 50, 0 }; //new int[6]{0,10,25,50,150,500};
            int multi = 1;
            
            multi = lineCount;

            int ind = (num%wins.Length);
            if (ind != 0 && wins[ind] != 0)
            {
                res = feed.Game.GenBonusFeedItem(new Random(), (wins[ind]/2) * multi, wins[ind] * multi, lineCount);
            }
            
            num++;

            return res;
        }
    }

    public class FeedFake_Novomat_50_100_500 : FeedFake
    {
        FeedItem[] items = new FeedItem[5];

        public int CurItem = 0;

        //private FeedItem CreateFeedItem(SlotNovomatic g)  //TODO TYOSHA
        //{
        //    SlotNovomatic.NovoFeedData[] spins = new SlotNovomatic.NovoFeedData[g.BonusSpinsInRound + 1];
        //    for (int i = 0; i < spins.Length; i++) spins[i] = new SlotNovomatic.NovoFeedData();
        //    spins[0] = new SlotNovomatic.NovoFeedData {Scatters = 3 };
        //    spins[1].Win = 100;
        //    spins[2].Win = 10;                            

        //    FeedItem res = new FeedItem();
        //    res.Win = spins[0].Win + spins.Where(x => x != spins[0]).Sum( x => x.Win) * g.MultiplierOnBonus;
        //    res.BonusDet = spins;
        //    res.Bonus = 1;

        //    return res;
        //}

        public FeedFake_Novomat_50_100_500(Game g) : base (g)
        {
        }

        public override FeedItem GetNextItem(Feed feed, int lineCount)
        {
            CurItem = (CurItem + 1 == items.Length) ? 0 : CurItem + 1;

            return items[CurItem];
        }        
    }

    public class FeedFake_Var_1 : FeedFake
    {
        public int CurItem;

        public override FeedItem GetNextItem(Feed feed, int lineCount)
        {
            Game g = feed.Game;
            FeedItem result = new FeedItemDebug();
            FeedItemDebug res = (FeedItemDebug) result;

            int ind = CurItem % 4;
           // ind = 3;
            
            // пустой спин
            //switch (ind)  //TODO TYOSHAs
            //{

            //    case 0:
            //        result.Win = 0;
            //        res.Maps.Add(g.GenSimpleMap());
            //        break;

            //        // спин с двумя скаттерами
            //    case 1:
            //        result = (FeedItem)g.GenBonusFeedItem(new Random(), 10, 50, lineCount);

            //        break;

            //    case 2:
            //        // спин с двумя скаттерами, плюс линия

            //        result = (FeedItem)g.GenBonusFeedItem(new Random(), 50, 100, lineCount);

            //        break;

            //    case 3:
            //        if (g is SlotNovomatic)
            //        {
            //            result = (FeedItem) g.GenBonusFeedItem(new Random(), 150, 500, lineCount);
            //        }
            //        else if (g is SlotIgrosoft)
            //        {
            //            result = (FeedItem)g.GenBonusFeedItem(new Random(), 50, 150, lineCount);                        
            //        }
            //        else if (g is Slot_MegaJack)
            //        {
            //            result = (FeedItem)g.GenBonusFeedItem(new Random(), 100, 300, lineCount);                        
            //        }
            //        break;
            //    case 4:
            //        if (g is SlotNovomatic || g is SlotIgrosoft || g is Slot_MegaJack)
            //        {
            //            result = (FeedItem) g.GenBonusFeedItem(new Random(), 10, 50, lineCount);
            //        }
            //        else
            //        {
            //            result.Win = g.WinTable[lineCount - 1,7];                        
            //        }
            //        break;
            //}

            
            foreach (var map in res.Maps)
            {
                result.Win += map.Win(lineCount);
            }
            
            CurItem++;

            return result;
        }

        public FeedFake_Var_1(Game g): base(g)
        {
        }
    }

    public class FeedFake_Var_2 : FeedFake
    {
        public int CurItem;

        public override FeedItem GetNextItem(Feed feed, int lineCount)
        {
            Game g = feed.Game;
            FeedItem result = new FeedItemDebug();
            FeedItemDebug res = (FeedItemDebug)result;

            int ind = CurItem % 4;

            result.Win = g.WinTable[lineCount - 1, (new Random()).Next(0,40)];

            foreach (var map in res.Maps)
            {
                result.Win += map.Win(lineCount);
            }

            CurItem++;

            return result;
        }

        public FeedFake_Var_2(Game g)
            : base(g)
        {
        }
    }

    public class FeedFake_NoFree : FeedFake
    {
        public int CurItem;

        public override FeedItem GetNextItem(Feed feed, int lineCount)
        {
            int variant = Array.IndexOf(feed.Game.Variants, lineCount);
            var fwin = 
                feed.FeedItem[variant].Where(p => p.Bonus == 1 && p.Win > 1);

            if (fwin.Count() > 0) return fwin.ElementAt(feed.Game.rnd.Next(0, fwin.Count()));

            return null;
        }

        public FeedFake_NoFree(Game g)
            : base(g)
        {
        }
    }

}

