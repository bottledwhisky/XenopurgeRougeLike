using HarmonyLib;
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
    /// Patch to add bonus uses when mission starts (after cards are created)
    /// </summary>
    [HarmonyPatch(typeof(TestGame), "StartGame")]
    public static class BiggerBackpack_StartGame_Patch
    {
        public static void Postfix()
        {
            if (!BiggerBackpack.Instance.IsActive)
                return;

            // Add bonus uses to injection, heal item, and explosive cards
            SupportAffinityHelpers.AddBonusUsesAtMissionStart(
                "BiggerBackpack",
                BiggerBackpack.BonusUses,
                (cardId) => SupportAffinityHelpers.IsInjectionCard(cardId) ||
                           SupportAffinityHelpers.IsHealItemCard(cardId) ||
                           SupportAffinityHelpers.IsExplosiveCard(cardId)
            );
        }
    }
}
