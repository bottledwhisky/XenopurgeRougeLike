using HarmonyLib;
using SpaceCommander.Weapons;
using System.Collections.Generic;
using System.Reflection;
using static XenopurgeRougeLike.ModLocalization;

namespace XenopurgeRougeLike.ScavengerReinforcements
{
    // 手枪双持：使用手枪类武器时，每次射击会发射两次
    // Dual Pistols: When using pistol weapons, each shot fires twice
    public class DualPistols : Reinforcement
    {
        public DualPistols()
        {
            company = Company.Scavenger;
            rarity = Rarity.Expert;
            stackable = false;
            name = L("scavenger.dual_pistols.name");
            description = L("scavenger.dual_pistols.description");
            flavourText = L("scavenger.dual_pistols.flavour");
        }

        protected static DualPistols instance;
        public static DualPistols Instance => instance ??= new();
    }

    /// <summary>
    /// Patch to make pistols fire double shots by calling Shoot twice
    /// </summary>
    [HarmonyPatch(typeof(RangedWeapon), "Shoot")]
    public static class DualPistols_DoubleShot_Patch
    {
        private static readonly FieldInfo _weaponIdField = AccessTools.Field(typeof(RangedWeapon), "_weaponId");
        private static readonly MethodInfo _shootMethod = AccessTools.Method(typeof(RangedWeapon), "Shoot");

        private static bool _isRecursiveCall = false;

        // Pistol IDs - same as in WeaponCategories
        private static readonly HashSet<string> PistolIds = new()
        {
            WeaponCategories.MOP_PISTOL,
            WeaponCategories.HEX_PISTOL,
            WeaponCategories.BOLT_SMG,
        };

        public static bool Prefix(RangedWeapon __instance)
        {
            // Prevent infinite recursion
            if (_isRecursiveCall)
                return true;

            if (!DualPistols.Instance.IsActive)
                return true; // Run original method

            // Check if this weapon is a pistol by ID
            string weaponId = _weaponIdField?.GetValue(__instance) as string;
            if (string.IsNullOrEmpty(weaponId) || !PistolIds.Contains(weaponId))
                return true; // Not a pistol, run original method

            // Call Shoot twice
            _isRecursiveCall = true;
            try
            {
                _shootMethod.Invoke(__instance, null); // First shot
                _shootMethod.Invoke(__instance, null); // Second shot
            }
            finally
            {
                _isRecursiveCall = false;
            }

            return false; // Skip original method
        }
    }
}
