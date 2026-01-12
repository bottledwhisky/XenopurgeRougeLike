
using HarmonyLib;
using MelonLoader;
using SpaceCommander;
using SpaceCommander.Area;
using SpaceCommander.Audio;
using SpaceCommander.Database;
using SpaceCommander.PartyCustomization;
using System;
using System.Collections.Generic;
using System.Linq;
using static SpaceCommander.Enumerations;

namespace XenopurgeRougeLike.RockstarReinforcements
{
    // 饭圈出征
    // Fandom Rallies
    // When a "Passionate Fan" dies, another "Passionate Fan" joins the battlefield.
    public class FandomRallies : Reinforcement
    {
        public FandomRallies()
        {
            company = Company.Rockstar;
            name = "Fandom Rallies";
            description = "When a \"Passionate Fan\" dies, another \"Passionate Fan\" joins the battlefield.";
        }

        private static FandomRallies _instance;
        public static FandomRallies Instance => _instance ??= new FandomRallies();
    }

    [HarmonyPatch(typeof(BattleUnit), "Die")]
    public class FandomRallies_BattleUnit_Die_Patch
    {
        public static void Postfix(BattleUnit __instance)
        {
            if (!FandomRallies.Instance.IsActive)
            {
                return;
            }

            // Check if the dead unit is a "Passionate Fan"
            if (__instance.UnitNameNoNumber != RockstarAffinityHelpers.FAN_NAME || __instance.Team != Team.Player)
            {
                return;
            }

            MelonLogger.Msg("FandomRallies: A Passionate Fan has died! Spawning a new one...");

            // Find TestGame instance to spawn the new fan
            var testGame = UnityEngine.Object.FindAnyObjectByType<TestGame>();
            if (testGame == null)
            {
                MelonLogger.Error("FandomRallies: TestGame not found!");
                return;
            }

            // Get UnitsPlacementPhase from TestGame to access PlayerPositionRooms
            var playerPositionRooms = testGame.UnitsPlacementPhase.UnitsPlacementRules.PlayerPositionRooms;

            // Find a random tile in one of the player position rooms
            var randomRoom = playerPositionRooms.ElementAt(UnityEngine.Random.Range(0, playerPositionRooms.Count()));
            var availableTiles = randomRoom.TilesOfRoom.Where(t => t.CurrentStateOfTile.HasAvailablePosition(Team.Player)).ToList();

            if (availableTiles.Count == 0)
            {
                MelonLogger.Error("FandomRallies: No available tiles in player position rooms!");
                return;
            }

            Tile spawnTile = availableTiles[UnityEngine.Random.Range(0, availableTiles.Count)];

            // Create fan unit data using shared helper method
            var fanUnitData = RockstarAffinityHelpers.CreateFanUnitData();
            var fanUnitDataSO = FanUnitDataSO.Create(fanUnitData);

            // Use TestGame's SpawnUnit method via reflection
            var spawnUnitMethod = AccessTools.Method(typeof(TestGame), "SpawnUnit");
            if (spawnUnitMethod == null)
            {
                MelonLogger.Error("FandomRallies: SpawnUnit method not found!");
                return;
            }

            spawnUnitMethod.Invoke(testGame, new object[] { fanUnitDataSO, spawnTile });

            MelonLogger.Msg($"FandomRallies: Successfully spawned a new Passionate Fan at {spawnTile.Coords}!");
        }
    }
}
