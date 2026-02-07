using HarmonyLib;
using SpaceCommander;
using SpaceCommander.Weapons;
using System.Collections.Generic;
using static XenopurgeRougeLike.ModLocalization;

namespace XenopurgeRougeLike.EngineerReinforcements
{
    // 重型炮台：炮台免疫远程伤害，炮台准确率+15
    // Heavy Turret: Turrets are immune to ranged damage, turret accuracy +15
    public class HeavyTurret : Reinforcement
    {
        public static readonly float AccuracyBonus = .15f;

        public HeavyTurret()
        {
            company = Company.Engineer;
            rarity = Rarity.Elite;
            stackable = false;
            name = L("engineer.heavy_turret.name");
            description = L("engineer.heavy_turret.description", (int)(AccuracyBonus * 100));
            flavourText = L("engineer.heavy_turret.flavour");
        }

        protected static HeavyTurret instance;
        public static HeavyTurret Instance => instance ??= new();
    }

    /// <summary>
    /// Patch to make turrets immune to ranged damage
    /// </summary>
    [HarmonyPatch(typeof(BattleUnit), "Damage")]
    public static class HeavyTurret_RangedImmunity_Patch
    {
        // Track which units are currently being shot by ranged weapons
        private static readonly HashSet<BattleUnit> _unitsBeingShotByRangedWeapon = [];

        public static void Prefix(BattleUnit __instance, ref float damage)
        {
            if (!HeavyTurret.Instance.IsActive)
                return;

            // Check if the unit being damaged is a turret
            if (__instance.UnitTag != Enumerations.UnitTag.Turret)
                return;

            // Check if this damage is from a ranged weapon
            if (_unitsBeingShotByRangedWeapon.Contains(__instance))
            {
                // Make turret immune to ranged damage
                damage = 0f;
            }
        }

        /// <summary>
        /// Mark when a unit is being shot by a ranged weapon
        /// </summary>
        [HarmonyPatch(typeof(RangedWeapon), "ShootOneBullet")]
        public static class RangedWeapon_ShootOneBullet_Patch
        {
            private static readonly AccessTools.FieldRef<RangedWeapon, IDamagable> _targetRef =
                AccessTools.FieldRefAccess<RangedWeapon, IDamagable>("_target");

            public static void Prefix(RangedWeapon __instance)
            {
                if (!HeavyTurret.Instance.IsActive)
                    return;

                IDamagable target = _targetRef(__instance);
                if (target is BattleUnit battleUnit)
                {
                    _unitsBeingShotByRangedWeapon.Add(battleUnit);
                }
            }

            public static void Postfix(RangedWeapon __instance)
            {
                if (!HeavyTurret.Instance.IsActive)
                    return;

                IDamagable target = _targetRef(__instance);
                if (target is BattleUnit battleUnit)
                {
                    _unitsBeingShotByRangedWeapon.Remove(battleUnit);
                }
            }
        }
    }

    /// <summary>
    /// Patch to increase turret accuracy when the BattleUnit is created
    /// </summary>
    [HarmonyPatch(typeof(Turret), "PlaceTurretOnTile")]
    public static class HeavyTurret_IncreaseAccuracy_Patch
    {
        public static void Postfix(Turret __instance)
        {
            if (!HeavyTurret.Instance.IsActive)
                return;

            // Get the turret's BattleUnit
            BattleUnit turretUnit = __instance.BattleUnitCreated;
            if (turretUnit == null)
                return;

            // Increase turret accuracy
            turretUnit.ChangeStat(
                Enumerations.UnitStats.Accuracy,
                HeavyTurret.AccuracyBonus,
                "HeavyTurret_AccuracyBonus"
            );
        }
    }
}
