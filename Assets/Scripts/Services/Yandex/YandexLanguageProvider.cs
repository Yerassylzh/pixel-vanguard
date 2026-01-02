#if UNITY_WEBGL
using UnityEngine;
using YG;

namespace PixelVanguard.Services
{
    /// <summary>
    /// Yandex-specific language provider for WebGL builds.
    /// Uses YG2.lang for language detection and YG2.SwitchLanguage for manual switching.
    /// </summary>
    public class YandexLanguageProvider : ILanguageProvider
    {
        public void Initialize()
        {
            Debug.Log("[YandexLanguageProvider] Initialized - using Yandex language detection");

            // Subscribe to Yandex language events
            YG2.onSwitchLang += OnYandexLanguageChanged;

            Debug.Log($"[YandexLanguageProvider] Current Yandex language: {YG2.lang}");
        }

        public string GetCurrentLanguage()
        {
            string yandexLang = YG2.lang;
            
            // Validate against supported languages
            if (yandexLang == "ru")
                return "ru";
            
            // Default to English for all other languages
            Debug.Log($"[YandexLanguageProvider] Yandex lang '{yandexLang}' → using English");
            return "en";
        }

        public string[] GetSupportedLanguages()
        {
            return new string[] { "en", "ru" };
        }

        public void SwitchLanguage(string languageCode)
        {
            // Validate language code
            if (languageCode != "en" && languageCode != "ru")
            {
                Debug.LogWarning($"[YandexLanguageProvider] Unsupported language: {languageCode}");
                return;
            }

            // Tell Yandex to switch language
            YG2.SwitchLanguage(languageCode);
            Debug.Log($"[YandexLanguageProvider] Switched Yandex language to: {languageCode}");
        }

        /// <summary>
        /// Called when Yandex language changes externally (e.g., user changes in their account).
        /// </summary>
        private void OnYandexLanguageChanged(string newLang)
        {
            Debug.Log($"[YandexLanguageProvider] Yandex language changed to: {newLang}");
            
            // Map to supported language
            string mappedLang = (newLang == "ru") ? "ru" : "en";
            
            // Update LocalizationManager
            Core.LocalizationManager.SwitchLanguage(mappedLang);
        }
    }
}
#endif
