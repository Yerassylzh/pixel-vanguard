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
                return;
            }

            // Subscribe to language change events
            Core.LocalizationManager.OnLanguageChanged += Refresh;
        }

        private void OnEnable()
        {
            // Refresh when object becomes active (in case language changed while inactive)
            Refresh();
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

            // Get translation (returns "[key]" if not found)
            string translation = Core.LocalizationManager.Get(translationKey);

            // Always update text - this ensures we see the translation or the debug key
            textComponent.text = translation;
        }

        private string formatArgs;
        
        /// <summary>
        /// Set formatted translation with arguments.
        /// Example: SetFormatted("ui.gold.total", 1500)
        /// </summary>
        public void SetFormatted(string key, params object[] args)
        {
            if (textComponent == null) return;

            translationKey = key;
            // Store args for refresh if needed (basic implementation)
            // Implementation note: Storing object[] args is tricky if they change. 
            // Better to only support static or dynamic-push updates.
            
            textComponent.text = Core.LocalizationManager.GetFormatted(key, args);
        }

        /// <summary>
        /// Set translation key and refresh (for dynamic UI).
        /// </summary>
        public void SetKey(string key)
        {
            translationKey = key;
            Refresh();
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
        }
#endif
    }
}
