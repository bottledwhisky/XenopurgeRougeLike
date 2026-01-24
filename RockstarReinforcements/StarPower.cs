using System.Linq;
using HarmonyLib;
using MelonLoader;
using SpaceCommander;
using SpaceCommander.EndGame;
using SpaceCommander.GameFlow;
using static SpaceCommander.Enumerations;

namespace XenopurgeRougeLike.RockstarReinforcements
{
    public class StarPower : Reinforcement
    {
        public const float FanBonusMultiplier = 0.5f;
        private const int HPBonus = 5;
        private const float AccuracyBonus = 0.05f;
        private const int SpeedBonus = 1;
        private const int PowerBonus = 1;
        public StarPower()
        {
            stackable = true;
            maxStacks = 2;
            company = Company.Rockstar;
            name = "Star Power";
            description = "Gain +50% fans on perfect victory. Every 1,000 fans grants the first squad member one random stat: +5 HP, +5 Aim, +1 Speed, or +1 Melee Damage. Star Power II: Uncollected digital collectibles no longer count against perfect victory.Gain 1 point after battle for every 1,000 fans.";
        }

        public override string GetDescriptionForStacks(int stacks)
        {
            if (stacks == 1)
            {
                return "Gain +50% fans on perfect victory. Every 1,000 fans grants the first squad member one random stat: +5 HP, +5 Aim, +1 Speed, or +1 Melee Damage.";
            }
            else
            {
                return "Gain +50% fans on perfect victory. Every 1,000 fans grants the first squad member one random stat: +5 HP, +5 Aim, +1 Speed, or +1 Melee Damage. Uncollected digital collectibles no longer count against perfect victory. Gain 1 point after battle for every 1,000 fans.";
            }
        }

        // Implementation of the Star Power reinforcement effect would go:
        // RockstarAffinity2FanCount_Patch
        protected static StarPower _instance;
        public static StarPower Instance =>  _instance ??= new();

        public bool IsPerfectVictory(EndGameResultData data)
        {
            var gameManager = GameManager.Instance;
            var _gridCollectiblesTilesFinder = AccessTools.Field(typeof(GameManager), "_gridCollectiblesTilesFinder").GetValue(gameManager) as GridCollectiblesTilesFinder;
            var uncollected = Instance.currentStacks == 2 ? 0 : _gridCollectiblesTilesFinder.SpecialTiles.Count(tile => !(tile as CollectibleItemTile).IsCollected);

            // Check standard perfect victory conditions
            bool standardPerfect = data.IsVictory && data.UnitsKilled.Count() == 0 && uncollected == 0;

            // Check In the Spotlight condition: Top Star extracted = perfect victory
            bool spotlightPerfect = InTheSpotlight.Instance.IsActive &&
                                   data.IsVictory &&
                                   InTheSpotlight.HasTopStarExtracted(data) &&
                                   uncollected == 0;

            return standardPerfect || spotlightPerfect;
        }

        /// <summary>
        /// Apply stat buffs to the first deployed unit based on fan count.
        /// Called from TestGame.StartGame postfix when DeploymentOrder is properly set.
        /// </summary>
        public static void ApplyStatBuffs()
        {
            if (!Instance.IsActive)
                return;

            int bonusCount = RockstarAffinityHelpers.fanCount / 1000;
            if (bonusCount <= 0)
                return;

            var gameManager = GameManager.Instance;
            var tm = gameManager.GetTeamManager(Team.Player);

            // Find the first deployed unit (DeploymentOrder == 1)
            var firstUnit = tm.BattleUnits.FirstOrDefault(u => u.DeploymentOrder == 1);
            if (firstUnit == null)
                return;

            MelonLogger.Msg($"StarPower: Applying {bonusCount} stat bonuses to {firstUnit.UnitName} (DeploymentOrder: {firstUnit.DeploymentOrder})");

            // Apply random stat bonuses based on fan count
            var random = new System.Random();
            for (int i = 0; i < bonusCount; i++)
            {
                int statChoice = random.Next(4);
                switch (statChoice)
                {
                    case 0:
                        firstUnit.ChangeStat(UnitStats.Health, HPBonus, $"StarPower_HP_{i}");
                        MelonLogger.Msg($"StarPower: +{HPBonus} HP");
                        break;
                    case 1:
                        firstUnit.ChangeStat(UnitStats.Accuracy, AccuracyBonus, $"StarPower_Accuracy_{i}");
                        MelonLogger.Msg($"StarPower: +{AccuracyBonus * 100}% Accuracy");
                        break;
                    case 2:
                        firstUnit.ChangeStat(UnitStats.Speed, SpeedBonus, $"StarPower_Speed_{i}");
                        MelonLogger.Msg($"StarPower: +{SpeedBonus} Speed");
                        break;
                    case 3:
                        firstUnit.ChangeStat(UnitStats.Power, PowerBonus, $"StarPower_Power_{i}");
                        MelonLogger.Msg($"StarPower: +{PowerBonus} Power");
                        break;
                }
            }
        }
    }

    [HarmonyPatch(typeof(TestGame), "StartGame")]
    public static class StarPower_StartGame_Patch
    {
        public static void Postfix()
        {
            StarPower.ApplyStatBuffs();
        }
    }

    [HarmonyPatch(typeof(TestGame), "EndGame")]
    public static class StarPower_EndGame_Patch
    {
        public static void Postfix(TestGame __instance, EndGameResultData data)
        {
            if (StarPower.Instance.IsActive && StarPower.Instance.currentStacks >= 2 && data.IsVictory)
            {
                var playerData = Singleton<Player>.Instance.PlayerData;
                if (playerData?.PlayerWallet != null)
                {
                    playerData.PlayerWallet.ChangeCoinsByValue(RockstarAffinityHelpers.fanCount / 1000);
                }
            }
        }
    }
}
