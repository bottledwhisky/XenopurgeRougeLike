using HarmonyLib;
using MelonLoader;
using SpaceCommander;
using SpaceCommander.ActionCards;
using SpaceCommander.Area;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace XenopurgeRougeLike.XenoReinforcements
{
    /// <summary>
    /// 控制异形：将一个敌人转化为友方单位，可使用1/2次
    /// Mind Control: Convert an enemy unit to your team, usable 1/2 times
    /// </summary>
    public class MindControl : Reinforcement
    {
        public static readonly int[] UsesPerStack = [1, 2];

        public MindControl()
        {
            company = Company.Xeno;
            rarity = Rarity.Expert;
            stackable = true;
            maxStacks = 2;
            name = "Mind Control";
            description = "Take control of an enemy unit, converting it to your team. Usable {0} time(s) per mission.";
            flavourText = "By tapping into the hive mind, you can sever an alien's connection and bend it to your will.";
        }

        public override string GetDescriptionForStacks(int stacks)
        {
            return string.Format(description, UsesPerStack[stacks - 1]);
        }

        public static MindControl Instance => (MindControl)Xeno.Reinforcements[typeof(MindControl)];
    }

    /// <summary>
    /// Patch to inject MindControlActionCard into InBattleActionCardsManager after initialization
    /// </summary>
    [HarmonyPatch(typeof(InBattleActionCardsManager), "Initialize")]
    public static class MindControl_InjectActionCard_Patch
    {
        public static void Postfix(InBattleActionCardsManager __instance)
        {
            if (!MindControl.Instance.IsActive)
                return;

            var actionCardInfo = new MindControlActionCardInfo();
            actionCardInfo.SetId("MindControl");

            var mindControlCard = new MindControlActionCard(actionCardInfo);

            __instance.InBattleActionCards.Add(mindControlCard);
        }
    }

    /// <summary>
    /// Mind Control action card - converts an enemy unit to the player's team.
    /// Implements IUnitTargetable to target enemy units.
    /// </summary>
    public class MindControlActionCard : ActionCard, IUnitTargetable
    {
        public Enumerations.Team TeamToAffect => Enumerations.Team.EnemyAI;

        public MindControlActionCard(ActionCardInfo actionCardInfo)
        {
            Info = actionCardInfo;
            _usesLeft = MindControl.UsesPerStack[MindControl.Instance.currentStacks - 1];
        }

        public override ActionCard GetCopy()
        {
            return new MindControlActionCard(Info);
        }

        public void ApplyCommand(BattleUnit unit)
        {
            if (!MindControl.Instance.IsActive)
                return;

            if (unit == null || !unit.IsAlive || unit.Team != Enumerations.Team.EnemyAI)
                return;

            MindControlSystem.ConvertUnitToPlayer(unit);
        }

        IEnumerable<CommandsAvailabilityChecker.UnitAnavailableReasons> IUnitTargetable.IsUnitValid(BattleUnit unit)
        {
            var reasons = new List<CommandsAvailabilityChecker.UnitAnavailableReasons>();

            if (!MindControl.Instance.IsActive)
            {
                return reasons;
            }

            // Can only target alive units
            if (!unit.IsAlive)
            {
                reasons.Add(CommandsAvailabilityChecker.UnitAnavailableReasons.UnitIsDead);
            }

            // Note: TeamToAffect property handles team filtering - only enemy units will be shown

            return reasons;
        }
    }

    /// <summary>
    /// Custom ActionCardInfo for MindControl
    /// </summary>
    public class MindControlActionCardInfo : ActionCardInfo
    {
        public string CustomCardName => "Mind Control";

        public string CustomCardDescription =>
            "Take control of an enemy unit, permanently converting it to your team.";
    }

    /// <summary>
    /// Patch to intercept CardName getter for MindControlActionCardInfo
    /// </summary>
    [HarmonyPatch(typeof(ActionCardInfo), "CardName", MethodType.Getter)]
    public static class MindControlActionCardInfo_CardName_Patch
    {
        public static bool Prefix(ActionCardInfo __instance, ref string __result)
        {
            if (__instance is MindControlActionCardInfo customInfo)
            {
                __result = customInfo.CustomCardName;
                return false;
            }
            return true;
        }
    }

    /// <summary>
    /// Patch to intercept CardDescription getter for MindControlActionCardInfo
    /// </summary>
    [HarmonyPatch(typeof(ActionCardInfo), "CardDescription", MethodType.Getter)]
    public static class MindControlActionCardInfo_CardDescription_Patch
    {
        public static bool Prefix(ActionCardInfo __instance, ref string __result)
        {
            if (__instance is MindControlActionCardInfo customInfo)
            {
                __result = customInfo.CustomCardDescription;
                return false;
            }
            return true;
        }
    }

    /// <summary>
    /// System that handles the conversion of enemy units to player team.
    /// This is complex because the game architecture assumes units never change teams.
    /// The approach is to:
    /// 1. Capture all state from the original enemy unit
    /// 2. Remove the enemy unit from battle (without triggering death events)
    /// 3. Create a new player unit with the captured state
    /// 4. Update visual representation
    /// </summary>
    public static class MindControlSystem
    {
        // Cache reflection fields for performance
        private static FieldInfo _unitDataField;
        private static FieldInfo _currentHealthField;
        private static FieldInfo _currentMaxHealthField;
        private static FieldInfo _currentArmorField;
        private static FieldInfo _currentSpeedField;
        private static FieldInfo _currentAccuracyField;
        private static FieldInfo _currentPowerField;
        private static FieldInfo _statChangesField;
        private static FieldInfo _permanentStatChangesField;
        private static FieldInfo _unitEquipmentManagerField;
        private static FieldInfo _isExtractedField;
        private static FieldInfo _battleUnitsField;
        private static MethodInfo _getPawnMethod;
        private static FieldInfo _battleUnitToPawnField;

        static MindControlSystem()
        {
            var battleUnitType = typeof(BattleUnit);
            var flags = BindingFlags.Instance | BindingFlags.NonPublic;

            _unitDataField = battleUnitType.GetField("_unitData", flags);
            _currentHealthField = battleUnitType.GetField("_currentHealth", flags);
            _currentMaxHealthField = battleUnitType.GetField("_currentMaxHealth", flags);
            _currentArmorField = battleUnitType.GetField("_currentArmor", flags);
            _currentSpeedField = battleUnitType.GetField("_currentSpeed", flags);
            _currentAccuracyField = battleUnitType.GetField("_currentAccuracy", flags);
            _currentPowerField = battleUnitType.GetField("_currentPower", flags);
            _statChangesField = battleUnitType.GetField("_statChanges", flags);
            _permanentStatChangesField = battleUnitType.GetField("_permanentStatChanges", flags);
            _unitEquipmentManagerField = battleUnitType.GetField("_unitEquipmentManager", flags);
            _isExtractedField = battleUnitType.GetField("_isExtracted", flags);

            var managerType = typeof(BattleUnitsManager);
            _battleUnitsField = managerType.GetField("_battleUnits", flags);

            // BattleUnitToPawnConnector.GetPawn is internal, so we need reflection
            var connectorType = typeof(BattleUnitToPawnConnector);
            _getPawnMethod = connectorType.GetMethod("GetPawn", flags);
            _battleUnitToPawnField = connectorType.GetField("_battleUnitToPawn", flags);
        }

        /// <summary>
        /// Converts an enemy unit to the player's team by removing it and spawning a clone.
        /// </summary>
        public static void ConvertUnitToPlayer(BattleUnit enemyUnit)
        {
            if (enemyUnit == null || !enemyUnit.IsAlive || enemyUnit.Team != Enumerations.Team.EnemyAI)
            {
                MelonLogger.Warning("MindControl: Invalid target unit");
                return;
            }

            var gameManager = GameManager.Instance;
            if (gameManager == null)
            {
                MelonLogger.Error("MindControl: GameManager not found");
                return;
            }

            var enemyManager = gameManager.GetTeamManager(Enumerations.Team.EnemyAI);
            var playerManager = gameManager.GetTeamManager(Enumerations.Team.Player);

            if (enemyManager == null || playerManager == null)
            {
                MelonLogger.Error("MindControl: Team managers not found");
                return;
            }

            // Step 1: Capture the unit's current state
            var capturedState = CaptureUnitState(enemyUnit);

            // Step 2: Get the current tile before removing the unit
            Tile currentTile = enemyUnit.CurrentTile;

            // Step 3: Remove the enemy unit without triggering death events
            RemoveUnitSilently(enemyUnit, enemyManager, gameManager);

            // Step 4: Create a new player unit with the captured state
            var playerUnit = CreatePlayerUnit(capturedState, gameManager.GridManager);

            // Step 5: Add to player team and place on the battlefield
            playerManager.AddBattleUnit(playerUnit);
            playerUnit.PlaceOnTile(currentTile);
            playerUnit.AddCommands();
            playerUnit.StartCommandsExecution();

            // Step 6: Create visual representation
            CreateVisualRepresentation(playerUnit, gameManager);

            // Track this unit as mind-controlled for DevourWill
            XenoStunTracker.MarkAsMindControlled(playerUnit);

            MelonLogger.Msg($"MindControl: Successfully converted {capturedState.UnitName} to player team");
        }

        /// <summary>
        /// Captures all relevant state from a BattleUnit for cloning.
        /// </summary>
        private static CapturedUnitState CaptureUnitState(BattleUnit unit)
        {
            var state = new CapturedUnitState();

            // Capture UnitData (this contains the base unit configuration)
            var unitData = _unitDataField.GetValue(unit) as UnitData;
            state.UnitData = unitData?.CreateCopy();
            state.UnitName = unit.UnitNameNoNumber;

            // Capture current combat stats
            state.CurrentHealth = (float)_currentHealthField.GetValue(unit);
            state.CurrentMaxHealth = (float)_currentMaxHealthField.GetValue(unit);
            state.CurrentArmor = (float)_currentArmorField.GetValue(unit);
            state.CurrentSpeed = (float)_currentSpeedField.GetValue(unit);
            state.CurrentAccuracy = (float)_currentAccuracyField.GetValue(unit);
            state.CurrentPower = (float)_currentPowerField.GetValue(unit);

            // Capture stat changes (buffs/debuffs)
            var statChanges = _statChangesField.GetValue(unit) as Dictionary<string, (Enumerations.UnitStats, float)>;
            state.StatChanges = statChanges != null
                ? new Dictionary<string, (Enumerations.UnitStats, float)>(statChanges)
                : new Dictionary<string, (Enumerations.UnitStats, float)>();

            // Capture permanent stat changes
            var permanentChanges = _permanentStatChangesField.GetValue(unit) as List<(Enumerations.UnitStats, float)>;
            state.PermanentStatChanges = permanentChanges != null
                ? new List<(Enumerations.UnitStats, float)>(permanentChanges)
                : new List<(Enumerations.UnitStats, float)>();

            return state;
        }

        /// <summary>
        /// Removes a unit from battle without triggering death events or achievements.
        /// This manually performs the cleanup that would normally happen in ExtractBattleUnit/DisposeUnit.
        /// </summary>
        private static void RemoveUnitSilently(BattleUnit unit, BattleUnitsManager enemyManager, GameManager gameManager)
        {
            // First, get and destroy the visual representation
            var pawnConnector = gameManager.BattleUnitToPawnConnector;

            // GetPawn is internal, so we need to access it via reflection or the dictionary directly
            BattleUnitGO pawn = null;
            if (_battleUnitToPawnField != null)
            {
                var dict = _battleUnitToPawnField.GetValue(pawnConnector) as Dictionary<BattleUnit, BattleUnitGO>;
                if (dict != null && dict.TryGetValue(unit, out var foundPawn))
                {
                    pawn = foundPawn;
                }
            }

            if (pawn != null)
            {
                // Unbind the character from the connector
                pawnConnector.UnBindCharacter(unit);
                // Destroy the GameObject
                UnityEngine.Object.Destroy(pawn.gameObject);
            }

            // Set the _isExtracted flag to prevent OnDeath from triggering achievement logic
            _isExtractedField.SetValue(unit, true);

            // Stop command execution and movement
            unit.CommandsManager.StopCommandsExecution();
            unit.MovementManager.ImmediatelyStopMovement();

            // Remove from tile
            if (unit.CurrentTile != null)
            {
                unit.CurrentTile.CurrentStateOfTile.RemoveUnit(unit);
            }

            // Clear line of sight
            unit.LineOfSight.ClearReferencesAndEvents();

            // Dispose abilities
            unit.UnitAbilityManager.DisposeAbilities();

            // Remove from the enemy manager's list directly (avoiding ExtractBattleUnit which fires events)
            var battleUnits = _battleUnitsField.GetValue(enemyManager) as List<BattleUnit>;
            if (battleUnits != null)
            {
                battleUnits.Remove(unit);
            }
        }

        /// <summary>
        /// Creates a new BattleUnit for the player team with the captured state.
        /// </summary>
        private static BattleUnit CreatePlayerUnit(CapturedUnitState state, GridManager gridManager)
        {
            // Update the UnitData with the captured health (so the new unit starts with correct health)
            if (state.UnitData != null)
            {
                state.UnitData.CurrentHealth = state.CurrentHealth;
            }

            // Create the new unit with Player team
            var playerUnit = new BattleUnit(state.UnitData, Enumerations.Team.Player, gridManager);

            // Restore current stats by using reflection
            // Note: The constructor initializes stats from UnitData, but we may have additional changes
            _currentHealthField.SetValue(playerUnit, state.CurrentHealth);
            _currentMaxHealthField.SetValue(playerUnit, state.CurrentMaxHealth);
            _currentArmorField.SetValue(playerUnit, state.CurrentArmor);
            _currentSpeedField.SetValue(playerUnit, state.CurrentSpeed);
            _currentAccuracyField.SetValue(playerUnit, state.CurrentAccuracy);
            _currentPowerField.SetValue(playerUnit, state.CurrentPower);

            // Restore stat changes (buffs/debuffs)
            // Get the existing dictionary and copy our captured values
            var currentStatChanges = _statChangesField.GetValue(playerUnit) as Dictionary<string, (Enumerations.UnitStats, float)>;
            if (currentStatChanges != null && state.StatChanges != null)
            {
                foreach (var kvp in state.StatChanges)
                {
                    currentStatChanges[kvp.Key] = kvp.Value;
                }
            }

            // Restore permanent stat changes
            var currentPermanentChanges = _permanentStatChangesField.GetValue(playerUnit) as List<(Enumerations.UnitStats, float)>;
            if (currentPermanentChanges != null && state.PermanentStatChanges != null)
            {
                currentPermanentChanges.AddRange(state.PermanentStatChanges);
            }

            return playerUnit;
        }

        /// <summary>
        /// Creates the visual representation (BattleUnitGO) for the new player unit.
        /// </summary>
        private static void CreateVisualRepresentation(BattleUnit playerUnit, GameManager gameManager)
        {
            // Find the Test_PawnsPosition component to create the visual pawn
            var testPawnsPosition = UnityEngine.Object.FindAnyObjectByType<SpaceCommander.Tests.Test_PawnsPosition>();
            if (testPawnsPosition != null)
            {
                testPawnsPosition.CreatePawn(playerUnit);
            }
            else
            {
                MelonLogger.Warning("MindControl: Could not find Test_PawnsPosition to create visual pawn");
            }
        }
    }

    /// <summary>
    /// Data class to hold captured unit state during the conversion process.
    /// </summary>
    public class CapturedUnitState
    {
        public UnitData UnitData { get; set; }
        public string UnitName { get; set; }
        public float CurrentHealth { get; set; }
        public float CurrentMaxHealth { get; set; }
        public float CurrentArmor { get; set; }
        public float CurrentSpeed { get; set; }
        public float CurrentAccuracy { get; set; }
        public float CurrentPower { get; set; }
        public Dictionary<string, (Enumerations.UnitStats, float)> StatChanges { get; set; }
        public List<(Enumerations.UnitStats, float)> PermanentStatChanges { get; set; }
    }
}
