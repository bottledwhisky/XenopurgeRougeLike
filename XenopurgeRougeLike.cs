using HarmonyLib;
using MelonLoader;
using MelonLoader.Utils;
using SpaceCommander;
using SpaceCommander.ActionCards;
using SpaceCommander.Commands;
using SpaceCommander.Database;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;


[assembly: MelonInfo(typeof(XenopurgeRougeLike.XenopurgeRougeLike), "Xenopurge RougeLike", "1.0.0", "Felix Hao")]
[assembly: MelonGame("Traptics", "Xenopurge")]

namespace XenopurgeRougeLike
{
    public class XenopurgeRougeLike : MelonMod
    {
        public static HarmonyLib.Harmony _HarmonyInstance;

        public static event Action OnUpdateEvent;

        public override void OnInitializeMelon()
        {
            Company.LoadSprites();
            MelonLogger.Msg("XenopurgeRougeLike initialized!");
            _HarmonyInstance = HarmonyInstance;

            Company.Synthetics.Affinities = Synthetics.Affinities;
            Company.Rockstar.Affinities = Rockstar.Affinities;
            Company.Xeno.Affinities = Xeno.Affinities;
            Company.Engineer.Affinities = Engineer.Affinities;
            Company.Support.Affinities = Support.Affinities;
            Company.Warrior.Affinities = Warrior.Affinities;
            Company.Gunslinger.Affinities = Gunslinger.Affinities;
            Company.Scavenger.Affinities = Scavenger.Affinities;
            Company.Clone.Affinities = Clone.Affinities;
            Company.Common.Affinities = Common.Affinities;
        }

        public override void OnUpdate()
        {
            if (Input.GetKeyUp(KeyCode.F11))
            {
                DebugPrintAllReinforcementToString();
            }
            else if (Input.GetKeyUp(KeyCode.F10))
            {
                LoggingUtils.LogAllDatabaseActionCards();
            }
            OnUpdateEvent?.Invoke();
        }

        private static void DebugPrintAllReinforcementToString()
        {
            MelonLogger.Msg("========== DEBUG: All Reinforcement ToString() ==========");

            foreach (var companyEntry in Company.Companies)
            {
                var companyType = companyEntry.Key;
                var company = companyEntry.Value;

                MelonLogger.Msg($"\n--- Company: {companyType} ---");

                try
                {
                    // Get the Reinforcements property using reflection
                    var reinforcementsProperty = company.ClassType.GetProperty("Reinforcements",
                        System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);

                    if (reinforcementsProperty != null)
                    {
                        var reinforcements = reinforcementsProperty.GetValue(null) as Dictionary<Type, Reinforcement>;

                        if (reinforcements != null)
                        {
                            foreach (var reinforcementEntry in reinforcements)
                            {
                                var reinforcementType = reinforcementEntry.Key;
                                var reinforcement = reinforcementEntry.Value;

                                try
                                {
                                    // For stackable reinforcements with currentStacks = 0, iterate through all possible stacks
                                    if (reinforcement.stackable && reinforcement.currentStacks == 0)
                                    {
                                        for (int stack = 1; stack <= reinforcement.maxStacks; stack++)
                                        {
                                            string description = reinforcement.GetDescriptionForStacks(stack);
                                        }
                                    }
                                    else
                                    {
                                        string toStringResult = reinforcement.ToString();
                                    }
                                }
                                catch (Exception ex)
                                {
                                    MelonLogger.Error($"  ✗ {reinforcementType.Name}: ERROR - {ex.Message}");
                                    MelonLogger.Error($"    Stack trace: {ex.StackTrace}");
                                }
                            }
                        }
                        else
                        {
                            MelonLogger.Msg($"  (No reinforcements dictionary found)");
                        }
                    }
                    else
                    {
                        MelonLogger.Msg($"  (No Reinforcements property found)");
                    }
                }
                catch (Exception ex)
                {
                    MelonLogger.Error($"  Error accessing company {companyType}: {ex.Message}");
                }
            }

            MelonLogger.Msg("\n========== END DEBUG ==========");
        }
        private static Queue<(BattleUnit unit, ICommand command)> _pendingCommands = new Queue<(BattleUnit, ICommand)>();

        public static void QueueCommand(BattleUnit bu, ICommand command)
        {
            _pendingCommands.Enqueue((bu, command));
        }

        static OverrideCommands_UnitAndTileAsTarget_CardSO _OverrideCommands_UnitAndTileTarget_CardSO_Collect;
        public static OverrideCommands_UnitAndTileAsTarget_CardSO OverrideCommands_UnitAndTileTarget_CardSO_Collect
        {
            get
            {
                if (_OverrideCommands_UnitAndTileTarget_CardSO_Collect == null)
                {
                    _OverrideCommands_UnitAndTileTarget_CardSO_Collect = AssetsDatabase.Instance.ActionCards.First(c =>
                    {
                        if (c is OverrideCommands_UnitAndTileAsTarget_CardSO cc)
                        {
                            var _commandDataSO = AccessTools.Field(typeof(OverrideCommands_UnitAndTileAsTarget_CardSO), "_commandDataSO").GetValue(c) as MoveToSpecificLocationForActionCommandDataSO;
                            if (_commandDataSO != null && _commandDataSO.ActionCommandDataSO is CollectCommandDataSO)
                            {
                                return true;
                            }
                        }
                        return false;
                    }) as OverrideCommands_UnitAndTileAsTarget_CardSO;
                }
                return _OverrideCommands_UnitAndTileTarget_CardSO_Collect;
            }
        }

        public static void ReplaceCommandActual(BattleUnit bu, ICommand command)
        {
            MelonLogger.Msg($"ReplaceCommandActual: {bu.UnitName} {command.CommandName}");
            if (command is CollectCommand)
            {
                ActionCard.CostOfActionCard costOfActionCard = default;
                costOfActionCard.ActionCardId = OverrideCommands_UnitAndTileTarget_CardSO_Collect.Info.Id;
                bu.CommandsManager.OverrideCurrentCommandFromActionCard(command, costOfActionCard);
            }
            else
            {
                bu.CommandsManager.ReplaceCommand(command);
            }
        }

        public override void OnLateUpdate()
        {
            while (_pendingCommands.Count > 0)
            {
                var (unit, command) = _pendingCommands.Dequeue();
                ReplaceCommandActual(unit, command);
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

}
