using HarmonyLib;
using SpaceCommander;
using SpaceCommander.Area;
using System;
using System.Collections.Generic;
using UnityEngine;
using static SpaceCommander.Enumerations;

namespace XenopurgeRougeLike.XenoReinforcements
{
    public class XenoAffinity4 : CompanyAffinity
    {
        public XenoAffinity4()
        {
            unlockLevel = 4;
            company = Company.Xeno;
            description = "对异形伤害+45%，控制/发狂持续时间+75%，更高概率获得同流派增援";
        }

        // Damage bonus constants
        public const float XenoDamageBonus = 0.45f; // +45% damage to xenos

        // Control/Madness duration bonus - to be used later
        public const float ControlDurationBonus = 0.75f; // +75% duration

        // Reinforcement chance multiplier
        public const float ReinforcementChanceBonus = 2f;

        public static XenoAffinity4 _instance;

        public static XenoAffinity4 Instance => _instance ??= new();

        public override void OnActivate()
        {
            XenopurgeRougeLike.WeightModifiers.Add(ModifyWeights);
        }

        public override void OnDeactivate()
        {
            XenopurgeRougeLike.WeightModifiers.Remove(ModifyWeights);
        }

        private void ModifyWeights(List<Tuple<int, Reinforcement>> choices)
        {
            for (int i = 0; i < choices.Count; i++)
            {
                // Check if this reinforcement belongs to Xeno company
                if (choices[i].Item2.company.Type == CompanyType.Xeno)
                {
                    int newWeight = (int)(choices[i].Item1 * ReinforcementChanceBonus);
                    choices[i] = new Tuple<int, Reinforcement>(newWeight, choices[i].Item2);
                }
            }
        }
    }

    // Patch BattleUnit.Damage to increase damage dealt to xenos
    [HarmonyPatch(typeof(BattleUnit), "Damage")]
    public static class XenoAffinity4_Damage_Patch
    {
        public static void Prefix(BattleUnit __instance, ref float damage)
        {
            if (!XenoAffinity4.Instance.IsActive)
                return;

            // Check if the damaged unit is an enemy (xeno)
            if (__instance.Team != Team.EnemyAI)
                return;

            // Increase damage dealt to xenos
            damage *= (1f + XenoAffinity4.XenoDamageBonus);
        }
    }
}
