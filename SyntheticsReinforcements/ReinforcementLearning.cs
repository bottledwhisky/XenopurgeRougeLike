using System;
using System.Collections.Generic;
using HarmonyLib;
using SpaceCommander;
using SpaceCommander.Abilities;
using SpaceCommander.Area;
using UnityEngine;
using static XenopurgeRougeLike.ModLocalization;

namespace XenopurgeRougeLike.SyntheticsReinforcements
{
    // Reinforcement Learning: Gain all three smart weapons (Hemogrip, BARK, TRAC), start with 50% of max kills for smart weapon upgrades, and Hemogrip provides +10 armor at mission start.
    public class ReinforcementLearning : SmartWeaponReinforcementBase
    {
        private bool weaponsEquipped = false;
        public const float StartingKillsMultiplier = 0.5f;
        public const int HemogripArmorBonus = 10;

        public ReinforcementLearning()
        {
            name = L("synthetics.reinforcement_learning.name");
            description = L("synthetics.reinforcement_learning.description", (int)(StartingKillsMultiplier * 100), HemogripArmorBonus);
            rarity = Rarity.Expert;
            flavourText = L("synthetics.reinforcement_learning.flavour");
        }

        protected static ReinforcementLearning instance;
        public static ReinforcementLearning Instance => instance ??= new();

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
            int startingKills = (int)Math.Floor(maxKills * ReinforcementLearning.StartingKillsMultiplier); // 50% of max

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
    [HarmonyPatch(typeof(BattleUnit), MethodType.Constructor, [typeof(UnitData), typeof(Enumerations.Team), typeof(GridManager)])]
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
                    armorField.SetValue(__instance, currentArmor + ReinforcementLearning.HemogripArmorBonus);
                    Debug.Log($"ReinforcementLearning: Added +{ReinforcementLearning.HemogripArmorBonus} armor to {__instance.UnitName} with Hemogrip");
                }
            }
        }
    }
}
