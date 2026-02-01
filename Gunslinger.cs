using SpaceCommander;
using System;
using System.Collections.Generic;
using XenopurgeRougeLike.GunslingerReinforcements;

namespace XenopurgeRougeLike
{
    // WIP: Gunslinger path not yet implemented
    public class Gunslinger
    {
        public static List<CompanyAffinity> _affinities;
        public static List<CompanyAffinity> Affinities => _affinities ??=
        [
            GunslingerAffinity2.Instance,
        ];

        public static Dictionary<Type, Reinforcement> _reinforcements;
        public static Dictionary<Type, Reinforcement> Reinforcements => _reinforcements ??= new()
        {
            { typeof(TargetingWeakspots), TargetingWeakspots.Instance },
            { typeof(SteadyShot), SteadyShot.Instance },
            { typeof(EnhancedSuppression), EnhancedSuppression.Instance },
            { typeof(AssaultTraining), AssaultTraining.Instance },
            { typeof(Ricochet), Ricochet.Instance },
            { typeof(AreaSuppression), AreaSuppression.Instance },
            { typeof(DeathsEye), DeathsEye.Instance },
            { typeof(PenetratingRounds), PenetratingRounds.Instance },
            { typeof(QuickDraw), QuickDraw.Instance },
        };

        public static bool IsAvailable()
        {
            return false;
        }
    }
}
