using System;
using System.Collections.Generic;
using System.Text;

namespace XenopurgeRougeLike.RockstarReinforcements
{
    public class ScreenUsed: Reinforcement
    {
        public ScreenUsed()
        {
            company = Company.Rockstar;
            name = "Screen-used Props";
            description = "You can sell your equipments at the full price plus the number of missions you completed with them.";
        }
    }
}
