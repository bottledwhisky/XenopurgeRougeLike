using HarmonyLib;
using MelonLoader;
using SpaceCommander;
using SpaceCommander.Abilities;
using SpaceCommander.Area;
using SpaceCommander.BattleManagement.UI;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace XenopurgeRougeLike
{
    public class UnitStatChange(Enumerations.UnitStats stat, float amount, Func<BattleUnit, Enumerations.Team, bool> condition = null)
    {
        public Enumerations.UnitStats Stat = stat;
        public float Amount = amount;
        public Func<BattleUnit, Enumerations.Team, bool> Condition = condition;
        public Enumerations.Team TargetTeam = Enumerations.Team.Player;

        public UnitStatChange(Enumerations.UnitStats stat, float amount, Enumerations.Team targetTeam, Func<BattleUnit, Enumerations.Team, bool> condition = null)
            : this(stat, amount, condition)
        {
            TargetTeam = targetTeam;
        }
    }

    public static class UnitStatsTools
    {
        public static FieldInfo ArmorField = AccessTools.Field(typeof(BattleUnit), "_currentArmor");

        public static Dictionary<string, UnitStatChange> InBattleUnitStatChanges = [];
        /// <summary>
        /// Add armor to a BattleUnit by directly modifying the private _currentArmor field
        /// </summary>
        public static void AddArmorToUnit(BattleUnit unit, float armorAmount)
        {
            float currentArmor = (float)ArmorField.GetValue(unit);
            ArmorField.SetValue(unit, currentArmor + armorAmount);
            RefreshUnitUI(unit);
        }

        /// <summary>
        /// Heal a BattleUnit using the built-in Heal method
        /// </summary>
        public static void HealUnit(BattleUnit unit, float healAmount)
        {
            if (unit == null || !unit.IsAlive)
                return;
            unit.Heal(healAmount);
            RefreshUnitUI(unit);
        }

        /// <summary>
        /// Heal all player units
        /// </summary>
        public static void HealAllPlayerUnits(float healAmount)
        {
            var gameManager = GameManager.Instance;
            if (gameManager == null)
                return;

            var playerManager = gameManager.GetTeamManager(Enumerations.Team.Player);
            if (playerManager == null)
                return;

            foreach (var unit in playerManager.BattleUnits)
            {
                if (unit.IsAlive)
                {
                    HealUnit(unit, healAmount);
                }
            }
        }

        /// <summary>
        /// Refresh the UI for a unit after stat changes
        /// </summary>
        private static void RefreshUnitUI(BattleUnit unit)
        {
            var unitsListView_BattleManagement = global::UnityEngine.Object.FindAnyObjectByType<UnitsListView_BattleManagement>();
            var UnitDataChanged = AccessTools.Method(typeof(UnitsListView_BattleManagement), "UnitDataChanged");
            UnitDataChanged.Invoke(unitsListView_BattleManagement, [unit]);
            unit.CommandsManager.UpdateValuesOfAllCommands();
            var StatsValuesGotUpdated = AccessTools.Method(typeof(UnitAbilityManager), "StatsValuesGotUpdated");
            StatsValuesGotUpdated.Invoke(unit.UnitAbilityManager, null);
        }
    }


    [HarmonyPatch(typeof(BattleUnit), MethodType.Constructor)]
    [HarmonyPatch([typeof(UnitData), typeof(Enumerations.Team), typeof(GridManager)])]
    public static class UnitStatsChangeConstructor
    {
        public static void Postfix(BattleUnit __instance, Enumerations.Team team)
        {
            foreach (var statChangePair in UnitStatsTools.InBattleUnitStatChanges)
            {
                var id = statChangePair.Key;
                var statChange = statChangePair.Value;
                if (statChange.TargetTeam != team)
                    continue;
                if (statChange.Condition != null && !statChange.Condition(__instance, team))
                    continue;
                MelonLogger.Msg($"UnitStatsChangeConstructor: Applying stat change {statChange.Stat} with amount {statChange.Amount} and id {id} to unit {__instance.UnitName} (team: {team})");
                switch (statChange.Stat)
                {
                    case Enumerations.UnitStats.Speed:
                        __instance.ChangeStat(Enumerations.UnitStats.Speed, statChange.Amount, id + "_UnitStatChangeSpeed");
                        break;
                    case Enumerations.UnitStats.Accuracy:
                        __instance.ChangeStat(Enumerations.UnitStats.Accuracy, statChange.Amount, id + "_UnitStatChangeAccuracy");
                        break;
                    case Enumerations.UnitStats.Power:
                        __instance.ChangeStat(Enumerations.UnitStats.Power, statChange.Amount, id + "_UnitStatChangePower");
                        break;
                    case Enumerations.UnitStats.Health:
                        __instance.ChangeStat(Enumerations.UnitStats.Health, statChange.Amount, id + "_UnitStatChangeHealth");
                        break;
                    default:
                        MelonLogger.Warning($"UnitStatsChangeConstructor: Unsupported stat {statChange.Stat} for unit stat change.");
                        break;
                }
            }
        }
    }
}
