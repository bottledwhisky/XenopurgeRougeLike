using HarmonyLib;
using SpaceCommander;
using TimeSystem;

namespace XenopurgeRougeLike.SyntheticsReinforcements
{
    // 高性能集群：接入点数会随时间（10s/5s/5s）缓慢增加（1/1/2）
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
            name = "High Performance Cluster";
            description = "Access points regenerate over time.";
            flavourText = "Additional processing nodes enable continuous background calculation of system vulnerabilities in the local environment.";
        }

        public override string GetDescriptionForStacks(int stacks)
        {
            if (stacks == 1)
                return "Access points regenerate over time (1 point every 10 seconds).";
            else if (stacks == 2)
                return "Access points regenerate over time (1 point every 5 seconds).";
            else
                return "Access points regenerate over time (2 points every 5 seconds).";
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
