using HarmonyLib;
using MelonLoader;
using SpaceCommander;
using SpaceCommander.ActionCards;
using static SpaceCommander.Enumerations;
using static XenopurgeRougeLike.ModLocalization;

namespace XenopurgeRougeLike.SupportReinforcements
{
    /// <summary>
    /// 更大背包：所有药剂、回复品和爆炸物可使用次数+1
    /// Bigger Backpack: All medicines, heal items, and explosives +1 use
    /// </summary>
    public class BiggerBackpack : Reinforcement
    {
        public const int BonusUses = 1;

        public BiggerBackpack()
        {
            company = Company.Support;
            rarity = Rarity.Expert;
            name = L("support.bigger_backpack.name");
            description = L("support.bigger_backpack.description");
            flavourText = L("support.bigger_backpack.flavour");
        }

        public override string GetDescriptionForStacks(int stacks)
        {
            return L("support.bigger_backpack.description");
        }

        private static BiggerBackpack _instance;
        public static BiggerBackpack Instance => _instance ??= new();
    }

    /// <summary>
    /// Patch to add bonus uses to all medicine, heal item, and explosive cards
    /// </summary>
    [HarmonyPatch(typeof(ActionCard), MethodType.Constructor)]
    public static class BiggerBackpack_BonusUses_Patch
    {
        public static void Postfix(ActionCard __instance)
        {
            // Only apply if BiggerBackpack is active
            if (!BiggerBackpack.Instance.IsActive)
                return;

            if (__instance?.Info == null)
                return;

            string cardId = __instance.Info.Id;

            // Boost uses for injection, heal item, AND explosive cards
            if (!SupportAffinityHelpers.IsInjectionCard(cardId) &&
                !SupportAffinityHelpers.IsHealItemCard(cardId) &&
                !SupportAffinityHelpers.IsExplosiveCard(cardId))
                return;

            // Get current uses
            int currentUses = __instance.UsesLeft;

            // Add bonus uses (only if card has limited uses)
            if (currentUses > 0)
            {
                int newUses = currentUses + BiggerBackpack.BonusUses;
                AccessTools.Field(typeof(ActionCard), "_usesLeft").SetValue(__instance, newUses);

                MelonLogger.Msg($"BiggerBackpack: Added +{BiggerBackpack.BonusUses} use to {__instance.Info.CardName} (now {newUses} uses)");
            }
        }
    }
}
