using HarmonyLib;
using MelonLoader;
using SpaceCommander;
using SpaceCommander.Area;
using System.Collections.Generic;
using System.Linq;
using TimeSystem;
using static XenopurgeRougeLike.ModLocalization;

namespace XenopurgeRougeLike.CommonReinforcements
{
    /// <summary>
    /// 孤狼：当视野内没有队友时，速度+2，瞄准+20，近战伤害+2
    /// Lone Wolf: When no allies are in sight, +2 Speed, +20 Accuracy, +2 Melee Damage
    /// </summary>
    public class LoneWolf : Reinforcement
    {
        public const float SpeedBonus = 2f;
        public const float AccuracyBonus = 0.2f; // +20 Accuracy (displayed as x100)
        public const int MeleeDamageBonus = 2;

        public LoneWolf()
        {
            company = Company.Common;
            rarity = Rarity.Standard;
            stackable = false;
            name = L("common.lone_wolf.name");
            flavourText = L("common.lone_wolf.flavour");
            description = L("common.lone_wolf.description", (int)SpeedBonus, (int)(AccuracyBonus * 100), MeleeDamageBonus);
        }

        protected static LoneWolf _instance;
        public static LoneWolf Instance => _instance ??= new();
    }

    /// <summary>
    /// System to track and apply Lone Wolf buffs based on NO allies in sight
    /// </summary>
    public static class LoneWolfSystem
    {
        // Track which units currently have Lone Wolf buffs active
        private static HashSet<BattleUnit> _unitsWithBuffs = new();
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
        /// Check if a unit has NO allies in line of sight
        /// </summary>
        private static bool HasNoAlliesInSight(BattleUnit unit)
        {
            if (unit == null || !unit.IsAlive || unit.LineOfSight == null)
                return false;

            var visibleTiles = unit.LineOfSight.Tiles;
            if (visibleTiles == null)
                return true; // No vision = no allies visible

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
                            return false; // Ally found, NOT a lone wolf
                        }
                    }
                }
            }

            return true; // No allies found, is a lone wolf
        }

        /// <summary>
        /// Apply Lone Wolf buffs to a unit
        /// </summary>
        private static void ApplyBuffs(BattleUnit unit)
        {
            if (_unitsWithBuffs.Contains(unit))
                return;

            unit.ChangeStat(Enumerations.UnitStats.Speed, LoneWolf.SpeedBonus, "LoneWolf_Speed");
            unit.ChangeStat(Enumerations.UnitStats.Accuracy, LoneWolf.AccuracyBonus, "LoneWolf_Accuracy");
            unit.ChangeStat(Enumerations.UnitStats.Power, LoneWolf.MeleeDamageBonus, "LoneWolf_MeleeDamage");

            _unitsWithBuffs.Add(unit);
        }

        /// <summary>
        /// Remove Lone Wolf buffs from a unit
        /// </summary>
        private static void RemoveBuffs(BattleUnit unit)
        {
            if (!_unitsWithBuffs.Contains(unit))
                return;

            unit.ReverseChangeOfStat("LoneWolf_Speed");
            unit.ReverseChangeOfStat("LoneWolf_Accuracy");
            unit.ReverseChangeOfStat("LoneWolf_MeleeDamage");

            _unitsWithBuffs.Remove(unit);
        }

        /// <summary>
        /// Update all player units based on their line of sight
        /// Called by TimeManager
        /// </summary>
        private static void OnTimeUpdate(float deltaTime)
        {
            if (!LoneWolf.Instance.IsActive)
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

                bool isLoneWolf = HasNoAlliesInSight(unit);
                bool hasBuffs = _unitsWithBuffs.Contains(unit);

                if (isLoneWolf && !hasBuffs)
                {
                    // Unit is alone and doesn't have buffs - apply them
                    ApplyBuffs(unit);
                }
                else if (!isLoneWolf && hasBuffs)
                {
                    // Unit has allies nearby but has buffs - remove them
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
    /// Patch to register Lone Wolf system when game starts
    /// </summary>
    [HarmonyPatch(typeof(TestGame), "StartGame")]
    public static class LoneWolf_StartGame_Patch
    {
        public static void Postfix()
        {
            if (!LoneWolf.Instance.IsActive)
                return;

            LoneWolfSystem.RegisterTimeUpdate();
        }
    }

    /// <summary>
    /// Patch to clear Lone Wolf state when mission ends
    /// </summary>
    [HarmonyPatch(typeof(TestGame), "EndGame")]
    public static class LoneWolf_EndGame_Patch
    {
        public static void Postfix()
        {
            LoneWolfSystem.ClearAll();
        }
    }

    /// <summary>
    /// Patch BattleUnit constructor to add OnDeath listener
    /// </summary>
    [HarmonyPatch(typeof(BattleUnit), MethodType.Constructor, [typeof(UnitData), typeof(Enumerations.Team), typeof(GridManager)])]
    public static class LoneWolf_BattleUnit_Constructor_Patch
    {
        public static void Postfix(BattleUnit __instance, Enumerations.Team team)
        {
            if (!LoneWolf.Instance.IsActive)
                return;

            if (team == Enumerations.Team.Player)
            {
                void action()
                {
                    LoneWolfSystem.OnUnitDeath(__instance);
                    __instance.OnDeath -= action;
                }

                __instance.OnDeath += action;
            }
        }
    }
}
