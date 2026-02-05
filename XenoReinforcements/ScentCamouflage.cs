using HarmonyLib;
using SpaceCommander;
using SpaceCommander.ActionCards;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TimeSystem;
using XenopurgeRougeLike.RockstarReinforcements;
using static SpaceCommander.Enumerations;
using static XenopurgeRougeLike.ModLocalization;

namespace XenopurgeRougeLike.XenoReinforcements
{
    /// <summary>
    /// 气味伪装：敌人不会主动攻击拥有气味伪装的单位，持续5秒+5秒*控制加成等级
    /// Scent Camouflage: Enemies will not actively target units with scent camouflage
    /// Duration: 5s + 5s * ControlDurationBonusLevel
    /// Usable 2/4 times per mission
    /// </summary>
    public class ScentCamouflage : Reinforcement
    {
        public const float BaseDuration = 5f;
        public const float BonusDurationPerLevel = 5f;
        public static readonly int Uses = 1;

        public ScentCamouflage()
        {
            company = Company.Xeno;
            name = L("xeno.scent_camouflage.name");
            description = L("xeno.scent_camouflage.description", GetDuration(), Uses);
            flavourText = L("xeno.scent_camouflage.flavour");
        }

        public override string GetDescriptionForStacks(int stacks)
        {
            return L("xeno.scent_camouflage.description", GetDuration(), Uses);
        }

        public static float GetDuration()
        {
            return BaseDuration + BonusDurationPerLevel * Xeno.GetControlDurationBonusLevel();
        }

        protected static ScentCamouflage instance;
        public static ScentCamouflage Instance => instance ??= new();
    }

    /// <summary>
    /// Tracks which units currently have scent camouflage active and their remaining duration
    /// </summary>
    public static class ScentCamouflageSystem
    {
        private static Dictionary<BattleUnit, float> _camouflagedUnits = new Dictionary<BattleUnit, float>();
        private static bool _isRegistered = false;

        public static void ActivateCamouflage(BattleUnit unit, float duration)
        {
            if (unit != null && unit.Team == Team.Player)
            {
                _camouflagedUnits[unit] = duration;
            }
        }

        public static void DeactivateCamouflage(BattleUnit unit)
        {
            _camouflagedUnits.Remove(unit);
        }

        public static bool HasCamouflage(BattleUnit unit)
        {
            return _camouflagedUnits.ContainsKey(unit) && _camouflagedUnits[unit] > 0f;
        }

        public static void ClearAll()
        {
            UnregisterTimeUpdate();
            _camouflagedUnits.Clear();
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
        /// Update all camouflage timers. Called by TimeManager.
        /// </summary>
        private static void OnTimeUpdate(float deltaTime)
        {
            if (_camouflagedUnits.Count == 0)
                return;

            var unitsToRemove = new List<BattleUnit>();

            foreach (var unit in _camouflagedUnits.Keys.ToList())
            {
                var newTime = _camouflagedUnits[unit] - deltaTime;
                if (newTime <= 0f)
                {
                    unitsToRemove.Add(unit);
                }
                else
                {
                    _camouflagedUnits[unit] = newTime;
                }
            }

            foreach (var unit in unitsToRemove)
            {
                _camouflagedUnits.Remove(unit);
            }
        }
    }

    /// <summary>
    /// Patch to inject ScentCamouflageActionCard into InBattleActionCardsManager after initialization
    /// </summary>
    [HarmonyPatch(typeof(InBattleActionCardsManager), "Initialize")]
    public static class ScentCamouflage_InjectActionCard_Patch
    {
        public static void Postfix(InBattleActionCardsManager __instance)
        {
            if (!ScentCamouflage.Instance.IsActive)
                return;

            var actionCardInfo = new ScentCamouflageActionCardInfo();
            actionCardInfo.SetId("ScentCamouflage");

            var scentCamouflageCard = new ScentCamouflageActionCard(actionCardInfo);

            __instance.InBattleActionCards.Add(scentCamouflageCard);
        }
    }

    /// <summary>
    /// Clear camouflage when mission ends
    /// </summary>
    [HarmonyPatch(typeof(TestGame), "EndGame")]
    public static class ScentCamouflage_MissionEnd_Patch
    {
        public static void Postfix()
        {
            ScentCamouflageSystem.ClearAll();
        }
    }

    /// <summary>
    /// Scent Camouflage action card - applies camouflage to a target unit.
    /// Implements IUnitTargetable to target player units.
    /// </summary>
    public class ScentCamouflageActionCard : ActionCard, IUnitTargetable
    {
        public Team TeamToAffect => Team.Player;

        public ScentCamouflageActionCard(ActionCardInfo actionCardInfo)
        {
            Info = actionCardInfo;
            _usesLeft = ScentCamouflage.Uses;
        }

        public override ActionCard GetCopy()
        {
            return new ScentCamouflageActionCard(Info);
        }

        public void ApplyCommand(BattleUnit unit)
        {
            if (!ScentCamouflage.Instance.IsActive)
                return;

            if (unit == null || !unit.IsAlive || unit.Team != Team.Player)
                return;

            var duration = ScentCamouflage.GetDuration();
            ScentCamouflageSystem.ActivateCamouflage(unit, duration);
            ScentCamouflageSystem.RegisterTimeUpdate();
        }

        IEnumerable<CommandsAvailabilityChecker.UnitAnavailableReasons> IUnitTargetable.IsUnitValid(BattleUnit unit)
        {
            var reasons = new List<CommandsAvailabilityChecker.UnitAnavailableReasons>();

            if (!ScentCamouflage.Instance.IsActive)
            {
                return reasons;
            }

            // Can only target alive, non-fan, non-turret human units
            if (!unit.IsAlive || unit.Team != Team.Player || MindControl.MindControlledUnits.Contains(unit) || UnitsPlacementPhasePatch.IsFan(unit) || unit.UnitTag == UnitTag.Turret)
            {
                reasons.Add(CommandsAvailabilityChecker.UnitAnavailableReasons.UnitIsDead);
            }

            // Note: TeamToAffect property handles team filtering - only player units will be shown

            return reasons;
        }
    }

    /// <summary>
    /// Custom ActionCardInfo for ScentCamouflage
    /// </summary>
    public class ScentCamouflageActionCardInfo : ActionCardInfo
    {
        public string CustomCardName => L("xeno.scent_camouflage.name");

        public string CustomCardDescription =>
            L("xeno.scent_camouflage.card_description", ScentCamouflage.GetDuration());

        public ScentCamouflageActionCardInfo()
        {
            AccessTools.Field(typeof(ActionCardInfo), "_uses").SetValue(this, 1);
            AccessTools.Field(typeof(ActionCardInfo), "canNotBeReplenished").SetValue(this, false);
        }
    }

    /// <summary>
    /// Patch to intercept CardName getter for ScentCamouflageActionCardInfo
    /// </summary>
    [HarmonyPatch(typeof(ActionCardInfo), "CardName", MethodType.Getter)]
    public static class ScentCamouflageActionCardInfo_CardName_Patch
    {
        public static bool Prefix(ActionCardInfo __instance, ref string __result)
        {
            if (__instance is ScentCamouflageActionCardInfo customInfo)
            {
                __result = customInfo.CustomCardName;
                return false;
            }
            return true;
        }
    }

    /// <summary>
    /// Patch to intercept CardDescription getter for ScentCamouflageActionCardInfo
    /// </summary>
    [HarmonyPatch(typeof(ActionCardInfo), "CardDescription", MethodType.Getter)]
    public static class ScentCamouflageActionCardInfo_CardDescription_Patch
    {
        public static bool Prefix(ActionCardInfo __instance, ref string __result)
        {
            if (__instance is ScentCamouflageActionCardInfo customInfo)
            {
                __result = customInfo.CustomCardDescription;
                return false;
            }
            return true;
        }
    }

    /// <summary>
    /// Patch LockTarget.GetClosestVisibleUnitFromTeam to skip camouflaged units when possible
    /// This is the core targeting logic that all AI commands use
    /// </summary>
    [HarmonyPatch(typeof(LockTarget), "GetClosestVisibleUnitFromTeam")]
    public static class ScentCamouflage_LockTarget_Patch
    {
        private static FieldInfo _lineOfSightField;
        private static FieldInfo _targetTeamField;
        private static FieldInfo _currentPositionField;
        private static FieldInfo _selfField;

        static ScentCamouflage_LockTarget_Patch()
        {
            var type = typeof(LockTarget);
            var bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic;
            _lineOfSightField = type.GetField("_lineOfSight", bindingFlags);
            _targetTeamField = type.GetField("_targetTeam", bindingFlags);
            _currentPositionField = type.GetField("_currentPosition", bindingFlags);
            _selfField = type.GetField("_self", bindingFlags);
        }

        public static bool Prefix(LockTarget __instance, ref BattleUnit __result)
        {
            if (!ScentCamouflage.Instance.IsActive)
                return true;

            // Get the targeting team - only affect enemy AI targeting player units
            var targetTeam = (Team)_targetTeamField.GetValue(__instance);
            if (targetTeam != Team.Player)
                return true; // Let original handle non-player targeting

            var self = (BattleUnit)_selfField.GetValue(__instance);
            if (self == null || self.Team != Team.EnemyAI)
                return true; // Only affect enemy AI

            var lineOfSight = (LineOfSight)_lineOfSightField.GetValue(__instance);
            var currentPosition = (UnityEngine.Vector2Int)_currentPositionField.GetValue(__instance);

            // Get all visible tiles with player units
            var visibleTiles = lineOfSight.Tiles
                .Where(tile => tile.CurrentStateOfTile.IsOccupiedByTeam(targetTeam))
                .ToList();

            // Collect all visible player units
            var allVisibleUnits = new List<BattleUnit>();
            foreach (var tile in visibleTiles)
            {
                var unitsOnTile = tile.CurrentStateOfTile.GetUnitsOnTile(targetTeam)
                    .Where(u => u != self && u.CanBeFollowed);
                allVisibleUnits.AddRange(unitsOnTile);
            }

            if (!allVisibleUnits.Any())
            {
                __result = null;
                return false;
            }

            // Filter out all camouflaged units - they cannot be targeted at all
            var targetableUnits = allVisibleUnits
                .Where(u => !ScentCamouflageSystem.HasCamouflage(u))
                .ToList();

            // If all units are camouflaged, return null (no target)
            if (!targetableUnits.Any())
            {
                __result = null;
                return false;
            }

            // Find the closest unit from targetable units
            float minDistance = float.MaxValue;
            BattleUnit closestUnit = null;

            foreach (var unit in targetableUnits)
            {
                var distance = UnityEngine.Vector2Int.Distance(currentPosition, unit.MovementManager.CurrentTileCoords);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestUnit = unit;
                }
            }

            __result = closestUnit;
            return false; // Skip original method
        }
    }

}
