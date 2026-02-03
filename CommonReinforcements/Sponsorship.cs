using SpaceCommander;
using static XenopurgeRougeLike.ModLocalization;

namespace XenopurgeRougeLike.CommonReinforcements
{
    // 一笔赞助：+10硬币（可叠加5，每次+10）
    // Sponsorship: +10 coins (stackable 5 times, +10 each)
    public class Sponsorship : Reinforcement
    {
        public const int CoinsPerStack = 10;

        public Sponsorship()
        {
            company = Company.Common;
            rarity = Rarity.Standard;
            stackable = true;
            maxStacks = 5;
            name = L("common.sponsorship.name");
            flavourText = L("common.sponsorship.flavour");
        }

        public override string GetDescriptionForStacks(int stacks)
        {
            int coinBonus = CoinsPerStack * stacks;
            return L("common.sponsorship.description", coinBonus);
        }

        protected static Sponsorship _instance;
        public static Sponsorship Instance => _instance ??= new();

        public override void OnActivate()
        {
            // Grant coins immediately when acquired
            PlayerWalletHelper.ChangeCoins(CoinsPerStack);
            MelonLoader.MelonLogger.Msg($"Sponsorship: Granted {CoinsPerStack} coins, total: {PlayerWalletHelper.GetCoins()}");
        }

        public override void OnDeactivate()
        {
            // No cleanup needed
        }
    }
}
