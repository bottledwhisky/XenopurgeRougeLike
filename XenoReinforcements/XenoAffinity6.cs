using HarmonyLib;
using SpaceCommander;
using SpaceCommander.Area;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static SpaceCommander.Enumerations;
using static XenopurgeRougeLike.ModLocalization;

namespace XenopurgeRougeLike.XenoReinforcements
{
    public class XenoAffinity6 : CompanyAffinity
    {
        public XenoAffinity6()
        {
            unlockLevel = 6;
            company = Company.Xeno;
            description = L("xeno.affinity6.description");
        }

        // Damage bonus constants
        public const float XenoDamageBonus = 0f; // +60% damage to xenos

        public const int ControlDurationBonusLevel = 3;

        // Reinforcement chance multiplier
        public const float ReinforcementChanceBonus = 2f;

        // Stun duration when xeno dies
        public const float StunDuration = 5f; // 5 seconds stun

        public static XenoAffinity6 _instance;

        public static XenoAffinity6 Instance => _instance ??= new();

        public override void OnActivate()
        {
            AwardSystem.WeightModifiers.Add(ModifyWeights);
        }

        public override void OnDeactivate()
        {
            AwardSystem.WeightModifiers.Remove(ModifyWeights);
            XenoAffinity6_StunSystem.ClearAllStuns();
        }

        private void ModifyWeights(List<Tuple<int, Reinforcement>> choices)
        {
            for (int i = 0; i < choices.Count; i++)
            {
                // Check if this reinforcement belongs to Xeno company
                if (choices[i].Item2.company.Type == CompanyType.Xeno)
                {
                    int newWeight = (int)(choices[i].Item1 * ReinforcementChanceBonus);
                    choices[i] = new Tuple<int, Reinforcement>(newWeight, choices[i].Item2);
                }
            }
        }
    }

    // Patch BattleUnit.Damage to increase damage dealt to xenos
    [HarmonyPatch(typeof(BattleUnit), "Damage")]
    public static class XenoAffinity6_Damage_Patch
    {
        public static void Prefix(BattleUnit __instance, ref float damage)
        {
            if (!XenoAffinity6.Instance.IsActive)
                return;

            // Check if the damaged unit is an enemy (xeno)
            if (__instance.Team != Team.EnemyAI)
                return;

            // Increase damage dealt to xenos
            damage *= (1f + XenoAffinity6.XenoDamageBonus);
        }
    }

    /// <summary>
    /// Stun system for XenoAffinity6
    /// Maintains a dictionary of stunned enemies
    /// </summary>
    public static class XenoAffinity6_StunSystem
    {
        private static Dictionary<BattleUnit, float> _stunnedEnemies = new Dictionary<BattleUnit, float>();

        public static void StunNearbyXenos(BattleUnit deadXeno)
        {
            var gameManager = GameManager.Instance;
            if (gameManager == null)
                return;

            var gridManager = gameManager.GridManager;
            if (gridManager == null)
                return;

            // Get the tile where the xeno died
            var deadXenoTile = deadXeno.CurrentTile;
            if (deadXenoTile == null)
                return;

            // Get all adjacent tiles (including the current tile)
            var tilesToCheck = new List<Tile> { deadXenoTile };

            // Add adjacent tiles in all four directions
            foreach (var direction in DirectionUtilities.Directions)
            {
                var offset = direction.GetDirectionVector();
                var adjacentCoords = deadXenoTile.Coords + offset;

                if (gridManager.Tiles.TryGetValue(adjacentCoords, out var adjacentTile))
                {
                    tilesToCheck.Add(adjacentTile);
                }
            }

            // Find all enemy units on these tiles and stun them
            foreach (var tile in tilesToCheck)
            {
                var enemyUnits = tile.CurrentStateOfTile.GetUnitsOnTile(Team.EnemyAI);
                foreach (var unit in enemyUnits)
                {
                    // Don't stun the dead xeno itself
                    if (unit != deadXeno && unit.IsAlive)
                    {
                        StunUnit(unit, XenoAffinity6.StunDuration);
                    }
                }
            }
        }

        public static void StunUnit(BattleUnit unit, float duration)
        {
            if (unit == null || !unit.IsAlive || unit.Team != Team.EnemyAI)
                return;

            // If already stunned, extend the duration if the new one is longer
            if (_stunnedEnemies.TryGetValue(unit, out var existingDuration))
            {
                if (duration > existingDuration)
                {
                    _stunnedEnemies[unit] = duration;
                }
            }
            else
            {
                _stunnedEnemies[unit] = duration;
            }

            // Register with the unified tracker
            XenoStunTracker.MarkAsStunned(unit);
        }

        public static bool IsStunned(BattleUnit unit)
        {
            return _stunnedEnemies.ContainsKey(unit) && unit.Team == Team.EnemyAI;
        }

        public static float UpdateStunTimers(BattleUnit unit, float deltaTime)
        {
            if (_stunnedEnemies.TryGetValue(unit, out var remainingTime))
            {
                remainingTime -= deltaTime;
                if (remainingTime <= 0f)
                {
                    _stunnedEnemies.Remove(unit);
                    XenoStunTracker.MarkAsNotStunned(unit);
                    return -remainingTime;
                }
                else
                {
                    _stunnedEnemies[unit] = remainingTime;
                    return 0;
                }
            }
            return 0f;
        }

        public static void ClearAllStuns()
        {
            _stunnedEnemies.Clear();
        }
    }

    // Patch BattleUnit constructor to add OnDeath listener
    [HarmonyPatch(typeof(BattleUnit), MethodType.Constructor)]
    [HarmonyPatch(new Type[] { typeof(UnitData), typeof(Team), typeof(GridManager) })]
    public static class XenoAffinity6_BattleUnit_Constructor_Patch
    {
        public static void Postfix(BattleUnit __instance, Team team)
        {
            if (!XenoAffinity6.Instance.IsActive)
                return;

            if (team == Team.EnemyAI)
            {
                Action action = null;
                action = () =>
                {
                    // When this xeno dies, stun nearby xenos
                    XenoAffinity6_StunSystem.StunNearbyXenos(__instance);
                    __instance.OnDeath -= action;
                };
                __instance.OnDeath += action;
            }
        }
    }

    // Patch ICommand UpdateTime to prevent stunned xenos from acting
    [HarmonyPatch]
    public static class XenoAffinity6_StunUpdateTime_Patch
    {
        // Dynamically find all types that implement both ICommand and ITimeUpdatedListener
        public static IEnumerable<System.Reflection.MethodBase> TargetMethods()
        {
            var commandInterfaceType = typeof(SpaceCommander.Commands.ICommand);
            var timeListenerType = typeof(TimeSystem.ITimeUpdatedListener);

            var implementers = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => {
                    try { return a.GetTypes(); }
                    catch { return Type.EmptyTypes; }
                })
                .Where(t => t.IsClass && !t.IsAbstract &&
                           commandInterfaceType.IsAssignableFrom(t) &&
                           timeListenerType.IsAssignableFrom(t));

            foreach (var implementer in implementers)
            {
                // Try to get the UpdateTime method
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

        private static readonly System.Runtime.CompilerServices.ConditionalWeakTable<Type, System.Reflection.FieldInfo> _fieldCache = new();

        [HarmonyPrefix]
        public static bool Prefix(object __instance, ref float __0)
        {
            if (!XenoAffinity6.Instance.IsActive)
                return true;

            // Try to get the BattleUnit from the command
            var type = __instance.GetType();

            if (!_fieldCache.TryGetValue(type, out var fieldInfo))
            {
                fieldInfo = type.GetField("_battleUnit", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                _fieldCache.Add(type, fieldInfo);
            }

            if (fieldInfo != null)
            {
                var battleUnit = fieldInfo.GetValue(__instance) as BattleUnit;
                if (battleUnit != null && XenoAffinity6_StunSystem.IsStunned(battleUnit))
                {
                    // Update the stun timer but don't execute the command
                    var remainingTime = XenoAffinity6_StunSystem.UpdateStunTimers(battleUnit, __0);
                    __0 = remainingTime;
                    return true; // Continue with original method but with modified time
                }
            }
            return true; // Execute the original method
        }
    }
}
