using HarmonyLib;
using SpaceCommander;
using System.Linq;
using static XenopurgeRougeLike.ModLocalization;

namespace XenopurgeRougeLike.WarriorReinforcements
{
    // 格挡训练：战斗开始时，获得等同于近战伤害数量*2的护甲
    // Block Training: At the start of battle, gain armor equal to melee damage * 2
    public class BlockTraining : Reinforcement
    {
        public const int ArmorMultiplier = 2;

        public BlockTraining()
        {
            company = Company.Warrior;
            rarity = Rarity.Standard;
            stackable = false;
            maxStacks = 1;
            name = L("warrior.block_training.name");
            flavourText = L("warrior.block_training.flavour");
        }

        public override string GetDescriptionForStacks(int stacks)
        {
            return L("warrior.block_training.description", ArmorMultiplier);
        }

        protected static BlockTraining _instance;
        public static BlockTraining Instance => _instance ??= new();

        /// <summary>
        /// Apply armor to all player units based on their melee damage when battle starts.
        /// Called from TestGame.StartGame postfix.
        /// </summary>
        public static void ApplyBlockTrainingArmor()
        {
            if (!Instance.IsActive)
                return;

            var gameManager = GameManager.Instance;
            var tm = gameManager.GetTeamManager(Enumerations.Team.Player);

            // Apply armor to all player units based on their Power stat
            foreach (var unit in tm.BattleUnits)
            {
                // Get the unit's current melee damage (Power stat)
                float meleeDamage = unit.Power;
                int armorToGain = (int)(meleeDamage * ArmorMultiplier);

                if (armorToGain > 0)
                {
                    UnitStatsTools.AddArmorToUnit(unit, armorToGain);
                }
            }
        }
    }

    /// <summary>
    /// Patch to apply block training armor when battle starts
    /// </summary>
    [HarmonyPatch(typeof(TestGame), "StartGame")]
    public static class BlockTraining_StartGame_Patch
    {
        public static void Postfix()
        {
            BlockTraining.ApplyBlockTrainingArmor();
        }
    }
}
