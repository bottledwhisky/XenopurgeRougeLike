using SpaceCommander;
using static XenopurgeRougeLike.ModLocalization;

namespace XenopurgeRougeLike.CommonReinforcements
{
    // 钝器专家：使用钝器类武器时，护甲+5，近战伤害+2
    // Blunt Weapon Specialist: When using blunt weapons, +5 Armor, +2 Melee Power
    public class BluntWeaponSpecialist : Reinforcement
    {
        public const int ArmorBonus = 5;
        public const int PowerBonus = 2;

        public BluntWeaponSpecialist()
        {
            company = Company.Common;
            rarity = Rarity.Standard;
            stackable = false;
            name = L("common.blunt_weapon_specialist.name");
            flavourText = L("common.blunt_weapon_specialist.flavour");
            description = L("common.blunt_weapon_specialist.description", ArmorBonus, PowerBonus);
        }

        protected static BluntWeaponSpecialist _instance;
        public static BluntWeaponSpecialist Instance => _instance ??= new();

        public override void OnActivate()
        {
            // Register conditional stat changes for units using blunt weapons
            UnitStatsTools.InBattleUnitStatChanges["BluntWeaponSpecialist_Armor"] = new UnitStatChange(
                Enumerations.UnitStats.Armor,
                ArmorBonus,
                Enumerations.Team.Player,
                (unit, team) => WeaponCategories.IsUsingBluntWeapon(unit)
            );

            UnitStatsTools.InBattleUnitStatChanges["BluntWeaponSpecialist_Power"] = new UnitStatChange(
                Enumerations.UnitStats.Power,
                PowerBonus,
                Enumerations.Team.Player,
                (unit, team) => WeaponCategories.IsUsingBluntWeapon(unit)
            );
        }

        public override void OnDeactivate()
        {
            UnitStatsTools.InBattleUnitStatChanges.Remove("BluntWeaponSpecialist_Armor");
            UnitStatsTools.InBattleUnitStatChanges.Remove("BluntWeaponSpecialist_Power");
        }
    }
}
