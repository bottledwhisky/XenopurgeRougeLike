using System;
using System.Collections.Generic;

namespace XenopurgeRougeLike
{
    // WIP: Support path not yet implemented
    public class Support
    {
        public static List<CompanyAffinity> _affinities;
        public static List<CompanyAffinity> Affinities => _affinities ??=
        [
            // TODO: Define support affinities
        ];

        public static Dictionary<Type, Reinforcement> _reinforcements;
        public static Dictionary<Type, Reinforcement> Reinforcements => _reinforcements ??= new()
        {
            // TODO: Register support reinforcements here
        };

        public static bool IsAvailable()
        {
            // TODO: Define availability conditions
            return true;
        }
    }
}
