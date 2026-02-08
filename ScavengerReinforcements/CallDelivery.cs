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
    /// 呼叫快递：获得呼叫快递指令，可花费1块钱获得治疗针和随机药剂各一个
    /// Call Delivery: Gain a Call Delivery action card that costs 1 coin to get a Health Stim and a random injection
    /// 实现细节：作为覆盖指令，显示计时器
    /// Implementation: Works as override command, shows timer
    /// </summary>
    public class CallDelivery : Reinforcement
    {
        // Cost in coins
        public const int CoinCost = 1;

        // Health Stim card ID
        public const string HealthStimCardId = ActionCardIds.INJECT_HEALTH_STIM;

        // Injection card IDs (for random selection)
        private static readonly string[] InjectionCardIds = new[]
        {
            ActionCardIds.INJECT_BRUTADYNE,
            ActionCardIds.INJECT_KINETRA,
            ActionCardIds.INJECT_OPTIVEX
        };

        // Duration of the delivery process (in seconds)
        public const float DeliveryDuration = 7.4f;

        public CallDelivery()
        {
            company = Company.Scavenger;
            rarity = Rarity.Standard;
            name = L("scavenger.call_delivery.name");
            description = L("scavenger.call_delivery.description", CoinCost);
            flavourText = L("scavenger.call_delivery.flavour");
        }

        private static CallDelivery _instance;
        public static CallDelivery Instance => _instance ??= new();

        public static void AddHealthStim()
        {
            var gainedActionCards = InBattleActionCardsManager.Instance.InBattleActionCards;
            var healthStimCardSO = Singleton<AssetsDatabase>.Instance.GetActionCardSO(HealthStimCardId);

            if (healthStimCardSO == null)
            {
                MelonLogger.Error($"CallDelivery: Failed to find Health Stim card with ID {HealthStimCardId}");
                return;
            }

            // Check if the card already exists
            var existingCard = gainedActionCards.FirstOrDefault(c => c.Info.Id == HealthStimCardId);
            if (existingCard != null)
            {
                // Increase uses by 1
                int currentUses = existingCard.UsesLeft;
                AccessTools.Field(typeof(ActionCard), "_usesLeft").SetValue(existingCard, currentUses + 1);
                MelonLogger.Msg($"CallDelivery: Increased Health Stim uses from {currentUses} to {currentUses + 1}");
            }
            else
            {
                // Add new card with 1 use
                var card = healthStimCardSO.CreateInstance();
                AccessTools.Field(typeof(ActionCard), "_usesLeft").SetValue(card, 1);
                gainedActionCards.Add(card);
                MelonLogger.Msg($"CallDelivery: Added new Health Stim card with 1 use");
            }
        }

        public static void AddRandomInjection()
        {
            var gainedActionCards = InBattleActionCardsManager.Instance.InBattleActionCards;

            // Randomly select an injection
            var random = new System.Random();
            string selectedInjectionId = InjectionCardIds[random.Next(InjectionCardIds.Length)];

            var injectionCardSO = Singleton<AssetsDatabase>.Instance.GetActionCardSO(selectedInjectionId);
            if (injectionCardSO == null)
            {
                MelonLogger.Error($"CallDelivery: Failed to find injection card with ID {selectedInjectionId}");
                return;
            }

            // Check if the card already exists
            var existingCard = gainedActionCards.FirstOrDefault(c => c.Info.Id == selectedInjectionId);
            if (existingCard != null)
            {
                // Increase uses by 1
                int currentUses = existingCard.UsesLeft;
                AccessTools.Field(typeof(ActionCard), "_usesLeft").SetValue(existingCard, currentUses + 1);
                MelonLogger.Msg($"CallDelivery: Increased {existingCard.Info.CardName} uses from {currentUses} to {currentUses + 1}");
            }
            else
            {
                // Add new card with 1 use
                var card = injectionCardSO.CreateInstance();
                AccessTools.Field(typeof(ActionCard), "_usesLeft").SetValue(card, 1);
                gainedActionCards.Add(card);
                MelonLogger.Msg($"CallDelivery: Added new {card.Info.CardName} card with 1 use");
            }
        }

        public static void UpdateCardsUI()
        {
            CardsButtons_BattleManagementDirectory cardsUI = CallDelivery_CardsButtons_Instance.Instance;
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
                AccessTools.Method(typeof(BattleManagementWindowController), "DirectoryUpdated").Invoke(CallDelivery_BattleManagementWindowController_Instance.Instance, [directoryData]);
                MelonLogger.Msg("CallDelivery: Updated Cards UI");
            }
        }
    }

    /// <summary>
    /// Patch to inject CallDeliveryActionCard into InBattleActionCardsManager after initialization
    /// </summary>
    [HarmonyPatch(typeof(InBattleActionCardsManager), "Initialize")]
    public static class CallDelivery_InjectActionCard_Patch
    {
        public static void Postfix(InBattleActionCardsManager __instance)
        {
            if (!CallDelivery.Instance.IsActive)
                return;

            var actionCardInfo = new CallDeliveryActionCardInfo();
            actionCardInfo.SetId("CallDelivery");

            var callDeliveryCard = new CallDeliveryOuterCard(actionCardInfo, UnityEngine.ScriptableObject.CreateInstance<CallDeliveryDelayCommandDataSO>());

            __instance.InBattleActionCards.Add(callDeliveryCard);
        }
    }

    /// <summary>
    /// Custom ActionCardInfo for CallDelivery
    /// </summary>
    public class CallDeliveryActionCardInfo : ActionCardInfo
    {
        public string CustomCardName => L("scavenger.call_delivery.card_name", CallDelivery.CoinCost);

        public string CustomCardDescription => L("scavenger.call_delivery.card_description", CallDelivery.CoinCost);

        public CallDeliveryActionCardInfo()
        {
            AccessTools.Field(typeof(ActionCardInfo), "_uses").SetValue(this, 0); // 0 = unlimited uses
            AccessTools.Field(typeof(ActionCardInfo), "canNotBeReplenished").SetValue(this, false);
        }
    }

    /// <summary>
    /// Patch to intercept CardName getter for CallDeliveryActionCardInfo
    /// </summary>
    [HarmonyPatch(typeof(ActionCardInfo), "CardName", MethodType.Getter)]
    public static class CallDeliveryActionCardInfo_CardName_Patch
    {
        public static bool Prefix(ActionCardInfo __instance, ref string __result)
        {
            if (__instance is CallDeliveryActionCardInfo customInfo)
            {
                __result = customInfo.CustomCardName;
                return false;
            }
            return true;
        }
    }

    /// <summary>
    /// Patch to intercept CardDescription getter for CallDeliveryActionCardInfo
    /// </summary>
    [HarmonyPatch(typeof(ActionCardInfo), "CardDescription", MethodType.Getter)]
    public static class CallDeliveryActionCardInfo_CardDescription_Patch
    {
        public static bool Prefix(ActionCardInfo __instance, ref string __result)
        {
            if (__instance is CallDeliveryActionCardInfo customInfo)
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
    public class CallDeliveryOuterCard : ActionCard, IUnitTargetable
    {
        private readonly DelayCardCommandDataSO _commandDataSO;

        public Team TeamToAffect => Team.Player;

        public CallDeliveryOuterCard(ActionCardInfo actionCardInfo, DelayCardCommandDataSO commandDataSO)
        {
            Info = actionCardInfo;
            _commandDataSO = commandDataSO;
            // Set to 0 for unlimited uses (but will be disabled if not enough coins)
            _usesLeft = 0;
        }

        public override ActionCard GetCopy()
        {
            return new CallDeliveryOuterCard(Info, _commandDataSO);
        }

        public void ApplyCommand(BattleUnit unit)
        {
            if (!CallDelivery.Instance.IsActive)
                return;

            if (unit == null || !unit.IsAlive || unit.Team != Team.Player)
                return;

            // Deduct coins immediately when starting the delivery
            var playerWallet = Singleton<Player>.Instance.PlayerData.PlayerWallet;
            if (!playerWallet.HasEnoughMoney(CallDelivery.CoinCost))
                return;

            playerWallet.ChangeCoinsByValue(-CallDelivery.CoinCost);
            ScavengerSpendingTracker.AddSpending(CallDelivery.CoinCost);
            MelonLogger.Msg($"CallDelivery: Deducted {CallDelivery.CoinCost} coin(s). Remaining: {playerWallet.Coins}");

            // Create DelayActionCardCommand using the game's built-in system
            DelayActionCardCommand delayCommand = new DelayActionCardCommand(unit);
            delayCommand.InitializeValues(_commandDataSO);

            // Override current command with delivery command
            ActionCard.CostOfActionCard costOfActionCard = default;
            costOfActionCard.ActionCardId = Info.Id;
            unit.CommandsManager.OverrideCurrentCommandFromActionCard(delayCommand, costOfActionCard);
        }

        IEnumerable<CommandsAvailabilityChecker.UnitAnavailableReasons> IUnitTargetable.IsUnitValid(BattleUnit unit)
        {
            var reasons = new List<CommandsAvailabilityChecker.UnitAnavailableReasons>();

            if (!CallDelivery.Instance.IsActive)
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
            // Can't target units already calling delivery
            else if (unit.CommandsManager.CurrentCommand is DelayActionCardCommand delayCmd)
            {
                var cmdData = delayCmd.CommandData as DelayCardCommandDataSO;
                if (cmdData is CallDeliveryDelayCommandDataSO)
                {
                    reasons.Add(CommandsAvailabilityChecker.UnitAnavailableReasons.AlreadyHasEffect);
                }
            }
            // Check if player has enough coins
            else
            {
                var playerWallet = Singleton<Player>.Instance.PlayerData.PlayerWallet;
                if (!playerWallet.HasEnoughMoney(CallDelivery.CoinCost))
                {
                    reasons.Add(CommandsAvailabilityChecker.UnitAnavailableReasons.InsufficientUnits);
                }
            }

            return reasons;
        }
    }

    /// <summary>
    /// DelayCardCommandDataSO that references the inner delivery action card
    /// This is used by DelayActionCardCommand to know what to execute after the delay
    /// </summary>
    public class CallDeliveryDelayCommandDataSO : DelayCardCommandDataSO
    {
        private static CallDeliveryInnerActionCardSO _actionCardSO;

        public string CustomCommandName => L("scavenger.call_delivery.command_name");
        public string CustomCommandDescription => L("scavenger.call_delivery.command_description");

        public CallDeliveryDelayCommandDataSO()
        {
            if (_actionCardSO == null)
            {
                _actionCardSO = UnityEngine.ScriptableObject.CreateInstance<CallDeliveryInnerActionCardSO>();
            }

            // Set the action card using reflection
            AccessTools.Field(typeof(DelayCardCommandDataSO), "_actionCard").SetValue(this, _actionCardSO);

            // Set command properties using reflection
            AccessTools.Field(typeof(CommandDataSO), "_id").SetValue(this, Guid.NewGuid().ToString());
            AccessTools.Field(typeof(CommandDataSO), "_commandDuration").SetValue(this, CallDelivery.DeliveryDuration);
            AccessTools.Field(typeof(CommandDataSO), "_marineState").SetValue(this, MarineState.Neutral);
            AccessTools.Field(typeof(CommandDataSO), "_commandCategory").SetValue(this, CommandCategories.Move);
            AccessTools.Field(typeof(CommandDataSO), "_showTimer").SetValue(this, true);
            AccessTools.Field(typeof(CommandDataSO), "_isOverrideCommand").SetValue(this, true);
        }
    }

    /// <summary>
    /// Patch to intercept CommandName getter for CallDeliveryDelayCommandDataSO
    /// </summary>
    [HarmonyPatch(typeof(CommandDataSO), "CommandName", MethodType.Getter)]
    public static class CallDeliveryDelayCommandDataSO_CommandName_Patch
    {
        public static bool Prefix(CommandDataSO __instance, ref string __result)
        {
            if (__instance is CallDeliveryDelayCommandDataSO customData)
            {
                __result = customData.CustomCommandName;
                return false;
            }
            return true;
        }
    }

    /// <summary>
    /// Patch to intercept CommandDescription getter for CallDeliveryDelayCommandDataSO
    /// </summary>
    [HarmonyPatch(typeof(CommandDataSO), "CommandDescription", MethodType.Getter)]
    public static class CallDeliveryDelayCommandDataSO_CommandDescription_Patch
    {
        public static bool Prefix(CommandDataSO __instance, ref string __result)
        {
            if (__instance is CallDeliveryDelayCommandDataSO customData)
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
    public class CallDeliveryInnerActionCardSO : ActionCardSO
    {
        public CallDeliveryInnerActionCardSO()
        {
            // Initialize the _actionCardInfo field using reflection
            var info = new CallDeliveryInnerActionCardInfo();
            info.SetId("CallDeliveryInner");
            AccessTools.Field(typeof(ActionCardSO), "_actionCardInfo").SetValue(this, info);
        }

        public override ActionCard CreateInstance()
        {
            return new CallDeliveryInnerActionCard(Info);
        }
    }

    /// <summary>
    /// Inner action card info (not shown to user, just for internal use)
    /// </summary>
    public class CallDeliveryInnerActionCardInfo : ActionCardInfo
    {
        public CallDeliveryInnerActionCardInfo()
        {
            AccessTools.Field(typeof(ActionCardInfo), "_uses").SetValue(this, 0);
            AccessTools.Field(typeof(ActionCardInfo), "canNotBeReplenished").SetValue(this, false);
        }
    }

    /// <summary>
    /// Inner action card that applies the delivery effect
    /// This is triggered by DelayActionCardCommand after the timer
    /// </summary>
    public class CallDeliveryInnerActionCard : ActionCard, IUnitTargetable
    {
        public Team TeamToAffect => Team.Player;

        public CallDeliveryInnerActionCard(ActionCardInfo info)
        {
            Info = info;
            _usesLeft = 0;
        }

        public override ActionCard GetCopy()
        {
            return new CallDeliveryInnerActionCard(Info);
        }

        public void ApplyCommand(BattleUnit unit)
        {
            // This is called by DelayActionCardCommand when timer completes
            if (!CallDelivery.Instance.IsActive)
                return;

            // Add Health Stim
            CallDelivery.AddHealthStim();

            // Add random injection
            CallDelivery.AddRandomInjection();

            // Update the UI
            CallDelivery.UpdateCardsUI();
        }

        IEnumerable<CommandsAvailabilityChecker.UnitAnavailableReasons> IUnitTargetable.IsUnitValid(BattleUnit unit)
        {
            // Always valid when called from DelayActionCardCommand
            return new List<CommandsAvailabilityChecker.UnitAnavailableReasons>();
        }
    }

    [HarmonyPatch(typeof(BattleManagementWindowController), "InitializeBattleManagementController")]
    public class CallDelivery_BattleManagementWindowController_Instance
    {
        public static BattleManagementWindowController Instance;
        public static void Prefix(BattleManagementWindowController __instance)
        {
            Instance = __instance;
        }
    }

    [HarmonyPatch(typeof(CardsButtons_BattleManagementDirectory), "Initialize")]
    public class CallDelivery_CardsButtons_Instance
    {
        public static CardsButtons_BattleManagementDirectory Instance;
        public static void Prefix(CardsButtons_BattleManagementDirectory __instance)
        {
            Instance = __instance;
        }
    }
}
