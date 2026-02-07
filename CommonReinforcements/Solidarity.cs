using HarmonyLib;
using SpaceCommander;
using SpaceCommander.Area;
using System.Collections.Generic;
using System.Linq;
using TimeSystem;
using static XenopurgeRougeLike.ModLocalization;

namespace XenopurgeRougeLike.CommonReinforcements
{
    /// <summary>
    /// 团结：当视野内有队友时，速度+1，瞄准+10，近战伤害+1
    /// Solidarity: When allies are in sight, +1 Speed, +10 Accuracy, +1 Melee Damage
    /// </summary>
    public class Solidarity : Reinforcement
    {
        public const float SpeedBonus = 1f;
        public const float AccuracyBonus = 0.1f; // +10 Accuracy (displayed as x100)
        public const int MeleeDamageBonus = 1;

        public Solidarity()
        {
            company = Company.Common;
            rarity = Rarity.Standard;
            stackable = false;
            name = L("common.solidarity.name");
            flavourText = L("common.solidarity.flavour");
            description = L("common.solidarity.description", (int)SpeedBonus, (int)(AccuracyBonus * 100), MeleeDamageBonus);
        }

        protected static Solidarity _instance;
        public static Solidarity Instance => _instance ??= new();
    }

    /// <summary>
    /// System to track and apply Solidarity buffs based on allies in sight
    /// </summary>
    public static class SolidaritySystem
    {
        // Track which units currently have Unity buffs active
        private static HashSet<BattleUnit> _unitsWithBuffs = [];
        private static bool _isRegistered = false;
        private const float CHECK_INTERVAL = 0.5f; // Check every 0.5 seconds
        private static float _timeSinceLastCheck = 0f;

        public static void ClearAll()
        {
            UnregisterTimeUpdate();

            // Remove buffs from all units
            foreach (var unit in _unitsWithBuffs.ToList())
            {
                RemoveBuffs(unit);
            }

            _unitsWithBuffs.Clear();
            _timeSinceLastCheck = 0f;
        }

        /// <summary>
        /// Register to TimeManager for time updates
        /// </summary>
        public static void RegisterTimeUpdate()
        {
            if (!_isRegistered)
            {
                TempSingleton<TimeManager>.Instance.OnTimeUpdated += OnTimeUpdate;
                _isRegistered = true;
            }
        }

        /// <summary>
        /// Unregister from TimeManager
        /// </summary>
        public static void UnregisterTimeUpdate()
        {
            if (_isRegistered)
            {
                TempSingleton<TimeManager>.Instance.OnTimeUpdated -= OnTimeUpdate;
                _isRegistered = false;
            }
        }

        /// <summary>
        /// Check if a unit has allies in line of sight
        /// </summary>
        private static bool HasAlliesInSight(BattleUnit unit)
        {
            if (unit == null || !unit.IsAlive || unit.LineOfSight == null)
                return false;

            var visibleTiles = unit.LineOfSight.Tiles;
            if (visibleTiles == null)
                return false;

            // Check if any visible tile has an ally
            foreach (var tile in visibleTiles)
            {
                if (tile?.CurrentStateOfTile == null)
                    continue;

                var unitsOnTile = tile.CurrentStateOfTile.GetUnitsOnTile(unit.Team);
                if (unitsOnTile != null)
                {
                    foreach (var allyUnit in unitsOnTile)
                    {
                        // Found an ally that's not this unit
                        if (allyUnit != unit && allyUnit.IsAlive)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Apply Unity buffs to a unit
        /// </summary>
        private static void ApplyBuffs(BattleUnit unit)
        {
            if (_unitsWithBuffs.Contains(unit))
                return;

            unit.ChangeStat(Enumerations.UnitStats.Speed, Solidarity.SpeedBonus, "Solidarity_Speed");
            unit.ChangeStat(Enumerations.UnitStats.Accuracy, Solidarity.AccuracyBonus, "Solidarity_Accuracy");
            unit.ChangeStat(Enumerations.UnitStats.Power, Solidarity.MeleeDamageBonus, "Solidarity_MeleeDamage");

            _unitsWithBuffs.Add(unit);
        }

        /// <summary>
        /// Remove Unity buffs from a unit
        /// </summary>
        private static void RemoveBuffs(BattleUnit unit)
        {
            if (!_unitsWithBuffs.Contains(unit))
                return;

            unit.ReverseChangeOfStat("Solidarity_Speed");
            unit.ReverseChangeOfStat("Solidarity_Accuracy");
            unit.ReverseChangeOfStat("Solidarity_MeleeDamage");

            _unitsWithBuffs.Remove(unit);
        }

        /// <summary>
        /// Update all player units based on their line of sight
        /// Called by TimeManager
        /// </summary>
        private static void OnTimeUpdate(float deltaTime)
        {
            if (!Solidarity.Instance.IsActive)
                return;

            _timeSinceLastCheck += deltaTime;

            // Only check periodically to reduce performance impact
            if (_timeSinceLastCheck < CHECK_INTERVAL)
                return;

            _timeSinceLastCheck = 0f;

            var gameManager = GameManager.Instance;
            if (gameManager == null)
                return;

            var playerManager = gameManager.GetTeamManager(Enumerations.Team.Player);
            if (playerManager == null)
                return;

            foreach (var unit in playerManager.BattleUnits)
            {
                if (!unit.IsAlive)
                {
                    // Remove buffs from dead units
                    if (_unitsWithBuffs.Contains(unit))
                    {
                        RemoveBuffs(unit);
                    }
                    continue;
                }

                bool hasAllies = HasAlliesInSight(unit);
                bool hasBuffs = _unitsWithBuffs.Contains(unit);

                if (hasAllies && !hasBuffs)
                {
                    // Unit can see allies but doesn't have buffs - apply them
                    ApplyBuffs(unit);
                }
                else if (!hasAllies && hasBuffs)
                {
                    // Unit can't see allies but has buffs - remove them
                    RemoveBuffs(unit);
                }
            }
        }

        /// <summary>
        /// Handle unit death - remove buffs
        /// </summary>
        public static void OnUnitDeath(BattleUnit unit)
        {
            if (_unitsWithBuffs.Contains(unit))
            {
                RemoveBuffs(unit);
            }
        }
    }

    /// <summary>
    /// Patch to register Unity system when game starts
    /// </summary>
    [HarmonyPatch(typeof(TestGame), "StartGame")]
    public static class Solidarity_StartGame_Patch
    {
        public static void Postfix()
        {
            if (!Solidarity.Instance.IsActive)
                return;

            SolidaritySystem.RegisterTimeUpdate();
        }
    }

    /// <summary>
    /// Patch to clear Unity state when mission ends
    /// </summary>
    [HarmonyPatch(typeof(TestGame), "EndGame")]
    public static class Solidarity_EndGame_Patch
    {
        public static void Postfix()
        {
            SolidaritySystem.ClearAll();
        }
    }

    /// <summary>
    /// Patch BattleUnit constructor to add OnDeath listener
    /// </summary>
    [HarmonyPatch(typeof(BattleUnit), MethodType.Constructor, [typeof(UnitData), typeof(Enumerations.Team), typeof(GridManager)])]
    public static class Solidarity_BattleUnit_Constructor_Patch
    {
        public static void Postfix(BattleUnit __instance, Enumerations.Team team)
        {
            if (!Solidarity.Instance.IsActive)
                return;

            if (team == Enumerations.Team.Player)
            {
                void action()
                {
                    SolidaritySystem.OnUnitDeath(__instance);
                    __instance.OnDeath -= action;
                }

                __instance.OnDeath += action;
            }
        }
    }
}
