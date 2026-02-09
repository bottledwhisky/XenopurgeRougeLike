using HarmonyLib;
using SpaceCommander;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using static SpaceCommander.Enumerations;

namespace XenopurgeRougeLike.XenoReinforcements
{
    /// <summary>
    /// Centralized stun controller that manages all stun effects from different sources.
    /// This prevents multiple patches from interfering with each other.
    /// </summary>
    public static class CentralizedStunController
    {
        /// <summary>
        /// Dictionary tracking stunned units and their remaining stun duration from each source.
        /// Key: BattleUnit, Value: Dictionary of (source name, remaining stun time)
        /// </summary>
        private static Dictionary<BattleUnit, Dictionary<string, float>> _stunnedUnits = new();

        /// <summary>
        /// Register a stun effect on a unit from a specific source
        /// </summary>
        /// <param name="unit">The unit to stun</param>
        /// <param name="duration">Duration of the stun in seconds</param>
        /// <param name="source">The source of the stun (e.g., "PsionicScream", "XenoAffinity6", "EnhancedHacking")</param>
        public static void StunUnit(BattleUnit unit, float duration, string source)
        {
            if (unit == null || !unit.IsAlive || unit.Team != Team.EnemyAI)
                return;

            // Initialize the unit's stun dictionary if it doesn't exist
            if (!_stunnedUnits.ContainsKey(unit))
            {
                _stunnedUnits[unit] = new Dictionary<string, float>();
            }

            // Add or update the stun duration for this source
            // If already stunned by this source, use the longer duration
            if (_stunnedUnits[unit].TryGetValue(source, out var existingDuration))
            {
                if (duration > existingDuration)
                {
                    _stunnedUnits[unit][source] = duration;
                }
            }
            else
            {
                _stunnedUnits[unit][source] = duration;
            }

            // Mark in the unified tracker for DevourWill compatibility
            XenoStunTracker.MarkAsStunned(unit);
        }

        /// <summary>
        /// Check if a unit is currently stunned by any source
        /// </summary>
        public static bool IsStunned(BattleUnit unit)
        {
            return _stunnedUnits.ContainsKey(unit) && _stunnedUnits[unit].Count > 0;
        }

        /// <summary>
        /// Update stun timers for a unit and consume deltaTime from all active stuns
        /// </summary>
        /// <param name="unit">The unit to update</param>
        /// <param name="deltaTime">Time to subtract from stun durations</param>
        public static void UpdateStunTimers(BattleUnit unit, float deltaTime)
        {
            if (!_stunnedUnits.TryGetValue(unit, out var stunSources))
                return;

            // Update all stun sources and collect expired ones
            var expiredSources = new List<string>();

            foreach (var source in stunSources.Keys.ToList())
            {
                var remainingTime = stunSources[source] - deltaTime;

                if (remainingTime <= 0f)
                {
                    expiredSources.Add(source);
                }
                else
                {
                    stunSources[source] = remainingTime;
                }
            }

            // Remove expired stun sources
            foreach (var source in expiredSources)
            {
                stunSources.Remove(source);
            }

            // If no stuns remaining, clean up the unit entry
            if (stunSources.Count == 0)
            {
                _stunnedUnits.Remove(unit);
                XenoStunTracker.MarkAsNotStunned(unit);
            }
        }

        /// <summary>
        /// Get the remaining stun duration for a unit (returns the longest remaining stun)
        /// </summary>
        public static float GetRemainingStunDuration(BattleUnit unit)
        {
            if (!_stunnedUnits.TryGetValue(unit, out var stunSources))
                return 0f;

            return stunSources.Count > 0 ? stunSources.Values.Max() : 0f;
        }

        /// <summary>
        /// Remove all stuns from a specific source
        /// </summary>
        public static void ClearStunsBySource(string source)
        {
            var unitsToCheck = _stunnedUnits.Keys.ToList();

            foreach (var unit in unitsToCheck)
            {
                if (_stunnedUnits[unit].ContainsKey(source))
                {
                    _stunnedUnits[unit].Remove(source);

                    // Clean up if no stuns remaining
                    if (_stunnedUnits[unit].Count == 0)
                    {
                        _stunnedUnits.Remove(unit);
                        XenoStunTracker.MarkAsNotStunned(unit);
                    }
                }
            }
        }

        /// <summary>
        /// Clear all stuns from all sources
        /// </summary>
        public static void ClearAllStuns()
        {
            _stunnedUnits.Clear();
        }
    }

    /// <summary>
    /// Single Harmony patch that handles all stun effects from any source.
    /// This replaces individual patches from PsionicScream, XenoAffinity6, and EnhancedHacking.
    /// </summary>
    [HarmonyPatch]
    public static class CentralizedStun_UpdateTime_Patch
    {
        // Dynamically find all types that implement both ICommand and ITimeUpdatedListener
        public static IEnumerable<System.Reflection.MethodBase> TargetMethods()
        {
            var commandInterfaceType = typeof(SpaceCommander.Commands.ICommand);
            var timeListenerType = typeof(TimeSystem.ITimeUpdatedListener);

            var implementers = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a =>
                {
                    try { return a.GetTypes(); }
                    catch { return Type.EmptyTypes; }
                })
                .Where(t => t.IsClass && !t.IsAbstract &&
                           commandInterfaceType.IsAssignableFrom(t) &&
                           timeListenerType.IsAssignableFrom(t));

            foreach (var implementer in implementers)
            {
                var method = implementer.GetMethod("UpdateTime",
                    System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic,
                    null,
                    new Type[] { typeof(float) },
                    null);

                if (method != null && method.DeclaringType == implementer)
                {
                    yield return method;
                }
            }
        }

        private static readonly ConditionalWeakTable<Type, System.Reflection.FieldInfo> _fieldCache = new();

        [HarmonyPrefix]
        public static bool Prefix(object __instance, ref float __0)
        {
            var type = __instance.GetType();

            if (!_fieldCache.TryGetValue(type, out var fieldInfo))
            {
                fieldInfo = type.GetField("_battleUnit", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                _fieldCache.Add(type, fieldInfo);
            }

            if (fieldInfo != null)
            {
                var battleUnit = fieldInfo.GetValue(__instance) as BattleUnit;
                if (battleUnit != null && CentralizedStunController.IsStunned(battleUnit))
                {
                    // Update stun timers to consume deltaTime
                    CentralizedStunController.UpdateStunTimers(battleUnit, __0);
                    // Return false to skip the original method and prevent the stunned unit from acting
                    return false;
                }
            }

            return true; // Execute the original method if not stunned
        }
    }

    /// <summary>
    /// Clear all stuns when the game ends
    /// </summary>
    [HarmonyPatch(typeof(TestGame), "EndGame")]
    public static class CentralizedStun_ClearOnEndGame_Patch
    {
        public static void Postfix()
        {
            CentralizedStunController.ClearAllStuns();
            XenoStunTracker.ClearAll();
        }
    }
}
