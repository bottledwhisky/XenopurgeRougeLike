using SpaceCommander;
using System;
using System.Collections.Generic;
using XenopurgeRougeLike.SyntheticsReinforcements;

namespace XenopurgeRougeLike
{
    public class Synthetics
    {
        public static List<CompanyAffinity> _affinities;
        public static List<CompanyAffinity> Affinities => _affinities ??=
        [
            //2：速度+1，瞄准+10，近战伤害+1
            //4：速度+2，瞄准+20，近战伤害+2，开局获得4点接入点数，更高概率获得同流派增援
            //6：速度+3，瞄准+30，近战伤害+3，开局获得8点接入点数
            SyntheticsAffinity2.Instance,
            SyntheticsAffinity4.Instance,
            SyntheticsAffinity6.Instance,
        ];

        public static Dictionary<Type, Reinforcement> _reinforcements;
        public static Dictionary<Type, Reinforcement> Reinforcements => _reinforcements ??= new()
        {
            { typeof(BioSteel), BioSteel.Instance },
            { typeof(HighPerformanceCluster), HighPerformanceCluster.Instance },
            { typeof(BattlefieldRepair), BattlefieldRepair.Instance },
            { typeof(SmartWeaponModule), SmartWeaponModule.Instance },
            { typeof(SmartWeaponBus), SmartWeaponBus.Instance },
            { typeof(ReinforcementLearning), ReinforcementLearning.Instance },
            { typeof(EnhancedHacking), EnhancedHacking.Instance },
            { typeof(FastHacking), FastHacking.Instance },
            { typeof(PerfectBeing), PerfectBeing.Instance },
        };

        public static bool IsAvailable()
        {
            PlayerData playerData = Singleton<Player>.Instance.PlayerData;
            return playerData.AccessPointsSystemEnabled;
        }

        public static BioSteel BioSteel => (BioSteel)Reinforcements[typeof(BioSteel)];
        public static HighPerformanceCluster HighPerformanceCluster => (HighPerformanceCluster)Reinforcements[typeof(HighPerformanceCluster)];
        public static BattlefieldRepair BattlefieldRepair => (BattlefieldRepair)Reinforcements[typeof(BattlefieldRepair)];
        public static SmartWeaponModule SmartWeaponModule => (SmartWeaponModule)Reinforcements[typeof(SmartWeaponModule)];
        public static ReinforcementLearning ReinforcementLearning => (ReinforcementLearning)Reinforcements[typeof(ReinforcementLearning)];
        public static EnhancedHacking EnhancedHacking => (EnhancedHacking)Reinforcements[typeof(EnhancedHacking)];
        public static FastHacking FastHacking => (FastHacking)Reinforcements[typeof(FastHacking)];
        public static PerfectBeing PerfectBeing => (PerfectBeing)Reinforcements[typeof(PerfectBeing)];
    }
}
