using SpaceCommander;
using static XenopurgeRougeLike.ModLocalization;

namespace XenopurgeRougeLike.CommonReinforcements
{
    // 耐力训练：全队+1/2/3/4/5速度（可叠加5）
    // Endurance Training: All units +1/2/3/4/5 Speed (stackable 5 times)
    public class EnduranceTraining : Reinforcement
    {
        public const float SpeedPerStack = 1f;

        public EnduranceTraining()
        {
            company = Company.Common;
            rarity = Rarity.Standard;
            stackable = true;
            maxStacks = 5;
            name = L("common.endurance_training.name");
            flavourText = L("common.endurance_training.flavour");
        }

        public override string GetDescriptionForStacks(int stacks)
        {
            int speedBonus = (int)(SpeedPerStack * stacks);
            return L("common.endurance_training.description", speedBonus);
        }

        protected static EnduranceTraining _instance;
        public static EnduranceTraining Instance => _instance ??= new();

        public override void OnActivate()
        {
            // Register stat change for all player units
            UnitStatsTools.InBattleUnitStatChanges["EnduranceTraining"] = new UnitStatChange(
                Enumerations.UnitStats.Speed,
                SpeedPerStack * currentStacks,
                Enumerations.Team.Player
            );
        }

        public override void OnDeactivate()
        {
            // Remove stat change
            UnitStatsTools.InBattleUnitStatChanges.Remove("EnduranceTraining");
        }
    }
}
