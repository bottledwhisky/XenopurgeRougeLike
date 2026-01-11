using System.Linq;
using HarmonyLib;
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

        public override string Description
        {
            get
            {
                if (currentStacks == 1)
                {
                    return "Gain +50% fans on perfect victory. Every 1,000 fans grants the first squad member one random stat: +5 HP, +5 Aim, +1 Speed, or +1 Melee Damage.";
                }
                else
                {
                    return "Gain +50% fans on perfect victory. Every 1,000 fans grants the first squad member one random stat: +5 HP, +5 Aim, +1 Speed, or +1 Melee Damage. Uncollected digital collectibles no longer count against perfect victory. Gain 1 point after battle for every 1,000 fans.";
                }
            }
        }

        // Implementation of the Star Power reinforcement effect would go:
        // RockstarAffinity2FanCount_Patch
        public static StarPower Instance => Rockstar.StarPower;

        public bool IsPerfectVictory(EndGameResultData data)
        {
            var gameManager = GameManager.Instance;
            var _gridCollectiblesTilesFinder = AccessTools.Field(typeof(GameManager), "_gridCollectiblesTilesFinder").GetValue(gameManager) as GridCollectiblesTilesFinder;
            var uncollected = Instance.currentStacks == 2 ? 0 : _gridCollectiblesTilesFinder.SpecialTiles.Count(tile => !(tile as CollectibleItemTile).IsCollected);
            return data.IsVictory && data.UnitsKilled.Count() == 0 && uncollected == 0;
        }

        public static bool ShouldApply(BattleUnit unit, Team team)
        {
            return Instance.IsActive && team == Team.Player && unit.DeploymentOrder == 1;
        }

        public override void OnActivate()
        {
            UnitStatsTools.InBattleUnitStatChanges["StarPower_HP"] = new UnitStatChange(UnitStats.Health, HPBonus, ShouldApply);
            UnitStatsTools.InBattleUnitStatChanges["StarPower_Speed"] = new UnitStatChange(UnitStats.Speed, SpeedBonus, ShouldApply);
            UnitStatsTools.InBattleUnitStatChanges["StarPower_Accuracy"] = new UnitStatChange(UnitStats.Accuracy, AccuracyBonus, ShouldApply);
            UnitStatsTools.InBattleUnitStatChanges["StarPower_Power"] = new UnitStatChange(UnitStats.Power, PowerBonus, ShouldApply);
        }

        public override void OnDeactivate()
        {
            UnitStatsTools.InBattleUnitStatChanges.Remove("StarPower_HP");
            UnitStatsTools.InBattleUnitStatChanges.Remove("StarPower_Speed");
            UnitStatsTools.InBattleUnitStatChanges.Remove("StarPower_Accuracy");
            UnitStatsTools.InBattleUnitStatChanges.Remove("StarPower_Power");
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
