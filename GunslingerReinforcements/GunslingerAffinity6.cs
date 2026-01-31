using SpaceCommander;
using static XenopurgeRougeLike.ModLocalization;

namespace XenopurgeRougeLike.GunslingerReinforcements
{
    // 枪手路径天赋6：+40瞄准，解锁暴击机制（30%基础概率，200%基础额外伤害）
    // Gunslinger Affinity 6: +40 accuracy, unlock crit mechanic (30% base chance, 200% base extra damage)
    public class GunslingerAffinity6 : CompanyAffinity
    {
        public const float AccuracyBonus = .4f;
        public const float CritChance = 0.30f;
        public const float CritDamageMultiplier = 2.0f; // 200% extra damage = 3x total damage

        public GunslingerAffinity6()
        {
            unlockLevel = 6;
            company = Company.Gunslinger;
            description = L("gunslinger.affinity6.description", (int)(AccuracyBonus * 100), (int)(CritChance * 100), (int)(CritDamageMultiplier * 100));
        }

        public static GunslingerAffinity6 _instance;
        public static GunslingerAffinity6 Instance => _instance ??= new();

        public override void OnActivate()
        {
            // Register accuracy boost
            UnitStatsTools.InBattleUnitStatChanges["GunslingerAffinity6_Accuracy"] = new UnitStatChange(
                Enumerations.UnitStats.Accuracy,
                AccuracyBonus,
                Enumerations.Team.Player
            );
        }

        public override void OnDeactivate()
        {
            // Remove accuracy boost
            UnitStatsTools.InBattleUnitStatChanges.Remove("GunslingerAffinity6_Accuracy");
        }
    }
}
