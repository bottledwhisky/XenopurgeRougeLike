using HarmonyLib;
using SpaceCommander;
using SpaceCommander.ActionCards;
using static XenopurgeRougeLike.ModLocalization;

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
            name = L("synthetics.battlefield_repair.name");
            description = L("synthetics.battlefield_repair.description");
            flavourText = L("synthetics.battlefield_repair.flavour");
        }

        public override string GetDescriptionForStacks(int stacks)
        {
            return L("synthetics.battlefield_repair.description", HealPerPoint[stacks - 1]);
        }

        protected static BattlefieldRepair instance;
        public static BattlefieldRepair Instance => instance ??= new();
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
