using System;
using System.Collections.Generic;
using HarmonyLib;
using SpaceCommander;
using SpaceCommander.Abilities;
using UnityEngine;

namespace XenopurgeRougeLike.SyntheticsReinforcements
{
    // 智能武器模块：获得Hemogrip、bark、trac三者之一，智能武器的提升上限+50%，血斧+2伤害
    // 装备逻辑：如果获得Hemogrip，寻找装备着Combat Knife的Power最高的单位
    // 否则寻找装备着MOP多用途手枪的Accuracy最高的单位
    // 如果没有这样的单位，则给玩家PlayerWallet加入对应武器的BuyingPrice
    public class SmartWeaponModule : SmartWeaponReinforcementBase
    {
        public SmartWeaponModule()
        {
            name = "Smart Weapon Module";
            description = "Receive one of three smart weapons (Hemogrip, BARK System, or TRAC Carbine). Smart weapon upgrade caps increased by 50%. Hemogrip gains +2 damage.";
            rarity = Rarity.Elite;
            flavourText = "Factory-installed interface ports allow direct neural integration with Weyland-Yutani smart weapon systems.";
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
                int newMaxKills = (int)Math.Floor(currentMaxKills * 1.5f);
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
                __result += 2f;
            }
        }
    }
}
