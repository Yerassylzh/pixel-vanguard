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
            
            // Load initial values
            LoadCurrentSettings();
            RefreshUI();
        }

        private void OnEnable()
        {
            // Reload settings when panel opens (in case they changed elsewhere)
            LoadCurrentSettings();
            RefreshUI();
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
            languageButtonText.text = (pendingLanguage == "en") ? "English" : "Русский";
            
            // Volume sliders
            musicSlider.SetValueWithoutNotify(pendingMusicVolume);
            soundsSlider.SetValueWithoutNotify(pendingSFXVolume);
            
            // Checkboxes
            UpdateCheckboxVisual(showDamageCheckImage, pendingShowDamage);
            UpdateCheckboxVisual(showFPSCheckImage, pendingShowFPS);
            
            // Gold
            RefreshGoldDisplay();
        }

        private async void RefreshGoldDisplay()
        {
            var saveService = Core.ServiceLocator.Get<Services.ISaveService>();
            if (saveService != null)
            {
                var saveData = await saveService.LoadData();
                if (goldText != null)
                {
                    goldText.text = saveData.totalGold.ToString();
                }
            }
        }

        // ============================================
        // SETTING CHANGE HANDLERS (Pending Only)
        // ============================================

        private void OnLanguageToggle()
        {
            pendingLanguage = (pendingLanguage == "en") ? "ru" : "en";
            languageButtonText.text = (pendingLanguage == "en") ? "English" : "Русский";
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
