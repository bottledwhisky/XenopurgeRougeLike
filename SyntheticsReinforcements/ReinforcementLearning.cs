using System;
using System.Collections.Generic;
using HarmonyLib;
using SpaceCommander;
using SpaceCommander.Abilities;
using SpaceCommander.Area;
using SpaceCommander.Database;
using SpaceCommander.Weapons;
using UnityEngine;

namespace XenopurgeRougeLike.SyntheticsReinforcements
{
    // 强化学习：同时获得血斧、bark、trac三种武器，开局即获得智能武器提升上限的50%增幅，血斧在任务开始时提供额外10点护甲
    public class ReinforcementLearning : SmartWeaponReinforcementBase
    {
        private bool weaponsEquipped = false;

        public ReinforcementLearning()
        {
            name = "Reinforcement Learning";
            description = "Receive all three smart weapons (Hemogrip, BARK System, and TRAC Carbine). At mission start, smart weapons gain 50% of their max upgrade stacks immediately. Hemogrip grants +10 armor at mission start.";
            rarity = Rarity.Expert;
            flavourText = "Combat data is analyzed in real-time, allowing the synthetic to achieve peak weapon synchronization from the moment of deployment.";
        }

        public static ReinforcementLearning Instance => (ReinforcementLearning)Synthetics.Reinforcements[typeof(ReinforcementLearning)];

        /// <summary>
        /// Equip all three smart weapons
        /// </summary>
        public void EquipAllWeapons()
        {
            if (weaponsEquipped)
                return;

            weaponsEquipped = true;

            // Equip all three weapons using the base class method
            EquipWeaponById(HEMOGRIP_ID);
            EquipWeaponById(BARK_ID);
            EquipWeaponById(TRAC_ID);

            Debug.Log("ReinforcementLearning: Equipped all three smart weapons (Hemogrip, BARK, TRAC)");
        }

        public override void OnActivate()
        {
            Instance.EquipAllWeapons();
        }
    }

    /// <summary>
    /// Patch ActivateAbility to grant 50% of max kills immediately at mission start
    /// </summary>
    [HarmonyPatch(typeof(OnKillEnemyChangeOwnerStatsAbility), "ActivateAbility")]
    public static class ReinforcementLearning_ActivateAbility_Patch
    {
        public static void Postfix(OnKillEnemyChangeOwnerStatsAbility __instance)
        {
            if (!ReinforcementLearning.Instance.IsActive)
                return;

            // Access private fields
            var maxKillsField = AccessTools.Field(typeof(OnKillEnemyChangeOwnerStatsAbility), "_maxKills");
            var currentKillsField = AccessTools.Field(typeof(OnKillEnemyChangeOwnerStatsAbility), "_currentKills");
            var statChangesField = AccessTools.Field(typeof(OnKillEnemyChangeOwnerStatsAbility), "_statChanges");
            var ownerField = AccessTools.Field(typeof(OnKillEnemyChangeOwnerStatsAbility), "_owner");
            var abilityIdField = AccessTools.Field(typeof(OnKillEnemyChangeOwnerStatsAbility), "_abilityId");

            if (maxKillsField == null || currentKillsField == null || statChangesField == null || ownerField == null || abilityIdField == null)
            {
                Debug.LogError("ReinforcementLearning: Could not access required fields");
                return;
            }

            int maxKills = (int)maxKillsField.GetValue(__instance);
            int startingKills = (int)Math.Floor(maxKills * 0.5f); // 50% of max

            if (startingKills <= 0)
                return;

            var statChanges = (List<StatChange>)statChangesField.GetValue(__instance);
            var owner = (BattleUnit)ownerField.GetValue(__instance);
            var abilityIdList = (List<string>)abilityIdField.GetValue(__instance);

            if (statChanges == null || owner == null || abilityIdList == null)
                return;

            // Apply stat changes for each starting kill
            for (int i = 0; i < startingKills; i++)
            {
                string abilityId = Guid.NewGuid().ToString();
                abilityIdList.Add(abilityId);

                foreach (var statChange in statChanges)
                {
                    owner.ChangeStat(statChange.StatToChange, statChange.ChangeValueOfStat, abilityId);
                }
            }

            // Update current kills
            currentKillsField.SetValue(__instance, startingKills);

            Debug.Log($"ReinforcementLearning: Granted {startingKills} starting kills (50% of {maxKills} max) to {owner.UnitName}");
        }
    }

    /// <summary>
    /// Patch BattleUnit constructor to add +10 armor at mission start for units with Hemogrip
    /// </summary>
    [HarmonyPatch(typeof(BattleUnit), MethodType.Constructor)]
    [HarmonyPatch([typeof(UnitData), typeof(Enumerations.Team), typeof(GridManager)])]
    public static class ReinforcementLearning_BattleUnitConstructor_Patch
    {
        public static void Postfix(BattleUnit __instance, Enumerations.Team team)
        {
            if (team != Enumerations.Team.Player)
                return;

            if (!ReinforcementLearning.Instance.IsActive)
                return;

            // Check if this unit has Hemogrip equipped
            var meleeWeapon = __instance.MeleeWeaponDataSO;
            if (meleeWeapon != null && meleeWeapon.Id == SmartWeaponReinforcementBase.HEMOGRIP_ID)
            {
                var armorField = AccessTools.Field(typeof(BattleUnit), "_currentArmor");
                if (armorField != null)
                {
                    float currentArmor = (float)armorField.GetValue(__instance);
                    armorField.SetValue(__instance, currentArmor + 10f);
                    Debug.Log($"ReinforcementLearning: Added +10 armor to {__instance.UnitName} with Hemogrip");
                }
            }
        }
    }
}
