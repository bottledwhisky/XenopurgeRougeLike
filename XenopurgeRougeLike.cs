using HarmonyLib;
using MelonLoader;
using MelonLoader.Utils;
using SpaceCommander;
using SpaceCommander.ActionCards;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;


[assembly: MelonInfo(typeof(XenopurgeRougeLike.XenopurgeRougeLike), "Xenopurge RougeLike", "1.0.0", "Felix Hao")]
[assembly: MelonGame("Traptics", "Xenopurge")]

namespace XenopurgeRougeLike
{
    public static class ActionCardDumper
    {
        public static string DumpAllActionCards()
        {
            // Find all ActionCardSO assets in the project
            ActionCardSO[] allCards = Resources.FindObjectsOfTypeAll<ActionCardSO>();

            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"=== Action Cards Dump ({allCards.Length} cards found) ===\n");

            foreach (ActionCardSO card in allCards)
            {
                sb.AppendLine(DumpActionCard(card));
                sb.AppendLine(new string('-', 50));
            }

            return sb.ToString();
        }

        public static string DumpActionCard(ActionCardSO card)
        {
            if (card == null) return "null";

            ActionCardInfo info = card.Info;
            StringBuilder sb = new StringBuilder();

            sb.AppendLine($"Asset Name: {card.name}");
            sb.AppendLine($"Type: {card.GetType().Name}");
            sb.AppendLine($"Id: {info.Id}");
            sb.AppendLine($"Card Name: {info.CardName}");
            sb.AppendLine($"Description: {info.CardDescription}");
            sb.AppendLine($"Group: {info.Group}");
            sb.AppendLine($"Sorting Order: {info.SortingOrder}");
            sb.AppendLine($"Buying Cost: {info.BuyingCost}");
            sb.AppendLine($"Access Points Cost: {info.AccessPointsCost}");
            sb.AppendLine($"Bioweave Points Cost: {info.BioweavePointsCost}");
            sb.AppendLine($"Uses: {(info.Uses == 0 ? "Unlimited" : info.Uses.ToString())}");
            sb.AppendLine($"Available On Deployment Phase: {info.AvailableOnDeploymentPhase}");

            if (info.AvailableOnDeploymentPhase)
            {
                sb.AppendLine($"  - Access Points Cost (Deployment): {info.AccessPointsCostOnDeploymentPhase}");
                sb.AppendLine($"  - Bioweave Points Cost (Deployment): {info.BioweavePointsCostOnDeploymentPhase}");
            }

            if (info.SquadIdsThatCannotUseCommand != null && info.SquadIdsThatCannotUseCommand.Count > 0)
            {
                sb.AppendLine($"Excluded Squad IDs: {string.Join(", ", info.SquadIdsThatCannotUseCommand)}");
            }

            return sb.ToString();
        }

        // Optional: Log to console
        public static void LogAllActionCards()
        {
            Debug.Log(DumpAllActionCards());
        }

        // Optional: Save to file
        public static void SaveToFile(string path = "ActionCardsDump.txt")
        {
            string dump = DumpAllActionCards();
            System.IO.File.WriteAllText(path, dump);
            Debug.Log($"Action cards dumped to: {path}");
        }
    }

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
            if (Input.GetKeyUp(KeyCode.F12))
            {
                ActionCardDumper.SaveToFile("D:\\projects\\xenopurge\\ActionCardSO.txt");
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
                    .GetField("Reinforcements")
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
}
