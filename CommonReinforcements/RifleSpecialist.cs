using SpaceCommander;
using static XenopurgeRougeLike.ModLocalization;

namespace XenopurgeRougeLike.CommonReinforcements
{
    // 步枪专家：使用步枪类武器时，瞄准+30
    // Rifle Specialist: When using rifles, +30 Accuracy
    public class RifleSpecialist : Reinforcement
    {
        public const float AccuracyBonus = 0.3f; // +30 Accuracy (displayed as x100)

        public RifleSpecialist()
        {
            company = Company.Common;
            rarity = Rarity.Standard;
            stackable = false;
            name = L("common.rifle_specialist.name");
            flavourText = L("common.rifle_specialist.flavour");
            description = L("common.rifle_specialist.description", (int)(AccuracyBonus * 100));
        }

        protected static RifleSpecialist _instance;
        public static RifleSpecialist Instance => _instance ??= new();

        public override void OnActivate()
        {
            // Register conditional stat change for units using rifles
            UnitStatsTools.InBattleUnitStatChanges["RifleSpecialist_Accuracy"] = new UnitStatChange(
                Enumerations.UnitStats.Accuracy,
                AccuracyBonus,
                Enumerations.Team.Player,
                (unit, team) => WeaponCategories.IsUsingRifle(unit)
            );
        }

        public override void OnDeactivate()
        {
            UnitStatsTools.InBattleUnitStatChanges.Remove("RifleSpecialist_Accuracy");
        }
    }
}
