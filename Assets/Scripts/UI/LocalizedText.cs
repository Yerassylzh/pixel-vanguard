using TMPro;
using UnityEngine;

namespace PixelVanguard.UI
{
    /// <summary>
    /// Auto-translating TextMeshProUGUI component.
    /// Subscribes to language change events and updates text automatically.
    /// </summary>
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class LocalizedText : MonoBehaviour
    {
        [Header("Translation")]
        [Tooltip("Translation key (e.g., 'ui.shop.title')")]
        [SerializeField] private string translationKey;

        [Header("Format Arguments (optional)")]
        [Tooltip("Arguments for string formatting ({0}, {1}, etc.)")]
        [SerializeField] private bool useFormatting = false;

        private TextMeshProUGUI textComponent;

        private void Awake()
        {
            textComponent = GetComponent<TextMeshProUGUI>();
            
            if (string.IsNullOrEmpty(translationKey))
            {
                Debug.LogWarning($"[LocalizedText] Translation key not set on {gameObject.name}");
                return;
            }

            // Subscribe to language change events
            Core.LocalizationManager.OnLanguageChanged += Refresh;
        }

        private void Start()
        {
            // Initial translation (done in Start to ensure LocalizationManager is initialized)
            Refresh();
        }

        private void OnDestroy()
        {
            // Unsubscribe to prevent memory leaks
            Core.LocalizationManager.OnLanguageChanged -= Refresh;
        }

        /// <summary>
        /// Refresh the text with current language translation.
        /// </summary>
        private void Refresh()
        {
            if (textComponent == null || string.IsNullOrEmpty(translationKey))
                return;

            // Check if LocalizationManager is initialized
            // This handles script execution order issues
            string translation = Core.LocalizationManager.Get(translationKey);
            
            // Only update if we got a valid translation (not the [key] fallback)
            if (!translation.StartsWith("["))
            {
                textComponent.text = translation;
            }
        }

        /// <summary>
        /// Set translation key and refresh (for dynamic UI).
        /// </summary>
        public void SetKey(string key)
        {
            translationKey = key;
            Refresh();
        }

        /// <summary>
        /// Set formatted translation with arguments.
        /// Example: SetFormatted("ui.gold.total", 1500)
        /// </summary>
        public void SetFormatted(string key, params object[] args)
        {
            if (textComponent == null)
                return;

            translationKey = key;
            textComponent.text = Core.LocalizationManager.GetFormatted(key, args);
        }

#if UNITY_EDITOR
        /// <summary>
        /// Preview translation in Editor (context menu).
        /// </summary>
        [ContextMenu("Preview Translation")]
        private void PreviewTranslation()
        {
            if (textComponent == null)
                textComponent = GetComponent<TextMeshProUGUI>();

            if (string.IsNullOrEmpty(translationKey))
            {
                Debug.LogWarning("Translation key not set!");
                return;
            }

            // Show English preview
            Debug.Log($"Key: {translationKey}\nEnglish: [Preview in Play Mode]");
        }
#endif
    }
}
