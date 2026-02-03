using SpaceCommander;
using static SpaceCommander.Enumerations;
using static XenopurgeRougeLike.ModLocalization;

namespace XenopurgeRougeLike.XenoReinforcements
{
    /// <summary>
    /// 威吓：所有异形永久降低速度-5
    /// Intimidation: All Xenos permanently have -5 speed
    /// </summary>
    public class Intimidation : Reinforcement
    {
        public const float SpeedReduction = -5f;

        public Intimidation()
        {
            company = Company.Xeno;
            rarity = Rarity.Expert;
            stackable = false;
            name = L("xeno.intimidation.name");
            description = L("xeno.intimidation.description", SpeedReduction);
            flavourText = L("xeno.intimidation.flavour");
        }

        public override string GetDescriptionForStacks(int stacks)
        {
            return L("xeno.intimidation.description", SpeedReduction);
        }

        protected static Intimidation instance;
        public static Intimidation Instance => instance ??= new();

        public override void OnActivate()
        {
            UnitStatsTools.InBattleUnitStatChanges["Intimidation_Speed"] = new UnitStatChange(
                UnitStats.Speed,
                SpeedReduction,
                Team.EnemyAI,
                null
            );
        }

        public override void OnDeactivate()
        {
            UnitStatsTools.InBattleUnitStatChanges.Remove("Intimidation_Speed");
        }
    }
}
