using HarmonyLib;
using MelonLoader;
using SpaceCommander;
using SpaceCommander.Area;
using static SpaceCommander.Enumerations;
using static XenopurgeRougeLike.ModLocalization;

namespace XenopurgeRougeLike.ScavengerReinforcements
{
    // 战利品: When killing a xeno, 15% chance to gain 1 coin
    public class Loot : Reinforcement
    {
        public const float DropChance = 0.15f; // 15% chance

        // Track coins gained during the current mission
        private static int _coinsGainedThisMission = 0;
        public static int CoinsGainedThisMission => _coinsGainedThisMission;

        public Loot()
        {
            company = Company.Scavenger;
            rarity = Rarity.Standard;
            name = L("scavenger.loot.name");
            description = L("scavenger.loot.description", (int)(DropChance * 100));
            flavourText = L("scavenger.loot.flavour");
        }

        protected static Loot _instance;
        public static Loot Instance => _instance ??= new();

        public override void OnActivate()
        {
            // Patch is applied via Harmony - no action needed here
            MelonLogger.Msg("Loot reinforcement activated");
        }

        public override void OnDeactivate()
        {
            // No cleanup needed
        }

        public static void AddCoinDrop()
        {
            _coinsGainedThisMission++;
        }

        public static void ClearCoinDrops()
        {
            _coinsGainedThisMission = 0;
        }
    }

    // Patch BattleUnit constructor to add OnDeath listener
    [HarmonyPatch(typeof(BattleUnit), MethodType.Constructor, [typeof(UnitData), typeof(Team), typeof(GridManager)])]
    public static class Loot_BattleUnit_Constructor_Patch
    {
        public static void Postfix(BattleUnit __instance, Team team)
        {
            if (!Loot.Instance.IsActive)
                return;

            if (team == Team.EnemyAI)
            {
                void action()
                {
                    // 15% chance to drop a coin when this xeno dies
                    if (UnityEngine.Random.value < Loot.DropChance)
                    {
                        PlayerWalletHelper.ChangeCoins(1);
                        Loot.AddCoinDrop();
                        MelonLogger.Msg($"Loot: Xeno dropped 1 coin! Total coins: {PlayerWalletHelper.GetCoins()}, gained this mission: {Loot.CoinsGainedThisMission}");
                    }
                    __instance.OnDeath -= action;
                }

                __instance.OnDeath += action;
            }
        }
    }

    // Clear coin tracking when mission starts
    [HarmonyPatch(typeof(TestGame), "StartGame")]
    public static class Loot_ClearTracking_StartGame_Patch
    {
        public static void Postfix()
        {
            Loot.ClearCoinDrops();
            MelonLogger.Msg("Loot: Cleared coin drop tracking for new mission");
        }
    }
}
