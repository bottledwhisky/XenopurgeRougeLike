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
        public static float GetCritChance()
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
        public static bool RollCrit()
        {
            float critChance = GetCritChance();
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

        public static void MarkRangedAttackStart()
        {
            _isRangedAttack = true;
            // Roll for crit at the start of each bullet
            _shouldCrit = GunslingerAffinityHelpers.RollCrit();
        }

        public static void MarkRangedAttackEnd()
        {
            _isRangedAttack = false;
            _shouldCrit = false;
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
        public static void Prefix()
        {
            // Check if any Gunslinger affinity is active
            if (!GunslingerAffinity2.Instance.IsActive &&
                !GunslingerAffinity4.Instance?.IsActive == true &&
                !GunslingerAffinity6.Instance?.IsActive == true)
                return;

            GunslingerCritSystem.MarkRangedAttackStart();
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
