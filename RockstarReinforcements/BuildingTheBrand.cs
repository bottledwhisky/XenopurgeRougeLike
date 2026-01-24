using SpaceCommander;

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
            name = "Building the Brand";
            description = "Double the amount of fans gained.";
        }

        private static BuildingTheBrand _instance;
        public static BuildingTheBrand Instance => _instance ??= new();
    }
}
