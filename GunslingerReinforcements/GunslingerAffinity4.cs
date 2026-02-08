using SpaceCommander;
using System;
using System.Collections.Generic;
using static XenopurgeRougeLike.ModLocalization;

namespace XenopurgeRougeLike.GunslingerReinforcements
{
    // 枪手路径天赋4：+30瞄准，解锁暴击机制（20%基础概率，150%基础额外伤害）
    // Gunslinger Affinity 4: +30 accuracy, unlock crit mechanic (20% base chance, 150% base extra damage)
    public class GunslingerAffinity4 : CompanyAffinity
    {
        public const float AccuracyBonus = .3f;
        public const float CritChance = 0.20f;
        public const float CritDamageMultiplier = 1.5f; // 150% extra damage = 2.5x total damage
        public const float ReinforcementChanceBonus = 2f;

        public GunslingerAffinity4()
        {
            unlockLevel = 4;
            company = Company.Gunslinger;
            description = L("gunslinger.affinity4.description", (int)(AccuracyBonus * 100), (int)(CritChance * 100), (int)(CritDamageMultiplier * 100));
        }

        public static GunslingerAffinity4 _instance;
        public static GunslingerAffinity4 Instance => _instance ??= new();

        public override void OnActivate()
        {
            // Register accuracy boost
            UnitStatsTools.InBattleUnitStatChanges["GunslingerAffinity4_Accuracy"] = new UnitStatChange(
                Enumerations.UnitStats.Accuracy,
                AccuracyBonus,
                Enumerations.Team.Player
            );

            // Register reinforcement weight modifier
            AwardSystem.WeightModifiers.Add(ModifyWeights);
        }

        public override void OnDeactivate()
        {
            // Remove accuracy boost
            UnitStatsTools.InBattleUnitStatChanges.Remove("GunslingerAffinity4_Accuracy");

            // Remove reinforcement weight modifier
            AwardSystem.WeightModifiers.Remove(ModifyWeights);
        }

        private void ModifyWeights(List<Tuple<int, Reinforcement>> choices)
        {
            for (int i = 0; i < choices.Count; i++)
            {
                // Check if this reinforcement belongs to Gunslinger company
                if (choices[i].Item2.company.Type == CompanyType.Gunslinger)
                {
                    int newWeight = (int)(choices[i].Item1 * ReinforcementChanceBonus);
                    choices[i] = new Tuple<int, Reinforcement>(newWeight, choices[i].Item2);
                }
            }
        }
    }
}
