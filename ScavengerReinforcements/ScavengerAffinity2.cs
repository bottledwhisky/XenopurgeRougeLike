using static XenopurgeRougeLike.ModLocalization;

namespace XenopurgeRougeLike.ScavengerReinforcements
{
    // Scavenger Affinity Level 2: 25% more collectibles spawn
    public class ScavengerAffinity2 : CompanyAffinity
    {
        public const float CollectibleMultiplier = 1.25f;

        public ScavengerAffinity2()
        {
            unlockLevel = 2;
            company = Company.Scavenger;
            description = L("scavenger.affinity2.description", (int)((CollectibleMultiplier - 1) * 100));
        }

        public static ScavengerAffinity2 _instance;
        public static ScavengerAffinity2 Instance => _instance ??= new();

        public override void OnActivate()
        {
            // Implementation is in ScavengerAffinityHelpers
        }

        public override void OnDeactivate()
        {
            // Implementation is in ScavengerAffinityHelpers
        }
    }
}
