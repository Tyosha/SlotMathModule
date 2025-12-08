using SlotMathModule.GameLogic.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
 

namespace SlotMathModule.GameLogic.Core
{
    public class Play
    {
        #region [Properties]

        public Slot Slot;
        public int Bet=1;
        public int Lines=1;

        public int CashIn;
        public int CashOut;
        public int AverageWin;
        public int SpinCount;
        public int WinCount;
        public int MaxWin;

        public int FeedNumber {
            get { return 0; }
            set
            { 
                r = new Random( value );
                ResetCounters();
            }
        }
        public int TicketNumber;

        private Random r;

        public int CurWin;

        #endregion

        #region [Debug]

        public bool LogMatrix = false;
        public bool LogSummaryOnSpin = false;

        #endregion

        public void ResetCounters()
        {
            CashIn = 0;
            CashOut = 0;
            AverageWin = 0;
            SpinCount = 0;
            WinCount = 0;
            MaxWin = 0;
        }

        public Play(Slot s)
        {
            Slot = s;
            ResetCounters();
        }

        public void Spin()
        {

            SpinCount++;

            // генерируем комбинацию
            Slot.Lines = Lines;
            Slot.Bet = Bet;

            int result = Slot.Spin();
            if (result == 301) Slot.BuyTicket(Slot.Feed.CurTicket+1);

            CurWin = Slot.CurWin;
            if (CurWin > 0) WinCount++;
            if (CurWin > MaxWin) MaxWin = CurWin;
            CashOut += CurWin;
            if (CurWin > 0) AverageWin = CashOut / WinCount;


            if (LogSummaryOnSpin) 
            {
                if (Slot.BonusGameRemain == 0 && Slot.BonusWin > 0) Console.WriteLine("\n $$$$$$$$$$$ BonusWin = {0} $$$$$$$$$$$$ \n", Slot.BonusWin);

                if (Slot.BonusGame)          Console.WriteLine("\n $$$$$$$$$$$ Bonus $$$$$$$$$$$$");
                if (Slot.BonusGameRemain > 0) Console.WriteLine("  BonusGameRemain = {0}", Slot.BonusGameRemain);

                //OutCashSummary();
                Slot.OutSummary();
            }

            if (LogMatrix || LogSummaryOnSpin) Console.ReadLine();
        }

        public void SpinMany(int spinCount_, int spinCountReport = 0)
        {
            for (int n = 0; n < spinCount_; n++)
            {
                Spin();
               
                if (spinCountReport > 0 && (n+1) % spinCountReport == 0) Slot.OutSummary();
            }
        }

        public void OutCashSummary()
        {
            if (Slot.CashIn == 0) Slot.CashIn = 1;
            Console.WriteLine(String.Format("Credits = {0}    CashIn/CashOut {1}:{2} = {3}%", Slot.Credits, Slot.CashOut, Slot.CashIn, Slot.CashOut * 100 / Slot.CashIn));
        }

        public void OutFinalResult()
        {
            Console.WriteLine("");
            Console.WriteLine("======== Play final result ===========");
            OutCashSummary();
            if (SpinCount > 0) Console.WriteLine(String.Format("MaxWin={0} Average={1} PercentSpinWin={2}", MaxWin, AverageWin, WinCount*100/SpinCount));
            Console.WriteLine("=================================");
        }
    }
}

