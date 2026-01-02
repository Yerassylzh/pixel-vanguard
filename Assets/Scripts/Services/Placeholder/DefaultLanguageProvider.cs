using UnityEngine;

namespace PixelVanguard.Services
{
    /// <summary>
    /// Default language provider for Android/Editor builds.
    /// Uses PlayerPrefs for language persistence, defaults to English.
    /// </summary>
    public class DefaultLanguageProvider : ILanguageProvider
    {
        private const string LANGUAGE_PREF_KEY = "PixelVanguard_Language";

        public void Initialize()
        {
        }

        public string GetCurrentLanguage()
        {
            // Load from PlayerPrefs, default to English
            string savedLang = PlayerPrefs.GetString(LANGUAGE_PREF_KEY, "en");
            
            // Validate
            if (savedLang != "en" && savedLang != "ru")
            {
                Debug.LogWarning($"[DefaultLanguageProvider] Invalid saved language '{savedLang}', defaulting to English");
                savedLang = "en";
            }

            return savedLang;
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
                Debug.LogWarning($"[DefaultLanguageProvider] Unsupported language: {languageCode}");
                return;
            }

            // Save to PlayerPrefs
            PlayerPrefs.SetString(LANGUAGE_PREF_KEY, languageCode);
            PlayerPrefs.Save();
        }
    }
}
