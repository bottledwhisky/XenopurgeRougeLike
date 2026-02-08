using SpaceCommander;
using static XenopurgeRougeLike.ModLocalization;

namespace XenopurgeRougeLike.ScavengerReinforcements
{
    // 手枪专家：使用手枪类武器时，瞄准+15/30（可叠加2）
    // Pistol Specialist: When using pistols, +15/30 Accuracy (stackable x2)
    public class PistolSpecialist : Reinforcement
    {
        public const float AccuracyBonus = 0.15f; // +15 Accuracy per stack (displayed as x100)

        public PistolSpecialist()
        {
            company = Company.Scavenger;
            rarity = Rarity.Standard;
            stackable = true;
            maxStacks = 2;
            name = L("scavenger.pistol_specialist.name");
            flavourText = L("scavenger.pistol_specialist.flavour");
        }

        public override string GetDescriptionForStacks(int stacks)
        {
            int accuracyBonus = (int)(AccuracyBonus * stacks * 100);
            return L("scavenger.pistol_specialist.description", accuracyBonus);
        }

        protected static PistolSpecialist _instance;
        public static PistolSpecialist Instance => _instance ??= new();

        public override void OnActivate()
        {
            // Register conditional stat change for units using pistols
            UnitStatsTools.InBattleUnitStatChanges["PistolSpecialist_Accuracy"] = new UnitStatChange(
                Enumerations.UnitStats.Accuracy,
                AccuracyBonus * currentStacks,
                Enumerations.Team.Player,
                (unit, team) => WeaponCategories.IsUsingPistol(unit)
            );
        }

        public override void OnDeactivate()
        {
            UnitStatsTools.InBattleUnitStatChanges.Remove("PistolSpecialist_Accuracy");
        }
    }
}
