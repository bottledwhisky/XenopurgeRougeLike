using HarmonyLib;
using SpaceCommander;
using SpaceCommander.Area;
using System;
using UnityEngine;
using static SpaceCommander.Enumerations;

namespace XenopurgeRougeLike.XenoReinforcements
{
    public class XenoAffinity2 : CompanyAffinity
    {
        public XenoAffinity2()
        {
            unlockLevel = 2;
            company = Company.Xeno;
            description = "对异形伤害+30%，控制类效果持续时间增加";
        }

        // Damage bonus constants
        public const float XenoDamageBonus = 0.30f; // +30% damage to xenos

        public const int ControlDurationBonusLevel = 1;

        public static XenoAffinity2 _instance;

        public static XenoAffinity2 Instance => _instance ??= new();

        public override void OnActivate()
        {
            // No need to register stat changes here, we'll handle damage in the patch
        }

        public override void OnDeactivate()
        {
            // Nothing to clean up
        }
    }

    // Patch BattleUnit.Damage to increase damage dealt to xenos
    [HarmonyPatch(typeof(BattleUnit), "Damage")]
    public static class XenoAffinity2_Damage_Patch
    {
        public static void Prefix(BattleUnit __instance, ref float damage)
        {
            if (!XenoAffinity2.Instance.IsActive)
                return;

            // Check if the damaged unit is an enemy (xeno)
            if (__instance.Team != Team.EnemyAI)
                return;

            // Increase damage dealt to xenos
            damage *= (1f + XenoAffinity2.XenoDamageBonus);
        }
    }
}
