using HarmonyLib;
using MelonLoader;
using SpaceCommander;
using SpaceCommander.ActionCards;
using SpaceCommander.Commands;
using SpaceCommander.Database;
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
    /// </summary>
    public class CraftIED : Reinforcement
    {
        // Cost in coins
        public const int CoinCost = 1;

        // Setup Mine card ID
        public const string MineCardId = "8daa3d58-73aa-4c26-a20f-954686777d1f";

        // Frag Grenade card ID
        public const string FragGrenadeCardId = "bfb700d8-5fa2-4bd0-b1dd-94842f66c031";

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

        /// <summary>
        /// Try to craft IED - deduct coins and add cards
        /// </summary>
        public static bool TryCraftIED()
        {
            var playerWallet = Singleton<Player>.Instance.PlayerData.PlayerWallet;

            if (!playerWallet.HasEnoughMoney(CoinCost))
            {
                MelonLogger.Msg($"CraftIED: Not enough coins. Need {CoinCost}, have {playerWallet.Coins}");
                return false;
            }

            // Deduct the cost
            playerWallet.ChangeCoinsByValue(-CoinCost);
            MelonLogger.Msg($"CraftIED: Deducted {CoinCost} coin(s). Remaining: {playerWallet.Coins}");

            // Add Mine
            AddMine();

            // Add Frag Grenade
            AddFragGrenade();

            return true;
        }

        private static void AddMine()
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

        private static void AddFragGrenade()
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

            var craftIEDCard = new CraftIEDActionCard(actionCardInfo);

            __instance.InBattleActionCards.Add(craftIEDCard);
        }
    }

    /// <summary>
    /// Custom ActionCardInfo for CraftIED
    /// </summary>
    public class CraftIEDActionCardInfo : ActionCardInfo
    {
        public string CustomCardName => L("scavenger.craft_ied.card_name");

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
    /// The action card that uses coins to craft IED (mine + frag grenade)
    /// </summary>
    public class CraftIEDActionCard : ActionCard, INoTargetable
    {
        public CraftIEDActionCard(ActionCardInfo actionCardInfo)
        {
            Info = actionCardInfo;
            // Set to 0 for unlimited uses (but will be disabled if not enough coins)
            _usesLeft = 0;
        }

        public override ActionCard GetCopy()
        {
            return new CraftIEDActionCard(Info);
        }

        public void ApplyCommand()
        {
            if (!CraftIED.Instance.IsActive)
                return;

            CraftIED.TryCraftIED();
        }

        IEnumerable<CommandsAvailabilityChecker.CardUnavailableReason> INoTargetable.IsCardValid()
        {
            var reasons = new List<CommandsAvailabilityChecker.CardUnavailableReason>();

            if (!CraftIED.Instance.IsActive)
            {
                return reasons;
            }

            // Check if player has enough coins
            // Note: Using NotEnoughtAccessPoints as a generic "not enough resources" indicator
            // since there's no specific enum value for coins
            var playerWallet = Singleton<Player>.Instance.PlayerData.PlayerWallet;
            if (!playerWallet.HasEnoughMoney(CraftIED.CoinCost))
            {
                reasons.Add(CommandsAvailabilityChecker.CardUnavailableReason.NotEnoughtAccessPoints);
            }

            return reasons;
        }
    }
}
