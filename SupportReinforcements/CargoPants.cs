using HarmonyLib;
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
    /// Patch to add bonus uses when mission starts (after cards are created)
    /// </summary>
    [HarmonyPatch(typeof(TestGame), "StartGame")]
    public static class CargoPants_StartGame_Patch
    {
        public static void Postfix()
        {
            if (!CargoPants.Instance.IsActive)
                return;

            // Add bonus uses to injection and heal item cards
            SupportAffinityHelpers.AddBonusUsesAtMissionStart(
                "CargoPants",
                CargoPants.BonusUses,
                SupportAffinityHelpers.IsInjectionCard
            );
        }
    }
}
