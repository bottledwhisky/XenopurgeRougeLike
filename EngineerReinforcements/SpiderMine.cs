using HarmonyLib;
using SpaceCommander.Commands;
using static XenopurgeRougeLike.ModLocalization;

namespace XenopurgeRougeLike.EngineerReinforcements
{
    // 蜘蛛雷：地雷部署时间加快（可叠加2）
    public class SpiderMine : Reinforcement
    {
        public static readonly float[] DeployTimeMultiplier = [0.5f, 0.1f];

        public SpiderMine()
        {
            company = Company.Engineer;
            rarity = Rarity.Standard;
            stackable = true;
            maxStacks = 2;
            name = L("engineer.spider_mine.name");
            description = L("engineer.spider_mine.description");
            flavourText = L("engineer.spider_mine.flavour");
        }

        public override string GetDescriptionForStacks(int stacks)
        {
            int reductionPercent = (int)((1f - DeployTimeMultiplier[stacks - 1]) * 100);
            return L("engineer.spider_mine.description", reductionPercent);
        }

        protected static SpiderMine instance;
        public static SpiderMine Instance => instance ??= new();
    }

    /// <summary>
    /// Patch to reduce mine deployment time in SetMineCommand
    /// </summary>
    [HarmonyPatch(typeof(SetMineCommand), "InitializeValues")]
    public static class SpiderMine_ReduceDeployTime_Patch
    {
        private static readonly AccessTools.FieldRef<SetMineCommand, float> _timeOfCommandRef =
            AccessTools.FieldRefAccess<SetMineCommand, float>("_timeOfCommand");

        public static void Postfix(SetMineCommand __instance)
        {
            if (!SpiderMine.Instance.IsActive)
                return;

            // Get the current deployment time
            ref float timeOfCommand = ref _timeOfCommandRef(__instance);

            // Apply the speed multiplier
            float multiplier = SpiderMine.DeployTimeMultiplier[SpiderMine.Instance.currentStacks - 1];
            timeOfCommand *= multiplier;
        }
    }

    /// <summary>
    /// Patch to update deployment time when UpdateValues is called
    /// (this happens when unit stats change during deployment)
    /// </summary>
    [HarmonyPatch(typeof(SetMineCommand), "UpdateValues")]
    public static class SpiderMine_UpdateDeployTime_Patch
    {
        private static readonly AccessTools.FieldRef<SetMineCommand, float> _timeOfCommandRef =
            AccessTools.FieldRefAccess<SetMineCommand, float>("_timeOfCommand");
        private static readonly AccessTools.FieldRef<SetMineCommand, float> _remainingTimeRef =
            AccessTools.FieldRefAccess<SetMineCommand, float>("_remainingTime");

        public static void Postfix(SetMineCommand __instance)
        {
            if (!SpiderMine.Instance.IsActive)
                return;

            // Get the current values
            ref float timeOfCommand = ref _timeOfCommandRef(__instance);
            ref float remainingTime = ref _remainingTimeRef(__instance);

            // Calculate the completion percentage before modification
            float completionPercent = (timeOfCommand > 0f) ? (timeOfCommand - remainingTime) / timeOfCommand : 0f;

            // Apply the speed multiplier
            float multiplier = SpiderMine.DeployTimeMultiplier[SpiderMine.Instance.currentStacks - 1];
            timeOfCommand *= multiplier;

            // Maintain the same completion percentage
            remainingTime = timeOfCommand * (1f - completionPercent);
        }
    }
}
