using System;
using HarmonyLib;
using MelonLoader;
using SpaceCommander;
using static XenopurgeRougeLike.ModLocalization;

namespace XenopurgeRougeLike.SupportReinforcements
{
    /// <summary>
    /// 药物过量：溢出的治疗量会变成护甲
    /// Overdose: Overflow healing is converted into armor
    /// </summary>
    public class Overdose : Reinforcement
    {
        public Overdose()
        {
            company = Company.Support;
            rarity = Rarity.Expert;
            name = L("support.overdose.name");
            description = L("support.overdose.description");
            flavourText = L("support.overdose.flavour");
        }

        public override string GetDescriptionForStacks(int stacks)
        {
            return L("support.overdose.description");
        }

        private static Overdose _instance;
        public static Overdose Instance => _instance ??= new();
    }

    /// <summary>
    /// Patch BattleUnit.Heal to convert overflow healing into armor
    /// </summary>
    [HarmonyPatch(typeof(BattleUnit), "Heal")]
    public static class Overdose_Heal_Patch
    {
        public static bool Prefix(BattleUnit __instance, float heal)
        {
            if (!Overdose.Instance.IsActive)
                return true; // Run original method

            if (!__instance.IsAlive)
                return true; // Run original method

            // Get current health and max health
            float currentHealth = __instance.CurrentHealth;
            float maxHealth = __instance.CurrentMaxHealth;

            // Calculate the new health after healing
            float newHealth = currentHealth + heal;

            // Calculate overflow (healing beyond max HP)
            float overflow = 0f;
            if (newHealth > maxHealth)
            {
                overflow = newHealth - maxHealth;
                newHealth = maxHealth;
            }

            // Apply the healing (capped at max health)
            float actualHeal = newHealth - currentHealth;
            if (actualHeal > 0)
            {
                // Use reflection to set health directly since we're replacing the method
                var healthField = AccessTools.Field(typeof(BattleUnit), "_currentHealth");
                healthField.SetValue(__instance, newHealth);

                // Trigger health changed event
                var onHealthChanged = AccessTools.Field(typeof(BattleUnit), "OnHealthChanged");
                var healthChangedAction = onHealthChanged.GetValue(__instance) as System.Action<float>;
                healthChangedAction?.Invoke(newHealth);
            }

            // Convert overflow to armor
            if (overflow > 0)
            {
                overflow = (float)Math.Floor(overflow);
                UnitStatsTools.AddArmorToUnit(__instance, overflow);
                MelonLogger.Msg($"Overdose: {__instance.UnitName} received {actualHeal:F1} healing and {overflow:F1} armor from overflow healing");
            }

            return false; // Skip original method
        }
    }
}
