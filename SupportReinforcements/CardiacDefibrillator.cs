using HarmonyLib;
using SpaceCommander;
using System.Collections.Generic;
using UnityEngine;
using static XenopurgeRougeLike.ModLocalization;

namespace XenopurgeRougeLike.SupportReinforcements
{
    /// <summary>
    /// 心脏起搏器：受到致命伤害时，立刻恢复至50%血量，一场战斗仅限一次
    /// Cardiac Defibrillator: When taking lethal damage, immediately restore to 50% HP, once per battle
    /// </summary>
    public class CardiacDefibrillator : Reinforcement
    {
        public const float HealthRestorePercent = 0.5f;

        // Track which units have already used the defibrillator this battle
        private static HashSet<BattleUnit> _usedUnits = [];

        public CardiacDefibrillator()
        {
            company = Company.Support;
            rarity = Rarity.Elite;
            stackable = false;
            name = L("support.cardiac_defibrillator.name");
            description = L("support.cardiac_defibrillator.description");
            flavourText = L("support.cardiac_defibrillator.flavour");
        }

        public override void OnActivate()
        {
            // Patches are automatically applied when Harmony is initialized
        }

        public override void OnDeactivate()
        {
            // Clear tracking data
            _usedUnits.Clear();
        }

        /// <summary>
        /// Check if unit can use defibrillator and mark as used
        /// </summary>
        public static bool TryConsumeUse(BattleUnit unit)
        {
            if (!Instance.IsActive)
                return false;

            if (!_usedUnits.Contains(unit))
            {
                _usedUnits.Add(unit);
                Debug.Log($"CardiacDefibrillator: {unit.UnitName} revived to 50% HP!");
                return true;
            }

            return false;
        }

        protected static CardiacDefibrillator instance;
        public static CardiacDefibrillator Instance => instance ??= new();
    }

    /// <summary>
    /// Prevent lethal damage and restore unit to 50% HP
    /// Higher priority than Unyielding to execute first
    /// </summary>
    [HarmonyPatch(typeof(BattleUnit), "Damage")]
    [HarmonyPriority(Priority.High)]
    public static class CardiacDefibrillator_PreventLethalDamage_Patch
    {
        public static void Prefix(BattleUnit __instance, ref float damage)
        {
            if (!CardiacDefibrillator.Instance.IsActive)
                return;

            // Only apply to player team units
            if (__instance.Team != Enumerations.Team.Player)
                return;

            // Check if this damage would be lethal
            float currentHealth = __instance.CurrentHealth;
            if (currentHealth - damage <= 0f && currentHealth > 0f)
            {
                // Try to consume defibrillator use
                if (CardiacDefibrillator.TryConsumeUse(__instance))
                {
                    // Calculate target health (50% of max HP)
                    float targetHealth = __instance.CurrentMaxHealth * CardiacDefibrillator.HealthRestorePercent;

                    // Reduce damage so unit ends up at 50% HP
                    // If current health is already below target, this will "heal" by reducing damage
                    damage = currentHealth - targetHealth;

                    Debug.Log($"CardiacDefibrillator: {__instance.UnitName} saved from death!");
                }
            }
        }
    }
}
