namespace SlotMathModule.Common.Models
{
    public class TiketGameModel
    {
        public int TotalWin { get; set; }
        public long Balance { get; set; }     // баланс игры
        public bool IsJackpot { get; set; }     // баланс игры

        public TiketGameModel()
        {
            IsJackpot = false;
        }
    }
}

