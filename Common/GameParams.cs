namespace SlotMathModule.Common
{
    public class GameParams
    {
        //public long GameBalance = 0; //  баланс игры
        //public int Percent = 90; // процент возврата
        //public int JackPotPercent = 1; // процент джекпота
        //public int MaxOverdraft = 0; // макс. уход в минус
        //public int RiskPercent = 50; // процент на риске
        //public int MaxWinLimit = int.MaxValue; // макс. выигрыш
        //public WriteLogDelegate OnLog = null;  // делегат логирования
        //public bool IsFullLog = true; // полный лог

        public int RiskPercent { get; set; } // процент риска
        public bool IsLightTape { get; set; } // вид ленты
        public int MinKoefToBigWin { get; set; } // минимальный кофэффициент, с которого считается крупный выигрыш
        public int FirstPositionBigWin { get; set; } // с какого номера начинаются крупные выигрыши
        public int MinPositionFirstBigWin { get; set; } // с какого номера долен прийти первый крупный выигрыш
        public int MaxPositionFirstBigWin { get; set; } // до какого номера долен прийти первый крупный выигрыш

        public GameParams()
        {
            RiskPercent = 40;
            
            IsLightTape = true;
            MinKoefToBigWin = int.MaxValue;
            FirstPositionBigWin = -1;
            MinPositionFirstBigWin = 0;
            MaxPositionFirstBigWin = 0;
        }
    }
}

