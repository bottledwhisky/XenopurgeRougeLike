using SpaceCommander;
using static XenopurgeRougeLike.ModLocalization;

namespace XenopurgeRougeLike.CommonReinforcements
{
    // 匕首专家：使用匕首类武器时，速度+2，近战伤害+1
    // Dagger Specialist: When using dagger weapons, +2 Speed, +1 Melee Power
    public class DaggerSpecialist : Reinforcement
    {
        public const int SpeedBonus = 2;
        public const int PowerBonus = 1;

        public DaggerSpecialist()
        {
            company = Company.Common;
            rarity = Rarity.Standard;
            stackable = false;
            name = L("common.dagger_specialist.name");
            flavourText = L("common.dagger_specialist.flavour");
            description = L("common.dagger_specialist.description", SpeedBonus, PowerBonus);
        }

        protected static DaggerSpecialist _instance;
        public static DaggerSpecialist Instance => _instance ??= new();

        public override void OnActivate()
        {
            // Register conditional stat changes for units using daggers
            UnitStatsTools.InBattleUnitStatChanges["DaggerSpecialist_Speed"] = new UnitStatChange(
                Enumerations.UnitStats.Speed,
                SpeedBonus,
                Enumerations.Team.Player,
                (unit, team) => WeaponCategories.IsUsingDagger(unit)
            );

            UnitStatsTools.InBattleUnitStatChanges["DaggerSpecialist_Power"] = new UnitStatChange(
                Enumerations.UnitStats.Power,
                PowerBonus,
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
}
