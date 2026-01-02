using System.Collections.Generic;
using UnityEngine;

namespace PixelVanguard.Data
{
    /// <summary>
    /// ScriptableObject containing all translation data for the game.
    /// Create via: Assets → Create → Localization → Translation Data
    /// </summary>
    [CreateAssetMenu(fileName = "Translations", menuName = "Localization/Translation Data")]
    public class TranslationData : ScriptableObject
    {
        [System.Serializable]
        public class LocalizedString
        {
            [Tooltip("Unique key for this translation (e.g., 'ui.shop.title')")]
            public string key;

            [Tooltip("English text")]
            [TextArea(1, 3)]
            public string english;

            [Tooltip("Russian text")]
            [TextArea(1, 3)]
            public string russian;
        }

        [SerializeField]
        private List<LocalizedString> strings = new List<LocalizedString>();

        /// <summary>
        /// Get translation for a specific key and language.
        /// Returns null if not found.
        /// </summary>
        public string GetTranslation(string key, string languageCode)
        {
            var entry = strings.Find(s => s.key == key);
            if (entry == null)
                return null;

            return languageCode switch
            {
                "en" => entry.english,
                "ru" => entry.russian,
                _ => entry.english // Fallback to English
            };
        }

        /// <summary>
        /// Get all translation keys (for debugging/validation).
        /// </summary>
        public List<string> GetAllKeys()
        {
            var keys = new List<string>();
            foreach (var str in strings)
            {
                keys.Add(str.key);
            }
            return keys;
        }

#if UNITY_EDITOR
        /// <summary>
        /// Add a new translation entry (Editor only).
        /// </summary>
        public void AddEntry(string key, string englishText, string russianText)
        {
            if (strings.Exists(s => s.key == key))
            {
                Debug.LogWarning($"[TranslationData] Key '{key}' already exists!");
                return;
            }

            strings.Add(new LocalizedString
            {
                key = key,
                english = englishText,
                russian = russianText
            });
        }
#endif
    }
}
