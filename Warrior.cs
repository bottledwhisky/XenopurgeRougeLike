using SpaceCommander;
using System;
using System.Collections.Generic;

namespace XenopurgeRougeLike
{
    // WIP: Warrior path not yet implemented
    public class Warrior
    {
        public static List<CompanyAffinity> _affinities;
        public static List<CompanyAffinity> Affinities => _affinities ??=
        [
            // TODO: Define warrior affinities
        ];

        public static Dictionary<Type, Reinforcement> _reinforcements;
        public static Dictionary<Type, Reinforcement> Reinforcements => _reinforcements ??= new()
        {
            // TODO: Register warrior reinforcements here
        };

        public static bool IsAvailable()
        {
            // TODO: Define availability conditions
            return true;
        }
    }
}
