using HarmonyLib;
using SpaceCommander;
using SpaceCommander.Commands;
using System.Reflection;
using static XenopurgeRougeLike.ModLocalization;

namespace XenopurgeRougeLike.ScavengerReinforcements
{
    // 匕首双持：使用匕首类武器时，近战攻击次数+1
    // Dual Daggers: When using dagger weapons, melee attack count +1
    public class DualDaggers : Reinforcement
    {
        public DualDaggers()
        {
            company = Company.Scavenger;
            rarity = Rarity.Expert;
            stackable = false;
            name = L("scavenger.dual_daggers.name");
            description = L("scavenger.dual_daggers.description");
            flavourText = L("scavenger.dual_daggers.flavour");
        }

        protected static DualDaggers instance;
        public static DualDaggers Instance => instance ??= new();
    }

    /// <summary>
    /// Patch to make daggers attack twice by calling Attack twice
    /// </summary>
    [HarmonyPatch(typeof(Melee), "Attack")]
    public static class DualDaggers_DoubleAttack_Patch
    {
        private static readonly MethodInfo _attackMethod = AccessTools.Method(typeof(Melee), "Attack");
        private static readonly FieldInfo _battleUnitField = AccessTools.Field(typeof(Melee), "_battleUnit");

        private static bool _isRecursiveCall = false;

        public static bool Prefix(Melee __instance)
        {
            // Prevent infinite recursion
            if (_isRecursiveCall)
                return true;

            if (!DualDaggers.Instance.IsActive)
                return true; // Run original method

            // Check if this unit is using a dagger
            BattleUnit battleUnit = _battleUnitField.GetValue(__instance) as BattleUnit;
            if (battleUnit == null || !WeaponCategories.IsUsingDagger(battleUnit))
                return true; // Not using a dagger, run original method

            // Call Attack twice
            _isRecursiveCall = true;
            try
            {
                _attackMethod.Invoke(__instance, null); // First attack
                _attackMethod.Invoke(__instance, null); // Second attack
            }
            finally
            {
                _isRecursiveCall = false;
            }

            return false; // Skip original method
        }
    }
}
