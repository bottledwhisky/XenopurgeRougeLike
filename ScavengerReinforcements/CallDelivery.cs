using HarmonyLib;
using MelonLoader;
using SpaceCommander;
using SpaceCommander.ActionCards;
using SpaceCommander.Commands;
using SpaceCommander.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using static SpaceCommander.Enumerations;
using static XenopurgeRougeLike.ModLocalization;

namespace XenopurgeRougeLike.ScavengerReinforcements
{
    /// <summary>
    /// 呼叫快递：获得呼叫快递指令，可花费1块钱获得治疗针和随机药剂各一个
    /// Call Delivery: Gain a Call Delivery action card that costs 1 coin to get a Health Stim and a random injection
    /// </summary>
    public class CallDelivery : Reinforcement
    {
        // Cost in coins
        public const int CoinCost = 1;

        // Health Stim card ID
        public const string HealthStimCardId = "6569d382-07ef-4db5-86ac-bac9eb249889";

        // Injection card IDs (for random selection)
        private static readonly string[] InjectionCardIds = new[]
        {
            "86cafd8b-9e28-4fd1-9e44-4ccdabb00137", // Inject Brutadyne
            "82a8cd80-af72-4785-b4c9-eab1a498a125", // Inject Kinetra
            "b51454f9-5641-4b07-94bf-93312555e860"  // Inject Optivex
        };

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

        /// <summary>
        /// Try to use the delivery service - deduct coins and add cards
        /// </summary>
        public static bool TryUseDelivery()
        {
            var playerWallet = Singleton<Player>.Instance.PlayerData.PlayerWallet;

            if (!playerWallet.HasEnoughMoney(CoinCost))
            {
                MelonLogger.Msg($"CallDelivery: Not enough coins. Need {CoinCost}, have {playerWallet.Coins}");
                return false;
            }

            // Deduct the cost
            playerWallet.ChangeCoinsByValue(-CoinCost);
            MelonLogger.Msg($"CallDelivery: Deducted {CoinCost} coin(s). Remaining: {playerWallet.Coins}");

            // Add Health Stim
            AddHealthStim();

            // Add random injection
            AddRandomInjection();

            return true;
        }

        private static void AddHealthStim()
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

        private static void AddRandomInjection()
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

            var callDeliveryCard = new CallDeliveryActionCard(actionCardInfo);

            __instance.InBattleActionCards.Add(callDeliveryCard);
        }
    }

    /// <summary>
    /// Custom ActionCardInfo for CallDelivery
    /// </summary>
    public class CallDeliveryActionCardInfo : ActionCardInfo
    {
        public string CustomCardName => L("scavenger.call_delivery.card_name");

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
    /// The action card that uses coins to deliver items
    /// </summary>
    public class CallDeliveryActionCard : ActionCard, INoTargetable
    {
        public CallDeliveryActionCard(ActionCardInfo actionCardInfo)
        {
            Info = actionCardInfo;
            // Set to 0 for unlimited uses (but will be disabled if not enough coins)
            _usesLeft = 0;
        }

        public override ActionCard GetCopy()
        {
            return new CallDeliveryActionCard(Info);
        }

        public void ApplyCommand()
        {
            if (!CallDelivery.Instance.IsActive)
                return;

            CallDelivery.TryUseDelivery();
        }

        IEnumerable<CommandsAvailabilityChecker.CardUnavailableReason> INoTargetable.IsCardValid()
        {
            var reasons = new List<CommandsAvailabilityChecker.CardUnavailableReason>();

            if (!CallDelivery.Instance.IsActive)
            {
                return reasons;
            }

            // Check if player has enough coins
            // Note: Using NotEnoughtAccessPoints as a generic "not enough resources" indicator
            // since there's no specific enum value for coins
            var playerWallet = Singleton<Player>.Instance.PlayerData.PlayerWallet;
            if (!playerWallet.HasEnoughMoney(CallDelivery.CoinCost))
            {
                reasons.Add(CommandsAvailabilityChecker.CardUnavailableReason.NotEnoughtAccessPoints);
            }

            return reasons;
        }
    }
}
