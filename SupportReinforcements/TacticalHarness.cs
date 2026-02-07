using HarmonyLib;
using MelonLoader;
using SpaceCommander;
using SpaceCommander.ActionCards;
using static SpaceCommander.Enumerations;
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
    /// Patch to add bonus uses to all medicine and heal item cards
    /// </summary>
    [HarmonyPatch(typeof(ActionCard), MethodType.Constructor)]
    public static class TacticalHarness_BonusUses_Patch
    {
        public static void Postfix(ActionCard __instance)
        {
            // Only apply if TacticalHarness is active
            if (!TacticalHarness.Instance.IsActive)
                return;

            if (__instance?.Info == null)
                return;

            string cardId = __instance.Info.Id;

            // Boost uses for BOTH injection and heal item cards
            if (!SupportAffinityHelpers.IsInjectionCard(cardId) && !SupportAffinityHelpers.IsHealItemCard(cardId))
                return;

            // Get current uses
            int currentUses = __instance.UsesLeft;

            // Add bonus uses (only if card has limited uses)
            if (currentUses > 0)
            {
                int newUses = currentUses + TacticalHarness.BonusUses;
                AccessTools.Field(typeof(ActionCard), "_usesLeft").SetValue(__instance, newUses);

                MelonLogger.Msg($"TacticalHarness: Added +{TacticalHarness.BonusUses} use to {__instance.Info.CardName} (now {newUses} uses)");
            }
        }
    }
}
