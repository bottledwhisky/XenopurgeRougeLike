using SpaceCommander;
using static XenopurgeRougeLike.ModLocalization;

namespace XenopurgeRougeLike.CommonReinforcements
{
    // 手枪专家：使用手枪类武器时，速度+2，瞄准+10
    // Pistol Specialist: When using pistols, +2 Speed, +10 Accuracy
    public class PistolSpecialist : Reinforcement
    {
        public const int SpeedBonus = 2;
        public const float AccuracyBonus = 0.1f; // +10 Accuracy (displayed as x100)

        public PistolSpecialist()
        {
            company = Company.Common;
            rarity = Rarity.Standard;
            stackable = false;
            name = L("common.pistol_specialist.name");
            flavourText = L("common.pistol_specialist.flavour");
            description = L("common.pistol_specialist.description", SpeedBonus, (int)(AccuracyBonus * 100));
        }

        protected static PistolSpecialist _instance;
        public static PistolSpecialist Instance => _instance ??= new();

        public override void OnActivate()
        {
            // Register conditional stat changes for units using pistols
            UnitStatsTools.InBattleUnitStatChanges["PistolSpecialist_Speed"] = new UnitStatChange(
                Enumerations.UnitStats.Speed,
                SpeedBonus,
                Enumerations.Team.Player,
                (unit, team) => WeaponCategories.IsUsingPistol(unit)
            );

            UnitStatsTools.InBattleUnitStatChanges["PistolSpecialist_Accuracy"] = new UnitStatChange(
                Enumerations.UnitStats.Accuracy,
                AccuracyBonus,
                Enumerations.Team.Player,
                (unit, team) => WeaponCategories.IsUsingPistol(unit)
            );
        }

        public override void OnDeactivate()
        {
            UnitStatsTools.InBattleUnitStatChanges.Remove("PistolSpecialist_Speed");
            UnitStatsTools.InBattleUnitStatChanges.Remove("PistolSpecialist_Accuracy");
        }
    }
}
