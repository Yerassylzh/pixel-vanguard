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
            // Load from PlayerPrefs. If not set, try to detect system language.
            string defaultLang = "en";
            bool hasKey = PlayerPrefs.HasKey(LANGUAGE_PREF_KEY);
            
            Debug.Log($"[DefaultLanguageProvider] PlayerPrefs.HasKey('{LANGUAGE_PREF_KEY}'): {hasKey}");
            
            if (!hasKey)
            {
                if (Application.systemLanguage == SystemLanguage.Russian)
                {
                    defaultLang = "ru";
                    Debug.Log($"[DefaultLanguageProvider] Detected system language: Russian, using 'ru'");
                }
                else
                {
                    Debug.Log($"[DefaultLanguageProvider] Detected system language: {Application.systemLanguage}, using 'en'");
                }
            }
        
            string savedLang = PlayerPrefs.GetString(LANGUAGE_PREF_KEY, defaultLang);
            Debug.Log($"[DefaultLanguageProvider] PlayerPrefs.GetString result: '{savedLang}' (default: '{defaultLang}')");
            
            // Validate
            if (savedLang != "en" && savedLang != "ru")
            {
                Debug.LogWarning($"[DefaultLanguageProvider] ⚠️ Invalid saved language '{savedLang}', defaulting to English");
                savedLang = "en";
            }
            
            Debug.Log($"[DefaultLanguageProvider] ✅ Final language: '{savedLang}'");

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
