using HarmonyLib;
using SpaceCommander;
using System;
using static XenopurgeRougeLike.ModLocalization;

namespace XenopurgeRougeLike.EngineerReinforcements
{
    // 高级弹鼓：炮台弹药量+50%/100%（可叠加2），炮台准确率+15
    public class AdvancedMagazine : Reinforcement
    {
        public static readonly float[] DurabilityMultiplier = [2.0f, 3.0f];
        public static readonly float AccuracyBonus = .15f;

        public AdvancedMagazine()
        {
            company = Company.Engineer;
            stackable = true;
            maxStacks = 2;
            name = L("engineer.advanced_magazine.name");
            flavourText = L("engineer.advanced_magazine.flavour");
        }

        public override string GetDescriptionForStacks(int stacks)
        {
            int durabilityPercent = (int)((DurabilityMultiplier[stacks - 1] - 1f) * 100);
            int accuracyBonus = (int)(AccuracyBonus * 100);
            return L("engineer.advanced_magazine.description", durabilityPercent, accuracyBonus);
        }

        protected static AdvancedMagazine instance;
        public static AdvancedMagazine Instance => instance ??= new();
    }

    /// <summary>
    /// Patch to increase turret durability when created
    /// </summary>
    [HarmonyPatch(typeof(Turret), MethodType.Constructor)]
    [HarmonyPatch([typeof(UnitDataSO), typeof(TurretDataSO)])]
    public static class AdvancedMagazine_IncreaseDurability_Patch
    {
        public static void Postfix(Turret __instance, TurretDataSO turretDataSO)
        {
            if (!AdvancedMagazine.Instance.IsActive)
                return;

            // Access private fields using reflection
            var durabilityField = AccessTools.Field(typeof(Turret), "_durability");
            var maxDurabilityField = AccessTools.Field(typeof(Turret), "_maxDurability");

            // Get current durability values
            float baseDurability = turretDataSO.Durability;
            float multiplier = AdvancedMagazine.DurabilityMultiplier[AdvancedMagazine.Instance.currentStacks - 1];
            float newDurability = baseDurability * multiplier;

            // Set increased durability
            durabilityField.SetValue(__instance, newDurability);
            maxDurabilityField.SetValue(__instance, newDurability);
        }
    }

    /// <summary>
    /// Patch to increase turret accuracy when the BattleUnit is created
    /// </summary>
    [HarmonyPatch(typeof(Turret), "PlaceTurretOnTile")]
    public static class AdvancedMagazine_IncreaseAccuracy_Patch
    {
        public static void Postfix(Turret __instance)
        {
            if (!AdvancedMagazine.Instance.IsActive)
                return;

            // Get the turret's BattleUnit
            BattleUnit turretUnit = __instance.BattleUnitCreated;
            if (turretUnit == null)
                return;

            // Increase turret accuracy
            turretUnit.ChangeStat(
                Enumerations.UnitStats.Accuracy,
                AdvancedMagazine.AccuracyBonus,
                "AdvancedMagazine_AccuracyBonus"
            );
        }
    }
}
