using HarmonyLib;
using SpaceCommander;
using UnityEngine;
using static XenopurgeRougeLike.ModLocalization;

namespace XenopurgeRougeLike.ScavengerReinforcements
{
    // 匕首专家：使用匕首类武器时，有10%/20%概率闪避伤害，速度+1/2，近战伤害+1/2（可叠加2）
    // Dagger Specialist: When using dagger weapons, 10%/20% dodge chance, +1/2 Speed, +1/2 Power (stackable up to 2 times)
    public class DaggerSpecialist : Reinforcement
    {
        public const int SpeedPerStack = 1;
        public const int PowerPerStack = 1;
        public static readonly float[] DodgeChancePerStack = [0.1f, 0.2f]; // 10%, 20%

        public DaggerSpecialist()
        {
            company = Company.Scavenger;
            rarity = Rarity.Standard;
            stackable = true;
            maxStacks = 2;
            name = L("scavenger.dagger_specialist.name");
            flavourText = L("scavenger.dagger_specialist.flavour");
        }

        public override string GetDescriptionForStacks(int stacks)
        {
            int dodgePercentage = (int)(DodgeChancePerStack[stacks - 1] * 100);
            int speedBonus = SpeedPerStack * stacks;
            int powerBonus = PowerPerStack * stacks;
            return L("scavenger.dagger_specialist.description", dodgePercentage, speedBonus, powerBonus);
        }

        protected static DaggerSpecialist _instance;
        public static DaggerSpecialist Instance => _instance ??= new();

        public override void OnActivate()
        {
            // Register conditional stat changes for units using daggers
            UnitStatsTools.InBattleUnitStatChanges["DaggerSpecialist_Speed"] = new UnitStatChange(
                Enumerations.UnitStats.Speed,
                SpeedPerStack * currentStacks,
                Enumerations.Team.Player,
                (unit, team) => WeaponCategories.IsUsingDagger(unit)
            );

            UnitStatsTools.InBattleUnitStatChanges["DaggerSpecialist_Power"] = new UnitStatChange(
                Enumerations.UnitStats.Power,
                PowerPerStack * currentStacks,
                Enumerations.Team.Player,
                (unit, team) => WeaponCategories.IsUsingDagger(unit)
            );
        }

        public override void OnDeactivate()
        {
            UnitStatsTools.InBattleUnitStatChanges.Remove("DaggerSpecialist_Speed");
            UnitStatsTools.InBattleUnitStatChanges.Remove("DaggerSpecialist_Power");
        }
    }

    /// <summary>
    /// Patch to add dodge mechanics when using daggers
    /// </summary>
    [HarmonyPatch(typeof(BattleUnit), "Damage")]
    public static class DaggerSpecialist_Dodge_Patch
    {
        public static void Prefix(BattleUnit __instance, ref float damage)
        {
            if (!DaggerSpecialist.Instance.IsActive)
                return;

            // Only apply to player team units
            if (__instance.Team != Enumerations.Team.Player)
                return;

            // Check if unit is using a dagger
            if (!WeaponCategories.IsUsingDagger(__instance))
                return;

            // Check dodge chance based on current stacks
            float dodgeChance = DaggerSpecialist.DodgeChancePerStack[DaggerSpecialist.Instance.currentStacks - 1];
            if (Random.value < dodgeChance)
            {
                // Dodge successful - negate all damage
                damage = 0f;
                Debug.Log($"DaggerSpecialist: {__instance.UnitName} dodged the attack!");
            }
        }
    }
}
