using HarmonyLib;
using SpaceCommander.Commands;
using static XenopurgeRougeLike.ModLocalization;

namespace XenopurgeRougeLike.GunslingerReinforcements
{
    /// <summary>
    /// 压制强化：压制的效果翻倍
    /// Enhanced Suppression: Double the suppression effect
    /// </summary>
    public class EnhancedSuppression : Reinforcement
    {
        public const float SuppressionMultiplier = 2.0f; // Double the effect

        public EnhancedSuppression()
        {
            company = Company.Gunslinger;
            rarity = Rarity.Standard;
            stackable = false;
            name = L("gunslinger.enhanced_suppression.name");
            flavourText = L("gunslinger.enhanced_suppression.flavour");
            description = L("gunslinger.enhanced_suppression.description", (int)(SuppressionMultiplier * 100));
        }

        protected static EnhancedSuppression _instance;
        public static EnhancedSuppression Instance => _instance ??= new();
    }

    /// <summary>
    /// Patch SuppressiveFireCommandDataSO to double the speed debuff when Enhanced Suppression is active
    /// </summary>
    [HarmonyPatch(typeof(SuppressiveFireCommandDataSO), nameof(SuppressiveFireCommandDataSO.SpeedDebuff), MethodType.Getter)]
    public static class EnhancedSuppression_SpeedDebuff_Patch
    {
        public static void Postfix(ref float __result)
        {
            if (!EnhancedSuppression.Instance.IsActive)
                return;

            // Double the speed debuff (debuff is negative, so multiply to make it more negative)
            __result *= EnhancedSuppression.SuppressionMultiplier;
        }
    }
}
