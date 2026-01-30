using HarmonyLib;
using SpaceCommander;
using SpaceCommander.ActionCards;
using SpaceCommander.Area;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using static XenopurgeRougeLike.ModLocalization;

namespace XenopurgeRougeLike.EngineerReinforcements
{
    /// <summary>
    /// Big Bang: Increases the range of mines, grenades, and flashbangs by 1 tile
    /// 大爆炸：地雷，手雷，闪光弹范围加1
    /// </summary>
    public class BigBang : Reinforcement
    {
        public static readonly int AreaExpansion = 1;

        public BigBang()
        {
            name = L("engineer.big_bang.name");
            description = L("engineer.big_bang.description", AreaExpansion);
            flavourText = L("engineer.big_bang.flavour");
            rarity = Rarity.Expert;
            company = Company.Engineer;
            stackable = false;
        }

        private static BigBang _instance;
        public static BigBang Instance => _instance ??= new();
    }

    /// <summary>
    /// Helper class for expanding area effects to neighboring tiles
    /// </summary>
    public static class BigBangHelpers
    {
        // Action card IDs that should have extended range
        public static readonly HashSet<string> ExtendedRangeCards = new()
        {
            "bfb700d8-5fa2-4bd0-b1dd-94842f66c031", // Frag Grenade
            "3b1ee954-9aec-45fe-afa0-46fbc9fc99a0", // Flash Grenade
            "8daa3d58-73aa-4c26-a20f-954686777d1f", // Setup Mine
        };

        /// <summary>
        /// Gets all units within range (center tile + adjacent tiles if Big Bang is active)
        /// </summary>
        public static List<BattleUnit> GetUnitsInExtendedArea(Tile centerTile)
        {
            List<BattleUnit> units = new List<BattleUnit>();

            // Always include units on the center tile
            units.AddRange(centerTile.CurrentStateOfTile.UnitsOnTile);

            // If Big Bang is active, also include units on adjacent tiles
            if (BigBang.Instance.IsActive)
            {
                // Get GridManager from the BattleManager singleton
                var gridManager = GetGridManager();
                if (gridManager != null)
                {
                    // Get all adjacent tiles (passable neighbors)
                    var neighbors = gridManager.GetSeeThroughNeighboursOfTile(centerTile);
                    foreach (var neighborTile in neighbors)
                    {
                        units.AddRange(neighborTile.CurrentStateOfTile.UnitsOnTile);
                    }
                }
            }

            return units;
        }

        /// <summary>
        /// Gets the GridManager from the BattleManager
        /// </summary>
        private static GridManager GetGridManager()
        {
            try
            {
                // Try to get BattleManager instance
                var battleManagerType = AccessTools.TypeByName("SpaceCommander.BattleManagement.BattleManager");
                if (battleManagerType == null)
                    return null;

                var instanceProperty = AccessTools.Property(battleManagerType, "Instance");
                if (instanceProperty == null)
                    return null;

                var battleManager = instanceProperty.GetValue(null);
                if (battleManager == null)
                    return null;

                // Get GridManager from BattleManager
                var gridManagerField = AccessTools.Field(battleManagerType, "_gridManager");
                if (gridManagerField == null)
                    return null;

                return gridManagerField.GetValue(battleManager) as GridManager;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Check if a card should have extended range
        /// </summary>
        public static bool ShouldExtendRange(ActionCard card)
        {
            return BigBang.Instance.IsActive &&
                   card?.Info?.Id != null &&
                   ExtendedRangeCards.Contains(card.Info.Id);
        }
    }

    /// <summary>
    /// Patch to extend grenade/mine damage range
    /// </summary>
    [HarmonyPatch(typeof(ChangeCurrentHealthArea_Card), "ApplyCommand")]
    public static class BigBang_GrenadeDamage_Patch
    {
        public static bool Prefix(ChangeCurrentHealthArea_Card __instance, BattleUnit unit)
        {
            // Only override if Big Bang is active and this is a relevant card
            if (!BigBangHelpers.ShouldExtendRange(__instance))
                return true; // Run original method

            // Custom implementation with extended range
            Tile centerTile = unit.MovementManager.CurrentTile;
            List<BattleUnit> affectedUnits = BigBangHelpers.GetUnitsInExtendedArea(centerTile);

            // Get the change value using reflection
            var changeValueField = AccessTools.Field(typeof(ChangeCurrentHealthArea_Card), "_changeValue");
            float changeValue = (float)changeValueField.GetValue(__instance);

            List<BattleUnit> killedUnits = new List<BattleUnit>();

            // Apply damage/healing to all affected units
            foreach (BattleUnit affectedUnit in affectedUnits)
            {
                if (changeValue < 0f && affectedUnit.CurrentHealth > 0f)
                {
                    if (!affectedUnit.IsIgnoringAOEDamage)
                    {
                        affectedUnit.Damage(System.Math.Abs(changeValue));
                        if (affectedUnit.CurrentHealth <= 0f)
                        {
                            killedUnits.Add(affectedUnit);
                        }
                    }
                }
                else
                {
                    affectedUnit.Heal(changeValue);
                }
            }

            // Achievement tracking (if damage was dealt)
            if (changeValue < 0f && affectedUnits.Count > 0)
            {
                try
                {
                    var achievementsManagerType = AccessTools.TypeByName("SpaceCommander.Achievements.AchievementsManager");
                    if (achievementsManagerType != null)
                    {
                        var instanceProperty = AccessTools.Property(achievementsManagerType, "Instance");
                        var achievementsManager = instanceProperty?.GetValue(null);

                        if (achievementsManager != null)
                        {
                            var killedMethod = AccessTools.Method(achievementsManagerType, "KilledUnitsWithOneGrenade");
                            killedMethod?.Invoke(achievementsManager, new object[] { killedUnits.Count, __instance.Info.Id });

                            if (killedUnits.Any(x => x.Team == Enumerations.Team.Player))
                            {
                                var friendlyFireMethod = AccessTools.Method(achievementsManagerType, "FriendlyFire");
                                friendlyFireMethod?.Invoke(achievementsManager, null);
                            }
                        }
                    }
                }
                catch
                {
                    // Ignore achievement errors
                }
            }

            // Trigger explosion visual effect
            try
            {
                var eventBusType = AccessTools.TypeByName("Traptics.EventsSystem.EventBus");
                var triggerExplosionEventType = AccessTools.TypeByName("Traptics.EventsSystem.TriggerExplosionEvent");

                if (eventBusType != null && triggerExplosionEventType != null)
                {
                    var publishMethod = AccessTools.Method(eventBusType, "Publish");
                    var explosionEvent = System.Activator.CreateInstance(triggerExplosionEventType, centerTile);
                    publishMethod?.Invoke(null, new object[] { explosionEvent });
                }
            }
            catch
            {
                // Ignore visual effect errors
            }

            return false; // Skip original method
        }
    }

    /// <summary>
    /// Patch to extend flashbang duration range
    /// </summary>
    [HarmonyPatch(typeof(ChangeStatArea_Card), "ApplyCommand")]
    public static class BigBang_FlashbangArea_Patch
    {
        public static bool Prefix(ChangeStatArea_Card __instance, BattleUnit unit)
        {
            // Only override if Big Bang is active and this is a relevant card
            if (!BigBangHelpers.ShouldExtendRange(__instance))
                return true; // Run original method

            // Custom implementation with extended range
            Tile centerTile = unit.MovementManager.CurrentTile;
            List<BattleUnit> affectedUnits = BigBangHelpers.GetUnitsInExtendedArea(centerTile);

            // Get the stat change parameters using reflection
            var statToChangeField = AccessTools.Field(typeof(ChangeStatArea_Card), "_statToChange");
            var changeValueField = AccessTools.Field(typeof(ChangeStatArea_Card), "_changeValue");
            var durationField = AccessTools.Field(typeof(ChangeStatArea_Card), "_duration");
            var addStatChangeMethod = AccessTools.Method(typeof(ChangeStatArea_Card), "AddStatChange");

            var statToChange = (Enumerations.UnitStats)statToChangeField.GetValue(__instance);
            float changeValue = (float)changeValueField.GetValue(__instance);
            float duration = (float)durationField.GetValue(__instance);

            // Apply stat change to all affected units
            foreach (BattleUnit affectedUnit in affectedUnits)
            {
                // Call the private AddStatChange method
                addStatChangeMethod?.Invoke(__instance, new object[] { affectedUnit, statToChange, changeValue, duration });
            }

            return false; // Skip original method
        }
    }
}
