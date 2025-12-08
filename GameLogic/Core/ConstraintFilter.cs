using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
 

namespace SlotMathModule.GameLogic.Core
{
    static public class ConstraintFilter
    {
        static public bool IsValidWin(int win, int bet, int line)
        {
            // if (win > 3500) return false;

            // if (win*bet > 50000) return false;

            return true;
        }
    }
}
