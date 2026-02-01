using HarmonyLib;
using MelonLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UI.Settings;

namespace XenopurgeRougeLike
{
    public class LocalizedString(string key, params object[] args)
    {
        private readonly string _key = key;
        private readonly object[] _args = args;

        public override string ToString()
        {
            var _translations = I18nData._translations;
            if (_translations.ContainsKey(_key) &&
                _translations[_key].ContainsKey(ModLocalization.CurrentLanguage))
            {
                string text = _translations[_key][ModLocalization.CurrentLanguage];
                return string.Format(text, _args);
            }

            // Fallback to English
            if (ModLocalization.CurrentLanguage != "en" &&
                _translations.ContainsKey(_key) &&
                _translations[_key].ContainsKey("en"))
            {
                string text = _translations[_key]["en"];
                return string.Format(text, _args);
            }

            return _key;
        }

        public static implicit operator string(LocalizedString _this) => _this.ToString();
    }

    public static class ModLocalization
    {
        private static string _currentLanguage = "en";

        private static readonly Dictionary<string, Dictionary<string, string>> _translations = I18nData._translations;

        // Event that fires when language changes
        public static event Action<string> OnLanguageChanged;

        public static void SetLanguage(string languageCode)
        {
            if (string.IsNullOrEmpty(languageCode))
            {
                languageCode = "en";
            }

            // Normalize language code
            string normalizedCode = languageCode.ToLower();
            string oldLanguage = _currentLanguage;

            // Check if any translation exists for this language
            bool languageExists = _translations.Values.Any(dict => dict.ContainsKey(normalizedCode));

            if (languageExists)
            {
                _currentLanguage = normalizedCode;
            }
            else
            {
                // Try without region code (e.g., "en-US" -> "en")
                string baseCode = normalizedCode.Split('-')[0];
                bool baseLanguageExists = _translations.Values.Any(dict => dict.ContainsKey(baseCode));

                if (baseLanguageExists)
                {
                    _currentLanguage = baseCode;
                }
                else
                {
                    _currentLanguage = "en";
                }
            }

            // Fire event if language actually changed
            if (oldLanguage != _currentLanguage)
            {
                OnLanguageChanged?.Invoke(_currentLanguage);
            }
        }

        public static LocalizedString L(string key, params object[] args)
        {
            return new LocalizedString(key, args);
        }

        public static string CurrentLanguage => _currentLanguage;
    }

    // Patch SettingsManager to detect language changes
    [HarmonyPatch(typeof(SettingsManager))]
    public class SettingsManager_Patches
    {
        // Hook into LoadPlayerPrefs to get initial language
        [HarmonyPatch("LoadPlayerPrefs")]
        [HarmonyPostfix]
        public static void LoadPlayerPrefs_Postfix(SettingsManager __instance)
        {
            try
            {
                if (__instance.HasLoadedSettings)
                {
                    string currentLanguage = __instance.Language;
                    ModLocalization.SetLanguage(currentLanguage);
                }
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"Error in LoadPlayerPrefs patch: {ex}");
            }
        }

        // Hook into SetLanguage to detect language changes
        [HarmonyPatch("SetLanguage")]
        [HarmonyPostfix]
        public static void SetLanguage_Postfix(SettingsManager __instance, string value)
        {
            try
            {
                ModLocalization.SetLanguage(value);
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"Error in SetLanguage patch: {ex}");
            }
        }
    }
}