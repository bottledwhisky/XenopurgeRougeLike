using HarmonyLib;
using MelonLoader;
using MelonLoader.Utils;
using SaveSystem;
using SpaceCommander;
using SpaceCommander.Area;
using SpaceCommander.Commands;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

        public static event Action OnUpdateEvent;

        public override void OnInitializeMelon()
        {
            Company.LoadSprites();
            MelonLogger.Msg("XenopurgeRougeLike initialized!");
            _HarmonyInstance = HarmonyInstance;

            Company.Synthetics.Affinities = Synthetics.Affinities;
            Company.Rockstar.Affinities = Rockstar.Affinities;
            Company.Xeno.Affinities = Xeno.Affinities;
        }

        public override void OnUpdate()
        {
            if (Input.GetKeyUp(KeyCode.F11))
            {
                var gameManager = GameManager.Instance;
                var playerTM = gameManager.GetTeamManager(Team.Player);
                foreach (var unit in playerTM.BattleUnits)
                {
                    MelonLogger.Msg($"Interrupting {unit.UnitName} {unit.CommandsManager.CurrentCommand?.CommandName}");
                    foreach (var cmd in unit.CommandsManager.Commands)
                    {
                        var _lockTarget = AccessTools.FindIncludingBaseTypes(cmd.GetType(), t => t.GetField("_lockTarget", AccessTools.all))?.GetValue(cmd);
                        if (_lockTarget != null && _lockTarget is LockCloseCombatTarget lcbt)
                        {
                            var damagableTarget = lcbt.DamagableTarget;
                            MelonLogger.Msg($"Interrupting {cmd.GetType().Name} of {unit.UnitName} on {damagableTarget}");
                            AccessTools.Field(lcbt.GetType(), "_target").SetValue(lcbt, null);
                            lcbt.UpdateTarget(unit.MovementManager.CurrentTileCoords);
                        }
                        else if (_lockTarget != null && _lockTarget is LockTarget lt)
                        {
                            var damagableTarget = lt.DamagableTarget;
                            MelonLogger.Msg($"Interrupting {cmd.GetType().Name} of {unit.UnitName} on {damagableTarget}");
                            AccessTools.Field(lt.GetType(), "_target").SetValue(lt, null);
                            lt.UpdateTarget(unit.MovementManager.CurrentTileCoords);
                        }
                    }

                    unit.CommandsManager.CurrentCommand?.Interrupt();
                    unit.CommandsManager.StopCommandsExecution();
                    unit.CommandsManager.StartCommandsExecution();
                }
            }
            else if (Input.GetKeyUp(KeyCode.F10))
            {
                var gameManager = GameManager.Instance;
                var playerTM = gameManager.GetTeamManager(Team.Player);
                foreach (var unit in playerTM.BattleUnits)
                {
                    MelonLogger.Msg($"{unit.UnitName} has Speed {unit.Speed}");
                }
            }
            OnUpdateEvent?.Invoke();
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
                MelonLogger.Msg($"Acquired reinforcements: {acquiredReinforcements.Select(r => r.Name).Join(delimiter: " ")}");
                foreach (var reinforcement in reinforcements.Values)
                {
                    MelonLogger.Msg($"  - Reinforcement: {reinforcement.Name}");
                    // Skip already got reinforcements
                    if (acquiredReinforcements.Contains(reinforcement) && (!reinforcement.stackable || reinforcement.currentStacks == reinforcement.maxStacks))
                    {
                        MelonLogger.Msg($"    - Skipped: already acquired {reinforcement.Name} stackable={reinforcement.stackable} {reinforcement.currentStacks}/{reinforcement.maxStacks}");
                        continue;
                    }
                    MelonLogger.Msg($"    - Added: {reinforcement.Name} stackable={reinforcement.stackable} {reinforcement.currentStacks}/{reinforcement.maxStacks}");
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
            if (!acquiredReinforcements.Contains(reinforcement))
            {
                reinforcement.IsActive = true;
                acquiredReinforcements.Add(reinforcement);
                CalculateAffinity(reinforcement.company);
            }

            if (SaveFile_Patch.lastSaveName != null)
            {
                SaveFile_Patch.Postfix(SaveFile_Patch.lastSaveName);
            }
        }

        public static void CalculateAffinity(Company company)
        {
            var nExsitingCompanyReinforces = acquiredReinforcements.Where(r => r.company.Type == company.Type).Count();
            CompanyAffinity affinityToDisable = null;
            CompanyAffinity affinityToEnable = null;
            foreach (var affiny in company.Affinities)
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
                string saveFolderPath = Path.Combine(Application.persistentDataPath, saveName);
                if (!Directory.Exists(saveFolderPath))
                {
                    Directory.CreateDirectory(saveFolderPath);
                }

                // Serialize the reinforcements list with stacks and custom state
                List<Dictionary<string, object>> reinforcementData = new List<Dictionary<string, object>>();
                foreach (Reinforcement reinforcement in XenopurgeRougeLike.acquiredReinforcements)
                {
                    if (reinforcement != null)
                    {
                        var data = new Dictionary<string, object>
                        {
                            { "type", reinforcement.GetType().FullName },
                            { "stacks", reinforcement.currentStacks }
                        };

                        // Save custom state if available
                        var customState = reinforcement.SaveState();
                        if (customState != null && customState.Count > 0)
                        {
                            data["customState"] = customState;
                            MelonLogger.Msg($"[SaveFile_Patch] Saving custom state for {reinforcement.GetType().Name}: {customState.Count} entries");
                        }

                        reinforcementData.Add(data);
                        MelonLogger.Msg($"[SaveFile_Patch] Serialized reinforcement: {reinforcement.GetType().Name} with {reinforcement.currentStacks} stacks");
                    }
                }

                string reinforcementsFilePath = Path.Combine(saveFolderPath, "reinforcements.json");
                string json = SaveLoadUtils.Serialize(reinforcementData);
                File.WriteAllText(reinforcementsFilePath, json);

                MelonLogger.Msg($"[SaveFile_Patch] Successfully saved {reinforcementData.Count} reinforcements to {reinforcementsFilePath}");

                // Save affinity states
                List<Dictionary<string, object>> affinityData = new List<Dictionary<string, object>>();
                foreach (var company in Company.Companies.Values)
                {
                    if (company.Affinities != null)
                    {
                        foreach (var affinity in company.Affinities)
                        {
                            if (affinity.IsActive)
                            {
                                var customState = affinity.SaveState();
                                if (customState != null && customState.Count > 0)
                                {
                                    var data = new Dictionary<string, object>
                                    {
                                        { "type", affinity.GetType().FullName },
                                        { "customState", customState }
                                    };
                                    affinityData.Add(data);
                                    MelonLogger.Msg($"[SaveFile_Patch] Saving custom state for affinity {affinity.GetType().Name}: {customState.Count} entries");
                                }
                            }
                        }
                    }
                }

                if (affinityData.Count > 0)
                {
                    string affinityFilePath = Path.Combine(saveFolderPath, "affinities.json");
                    string affinityJson = SaveLoadUtils.Serialize(affinityData);
                    File.WriteAllText(affinityFilePath, affinityJson);
                    MelonLogger.Msg($"[SaveFile_Patch] Successfully saved {affinityData.Count} affinity states to {affinityFilePath}");
                }
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

                HashSet<Company> companies = [];

                var reinforcementDataList = SaveLoadUtils.Deserialize<List<Dictionary<string, object>>>(json);
                foreach (var data in reinforcementDataList)
                {
                    string typeName = data["type"] as string;
                    int stacks = data.ContainsKey("stacks") ? Convert.ToInt32(data["stacks"]) : 1;

                    Type type = Type.GetType(typeName);
                    if (type == null)
                    {
                        MelonLogger.Warning($"[LoadFile_Patch] Could not find type: {typeName}");
                        continue;
                    }

                    var instanceProperty = type.GetProperty("Instance", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                    if (instanceProperty != null)
                    {
                        Reinforcement reinforcement = (Reinforcement)instanceProperty.GetValue(null);
                        if (reinforcement != null)
                        {
                            reinforcement.currentStacks = stacks;
                            XenopurgeRougeLike.acquiredReinforcements.Add(reinforcement);
                            companies.Add(reinforcement.company);
                            reinforcement.IsActive = true;

                            // Load custom state if present
                            if (data.ContainsKey("customState") && data["customState"] is Dictionary<string, object> customState)
                            {
                                reinforcement.LoadState(customState);
                                MelonLogger.Msg($"[LoadFile_Patch] Loaded custom state for {reinforcement.GetType().Name}: {customState.Count} entries");
                            }

                            MelonLogger.Msg($"[LoadFile_Patch] Loaded and activated reinforcement: {reinforcement.GetType().Name} with {stacks} stacks");
                        }
                    }
                    else
                    {
                        MelonLogger.Warning($"[LoadFile_Patch] Type {typeName} does not have an Instance property");
                    }
                }

                MelonLogger.Msg($"[LoadFile_Patch] Successfully loaded {XenopurgeRougeLike.acquiredReinforcements.Count} reinforcements");

                foreach (Company company in companies)
                {
                    XenopurgeRougeLike.CalculateAffinity(company);
                }

                // Load affinity states
                string affinityFilePath = Path.Combine(saveFolderPath, "affinities.json");
                if (File.Exists(affinityFilePath))
                {
                    string affinityJson = File.ReadAllText(affinityFilePath);
                    var affinityDataList = SaveLoadUtils.Deserialize<List<Dictionary<string, object>>>(affinityJson);

                    foreach (var data in affinityDataList)
                    {
                        string typeName = data["type"] as string;
                        Type type = Type.GetType(typeName);
                        if (type == null)
                        {
                            MelonLogger.Warning($"[LoadFile_Patch] Could not find affinity type: {typeName}");
                            continue;
                        }

                        var instanceProperty = type.GetProperty("Instance", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                        if (instanceProperty != null)
                        {
                            CompanyAffinity affinity = (CompanyAffinity)instanceProperty.GetValue(null);
                            if (affinity != null && affinity.IsActive && data.ContainsKey("customState"))
                            {
                                var customState = data["customState"] as Dictionary<string, object>;
                                if (customState != null)
                                {
                                    affinity.LoadState(customState);
                                    MelonLogger.Msg($"[LoadFile_Patch] Loaded custom state for affinity {affinity.GetType().Name}: {customState.Count} entries");
                                }
                            }
                        }
                    }

                    MelonLogger.Msg($"[LoadFile_Patch] Successfully loaded {affinityDataList.Count} affinity states");
                }
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
