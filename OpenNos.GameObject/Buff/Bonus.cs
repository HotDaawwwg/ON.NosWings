﻿

namespace OpenNos.GameObject.BcardsBonus
{
    public class Bonus
    {
        public Bonus()
        {
            // CardType // Additional Type // Data // IsLevelScaled ( 0 = no / 1 = IsLevelDivided / 2 = IsLevelMultiplied)
            Number = new int[90,60,1,2];
        }

        public int[,,,] Number { get; set; }
    }
}
