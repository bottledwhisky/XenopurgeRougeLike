using HarmonyLib;
using SpaceCommander;
using SpaceCommander.Commands;
using System.Reflection;
using static XenopurgeRougeLike.ModLocalization;

namespace XenopurgeRougeLike.ScavengerReinforcements
{
    // 匕首双持：使用匕首类武器时，近战伤害x2
    // Dual Daggers: When using dagger weapons, melee damage x2
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
    /// Patch to double melee damage for daggers
    /// </summary>
    [HarmonyPatch(typeof(Melee), "Attack", MethodType.Normal)]
    public static class DualDaggers_DoubleDamage_Patch
    {
        private static readonly FieldInfo _damageField = AccessTools.Field(typeof(Melee), "_damage");
        private static readonly FieldInfo _battleUnitField = AccessTools.Field(typeof(Melee), "_battleUnit");

        public static void Prefix(Melee __instance)
        {
            if (!DualDaggers.Instance.IsActive)
                return;

            // Check if this unit is using a dagger
            BattleUnit battleUnit = _battleUnitField.GetValue(__instance) as BattleUnit;
            if (battleUnit == null || !WeaponCategories.IsUsingDagger(battleUnit))
                return;

            // Double the damage
            float currentDamage = (float)_damageField.GetValue(__instance);
            _damageField.SetValue(__instance, currentDamage * 2f);
        }

        public static void Postfix(Melee __instance)
        {
            if (!DualDaggers.Instance.IsActive)
                return;

            // Check if this unit is using a dagger
            BattleUnit battleUnit = _battleUnitField.GetValue(__instance) as BattleUnit;
            if (battleUnit == null || !WeaponCategories.IsUsingDagger(battleUnit))
                return;

            // Restore the damage to normal
            float currentDamage = (float)_damageField.GetValue(__instance);
            _damageField.SetValue(__instance, currentDamage / 2f);
        }
    }
}
