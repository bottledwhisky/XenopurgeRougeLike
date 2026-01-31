using SpaceCommander;
using static XenopurgeRougeLike.ModLocalization;

namespace XenopurgeRougeLike.WarriorReinforcements
{
    // 受身训练：最大生命值+15/30/45（可叠加3）
    // Ukemi Training: Max health +15/30/45 (stackable up to 3)
    public class UkemiTraining : Reinforcement
    {
        public static readonly float[] HealthBonus = [15f, 30f, 45f];

        public UkemiTraining()
        {
            company = Company.Warrior;
            rarity = Rarity.Standard;
            stackable = true;
            maxStacks = 3;
            name = L("warrior.ukemi_training.name");
            flavourText = L("warrior.ukemi_training.flavour");
        }

        public override string GetDescriptionForStacks(int stacks)
        {
            return L("warrior.ukemi_training.description", (int)HealthBonus[stacks - 1]);
        }

        public override void OnActivate()
        {
            // Register max health bonus
            UnitStatsTools.InBattleUnitStatChanges["UkemiTraining_MaxHealth"] = new UnitStatChange(
                Enumerations.UnitStats.Health,
                HealthBonus[currentStacks - 1]
            );
        }

        public override void OnDeactivate()
        {
            // Remove max health bonus
            UnitStatsTools.InBattleUnitStatChanges.Remove("UkemiTraining_MaxHealth");
        }

        protected static UkemiTraining instance;
        public static UkemiTraining Instance => instance ??= new();
    }
}
