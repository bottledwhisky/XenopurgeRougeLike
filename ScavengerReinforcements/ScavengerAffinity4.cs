using static XenopurgeRougeLike.ModLocalization;

namespace XenopurgeRougeLike.ScavengerReinforcements
{
    // Scavenger Affinity Level 4: 50% more collectibles spawn, -50% collection time, higher chance of same-company reinforcements
    public class ScavengerAffinity4 : CompanyAffinity
    {
        public const float CollectibleMultiplier = 1.5f;
        public const float CollectionTimeMultiplier = 0.5f;

        public ScavengerAffinity4()
        {
            unlockLevel = 4;
            company = Company.Scavenger;
            description = L("scavenger.affinity4.description",
                (int)((CollectibleMultiplier - 1) * 100),
                (int)((1 - CollectionTimeMultiplier) * 100));
        }

        public static ScavengerAffinity4 _instance;
        public static ScavengerAffinity4 Instance => _instance ??= new();

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
