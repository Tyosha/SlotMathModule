using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
 

namespace SlotMathModule.GameLogic.Games
{
    public class GameIdentity : System.Attribute
    {
        private bool isGame;
        
        public GameIdentity(bool isGame)
        {
            this.isGame = isGame;
        }
    }
}
