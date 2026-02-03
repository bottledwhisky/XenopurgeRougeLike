using SpaceCommander;
using static XenopurgeRougeLike.ModLocalization;

namespace XenopurgeRougeLike.CommonReinforcements
{
    // 重新考虑：每次选择增援时，前1/2/3次刷新免费（可叠加3）
    // Reconsider: First 1/2/3 rerolls are free each reinforcement selection (stackable 3 times)
    public class Reconsider : Reinforcement
    {
        public const int FreeRerollsPerStack = 1;

        // Track how many free rerolls have been used in the current session
        public static int usedFreeRerolls = 0;

        public Reconsider()
        {
            company = Company.Common;
            rarity = Rarity.Standard;
            stackable = true;
            maxStacks = 3;
            name = L("common.reconsider.name");
            flavourText = L("common.reconsider.flavour");
        }

        public override string GetDescriptionForStacks(int stacks)
        {
            int freeRerolls = FreeRerollsPerStack * stacks;
            return L("common.reconsider.description", freeRerolls);
        }

        protected static Reconsider _instance;
        public static Reconsider Instance => _instance ??= new();

        public override void OnActivate()
        {
            // No immediate action needed - the effect is applied in UI.cs
        }

        public override void OnDeactivate()
        {
            // No cleanup needed
        }

        // Get the number of free rerolls available
        public static int GetFreeRerollsAvailable()
        {
            if (!Instance.IsActive)
                return 0;

            int totalFreeRerolls = FreeRerollsPerStack * Instance.currentStacks;
            int remainingFreeRerolls = totalFreeRerolls - usedFreeRerolls;
            return remainingFreeRerolls > 0 ? remainingFreeRerolls : 0;
        }

        // Use one free reroll
        public static void UseFreeReroll()
        {
            if (GetFreeRerollsAvailable() > 0)
            {
                usedFreeRerolls++;
                MelonLoader.MelonLogger.Msg($"Reconsider: Used free reroll, {usedFreeRerolls}/{FreeRerollsPerStack * Instance.currentStacks} used");
            }
        }

        // Reset free reroll tracking at the start of a new reward session
        public static void ResetFreeRerolls()
        {
            usedFreeRerolls = 0;
            MelonLoader.MelonLogger.Msg("Reconsider: Reset free reroll tracking");
        }
    }
}
