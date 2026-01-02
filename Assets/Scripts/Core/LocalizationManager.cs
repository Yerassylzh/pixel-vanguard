using System;
using UnityEngine;

namespace PixelVanguard.Core
{
    /// <summary>
    /// Singleton manager for game localization.
    /// Handles translation lookups, language switching, and UI refresh events.
    /// </summary>
    public static class LocalizationManager
    {
        private static Services.ILanguageProvider _languageProvider;
        private static Data.TranslationData _translationData;
        private static string _currentLanguage = "en";

        /// <summary>
        /// Event fired when language changes. UI components should subscribe to this.
        /// </summary>
        public static event Action OnLanguageChanged;

        /// <summary>
        /// Current language code ("en" or "ru").
        /// </summary>
        public static string CurrentLanguage => _currentLanguage;

        /// <summary>
        /// Initialize the localization system.
        /// Called from GameBootstrap.
        /// </summary>
        public static void Initialize(Services.ILanguageProvider languageProvider, Data.TranslationData translationData)
        {
            _languageProvider = languageProvider;
            _translationData = translationData;

            if (_translationData == null)
            {
                Debug.LogError("[LocalizationManager] TranslationData is null! Cannot load translations.");
                return;
            }

            // Get initial language from platform
            _currentLanguage = _languageProvider.GetCurrentLanguage();
        }

        /// <summary>
        /// Get translated string for a key.
        /// Falls back to English if key not found for current language.
        /// </summary>
        public static string Get(string key)
        {
            if (_translationData == null)
            {
                Debug.LogError($"[LocalizationManager] Not initialized! Returning key: {key}");
                return $"[{key}]";
            }

            string translation = _translationData.GetTranslation(key, _currentLanguage);

            if (string.IsNullOrEmpty(translation))
            {
                Debug.LogWarning($"[LocalizationManager] Missing translation for key '{key}' in language '{_currentLanguage}'");
                
                // Fallback to English
                if (_currentLanguage != "en")
                {
                    translation = _translationData.GetTranslation(key, "en");
                }

                // If still null, return the key
                if (string.IsNullOrEmpty(translation))
                {
                    return $"[{key}]";
                }
            }

            return translation;
        }

        /// <summary>
        /// Get formatted translated string with arguments.
        /// Example: GetFormatted("ui.gold.total", 1500) → "Total Gold: 1500"
        /// </summary>
        public static string GetFormatted(string key, params object[] args)
        {
            string template = Get(key);
            try
            {
                return string.Format(template, args);
            }
            catch (FormatException e)
            {
                Debug.LogError($"[LocalizationManager] Format error for key '{key}': {e.Message}");
                return template;
            }
        }

        /// <summary>
        /// Switch to a different language.
        /// Triggers OnLanguageChanged event for UI refresh.
        /// </summary>
        public static void SwitchLanguage(string languageCode)
        {
            if (languageCode != "en" && languageCode != "ru")
            {
                Debug.LogWarning($"[LocalizationManager] Unsupported language '{languageCode}', defaulting to English");
                languageCode = "en";
            }

            if (_currentLanguage == languageCode)
            {
                return;
            }

            _currentLanguage = languageCode;

            // Notify platform provider (saves to Yandex or PlayerPrefs)
            _languageProvider?.SwitchLanguage(languageCode);

            // Trigger UI refresh
            OnLanguageChanged?.Invoke();
        }

        /// <summary>
        /// Get list of supported languages.
        /// </summary>
        public static string[] GetSupportedLanguages()
        {
            return new string[] { "en", "ru" };
        }

        /// <summary>
        /// Get human-readable name for a language code.
        /// </summary>
        public static string GetLanguageName(string languageCode)
        {
            return languageCode switch
            {
                "en" => "English",
                "ru" => "Русский",
                _ => "Unknown"
            };
        }
    }
}
