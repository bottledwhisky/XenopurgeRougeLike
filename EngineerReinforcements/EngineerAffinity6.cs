using HarmonyLib;
using SpaceCommander;
using SpaceCommander.ActionCards;
using System;
using System.Collections.Generic;
using static XenopurgeRougeLike.ModLocalization;

namespace XenopurgeRougeLike.EngineerReinforcements
{
    public class EngineerAffinity6 : CompanyAffinity
    {
        public const float ExplosiveDamageMultiplier = 2f;
        public const float FlashbangDurationMultiplier = 2f;
        public const int ShopProbabilityBoostCopies = 4;
        public const float ReinforcementChanceBonus = 2f;
        public const int BonusUses = 1;

        public EngineerAffinity6()
        {
            unlockLevel = 6;
            company = Company.Engineer;
            int explosiveDamagePercent = (int)((ExplosiveDamageMultiplier - 1f) * 100);
            int flashbangDurationPercent = (int)((FlashbangDurationMultiplier - 1f) * 100);
            description = L("engineer.affinity6.description", explosiveDamagePercent, flashbangDurationPercent, BonusUses);
        }

        public static EngineerAffinity6 _instance;

        public static EngineerAffinity6 Instance => _instance ??= new();

        public override void OnActivate()
        {
            // Register shop probability modifier for Engineer action cards
            ActionCardsUpgraderTools.RegisterProbabilityModifier(
                "EngineerAffinity6_ShopBoost",
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
            ActionCardsUpgraderTools.UnregisterProbabilityModifier("EngineerAffinity6_ShopBoost");

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
