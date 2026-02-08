using SpaceCommander;
using System;
using System.Collections.Generic;
using static XenopurgeRougeLike.ModLocalization;

namespace XenopurgeRougeLike.GunslingerReinforcements
{
    // 枪手路径天赋6：+40瞄准，解锁暴击机制（30%基础概率，200%基础额外伤害）
    // Gunslinger Affinity 6: +40 accuracy, unlock crit mechanic (30% base chance, 200% base extra damage)
    public class CommonAffinity1 : CompanyAffinity
    {
        public CommonAffinity1()
        {
            unlockLevel = 1;
            company = Company.Common;
            description = L("common.affinity1.description");
        }

        public static CommonAffinity1 _instance;
        public static CommonAffinity1 Instance => _instance ??= new();
    }
}
