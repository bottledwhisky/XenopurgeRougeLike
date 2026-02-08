namespace XenopurgeRougeLike
{
    /// <summary>
    /// Centralized storage for all action card IDs used in the mod.
    /// This prevents duplicate hard-coded IDs scattered across multiple files.
    /// </summary>
    public static class ActionCardIds
    {
        // === Injection Cards (Temporary Stat Buffs) ===
        public const string INJECT_BRUTADYNE = "86cafd8b-9e28-4fd1-9e44-4ccdabb00137";  // Inject Brutadyne (BuffPower)
        public const string INJECT_KINETRA = "82a8cd80-af72-4785-b4c9-eab1a498a125";    // Inject Kinetra (BuffSpeed)
        public const string INJECT_OPTIVEX = "b51454f9-5641-4b07-94bf-93312555e860";    // Inject Optivex (BuffAccuracy)

        // === Heal Cards ===
        public const string INJECT_HEALTH_STIM = "6569d382-07ef-4db5-86ac-bac9eb249889"; // Inject Health Stim (Heal)
        public const string APPLY_FIRST_AID_KIT = "90793211-9445-4ac1-9d06-fc94e547b416"; // Apply First Aid Kit (FirstAidKit)

        // === Explosive Cards ===
        public const string FRAG_GRENADE = "bfb700d8-5fa2-4bd0-b1dd-94842f66c031";      // Frag Grenade
        public const string FLASH_GRENADE = "3b1ee954-9aec-45fe-afa0-46fbc9fc99a0";     // Flash Grenade
        public const string SETUP_MINE = "8daa3d58-73aa-4c26-a20f-954686777d1f";        // Setup Mine

        // === Turret Cards ===
        public const string SETUP_TURRET_RAT = "3e9b1bb6-b377-49cd-af43-9c10dee7e81c";  // Setup Turret (RAT)
        public const string SETUP_TURRET_BANG = "8b9dc11f-7e75-4295-92b8-1eb9417896f6"; // Setup Turret (BANG)

        // === Category Helper Methods ===

        /// <summary>
        /// Check if a card is an injection card (temporary stat buff)
        /// </summary>
        public static bool IsInjectionCard(string cardId)
        {
            return cardId == INJECT_BRUTADYNE ||
                   cardId == INJECT_KINETRA ||
                   cardId == INJECT_OPTIVEX;
        }

        /// <summary>
        /// Check if a card is a heal item card
        /// </summary>
        public static bool IsHealItemCard(string cardId)
        {
            return cardId == INJECT_HEALTH_STIM ||
                   cardId == APPLY_FIRST_AID_KIT;
        }

        /// <summary>
        /// Check if a card is an explosive card (grenades, mines)
        /// </summary>
        public static bool IsExplosiveCard(string cardId)
        {
            return cardId == FRAG_GRENADE ||
                   cardId == FLASH_GRENADE ||
                   cardId == SETUP_MINE;
        }

        /// <summary>
        /// Check if a card is a turret setup card
        /// </summary>
        public static bool IsTurretCard(string cardId)
        {
            return cardId == SETUP_TURRET_RAT ||
                   cardId == SETUP_TURRET_BANG;
        }
    }
}
