using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using PixelVanguard.Core;

namespace PixelVanguard.UI
{
    /// <summary>
    /// Controls the Settings menu with all toggles, sliders, and external links.
    /// Uses "Apply" button pattern - changes are pending until user clicks Apply.
    /// </summary>
    public class SettingsController : MonoBehaviour
    {
        private MainMenuManager mainMenuManager;
        private GameSettings gameSettings;

        [Header("UI References")]
        [SerializeField] private GameObject settingsPanel;

        [Header("Language")]
        [SerializeField] private GameObject languageRow;  // Parent row to hide on WebGL
        [SerializeField] private Button languageButton;
        [SerializeField] private TextMeshProUGUI languageButtonText;

        [Header("Volume Sliders")]
        [SerializeField] private Slider musicSlider;
        [SerializeField] private Slider soundsSlider;

        [Header("Checkboxes (Image-based)")]
        [SerializeField] private Button showDamageCheckbox;
        [SerializeField] private Image showDamageCheckImage;
        [SerializeField] private Button showFPSCheckbox;
        [SerializeField] private Image showFPSCheckImage;

        [Header("Checkbox Sprites")]
        [SerializeField] private Sprite checkedSprite;
        [SerializeField] private Sprite uncheckedSprite;

        [Header("External Links")]
        [SerializeField] private GameObject externalLinksRow;  // Parent row to hide on WebGL
        [SerializeField] private Button contactUsButton;
        [SerializeField] private Button privacyPolicyButton;

        [Header("URLs")]
        [SerializeField] private string telegramChatURL = "https://t.me/yourusername";
        [SerializeField] private string privacyPolicyURL = "https://yourwebsite.com/privacy";

        [Header("Navigation")]
        [SerializeField] private Button backButton;
        [SerializeField] private Button applyButton;

        [Header("Gold Display")]
        [SerializeField] private TextMeshProUGUI goldText;

        [Header("Remove Ads IAP")]
        [SerializeField] private GameObject removeAdsRow;
        [SerializeField] private Button removeAdsButton;
        [SerializeField] private TextMeshProUGUI removeAdsButtonText;
        
        private UI.LocalizedText removeAdsLocalizedText;  // Cached component

        // Pending changes (not yet saved)
        private string pendingLanguage;
        private float pendingMusicVolume;
        private float pendingSFXVolume;
        private bool pendingShowDamage;
        private bool pendingShowFPS;

        private void Start()
        {
            // Get GameSettings service
            gameSettings = Core.ServiceLocator.Get<GameSettings>();
            mainMenuManager = FindFirstObjectByType<MainMenuManager>();
            // Add listeners
            languageButton.onClick.AddListener(OnLanguageToggle);
            musicSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
            soundsSlider.onValueChanged.AddListener(OnSoundsVolumeChanged);
            showDamageCheckbox.onClick.AddListener(OnShowDamageToggle);
            showFPSCheckbox.onClick.AddListener(OnShowFPSToggle);
            contactUsButton.onClick.AddListener(OnContactUsClicked);
            privacyPolicyButton.onClick.AddListener(OnPrivacyPolicyClicked);
            backButton.onClick.AddListener(OnBackClicked);
            applyButton.onClick.AddListener(OnApplyClicked);

            if (removeAdsButton != null)
                removeAdsButton.onClick.AddListener(OnRemoveAdsClicked);

            // Get or add LocalizedText component to Remove Ads button
            if (removeAdsButtonText != null)
            {
                removeAdsLocalizedText = removeAdsButtonText.GetComponent<UI.LocalizedText>();
                if (removeAdsLocalizedText == null)
                {
                    removeAdsLocalizedText = removeAdsButtonText.gameObject.AddComponent<UI.LocalizedText>();
                }
            }

            // Load initial values
            LoadCurrentSettings();
            RefreshUI();
            RefreshRemoveAdsButton();

            // Hide platform-specific UI on WebGL
#if UNITY_WEBGL
            // Yandex controls language
            if (languageRow != null)
            {
                languageRow.SetActive(false);
            }
            
            // No IAP on WebGL
            if (removeAdsRow != null)
            {
                removeAdsRow.SetActive(false);
            }
            
            // No external links on WebGL
            if (externalLinksRow != null)
            {
                externalLinksRow.SetActive(false);
            }
#endif
        }

        private void OnEnable()
        {
            // Reload settings when panel opens (in case they changed elsewhere)
            LoadCurrentSettings();
            RefreshUI();
            
            // Only refresh if services are ready (after Start() has run)
            if (Core.ServiceLocator.Get<Services.ISaveService>() != null)
            {
                RefreshRemoveAdsButton();
            }
        }

        /// <summary>
        /// Load current saved settings into pending variables.
        /// </summary>
        private void LoadCurrentSettings()
        {
            if (gameSettings == null) return;
            
            pendingLanguage = gameSettings.Language;
            pendingMusicVolume = gameSettings.MusicVolume;
            pendingSFXVolume = gameSettings.SFXVolume;
            pendingShowDamage = gameSettings.ShowDamageNumbers;
            pendingShowFPS = gameSettings.ShowFPS;
        }

        /// <summary>
        /// Update UI to reflect pending settings (not yet saved).
        /// </summary>
        private void RefreshUI()
        {
            // Language
            languageButtonText.text = Core.LocalizationManager.GetLanguageName(pendingLanguage);

            // Volume sliders
            musicSlider.SetValueWithoutNotify(pendingMusicVolume);
            soundsSlider.SetValueWithoutNotify(pendingSFXVolume);

            // Checkboxes
            UpdateCheckboxVisual(showDamageCheckImage, pendingShowDamage);
            UpdateCheckboxVisual(showFPSCheckImage, pendingShowFPS);

            // Gold
            RefreshGoldDisplay();
        }

        private void RefreshGoldDisplay()
        {
            var cachedSave = Core.ServiceLocator.Get<Services.CachedSaveDataService>();
            if (cachedSave != null && goldText != null)
            {
                goldText.text = cachedSave.TotalGold.ToString();
            }
            else if (goldText == null)
            {
                Debug.LogError("[Settings] Gold Text component is MISSING!");
            }
        }

        /// <summary>
        /// Public method to refresh gold when panel is shown.
        /// Called by MainMenuManager when opening settings.
        /// </summary>
        public void RefreshGold()
        {
            RefreshGoldDisplay();
        }

        // ============================================
        // SETTING CHANGE HANDLERS (Pending Only)
        // ============================================

        private void OnLanguageToggle()
        {
            pendingLanguage = (pendingLanguage == "en") ? "ru" : "en";
            languageButtonText.text = Core.LocalizationManager.GetLanguageName(pendingLanguage);
        }

        private void OnMusicVolumeChanged(float value)
        {
            pendingMusicVolume = value;

            // Apply volume change immediately for preview (but don't save yet)
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.SetMusicVolume(value);
            }
        }

        private void OnSoundsVolumeChanged(float value)
        {
            pendingSFXVolume = value;

            // Apply volume change immediately for preview (but don't save yet)
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.SetSFXVolume(value);
            }
        }

        private void OnShowDamageToggle()
        {
            pendingShowDamage = !pendingShowDamage;
            UpdateCheckboxVisual(showDamageCheckImage, pendingShowDamage);
        }

        private void OnShowFPSToggle()
        {
            pendingShowFPS = !pendingShowFPS;
            UpdateCheckboxVisual(showFPSCheckImage, pendingShowFPS);
        }

        // ============================================
        // APPLY BUTTON - Save All Changes
        // ============================================

        private void OnApplyClicked()
        {
            if (gameSettings == null) return;
            
            // Save all pending changes to GameSettings service
            gameSettings.Language = pendingLanguage;
            gameSettings.MusicVolume = pendingMusicVolume;
            gameSettings.SFXVolume = pendingSFXVolume;
            gameSettings.ShowDamageNumbers = pendingShowDamage;
            gameSettings.ShowFPS = pendingShowFPS;

            // Apply FPS counter visibility if in GameScene
            var fpsCounter = FindFirstObjectByType<FPSCounter>();
            if (fpsCounter != null)
            {
                fpsCounter.SetVisible(pendingShowFPS);
            }
        }

        // ============================================
        // EXTERNAL LINKS
        // ============================================

        private void OnContactUsClicked()
        {
            Application.OpenURL(telegramChatURL);
        }

        private void OnPrivacyPolicyClicked()
        {
            Application.OpenURL(privacyPolicyURL);
        }

        // ============================================
        // REMOVE ADS IAP
        // ============================================

        private void OnRemoveAdsClicked()
        {
            var cachedSave = Core.ServiceLocator.Get<Services.CachedSaveDataService>();
            if (cachedSave == null) return;

            int cost = 4990;

            if (cachedSave.SpendGold(cost))
            {
                cachedSave.Data.adsRemoved = true;
                cachedSave.Save();

                // Update UI
                if (goldText != null) goldText.text = cachedSave.TotalGold.ToString();
                RefreshRemoveAdsButton();
                
                Debug.Log("[Settings] Remove Ads purchased for 4990 coins!");
            }
            else
            {
                Debug.LogWarning("[Settings] Not enough gold for Remove Ads!");
            }
        }

        private void RefreshRemoveAdsButton()
        {            
            if (removeAdsRow == null) return;

            var cachedSave = Core.ServiceLocator.Get<Services.CachedSaveDataService>();
            if (cachedSave == null) return;

            if (cachedSave.Data.adsRemoved)
            {
                // Disable button and change to "Ads Removed"
                removeAdsRow.SetActive(true);
                if (removeAdsButton != null)
                    removeAdsButton.interactable = false;
                
                if (removeAdsLocalizedText != null)
                    removeAdsLocalizedText.SetKey("ui.settings.ads_removed");
            }
            else
            {
                // Enable button and change to "Remove Ads - 4990 coins"
                removeAdsRow.SetActive(true);
                if (removeAdsButton != null)
                    removeAdsButton.interactable = true;
                
                if (removeAdsLocalizedText != null)
                    removeAdsLocalizedText.SetKey("ui.settings.remove_ads");
            }
        }

        // ============================================
        // NAVIGATION
        // ============================================

        private void OnBackClicked()
        {
            // Revert to saved settings (discard pending changes)
            LoadCurrentSettings();

            // Restore audio to saved values
            if (AudioManager.Instance != null && gameSettings != null)
            {
                AudioManager.Instance.SetMusicVolume(gameSettings.MusicVolume);
                AudioManager.Instance.SetSFXVolume(gameSettings.SFXVolume);
            }

            if (settingsPanel != null)
            {
                settingsPanel.SetActive(false);
                mainMenuManager.ReturnToMainMenu();
            }
        }

        // ============================================
        // HELPERS
        // ============================================

        private void UpdateCheckboxVisual(Image checkboxImage, bool isChecked)
        {
            if (checkboxImage != null)
            {
                checkboxImage.sprite = isChecked ? checkedSprite : uncheckedSprite;
            }
        }

        private void OnDestroy()
        {
            // Clean up listeners
            languageButton.onClick.RemoveAllListeners();
            musicSlider.onValueChanged.RemoveAllListeners();
            soundsSlider.onValueChanged.RemoveAllListeners();
            showDamageCheckbox.onClick.RemoveAllListeners();
            showFPSCheckbox.onClick.RemoveAllListeners();
            contactUsButton.onClick.RemoveAllListeners();
            privacyPolicyButton.onClick.RemoveAllListeners();
            backButton.onClick.RemoveAllListeners();
            applyButton.onClick.RemoveAllListeners();
        }
    }
}
