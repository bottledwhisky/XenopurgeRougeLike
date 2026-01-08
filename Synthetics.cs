using SpaceCommander;
using System;
using System.Collections.Generic;
using System.Text;
using XenopurgeRougeLike.SyntheticsReinforcements;

namespace XenopurgeRougeLike
{
    public class Synthetics
    {
        public static Dictionary<int, CompanyAffinity> Affinities = new()
        {
            //2：速度+1，瞄准+10，近战伤害+1
            //4：速度+2，瞄准+20，近战伤害+2，开局获得4点接入点数，更高概率获得同流派增援
            //6：速度+3，瞄准+30，近战伤害+3，开局获得8点接入点数
            { 2, new CompanyAffinity
                {
                    unlockLevel = 2,
                    description = "速度+1，瞄准+10，近战伤害+1"
                }
            },
            { 4, new CompanyAffinity
                {
                    unlockLevel = 4,
                    description = "速度+2，瞄准+20，近战伤害+2，开局获得4点接入点数，更高概率获得同流派增援"
                }
            },
            { 6, new CompanyAffinity
                {
                    unlockLevel = 6,
                    description = "速度+3，瞄准+30，近战伤害+3，开局获得8点接入点数"
                }
            }
        };

        public static Dictionary<Type, Reinforcement> Reinforcements = new()
        {
            { typeof(BioSteel), new BioSteel() },
            { typeof(HighPerformanceCluster), new HighPerformanceCluster() },
            { typeof(BattlefieldRepair), new BattlefieldRepair() },
            { typeof(SmartWeaponModule), new SmartWeaponModule() },
            { typeof(SmartWeaponBus), new SmartWeaponBus() },
            { typeof(ReinforcementLearning), new ReinforcementLearning() },
            { typeof(EnhancedHacking), new EnhancedHacking() },
            { typeof(FastHacking), new FastHacking() },
            { typeof(PerfectBeing), new PerfectBeing() },
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
