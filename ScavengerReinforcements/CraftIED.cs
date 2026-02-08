using HarmonyLib;
using MelonLoader;
using SpaceCommander;
using SpaceCommander.ActionCards;
using SpaceCommander.BattleManagement;
using SpaceCommander.BattleManagement.UI;
using SpaceCommander.Commands;
using SpaceCommander.Database;
using SpaceCommander.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using static SpaceCommander.Enumerations;
using static XenopurgeRougeLike.ModLocalization;

namespace XenopurgeRougeLike.ScavengerReinforcements
{
    /// <summary>
    /// 制作IED：获得制作IED指令，可花费1块钱获得地雷和破片手雷各一个
    /// Craft IED: Gain a Craft IED action card that costs 1 coin to get a Mine and a Frag Grenade
    /// 实现细节：作为覆盖指令，显示计时器
    /// Implementation: Works as override command, shows timer
    /// </summary>
    public class CraftIED : Reinforcement
    {
        // Cost in coins
        public const int CoinCost = 1;

        // Setup Mine card ID
        public const string MineCardId = ActionCardIds.SETUP_MINE;

        // Frag Grenade card ID
        public const string FragGrenadeCardId = ActionCardIds.FRAG_GRENADE;

        // Duration of the crafting process (in seconds)
        public const float CraftDuration = 7.4f;

        public CraftIED()
        {
            company = Company.Scavenger;
            rarity = Rarity.Standard;
            name = L("scavenger.craft_ied.name");
            description = L("scavenger.craft_ied.description", CoinCost);
            flavourText = L("scavenger.craft_ied.flavour");
        }

        private static CraftIED _instance;
        public static CraftIED Instance => _instance ??= new();


        public static void AddMine()
        {
            var gainedActionCards = InBattleActionCardsManager.Instance.InBattleActionCards;
            var mineCardSO = Singleton<AssetsDatabase>.Instance.GetActionCardSO(MineCardId);

            if (mineCardSO == null)
            {
                MelonLogger.Error($"CraftIED: Failed to find Mine card with ID {MineCardId}");
                return;
            }

            // Check if the card already exists
            var existingCard = gainedActionCards.FirstOrDefault(c => c.Info.Id == MineCardId);
            if (existingCard != null)
            {
                // Increase uses by 1
                int currentUses = existingCard.UsesLeft;
                AccessTools.Field(typeof(ActionCard), "_usesLeft").SetValue(existingCard, currentUses + 1);
                MelonLogger.Msg($"CraftIED: Increased Mine uses from {currentUses} to {currentUses + 1}");
            }
            else
            {
                // Add new card with 1 use
                var card = mineCardSO.CreateInstance();
                AccessTools.Field(typeof(ActionCard), "_usesLeft").SetValue(card, 1);
                gainedActionCards.Add(card);
                MelonLogger.Msg($"CraftIED: Added new Mine card with 1 use");
            }
        }

        public static void AddFragGrenade()
        {
            var gainedActionCards = InBattleActionCardsManager.Instance.InBattleActionCards;
            var fragGrenadeCardSO = Singleton<AssetsDatabase>.Instance.GetActionCardSO(FragGrenadeCardId);

            if (fragGrenadeCardSO == null)
            {
                MelonLogger.Error($"CraftIED: Failed to find Frag Grenade card with ID {FragGrenadeCardId}");
                return;
            }

            // Check if the card already exists
            var existingCard = gainedActionCards.FirstOrDefault(c => c.Info.Id == FragGrenadeCardId);
            if (existingCard != null)
            {
                // Increase uses by 1
                int currentUses = existingCard.UsesLeft;
                AccessTools.Field(typeof(ActionCard), "_usesLeft").SetValue(existingCard, currentUses + 1);
                MelonLogger.Msg($"CraftIED: Increased Frag Grenade uses from {currentUses} to {currentUses + 1}");
            }
            else
            {
                // Add new card with 1 use
                var card = fragGrenadeCardSO.CreateInstance();
                AccessTools.Field(typeof(ActionCard), "_usesLeft").SetValue(card, 1);
                gainedActionCards.Add(card);
                MelonLogger.Msg($"CraftIED: Added new Frag Grenade card with 1 use");
            }
        }

        public static void UpdateCardsUI()
        {
            CardsButtons_BattleManagementDirectory cardsUI = CardsButtons_BattleManagementDirectory_Instance.Instance;
            if (cardsUI != null && cardsUI.gameObject.activeInHierarchy)
            {
                var flowController = AccessTools.Field(typeof(CardsButtons_BattleManagementDirectory), "_directoriesFlowController")
                    .GetValue(cardsUI) as DirectoriesFlowController;

                if (flowController != null)
                {
                    flowController.ShowDirectory();  // This will call Initialize() and fire OnDirectoryUpdate -> CreateMenu()
                }
                var _directoryId = AccessTools.Field(typeof(CardsButtons_BattleManagementDirectory), "_directoryId").GetValue(cardsUI);
                var directoryData = cardsUI.Initialize();
                // directoryData.DirectoryId = _directoryId ?? Guid.NewGuid();
                if (_directoryId != null)
                {
                    AccessTools.Field(typeof(DirectoryData), "_directoryId").SetValue(directoryData, _directoryId);
                }
                cardsUI.UpdateDirectory(directoryData);
                //BattleManagementWindowController.DirectoryUpdated(directoryData);
                AccessTools.Method(typeof(BattleManagementWindowController), "DirectoryUpdated").Invoke(BattleManagementWindowController_Instance.Instance, [directoryData]);
                MelonLogger.Msg("CraftIED: Updated Cards UI");
            }
        }
    }

    /// <summary>
    /// Patch to inject CraftIEDActionCard into InBattleActionCardsManager after initialization
    /// </summary>
    [HarmonyPatch(typeof(InBattleActionCardsManager), "Initialize")]
    public static class CraftIED_InjectActionCard_Patch
    {
        public static void Postfix(InBattleActionCardsManager __instance)
        {
            if (!CraftIED.Instance.IsActive)
                return;

            var actionCardInfo = new CraftIEDActionCardInfo();
            actionCardInfo.SetId("CraftIED");

            var craftIEDCard = new CraftIEDOuterCard(actionCardInfo, UnityEngine.ScriptableObject.CreateInstance<CraftIEDDelayCommandDataSO>());

            __instance.InBattleActionCards.Add(craftIEDCard);
        }
    }

    /// <summary>
    /// Custom ActionCardInfo for CraftIED
    /// </summary>
    public class CraftIEDActionCardInfo : ActionCardInfo
    {
        public string CustomCardName => $"{L("scavenger.craft_ied.card_name")} {CraftIED.CoinCost} <sprite name=\"CoinIcon\">";

        public string CustomCardDescription => L("scavenger.craft_ied.card_description", CraftIED.CoinCost);

        public CraftIEDActionCardInfo()
        {
            AccessTools.Field(typeof(ActionCardInfo), "_uses").SetValue(this, 0); // 0 = unlimited uses
            AccessTools.Field(typeof(ActionCardInfo), "canNotBeReplenished").SetValue(this, false);
        }
    }

    /// <summary>
    /// Patch to intercept CardName getter for CraftIEDActionCardInfo
    /// </summary>
    [HarmonyPatch(typeof(ActionCardInfo), "CardName", MethodType.Getter)]
    public static class CraftIEDActionCardInfo_CardName_Patch
    {
        public static bool Prefix(ActionCardInfo __instance, ref string __result)
        {
            if (__instance is CraftIEDActionCardInfo customInfo)
            {
                __result = customInfo.CustomCardName;
                return false;
            }
            return true;
        }
    }

    /// <summary>
    /// Patch to intercept CardDescription getter for CraftIEDActionCardInfo
    /// </summary>
    [HarmonyPatch(typeof(ActionCardInfo), "CardDescription", MethodType.Getter)]
    public static class CraftIEDActionCardInfo_CardDescription_Patch
    {
        public static bool Prefix(ActionCardInfo __instance, ref string __result)
        {
            if (__instance is CraftIEDActionCardInfo customInfo)
            {
                __result = customInfo.CustomCardDescription;
                return false;
            }
            return true;
        }
    }

    /// <summary>
    /// The outer action card that appears in the card list
    /// Uses OverrideCommands_UnitAsTarget_Card pattern to create DelayActionCardCommand
    /// </summary>
    public class CraftIEDOuterCard : ActionCard, IUnitTargetable
    {
        private readonly DelayCardCommandDataSO _commandDataSO;

        public Team TeamToAffect => Team.Player;

        public CraftIEDOuterCard(ActionCardInfo actionCardInfo, DelayCardCommandDataSO commandDataSO)
        {
            Info = actionCardInfo;
            _commandDataSO = commandDataSO;
            // Set to 0 for unlimited uses (but will be disabled if not enough coins)
            _usesLeft = 0;
        }

        public override ActionCard GetCopy()
        {
            return new CraftIEDOuterCard(Info, _commandDataSO);
        }

        public void ApplyCommand(BattleUnit unit)
        {
            if (!CraftIED.Instance.IsActive)
                return;

            if (unit == null || !unit.IsAlive || unit.Team != Team.Player)
                return;

            // Deduct coins immediately when starting the craft
            var playerWallet = Singleton<Player>.Instance.PlayerData.PlayerWallet;
            if (!playerWallet.HasEnoughMoney(CraftIED.CoinCost))
                return;

            playerWallet.ChangeCoinsByValue(-CraftIED.CoinCost);
            ScavengerSpendingTracker.AddSpending(CraftIED.CoinCost);
            MelonLogger.Msg($"CraftIED: Deducted {CraftIED.CoinCost} coin(s). Remaining: {playerWallet.Coins}");

            // Create DelayActionCardCommand using the game's built-in system
            DelayActionCardCommand delayCommand = new DelayActionCardCommand(unit);
            delayCommand.InitializeValues(_commandDataSO);

            // Override current command with craft command
            ActionCard.CostOfActionCard costOfActionCard = default;
            costOfActionCard.ActionCardId = Info.Id;
            unit.CommandsManager.OverrideCurrentCommandFromActionCard(delayCommand, costOfActionCard);
        }

        IEnumerable<CommandsAvailabilityChecker.UnitAnavailableReasons> IUnitTargetable.IsUnitValid(BattleUnit unit)
        {
            var reasons = new List<CommandsAvailabilityChecker.UnitAnavailableReasons>();

            if (!CraftIED.Instance.IsActive)
            {
                return reasons;
            }

            // Can only target alive units
            if (!unit.IsAlive)
            {
                reasons.Add(CommandsAvailabilityChecker.UnitAnavailableReasons.UnitIsDead);
            }
            // Can't target units in close combat
            else if (unit.CommandsManager.IsEngagedInCloseCombat)
            {
                reasons.Add(CommandsAvailabilityChecker.UnitAnavailableReasons.UnitIsEngagedInCloseCombat);
            }
            // Can't target units already crafting IEDs
            else if (unit.CommandsManager.CurrentCommand is DelayActionCardCommand delayCmd)
            {
                var cmdData = delayCmd.CommandData as DelayCardCommandDataSO;
                if (cmdData is CraftIEDDelayCommandDataSO)
                {
                    reasons.Add(CommandsAvailabilityChecker.UnitAnavailableReasons.AlreadyHasEffect);
                }
            }
            // Check if player has enough coins
            else
            {
                var playerWallet = Singleton<Player>.Instance.PlayerData.PlayerWallet;
                if (!playerWallet.HasEnoughMoney(CraftIED.CoinCost))
                {
                    reasons.Add(CommandsAvailabilityChecker.UnitAnavailableReasons.InsufficientUnits);
                }
            }

            return reasons;
        }
    }

    /// <summary>
    /// DelayCardCommandDataSO that references the inner craft action card
    /// This is used by DelayActionCardCommand to know what to execute after the delay
    /// </summary>
    public class CraftIEDDelayCommandDataSO : DelayCardCommandDataSO
    {
        private static CraftIEDInnerActionCardSO _actionCardSO;

        public string CustomCommandName => L("scavenger.craft_ied.command_name");
        public string CustomCommandDescription => L("scavenger.craft_ied.command_description");

        public CraftIEDDelayCommandDataSO()
        {
            if (_actionCardSO == null)
            {
                _actionCardSO = UnityEngine.ScriptableObject.CreateInstance<CraftIEDInnerActionCardSO>();
            }

            // Set the action card using reflection
            AccessTools.Field(typeof(DelayCardCommandDataSO), "_actionCard").SetValue(this, _actionCardSO);

            // Set command properties using reflection
            AccessTools.Field(typeof(CommandDataSO), "_id").SetValue(this, Guid.NewGuid().ToString());
            AccessTools.Field(typeof(CommandDataSO), "_commandDuration").SetValue(this, CraftIED.CraftDuration);
            AccessTools.Field(typeof(CommandDataSO), "_marineState").SetValue(this, MarineState.Neutral);
            AccessTools.Field(typeof(CommandDataSO), "_commandCategory").SetValue(this, CommandCategories.Move);
            AccessTools.Field(typeof(CommandDataSO), "_showTimer").SetValue(this, true);
            AccessTools.Field(typeof(CommandDataSO), "_isOverrideCommand").SetValue(this, true);
        }
    }

    /// <summary>
    /// Patch to intercept CommandName getter for CraftIEDDelayCommandDataSO
    /// </summary>
    [HarmonyPatch(typeof(CommandDataSO), "CommandName", MethodType.Getter)]
    public static class CraftIEDDelayCommandDataSO_CommandName_Patch
    {
        public static bool Prefix(CommandDataSO __instance, ref string __result)
        {
            if (__instance is CraftIEDDelayCommandDataSO customData)
            {
                __result = customData.CustomCommandName;
                return false;
            }
            return true;
        }
    }

    /// <summary>
    /// Patch to intercept CommandDescription getter for CraftIEDDelayCommandDataSO
    /// </summary>
    [HarmonyPatch(typeof(CommandDataSO), "CommandDescription", MethodType.Getter)]
    public static class CraftIEDDelayCommandDataSO_CommandDescription_Patch
    {
        public static bool Prefix(CommandDataSO __instance, ref string __result)
        {
            if (__instance is CraftIEDDelayCommandDataSO customData)
            {
                __result = customData.CustomCommandDescription;
                return false;
            }
            return true;
        }
    }

    /// <summary>
    /// Inner ActionCardSO that represents what happens after the delay
    /// This is the "effect" that DelayActionCardCommand will trigger
    /// </summary>
    public class CraftIEDInnerActionCardSO : ActionCardSO
    {
        public CraftIEDInnerActionCardSO()
        {
            // Initialize the _actionCardInfo field using reflection
            var info = new CraftIEDInnerActionCardInfo();
            info.SetId("CraftIEDInner");
            AccessTools.Field(typeof(ActionCardSO), "_actionCardInfo").SetValue(this, info);
        }

        public override ActionCard CreateInstance()
        {
            return new CraftIEDInnerActionCard(Info);
        }
    }

    /// <summary>
    /// Inner action card info (not shown to user, just for internal use)
    /// </summary>
    public class CraftIEDInnerActionCardInfo : ActionCardInfo
    {
        public CraftIEDInnerActionCardInfo()
        {
            AccessTools.Field(typeof(ActionCardInfo), "_uses").SetValue(this, 0);
            AccessTools.Field(typeof(ActionCardInfo), "canNotBeReplenished").SetValue(this, false);
        }
    }

    /// <summary>
    /// Inner action card that applies the craft effect
    /// This is triggered by DelayActionCardCommand after the timer
    /// </summary>
    public class CraftIEDInnerActionCard : ActionCard, IUnitTargetable
    {
        public Team TeamToAffect => Team.Player;

        public CraftIEDInnerActionCard(ActionCardInfo info)
        {
            Info = info;
            _usesLeft = 0;
        }

        public override ActionCard GetCopy()
        {
            return new CraftIEDInnerActionCard(Info);
        }

        public void ApplyCommand(BattleUnit unit)
        {
            // This is called by DelayActionCardCommand when timer completes
            if (!CraftIED.Instance.IsActive)
                return;

            // Add Mine
            CraftIED.AddMine();

            // Add Frag Grenade
            CraftIED.AddFragGrenade();

            // Update the UI
            CraftIED.UpdateCardsUI();
        }

        IEnumerable<CommandsAvailabilityChecker.UnitAnavailableReasons> IUnitTargetable.IsUnitValid(BattleUnit unit)
        {
            // Always valid when called from DelayActionCardCommand
            return new List<CommandsAvailabilityChecker.UnitAnavailableReasons>();
        }
    }

    [HarmonyPatch(typeof(BattleManagementWindowController), "InitializeBattleManagementController")]
    public class BattleManagementWindowController_Instance
    {
        public static BattleManagementWindowController Instance;
        public static void Prefix(BattleManagementWindowController __instance)
        {
            Instance = __instance;
        }
    }

    [HarmonyPatch(typeof(CardsButtons_BattleManagementDirectory), "Initialize")]
    public class CardsButtons_BattleManagementDirectory_Instance
    {
        public static CardsButtons_BattleManagementDirectory Instance;
        public static void Prefix(CardsButtons_BattleManagementDirectory __instance)
        {
            Instance = __instance;
        }
    }
}
