using System.Collections.Generic;
using static XenopurgeRougeLike.ModLocalization;

namespace XenopurgeRougeLike.SupportReinforcements
{
    /// <summary>
    /// Support Affinity 2: Injection duration +25%, increased shop spawn rate for injections and heal items
    /// 支援兵天赋2：药剂持续时间+25%，指令商店出现药剂和回复品的概率提升
    /// </summary>
    public class SupportAffinity2 : CompanyAffinity
    {
        public const float InjectionDurationMultiplier = 1.25f;
        public const int ShopProbabilityBoostCopies = 2;

        public SupportAffinity2()
        {
            unlockLevel = 2;
            company = Company.Support;
            int durationPercent = (int)((InjectionDurationMultiplier - 1f) * 100);
            description = L("support.affinity2.description", durationPercent);
        }

        // Action card IDs for Support-related cards
        // 药剂 (Injections): Brutadyne (Power), Kinetra (Speed), Optivex (Accuracy)
        // 回复品 (Heal Items): Health Stim, First Aid Kit
        public static readonly List<string> SupportActionCards = new()
        {
            "86cafd8b-9e28-4fd1-9e44-4ccdabb00137", // Inject Brutadyne (BuffPower)
            "82a8cd80-af72-4785-b4c9-eab1a498a125", // Inject Kinetra (BuffSpeed)
            "b51454f9-5641-4b07-94bf-93312555e860", // Inject Optivex (BuffAccuracy)
            "6569d382-07ef-4db5-86ac-bac9eb249889", // Inject Health Stim (Heal)
            "90793211-9445-4ac1-9d06-fc94e547b416", // Apply First Aid Kit (FirstAidKit)
        };

        public static SupportAffinity2 _instance;

        public static SupportAffinity2 Instance => _instance ??= new();

        public override void OnActivate()
        {
            // Register shop probability modifier for Support action cards
            ActionCardsUpgraderTools.RegisterProbabilityModifier(
                "SupportAffinity2_ShopBoost",
                SupportActionCards,
                ShopProbabilityBoostCopies,
                () => Instance.IsActive
            );
        }

        public override void OnDeactivate()
        {
            // Unregister shop probability modifier
            ActionCardsUpgraderTools.UnregisterProbabilityModifier("SupportAffinity2_ShopBoost");
        }
    }
}
