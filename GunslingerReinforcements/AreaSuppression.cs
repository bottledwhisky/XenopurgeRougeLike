using HarmonyLib;
using MelonLoader;
using SpaceCommander;
using SpaceCommander.Commands;
using System;
using System.Collections.Generic;
using static XenopurgeRougeLike.ModLocalization;

namespace XenopurgeRougeLike.GunslingerReinforcements
{
    /// <summary>
    /// 范围压制：敌人被压制时，同格内所有其它敌人也会受到同样的压制效果
    /// Area Suppression: When an enemy is suppressed, all other enemies in the same tile also receive the same suppression effect
    /// </summary>
    public class AreaSuppression : Reinforcement
    {
        public AreaSuppression()
        {
            company = Company.Gunslinger;
            rarity = Rarity.Elite;
            stackable = false;
            name = L("gunslinger.area_suppression.name");
            flavourText = L("gunslinger.area_suppression.flavour");
            description = L("gunslinger.area_suppression.description");
        }

        protected static AreaSuppression _instance;
        public static AreaSuppression Instance => _instance ??= new();
    }

    /// <summary>
    /// Tracks area suppression effects for cleanup
    /// Each SuppressiveFire instance has its own set of area debuffs
    /// </summary>
    public static class AreaSuppressionTracker
    {
        // Maps a SuppressiveFire instance to a dictionary of BattleUnit -> GUID
        private static Dictionary<SuppressiveFire, Dictionary<BattleUnit, string>> _instanceDebuffs =
            new Dictionary<SuppressiveFire, Dictionary<BattleUnit, string>>();

        public static void ApplyAreaDebuffs(SuppressiveFire instance, BattleUnit primaryTarget, float speedDebuff)
        {
            // Clear previous area debuffs for this instance
            RemoveDebuffsForInstance(instance);

            if (primaryTarget == null)
                return;

            try
            {
                // Get the tile the target is on
                var targetTile = primaryTarget.MovementManager.CurrentTile;
                if (targetTile == null)
                    return;

                // Get all units on that tile
                var unitsOnTile = targetTile.CurrentStateOfTile.UnitsOnTile;
                if (unitsOnTile == null)
                    return;

                // Create tracking dictionary for this instance if needed
                if (!_instanceDebuffs.ContainsKey(instance))
                {
                    _instanceDebuffs[instance] = new Dictionary<BattleUnit, string>();
                }

                // Apply the same debuff to all other enemy units on the tile
                foreach (var unit in unitsOnTile)
                {
                    // Skip the primary target (it already gets the debuff from the original code)
                    if (unit == primaryTarget)
                        continue;

                    // Only apply to enemy units (same team as primary target)
                    if (unit.Team == primaryTarget.Team)
                    {
                        string areaDebuffGuid = Guid.NewGuid().ToString();
                        unit.ChangeStat(Enumerations.UnitStats.Speed, speedDebuff, areaDebuffGuid);
                        _instanceDebuffs[instance][unit] = areaDebuffGuid;
                    }
                }
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"[AreaSuppression] Error applying area suppression: {ex}");
                MelonLogger.Error(ex.StackTrace);
            }
        }

        public static void RemoveDebuffsForInstance(SuppressiveFire instance)
        {
            if (!_instanceDebuffs.ContainsKey(instance))
                return;

            foreach (var kvp in _instanceDebuffs[instance])
            {
                var unit = kvp.Key;
                var guid = kvp.Value;
                unit.ReverseChangeOfStat(guid);
            }

            _instanceDebuffs[instance].Clear();
        }

        public static void ClearAll()
        {
            foreach (var instanceKvp in _instanceDebuffs)
            {
                foreach (var unitKvp in instanceKvp.Value)
                {
                    unitKvp.Key.ReverseChangeOfStat(unitKvp.Value);
                }
            }
            _instanceDebuffs.Clear();
        }
    }

    /// <summary>
    /// Patch SuppressiveFire to apply area suppression effect
    /// We need to patch the TargetOfDebuff property setter
    /// </summary>
    [HarmonyPatch(typeof(SuppressiveFire), "TargetOfDebuff", MethodType.Setter)]
    public static class AreaSuppression_TargetOfDebuff_Patch
    {
        public static void Postfix(SuppressiveFire __instance, BattleUnit value)
        {
            if (!AreaSuppression.Instance.IsActive)
                return;

            // Get the current speed debuff from the SuppressiveFire instance
            var speedDebuffField = AccessTools.Field(typeof(SuppressiveFire), "_speedDebuff");
            if (speedDebuffField == null)
                return;

            float speedDebuff = (float)speedDebuffField.GetValue(__instance);

            // Apply area debuffs to all units on the same tile as the target
            // This will also clear previous debuffs for this instance
            AreaSuppressionTracker.ApplyAreaDebuffs(__instance, value, speedDebuff);
        }
    }

    /// <summary>
    /// Clear tracking data when mission ends
    /// </summary>
    [HarmonyPatch(typeof(TestGame), "EndGame")]
    public static class AreaSuppression_ClearTracking_Patch
    {
        public static void Postfix()
        {
            AreaSuppressionTracker.ClearAll();
        }
    }
}
