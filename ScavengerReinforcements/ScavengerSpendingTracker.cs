using HarmonyLib;
using MelonLoader;
using SpaceCommander;

namespace XenopurgeRougeLike.ScavengerReinforcements
{
    /// <summary>
    /// Tracks coins spent on scavenger reinforcements during a mission
    /// </summary>
    public static class ScavengerSpendingTracker
    {
        private static int _coinsSpentThisMission = 0;
        public static int CoinsSpentThisMission => _coinsSpentThisMission;

        public static void AddSpending(int amount)
        {
            _coinsSpentThisMission += amount;
            MelonLogger.Msg($"ScavengerSpendingTracker: Spent {amount} coins. Total spent this mission: {_coinsSpentThisMission}");
        }

        public static void ClearSpending()
        {
            _coinsSpentThisMission = 0;
        }
    }

    // Clear spending tracking when mission starts
    [HarmonyPatch(typeof(TestGame), "StartGame")]
    public static class ScavengerSpendingTracker_ClearTracking_StartGame_Patch
    {
        public static void Postfix()
        {
            ScavengerSpendingTracker.ClearSpending();
            MelonLogger.Msg("ScavengerSpendingTracker: Cleared spending tracking for new mission");
        }
    }
}
