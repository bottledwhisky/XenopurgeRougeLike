using System;
using System.Collections.Generic;
using static XenopurgeRougeLike.ModLocalization;

namespace XenopurgeRougeLike.EngineerReinforcements
{
    public class EngineerAffinity4 : CompanyAffinity
    {
        public const float ExplosiveDamageMultiplier = 1.5f;
        public const float FlashbangDurationMultiplier = 2f;
        public const int ShopProbabilityBoostCopies = 3;
        public const float ReinforcementChanceBonus = 2f;

        public EngineerAffinity4()
        {
            unlockLevel = 4;
            company = Company.Engineer;
            int explosiveDamagePercent = (int)((ExplosiveDamageMultiplier - 1f) * 100);
            int flashbangDurationPercent = (int)((FlashbangDurationMultiplier - 1f) * 100);
            description = L("engineer.affinity4.description", explosiveDamagePercent, flashbangDurationPercent);
        }

        public static EngineerAffinity4 _instance;

        public static EngineerAffinity4 Instance => _instance ??= new();

        public override void OnActivate()
        {
            // Register shop probability modifier for Engineer action cards
            ActionCardsUpgraderTools.RegisterProbabilityModifier(
                "EngineerAffinity4_ShopBoost",
                EngineerAffinity2.EngineerActionCards,
                ShopProbabilityBoostCopies,
                () => Instance.IsActive
            );

            // Register reinforcement weight modifier
            AwardSystem.WeightModifiers.Add(ModifyWeights);
        }

        public override void OnDeactivate()
        {
            // Unregister shop probability modifier
            ActionCardsUpgraderTools.UnregisterProbabilityModifier("EngineerAffinity4_ShopBoost");

            // Unregister reinforcement weight modifier
            AwardSystem.WeightModifiers.Remove(ModifyWeights);
        }

        private void ModifyWeights(List<Tuple<int, Reinforcement>> choices)
        {
            for (int i = 0; i < choices.Count; i++)
            {
                // Check if this reinforcement belongs to Engineer company
                if (choices[i].Item2.company.Type == CompanyType.Engineer)
                {
                    int newWeight = (int)(choices[i].Item1 * ReinforcementChanceBonus);
                    choices[i] = new Tuple<int, Reinforcement>(newWeight, choices[i].Item2);
                }
            }
        }
    }
}
