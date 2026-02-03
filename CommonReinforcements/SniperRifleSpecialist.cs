using SpaceCommander;
using static XenopurgeRougeLike.ModLocalization;

namespace XenopurgeRougeLike.CommonReinforcements
{
    // 狙击枪专家：使用狙击枪类武器时，速度-1，瞄准+40
    // Sniper Rifle Specialist: When using sniper rifles, -1 Speed, +40 Accuracy
    public class SniperRifleSpecialist : Reinforcement
    {
        public const int SpeedPenalty = -1;
        public const float AccuracyBonus = 0.4f; // +40 Accuracy (displayed as x100)

        public SniperRifleSpecialist()
        {
            company = Company.Common;
            rarity = Rarity.Standard;
            stackable = false;
            name = L("common.sniper_rifle_specialist.name");
            flavourText = L("common.sniper_rifle_specialist.flavour");
            description = L("common.sniper_rifle_specialist.description", SpeedPenalty, (int)(AccuracyBonus * 100));
        }

        protected static SniperRifleSpecialist _instance;
        public static SniperRifleSpecialist Instance => _instance ??= new();

        public override void OnActivate()
        {
            // Register conditional stat changes for units using sniper rifles
            UnitStatsTools.InBattleUnitStatChanges["SniperRifleSpecialist_Speed"] = new UnitStatChange(
                Enumerations.UnitStats.Speed,
                SpeedPenalty,
                Enumerations.Team.Player,
                (unit, team) => WeaponCategories.IsUsingSniperRifle(unit)
            );

            UnitStatsTools.InBattleUnitStatChanges["SniperRifleSpecialist_Accuracy"] = new UnitStatChange(
                Enumerations.UnitStats.Accuracy,
                AccuracyBonus,
                Enumerations.Team.Player,
                (unit, team) => WeaponCategories.IsUsingSniperRifle(unit)
            );
        }

        public override void OnDeactivate()
        {
            UnitStatsTools.InBattleUnitStatChanges.Remove("SniperRifleSpecialist_Speed");
            UnitStatsTools.InBattleUnitStatChanges.Remove("SniperRifleSpecialist_Accuracy");
        }
    }
}
