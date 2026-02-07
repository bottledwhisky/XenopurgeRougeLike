using HarmonyLib;
using static XenopurgeRougeLike.ModLocalization;

namespace XenopurgeRougeLike.SupportReinforcements
{
    /// <summary>
    /// 战术背带：所有药剂、回复品可使用次数+1
    /// Tactical Harness: All medicines and heal items +1 use
    /// </summary>
    public class TacticalHarness : Reinforcement
    {
        public const int BonusUses = 1;

        public TacticalHarness()
        {
            company = Company.Support;
            rarity = Rarity.Elite;
            name = L("support.tactical_harness.name");
            description = L("support.tactical_harness.description");
            flavourText = L("support.tactical_harness.flavour");
        }

        public override string GetDescriptionForStacks(int stacks)
        {
            return L("support.tactical_harness.description");
        }

        private static TacticalHarness _instance;
        public static TacticalHarness Instance => _instance ??= new();
    }

    /// <summary>
    /// Patch to add bonus uses when mission starts (after cards are created)
    /// </summary>
    [HarmonyPatch(typeof(TestGame), "StartGame")]
    public static class TacticalHarness_StartGame_Patch
    {
        public static void Postfix()
        {
            if (!TacticalHarness.Instance.IsActive)
                return;

            // Add bonus uses to injection and heal item cards
            SupportAffinityHelpers.AddBonusUsesAtMissionStart(
                "TacticalHarness",
                TacticalHarness.BonusUses,
                (cardId) => SupportAffinityHelpers.IsInjectionCard(cardId) ||
                           SupportAffinityHelpers.IsHealItemCard(cardId)
            );
        }
    }
}
