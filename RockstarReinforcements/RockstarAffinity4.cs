namespace XenopurgeRougeLike.RockstarReinforcements
{
    public class RockstarAffinity4 : CompanyAffinity
    {
        public RockstarAffinity4()
        {
            unlockLevel = 4;
            company = Company.Rockstar;
            description = "A \"Passionate Fan\" is automatically deployed at the start of battle and will find their own fun. \"Fan\" has their combat logic upgraded, and is slightly stronger. Unlock Fan Count; Gain 2k-3k fans after each battle.";
        }

        public static RockstarAffinity4 _instance;

        public static RockstarAffinity4 Instance => _instance ??= new();

        public override void OnActivate()
        {
            RockstarAffinityHelpers.fanMoney += 10;
            RockstarAffinityHelpers.fanGainLow += 1000;
            RockstarAffinityHelpers.fanGainHigh += 1000;
        }

        public override void OnDeactivate()
        {
            RockstarAffinityHelpers.fanMoney -= 10;
            RockstarAffinityHelpers.fanGainLow -= 1000;
            RockstarAffinityHelpers.fanGainHigh -= 1000;
        }

        public override string ToFullDescription()
        {
            return base.ToFullDescription() + $"\nCurrent Fan Count: {RockstarAffinityHelpers.fanCount}";
        }
    }
}
