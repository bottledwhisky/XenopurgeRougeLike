using System.Collections.Generic;
using HarmonyLib;
using SpaceCommander;
using SpaceCommander.Abilities;
using UnityEngine;
using static XenopurgeRougeLike.ModLocalization;

namespace XenopurgeRougeLike.SyntheticsReinforcements
{
    // Smart Weapon Bus: Gain one of three smart weapons (Hemogrip, BARK, TRAC), and smart weapon stat improvements are increased by 50%.
    public class SmartWeaponBus : SmartWeaponReinforcementBase
    {
        public const float StatImprovementMultiplier = 1.5f;

        public SmartWeaponBus()
        {
            name = L("synthetics.smart_weapon_bus.name");
            description = L("synthetics.smart_weapon_bus.description", (int)((StatImprovementMultiplier - 1) * 100));
            rarity = Rarity.Elite;
            flavourText = L("synthetics.smart_weapon_bus.flavour");
        }

        protected static SmartWeaponBus instance;
        public static SmartWeaponBus Instance => instance ??= new();

        public override void OnActivate()
        {
            Instance.EquipWeapon();
        }
    }

    /// <summary>
    /// Patch to increase smart weapon stat improvement amounts by 50%
    /// This modifies the ChangeValueOfStat instead of maxKills
    /// Applies to BARK, TRAC, and Hemogrip abilities
    /// </summary>
    [HarmonyPatch(typeof(OnKillEnemyChangeOwnerStatsAbility), "ApplyStatChange")]
    public static class SmartWeaponBus_IncreaseStatChange_Patch
    {
        public static void Prefix(OnKillEnemyChangeOwnerStatsAbility __instance)
        {
            if (!SmartWeaponBus.Instance.IsActive)
                return;

            // Access the private _statChanges field using Harmony
            var statChangesField = AccessTools.Field(typeof(OnKillEnemyChangeOwnerStatsAbility), "_statChanges");
            if (statChangesField != null)
            {
                var statChanges = (List<StatChange>)statChangesField.GetValue(__instance);
                if (statChanges != null && statChanges.Count > 0)
                {
                    // Create a modified list with increased stat change values
                    var modifiedStatChanges = new List<StatChange>();
                    foreach (var statChange in statChanges)
                    {
                        var modifiedStatChange = statChange;

                        // Increase the change value by 50% using reflection
                        if (statChange.StatToChange == Enumerations.UnitStats.Accuracy)
                        {
                            var accuracyField = AccessTools.Field(typeof(StatChange), "AccuracyChangeValue");
                            float originalValue = (float)accuracyField.GetValue(statChange);
                            float newValue = originalValue * SmartWeaponBus.StatImprovementMultiplier;
                            accuracyField.SetValue(modifiedStatChange, newValue);
                        }
                        else
                        {
                            var changeValueField = AccessTools.Field(typeof(StatChange), "ChangeValue");
                            float originalValue = (float)changeValueField.GetValue(statChange);
                            float newValue = originalValue * SmartWeaponBus.StatImprovementMultiplier;
                            changeValueField.SetValue(modifiedStatChange, newValue);
                        }

                        modifiedStatChanges.Add(modifiedStatChange);
                    }

                    statChangesField.SetValue(__instance, modifiedStatChanges);
                    Debug.Log($"SmartWeaponBus: Increased smart weapon stat changes by 50%");
                }
            }
        }
    }
}
