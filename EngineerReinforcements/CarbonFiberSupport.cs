using HarmonyLib;
using SpaceCommander.ActionCards;
using System;
using System.Collections.Generic;
using static XenopurgeRougeLike.ModLocalization;

namespace XenopurgeRougeLike.EngineerReinforcements
{
    // 碳纤维支架：可额外携带一架炮台进入任务
    public class CarbonFiberSupport : Reinforcement
    {
        public static readonly int BonusTurretUses = 1;

        // Turret action card IDs
        public static readonly List<string> TurretActionCards = new()
        {
            "3e9b1bb6-b377-49cd-af43-9c10dee7e81c", // Setup Turret (RAT)
            "8b9dc11f-7e75-4295-92b8-1eb9417896f6", // Setup Turret (BANG)
        };

        public CarbonFiberSupport()
        {
            company = Company.Engineer;
            rarity = Rarity.Expert;
            stackable = false;
            maxStacks = 1;
            name = L("engineer.carbon_fiber_support.name");
            description = L("engineer.carbon_fiber_support.description", BonusTurretUses);
            flavourText = L("engineer.carbon_fiber_support.flavour");
        }

        protected static CarbonFiberSupport instance;
        public static CarbonFiberSupport Instance => instance ??= new();
    }

    /// <summary>
    /// Patch ActionCard constructor to add bonus uses for turret cards when CarbonFiberSupport is active
    /// </summary>
    [HarmonyPatch(typeof(ActionCard), MethodType.Constructor)]
    public static class CarbonFiberSupport_ActionCard_Constructor_Patch
    {
        public static void Postfix(ActionCard __instance)
        {
            // Check if CarbonFiberSupport is active
            if (!CarbonFiberSupport.Instance.IsActive)
                return;

            // Check if the card is valid
            if (__instance?.Info == null)
                return;

            string cardId = __instance.Info.Id;

            // Only boost uses for Turret action cards
            if (!CarbonFiberSupport.TurretActionCards.Contains(cardId))
                return;

            // Get current uses
            int currentUses = __instance.UsesLeft;

            // Add bonus uses (only if card has limited uses)
            if (currentUses > 0)
            {
                int newUses = currentUses + CarbonFiberSupport.BonusTurretUses;
                AccessTools.Field(typeof(ActionCard), "_usesLeft").SetValue(__instance, newUses);
            }
        }
    }
}
