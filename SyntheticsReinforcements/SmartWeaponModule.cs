using System;
using System.Collections.Generic;
using HarmonyLib;
using SpaceCommander;
using SpaceCommander.Abilities;
using UnityEngine;
using static XenopurgeRougeLike.ModLocalization;

namespace XenopurgeRougeLike.SyntheticsReinforcements
{
    // Smart Weapon Module: Gain one of three smart weapons (Hemogrip, BARK, TRAC), smart weapon upgrade cap +50%, and Hemogrip gets +2 damage.
    public class SmartWeaponModule : SmartWeaponReinforcementBase
    {
        public const float UpgradeCapMultiplier = 1.5f;
        public const int HemogripDamageBonus = 2;

        public SmartWeaponModule()
        {
            name = L("synthetics.smart_weapon_module.name");
            description = L("synthetics.smart_weapon_module.description", (int)((UpgradeCapMultiplier - 1) * 100), HemogripDamageBonus);
            rarity = Rarity.Elite;
            flavourText = L("synthetics.smart_weapon_module.flavour");
        }

        protected static SmartWeaponModule instance;
        public static SmartWeaponModule Instance => instance ??= new();
        public override void OnActivate()
        {
            Instance.EquipWeapon();
        }
    }

    /// <summary>
    /// Patch to increase smart weapon upgrade caps by 50%
    /// Applies to BARK, TRAC, and Hemogrip abilities
    /// </summary>
    [HarmonyPatch(typeof(OnKillEnemyChangeOwnerStatsAbility), MethodType.Constructor, [typeof(List<StatChange>), typeof(int), typeof(Enumerations.CommandCategories)])]
    public static class SmartWeaponModule_IncreaseUpgradeCap_Patch
    {
        public static void Postfix(OnKillEnemyChangeOwnerStatsAbility __instance, ref int maxKills)
        {
            if (!SmartWeaponModule.Instance.IsActive)
                return;

            // Access the private _maxKills field using Harmony
            var maxKillsField = AccessTools.Field(typeof(OnKillEnemyChangeOwnerStatsAbility), "_maxKills");
            if (maxKillsField != null)
            {
                int currentMaxKills = (int)maxKillsField.GetValue(__instance);
                // Increase by 50% (e.g., 5 -> 7, 6 -> 9)
                int newMaxKills = (int)Math.Floor(currentMaxKills * SmartWeaponModule.UpgradeCapMultiplier);
                maxKillsField.SetValue(__instance, newMaxKills);

                Debug.Log($"SmartWeaponModule: Increased weapon upgrade cap from {currentMaxKills} to {newMaxKills}");
            }
        }
    }

    /// <summary>
    /// Patch to add +2 damage to units equipped with Hemogrip
    /// </summary>
    [HarmonyPatch(typeof(BattleUnit), "Power", MethodType.Getter)]
    public static class SmartWeaponModule_HemogripDamageBoost_Patch
    {
        public static void Postfix(BattleUnit __instance, ref float __result)
        {
            if (!SmartWeaponModule.Instance.IsActive)
                return;

            // Check if this unit has Hemogrip equipped
            var meleeWeapon = __instance.MeleeWeaponDataSO;
            if (meleeWeapon != null && meleeWeapon.Id == SmartWeaponReinforcementBase.HEMOGRIP_ID)
            {
                __result += SmartWeaponModule.HemogripDamageBonus;
            }
        }
    }
}
