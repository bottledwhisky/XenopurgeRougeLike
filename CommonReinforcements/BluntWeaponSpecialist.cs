using HarmonyLib;
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
            // Register Power bonus for units using blunt weapons
            UnitStatsTools.InBattleUnitStatChanges["BluntWeaponSpecialist_Power"] = new UnitStatChange(
                Enumerations.UnitStats.Power,
                PowerBonus,
                Enumerations.Team.Player,
                (unit, team) => WeaponCategories.IsUsingBluntWeapon(unit)
            );
        }

        public override void OnDeactivate()
        {
            UnitStatsTools.InBattleUnitStatChanges.Remove("BluntWeaponSpecialist_Power");
        }

        /// <summary>
        /// Apply armor to all player units using blunt weapons when battle starts.
        /// Called from TestGame.StartGame postfix.
        /// </summary>
        public static void ApplyBluntWeaponArmor()
        {
            if (!Instance.IsActive)
                return;

            var gameManager = GameManager.Instance;
            var tm = gameManager.GetTeamManager(Enumerations.Team.Player);

            // Apply armor to player units using blunt weapons
            foreach (var unit in tm.BattleUnits)
            {
                if (WeaponCategories.IsUsingBluntWeapon(unit))
                {
                    UnitStatsTools.AddArmorToUnit(unit, ArmorBonus);
                }
            }
        }
    }

    /// <summary>
    /// Patch to apply blunt weapon armor bonus when battle starts
    /// </summary>
    [HarmonyPatch(typeof(TestGame), "StartGame")]
    public static class BluntWeaponSpecialist_StartGame_Patch
    {
        public static void Postfix()
        {
            BluntWeaponSpecialist.ApplyBluntWeaponArmor();
        }
    }
}
