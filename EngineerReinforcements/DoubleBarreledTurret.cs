using HarmonyLib;
using SpaceCommander;
using SpaceCommander.Weapons;
using System.Reflection;

namespace XenopurgeRougeLike.EngineerReinforcements
{
    // 双联发炮台：炮台每次发射双倍的投射物
    // Double-Barreled Turret: Turrets fire double projectiles each shot
    public class DoubleBarreledTurret : Reinforcement
    {
        public DoubleBarreledTurret()
        {
            company = Company.Engineer;
            rarity = Rarity.Expert;
            stackable = false;
            name = "Double-Barreled Turret";
            description = "Turrets fire double projectiles each shot.";
            flavourText = "Twin-linked barrels synchronized to fire in perfect unison, doubling your suppressive firepower.";
        }

        protected static DoubleBarreledTurret instance;
        public static DoubleBarreledTurret Instance => instance ??= new();
    }

    /// <summary>
    /// Patch to make turrets fire double projectiles by calling Shoot twice
    /// </summary>
    [HarmonyPatch(typeof(RangedWeapon), "Shoot")]
    public static class DoubleBarreledTurret_DoubleProjectiles_Patch
    {
        private static readonly FieldInfo _battleUnit = AccessTools.Field(typeof(RangedWeapon), "_battleUnit");
        private static readonly MethodInfo _shootMethod = AccessTools.Method(typeof(RangedWeapon), "Shoot");

        private static bool _isRecursiveCall = false;

        public static bool Prefix(RangedWeapon __instance)
        {
            // Prevent infinite recursion
            if (_isRecursiveCall)
                return true;

            if (!DoubleBarreledTurret.Instance.IsActive)
                return true; // Run original method

            // Check if this weapon belongs to a turret
            BattleUnit owner = _battleUnit.GetValue(__instance) as BattleUnit;
            if (owner == null || owner.UnitTag != SpaceCommander.Enumerations.UnitTag.Turret)
                return true; // Not a turret, run original method

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
