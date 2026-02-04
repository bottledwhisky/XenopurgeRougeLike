using HarmonyLib;
using SpaceCommander;
using static XenopurgeRougeLike.ModLocalization;

namespace XenopurgeRougeLike.CommonReinforcements
{
    // 霰弹枪专家：使用霰弹枪类武器时，速度+2，护甲+5
    // Shotgun Specialist: When using shotguns, +2 Speed, +5 Armor
    public class ShotgunSpecialist : Reinforcement
    {
        public const int SpeedBonus = 2;
        public const int ArmorBonus = 5;

        public ShotgunSpecialist()
        {
            company = Company.Common;
            rarity = Rarity.Standard;
            stackable = false;
            name = L("common.shotgun_specialist.name");
            flavourText = L("common.shotgun_specialist.flavour");
            description = L("common.shotgun_specialist.description", SpeedBonus, ArmorBonus);
        }

        protected static ShotgunSpecialist _instance;
        public static ShotgunSpecialist Instance => _instance ??= new();

        public override void OnActivate()
        {
            // Register Speed bonus for units using shotguns
            UnitStatsTools.InBattleUnitStatChanges["ShotgunSpecialist_Speed"] = new UnitStatChange(
                Enumerations.UnitStats.Speed,
                SpeedBonus,
                Enumerations.Team.Player,
                (unit, team) => WeaponCategories.IsUsingShotgun(unit)
            );
        }

        public override void OnDeactivate()
        {
            UnitStatsTools.InBattleUnitStatChanges.Remove("ShotgunSpecialist_Speed");
        }

        /// <summary>
        /// Apply armor to all player units using shotguns when battle starts.
        /// Called from TestGame.StartGame postfix.
        /// </summary>
        public static void ApplyShotgunArmor()
        {
            if (!Instance.IsActive)
                return;

            var gameManager = GameManager.Instance;
            var tm = gameManager.GetTeamManager(Enumerations.Team.Player);

            // Apply armor to player units using shotguns
            foreach (var unit in tm.BattleUnits)
            {
                if (WeaponCategories.IsUsingShotgun(unit))
                {
                    UnitStatsTools.AddArmorToUnit(unit, ArmorBonus);
                }
            }
        }
    }

    /// <summary>
    /// Patch to apply shotgun armor bonus when battle starts
    /// </summary>
    [HarmonyPatch(typeof(TestGame), "StartGame")]
    public static class ShotgunSpecialist_StartGame_Patch
    {
        public static void Postfix()
        {
            ShotgunSpecialist.ApplyShotgunArmor();
        }
    }
}
