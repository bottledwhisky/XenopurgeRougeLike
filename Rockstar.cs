using System;
using System.Collections.Generic;
using System.Text;
using XenopurgeRougeLike.RockstarReinforcements;

namespace XenopurgeRougeLike
{
    public class Rockstar
    {
        internal static Dictionary<int, CompanyAffinity> Affinities;

        public static Dictionary<Type, Reinforcement> Reinforcements
        {
            get
            {
                return new Dictionary<Type, Reinforcement>()
                {
                    { typeof(ScreenUsed), new ScreenUsed() }
                };
            }
        }

        public static ScreenUsed ScreenUsed => (ScreenUsed)Reinforcements[typeof(ScreenUsed)];
        public static bool IsAvailable()
        {
            return false;
        }
    }
}
