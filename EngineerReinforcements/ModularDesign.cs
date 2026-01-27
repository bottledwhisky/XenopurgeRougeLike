using HarmonyLib;
using SpaceCommander;
using SpaceCommander.ActionCards;
using SpaceCommander.Area;
using SpaceCommander.Commands;
using SpaceCommander.Weapons;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using static SpaceCommander.Enumerations;

namespace XenopurgeRougeLike.EngineerReinforcements
{
    /// <summary>
    /// 模块化设计：炮台被摧毁后可以在原地重新部署
    /// Modular Design: Destroyed turrets can be redeployed at their original location
    /// </summary>
    public class ModularDesign : Reinforcement
    {
        public ModularDesign()
        {
            company = Company.Engineer;
            rarity = Rarity.Elite;
            stackable = false;
            name = "Modular Design";
            description = "Destroyed turrets can be quickly redeployed at their original location.";
            flavourText = "Prefabricated turret modules allow for rapid redeployment on familiar mounting points.";
        }

        protected static ModularDesign instance;
        public static ModularDesign Instance => instance ??= new();
    }

    /// <summary>
    /// Stores information about a destroyed turret for redeployment
    /// </summary>
    public class DestroyedTurretInfo
    {
        public Tile Tile { get; set; }
        public SetTurretCommandDataSO TurretCommandData { get; set; }
        public BattleUnit OwnerUnit { get; set; }

        public DestroyedTurretInfo(Tile tile, SetTurretCommandDataSO turretCommandData, BattleUnit ownerUnit)
        {
            Tile = tile;
            TurretCommandData = turretCommandData;
            OwnerUnit = ownerUnit;
        }
    }

    /// <summary>
    /// System to track destroyed turrets and their locations
    /// </summary>
    public static class TurretRedeploymentTracker
    {
        private static readonly List<DestroyedTurretInfo> _destroyedTurrets = new List<DestroyedTurretInfo>();
        // Map from turret instance to its SetTurretCommandDataSO
        private static readonly Dictionary<Turret, SetTurretCommandDataSO> _turretCommandDataMap = new Dictionary<Turret, SetTurretCommandDataSO>();

        public static IEnumerable<DestroyedTurretInfo> DestroyedTurrets => _destroyedTurrets;

        public static void RegisterTurretCommandData(Turret turret, SetTurretCommandDataSO commandData)
        {
            if (turret != null && commandData != null)
            {
                _turretCommandDataMap[turret] = commandData;
            }
        }

        public static void TrackDestroyedTurret(Turret turret, BattleUnit ownerUnit)
        {
            if (turret == null || turret.Tile == null)
                return;

            // Get the turret command data from our tracking map
            if (!_turretCommandDataMap.TryGetValue(turret, out var turretCommandData))
                return;

            var destroyedTurret = new DestroyedTurretInfo(
                turret.Tile,
                turretCommandData,
                ownerUnit
            );

            _destroyedTurrets.Add(destroyedTurret);

            // Clean up the mapping since the turret is destroyed
            _turretCommandDataMap.Remove(turret);
        }

        public static void RemoveRedeployedTurret(DestroyedTurretInfo turretInfo)
        {
            _destroyedTurrets.Remove(turretInfo);
        }

        public static void ClearAll()
        {
            _destroyedTurrets.Clear();
            _turretCommandDataMap.Clear();
        }

        public static bool HasDestroyedTurrets()
        {
            return _destroyedTurrets.Any();
        }
    }

    /// <summary>
    /// Patch to track turrets when they are destroyed
    /// </summary>
    [HarmonyPatch(typeof(Turret), "BreakTurret")]
    public static class ModularDesign_TrackDestroyedTurret_Patch
    {
        private static readonly AccessTools.FieldRef<Turret, BattleUnit> _battleUnitCreatedRef =
            AccessTools.FieldRefAccess<Turret, BattleUnit>("_battleUnitCreated");

        public static void Prefix(Turret __instance)
        {
            if (!ModularDesign.Instance.IsActive)
                return;

            // Try to find the owner unit by looking at who deployed this turret
            // We'll need to track this through the SetTurretCommand
            var battleUnit = _battleUnitCreatedRef(__instance);
            if (battleUnit != null)
            {
                // For now, we'll pass null as ownerUnit since we don't have a direct reference
                // The action card will handle finding valid deployers
                TurretRedeploymentTracker.TrackDestroyedTurret(__instance, null);
            }
        }
    }

    /// <summary>
    /// Patch to track which unit deployed a turret and store the command data
    /// </summary>
    [HarmonyPatch(typeof(Turret), "PlaceTurretOnTile")]
    public static class ModularDesign_TrackTurretDeployment_Patch
    {
        // Temporary storage for the most recent turret deployment
        private static SetTurretCommandDataSO _pendingCommandData;

        public static void Prefix(Turret __instance)
        {
            if (!ModularDesign.Instance.IsActive)
                return;

            // Register the turret with its command data
            if (_pendingCommandData != null)
            {
                TurretRedeploymentTracker.RegisterTurretCommandData(__instance, _pendingCommandData);
                _pendingCommandData = null;
            }
        }

        // Called from SetTurretCommand patch to set pending data
        public static void SetPendingDeployment(SetTurretCommandDataSO commandData)
        {
            _pendingCommandData = commandData;
        }
    }

    /// <summary>
    /// Patch to intercept SetTurret to capture the command data before turret creation
    /// </summary>
    [HarmonyPatch(typeof(SetTurretCommand), "SetTurret")]
    public static class ModularDesign_CaptureCommandData_Patch
    {
        private static readonly AccessTools.FieldRef<SetTurretCommand, SetTurretCommandDataSO> _setTurretCommandDataSORef =
            AccessTools.FieldRefAccess<SetTurretCommand, SetTurretCommandDataSO>("_setTurretCommandDataSO");

        public static void Prefix(SetTurretCommand __instance)
        {
            if (!ModularDesign.Instance.IsActive)
                return;

            var turretCommandData = _setTurretCommandDataSORef(__instance);

            if (turretCommandData != null)
            {
                // Store this data so PlaceTurretOnTile patch can use it
                ModularDesign_TrackTurretDeployment_Patch.SetPendingDeployment(turretCommandData);
            }
        }
    }

    /// <summary>
    /// Patch to inject RedeployTurretActionCard into InBattleActionCardsManager
    /// </summary>
    [HarmonyPatch(typeof(InBattleActionCardsManager), "Initialize")]
    public static class ModularDesign_InjectActionCard_Patch
    {
        public static void Postfix(InBattleActionCardsManager __instance)
        {
            if (!ModularDesign.Instance.IsActive)
                return;

            // Create custom ActionCardInfo
            var actionCardInfo = new RedeployTurretActionCardInfo();
            actionCardInfo.SetId("RedeployTurret");

            // Create and add the RedeployTurretActionCard instance
            var redeployCard = new RedeployTurretActionCard(actionCardInfo);

            // Add to the InBattleActionCards list
            __instance.InBattleActionCards.Add(redeployCard);
        }
    }

    /// <summary>
    /// Action card that allows redeploying destroyed turrets
    /// Similar to SetTurret command but only at destroyed turret locations
    /// </summary>
    public class RedeployTurretActionCard : ActionCard, IUnitAndTileTargetable
    {
        private BattleUnit _selectedUnit;
        private Tile _selectedTile;
        private DestroyedTurretInfo _selectedTurretInfo;

        public Team TeamToAffect => Team.Player;

        public RedeployTurretActionCard(ActionCardInfo actionCardInfo)
        {
            Info = actionCardInfo;
        }

        public override ActionCard GetCopy()
        {
            return new RedeployTurretActionCard(Info);
        }

        public void ApplyCommand(BattleUnit unit, Tile tile)
        {
            _selectedUnit = unit;
            _selectedTile = tile;
            ApplyCommand();
        }

        public void ApplyCommand(BattleUnit unit, SpecialTile specialTile)
        {
            // Not applicable for turret redeployment
        }

        public IEnumerable<CommandsAvailabilityChecker.UnitAnavailableReasons> IsUnitValid(BattleUnit unit)
        {
            var reasons = new List<CommandsAvailabilityChecker.UnitAnavailableReasons>();
            if (unit == null || !unit.IsAlive || unit.Team != Team.Player)
            {
                reasons.Add(CommandsAvailabilityChecker.UnitAnavailableReasons.UnitIsDead);
            }
            return reasons;
        }

        public IEnumerable<CommandsAvailabilityChecker.TileAnavailableReasons> IsTileValid(Tile tile, BattleUnit unit)
        {
            var reasons = new List<CommandsAvailabilityChecker.TileAnavailableReasons>();

            if (!ModularDesign.Instance.IsActive)
            {
                reasons.Add(CommandsAvailabilityChecker.TileAnavailableReasons.AlreadyHasLogic);
                return reasons;
            }

            // Check if the tile matches a destroyed turret location
            var matchingTurret = TurretRedeploymentTracker.DestroyedTurrets
                .FirstOrDefault(t => t.Tile == tile);

            if (matchingTurret == null)
            {
                reasons.Add(CommandsAvailabilityChecker.TileAnavailableReasons.AlreadyHasLogic);
                return reasons;
            }

            // Check if tile is not occupied
            var deployedItemsManager = GameManager.Instance?.DeployedItemsManager;
            if (deployedItemsManager != null && deployedItemsManager.IsTileOccupied(tile))
            {
                reasons.Add(CommandsAvailabilityChecker.TileAnavailableReasons.ItemAlreadyDeployedThere);
                return reasons;
            }

            // Store the selection for ApplyCommand
            _selectedTurretInfo = matchingTurret;

            return reasons;
        }

        public IEnumerable<CommandsAvailabilityChecker.TileAnavailableReasons> IsTileValid(SpecialTile specialTile, BattleUnit unit)
        {
            // Not applicable for turret redeployment
            return new List<CommandsAvailabilityChecker.TileAnavailableReasons> { CommandsAvailabilityChecker.TileAnavailableReasons.AlreadyHasLogic };
        }

        public Enumerations.TypeOfTileForCommand GetTypeOfTile()
        {
            // Return None to allow targeting any tile (the IsTileValid method will determine which specific tiles are valid)
            return Enumerations.TypeOfTileForCommand.None;
        }

        public void ApplyCommand()
        {
            if (!ModularDesign.Instance.IsActive || _selectedUnit == null || _selectedTile == null)
                return;

            if (_selectedTurretInfo == null)
                return;

            // Create a MoveToSpecificLocationForActionCommandDataSO that wraps the turret deployment
            var moveToActionCommandData = ScriptableObject.CreateInstance<MoveToSpecificLocationForActionCommandDataSO>();

            // Use reflection to set the private _actionCommandDataSO field
            var actionCommandDataSOField = typeof(MoveToSpecificLocationForActionCommandDataSO)
                .GetField("_actionCommandDataSO", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            actionCommandDataSOField?.SetValue(moveToActionCommandData, _selectedTurretInfo.TurretCommandData);

            // Create the move-to-action command
            var moveToAction = new MoveToSpecificLocationForAction(_selectedUnit, _selectedTile);
            moveToAction.SetCostOfActionCard(GetCostOfActionCard());
            moveToAction.InitializeValues(moveToActionCommandData);

            // Override current command with the turret redeployment
            _selectedUnit.CommandsManager.OverrideCurrentCommandFromActionCard(moveToAction, GetCostOfActionCard());

            // Remove this turret from the destroyed list
            TurretRedeploymentTracker.RemoveRedeployedTurret(_selectedTurretInfo);
        }
    }

    /// <summary>
    /// Custom ActionCardInfo for RedeployTurret
    /// </summary>
    public class RedeployTurretActionCardInfo : ActionCardInfo
    {
        public string CustomCardName => "Redeploy Turret";

        public string CustomCardDescription =>
            "Quickly redeploy a destroyed turret at its original location.";
    }

    /// <summary>
    /// Patch to intercept CardName getter for RedeployTurretActionCardInfo
    /// </summary>
    [HarmonyPatch(typeof(ActionCardInfo), "CardName", MethodType.Getter)]
    public static class RedeployTurretActionCardInfo_CardName_Patch
    {
        public static bool Prefix(ActionCardInfo __instance, ref string __result)
        {
            if (__instance is RedeployTurretActionCardInfo customInfo)
            {
                __result = customInfo.CustomCardName;
                return false;
            }
            return true;
        }
    }

    /// <summary>
    /// Patch to intercept CardDescription getter for RedeployTurretActionCardInfo
    /// </summary>
    [HarmonyPatch(typeof(ActionCardInfo), "CardDescription", MethodType.Getter)]
    public static class RedeployTurretActionCardInfo_CardDescription_Patch
    {
        public static bool Prefix(ActionCardInfo __instance, ref string __result)
        {
            if (__instance is RedeployTurretActionCardInfo customInfo)
            {
                __result = customInfo.CustomCardDescription;
                return false;
            }
            return true;
        }
    }

    /// <summary>
    /// Clear tracked turrets when mission ends
    /// </summary>
    [HarmonyPatch(typeof(TestGame), "EndGame")]
    public static class ModularDesign_ClearTracking_Patch
    {
        public static void Postfix()
        {
            TurretRedeploymentTracker.ClearAll();
        }
    }
}
