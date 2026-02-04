using HarmonyLib;
using SpaceCommander;
using static XenopurgeRougeLike.ModLocalization;

namespace XenopurgeRougeLike.GunslingerReinforcements
{
    /// <summary>
    /// 瞄准弱点：+20%暴击率
    /// Targeting Weakspots: +20% crit chance
    /// </summary>
    public class TargetingWeakspots : Reinforcement
    {
        public const float CritChanceBonus = 0.20f; // +20% crit chance

        public TargetingWeakspots()
        {
            company = Company.Gunslinger;
            rarity = Rarity.Standard;
            stackable = false;
            name = L("gunslinger.targeting_weakspots.name");
            flavourText = L("gunslinger.targeting_weakspots.flavour");
            description = L("gunslinger.targeting_weakspots.description", (int)(CritChanceBonus * 100));
        }

        protected static TargetingWeakspots _instance;
        public static TargetingWeakspots Instance => _instance ??= new();
    }

}
