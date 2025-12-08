namespace SlotMathModule.GameLogic.Core
{
    public class Symbol
    {
        public String Name { get; set; }
        public int[] Wins { get; set; }
        public int Index { get; set; }
        public bool NotUsedForFIll;

        public Symbol(string name_)
        {
            Name = name_;
        }

        public Symbol(string name_, int[] wins_, bool notUsedForFill = false )
        {
            Name = name_;
            Wins = wins_;
            NotUsedForFIll = notUsedForFill;
        }
    }
}

