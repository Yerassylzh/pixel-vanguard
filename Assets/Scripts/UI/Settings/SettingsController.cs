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
        [SerializeField] private GameObject removeAdsRow;  // Parent row to hide after purchase
        [SerializeField] private Button removeAdsButton;
        [SerializeField] private TextMeshProUGUI removeAdsButtonText;

        // Pending changes (not yet saved)
        private string pendingLanguage;
        private float pendingMusicVolume;
        private float pendingSFXVolume;
        private bool pendingShowDamage;
        private bool pendingShowFPS;

        private void Start()
        {
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

            // Load initial values
            LoadCurrentSettings();
            RefreshUI();
            RefreshRemoveAdsButton();

            // Hide language button on WebGL (Yandex controls language)
#if UNITY_WEBGL
            if (languageRow != null)
            {
                languageRow.SetActive(false);
                Debug.Log("[Settings] Language row hidden - Yandex platform controls language");
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
            pendingLanguage = GameSettings.Language;
            pendingMusicVolume = GameSettings.MusicVolume;
            pendingSFXVolume = GameSettings.SFXVolume;
            pendingShowDamage = GameSettings.ShowDamageNumbers;
            pendingShowFPS = GameSettings.ShowFPS;
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
            var saveService = Core.ServiceLocator.Get<Services.ISaveService>();
            if (saveService != null)
            {
                var saveData = saveService.LoadData();
                if (goldText != null)
                {
                    Debug.Log($"[Settings] Refreshing Gold Display: {saveData.totalGold}");
                    goldText.text = saveData.totalGold.ToString();
                }
                else
                {
                    Debug.LogError("[Settings] Gold Text component is MISSING!");
                }
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
            Debug.Log("[Settings] Applying changes...");

            // Save all pending changes to GameSettings (PlayerPrefs)
            GameSettings.Language = pendingLanguage;
            GameSettings.MusicVolume = pendingMusicVolume;
            GameSettings.SFXVolume = pendingSFXVolume;
            GameSettings.ShowDamageNumbers = pendingShowDamage;
            GameSettings.ShowFPS = pendingShowFPS;

            // Apply FPS counter visibility if in GameScene
            var fpsCounter = FindFirstObjectByType<FPSCounter>();
            if (fpsCounter != null)
            {
                fpsCounter.SetVisible(pendingShowFPS);
            }

            Debug.Log("[Settings] ✅ All settings saved!");

            // Optional: Show confirmation message
            // ShowConfirmationMessage("Settings Saved!");
        }

        // ============================================
        // EXTERNAL LINKS
        // ============================================

        private void OnContactUsClicked()
        {
            Application.OpenURL(telegramChatURL);
            Debug.Log($"[Settings] Opening Telegram: {telegramChatURL}");
        }

        private void OnPrivacyPolicyClicked()
        {
            Application.OpenURL(privacyPolicyURL);
            Debug.Log($"[Settings] Opening Privacy Policy: {privacyPolicyURL}");
        }

        // ============================================
        // REMOVE ADS IAP
        // ============================================

        private async void OnRemoveAdsClicked()
        {
            Debug.Log("[Settings] ========== REMOVE ADS CLICKED ==========");

            var iapService = Core.ServiceLocator.Get<Services.IIAPService>();
            if (iapService == null)
            {
                Debug.LogError("[Settings] IAPService not found!");
                return;
            }

            Debug.Log("[Settings] Calling PurchaseProduct...");
            // Purchase remove ads
            bool success = await iapService.PurchaseProduct(Services.ProductIDs.REMOVE_ADS);

            Debug.Log($"[Settings] Purchase result: {success}");

            if (success)
            {
                Debug.Log("[Settings] Remove Ads purchased successfully!");

                // Update save data
                var saveService = Core.ServiceLocator.Get<Services.ISaveService>();
                if (saveService != null)
                {
                    var saveData = saveService.LoadData();
                    Debug.Log($"[Settings] BEFORE: adsRemoved = {saveData.adsRemoved}");
                    
                    saveData.adsRemoved = true;
                    
                    Debug.Log($"[Settings] AFTER: adsRemoved = {saveData.adsRemoved}");
                    Debug.Log("[Settings] Calling SaveData...");
                    
                    saveService.SaveData(saveData);
                    
                    Debug.Log("[Settings] SaveData complete");
                    
                    // Pass the updated saveData directly (don't reload from disk yet)
                    Debug.Log("[Settings] Calling RefreshRemoveAdsButton with updated data...");
                    RefreshRemoveAdsButton(saveData);
                }
                else
                {
                    Debug.LogError("[Settings] SaveService is NULL!");
                }
            }
            else
            {
                Debug.LogWarning("[Settings] Remove Ads purchase failed or was cancelled");
            }
            
            Debug.Log("[Settings] ========== REMOVE ADS HANDLER END ==========");
        }

        private void RefreshRemoveAdsButton(Services.SaveData cachedData = null)
        {
            Debug.Log("[Settings] RefreshRemoveAdsButton called");
            
            if (removeAdsRow == null)
            {
                Debug.LogError("[Settings] CRITICAL: removeAdsRow is NULL!");
                return;
            }

            Debug.Log($"[Settings] removeAdsRow exists: {removeAdsRow.name}");

            // Use cached data if provided (e.g., right after purchase), otherwise load
            Services.SaveData saveData = cachedData;
            if (saveData == null)
            {
                var saveService = Core.ServiceLocator.Get<Services.ISaveService>();
                if (saveService != null)
                {
                    saveData = saveService.LoadData();
                }
                else
                {
                    Debug.LogError("[Settings] SaveService is NULL in RefreshRemoveAdsButton!");
                    return;
                }
            }
            
            Debug.Log($"[Settings] SaveData - adsRemoved: {saveData.adsRemoved}");

            if (saveData.adsRemoved)
            {
                // Already purchased - hide entire row
                Debug.Log("[Settings] *** HIDING ROW *** SetActive(false)");
                removeAdsRow.SetActive(false);
                Debug.Log($"[Settings] Row active state after hide: {removeAdsRow.activeSelf}");
            }
            else
            {
                // Not purchased - show row with price
                Debug.Log("[Settings] Not purchased - showing row");
                removeAdsRow.SetActive(true);
                
                if (removeAdsButton != null)
                    removeAdsButton.interactable = true;

                // Get price from IAP service
                    if (removeAdsButtonText != null)
                    {
                        var iapService = Core.ServiceLocator.Get<Services.IIAPService>();
                        if (iapService != null)
                        {
                            string price = iapService.GetLocalizedPrice(Services.ProductIDs.REMOVE_ADS);
                            removeAdsButtonText.text = Core.LocalizationManager.GetFormatted("ui.settings.remove_ads", price);
                            Debug.Log($"[Settings] Set price: {price}");
                        }
                        else
                        {
                            removeAdsButtonText.text = Core.LocalizationManager.GetFormatted("ui.settings.remove_ads", "29 YAN");
                        }
                    }
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
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.SetMusicVolume(GameSettings.MusicVolume);
                AudioManager.Instance.SetSFXVolume(GameSettings.SFXVolume);
            }

            if (settingsPanel != null)
            {
                settingsPanel.SetActive(false);
                mainMenuManager.ReturnToMainMenu();
            }

            Debug.Log("[Settings] Back (changes discarded)");
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
