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
        private static Services.ILanguageProvider _languageProvider; // Optional, for Yandex WebGL
        private static Services.ISaveService _saveService; // Primary storage
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
        public static void Initialize(Services.ILanguageProvider languageProvider, Data.TranslationData translationData, Services.ISaveService saveService)
        {
            _languageProvider = languageProvider; // Keep for Yandex WebGL compatibility
            _translationData = translationData;
            _saveService = saveService;

            if (_translationData == null)
            {
                Debug.LogError("[LocalizationManager] TranslationData is null! Cannot load translations.");
                return;
            }

            // Get initial language from SaveData (single source of truth)
            var saveData = _saveService.LoadData();
            _currentLanguage = saveData?.language ?? "en";
            
            Debug.Log($"[LocalizationManager] Initialized with language: {_currentLanguage}");
            
            // Fire event to refresh any LocalizedText components that already ran Refresh() before we were ready
            OnLanguageChanged?.Invoke();
        }

        /// <summary>
        /// Get translated string for a key.
        /// Falls back to English if key not found for current language.
        /// </summary>
        public static string Get(string key)
        {
            if (_translationData == null)
            {
                Debug.LogError($"[LocalizationManager] ❌ Not initialized! Returning key: {key}");
                return $"[{key}]";
            }

            string translation = _translationData.GetTranslation(key, _currentLanguage);

            if (string.IsNullOrEmpty(translation))
            {
                Debug.LogWarning($"[LocalizationManager] ⚠️ Missing translation for key '{key}' in language '{_currentLanguage}'");
                
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
        /// Saves to storage and triggers OnLanguageChanged event.
        /// Called by GameSettings when language property is set.
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
                Debug.Log($"[LocalizationManager] Language already set to '{languageCode}', ignoring");
                return;
            }

            _currentLanguage = languageCode;
            
            // Save to SaveData (single source of truth)
            if (_saveService != null)
            {
                var saveData = _saveService.LoadData();
                if (saveData != null)
                {
                    saveData.language = languageCode;
                    _saveService.SaveData(saveData);
                    Debug.Log($"[LocalizationManager] Saved language '{languageCode}' to SaveData");
                }
            }

            // Notify platform provider (for Yandex WebGL compatibility)
            _languageProvider?.SwitchLanguage(languageCode);

            // Trigger UI refresh
            OnLanguageChanged?.Invoke();
            
            Debug.Log($"[LocalizationManager] ✅ Language switched to: {languageCode}");
        }
        
        /// <summary>
        /// Set current language without saving (used by GameSettings).
        /// GameSettings handles the save, we just update UI.
        /// </summary>
        public static void SetLanguage(string languageCode)
        {
            if (languageCode != "en" && languageCode != "ru")
            {
                Debug.LogWarning($"[LocalizationManager] Unsupported language '{languageCode}', defaulting to English");
                languageCode = "en";
            }

            if (_currentLanguage == languageCode)
            {
                return;  // No change needed
            }

            _currentLanguage = languageCode;
            
            // Notify platform provider (for Yandex WebGL compatibility)
            _languageProvider?.SwitchLanguage(languageCode);

            // Trigger UI refresh
            OnLanguageChanged?.Invoke();
            
            Debug.Log($"[LocalizationManager] ✅ Language set to: {languageCode}");
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
