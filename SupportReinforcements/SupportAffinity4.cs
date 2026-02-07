using System;
using System.Collections.Generic;
using static XenopurgeRougeLike.ModLocalization;

namespace XenopurgeRougeLike.SupportReinforcements
{
    /// <summary>
    /// Support Affinity 4: Injection duration +50%, heal item effectiveness +50%, increased shop spawn rate, same-company reinforcement boost
    /// 支援兵天赋4：药剂持续时间+50%，回复品效果+50%，指令商店出现药剂和回复品的概率提升，同流派增援获得概率提升
    /// </summary>
    public class SupportAffinity4 : CompanyAffinity
    {
        public const float InjectionDurationMultiplier = 1.5f;
        public const float HealEffectivenessMultiplier = 1.5f;
        public const int ShopProbabilityBoostCopies = 3;
        public const float ReinforcementChanceBonus = 2f;

        public SupportAffinity4()
        {
            unlockLevel = 4;
            company = Company.Support;
            int durationPercent = (int)((InjectionDurationMultiplier - 1f) * 100);
            int healPercent = (int)((HealEffectivenessMultiplier - 1f) * 100);
            description = L("support.affinity4.description", durationPercent, healPercent);
        }

        public static SupportAffinity4 _instance;

        public static SupportAffinity4 Instance => _instance ??= new();

        public override void OnActivate()
        {
            // Register shop probability modifier for Support action cards
            ActionCardsUpgraderTools.RegisterProbabilityModifier(
                "SupportAffinity4_ShopBoost",
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
            ActionCardsUpgraderTools.UnregisterProbabilityModifier("SupportAffinity4_ShopBoost");

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
