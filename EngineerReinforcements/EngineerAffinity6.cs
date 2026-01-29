using HarmonyLib;
using SpaceCommander;
using SpaceCommander.ActionCards;
using System;
using System.Collections.Generic;

namespace XenopurgeRougeLike.EngineerReinforcements
{
    public class EngineerAffinity6 : CompanyAffinity
    {
        public EngineerAffinity6()
        {
            unlockLevel = 6;
            company = Company.Engineer;
            description = "地雷、手雷伤害+100%，闪光弹效果持续时间+100%，指令商店出现地雷、手雷、闪光弹、炮台的概率提升，同流派增援获得概率提升，地雷、手雷、闪光弹使用次数+1";
        }

        public const float ExplosiveDamageMultiplier = 2f;
        public const float FlashbangDurationMultiplier = 2f;
        public const int ShopProbabilityBoostCopies = 4;
        public const float ReinforcementChanceBonus = 2f;
        public const int BonusUses = 1;

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
