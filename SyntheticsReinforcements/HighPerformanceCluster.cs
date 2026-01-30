using HarmonyLib;
using SpaceCommander;
using TimeSystem;
using static XenopurgeRougeLike.ModLocalization;

namespace XenopurgeRougeLike.SyntheticsReinforcements
{
    // High Performance Cluster: Access points slowly increase over time (10s/5s/5s intervals, granting 1/1/2 access points respectively)
    public class HighPerformanceCluster : Reinforcement
    {
        // Time intervals for each stack level
        public static readonly float[] TimeIntervals = [10f, 5f, 5f];
        // Access points added per interval for each stack level
        public static readonly int[] AccessPointsAdded = [1, 1, 2];

        private float _timer = 0f;
        private int _stackLevel = 0;

        public HighPerformanceCluster()
        {
            company = Company.Synthetics;
            stackable = true;
            maxStacks = 3;
            name = L("synthetics.high_performance_cluster.name");
            description = L("synthetics.high_performance_cluster.description");
            flavourText = L("synthetics.high_performance_cluster.flavour");
        }

        public override string GetDescriptionForStacks(int stacks)
        {
            if (stacks == 1)
                return L("synthetics.high_performance_cluster.description_lv1", TimeIntervals[0], AccessPointsAdded[0]);
            else if (stacks == 2)
                return L("synthetics.high_performance_cluster.description_lv2", TimeIntervals[1], AccessPointsAdded[1]);
            else
                return L("synthetics.high_performance_cluster.description_lv3", TimeIntervals[2], AccessPointsAdded[2]);
        }

        protected static HighPerformanceCluster instance;
        public static HighPerformanceCluster Instance => instance ??= new();

        public void UpdateTime(float timePassed)
        {
            if (!isActive)
                return;

            _stackLevel = currentStacks - 1; // Convert to 0-indexed
            if (_stackLevel < 0 || _stackLevel >= TimeIntervals.Length)
                return;

            _timer += timePassed;
            float interval = TimeIntervals[_stackLevel];

            if (_timer >= interval)
            {
                _timer -= interval;
                var gameManager = GameManager.Instance;
                if (gameManager != null && gameManager.AccessPointsManager != null)
                {
                    gameManager.AccessPointsManager.AddAccessPoints(AccessPointsAdded[_stackLevel]);
                }
            }
        }
    }

    [HarmonyPatch(typeof(GameManager), "CreateNewGame")]
    public static class HighPerformanceCluster_CreateNewGame_Patch
    {
        public static void Postfix(GameManager __instance)
        {
            if (!HighPerformanceCluster.Instance.IsActive)
                return;

            // Subscribe to time updates
            TempSingleton<TimeManager>.Instance.OnTimeLateUpdated += HighPerformanceCluster.Instance.UpdateTime;
            // Subscribe to game finished event to clean up
            __instance.OnGameFinished += OnGameFinished;
        }
        public static void OnGameFinished(SpaceCommander.EndGame.EndGameResultData data)
        {
            // Unsubscribe from time updates when game finishes
            TempSingleton<TimeManager>.Instance.OnTimeLateUpdated -= HighPerformanceCluster.Instance.UpdateTime;
        }
    }
}
