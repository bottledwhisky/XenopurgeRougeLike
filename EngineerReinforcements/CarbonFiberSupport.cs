using HarmonyLib;
using SpaceCommander.ActionCards;
using System.Collections.Generic;
using static XenopurgeRougeLike.ModLocalization;

namespace XenopurgeRougeLike.EngineerReinforcements
{
    // 碳纤维支架：可额外携带一架炮台进入任务
    public class CarbonFiberSupport : Reinforcement
    {
        public static readonly int BonusTurretUses = 1;

        // Turret action card IDs
        public static readonly List<string> TurretActionCards =
        [
            "3e9b1bb6-b377-49cd-af43-9c10dee7e81c", // Setup Turret (RAT)
            "8b9dc11f-7e75-4295-92b8-1eb9417896f6", // Setup Turret (BANG)
        ];

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

    [HarmonyPatch(typeof(TestGame), "StartGame")]
    public static class CarbonFiberSupport_ActionCard_Constructor_Patch
    {
        public static void Postfix()
        {
            // Check if CarbonFiberSupport is active
            if (!CarbonFiberSupport.Instance.IsActive)
                return;

            var gainedActionCards = InBattleActionCardsManager.Instance.InBattleActionCards;

            foreach (var card in gainedActionCards)
            {
                if (card?.Info == null)
                {
                    continue;
                }

                string cardId = card.Info.Id;
                string cardName = card.Info.CardName;

                // Check if this card should be processed
                if (!CarbonFiberSupport.TurretActionCards.Contains(cardId))
                    continue;

                // Get current uses
                int currentUses = card.UsesLeft;

                // Add bonus uses (only if card has limited uses)
                if (currentUses > 0)
                {
                    int newUses = currentUses + CarbonFiberSupport.BonusTurretUses;
                    AccessTools.Field(typeof(ActionCard), "_usesLeft").SetValue(card, newUses);
                }
            }
        }
    }
}
