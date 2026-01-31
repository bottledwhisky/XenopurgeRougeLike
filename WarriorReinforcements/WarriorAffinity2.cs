using HarmonyLib;
using SpaceCommander;
using SpaceCommander.Weapons;
using System;
using System.Collections.Generic;
using System.Reflection;
using static XenopurgeRougeLike.ModLocalization;

namespace XenopurgeRougeLike.WarriorReinforcements
{
    // 勇士路径天赋2：近战伤害+2，霰弹枪每一枪投射物数量+1
    // Warrior Affinity 2: Melee damage +2, shotguns fire +1 projectile per shot
    public class WarriorAffinity2 : CompanyAffinity
    {
        public const int MeleeDamageBonus = 2;
        public const int ShotgunProjectileBonus = 1;

        public WarriorAffinity2()
        {
            unlockLevel = 2;
            company = Company.Warrior;
            description = L("warrior.affinity2.description", MeleeDamageBonus, ShotgunProjectileBonus);
        }

        public static WarriorAffinity2 _instance;
        public static WarriorAffinity2 Instance => _instance ??= new();

        public override void OnActivate()
        {
            // Register melee damage boost
            WarriorAffinityHelpers.RegisterMeleeDamageBoost("WarriorAffinity2_MeleeDamage", MeleeDamageBonus);
        }

        public override void OnDeactivate()
        {
            // Remove melee damage boost
            WarriorAffinityHelpers.RemoveMeleeDamageBoost("WarriorAffinity2_MeleeDamage");
        }
    }

    /// <summary>
    /// Shared helpers for Warrior affinity patches
    /// </summary>
    public static class WarriorAffinityHelpers
    {
        // Cached field accessor for _firingMode in RangedWeapon
        private static readonly FieldInfo _firingModeField =
            AccessTools.Field(typeof(RangedWeapon), "_firingMode");

        // Cached field accessor for _burstShotsFiringModeDataSO in BurstShotsFiringMode
        private static readonly FieldInfo _burstDataField =
            AccessTools.Field(typeof(BurstShotsFiringMode), "_burstShotsFiringModeDataSO");

        // Cached field accessor for _bulletsPerBurst in BurstShotsFiringModeDataSO
        private static readonly FieldInfo _bulletsPerBurstField =
            AccessTools.Field(typeof(BurstShotsFiringModeDataSO), "_bulletsPerBurst");

        /// <summary>
        /// Get the total projectile bonus for shotguns from all active Warrior affinities
        /// </summary>
        public static int GetShotgunProjectileBonus()
        {
            int bonus = 0;

            if (WarriorAffinity2.Instance.IsActive)
                bonus += WarriorAffinity2.ShotgunProjectileBonus;

            if (WarriorAffinity4.Instance.IsActive)
                bonus += WarriorAffinity4.ShotgunProjectileBonus;

            if (WarriorAffinity6.Instance.IsActive)
                bonus += WarriorAffinity6.ShotgunProjectileBonus;

            return bonus;
        }

        /// <summary>
        /// Check if a weapon is a shotgun (BulletsPerBurst > 1)
        /// </summary>
        public static bool IsShotgun(IFiringMode firingMode)
        {
            return firingMode != null && firingMode.BulletsPerBurst > 1;
        }

        /// <summary>
        /// Apply shotgun projectile bonus to a ranged weapon
        /// </summary>
        public static void ApplyShotgunProjectileBonus(RangedWeapon weapon)
        {
            int bonus = GetShotgunProjectileBonus();
            if (bonus <= 0)
                return;

            // Get the firing mode
            IFiringMode firingMode = _firingModeField.GetValue(weapon) as IFiringMode;
            if (firingMode == null || !IsShotgun(firingMode))
                return;

            // Only modify BurstShotsFiringMode (shotguns use this mode)
            if (firingMode is BurstShotsFiringMode burstMode)
            {
                // Get the data SO from the firing mode
                BurstShotsFiringModeDataSO dataSO = _burstDataField.GetValue(burstMode) as BurstShotsFiringModeDataSO;
                if (dataSO != null)
                {
                    // Modify the bullets per burst directly
                    int originalBullets = dataSO.BulletsPerBurst;
                    _bulletsPerBurstField.SetValue(dataSO, originalBullets + bonus);
                }
            }
        }

        /// <summary>
        /// Register melee damage boost for a Warrior affinity
        /// </summary>
        public static void RegisterMeleeDamageBoost(string key, int damageBonus)
        {
            UnitStatsTools.InBattleUnitStatChanges[key] = new UnitStatChange(
                Enumerations.UnitStats.Power,
                damageBonus,
                Enumerations.Team.Player
            );
        }

        /// <summary>
        /// Remove melee damage boost for a Warrior affinity
        /// </summary>
        public static void RemoveMeleeDamageBoost(string key)
        {
            UnitStatsTools.InBattleUnitStatChanges.Remove(key);
        }

        /// <summary>
        /// Modify reinforcement weights to increase Warrior company probability
        /// </summary>
        public static void ModifyReinforcementWeights(List<Tuple<int, Reinforcement>> choices, float multiplier)
        {
            for (int i = 0; i < choices.Count; i++)
            {
                // Check if this reinforcement belongs to Warrior company
                if (choices[i].Item2.company.Type == CompanyType.Warrior)
                {
                    int newWeight = (int)(choices[i].Item1 * multiplier);
                    choices[i] = new Tuple<int, Reinforcement>(newWeight, choices[i].Item2);
                }
            }
        }
    }

    /// <summary>
    /// Patch to boost shotgun projectile count when RangedWeapon is constructed
    /// </summary>
    [HarmonyPatch(typeof(RangedWeapon), MethodType.Constructor)]
    [HarmonyPatch(new[] { typeof(RangedWeaponDataSO), typeof(float), typeof(float), typeof(BattleUnit) })]
    public static class WarriorAffinity2_ShotgunProjectiles_Patch
    {
        public static void Postfix(RangedWeapon __instance, BattleUnit battleUnit)
        {
            // Only apply to player units
            if (battleUnit == null || battleUnit.Team != Enumerations.Team.Player)
                return;

            if (!WarriorAffinity2.Instance.IsActive)
                return;

            WarriorAffinityHelpers.ApplyShotgunProjectileBonus(__instance);
        }
    }
}
