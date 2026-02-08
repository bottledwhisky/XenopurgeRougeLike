using System;
using System.Collections.Generic;

namespace XenopurgeRougeLike
{
    // WIP: Clone path not yet implemented
    public class Clone
    {
        public static List<CompanyAffinity> _affinities;
        public static List<CompanyAffinity> Affinities => _affinities ??=
        [
            // TODO: Define clone affinities
        ];

        public static Dictionary<Type, Reinforcement> _reinforcements;
        public static Dictionary<Type, Reinforcement> Reinforcements => _reinforcements ??= [];

        public static bool IsAvailable()
        {
            // TODO: Define availability conditions
            return false;
        }
    }
}
