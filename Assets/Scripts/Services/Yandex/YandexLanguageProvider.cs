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
            // Subscribe to Yandex language events
            YG2.onSwitchLang += OnYandexLanguageChanged;
        }

        public string GetCurrentLanguage()
        {
            string yandexLang = YG2.lang;
            
            // Validate against supported languages
            if (yandexLang == "ru")
                return "ru";
            
            // Default to English for all other languages
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
                return;
            }

            // Tell Yandex to switch language
            YG2.SwitchLanguage(languageCode);
        }

        /// <summary>
        /// Called when Yandex language changes externally (e.g., user changes in their account).
        /// </summary>
        private void OnYandexLanguageChanged(string newLang)
        {            
            // Map to supported language
            string mappedLang = (newLang == "ru") ? "ru" : "en";
            
            // Update LocalizationManager
            Core.LocalizationManager.SwitchLanguage(mappedLang);
        }
    }
}
#endif
