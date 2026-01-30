using HarmonyLib;
using SpaceCommander.ActionCards;
using static XenopurgeRougeLike.ModLocalization;

namespace XenopurgeRougeLike.SyntheticsReinforcements
{
    /// <summary>
    /// Custom ActionCardInfo for BattlefieldRepair
    /// Since CardName and CardDescription are not virtual, we'll use Harmony patches to intercept them
    /// </summary>
    public class BattlefieldRepairActionCardInfo : ActionCardInfo
    {
        // These properties will be accessed by the Harmony patches
        public string CustomCardName => L("synthetics.battlefield_repair.card_name");

        public string CustomCardDescription =>
            L("synthetics.battlefield_repair.card_description",
              BattlefieldRepair.HealPerPoint[BattlefieldRepair.Instance.currentStacks - 1]);
    }

    /// <summary>
    /// Patch to intercept CardName getter for BattlefieldRepairActionCardInfo
    /// </summary>
    [HarmonyPatch(typeof(ActionCardInfo), "CardName", MethodType.Getter)]
    public static class BattlefieldRepairActionCardInfo_CardName_Patch
    {
        public static bool Prefix(ActionCardInfo __instance, ref string __result)
        {
            if (__instance is BattlefieldRepairActionCardInfo customInfo)
            {
                __result = customInfo.CustomCardName;
                return false; // Skip original method
            }
            return true; // Run original method for other ActionCardInfo instances
        }
    }

    /// <summary>
    /// Patch to intercept CardDescription getter for BattlefieldRepairActionCardInfo
    /// </summary>
    [HarmonyPatch(typeof(ActionCardInfo), "CardDescription", MethodType.Getter)]
    public static class BattlefieldRepairActionCardInfo_CardDescription_Patch
    {
        public static bool Prefix(ActionCardInfo __instance, ref string __result)
        {
            if (__instance is BattlefieldRepairActionCardInfo customInfo)
            {
                __result = customInfo.CustomCardDescription;
                return false; // Skip original method
            }
            return true; // Run original method for other ActionCardInfo instances
        }
    }
}
