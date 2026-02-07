using HarmonyLib;
using MelonLoader;
using SpaceCommander;
using SpaceCommander.ActionCards;
using SpaceCommander.Area;
using SpaceCommander.BattleManagement;
using SpaceCommander.BattleManagement.UI;
using SpaceCommander.Tests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using static SpaceCommander.Enumerations;
using static XenopurgeRougeLike.ModLocalization;

namespace XenopurgeRougeLike.SupportReinforcements
{
    /// <summary>
    /// 心脏起搏器：获得心脏起搏器指令，使一名已阵亡队友复活并回复50%血量，一场战斗仅限一次
    /// Cardiac Defibrillator: Gain a Defibrillator action card that revives a dead teammate at 50% HP, once per battle
    /// </summary>
    public class CardiacDefibrillator : Reinforcement
    {
        public CardiacDefibrillator()
        {
            company = Company.Support;
            rarity = Rarity.Elite;
            stackable = false;
            name = L("support.cardiac_defibrillator.name");
            description = L("support.cardiac_defibrillator.description");
            flavourText = L("support.cardiac_defibrillator.flavour");
        }

        protected static CardiacDefibrillator instance;
        public static CardiacDefibrillator Instance => instance ??= new();
    }

    /// <summary>
    /// Stores information about a dead teammate for revival
    /// </summary>
    public class DeadTeammateInfo
    {
        public Tile DeathTile { get; set; }
        public UnitData UnitData { get; set; }
        public float MaxHealth { get; set; }
        public string UnitName { get; set; }

        public DeadTeammateInfo(Tile deathTile, UnitData unitData, float maxHealth, string unitName)
        {
            DeathTile = deathTile;
            UnitData = unitData;
            MaxHealth = maxHealth;
            UnitName = unitName;
        }
    }

    /// <summary>
    /// Tracks dead teammates and whether the defibrillator has been used this battle
    /// </summary>
    public static class DefibrillatorTracker
    {
        private static readonly List<DeadTeammateInfo> _deadTeammates = [];
        private static bool _usedThisBattle = false;

        // Cache reflection fields
        private static readonly FieldInfo _unitDataField = AccessTools.Field(typeof(BattleUnit), "_unitData");
        private static readonly FieldInfo _currentMaxHealthField = AccessTools.Field(typeof(BattleUnit), "_currentMaxHealth");

        public static IEnumerable<DeadTeammateInfo> DeadTeammates => _deadTeammates;

        public static bool UsedThisBattle => _usedThisBattle;

        public static void TrackDeadTeammate(BattleUnit unit)
        {
            if (unit == null || unit.CurrentTile == null)
            {
                MelonLogger.Warning("DefibrillatorTracker: Unit or tile is null");
                return;
            }

            // Exclude mind-controlled aliens
            if (XenoReinforcements.MindControl.MindControlledUnits.Contains(unit))
            {
                MelonLogger.Msg("DefibrillatorTracker: Ignoring mind-controlled alien death");
                return;
            }

            // Exclude fans
            if (RockstarReinforcements.UnitsPlacementPhasePatch.IsFan(unit))
            {
                MelonLogger.Msg("DefibrillatorTracker: Ignoring fan death");
                return;
            }

            var unitData = _unitDataField.GetValue(unit) as UnitData;
            var maxHealth = (float)_currentMaxHealthField.GetValue(unit);

            if (unitData == null)
            {
                MelonLogger.Warning("DefibrillatorTracker: Could not capture unit data");
                return;
            }

            var info = new DeadTeammateInfo(
                unit.CurrentTile,
                unitData.CreateCopy(),
                maxHealth,
                unit.UnitNameNoNumber
            );

            _deadTeammates.Add(info);
            MelonLogger.Msg($"DefibrillatorTracker: Tracked death of {info.UnitName} at {info.DeathTile.Coords}");
        }

        public static void MarkUsed()
        {
            _usedThisBattle = true;
        }

        public static void RemoveDeadTeammate(DeadTeammateInfo info)
        {
            _deadTeammates.Remove(info);
        }

        public static bool HasDeadTeammates()
        {
            return !_usedThisBattle && _deadTeammates.Any();
        }

        public static void ClearAll()
        {
            _deadTeammates.Clear();
            _usedThisBattle = false;
        }
    }

    /// <summary>
    /// Patch BattleUnit constructor to add OnDeath listener for tracking dead teammates
    /// </summary>
    [HarmonyPatch(typeof(BattleUnit), MethodType.Constructor, [typeof(UnitData), typeof(Team), typeof(GridManager)])]
    public static class CardiacDefibrillator_BattleUnit_Constructor_Patch
    {
        public static void Postfix(BattleUnit __instance, Team team)
        {
            if (!CardiacDefibrillator.Instance.IsActive)
                return;

            if (team != Team.Player)
                return;

            void action()
            {
                DefibrillatorTracker.TrackDeadTeammate(__instance);
                __instance.OnDeath -= action;
            }

            __instance.OnDeath += action;
        }
    }

    /// <summary>
    /// Inject CardiacDefibrillatorActionCard into InBattleActionCardsManager
    /// </summary>
    [HarmonyPatch(typeof(InBattleActionCardsManager), "Initialize")]
    public static class CardiacDefibrillator_InjectActionCard_Patch
    {
        public static void Postfix(InBattleActionCardsManager __instance)
        {
            if (!CardiacDefibrillator.Instance.IsActive)
                return;

            var actionCardInfo = new DefibrillatorActionCardInfo();
            actionCardInfo.SetId("CardiacDefibrillator");

            var card = new CardiacDefibrillatorActionCard(actionCardInfo);
            __instance.InBattleActionCards.Add(card);
        }
    }

    /// <summary>
    /// Cardiac Defibrillator action card - revives a dead teammate at their death location at 50% HP.
    /// Implements IUnitAndTileTargetable to target the death tile.
    /// </summary>
    public class CardiacDefibrillatorActionCard : ActionCard, IUnitAndTileTargetable
    {
        private BattleUnit _selectedUnit;
        private Tile _selectedTile;
        private DeadTeammateInfo _selectedDeadTeammate;

        public Team TeamToAffect => Team.Player;

        public CardiacDefibrillatorActionCard(ActionCardInfo actionCardInfo)
        {
            Info = actionCardInfo;
            _usesLeft = 0; // Dynamically calculated
        }

        public override ActionCard GetCopy()
        {
            return new CardiacDefibrillatorActionCard(Info);
        }

        public new int UsesLeft => DefibrillatorTracker.HasDeadTeammates() ? 1 : 0;

        public void ApplyCommand(BattleUnit unit, Tile tile)
        {
            _selectedUnit = unit;
            _selectedTile = tile;
            ApplyCommand();
        }

        public void ApplyCommand(BattleUnit unit, SpecialTile specialTile)
        {
            ApplyCommand(unit, specialTile.Tile);
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

            if (!CardiacDefibrillator.Instance.IsActive)
            {
                reasons.Add(CommandsAvailabilityChecker.TileAnavailableReasons.AlreadyHasLogic);
                return reasons;
            }

            var matchingDead = DefibrillatorTracker.DeadTeammates
                .FirstOrDefault(t => t.DeathTile == tile);

            if (matchingDead == null)
            {
                reasons.Add(CommandsAvailabilityChecker.TileAnavailableReasons.AlreadyHasLogic);
                return reasons;
            }

            _selectedDeadTeammate = matchingDead;
            return reasons;
        }

        public IEnumerable<CommandsAvailabilityChecker.TileAnavailableReasons> IsTileValid(SpecialTile specialTile, BattleUnit unit)
        {
            return IsTileValid(specialTile.Tile, unit);
        }

        public TypeOfTileForCommand GetTypeOfTile()
        {
            return TypeOfTileForCommand.None;
        }

        public void ApplyCommand()
        {
            if (!CardiacDefibrillator.Instance.IsActive || _selectedUnit == null || _selectedTile == null)
                return;

            _selectedDeadTeammate ??= DefibrillatorTracker.DeadTeammates
                .FirstOrDefault(t => t.DeathTile == _selectedTile);

            if (_selectedDeadTeammate == null)
                return;

            try
            {
                ReviveTeammate(_selectedDeadTeammate);
                DefibrillatorTracker.RemoveDeadTeammate(_selectedDeadTeammate);
                DefibrillatorTracker.MarkUsed();
            }
            catch (Exception e)
            {
                MelonLogger.Error($"CardiacDefibrillator: Failed to revive unit: {e}");
                MelonLogger.Error(e.StackTrace);
            }
        }

        private static void ReviveTeammate(DeadTeammateInfo deadInfo)
        {
            var gameManager = GameManager.Instance;
            if (gameManager == null)
            {
                MelonLogger.Error("CardiacDefibrillator: GameManager not found");
                return;
            }

            var testGame = UnityEngine.Object.FindAnyObjectByType<TestGame>();
            if (testGame == null)
            {
                MelonLogger.Error("CardiacDefibrillator: TestGame not found");
                return;
            }

            var gridManager = AccessTools.Field(typeof(TestGame), "_gridManager").GetValue(testGame) as GridManager;
            if (gridManager == null)
            {
                MelonLogger.Error("CardiacDefibrillator: GridManager not found");
                return;
            }

            var testPawnsPosition = AccessTools.Field(typeof(TestGame), "_test_PawnsPosition").GetValue(testGame) as Test_PawnsPosition;
            if (testPawnsPosition == null)
            {
                MelonLogger.Error("CardiacDefibrillator: Test_PawnsPosition not found");
                return;
            }

            // Set health to 50% of max
            float reviveHealth = deadInfo.MaxHealth * 0.5f;
            deadInfo.UnitData.CurrentHealth = reviveHealth;

            // Create the revived unit on the player team
            var playerManager = gameManager.GetTeamManager(Team.Player);
            var revivedUnit = new BattleUnit(deadInfo.UnitData, Team.Player, gridManager)
            {
                DeploymentOrder = playerManager.BattleUnits.Count() + 1
            };

            // Set current health to 50% via reflection (constructor may use UnitData values)
            var currentHealthField = AccessTools.Field(typeof(BattleUnit), "_currentHealth");
            var currentMaxHealthField = AccessTools.Field(typeof(BattleUnit), "_currentMaxHealth");
            currentHealthField.SetValue(revivedUnit, reviveHealth);
            currentMaxHealthField.SetValue(revivedUnit, deadInfo.MaxHealth);

            revivedUnit.AddCommands();

            playerManager.AddBattleUnit(revivedUnit);
            revivedUnit.PlaceOnTile(deadInfo.DeathTile);

            testPawnsPosition.CreatePawn(revivedUnit);

            revivedUnit.StartCommandsExecution();

            MelonLogger.Msg($"CardiacDefibrillator: Revived {deadInfo.UnitName} at {deadInfo.DeathTile.Coords} with {reviveHealth} HP");
        }
    }

    /// <summary>
    /// Custom ActionCardInfo for CardiacDefibrillator
    /// </summary>
    public class DefibrillatorActionCardInfo : ActionCardInfo
    {
        public string CustomCardName => L("support.cardiac_defibrillator.action_card_name");
        public string CustomCardDescription => L("support.cardiac_defibrillator.action_card_description");

        public DefibrillatorActionCardInfo()
        {
            AccessTools.Field(typeof(ActionCardInfo), "_uses").SetValue(this, 1);
            AccessTools.Field(typeof(ActionCardInfo), "canNotBeReplenished").SetValue(this, true);
        }
    }

    /// <summary>
    /// Patch to intercept CardName getter for DefibrillatorActionCardInfo
    /// </summary>
    [HarmonyPatch(typeof(ActionCardInfo), "CardName", MethodType.Getter)]
    public static class DefibrillatorActionCardInfo_CardName_Patch
    {
        public static bool Prefix(ActionCardInfo __instance, ref string __result)
        {
            if (__instance is DefibrillatorActionCardInfo customInfo)
            {
                __result = customInfo.CustomCardName;
                return false;
            }
            return true;
        }
    }

    /// <summary>
    /// Patch to intercept CardDescription getter for DefibrillatorActionCardInfo
    /// </summary>
    [HarmonyPatch(typeof(ActionCardInfo), "CardDescription", MethodType.Getter)]
    public static class DefibrillatorActionCardInfo_CardDescription_Patch
    {
        public static bool Prefix(ActionCardInfo __instance, ref string __result)
        {
            if (__instance is DefibrillatorActionCardInfo customInfo)
            {
                __result = customInfo.CustomCardDescription;
                return false;
            }
            return true;
        }
    }

    /// <summary>
    /// Custom SpecialTile for dead teammate locations
    /// </summary>
    public class DeadTeammateTile : SpecialTile
    {
        private readonly string _unitName;

        public override string Name => _unitName;

        public override bool ShowNameOnGridForSelector => true;

        public DeadTeammateTile(Tile tile, string unitName) : base(tile)
        {
            _unitName = unitName;
            _isVisible = true;
        }
    }

    /// <summary>
    /// Check card availability - hide if no dead teammates or already used
    /// </summary>
    [HarmonyPatch(typeof(CommandsAvailabilityChecker), "CheckCardsAvailability")]
    public static class CardiacDefibrillator_CheckCardsAvailability_Patch
    {
        public static bool Prefix(ActionCard actionCard, ref IEnumerable<CommandsAvailabilityChecker.CardUnavailableReason> __result)
        {
            if (!CardiacDefibrillator.Instance.IsActive)
                return true;

            if (actionCard is CardiacDefibrillatorActionCard)
            {
                if (!DefibrillatorTracker.HasDeadTeammates())
                {
                    __result = [CommandsAvailabilityChecker.CardUnavailableReason.NoUsesLeft];
                }
                else
                {
                    __result = [];
                }
                return false;
            }
            return true;
        }
    }

    /// <summary>
    /// Inject dead teammate tiles when CardiacDefibrillatorActionCard is picked
    /// </summary>
    [HarmonyPatch(typeof(ChooseTileForCard_BattleManagementDirectory), "SetCardPicked")]
    public static class CardiacDefibrillator_InjectTiles_Patch
    {
        private static readonly AccessTools.FieldRef<ChooseTileForCard_BattleManagementDirectory, List<SpecialTile>> _specialTilesRef =
            AccessTools.FieldRefAccess<ChooseTileForCard_BattleManagementDirectory, List<SpecialTile>>("_specialTiles");

        private static readonly AccessTools.FieldRef<CardPickedInfo, ActionCard> _cardRef =
            AccessTools.FieldRefAccess<CardPickedInfo, ActionCard>("_card");

        public static void Postfix(ChooseTileForCard_BattleManagementDirectory __instance, CardPickedInfo cardPickedInfo, ref bool ____canChooseCurrentUnitPositionTile)
        {
            if (!CardiacDefibrillator.Instance.IsActive)
                return;

            var card = _cardRef(cardPickedInfo);

            if (card is CardiacDefibrillatorActionCard)
            {
                ____canChooseCurrentUnitPositionTile = false;

                var specialTiles = _specialTilesRef(__instance);

                foreach (var deadInfo in DefibrillatorTracker.DeadTeammates)
                {
                    var deadTile = new DeadTeammateTile(deadInfo.DeathTile, deadInfo.UnitName);
                    specialTiles.Add(deadTile);
                }
            }
        }
    }

    /// <summary>
    /// Clear tracker when mission ends
    /// </summary>
    [HarmonyPatch(typeof(TestGame), "EndGame")]
    public static class CardiacDefibrillator_ClearTracking_Patch
    {
        public static void Postfix()
        {
            DefibrillatorTracker.ClearAll();
        }
    }
}
