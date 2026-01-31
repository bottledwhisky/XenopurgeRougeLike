using HarmonyLib;
using SpaceCommander;
using SpaceCommander.Weapons;
using System.Reflection;
using static XenopurgeRougeLike.ModLocalization;

namespace XenopurgeRougeLike.GunslingerReinforcements
{
    // 突击训练：移除远程武器在非最优距离上的瞄准惩罚
    // Assault Training: Remove accuracy penalties on ranged weapons at non-optimal distances
    public class AssaultTraining : Reinforcement
    {
        public AssaultTraining()
        {
            company = Company.Gunslinger;
            rarity = Rarity.Standard;
            stackable = false;
            maxStacks = 1;
            name = L("gunslinger.assault_training.name");
            flavourText = L("gunslinger.assault_training.flavour");
            description = L("gunslinger.assault_training.description");
        }

        protected static AssaultTraining _instance;
        public static AssaultTraining Instance => _instance ??= new();
    }

    /// <summary>
    /// Patch to remove accuracy penalties from ranged weapons at non-optimal distances.
    /// This modifies the _accuracyModifierPerTile array by setting all negative values to 0.
    /// </summary>
    [HarmonyPatch(typeof(RangedWeapon), MethodType.Constructor, new[] { typeof(RangedWeaponDataSO), typeof(float), typeof(float), typeof(BattleUnit) })]
    public static class AssaultTraining_RangedWeapon_Constructor_Patch
    {
        // Cached field accessor for the private _accuracyModifierPerTile field
        private static readonly FieldInfo _accuracyModifierPerTileField = AccessTools.Field(typeof(RangedWeapon), "_accuracyModifierPerTile");
        // Cached field accessor for the private _battleUnit field
        private static readonly FieldInfo _battleUnitField = AccessTools.Field(typeof(RangedWeapon), "_battleUnit");

        public static void Postfix(RangedWeapon __instance)
        {
            if (!AssaultTraining.Instance.IsActive)
                return;

            // Get the battle unit that owns this weapon
            BattleUnit battleUnit = _battleUnitField.GetValue(__instance) as BattleUnit;

            // Only apply to player units
            if (battleUnit == null || battleUnit.Team != Enumerations.Team.Player)
                return;

            // Get the current accuracy modifier array
            float[] accuracyModifiers = _accuracyModifierPerTileField.GetValue(__instance) as float[];

            if (accuracyModifiers == null || accuracyModifiers.Length == 0)
                return;

            // Create a new array with penalties removed (all negative values set to 0)
            float[] modifiedAccuracyModifiers = new float[accuracyModifiers.Length];
            for (int i = 0; i < accuracyModifiers.Length; i++)
            {
                // Keep positive bonuses, remove negative penalties
                modifiedAccuracyModifiers[i] = accuracyModifiers[i] < 0 ? 0f : accuracyModifiers[i];
            }

            // Set the modified array back
            _accuracyModifierPerTileField.SetValue(__instance, modifiedAccuracyModifiers);
        }
    }
}
