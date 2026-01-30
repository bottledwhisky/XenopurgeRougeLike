using MelonLoader;
using MelonLoader.Utils;
using System;
using System.IO;
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
        }

        public override void OnUpdate()
        {
            if (Input.GetKeyUp(KeyCode.F11))
            {
            }
            else if (Input.GetKeyUp(KeyCode.F10))
            {
                LoggingUtils.LogAllDatabaseActionCards();
            }
            OnUpdateEvent?.Invoke();
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
