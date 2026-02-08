using static XenopurgeRougeLike.ModLocalization;

namespace XenopurgeRougeLike.ScavengerReinforcements
{
    // Scavenger Affinity Level 6: 100% more collectibles spawn, -90% collection time, higher chance of same-company reinforcements
    public class ScavengerAffinity6 : CompanyAffinity
    {
        public const float CollectibleMultiplier = 2.0f;
        public const float CollectionTimeMultiplier = 0.1f;

        public ScavengerAffinity6()
        {
            unlockLevel = 6;
            company = Company.Scavenger;
            description = L("scavenger.affinity6.description",
                (int)((CollectibleMultiplier - 1) * 100),
                (int)((1 - CollectionTimeMultiplier) * 100));
        }

        public static ScavengerAffinity6 _instance;
        public static ScavengerAffinity6 Instance => _instance ??= new();

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
