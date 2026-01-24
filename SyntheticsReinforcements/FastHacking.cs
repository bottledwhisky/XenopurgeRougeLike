using HarmonyLib;
using SpaceCommander.ActionCards;

namespace XenopurgeRougeLike.SyntheticsReinforcements
{
    // 快速黑入：所有黑入命令消耗-1（所有消耗接入点数的ActionCard成本-1）
    public class FastHacking : Reinforcement
    {
        public static readonly int CostReduction = 1;

        public FastHacking()
        {
            company = Company.Synthetics;
            stackable = false;
            maxStacks = 1;
            rarity = Rarity.Expert;
            name = "Fast Hacking";
            description = "All hacking commands cost 1 less access point.";
            flavourText = "Optimized bypass routines reduce the processing cycles required to breach security protocols.";
        }

        protected static FastHacking instance;
        public static FastHacking Instance => instance ??= new();
    }

    /// <summary>
    /// Patch to reduce the access points cost of all action cards by 1
    /// This patches the ActionCard.GetCostOfActionCard method to reduce the access points cost
    /// </summary>
    [HarmonyPatch(typeof(ActionCard), "GetCostOfActionCard")]
    public static class FastHacking_ReduceCost_Patch
    {
        public static void Postfix(ref ActionCard.CostOfActionCard __result)
        {
            if (!FastHacking.Instance.IsActive)
                return;

            // Reduce access points cost by 1, but never go below 0
            if (__result.CostOfAccessPoints > 0)
            {
                __result.CostOfAccessPoints = System.Math.Max(0, __result.CostOfAccessPoints - FastHacking.CostReduction);
            }
        }
    }
}
