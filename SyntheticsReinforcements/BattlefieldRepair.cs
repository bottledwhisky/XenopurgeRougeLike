using HarmonyLib;
using SpaceCommander;
using SpaceCommander.ActionCards;

namespace XenopurgeRougeLike.SyntheticsReinforcements
{
    // 可以在战斗中消耗点数回复生命值，溢出的治疗会转换为护甲，1点数->15/25/35
    public class BattlefieldRepair : Reinforcement
    {
        public static readonly float[] HealPerPoint = [15f, 25f, 35f];

        public BattlefieldRepair()
        {
            company = Company.Synthetics;
            stackable = true;
            maxStacks = 3;
            name = "Battlefield Repair";
            description = "Consume points during battle to restore health. Excess healing converts to armor. 1 point -> {0} HP/Armor.";
            flavourText = "Weyland-Yutani synthetics can consume patented nano robots to accelerate tissue regeneration during combat operations.";
        }

        public override string GetDescriptionForStacks(int stacks)
        {
            return string.Format(description, HealPerPoint[stacks - 1]);
        }

        public static BattlefieldRepair Instance => (BattlefieldRepair)Synthetics.Reinforcements[typeof(BattlefieldRepair)];
    }

    /// <summary>
    /// Patch to inject BattlefieldRepairActionCard into InBattleActionCardsManager after initialization
    /// </summary>
    [HarmonyPatch(typeof(InBattleActionCardsManager), "Initialize")]
    public static class BattlefieldRepair_InjectActionCard_Patch
    {
        public static void Postfix(InBattleActionCardsManager __instance)
        {
            if (!BattlefieldRepair.Instance.IsActive)
                return;

            // Create custom ActionCardInfo with English localization
            var actionCardInfo = new BattlefieldRepairActionCardInfo();
            actionCardInfo.SetId("BattlefieldRepair");

            // Create and add the BattlefieldRepairActionCard instance
            var repairCard = new BattlefieldRepairActionCard(
                actionCardInfo,
                teamToAffect: Enumerations.Team.Player
            );

            // Add to the InBattleActionCards list
            __instance.InBattleActionCards.Add(repairCard);
        }
    }
}
