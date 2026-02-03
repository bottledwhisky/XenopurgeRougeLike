using SpaceCommander;
using static XenopurgeRougeLike.ModLocalization;

namespace XenopurgeRougeLike.CommonReinforcements
{
    // 力量训练：全队+1/2/3/4/5近战伤害（可叠加5）
    // Strength Training: All units +1/2/3/4/5 Melee Damage (stackable 5 times)
    public class StrengthTraining : Reinforcement
    {
        public const float PowerPerStack = 1f;

        public StrengthTraining()
        {
            company = Company.Common;
            rarity = Rarity.Standard;
            stackable = true;
            maxStacks = 5;
            name = L("common.strength_training.name");
            flavourText = L("common.strength_training.flavour");
        }

        public override string GetDescriptionForStacks(int stacks)
        {
            int powerBonus = (int)(PowerPerStack * stacks);
            return L("common.strength_training.description", powerBonus);
        }

        protected static StrengthTraining _instance;
        public static StrengthTraining Instance => _instance ??= new();

        public override void OnActivate()
        {
            // Register stat change for all player units
            UnitStatsTools.InBattleUnitStatChanges["StrengthTraining"] = new UnitStatChange(
                Enumerations.UnitStats.Power,
                PowerPerStack * currentStacks,
                Enumerations.Team.Player
            );
        }

        public override void OnDeactivate()
        {
            // Remove stat change
            UnitStatsTools.InBattleUnitStatChanges.Remove("StrengthTraining");
        }
    }
}
