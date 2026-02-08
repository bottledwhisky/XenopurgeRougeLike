using HarmonyLib;
using MelonLoader;
using SpaceCommander;
using SpaceCommander.Area;
using SpaceCommander.Difficulties;
using System;
using static SpaceCommander.Enumerations;

namespace XenopurgeRougeLike
{
    /// <summary>
    /// Manages dynamic difficulty scaling by buffing enemy health based on:
    /// 1. Game difficulty level (0-5+, supporting future difficulty levels)
    /// 2. Number of battles won in the current run (scales from 0% to 100% over 10 victories)
    /// </summary>
    public class DifficultyManager
    {
        private static int _battlesWon = 0;

        /// <summary>
        /// Get the current number of battles won in this run
        /// </summary>
        public static int BattlesWon => _battlesWon;

        /// <summary>
        /// Calculate the enemy health multiplier based on difficulty and battle progression
        /// </summary>
        /// <returns>Health multiplier (0.0 = no bonus, 1.0 = 100% bonus)</returns>
        public static float GetEnemyHealthMultiplier()
        {
            // Get difficulty level (0-5+)
            int difficulty = GetCurrentDifficulty();

            if (difficulty == 0)
            {
                // No health buff on easiest difficulty
                return 0f;
            }

            // Max health multiplier for this difficulty
            // 0% at difficulty 0, 20% per level (100% at difficulty 5)
            // Supports future difficulty levels beyond 5 (e.g., difficulty 6 = 120%)
            float maxHealthMultiplier = difficulty * 0.2f;

            // Battle progression (0 to 1.0)
            // 0 wins = 0%, 10+ wins = 100%
            float battleProgression = Math.Min(_battlesWon / 10f, 1.0f);

            // Final multiplier
            return maxHealthMultiplier * battleProgression;
        }

        /// <summary>
        /// Get the current difficulty level for the active squad
        /// </summary>
        private static int GetCurrentDifficulty()
        {
            try
            {
                var difficultyController = Singleton<DifficultiesController>.Instance;
                if (difficultyController == null)
                {
                    MelonLogger.Warning("[DifficultyManager] DifficultiesController instance not found, defaulting to difficulty 0");
                    return 0;
                }

                var player = Singleton<Player>.Instance;
                if (player?.PlayerData?.Squad == null)
                {
                    MelonLogger.Warning("[DifficultyManager] Player or Squad data not found, defaulting to difficulty 0");
                    return 0;
                }

                string squadId = player.PlayerData.Squad.StartingSquadId;
                int difficulty = difficultyController.GetDifficultyLevelForSquad(squadId);

                return difficulty;
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"[DifficultyManager] Error getting difficulty level: {ex.Message}");
                return 0;
            }
        }

        /// <summary>
        /// Increment the battle win count (called after victory)
        /// </summary>
        public static void IncrementBattleCount()
        {
            _battlesWon++;
            int difficulty = GetCurrentDifficulty();
            float multiplier = GetEnemyHealthMultiplier();
            MelonLogger.Msg($"[DifficultyManager] Battle won! Total wins: {_battlesWon}, Difficulty: {difficulty}, Next battle enemy health multiplier: {multiplier:P0}");
        }

        /// <summary>
        /// Reset the battle count (called when starting a new run)
        /// </summary>
        public static void ResetBattleCount()
        {
            _battlesWon = 0;
            MelonLogger.Msg("[DifficultyManager] Battle count reset for new run");
        }
    }

    /// <summary>
    /// Patch BattleUnit constructor to apply health buff to enemies
    /// </summary>
    [HarmonyPatch(typeof(BattleUnit), MethodType.Constructor, [typeof(UnitData), typeof(Team), typeof(GridManager)])]
    public static class DifficultyManager_BattleUnit_Constructor_Patch
    {
        public static void Postfix(BattleUnit __instance, Team team)
        {
            // Only apply to enemy units
            if (team != Team.EnemyAI)
                return;

            // Get the health multiplier
            float healthMultiplier = DifficultyManager.GetEnemyHealthMultiplier();

            // No buff needed if multiplier is 0
            if (healthMultiplier <= 0f)
                return;

            // Calculate bonus health
            float currentMaxHealth = __instance.CurrentMaxHealth;
            float bonusHealth = currentMaxHealth * healthMultiplier;

            // Apply the health bonus
            __instance.ChangeStat(UnitStats.Health, bonusHealth, "DifficultyManager_HealthBonus");

            MelonLogger.Msg($"[DifficultyManager] Applied {healthMultiplier:P0} health buff to {__instance.UnitName}: {currentMaxHealth:F0} -> {(currentMaxHealth + bonusHealth):F0} HP");
        }
    }

    /// <summary>
    /// Patch GameManager.GiveEndGameRewards to increment battle count on victory
    /// </summary>
    [HarmonyPatch(typeof(GameManager), "GiveEndGameRewards")]
    public static class DifficultyManager_GiveEndGameRewards_Patch
    {
        public static void Prefix(bool victory)
        {
            // Only increment on victory
            if (!victory)
                return;

            DifficultyManager.IncrementBattleCount();
        }
    }

    /// <summary>
    /// Patch InitializeGame.InitializePlayer to reset battle count on new run
    /// </summary>
    [HarmonyPatch(typeof(InitializeGame), "InitializePlayer")]
    public static class DifficultyManager_InitializePlayer_Patch
    {
        public static void Postfix()
        {
            DifficultyManager.ResetBattleCount();
        }
    }
}
