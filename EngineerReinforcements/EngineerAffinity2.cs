using HarmonyLib;
using SpaceCommander.ActionCards;
using SpaceCommander.Area;
using System.Collections.Generic;

namespace XenopurgeRougeLike.EngineerReinforcements
{
    public class EngineerAffinity2 : CompanyAffinity
    {
        public EngineerAffinity2()
        {
            unlockLevel = 2;
            company = Company.Engineer;
            description = "地雷、手雷伤害+25%，闪光弹效果持续时间+50%，指令商店出现地雷、手雷、闪光弹、炮台的概率提升";
        }

        public const float ExplosiveDamageMultiplier = 1.25f;
        public const float FlashbangDurationMultiplier = 1.5f;
        public const int ShopProbabilityBoostCopies = 2;

        // Action card IDs for Engineer-related equipment
        public static readonly List<string> EngineerActionCards = new()
        {
            "bfb700d8-5fa2-4bd0-b1dd-94842f66c031", // Frag Grenade
            "3b1ee954-9aec-45fe-afa0-46fbc9fc99a0", // Flash Grenade
            "8daa3d58-73aa-4c26-a20f-954686777d1f", // Setup Mine
            "3e9b1bb6-b377-49cd-af43-9c10dee7e81c", // Setup Turret (RAT)
            "8b9dc11f-7e75-4295-92b8-1eb9417896f6", // Setup Turret (BANG)
        };

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
    /// Patch to boost grenade damage (ChangeCurrentHealthArea_Card)
    /// </summary>
    [HarmonyPatch(typeof(ChangeCurrentHealthArea_Card), "ApplyCommand")]
    public static class EngineerAffinity2_GrenadeDamage_Patch
    {
        private static readonly AccessTools.FieldRef<ChangeCurrentHealthArea_Card, float> _changeValueRef =
            AccessTools.FieldRefAccess<ChangeCurrentHealthArea_Card, float>("_changeValue");

        public static void Prefix(ChangeCurrentHealthArea_Card __instance)
        {
            if (!EngineerAffinity2.Instance.IsActive)
                return;

            // Get the damage value (negative value for damage)
            ref float changeValue = ref _changeValueRef(__instance);

            // Only boost if it's damage (negative value)
            if (changeValue < 0f)
            {
                // Apply explosive damage multiplier (grenades and mines)
                changeValue *= EngineerAffinity2.ExplosiveDamageMultiplier;
            }
        }
    }

    /// <summary>
    /// Patch to boost flashbang duration (ChangeStatArea_Card)
    /// </summary>
    [HarmonyPatch(typeof(ChangeStatArea_Card), "ApplyCommand")]
    public static class EngineerAffinity2_FlashbangDuration_Patch
    {
        private static readonly AccessTools.FieldRef<ChangeStatArea_Card, float> _durationRef =
            AccessTools.FieldRefAccess<ChangeStatArea_Card, float>("_duration");
        private static readonly AccessTools.FieldRef<ChangeStatArea_Card, float> _remainingTimeRef =
            AccessTools.FieldRefAccess<ChangeStatArea_Card, float>("_remainingTime");

        public static void Prefix(ChangeStatArea_Card __instance)
        {
            if (!EngineerAffinity2.Instance.IsActive)
                return;

            // Get references to the duration values
            ref float duration = ref _durationRef(__instance);
            ref float remainingTime = ref _remainingTimeRef(__instance);

            // Apply flashbang duration multiplier
            duration *= EngineerAffinity2.FlashbangDurationMultiplier;
            remainingTime *= EngineerAffinity2.FlashbangDurationMultiplier;
        }
    }
}
