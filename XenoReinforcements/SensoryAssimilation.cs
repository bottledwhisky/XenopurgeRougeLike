using HarmonyLib;
using MelonLoader;
using SpaceCommander;
using SpaceCommander.Area;
using SpaceCommander.GameFlow;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using static XenopurgeRougeLike.ModLocalization;

namespace XenopurgeRougeLike.XenoReinforcements
{
    // 感官同化：开局时揭示所有异形出生点
    public class SensoryAssimilation : Reinforcement
    {
        public SensoryAssimilation()
        {
            company = Company.Xeno;
            name = L("xeno.sensory_assimilation.name");
            description = L("xeno.sensory_assimilation.description");
            flavourText = L("xeno.sensory_assimilation.flavour");
        }

        protected static SensoryAssimilation instance;
        public static SensoryAssimilation Instance => instance ??= new();
    }

    /// <summary>
    /// Patch to reveal all enemy spawn points at the start of battle
    /// This patches BattlePhase.StartBattlePhase to reveal spawn point rooms after battle starts
    /// </summary>
    [HarmonyPatch(typeof(BattlePhase), "StartBattlePhase")]
    public static class SensoryAssimilation_RevealSpawnPoints_Patch
    {
        // Cache the FieldInfo for better performance
        private static FieldInfo _spawnPointsField = null;

        public static void Postfix()
        {
            if (!SensoryAssimilation.Instance.IsActive)
                return;

            var gameManager = GameManager.Instance;
            var spawner = gameManager.EnemiesSpawnerInBattle;

            if (spawner == null)
                return;

            // Use reflection to access the private _spawnPoints field
            if (_spawnPointsField == null)
            {
                _spawnPointsField = AccessTools.Field(typeof(SpawnEnemiesManager), "_spawnPoints");
            }

            if (_spawnPointsField == null)
            {
                MelonLogger.Error("SensoryAssimilation_RevealSpawnPoints_Patch: Unable to find _spawnPoints field via reflection.");
                return;
            }

            // Get the spawn points list
            var spawnPoints = _spawnPointsField.GetValue(spawner) as List<EnemyUnitSpawner>;

            if (spawnPoints == null || spawnPoints.Count == 0)
                return;

            // Reveal all rooms containing spawn points
            foreach (var spawnPoint in spawnPoints)
            {
                if (spawnPoint?.Tile?.Room != null)
                {
                    spawnPoint.Tile.Room.SetExplored(Enumerations.Team.Player);
                    foreach (var tile in spawnPoint.Tile.Room.TilesOfRoom)
                    {
                        SensoryAssimilation_VisionStorage.RevealedRoomTiles.Add(tile);
                    }
                }
            }
        }
    }

    public static class SensoryAssimilation_VisionStorage
    {
        public static HashSet<Tile> RevealedRoomTiles = [];
    }

    // Patch to clear buffs when mission ends
    [HarmonyPatch(typeof(TestGame), "EndGame")]
    public class EnhancedHacking_TestGame_EndGame_Patch
    {
        public static void Postfix()
        {
            SensoryAssimilation_VisionStorage.RevealedRoomTiles.Clear();
        }
    }

    /// <summary>
    /// Lv2: Grant vision for tiles in explored rooms
    /// </summary>
    [HarmonyPatch(typeof(PlayerLineOfSightTiles), "UpdateLineOfSightTiles")]
    public static class EnhancedHacking_AddVisionTiles_Patch
    {
        public static void Postfix(ref IEnumerable<Tile> __result)
        {
            if (SensoryAssimilation_VisionStorage.RevealedRoomTiles.Count == 0)
                return;

            // Combine the original line of sight with our revealed room tiles
            var combinedTiles = __result.ToList();
            combinedTiles.AddRange(SensoryAssimilation_VisionStorage.RevealedRoomTiles);
            __result = [.. combinedTiles.Distinct()];
        }
    }
}
