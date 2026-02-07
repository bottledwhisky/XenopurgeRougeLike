using HarmonyLib;
using SpaceCommander;
using SpaceCommander.ActionCards;
using System.Collections.Generic;

namespace XenopurgeRougeLike.SupportReinforcements
{
    /// <summary>
    /// Shared helpers for Support affinity patches
    /// </summary>
    public static class SupportAffinityHelpers
    {
        // Cached field accessors for ChangeStat_Card (injections)
        private static readonly AccessTools.FieldRef<ChangeStat_Card, float> _durationRef =
            AccessTools.FieldRefAccess<ChangeStat_Card, float>("_duration");
        private static readonly AccessTools.FieldRef<ChangeStat_Card, float> _remainingTimeRef =
            AccessTools.FieldRefAccess<ChangeStat_Card, float>("_remainingTime");

        // Cached field accessor for ChangeCurrentHealth_Card (heal items)
        private static readonly AccessTools.FieldRef<ChangeCurrentHealth_Card, float> _changeValueRef =
            AccessTools.FieldRefAccess<ChangeCurrentHealth_Card, float>("_changeValue");

        /// <summary>
        /// Get the highest active injection duration multiplier
        /// </summary>
        public static float GetInjectionDurationMultiplier()
        {
            float multiplier = 1f;

            if (SupportAffinity2.Instance.IsActive)
                multiplier = SupportAffinity2.InjectionDurationMultiplier;

            // Higher level affinities override
            if (SupportAffinity4.Instance?.IsActive == true)
                multiplier = SupportAffinity4.InjectionDurationMultiplier;

            if (SupportAffinity6.Instance?.IsActive == true)
                multiplier = SupportAffinity6.InjectionDurationMultiplier;

            return multiplier;
        }

        /// <summary>
        /// Get the highest active heal effectiveness multiplier
        /// </summary>
        public static float GetHealEffectivenessMultiplier()
        {
            float multiplier = 1f;

            // Only affinity 4 and 6 boost heal effectiveness
            if (SupportAffinity4.Instance?.IsActive == true)
                multiplier = SupportAffinity4.HealEffectivenessMultiplier;

            if (SupportAffinity6.Instance?.IsActive == true)
                multiplier = SupportAffinity6.HealEffectivenessMultiplier;

            return multiplier;
        }

        /// <summary>
        /// Apply injection duration multiplier to ChangeStat_Card
        /// </summary>
        public static void ApplyInjectionDurationBoost(ChangeStat_Card instance)
        {
            float multiplier = GetInjectionDurationMultiplier();
            if (multiplier <= 1f)
                return;

            ref float duration = ref _durationRef(instance);
            ref float remainingTime = ref _remainingTimeRef(instance);

            duration *= multiplier;
            remainingTime *= multiplier;
        }

        /// <summary>
        /// Apply heal effectiveness multiplier to ChangeCurrentHealth_Card
        /// </summary>
        public static void ApplyHealEffectivenessBoost(ChangeCurrentHealth_Card instance)
        {
            float multiplier = GetHealEffectivenessMultiplier();
            if (multiplier <= 1f)
                return;

            ref float changeValue = ref _changeValueRef(instance);

            // Only boost healing (positive values)
            if (changeValue > 0f)
            {
                changeValue *= multiplier;
            }
        }

        /// <summary>
        /// Check if a card is an injection card (temporary stat buff)
        /// </summary>
        public static bool IsInjectionCard(string cardId)
        {
            return cardId == "86cafd8b-9e28-4fd1-9e44-4ccdabb00137" || // Inject Brutadyne
                   cardId == "82a8cd80-af72-4785-b4c9-eab1a498a125" || // Inject Kinetra
                   cardId == "b51454f9-5641-4b07-94bf-93312555e860";   // Inject Optivex
        }

        /// <summary>
        /// Check if a card is a heal item card
        /// </summary>
        public static bool IsHealItemCard(string cardId)
        {
            return cardId == "6569d382-07ef-4db5-86ac-bac9eb249889" || // Inject Health Stim
                   cardId == "90793211-9445-4ac1-9d06-fc94e547b416";   // Apply First Aid Kit
        }

        /// <summary>
        /// Check if a card is an explosive card (grenades, mines)
        /// </summary>
        public static bool IsExplosiveCard(string cardId)
        {
            return cardId == "bfb700d8-5fa2-4bd0-b1dd-94842f66c031" || // Frag Grenade
                   cardId == "3b1ee954-9aec-45fe-afa0-46fbc9fc99a0" || // Flash Grenade
                   cardId == "8daa3d58-73aa-4c26-a20f-954686777d1f";   // Setup Mine
        }

        /// <summary>
        /// Shared helper to add bonus uses to action cards at mission start
        /// </summary>
        /// <param name="reinforcementName">Name of the reinforcement for logging</param>
        /// <param name="bonusUses">Number of bonus uses to add</param>
        /// <param name="shouldProcessCard">Function to check if a card should be processed</param>
        public static void AddBonusUsesAtMissionStart(string reinforcementName, int bonusUses, System.Func<string, bool> shouldProcessCard)
        {
            MelonLoader.MelonLogger.Msg($"[DEBUG] {reinforcementName}: Processing action cards");

            var gainedActionCards = InBattleActionCardsManager.Instance.InBattleActionCards;
            MelonLoader.MelonLogger.Msg($"[DEBUG] {reinforcementName}: Found {gainedActionCards.Count} total action cards");

            int processedCount = 0;
            foreach (var card in gainedActionCards)
            {
                if (card?.Info == null)
                {
                    MelonLoader.MelonLogger.Msg($"[DEBUG] {reinforcementName}: Skipping card with null Info");
                    continue;
                }

                string cardId = card.Info.Id;
                string cardName = card.Info.CardName;

                // Check if this card should be processed
                if (!shouldProcessCard(cardId))
                    continue;

                // Get current uses
                int currentUses = card.UsesLeft;

                // Add bonus uses (only if card has limited uses)
                if (currentUses > 0)
                {
                    int newUses = currentUses + bonusUses;
                    AccessTools.Field(typeof(ActionCard), "_usesLeft").SetValue(card, newUses);

                    MelonLoader.MelonLogger.Msg($"{reinforcementName}: Added +{bonusUses} use to {cardName} ({currentUses} -> {newUses} uses)");
                    processedCount++;
                }
            }

            MelonLoader.MelonLogger.Msg($"[DEBUG] {reinforcementName}: Processed {processedCount} cards");
        }
    }

    /// <summary>
    /// Patch to boost injection card duration (ChangeStat_Card)
    /// </summary>
    [HarmonyPatch(typeof(ChangeStat_Card), MethodType.Constructor, new[] { typeof(ActionCardInfo), typeof(IEnumerable<StatChange>), typeof(Enumerations.Team), typeof(float), typeof(bool) })]
    public static class Support_InjectionDuration_Patch
    {
        public static void Postfix(ChangeStat_Card __instance)
        {
            // Check if this is an injection card
            if (__instance?.Info == null)
                return;

            if (!SupportAffinityHelpers.IsInjectionCard(__instance.Info.Id))
                return;

            // Apply duration boost if any affinity is active
            SupportAffinityHelpers.ApplyInjectionDurationBoost(__instance);
        }
    }

    /// <summary>
    /// Patch to boost heal effectiveness (ChangeCurrentHealth_Card)
    /// </summary>
    [HarmonyPatch(typeof(ChangeCurrentHealth_Card), MethodType.Constructor, new[] { typeof(ActionCardInfo), typeof(float), typeof(Enumerations.Team) })]
    public static class Support_HealEffectiveness_Patch
    {
        public static void Postfix(ChangeCurrentHealth_Card __instance)
        {
            // Check if this is a heal item card
            if (__instance?.Info == null)
                return;

            if (!SupportAffinityHelpers.IsHealItemCard(__instance.Info.Id))
                return;

            // Apply heal effectiveness boost if any affinity is active
            SupportAffinityHelpers.ApplyHealEffectivenessBoost(__instance);
        }
    }

    /// <summary>
    /// Patch to add bonus uses to injection and heal item cards (for SupportAffinity6)
    /// </summary>
    [HarmonyPatch(typeof(TestGame), "StartGame")]
    public static class SupportAffinity6_BonusUses_Patch
    {
        public static void Postfix()
        {
            if (!SupportAffinity6.Instance.IsActive)
                return;

            // Add bonus uses to injection and heal item cards
            SupportAffinityHelpers.AddBonusUsesAtMissionStart(
                "SupportAffinity6",
                SupportAffinity6.BonusUses,
                (cardId) => SupportAffinityHelpers.IsInjectionCard(cardId) ||
                           SupportAffinityHelpers.IsHealItemCard(cardId)
            );
        }
    }

    /// <summary>
    /// Patch to boost heal amount for OverrideCommands_UnitAsTarget_Card (First Aid Kit)
    /// This card type uses a different mechanism where healing happens during ApplyCommand
    /// </summary>
    [HarmonyPatch(typeof(OverrideCommands_UnitAsTarget_Card), "ApplyCommand")]
    public static class Support_FirstAidKitHeal_Patch
    {
        public static void Prefix(OverrideCommands_UnitAsTarget_Card __instance)
        {
            // Check if this is the First Aid Kit card
            if (__instance?.Info?.Id != "90793211-9445-4ac1-9d06-fc94e547b416")
                return;

            // The First Aid Kit card heals 40 HP in its ApplyCommand
            // We need to modify the heal amount during the actual ApplyCommand call
            // This is handled by patching BattleUnit.Heal instead
        }
    }

    /// <summary>
    /// Patch BattleUnit.Heal to boost healing from First Aid Kit
    /// We track when First Aid Kit is being applied and boost the heal amount
    /// </summary>
    [HarmonyPatch(typeof(SpaceCommander.BattleUnit), "Heal")]
    public static class Support_BattleUnitHeal_Patch
    {
        private static bool _isProcessingFirstAidKit = false;
        private static float _originalHealAmount = 0f;

        public static void Prefix(ref float heal)
        {
            // Check if we should boost healing
            float multiplier = SupportAffinityHelpers.GetHealEffectivenessMultiplier();
            if (multiplier <= 1f)
                return;

            // Check if this is likely from First Aid Kit (40 HP is the signature amount)
            // We use a simple heuristic: if heal amount is 40, it's likely First Aid Kit
            if (heal == 40f && !_isProcessingFirstAidKit)
            {
                _isProcessingFirstAidKit = true;
                _originalHealAmount = heal;
                heal *= multiplier;
            }
        }

        public static void Postfix()
        {
            _isProcessingFirstAidKit = false;
            _originalHealAmount = 0f;
        }
    }
}
