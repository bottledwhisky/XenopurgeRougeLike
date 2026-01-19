using SpaceCommander;
using static SpaceCommander.Enumerations;

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
            rarity = Rarity.Elite;
            stackable = false;
            name = "Intimidation";
            description = "All Xenos permanently have {0} speed.";
            flavourText = "Your mere presence fills them with dread, slowing their movements as fear grips their primitive minds.";
        }

        public override string Description
        {
            get { return string.Format(description, SpeedReduction); }
        }

        public static Intimidation Instance => (Intimidation)Xeno.Reinforcements[typeof(Intimidation)];

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
