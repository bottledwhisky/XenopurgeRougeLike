using System;
using System.Collections.Generic;
using static XenopurgeRougeLike.ModLocalization;

namespace XenopurgeRougeLike.RockstarReinforcements
{
    public class RockstarAffinity6 : RockstarAffinityBase
    {
        public const float ReinforcementChanceBonus = 2f;

        public RockstarAffinity6()
        {
            unlockLevel = 6;
            company = Company.Rockstar;
            description = L("rockstar.affinity6.description");
        }

        public static RockstarAffinity6 _instance;

        public static RockstarAffinity6 Instance => _instance ??= new();

        public override void OnActivate()
        {
            RockstarAffinityHelpers.fanMoney += 30;
            RockstarAffinityHelpers.fanGainLow += 2000;
            RockstarAffinityHelpers.fanGainHigh += 2000;
            AwardSystem.WeightModifiers.Add(ModifyWeights);
        }

        public override void OnDeactivate()
        {
            RockstarAffinityHelpers.fanMoney -= 30;
            RockstarAffinityHelpers.fanGainLow -= 2000;
            RockstarAffinityHelpers.fanGainHigh -= 2000;
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
