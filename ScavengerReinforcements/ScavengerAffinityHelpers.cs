using HarmonyLib;
using SpaceCommander.Commands;
using SpaceCommander.GameFlow;

namespace XenopurgeRougeLike.ScavengerReinforcements
{
    // Shared helpers for Scavenger affinities
    public static class ScavengerAffinityHelpers
    {
        // Get the current active collectible multiplier based on which affinity is active
        public static float GetCollectibleMultiplier()
        {
            if (ScavengerAffinity6.Instance.IsActive)
                return ScavengerAffinity6.CollectibleMultiplier;
            if (ScavengerAffinity4.Instance.IsActive)
                return ScavengerAffinity4.CollectibleMultiplier;
            if (ScavengerAffinity2.Instance.IsActive)
                return ScavengerAffinity2.CollectibleMultiplier;
            return 1.0f;
        }

        // Check if any Scavenger affinity that affects collectibles is active
        public static bool IsAnyScavengerAffinityActive()
        {
            return ScavengerAffinity2.Instance.IsActive ||
                   ScavengerAffinity4.Instance.IsActive ||
                   ScavengerAffinity6.Instance.IsActive;
        }
    }

    // Patch UnitsPlacementPhase.PlaceCollectibles to increase collectible count
    [HarmonyPatch(typeof(UnitsPlacementPhase), "PlaceCollectibles")]
    public static class ScavengerAffinity_PlaceCollectibles_Patch
    {
        public static void Prefix(ref int count)
        {
            if (!ScavengerAffinityHelpers.IsAnyScavengerAffinityActive())
                return;

            float multiplier = ScavengerAffinityHelpers.GetCollectibleMultiplier();
            if (multiplier > 1.0f)
            {
                int originalCount = count;
                count = (int)(count * multiplier);
                // Ensure at least 1 extra collectible if multiplier would round down
                if (count == originalCount && originalCount > 0)
                {
                    count = originalCount + 1;
                }
            }
        }
    }
}
