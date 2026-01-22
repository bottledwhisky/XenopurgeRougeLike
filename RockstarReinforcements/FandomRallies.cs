using HarmonyLib;
using MelonLoader;
using SpaceCommander;
using SpaceCommander.Area;
using SpaceCommander.Tests;
using System;
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
            rarity = Rarity.Elite;
            company = Company.Rockstar;
            name = "Fandom Rallies";
            description = "When a \"Passionate Fan\" dies, another \"Passionate Fan\" joins the battlefield.";
        }

        private static FandomRallies _instance;
        public static FandomRallies Instance => _instance ??= new FandomRallies();

        override public void OnActivate()
        {
            UnitsPlacementPhasePatch.OnFanCreated += HandleFanCreated;
        }

        override public void OnDeactivate()
        {
            UnitsPlacementPhasePatch.OnFanCreated -= HandleFanCreated;
        }

        public static void HandleFanCreated(BattleUnit fanUnit)
        {
            fanUnit.OnDeath += () => SpawnNewFan(fanUnit);
        }

        public static BattleUnit LastSpawnedFan;

        public static void SpawnNewFan(BattleUnit deadFan)
        {
            try
            {
                MelonLogger.Msg("FandomRallies: A Passionate Fan has died! Spawning a new one...");

                // Remove the dead fan from the fans list to prevent RestoreFans() from restoring it
                UnitsPlacementPhasePatch.fans.Remove(deadFan);

                var gameManager = GameManager.Instance;
                if (gameManager == null)
                {
                    MelonLogger.Error("FandomRallies: GameManager not found!");
                    return;
                }

                // Find TestGame instance to access grid and pawn creation
                var testGame = UnityEngine.Object.FindAnyObjectByType<TestGame>();
                if (testGame == null)
                {
                    MelonLogger.Error("FandomRallies: TestGame not found!");
                    return;
                }

                // Get GridManager via reflection
                var gridManager = AccessTools.Field(typeof(TestGame), "_gridManager").GetValue(testGame) as GridManager;
                if (gridManager == null)
                {
                    MelonLogger.Error("FandomRallies: GridManager not found!");
                    return;
                }

                // Get Test_PawnsPosition for pawn creation
                var testPawnsPosition = AccessTools.Field(typeof(TestGame), "_test_PawnsPosition").GetValue(testGame) as Test_PawnsPosition;
                if (testPawnsPosition == null)
                {
                    MelonLogger.Error("FandomRallies: Test_PawnsPosition not found!");
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

                // Get the player team manager
                BattleUnitsManager teamManager = gameManager.GetTeamManager(Team.Player);

                // Create the BattleUnit for the Player team (not EnemyAI!)
                var fan = new BattleUnit(fanUnitData, Team.Player, gridManager)
                {
                    DeploymentOrder = teamManager.BattleUnits.Count() + 1
                };

                // Add commands to the unit
                fan.AddCommands();

                // Register with FandomRallies so the next death also spawns a new fan
                HandleFanCreated(fan);

                // Add to the fans list so other systems recognize it as a fan
                UnitsPlacementPhasePatch.fans.Add(fan);

                // Add to team manager (this also enables inter-communications and registers death handler)
                teamManager.AddBattleUnit(fan);

                // Place the unit on the tile
                fan.PlaceOnTile(spawnTile);

                // Create the visual pawn (this handles health bar, name, etc.)
                testPawnsPosition.CreatePawn(fan);

                // Start executing commands immediately since battle is in progress
                fan.StartCommandsExecution();

                MelonLogger.Msg($"FandomRallies: Successfully spawned a new Passionate Fan at {spawnTile.Coords}!");
                LastSpawnedFan = fan;
            }
            catch (Exception ex)
            {
                MelonLogger.Error(ex);
                MelonLogger.Error(ex.StackTrace);
            }
        }
    }
}
