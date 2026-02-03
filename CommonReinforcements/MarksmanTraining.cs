using SpaceCommander;
using static XenopurgeRougeLike.ModLocalization;

namespace XenopurgeRougeLike.CommonReinforcements
{
    // 射击训练：全队+10/20/30/40/50瞄准（可叠加5）
    // Marksman Training: All units +10/20/30/40/50 Accuracy (stackable 5 times)
    public class MarksmanTraining : Reinforcement
    {
        public const float AccuracyPerStack = 0.1f; // +10 Accuracy per stack (displayed as x100)

        public MarksmanTraining()
        {
            company = Company.Common;
            rarity = Rarity.Standard;
            stackable = true;
            maxStacks = 5;
            name = L("common.marksman_training.name");
            flavourText = L("common.marksman_training.flavour");
        }

        public override string GetDescriptionForStacks(int stacks)
        {
            int accuracyBonus = (int)(AccuracyPerStack * 100 * stacks);
            return L("common.marksman_training.description", accuracyBonus);
        }

        protected static MarksmanTraining _instance;
        public static MarksmanTraining Instance => _instance ??= new();

        public override void OnActivate()
        {
            // Register stat change for all player units
            UnitStatsTools.InBattleUnitStatChanges["MarksmanTraining"] = new UnitStatChange(
                Enumerations.UnitStats.Accuracy,
                AccuracyPerStack * currentStacks,
                Enumerations.Team.Player
            );
        }

        public override void OnDeactivate()
        {
            // Remove stat change
            UnitStatsTools.InBattleUnitStatChanges.Remove("MarksmanTraining");
        }
    }
}
