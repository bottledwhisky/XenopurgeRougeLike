using SpaceCommander;
using static XenopurgeRougeLike.ModLocalization;

namespace XenopurgeRougeLike.RockstarReinforcements
{
    // 粉丝头目
    // Superfan
    // An additional "Passionate Fan" is automatically deployed at the start of battle.
    public class Superfan : Reinforcement
    {
        public Superfan()
        {
            company = Company.Rockstar;
            rarity = Rarity.Expert;
            name = L("rockstar.superfan.name");
            description = L("rockstar.superfan.description");
        }

        private static Superfan _instance;
        public static Superfan Instance => _instance ??= new Superfan();
    }
}
