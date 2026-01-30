using static XenopurgeRougeLike.ModLocalization;

namespace XenopurgeRougeLike.RockstarReinforcements
{
    public class RockstarAffinity6 : RockstarAffinityBase
    {
        public RockstarAffinity6()
        {
            unlockLevel = 6;
            company = Company.Rockstar;
            description = L("rockstar.affinity6.description");
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
            return base.ToFullDescription() + $"\n{L("rockstar.affinity2.fan_count", RockstarAffinityHelpers.fanCount)}";
        }
    }
}
