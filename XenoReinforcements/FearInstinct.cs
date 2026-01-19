using HarmonyLib;
using MelonLoader;
using SpaceCommander;
using SpaceCommander.Area;
using System;
using static SpaceCommander.Enumerations;

namespace XenopurgeRougeLike.XenoReinforcements
{
    /// <summary>
    /// 恐惧本能：每杀死一个敌人，就延缓下一波敌人到来的时间（2s）
    /// Fear Instinct: Each enemy killed delays the next wave by 2 seconds
    /// </summary>
    public class FearInstinct : Reinforcement
    {
        public const float DelayPerKill = 2f;

        public FearInstinct()
        {
            company = Company.Xeno;
            rarity = Rarity.Elite;
            stackable = false;
            name = "Fear Instinct";
            description = "Each enemy killed delays the next enemy wave by {0} seconds.";
            flavourText = "The death of their kin sends ripples of terror through the hive mind, causing them to hesitate.";
        }

        public override string Description
        {
            get { return string.Format(description, DelayPerKill); }
        }

        public static FearInstinct Instance => (FearInstinct)Xeno.Reinforcements[typeof(FearInstinct)];
    }

    /// <summary>
    /// Patch BattleUnit constructor to add OnDeath listener for FearInstinct
    /// When an enemy dies, delay the next wave spawn
    /// </summary>
    [HarmonyPatch(typeof(BattleUnit), MethodType.Constructor)]
    [HarmonyPatch(new Type[] { typeof(UnitData), typeof(Team), typeof(GridManager) })]
    public static class FearInstinct_BattleUnit_Constructor_Patch
    {
        public static void Postfix(BattleUnit __instance, Team team)
        {
            if (!FearInstinct.Instance.IsActive)
                return;

            // Only hook enemy units
            if (team == Team.EnemyAI)
            {
                Action action = null;
                action = () =>
                {
                    // When any enemy dies, delay the next wave
                    var spawner = TempSingleton<GameManager>.Instance?.EnemiesSpawnerInBattle;
                    if (spawner != null && spawner.IsSpawningEnemies)
                    {
                        spawner.AddDelayToNextSpawn(FearInstinct.DelayPerKill);
                        MelonLogger.Msg($"FearInstinct: Enemy killed, delaying next wave by {FearInstinct.DelayPerKill}s");
                    }
                    __instance.OnDeath -= action;
                };
                __instance.OnDeath += action;
            }
        }
    }
}
