using HarmonyLib;
using SpaceCommander;
using SpaceCommander.Achievements;
using SpaceCommander.ActionCards;
using SpaceCommander.Area;
using SpaceCommander.Weapons;
using System.Collections.Generic;
using System.Linq;
using Traptics.EventsSystem;
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
        public static readonly HashSet<string> ExtendedRangeCards =
        [
            ActionCardIds.FRAG_GRENADE,
            ActionCardIds.FLASH_GRENADE,
            ActionCardIds.SETUP_MINE,
        ];

        /// <summary>
        /// Gets all units within range (center tile + adjacent tiles if Big Bang is active)
        /// </summary>
        public static List<BattleUnit> GetUnitsInExtendedArea(Tile centerTile)
        {
            List<BattleUnit> units =
            [
                // Always include units on the center tile
                .. centerTile.CurrentStateOfTile.UnitsOnTile,
            ];

            // If Big Bang is active, also include units on adjacent tiles
            if (BigBang.Instance.IsActive)
            {
                // Get GridManager from the BattleManager singleton
                var gridManager = GameManager.Instance.GridManager;
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

            List<BattleUnit> killedUnits = [];

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
                    var achievementsManager = AchievementsManager.Instance;

                    if (achievementsManager != null)
                    {
                        achievementsManager.KilledUnitsWithOneGrenade(killedUnits.Count, __instance.Info.Id);

                        if (killedUnits.Any(x => x.Team == Enumerations.Team.Player))
                        {
                            achievementsManager.FriendlyFire();
                        }
                    }
                }
                catch
                {
                    // Ignore achievement errors
                }
            }

            // Trigger explosion visual effect
            var explosionEvent = new TriggerExplosionEvent(centerTile);
            EventBus.Publish(explosionEvent);

            return false; // Skip original method
        }
    }

    /// <summary>
    /// Patch to extend flashbang duration range
    /// </summary>
    [HarmonyPatch(typeof(ChangeStatArea_Card), "ApplyCommand")]
    public static class BigBang_FlashbangArea_Patch
    {
        // Field accessors for ChangeStatArea_Card private fields
        private static readonly AccessTools.FieldRef<ChangeStatArea_Card, List<StatChange>> _statChangesRef =
            AccessTools.FieldRefAccess<ChangeStatArea_Card, List<StatChange>>("_statChanges");

        public static void Prefix(ChangeStatArea_Card __instance, BattleUnit unit, ref List<BattleUnit> __state)
        {
            // Only override if Big Bang is active and this is a relevant card
            if (!BigBangHelpers.ShouldExtendRange(__instance))
            {
                __state = null;
                return;
            }

            // Get all units that should be affected (center + neighbors)
            Tile centerTile = unit.MovementManager.CurrentTile;
            List<BattleUnit> affectedUnits = BigBangHelpers.GetUnitsInExtendedArea(centerTile);

            // Get the stat changes list
            List<StatChange> statChanges = _statChangesRef(__instance);

            // The original method will handle the center tile
            // We need to apply effects to ONLY the adjacent tiles (not center)
            var neighborsOnly = affectedUnits.Except(centerTile.CurrentStateOfTile.UnitsOnTile).ToList();

            // Store neighbor units for postfix to track them for removal
            __state = neighborsOnly;

            // Apply stat changes to neighbor units immediately
            foreach (BattleUnit neighborUnit in neighborsOnly)
            {
                foreach (StatChange statChange in statChanges)
                {
                    // Skip if unit ignores AOE damage and this is a debuff
                    if (!neighborUnit.IsIgnoringAOEDamage || !(statChange.ChangeValueOfStat < 0f))
                    {
                        // Generate a GUID for this stat change so it can be reversed later
                        string guid = System.Guid.NewGuid().ToString();

                        // We'll track this in our own system since the card only tracks center tile units
                        BigBang_StatChangeTracker.TrackStatChange(__instance, neighborUnit, guid);

                        neighborUnit.ChangeStat(statChange.StatToChange, statChange.ChangeValueOfStat, guid);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Tracks stat changes applied to neighbor units for later reversal
    /// </summary>
    public static class BigBang_StatChangeTracker
    {
        private static readonly Dictionary<ChangeStatArea_Card, List<(BattleUnit unit, string guid)>> _trackedChanges = [];

        public static void TrackStatChange(ChangeStatArea_Card card, BattleUnit unit, string guid)
        {
            if (!_trackedChanges.ContainsKey(card))
            {
                _trackedChanges[card] = [];
            }
            _trackedChanges[card].Add((unit, guid));
        }

        public static void RemoveTrackedChanges(ChangeStatArea_Card card)
        {
            if (_trackedChanges.TryGetValue(card, out var changes))
            {
                foreach (var (unit, guid) in changes)
                {
                    if (unit != null && unit.IsAlive)
                    {
                        unit.ReverseChangeOfStat(guid);
                    }
                }
                _trackedChanges.Remove(card);
            }
        }
    }

    /// <summary>
    /// Patch OnTimeUpdate to also remove our tracked stat changes when duration expires
    /// </summary>
    [HarmonyPatch(typeof(ChangeStatArea_Card), "OnTimeUpdate")]
    public static class BigBang_FlashbangCleanup_Patch
    {
        private static readonly AccessTools.FieldRef<ChangeStatArea_Card, float> _remainingTimeRef =
            AccessTools.FieldRefAccess<ChangeStatArea_Card, float>("_remainingTime");

        public static void Postfix(ChangeStatArea_Card __instance, float time)
        {
            if (!BigBang.Instance.IsActive)
                return;

            // Check if the duration has expired
            float remainingTime = _remainingTimeRef(__instance);
            if (remainingTime <= 0f)
            {
                // Remove our tracked stat changes for neighbor units
                BigBang_StatChangeTracker.RemoveTrackedChanges(__instance);
            }
        }
    }
}
