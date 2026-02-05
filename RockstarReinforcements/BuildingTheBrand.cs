using static XenopurgeRougeLike.ModLocalization;

namespace XenopurgeRougeLike.RockstarReinforcements
{
    // 构造人设
    // Building the Brand
    // Double the amount of fans gained.
    public class BuildingTheBrand : Reinforcement
    {
        public const float FanMultiplier = 2.0f;

        public BuildingTheBrand()
        {
            company = Company.Rockstar;
            rarity = Rarity.Expert;
            name = L("rockstar.building_the_brand.name");
            description = L("rockstar.building_the_brand.description");
        }

        private static BuildingTheBrand _instance;
        public static BuildingTheBrand Instance => _instance ??= new();
    }
}
