using HarmonyLib;
using SpaceCommander;
using SpaceCommander.Weapons;
using System;
using static XenopurgeRougeLike.ModLocalization;

namespace XenopurgeRougeLike.GunslingerReinforcements
{
    // 枪手路径天赋2：+20瞄准，解锁暴击机制（10%基础概率，100%基础额外伤害）
    // Gunslinger Affinity 2: +20 accuracy, unlock crit mechanic (10% base chance, 100% base extra damage)
    public class GunslingerAffinity2 : CompanyAffinity
    {
        public const float AccuracyBonus = .2f;
        public const float CritChance = 0.10f;
        public const float CritDamageMultiplier = 1.0f; // 100% extra damage = 2x total damage

        public GunslingerAffinity2()
        {
            unlockLevel = 2;
            company = Company.Gunslinger;
            description = L("gunslinger.affinity2.description", (int)(AccuracyBonus * 100), (int)(CritChance * 100), (int)(CritDamageMultiplier * 100));
        }

        public static GunslingerAffinity2 _instance;
        public static GunslingerAffinity2 Instance => _instance ??= new();

        public override void OnActivate()
        {
            // Register accuracy boost
            UnitStatsTools.InBattleUnitStatChanges["GunslingerAffinity2_Accuracy"] = new UnitStatChange(
                Enumerations.UnitStats.Accuracy,
                AccuracyBonus,
                Enumerations.Team.Player
            );
        }

        public override void OnDeactivate()
        {
            // Remove accuracy boost
            UnitStatsTools.InBattleUnitStatChanges.Remove("GunslingerAffinity2_Accuracy");
        }
    }

    /// <summary>
    /// Shared helpers for Gunslinger affinity patches
    /// </summary>
    public static class GunslingerAffinityHelpers
    {
        /// <summary>
        /// Get the highest active crit chance from all Gunslinger affinities
        /// </summary>
        /// <param name="shootingUnit">The unit performing the attack (optional, needed for accuracy-to-crit conversion)</param>
        public static float GetCritChance(BattleUnit shootingUnit = null)
        {
            float critChance = 0f;

            if (GunslingerAffinity2.Instance.IsActive)
                critChance = GunslingerAffinity2.CritChance;

            // Higher level affinities will override
            else if (GunslingerAffinity4.Instance.IsActive)
                critChance = GunslingerAffinity4.CritChance;

            else if (GunslingerAffinity6.Instance.IsActive)
                critChance = GunslingerAffinity6.CritChance;

            // Add TargetingWeakspots bonus
            if (TargetingWeakspots.Instance.IsActive)
                critChance += TargetingWeakspots.CritChanceBonus;

            // Gunslinger Affinity 6: Convert accuracy above 120 to crit chance
            if (GunslingerAffinity6.Instance.IsActive && shootingUnit != null && shootingUnit.Team == Enumerations.Team.Player)
            {
                float accuracy = shootingUnit.Accuracy * 100f; // Convert to x100 scale
                if (accuracy > 120f)
                {
                    float excessAccuracy = accuracy - 120f;
                    critChance += excessAccuracy / 100f; // Convert back to 0-1 scale for crit chance
                }
            }

            return critChance;
        }

        /// <summary>
        /// Get the highest active crit damage multiplier from all Gunslinger affinities
        /// </summary>
        public static float GetCritDamageMultiplier()
        {
            float critMultiplier = 0f;

            if (GunslingerAffinity2.Instance.IsActive)
                critMultiplier = GunslingerAffinity2.CritDamageMultiplier;

            // Higher level affinities will override
            else if (GunslingerAffinity4.Instance.IsActive)
                critMultiplier = GunslingerAffinity4.CritDamageMultiplier;

            else if (GunslingerAffinity6.Instance.IsActive)
                critMultiplier = GunslingerAffinity6.CritDamageMultiplier;

            return critMultiplier;
        }

        /// <summary>
        /// Roll for critical hit
        /// </summary>
        /// <param name="shootingUnit">The unit performing the attack (optional, needed for accuracy-to-crit conversion)</param>
        public static bool RollCrit(BattleUnit shootingUnit = null)
        {
            float critChance = GetCritChance(shootingUnit);
            if (critChance <= 0f)
                return false;

            return UnityEngine.Random.value < critChance;
        }
    }

    /// <summary>
    /// System to track ongoing ranged attacks for crit calculation
    /// </summary>
    public static class GunslingerCritSystem
    {
        // Track if current attack is a ranged attack
        [ThreadStatic]
        private static bool _isRangedAttack = false;

        // Track if current bullet should crit
        [ThreadStatic]
        private static bool _shouldCrit = false;

        // Track the shooting unit for accuracy-to-crit conversion
        [ThreadStatic]
        private static BattleUnit _shootingUnit = null;

        public static void MarkRangedAttackStart(BattleUnit shootingUnit)
        {
            _isRangedAttack = true;
            _shootingUnit = shootingUnit;

            // Check if Death's Eye is active - if so, guarantee crit
            if (DeathsEye.Instance.IsActive && DeathsEye.IsBuffActive)
            {
                _shouldCrit = true;
            }
            else
            {
                // Roll for crit at the start of each bullet, passing the shooting unit for accuracy-to-crit conversion
                _shouldCrit = GunslingerAffinityHelpers.RollCrit(shootingUnit);
            }
        }

        public static void MarkRangedAttackEnd()
        {
            _isRangedAttack = false;
            _shouldCrit = false;
            _shootingUnit = null;
        }

        public static bool IsRangedAttack()
        {
            return _isRangedAttack;
        }

        public static bool ShouldCrit()
        {
            return _shouldCrit;
        }
    }

    /// <summary>
    /// Patch RangedWeapon.ShootOneBullet to mark ranged attacks (Prefix)
    /// </summary>
    [HarmonyPatch(typeof(RangedWeapon), "ShootOneBullet")]
    public static class GunslingerAffinity2_MarkRangedAttack_Prefix
    {
        public static void Prefix(RangedWeapon __instance)
        {
            // Check if any Gunslinger affinity is active
            if (!GunslingerAffinity2.Instance.IsActive &&
                !GunslingerAffinity4.Instance?.IsActive == true &&
                !GunslingerAffinity6.Instance?.IsActive == true)
                return;

            // Get the shooting unit from the RangedWeapon instance
            BattleUnit shootingUnit = __instance.GetType().GetField("_battleUnit",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.GetValue(__instance) as BattleUnit;

            GunslingerCritSystem.MarkRangedAttackStart(shootingUnit);
        }
    }

    /// <summary>
    /// Patch RangedWeapon.ShootOneBullet to unmark ranged attacks (Postfix)
    /// </summary>
    [HarmonyPatch(typeof(RangedWeapon), "ShootOneBullet")]
    public static class GunslingerAffinity2_UnmarkRangedAttack_Postfix
    {
        public static void Postfix()
        {
            // Check if any Gunslinger affinity is active
            if (!GunslingerAffinity2.Instance.IsActive &&
                !GunslingerAffinity4.Instance?.IsActive == true &&
                !GunslingerAffinity6.Instance?.IsActive == true)
                return;

            GunslingerCritSystem.MarkRangedAttackEnd();
        }
    }

    /// <summary>
    /// Patch BattleUnit.Damage to apply critical hit bonus (Prefix)
    /// </summary>
    [HarmonyPatch(typeof(BattleUnit), "Damage")]
    public static class GunslingerAffinity2_CritDamage_Patch
    {
        public static void Prefix(ref float damage)
        {
            // Check if any Gunslinger affinity is active
            if (!GunslingerAffinity2.Instance.IsActive &&
                !GunslingerAffinity4.Instance?.IsActive == true &&
                !GunslingerAffinity6.Instance?.IsActive == true)
                return;

            // Only apply crit during ranged attacks
            if (!GunslingerCritSystem.IsRangedAttack())
                return;

            // Check if this bullet should crit
            if (!GunslingerCritSystem.ShouldCrit())
                return;

            // Apply crit damage multiplier
            float critMultiplier = GunslingerAffinityHelpers.GetCritDamageMultiplier();
            damage *= (1f + critMultiplier);
        }
    }
}
