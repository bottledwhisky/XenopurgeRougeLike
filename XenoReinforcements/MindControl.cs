using HarmonyLib;
using MelonLoader;
using SpaceCommander;
using SpaceCommander.ActionCards;
using SpaceCommander.Area;
using SpaceCommander.Area.Drawers;
using SpaceCommander.Audio;
using SpaceCommander.BattleManagement.UI;
using SpaceCommander.Commands;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using static XenopurgeRougeLike.ModLocalization;

namespace XenopurgeRougeLike.XenoReinforcements
{
    /// <summary>
    /// 控制异形：将一个敌人转化为友方单位，可使用2/4次
    /// Mind Control: Convert an enemy unit to your team, usable 2/4 times
    /// </summary>
    public class MindControl : Reinforcement
    {
        public static readonly int[] UsesPerStack = [2, 4];

        public MindControl()
        {
            company = Company.Xeno;
            rarity = Rarity.Elite;
            stackable = true;
            maxStacks = 2;
            name = L("xeno.mind_control.name");
            description = L("xeno.mind_control.description");
            flavourText = L("xeno.mind_control.flavour");
        }

        public override string GetDescriptionForStacks(int stacks)
        {
            return L("xeno.mind_control.description", UsesPerStack[stacks - 1]);
        }

        protected static MindControl instance;
        public static MindControl Instance => instance ??= new();

        public static HashSet<BattleUnit> MindControlledUnits = [];
    }


    // Patch to clear buffs when mission ends
    [HarmonyPatch(typeof(TestGame), "StartGame")]
    public class MindControl_TestGame_StartGame_Patch
    {
        public static void Postfix()
        {
            MindControl.MindControlledUnits.Clear();
        }
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

            if (unit == null || !unit.IsAlive || unit.Team != Enumerations.Team.EnemyAI || unit.UnitTag == Enumerations.UnitTag.Hive)
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
            if (!unit.IsAlive || unit.UnitTag == Enumerations.UnitTag.Hive)
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
        public string CustomCardName => L("xeno.mind_control.name");

        public string CustomCardDescription =>
            L("xeno.mind_control.card_description");

        public MindControlActionCardInfo()
        {
            AccessTools.Field(typeof(ActionCardInfo), "_uses").SetValue(this, 1);
            AccessTools.Field(typeof(ActionCardInfo), "canNotBeReplenished").SetValue(this, false);
        }
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
            _unitDataField = AccessTools.Field(typeof(BattleUnit), "_unitData");
            _currentHealthField = AccessTools.Field(typeof(BattleUnit), "_currentHealth");
            _currentMaxHealthField = AccessTools.Field(typeof(BattleUnit), "_currentMaxHealth");
            _currentArmorField = AccessTools.Field(typeof(BattleUnit), "_currentArmor");
            _currentSpeedField = AccessTools.Field(typeof(BattleUnit), "_currentSpeed");
            _currentAccuracyField = AccessTools.Field(typeof(BattleUnit), "_currentAccuracy");
            _currentPowerField = AccessTools.Field(typeof(BattleUnit), "_currentPower");
            _statChangesField = AccessTools.Field(typeof(BattleUnit), "_statChanges");
            _permanentStatChangesField = AccessTools.Field(typeof(BattleUnit), "_permanentStatChanges");
            _unitEquipmentManagerField = AccessTools.Field(typeof(BattleUnit), "_unitEquipmentManager");
            _isExtractedField = AccessTools.Field(typeof(BattleUnit), "_isExtracted");

            _battleUnitsField = AccessTools.Field(typeof(BattleUnitsManager), "_battleUnits");

            _getPawnMethod = AccessTools.Method(typeof(BattleUnitToPawnConnector), "GetPawn");
            _battleUnitToPawnField = AccessTools.Field(typeof(BattleUnitToPawnConnector), "_battleUnitToPawn");
        }

        /// <summary>
        /// Converts an enemy unit to the player's team by removing it and spawning a clone.
        /// </summary>
        /// <param name="enemyUnit">The enemy unit to convert</param>
        /// <param name="attackerUnit">Optional: The unit that triggered the conversion (e.g., via Submission melee). If provided, their close combat command will be interrupted.</param>
        public static void ConvertUnitToPlayer(BattleUnit enemyUnit, BattleUnit attackerUnit = null)
        {
            try
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
                enemyUnit.Damage(9999);

                // Step 4: Create a new player unit with the captured state
                var playerUnit = CreatePlayerUnit(capturedState, gameManager.GridManager);

                // Step 5: Add to player team and place on the battlefield
                playerManager.AddBattleUnit(playerUnit);
                playerUnit.PlaceOnTile(currentTile);
                playerUnit.AddCommands();
                playerUnit.StartCommandsExecution();

                MindControl.MindControlledUnits.Add(playerUnit);
                // Step 6: Create visual representation
                CreateVisualRepresentation(playerUnit, gameManager);

                // Track this unit as mind-controlled for DevourWill
                XenoStunTracker.MarkAsMindControlled(playerUnit);

                MelonLogger.Msg($"MindControl: Successfully converted {capturedState.UnitName} to player team");
            }
            catch (Exception e)
            {
                MelonLogger.Error(e);
                MelonLogger.Error(e.StackTrace);
            }
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

    /// <summary>
    /// Patch to prevent "Line index out of range" error in PathDrawer.ClearPath
    /// when mind-controlled units have invalid DeploymentOrder values.
    /// </summary>
    [HarmonyPatch(typeof(PathDrawer), "ClearPath")]
    public static class PathDrawer_ClearPath_Patch
    {
        public static bool Prefix(BattleUnit battleUnit, LineRenderer[] ____lineRenderers)
        {
            if (battleUnit == null)
                return false;

            if (MindControl.MindControlledUnits.Contains(battleUnit))
            {
                return false;
            }

            return true;
        }
    }

    /// <summary>
    /// Patch to prevent mind-controlled units from drawing paths.
    /// </summary>
    [HarmonyPatch(typeof(PathDrawer), "DrawLine")]
    public static class PathDrawer_DrawLine_Patch
    {
        public static bool Prefix(BattleUnit battleUnit)
        {
            if (MindControl.MindControlledUnits.Contains(battleUnit))
            {
                return false;
            }
            return true;
        }
    }

    /// <summary>
    /// Patch to prevent ArgumentNullException when mind-controlled xeno units try to speak.
    /// Xeno units don't have voice actors, so their VoiceActorId is null.
    /// </summary>
    [HarmonyPatch(typeof(WalkieTalkieManager), "SoldierSpeaked", typeof(string), typeof(Enumerations.VoiceSounds))]
    public static class WalkieTalkieManager_SoldierSpeaked_Patch
    {
        public static bool Prefix(string voiceActorId)
        {
            // Skip if voiceActorId is null (xeno units don't have voice actors)
            if (voiceActorId == null)
            {
                return false;
            }
            return true;
        }
    }

    /// <summary>
    /// Patch to prevent NullReferenceException when mind-controlled xeno units kill enemies.
    /// Xeno units don't have MeleeWeaponDataSO, so accessing it causes a null reference.
    /// </summary>
    [HarmonyPatch(typeof(Melee), "KilledEnemy")]
    public static class Melee_KilledEnemy_Patch
    {
        public static bool Prefix(BattleUnit ____battleUnit)
        {
            // Skip original method for mind-controlled units (they don't have weapon data)
            if (MindControl.MindControlledUnits.Contains(____battleUnit))
            {
                return false;
            }
            return true;
        }
    }

    /// <summary>
    /// Patch to prevent mind-controlled units from counting as dead soldiers for end-game penalties.
    /// </summary>
    [HarmonyPatch(typeof(GameManager), "GiveEndGameRewards")]
    public static class MindControl_GiveEndGameRewards_Patch
    {
        public static void Prefix()
        {
            if (MindControl.MindControlledUnits.Count == 0)
                return;

            var gameManager = GameManager.Instance;
            var teamsField = AccessTools.Field(typeof(GameManager), "_teams");
            var teams = teamsField.GetValue(gameManager) as Dictionary<Enumerations.Team, BattleUnitsManager>;
            var playerManager = teams[Enumerations.Team.Player];
            var deadUnits = playerManager.DeadUnits as List<BattleUnit>;

            foreach (var unit in MindControl.MindControlledUnits)
            {
                deadUnits.Remove(unit);
            }
        }
    }

    /// <summary>
    /// Patch to exclude mind-controlled units from action card targeting.
    /// </summary>
    [HarmonyPatch(typeof(ChooseUnitForCard_BattleManagementDirectory), "Initialize")]
    public static class MindControl_ChooseUnitForCard_Patch
    {
        private static List<BattleUnit> removedUnits = [];

        public static void Prefix(ChooseUnitForCard_BattleManagementDirectory __instance)
        {
            if (MindControl.MindControlledUnits.Count == 0)
                return;

            var battleUnitsManagerField = AccessTools.Field(typeof(ChooseUnitForCard_BattleManagementDirectory), "_battleUnitsManager");
            var battleUnitsManager = battleUnitsManagerField.GetValue(__instance) as BattleUnitsManager;
            var battleUnits = battleUnitsManager.BattleUnits as List<BattleUnit>;

            removedUnits.Clear();
            for (int i = battleUnits.Count - 1; i >= 0; i--)
            {
                if (MindControl.MindControlledUnits.Contains(battleUnits[i]))
                {
                    removedUnits.Add(battleUnits[i]);
                    battleUnits.RemoveAt(i);
                }
            }
        }

        public static void Postfix(ChooseUnitForCard_BattleManagementDirectory __instance)
        {
            if (removedUnits.Count == 0)
                return;

            var battleUnitsManagerField = AccessTools.Field(typeof(ChooseUnitForCard_BattleManagementDirectory), "_battleUnitsManager");
            var battleUnitsManager = battleUnitsManagerField.GetValue(__instance) as BattleUnitsManager;
            var battleUnits = battleUnitsManager.BattleUnits as List<BattleUnit>;

            foreach (var unit in removedUnits)
            {
                battleUnits.Add(unit);
            }
            removedUnits.Clear();
        }
    }
}
