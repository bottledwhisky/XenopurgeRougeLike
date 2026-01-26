using SpaceCommander;
using System;
using System.Collections.Generic;

namespace XenopurgeRougeLike
{
    // WIP: Gunslinger path not yet implemented
    public class Gunslinger
    {
        public static List<CompanyAffinity> _affinities;
        public static List<CompanyAffinity> Affinities => _affinities ??=
        [
            // TODO: Define gunslinger affinities
        ];

        public static Dictionary<Type, Reinforcement> _reinforcements;
        public static Dictionary<Type, Reinforcement> Reinforcements => _reinforcements ??= new()
        {
            // TODO: Register gunslinger reinforcements here
        };

        public static bool IsAvailable()
        {
            // TODO: Define availability conditions
            return true;
        }
    }
}
