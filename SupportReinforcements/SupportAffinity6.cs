using System;
using System.Collections.Generic;
using static XenopurgeRougeLike.ModLocalization;

namespace XenopurgeRougeLike.SupportReinforcements
{
    /// <summary>
    /// Support Affinity 6: Injection duration +100%, heal item effectiveness +100%, increased shop spawn rate, same-company reinforcement boost, +1 uses
    /// 支援兵天赋6：药剂持续时间+100%，回复品效果+100%，指令商店出现药剂和回复品的概率提升，同流派增援获得概率提升，药剂和回复品使用次数+1
    /// </summary>
    public class SupportAffinity6 : CompanyAffinity
    {
        public const float InjectionDurationMultiplier = 2f;
        public const float HealEffectivenessMultiplier = 2f;
        public const int ShopProbabilityBoostCopies = 4;
        public const float ReinforcementChanceBonus = 2f;
        public const int BonusUses = 1;

        public SupportAffinity6()
        {
            unlockLevel = 6;
            company = Company.Support;
            int durationPercent = (int)((InjectionDurationMultiplier - 1f) * 100);
            int healPercent = (int)((HealEffectivenessMultiplier - 1f) * 100);
            description = L("support.affinity6.description", durationPercent, healPercent, BonusUses);
        }

        public static SupportAffinity6 _instance;

        public static SupportAffinity6 Instance => _instance ??= new();

        public override void OnActivate()
        {
            // Register shop probability modifier for Support action cards
            ActionCardsUpgraderTools.RegisterProbabilityModifier(
                "SupportAffinity6_ShopBoost",
                SupportAffinity2.SupportActionCards,
                ShopProbabilityBoostCopies,
                () => Instance.IsActive
            );

            // Register reinforcement weight modifier
            AwardSystem.WeightModifiers.Add(ModifyWeights);
        }

        public override void OnDeactivate()
        {
            // Unregister shop probability modifier
            ActionCardsUpgraderTools.UnregisterProbabilityModifier("SupportAffinity6_ShopBoost");

            // Unregister reinforcement weight modifier
            AwardSystem.WeightModifiers.Remove(ModifyWeights);
        }

        private void ModifyWeights(List<Tuple<int, Reinforcement>> choices)
        {
            for (int i = 0; i < choices.Count; i++)
            {
                // Check if this reinforcement belongs to Support company
                if (choices[i].Item2.company.Type == CompanyType.Support)
                {
                    int newWeight = (int)(choices[i].Item1 * ReinforcementChanceBonus);
                    choices[i] = new Tuple<int, Reinforcement>(newWeight, choices[i].Item2);
                }
            }
        }
    }
}
