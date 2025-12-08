using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
 
using SlotMathModule.GameLogic.Core;

namespace SlotMathModule.GameLogic.Games
{
    [GameIdentity(true)]
    public class Slot5_3 : Game
    {
        protected override void CreateLines()
        {
            MaxLine = 5;
            MapSize = 9;

            AddLine(new Line(new int[3] { 3, 4, 5 }));

            AddLine(new Line(new int[3] { 0, 1, 2 }));
            AddLine(new Line(new int[3] { 6, 7, 8 }));

            AddLine(new Line(new int[3] { 0, 4, 8 }));
            AddLine(new Line(new int[3] { 6, 4, 2 }));

            Variants = new int[5] { 10, 10, 10, 10, 10 };
        }

        public override List<Map> GetMapByFeedItem(FeedItem feedItem_, int lineCount_, int bet_)
        {
            List<Map> res = new List<Map>();
            if (feedItem_ != null)
            {
                res.Add(GetMapByWin(lineCount_, feedItem_.Win).FillMap(lineCount_));
            }
            else
                res.Add(new Map(this, new int[9] { 0, 0, 0, 0, 0, 0, 0, 0, 0 }).FillMap(lineCount_));

            return res;
        }

    }
}

