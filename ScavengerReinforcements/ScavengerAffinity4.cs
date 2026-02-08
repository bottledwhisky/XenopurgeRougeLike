using System;
using System.Collections.Generic;
using static XenopurgeRougeLike.ModLocalization;

namespace XenopurgeRougeLike.ScavengerReinforcements
{
    // Scavenger Affinity Level 4: 50% more collectibles spawn, -50% collection time, higher chance of same-company reinforcements
    public class ScavengerAffinity4 : CompanyAffinity
    {
        public const float CollectibleMultiplier = 1.5f;
        public const float CollectionTimeMultiplier = 0.5f;
        public const float ReinforcementChanceBonus = 2f;

        public ScavengerAffinity4()
        {
            unlockLevel = 4;
            company = Company.Scavenger;
            description = L("scavenger.affinity4.description",
                (int)((CollectibleMultiplier - 1) * 100),
                (int)((1 - CollectionTimeMultiplier) * 100));
        }

        public static ScavengerAffinity4 _instance;
        public static ScavengerAffinity4 Instance => _instance ??= new();

        public override void OnActivate()
        {
            // Implementation is in ScavengerAffinityHelpers
            AwardSystem.WeightModifiers.Add(ModifyWeights);
        }

        public override void OnDeactivate()
        {
            // Implementation is in ScavengerAffinityHelpers
            AwardSystem.WeightModifiers.Remove(ModifyWeights);
        }

        private void ModifyWeights(List<Tuple<int, Reinforcement>> choices)
        {
            for (int i = 0; i < choices.Count; i++)
            {
                // Check if this reinforcement belongs to Scavenger company
                if (choices[i].Item2.company.Type == CompanyType.Scavenger)
                {
                    int newWeight = (int)(choices[i].Item1 * ReinforcementChanceBonus);
                    choices[i] = new Tuple<int, Reinforcement>(newWeight, choices[i].Item2);
                }
            }
        }
    }
}
