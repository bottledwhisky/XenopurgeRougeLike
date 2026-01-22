using HarmonyLib;
using MelonLoader;
using MelonLoader.Utils;
using SaveSystem;
using SpaceCommander;
using SpaceCommander.ActionCards;
using SpaceCommander.Area;
using SpaceCommander.BattleManagement;
using SpaceCommander.BattleManagement.UI;
using SpaceCommander.Database;
using SpaceCommander.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TimeSystem;
using UnityEngine;
using XenopurgeRougeLike.RockstarReinforcements;
using static SpaceCommander.Enumerations;


[assembly: MelonInfo(typeof(XenopurgeRougeLike.XenopurgeRougeLike), "Xenopurge RougeLike", "1.0.0", "Felix Hao")]
[assembly: MelonGame("Traptics", "Xenopurge")]

namespace XenopurgeRougeLike
{
    public class XenopurgeRougeLike : MelonMod
    {
        public static HarmonyLib.Harmony _HarmonyInstance;
        public static List<Action<List<Tuple<int, Reinforcement>>>> WeightModifiers = [];
        public static List<Reinforcement> acquiredReinforcements = [];
        // Sample choices - replace with your actual data
        public static List<Reinforcement> choices = null;

        public override void OnInitializeMelon()
        {
            Company.LoadSprites();
            MelonLogger.Msg("XenopurgeRougeLike initialized!");
            _HarmonyInstance = HarmonyInstance;

            Company.Synthetics.Affinities = Synthetics.Affinities;
            Company.Rockstar.Affinities = Rockstar.Affinities;
            Company.Xeno.Affinities = Xeno.Affinities;

            RockstarReinforcements.RockstarAffinity4.Instance.IsActive = true;
        }

        public override void OnUpdate()
        {
            if (Input.GetKeyUp(KeyCode.F11))
            {
                var bu = FandomRallies.LastSpawnedFan;
                if (bu == null)
                {
                    MelonLogger.Msg("LastSpawnedFan is null");
                    return;
                }
                var los = bu.LineOfSight;
                var mm = bu.MovementManager;

                // Movement Manager's current coords
                Vector2Int currentCoords = mm.CurrentTileCoords;      // Current tile coords
                Vector2Int nextCoords = mm.NextTileCoords;            // Next tile coords (if moving)
                Vector3 currentPosition = mm.CurrentTilePosition;     // World position
                Tile currentTile = mm.CurrentTile;                    // Current Tile object

                // Visible tiles from LineOfSight
                IEnumerable<Tile> visibleTiles = los.Tiles;

                // Example dump:
                MelonLogger.Msg($"=== LineOfSight Debug for {bu.UnitName} {bu.UnitId} ===");
                MelonLogger.Msg($"Current Coords: {currentCoords}");
                MelonLogger.Msg($"Next Coords: {nextCoords}");
                MelonLogger.Msg($"World Position: {currentPosition}");
                MelonLogger.Msg($"Current Tile: {currentTile?.Coords}");
                MelonLogger.Msg($"Visible Tiles Count: {visibleTiles.Count()}");

                foreach (var tile in visibleTiles)
                {
                    MelonLogger.Msg($"  - Tile at {tile.Coords}");
                }

                var gameManager = GameManager.Instance;
                BattleUnitsManager teamManager = gameManager.GetTeamManager(Team.Player);

                foreach(var bu2 in teamManager.BattleUnits)
                {
                    MelonLogger.Msg($"=== Player has {bu2.UnitName} {bu2.UnitId} ===");
                }
            }
        }

        public static List<Reinforcement> GetChoices()
        {
            int nChoices = 3;
            List<Tuple<int, Reinforcement>> weightedChoices = [];

            foreach (var company in Company.Companies.Values)
            {
                MelonLogger.Msg($"Checking company: {company.Name}");
                if (!(bool)AccessTools.Method(company.ClassType, "IsAvailable").Invoke(null, null))
                {
                    MelonLogger.Msg($"Company {company.Name} is not available. Skipping.");
                    continue;
                }
                MelonLogger.Msg($"Company {company.Name} is available. Retrieving reinforcements.");

                var reinforcements = company.ClassType
                    .GetProperty("Reinforcements")
                    .GetValue(null) as Dictionary<Type, Reinforcement>;
                MelonLogger.Msg($"Found {reinforcements.Count} reinforcements for company {company.Name}.");
                foreach (var reinforcement in reinforcements.Values)
                {
                    // Skip already got reinforcements
                    if (acquiredReinforcements.Contains(reinforcement) && (!reinforcement.stackable || reinforcement.currentStacks == reinforcement.maxStacks))
                    {
                        continue;
                    }
                    int weight = Reinforcement.RarityWeights[reinforcement.rarity];
                    weightedChoices.Add(new Tuple<int, Reinforcement>(weight, reinforcement));
                }
            }

            MelonLogger.Msg($"Total weighted choices before modifiers: {weightedChoices.Count}");

            // Let other classes modify the weights
            foreach (var modifier in WeightModifiers)
            {
                modifier(weightedChoices);
            }

            // Now select nChoices using weighted random selection
            List<Reinforcement> selected = [];
            int totalWeight = weightedChoices.Sum(x => x.Item1);
            System.Random rng = new();

            for (int i = 0; i < nChoices && weightedChoices.Count > 0; i++)
            {
                int roll = rng.Next(totalWeight);
                int cumulative = 0;

                for (int j = 0; j < weightedChoices.Count; j++)
                {
                    cumulative += weightedChoices[j].Item1;
                    if (roll < cumulative)
                    {
                        selected.Add(weightedChoices[j].Item2);
                        totalWeight -= weightedChoices[j].Item1;
                        weightedChoices.RemoveAt(j); // Remove to avoid duplicates
                        break;
                    }
                }
            }

            MelonLogger.Msg($"Selected {selected.Count} reinforcements.");

            return selected;
        }

        public static void AcquireReinforcement(Reinforcement reinforcement)
        {
            reinforcement.currentStacks += 1;
            if (acquiredReinforcements.Contains(reinforcement))
            {
                reinforcement.currentStacks += 1;
            }
            else
            {
                reinforcement.IsActive = true;
                acquiredReinforcements.Add(reinforcement);
                var company = reinforcement.company.Type;
                var nExsitingCompanyReinforces = acquiredReinforcements.Where(r => r.company.Type == company).Count();
                CompanyAffinity affinityToDisable = null;
                CompanyAffinity affinityToEnable = null;
                foreach (var affiny in reinforcement.company.Affinities)
                {
                    int requiredNReinforces = affiny.unlockLevel;
                    if (requiredNReinforces > nExsitingCompanyReinforces)
                    {
                        break;
                    }
                    affinityToDisable = affinityToEnable;
                    affinityToEnable = affiny;
                }
                if (affinityToEnable != null)
                {
                    if (affinityToDisable != null)
                    {
                        affinityToDisable.IsActive = false;
                    }
                    affinityToEnable.IsActive = true;
                }
            }

            if (SaveFile_Patch.lastSaveName != null)
            {
                SaveFile_Patch.Postfix(SaveFile_Patch.lastSaveName);
            }
        }

        public static Sprite LoadCustomSpriteAsset(string path)
        {
            try
            {
                string spritePath = Path.Combine(MelonEnvironment.UserDataDirectory, "XenopurgeRougeLike", path);

                if (!File.Exists(spritePath))
                {
                    MelonLogger.Warning($"Sprite file not found: {spritePath}");
                    return null;
                }

                byte[] fileData = File.ReadAllBytes(spritePath);

                // Create texture with specific format and make it readable
                Texture2D iconTexture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                iconTexture.filterMode = FilterMode.Bilinear;
                iconTexture.wrapMode = TextureWrapMode.Clamp;

                // ImageConversion.LoadImage returns bool indicating success
                if (!ImageConversion.LoadImage(iconTexture, fileData))
                {
                    MelonLogger.Error("Failed to load image data into texture");
                    return null;
                }

                // Don't call Apply() after LoadImage - it already does this
                // But we need to prevent garbage collection
                iconTexture.hideFlags = HideFlags.DontUnloadUnusedAsset;

                MelonLogger.Msg($"Loaded texture: {iconTexture.width}x{iconTexture.height}, format: {iconTexture.format}");

                // Create the sprite with proper pivot and pixels per unit
                Sprite sprite = Sprite.Create(
                    iconTexture,
                    new Rect(0, 0, iconTexture.width, iconTexture.height),
                    new Vector2(0.5f, 0.5f),
                    100f,
                    0,
                    SpriteMeshType.FullRect
                );

                // Prevent garbage collection of the sprite
                sprite.hideFlags = HideFlags.DontUnloadUnusedAsset;

                MelonLogger.Msg($"Created sprite: {sprite.name}, rect: {sprite.rect}");
                return sprite;
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"Error loading custom sprite asset: {ex}");
            }
            return null;
        }
    }

    [HarmonyPatch(typeof(GameManager), "GiveEndGameRewards")]
    public static class GameManager_GiveEndGameRewards_Patch
    {
        public static void Prefix(bool victory)
        {
            try
            {
                if (!victory)
                    return;
                var reinforcements = XenopurgeRougeLike.GetChoices();
                XenopurgeRougeLike.choices = reinforcements;
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"Error in GameManager_GiveEndGameRewards_Patch: {ex}");
                MelonLogger.Error(ex.StackTrace);
            }
        }
    }

    [HarmonyPatch(typeof(SaveLoadManager))]
    [HarmonyPatch("SaveFile")]
    public class SaveFile_Patch
    {
        public static string lastSaveName;

        [HarmonyPostfix]
        public static void Postfix(string saveName)
        {
            MelonLogger.Msg("[SaveFile_Patch] Postfix called - Saving reinforcements");
            lastSaveName = saveName;

            try
            {
                // Serialize the reinforcements list
                List<string> reinforcementData = new List<string>();
                foreach (Reinforcement reinforcement in XenopurgeRougeLike.acquiredReinforcements)
                {
                    if (reinforcement != null)
                    {
                        string serialized = reinforcement.GetType().FullName;
                        reinforcementData.Add(serialized);
                        MelonLogger.Msg($"[SaveFile_Patch] Serialized reinforcement: {serialized}");
                    }
                }

                // Save to a separate file in the save folder
                string saveFolderPath = Path.Combine(Application.persistentDataPath, saveName);
                string reinforcementsFilePath = Path.Combine(saveFolderPath, "reinforcements.json");

                if (!Directory.Exists(saveFolderPath))
                {
                    Directory.CreateDirectory(saveFolderPath);
                }

                string json = SaveLoadUtils.Serialize(reinforcementData);
                File.WriteAllText(reinforcementsFilePath, json);

                MelonLogger.Msg($"[SaveFile_Patch] Successfully saved {reinforcementData.Count} reinforcements to {reinforcementsFilePath}");
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"[SaveFile_Patch] Error saving reinforcements: {ex.Message}");
                MelonLogger.Error($"Stack trace: {ex.StackTrace}");
            }
        }
    }

    [HarmonyPatch(typeof(SaveLoadManager))]
    [HarmonyPatch("LoadFile")]
    public class LoadFile_Patch
    {
        [HarmonyPostfix]
        public static void Postfix(string saveName)
        {
            MelonLogger.Msg("[LoadFile_Patch] Postfix called - Loading reinforcements");

            try
            {
                // Clear existing reinforcements
                XenopurgeRougeLike.acquiredReinforcements.Clear();

                // Load from the reinforcements file
                string saveFolderPath = Path.Combine(Application.persistentDataPath, saveName);
                string reinforcementsFilePath = Path.Combine(saveFolderPath, "reinforcements.json");

                if (!File.Exists(reinforcementsFilePath))
                {
                    MelonLogger.Msg("[LoadFile_Patch] No reinforcements file found - starting fresh");
                    return;
                }

                string json = File.ReadAllText(reinforcementsFilePath);
                MelonLogger.Msg($"[LoadFile_Patch] Loaded from {reinforcementsFilePath}");
                List<string> reinforcementData = SaveLoadUtils.Deserialize<List<string>>(json);

                foreach (string serialized in reinforcementData)
                {
                    Type type = Type.GetType(serialized);
                    if (type == null)
                    {
                        MelonLogger.Warning($"[LoadFile_Patch] Could not find type: {serialized}");
                        continue;
                    }

                    // Get the singleton instance of the reinforcement
                    var instanceProperty = type.GetProperty("Instance", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                    if (instanceProperty != null)
                    {
                        Reinforcement reinforcement = (Reinforcement)instanceProperty.GetValue(null);
                        if (reinforcement != null)
                        {
                            XenopurgeRougeLike.acquiredReinforcements.Add(reinforcement);
                            reinforcement.IsActive = true; // Activate the reinforcement
                            MelonLogger.Msg($"[LoadFile_Patch] Loaded and activated reinforcement: {reinforcement.GetType().Name}");
                        }
                    }
                    else
                    {
                        MelonLogger.Warning($"[LoadFile_Patch] Type {serialized} does not have an Instance property");
                    }
                }

                MelonLogger.Msg($"[LoadFile_Patch] Successfully loaded {XenopurgeRougeLike.acquiredReinforcements.Count} reinforcements");
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"[LoadFile_Patch] Error loading reinforcements: {ex.Message}");
                MelonLogger.Error($"Stack trace: {ex.StackTrace}");

                // On error, clear reinforcements to prevent issues
                foreach (var reinforcement in XenopurgeRougeLike.acquiredReinforcements)
                {
                    reinforcement.IsActive = false;
                }
                XenopurgeRougeLike.acquiredReinforcements.Clear();
            }
        }
    }
}
