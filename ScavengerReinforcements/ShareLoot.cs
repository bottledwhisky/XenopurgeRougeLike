using HarmonyLib;
using MelonLoader;
using SpaceCommander;
using SpaceCommander.ActionCards;
using SpaceCommander.Commands;
using System.Collections.Generic;
using System.Linq;
using static SpaceCommander.Enumerations;
using static XenopurgeRougeLike.ModLocalization;

namespace XenopurgeRougeLike.ScavengerReinforcements
{
    /// <summary>
    /// 分赃：获得分赃指令，可花费1块钱使一个队友获得+20瞄准，+2近战伤害，+2速度，持续到任务结束，该效果可叠加
    /// Share Loot: Gain a Share Loot action card that costs 1 coin to grant a soldier +20 Accuracy, +2 Power, +2 Speed until mission end. Effect is stackable.
    /// </summary>
    public class ShareLoot : Reinforcement
    {
        // Cost in coins
        public const int CoinCost = 1;

        // Stat bonuses per use
        public const float AccuracyBonus = 0.2f; // +20 Accuracy (displayed as x100)
        public const float PowerBonus = 2f;      // +2 Power (melee damage)
        public const float SpeedBonus = 2f;      // +2 Speed

        public ShareLoot()
        {
            company = Company.Scavenger;
            rarity = Rarity.Elite;
            name = L("scavenger.share_loot.name");
            description = L("scavenger.share_loot.description", CoinCost, (int)(AccuracyBonus * 100), (int)PowerBonus, (int)SpeedBonus);
            flavourText = L("scavenger.share_loot.flavour");
        }

        private static ShareLoot _instance;
        public static ShareLoot Instance => _instance ??= new();

        // Track how many times each unit has been buffed
        public static Dictionary<BattleUnit, int> UnitBuffStacks = new();
    }

    // Patch to clear state when mission starts
    [HarmonyPatch(typeof(TestGame), "StartGame")]
    public class ShareLoot_TestGame_StartGame_Patch
    {
        public static void Postfix()
        {
            ShareLoot.UnitBuffStacks.Clear();
        }
    }

    // Patch to clear state when mission ends
    [HarmonyPatch(typeof(TestGame), "EndGame")]
    public class ShareLoot_TestGame_EndGame_Patch
    {
        public static void Postfix()
        {
            ShareLoot.UnitBuffStacks.Clear();
        }
    }

    /// <summary>
    /// Patch to inject ShareLootActionCard into InBattleActionCardsManager after initialization
    /// </summary>
    [HarmonyPatch(typeof(InBattleActionCardsManager), "Initialize")]
    public static class ShareLoot_InjectActionCard_Patch
    {
        public static void Postfix(InBattleActionCardsManager __instance)
        {
            if (!ShareLoot.Instance.IsActive)
                return;

            var actionCardInfo = new ShareLootActionCardInfo();
            actionCardInfo.SetId("ShareLoot");

            var shareLootCard = new ShareLootActionCard(actionCardInfo);

            __instance.InBattleActionCards.Add(shareLootCard);
        }
    }

    /// <summary>
    /// Custom ActionCardInfo for ShareLoot
    /// </summary>
    public class ShareLootActionCardInfo : ActionCardInfo
    {
        public string CustomCardName => L("scavenger.share_loot.card_name");

        public string CustomCardDescription => L("scavenger.share_loot.card_description", ShareLoot.CoinCost, (int)(ShareLoot.AccuracyBonus * 100), (int)ShareLoot.PowerBonus, (int)ShareLoot.SpeedBonus);

        public ShareLootActionCardInfo()
        {
            AccessTools.Field(typeof(ActionCardInfo), "_uses").SetValue(this, 0); // 0 = unlimited uses
            AccessTools.Field(typeof(ActionCardInfo), "canNotBeReplenished").SetValue(this, false);
        }
    }

    /// <summary>
    /// Patch to intercept CardName getter for ShareLootActionCardInfo
    /// </summary>
    [HarmonyPatch(typeof(ActionCardInfo), "CardName", MethodType.Getter)]
    public static class ShareLootActionCardInfo_CardName_Patch
    {
        public static bool Prefix(ActionCardInfo __instance, ref string __result)
        {
            if (__instance is ShareLootActionCardInfo customInfo)
            {
                __result = customInfo.CustomCardName;
                return false;
            }
            return true;
        }
    }

    /// <summary>
    /// Patch to intercept CardDescription getter for ShareLootActionCardInfo
    /// </summary>
    [HarmonyPatch(typeof(ActionCardInfo), "CardDescription", MethodType.Getter)]
    public static class ShareLootActionCardInfo_CardDescription_Patch
    {
        public static bool Prefix(ActionCardInfo __instance, ref string __result)
        {
            if (__instance is ShareLootActionCardInfo customInfo)
            {
                __result = customInfo.CustomCardDescription;
                return false;
            }
            return true;
        }
    }

    /// <summary>
    /// The action card that uses coins to buff a unit
    /// Implements IUnitTargetable to target player units
    /// </summary>
    public class ShareLootActionCard : ActionCard, IUnitTargetable
    {
        public Team TeamToAffect => Team.Player;

        public ShareLootActionCard(ActionCardInfo actionCardInfo)
        {
            Info = actionCardInfo;
            // Set to 0 for unlimited uses (but will be disabled if not enough coins)
            _usesLeft = 0;
        }

        public override ActionCard GetCopy()
        {
            return new ShareLootActionCard(Info);
        }

        public void ApplyCommand(BattleUnit unit)
        {
            if (!ShareLoot.Instance.IsActive)
                return;

            if (unit == null || !unit.IsAlive || unit.Team != Team.Player)
                return;

            // Check if player has enough coins
            var playerWallet = Singleton<Player>.Instance.PlayerData.PlayerWallet;
            if (!playerWallet.HasEnoughMoney(ShareLoot.CoinCost))
            {
                MelonLogger.Msg($"ShareLoot: Not enough coins. Need {ShareLoot.CoinCost}, have {playerWallet.Coins}");
                return;
            }

            // Deduct the cost
            playerWallet.ChangeCoinsByValue(-ShareLoot.CoinCost);
            MelonLogger.Msg($"ShareLoot: Deducted {ShareLoot.CoinCost} coin(s). Remaining: {playerWallet.Coins}");

            ApplyBuffs(unit);
        }

        private void ApplyBuffs(BattleUnit unit)
        {
            // Get current stack count for this unit
            if (!ShareLoot.UnitBuffStacks.ContainsKey(unit))
            {
                ShareLoot.UnitBuffStacks[unit] = 0;
            }

            int stackCount = ShareLoot.UnitBuffStacks[unit];
            string stackSuffix = $"_{stackCount}";

            // Apply stat buffs with unique identifiers so they stack
            unit.ChangeStat(UnitStats.Accuracy, ShareLoot.AccuracyBonus, $"ShareLoot_Accuracy{stackSuffix}");
            unit.ChangeStat(UnitStats.Power, ShareLoot.PowerBonus, $"ShareLoot_Power{stackSuffix}");
            unit.ChangeStat(UnitStats.Speed, ShareLoot.SpeedBonus, $"ShareLoot_Speed{stackSuffix}");

            // Increment stack count
            ShareLoot.UnitBuffStacks[unit]++;

            MelonLogger.Msg($"ShareLoot: {unit.UnitNameNoNumber} received buffs (Stack {ShareLoot.UnitBuffStacks[unit]})");
        }

        IEnumerable<CommandsAvailabilityChecker.UnitAnavailableReasons> IUnitTargetable.IsUnitValid(BattleUnit unit)
        {
            var reasons = new List<CommandsAvailabilityChecker.UnitAnavailableReasons>();

            if (!ShareLoot.Instance.IsActive)
            {
                return reasons;
            }

            // Can only target alive units
            if (!unit.IsAlive)
            {
                reasons.Add(CommandsAvailabilityChecker.UnitAnavailableReasons.UnitIsDead);
            }

            return reasons;
        }
    }
}
