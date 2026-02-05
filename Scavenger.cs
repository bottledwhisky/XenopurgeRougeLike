using System;
using System.Collections.Generic;

namespace XenopurgeRougeLike
{
    // WIP: Scavenger path not yet implemented
    public class Scavenger
    {
        public static List<CompanyAffinity> _affinities;
        public static List<CompanyAffinity> Affinities => _affinities ??=
        [
            // TODO: Define scavenger affinities
        ];

        public static Dictionary<Type, Reinforcement> _reinforcements;
        public static Dictionary<Type, Reinforcement> Reinforcements => _reinforcements ??= new()
        {
            // TODO: Register scavenger reinforcements here
        };

        public static bool IsAvailable()
        {
            // TODO: Define availability conditions
            return true;
        }
    }
}
