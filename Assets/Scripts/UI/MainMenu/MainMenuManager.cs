using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

namespace PixelVanguard.UI
{
    /// <summary>
    /// Main Menu Manager - Handles navigation between pages and main menu buttons.
    /// </summary>
    public class MainMenuManager : MonoBehaviour
    {
        [Header("Panels")]
        [SerializeField] private GameObject mainMenuPanel;
        [SerializeField] private GameObject settingsPanel;
        [SerializeField] private GameObject shopPanel;        // For future implementation
        [SerializeField] private GameObject characterPanel;   // For future implementation

        [Header("Main Menu Buttons")]
        [SerializeField] private Button playButton;
        [SerializeField] private Button shopButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button quitButton;

        [Header("Gold Display")]
        [SerializeField] private TextMeshProUGUI goldText;

        private void Start()
        {
            // Show main menu by default
            ShowPanel(mainMenuPanel);

            // Setup button listeners
            playButton.onClick.AddListener(OnPlayClicked);
            shopButton.onClick.AddListener(OnShopClicked);
            settingsButton.onClick.AddListener(OnSettingsClicked);
            quitButton.onClick.AddListener(OnQuitClicked);

            // Refresh gold display
            RefreshGoldDisplay();

            // Apply saved audio settings
            ApplySavedAudioSettings();
        }

        private void OnEnable()
        {
            // Refresh gold whenever panel is enabled (returning from shop/character select)
            RefreshGoldDisplay();
        }

        /// <summary>
        /// Load and apply saved audio settings to AudioManager.
        /// </summary>
        private void ApplySavedAudioSettings()
        {
            if (Core.AudioManager.Instance != null)
            {
                float sfxVolume = Core.GameSettings.SFXVolume;
                float musicVolume = Core.GameSettings.MusicVolume;

                Core.AudioManager.Instance.SetSFXVolume(sfxVolume);
                Core.AudioManager.Instance.SetMusicVolume(musicVolume);

                Debug.Log($"[MainMenu] Applied audio settings - SFX: {sfxVolume:F2}, Music: {musicVolume:F2}");
            }
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

        /// <summary>
        /// Show a specific panel and hide all others.
        /// </summary>
        private void ShowPanel(GameObject panelToShow)
        {
            if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
            if (settingsPanel != null) settingsPanel.SetActive(false);
            if (shopPanel != null) shopPanel.SetActive(false);
            if (characterPanel != null) characterPanel.SetActive(false);

            if (panelToShow != null)
            {
                panelToShow.SetActive(true);
            }
        }

        /// <summary>
        /// Called from Settings "Back" button to return to main menu.
        /// </summary>
        public void ReturnToMainMenu()
        {
            ShowPanel(mainMenuPanel);
        }

        // ============================================
        // BUTTON HANDLERS
        // ============================================

        private void OnPlayClicked()
        {
            Debug.Log("[MainMenu] Play clicked - Showing Character Selection");
            ShowPanel(characterPanel);
        }

        private void OnShopClicked()
        {
            Debug.Log("[MainMenu] Shop clicked");
            ShowPanel(shopPanel);
        }

        private void OnSettingsClicked()
        {
            Debug.Log("[MainMenu] Settings clicked");
            ShowPanel(settingsPanel);
        }

        private void OnQuitClicked()
        {
            Debug.Log("[MainMenu] Quit clicked");
            
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
            #else
                Application.Quit();
            #endif
        }

        private void OnDestroy()
        {
            // Clean up listeners
            if (playButton != null) playButton.onClick.RemoveAllListeners();
            if (shopButton != null) shopButton.onClick.RemoveAllListeners();
            if (settingsButton != null) settingsButton.onClick.RemoveAllListeners();
            if (quitButton != null) quitButton.onClick.RemoveAllListeners();
        }
    }
}
