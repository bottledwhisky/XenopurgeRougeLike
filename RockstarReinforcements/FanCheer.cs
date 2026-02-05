using HarmonyLib;
using MelonLoader;
using SpaceCommander;
using System.Collections.Generic;
using System.Linq;
using TimeSystem;
using static SpaceCommander.Enumerations;
using static XenopurgeRougeLike.ModLocalization;

namespace XenopurgeRougeLike.RockstarReinforcements
{
    // Fan Cheer: The first squad member becomes the "Top Star". When the "Top Star" or "Passionate Fan" takes damage,
    // the stats of the other(s) greatly increase (non-stackable). Effect duration 5s and +1 second for every 2,000 fans.
    public class FanCheer : Reinforcement
    {
        // Stat bonuses for Passionate Fans when Top Star takes damage
        public const float HealthBonus = 10f;
        public const float AccuracyBonus = 0.25f;
        public const float SpeedBonus = 5f;
        public const float PowerBonus = 3f;

        // Base duration in seconds
        public const float BaseDuration = 5f;

        // Duration per 1000 fans
        public const float DurationPerThousandFans = 1f;
        public const int FanPerDuraction = 2000;

        // Dictionary to track active buff timers for each fan
        private static Dictionary<string, float> activeFanBuffs = new Dictionary<string, float>();

        public FanCheer()
        {
            company = Company.Rockstar;
            rarity = Rarity.Elite;
            name = L("rockstar.fan_cheer.name");
            description = L("rockstar.fan_cheer.description", (int)HealthBonus, (int)(AccuracyBonus * 100), (int)SpeedBonus, (int)PowerBonus, (int)BaseDuration, FanPerDuraction);
        }

        private static FanCheer _instance;
        public static FanCheer Instance => _instance ??= new();

        public override void OnActivate()
        {
            TempSingleton<TimeManager>.Instance.OnTimeUpdated += UpdateBuffTimers;
        }

        public override void OnDeactivate()
        {
            TempSingleton<TimeManager>.Instance.OnTimeUpdated -= UpdateBuffTimers;
            ClearAllBuffs();
        }

        public static float GetBuffDuration()
        {
            return BaseDuration + (RockstarAffinityHelpers.fanCount / FanPerDuraction) * DurationPerThousandFans;
        }

        public static bool IsFan(BattleUnit unit)
        {
            return unit != null && unit.Team == Team.Player && unit.UnitNameNoNumber == RockstarAffinityHelpers.FAN_NAME;
        }

        public static void ApplyFanBuffs(BattleUnit fan)
        {
            if (fan == null || !IsFan(fan))
                return;

            // Check if buff is already active (non-stackable)
            if (activeFanBuffs.ContainsKey(fan.UnitId) && activeFanBuffs[fan.UnitId] > 0)
            {
                // Refresh duration
                activeFanBuffs[fan.UnitId] = GetBuffDuration();
                MelonLogger.Msg($"FanCheer: Refreshed buff duration for Fan {fan.UnitId} to {activeFanBuffs[fan.UnitId]}s");
                return;
            }

            // Apply buffs
            fan.ChangeStat(UnitStats.Health, HealthBonus, "FanCheer_HealthBonus");
            fan.ChangeStat(UnitStats.Accuracy, AccuracyBonus, "FanCheer_AccuracyBonus");
            fan.ChangeStat(UnitStats.Speed, SpeedBonus, "FanCheer_SpeedBonus");
            fan.ChangeStat(UnitStats.Power, PowerBonus, "FanCheer_PowerBonus");

            activeFanBuffs[fan.UnitId] = GetBuffDuration();
            MelonLogger.Msg($"FanCheer: Applied buffs to Fan {fan.UnitId} for {activeFanBuffs[fan.UnitId]}s");
        }

        public static void RemoveFanBuffs(BattleUnit fan)
        {
            if (fan == null)
                return;

            fan.ReverseChangeOfStat("FanCheer_HealthBonus");
            fan.ReverseChangeOfStat("FanCheer_AccuracyBonus");
            fan.ReverseChangeOfStat("FanCheer_SpeedBonus");
            fan.ReverseChangeOfStat("FanCheer_PowerBonus");

            if (activeFanBuffs.ContainsKey(fan.UnitId))
            {
                activeFanBuffs.Remove(fan.UnitId);
            }

            MelonLogger.Msg($"FanCheer: Removed buffs from Fan {fan.UnitId}");
        }

        public static void UpdateBuffTimers(float deltaTime)
        {
            if (!Instance.IsActive)
                return;

            var gameManager = GameManager.Instance;
            if (gameManager == null)
                return;

            var playerTeam = gameManager.GetTeamManager(Team.Player);
            if (playerTeam == null)
                return;

            // Update all active buff timers
            var expiredBuffs = new List<string>();
            foreach (var kvp in activeFanBuffs.ToList())
            {
                var unitId = kvp.Key;
                var timeRemaining = kvp.Value - deltaTime;

                if (timeRemaining <= 0)
                {
                    // Buff expired
                    var fan = playerTeam.BattleUnits.FirstOrDefault(u => u.UnitId == unitId);
                    if (fan != null)
                    {
                        RemoveFanBuffs(fan);
                    }
                    expiredBuffs.Add(unitId);
                }
                else
                {
                    activeFanBuffs[unitId] = timeRemaining;
                }
            }

            foreach (var unitId in expiredBuffs)
            {
                activeFanBuffs.Remove(unitId);
            }
        }

        // Clear all active buffs (called when mission ends)
        public static void ClearAllBuffs()
        {
            activeFanBuffs.Clear();
        }
    }

    // Patch to detect when the Top Star takes damage
    [HarmonyPatch(typeof(BattleUnit), "Damage")]
    public class FanCheer_BattleUnit_TakeDamage_Patch
    {
        public static void Postfix(BattleUnit __instance, float damage)
        {
            if (!FanCheer.Instance.IsActive)
                return;

            // Apply buffs to all Passionate Fans
            var gameManager = GameManager.Instance;
            if (gameManager == null)
                return;

            var playerTeam = gameManager.GetTeamManager(Team.Player);
            if (playerTeam == null)
                return;

            // Check if the damaged unit is the Top Star
            if (InTheSpotlight.IsTopStar(__instance))
            {
                MelonLogger.Msg($"FanCheer: Top Star took {damage} damage! Activating Fan buffs!");

                foreach (var unit in playerTeam.BattleUnits)
                {
                    if (FanCheer.IsFan(unit))
                    {
                        FanCheer.ApplyFanBuffs(unit);
                    }
                }
            }
            else if (FanCheer.IsFan(__instance))
            {
                MelonLogger.Msg($"FanCheer: Fan took {damage} damage! Activating Top Star buffs!");

                foreach (var unit in playerTeam.BattleUnits)
                {
                    if (InTheSpotlight.IsTopStar(unit))
                    {
                        FanCheer.ApplyFanBuffs(unit);
                    }
                }
            }
        }
    }

    // Patch to update buff timers each frame
    [HarmonyPatch(typeof(TestGame), "Update")]
    public class FanCheer_TestGame_Update_Patch
    {
        public static void Postfix()
        {
            FanCheer.UpdateBuffTimers(UnityEngine.Time.deltaTime);
        }
    }

    // Patch to clear buffs when mission ends
    [HarmonyPatch(typeof(TestGame), "EndGame")]
    public class FanCheer_TestGame_EndGame_Patch
    {
        public static void Postfix()
        {
            FanCheer.ClearAllBuffs();
        }
    }
}
