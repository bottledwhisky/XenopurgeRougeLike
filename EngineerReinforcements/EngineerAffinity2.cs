using HarmonyLib;
using SpaceCommander.ActionCards;
using System.Collections.Generic;
using static XenopurgeRougeLike.ModLocalization;

namespace XenopurgeRougeLike.EngineerReinforcements
{
    public class EngineerAffinity2 : CompanyAffinity
    {
        public const float ExplosiveDamageMultiplier = 1.25f;
        public const float FlashbangDurationMultiplier = 1.5f;
        public const int ShopProbabilityBoostCopies = 2;

        public EngineerAffinity2()
        {
            unlockLevel = 2;
            company = Company.Engineer;
            int explosiveDamagePercent = (int)((ExplosiveDamageMultiplier - 1f) * 100);
            int flashbangDurationPercent = (int)((FlashbangDurationMultiplier - 1f) * 100);
            description = L("engineer.affinity2.description", explosiveDamagePercent, flashbangDurationPercent);
        }

        // Action card IDs for Engineer-related equipment
        public static readonly List<string> EngineerActionCards =
        [
            ActionCardIds.FRAG_GRENADE,
            ActionCardIds.FLASH_GRENADE,
            ActionCardIds.SETUP_MINE,
            ActionCardIds.SETUP_TURRET_RAT,
            ActionCardIds.SETUP_TURRET_BANG,
        ];

        public static EngineerAffinity2 _instance;

        public static EngineerAffinity2 Instance => _instance ??= new();

        public override void OnActivate()
        {
            // Register shop probability modifier for Engineer action cards
            ActionCardsUpgraderTools.RegisterProbabilityModifier(
                "EngineerAffinity2_ShopBoost",
                EngineerActionCards,
                ShopProbabilityBoostCopies,
                () => Instance.IsActive
            );
        }

        public override void OnDeactivate()
        {
            // Unregister shop probability modifier
            ActionCardsUpgraderTools.UnregisterProbabilityModifier("EngineerAffinity2_ShopBoost");
        }
    }

    /// <summary>
    /// Shared helpers for Engineer affinity patches
    /// </summary>
    public static class EngineerAffinityHelpers
    {
        // Cached field accessors
        private static readonly AccessTools.FieldRef<ChangeCurrentHealthArea_Card, float> _changeValueRef =
            AccessTools.FieldRefAccess<ChangeCurrentHealthArea_Card, float>("_changeValue");
        private static readonly AccessTools.FieldRef<ChangeStatArea_Card, float> _durationRef =
            AccessTools.FieldRefAccess<ChangeStatArea_Card, float>("_duration");
        private static readonly AccessTools.FieldRef<ChangeStatArea_Card, float> _remainingTimeRef =
            AccessTools.FieldRefAccess<ChangeStatArea_Card, float>("_remainingTime");

        /// <summary>
        /// Get the highest active explosive damage multiplier
        /// </summary>
        public static float GetExplosiveDamageMultiplier()
        {
            float multiplier = 1f;

            if (EngineerAffinity2.Instance.IsActive)
                multiplier = EngineerAffinity2.ExplosiveDamageMultiplier;

            // Higher level affinities override
            if (EngineerAffinity4.Instance?.IsActive == true)
                multiplier = EngineerAffinity4.ExplosiveDamageMultiplier;

            if (EngineerAffinity6.Instance?.IsActive == true)
                multiplier = EngineerAffinity6.ExplosiveDamageMultiplier;

            return multiplier;
        }

        /// <summary>
        /// Get the highest active flashbang duration multiplier
        /// </summary>
        public static float GetFlashbangDurationMultiplier()
        {
            float multiplier = 1f;

            if (EngineerAffinity2.Instance.IsActive)
                multiplier = EngineerAffinity2.FlashbangDurationMultiplier;

            // Higher level affinities override
            if (EngineerAffinity4.Instance?.IsActive == true)
                multiplier = EngineerAffinity4.FlashbangDurationMultiplier;

            if (EngineerAffinity6.Instance?.IsActive == true)
                multiplier = EngineerAffinity6.FlashbangDurationMultiplier;

            return multiplier;
        }

        /// <summary>
        /// Apply explosive damage multiplier to ChangeCurrentHealthArea_Card
        /// </summary>
        public static void ApplyExplosiveDamageBoost(ChangeCurrentHealthArea_Card instance)
        {
            float multiplier = GetExplosiveDamageMultiplier();
            if (multiplier <= 1f)
                return;

            ref float changeValue = ref _changeValueRef(instance);

            // Only boost if it's damage (negative value)
            if (changeValue < 0f)
            {
                changeValue *= multiplier;
            }
        }

        /// <summary>
        /// Apply flashbang duration multiplier to ChangeStatArea_Card
        /// </summary>
        public static void ApplyFlashbangDurationBoost(ChangeStatArea_Card instance)
        {
            float multiplier = GetFlashbangDurationMultiplier();
            if (multiplier <= 1f)
                return;

            ref float duration = ref _durationRef(instance);
            ref float remainingTime = ref _remainingTimeRef(instance);

            duration *= multiplier;
            remainingTime *= multiplier;
        }
    }

    /// <summary>
    /// Patch to boost grenade damage (ChangeCurrentHealthArea_Card)
    /// </summary>
    [HarmonyPatch(typeof(ChangeCurrentHealthArea_Card), "ApplyCommand")]
    public static class Engineer_GrenadeDamage_Patch
    {
        public static void Prefix(ChangeCurrentHealthArea_Card __instance)
        {
            EngineerAffinityHelpers.ApplyExplosiveDamageBoost(__instance);
        }
    }

    /// <summary>
    /// Patch to boost flashbang duration (ChangeStatArea_Card)
    /// </summary>
    [HarmonyPatch(typeof(ChangeStatArea_Card), "ApplyCommand")]
    public static class Engineer_FlashbangDuration_Patch
    {
        public static void Prefix(ChangeStatArea_Card __instance)
        {
            EngineerAffinityHelpers.ApplyFlashbangDurationBoost(__instance);
        }
    }
}
