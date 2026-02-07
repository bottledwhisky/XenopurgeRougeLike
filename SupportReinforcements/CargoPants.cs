using HarmonyLib;
using MelonLoader;
using SpaceCommander;
using SpaceCommander.ActionCards;
using static SpaceCommander.Enumerations;
using static XenopurgeRougeLike.ModLocalization;

namespace XenopurgeRougeLike.SupportReinforcements
{
    /// <summary>
    /// 裤子口袋：所有药剂可使用次数+1
    /// Cargo Pants: All medicines +1 use
    /// </summary>
    public class CargoPants : Reinforcement
    {
        public const int BonusUses = 1;

        public CargoPants()
        {
            company = Company.Support;
            rarity = Rarity.Standard;
            name = L("support.cargo_pants.name");
            description = L("support.cargo_pants.description");
            flavourText = L("support.cargo_pants.flavour");
        }

        public override string GetDescriptionForStacks(int stacks)
        {
            return L("support.cargo_pants.description");
        }

        private static CargoPants _instance;
        public static CargoPants Instance => _instance ??= new();
    }

    /// <summary>
    /// Patch to add bonus uses to all medicine cards (injections and heal items)
    /// </summary>
    [HarmonyPatch(typeof(ActionCard), MethodType.Constructor)]
    public static class CargoPants_BonusUses_Patch
    {
        public static void Postfix(ActionCard __instance)
        {
            // Only apply if CargoPants is active
            if (!CargoPants.Instance.IsActive)
                return;

            if (__instance?.Info == null)
                return;

            string cardId = __instance.Info.Id;

            // Only boost uses for injection and heal item cards
            if (!SupportAffinityHelpers.IsInjectionCard(cardId) && !SupportAffinityHelpers.IsHealItemCard(cardId))
                return;

            // Get current uses
            int currentUses = __instance.UsesLeft;

            // Add bonus uses (only if card has limited uses)
            if (currentUses > 0)
            {
                int newUses = currentUses + CargoPants.BonusUses;
                AccessTools.Field(typeof(ActionCard), "_usesLeft").SetValue(__instance, newUses);

                MelonLogger.Msg($"CargoPants: Added +{CargoPants.BonusUses} use to {__instance.Info.CardName} (now {newUses} uses)");
            }
        }
    }
}
