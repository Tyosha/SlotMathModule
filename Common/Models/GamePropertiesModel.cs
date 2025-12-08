namespace SlotMathModule.Common.Models
{
    public class GamePropertiesModel
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public bool Active { get; set; }
        public short Type { get; set; }
        public short Percent { get; set; }
        public short JackPotPercent { get; set; }
        public int MaxWinLimit { get; set; }
        public int MaxOverdraft { get; set; }
        public string AdditionGameModel { get; set; }
    }
}

