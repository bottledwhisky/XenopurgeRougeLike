using System;
using System.Collections.Generic;
using static XenopurgeRougeLike.ModLocalization;

namespace XenopurgeRougeLike.RockstarReinforcements
{
    public class RockstarAffinity4 : RockstarAffinityBase
    {
        public const float ReinforcementChanceBonus = 2f;

        public RockstarAffinity4()
        {
            unlockLevel = 4;
            company = Company.Rockstar;
            description = L("rockstar.affinity4.description");
        }

        public static RockstarAffinity4 _instance;

        public static RockstarAffinity4 Instance => _instance ??= new();

        public override void OnActivate()
        {
            RockstarAffinityHelpers.fanMoney += 10;
            RockstarAffinityHelpers.fanGainLow += 1000;
            RockstarAffinityHelpers.fanGainHigh += 1000;
            AwardSystem.WeightModifiers.Add(ModifyWeights);
        }

        public override void OnDeactivate()
        {
            RockstarAffinityHelpers.fanMoney -= 10;
            RockstarAffinityHelpers.fanGainLow -= 1000;
            RockstarAffinityHelpers.fanGainHigh -= 1000;
            AwardSystem.WeightModifiers.Remove(ModifyWeights);
        }

        private void ModifyWeights(List<Tuple<int, Reinforcement>> choices)
        {
            for (int i = 0; i < choices.Count; i++)
            {
                // Check if this reinforcement belongs to Rockstar company
                if (choices[i].Item2.company.Type == CompanyType.Rockstar)
                {
                    int newWeight = (int)(choices[i].Item1 * ReinforcementChanceBonus);
                    choices[i] = new Tuple<int, Reinforcement>(newWeight, choices[i].Item2);
                }
            }
        }

        public override string ToFullDescription()
        {
            return base.ToFullDescription() + $"\n{L("rockstar.affinity2.fan_count", RockstarAffinityHelpers.fanCount)}";
        }
    }
}
