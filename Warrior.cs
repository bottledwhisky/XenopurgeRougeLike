using SpaceCommander;
using System;
using System.Collections.Generic;
using XenopurgeRougeLike.WarriorReinforcements;

namespace XenopurgeRougeLike
{
    // WIP: Warrior path not yet implemented
    public class Warrior
    {
        public static List<CompanyAffinity> _affinities;
        public static List<CompanyAffinity> Affinities => _affinities ??=
        [
            WarriorAffinity2.Instance,
            WarriorAffinity4.Instance,
            WarriorAffinity6.Instance,
        ];

        public static Dictionary<Type, Reinforcement> _reinforcements;
        public static Dictionary<Type, Reinforcement> Reinforcements => _reinforcements ??= new()
        {
            { typeof(UkemiTraining), UkemiTraining.Instance },
            { typeof(BlockTraining), BlockTraining.Instance },
            { typeof(ShotgunCloseRange), ShotgunCloseRange.Instance },
            { typeof(MeleeMaster), MeleeMaster.Instance },
            { typeof(Unyielding), Unyielding.Instance },
            { typeof(Stimulants), Stimulants.Instance },
            { typeof(Bloodlust), Bloodlust.Instance },
            { typeof(Berserker), Berserker.Instance },
            { typeof(WhirlwindSlash), WhirlwindSlash.Instance },
        };

        public static bool IsAvailable()
        {
            return false;
        }
    }
}
