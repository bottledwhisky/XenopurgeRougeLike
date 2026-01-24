namespace XenopurgeRougeLike.RockstarReinforcements
{
    public class RockstarAffinity6 : RockstarAffinityBase
    {
        public RockstarAffinity6()
        {
            unlockLevel = 6;
            company = Company.Rockstar;
            description = "A \"Passionate Fan\" is automatically deployed at the start of battle and will find their own fun. \"Fan\" has their combat logic upgraded, and is much stronger. Unlock Fan Count; Gain 3k-4k fans after each battle.";
        }

        public static RockstarAffinity6 _instance;

        public static RockstarAffinity6 Instance => _instance ??= new();

        public override void OnActivate()
        {
            RockstarAffinityHelpers.fanMoney += 30;
            RockstarAffinityHelpers.fanGainLow += 2000;
            RockstarAffinityHelpers.fanGainHigh += 2000;
        }

        public override void OnDeactivate()
        {
            RockstarAffinityHelpers.fanMoney -= 30;
            RockstarAffinityHelpers.fanGainLow -= 2000;
            RockstarAffinityHelpers.fanGainHigh -= 2000;
        }

        public override string ToFullDescription()
        {
            return base.ToFullDescription() + $"\nCurrent Fan Count: {RockstarAffinityHelpers.fanCount}";
        }
    }
}
