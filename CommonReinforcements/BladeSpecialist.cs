using SpaceCommander;
using static XenopurgeRougeLike.ModLocalization;

namespace XenopurgeRougeLike.CommonReinforcements
{
    // 剑刃专家：使用剑刃类武器时，近战伤害+3
    // Blade Specialist: When using blade weapons, +3 Melee Power
    public class BladeSpecialist : Reinforcement
    {
        public const int PowerBonus = 3;

        public BladeSpecialist()
        {
            company = Company.Common;
            rarity = Rarity.Standard;
            stackable = false;
            name = L("common.blade_specialist.name");
            flavourText = L("common.blade_specialist.flavour");
            description = L("common.blade_specialist.description", PowerBonus);
        }

        protected static BladeSpecialist _instance;
        public static BladeSpecialist Instance => _instance ??= new();

        public override void OnActivate()
        {
            // Register conditional stat change for units using blades
            UnitStatsTools.InBattleUnitStatChanges["BladeSpecialist_Power"] = new UnitStatChange(
                Enumerations.UnitStats.Power,
                PowerBonus,
                Enumerations.Team.Player,
                (unit, team) => WeaponCategories.IsUsingBlade(unit)
            );
        }

        public override void OnDeactivate()
        {
            UnitStatsTools.InBattleUnitStatChanges.Remove("BladeSpecialist_Power");
        }
    }
}
