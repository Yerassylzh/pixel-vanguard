using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using PixelVanguard.UI.CharacterSelect;
using PixelVanguard.UI.Animations;

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

        [Header("Navigation")]
        [SerializeField] private MenuNavigationController navigationController;

        private bool navigationInitialized = false;

        private void Start()
        {
            // Initialize navigation controller
            InitializeNavigation();

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

        private void InitializeNavigation()
        {
            // Find or create navigation controller
            if (navigationController == null)
            {
                navigationController = GetComponent<MenuNavigationController>();
                if (navigationController == null)
                {
                    navigationController = gameObject.AddComponent<MenuNavigationController>();
                }
            }

            // Initialize with main menu as root
            if (mainMenuPanel != null)
            {
                // Hide all panels first
                if (settingsPanel != null) settingsPanel.SetActive(false);
                if (shopPanel != null) shopPanel.SetActive(false);
                if (characterPanel != null) characterPanel.SetActive(false);

                navigationController.Initialize(mainMenuPanel);
                navigationInitialized = true;
            }
            else
            {
                Debug.LogError("[MainMenuManager] Main menu panel is not assigned!");
            }
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
                var gameSettings = Core.ServiceLocator.Get<Core.GameSettings>();
                if (gameSettings != null)
                {
                    Core.AudioManager.Instance.SetSFXVolume(gameSettings.SFXVolume);
                    Core.AudioManager.Instance.SetMusicVolume(gameSettings.MusicVolume);
                }
            }
        }

        private void RefreshGoldDisplay()
        {
            var saveService = Core.ServiceLocator.Get<Services.ISaveService>();
            if (saveService != null)
            {
                var saveData = saveService.LoadData();
                if (goldText != null)
                {
                    goldText.text = saveData.totalGold.ToString();
                }
            }
        }

        /// <summary>
        /// Navigate to a specific panel using the navigation controller.
        /// </summary>
        private void NavigateToPanel(GameObject panelToShow)
        {
            if (!navigationInitialized || navigationController == null)
            {
                Debug.LogWarning("[MainMenuManager] Navigation not initialized, falling back to direct show.");
                ShowPanelDirect(panelToShow);
                return;
            }

            if (panelToShow == null) return;

            navigationController.NavigateToPanel(panelToShow);
        }

        /// <summary>
        /// Fallback: Show panel directly without animation (for compatibility).
        /// </summary>
        private void ShowPanelDirect(GameObject panelToShow)
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
        /// Called from Settings/Shop/Character "Back" button to return to main menu.
        /// </summary>
        public void ReturnToMainMenu()
        {
            if (navigationController != null && navigationInitialized)
            {
                navigationController.NavigateBack(() =>
                {
                    // Refresh gold after navigation
                    RefreshGoldDisplay();
                });
            }
            else
            {
                // Fallback
                ShowPanelDirect(mainMenuPanel);
                RefreshGoldDisplay();
            }
        }

        // ============================================
        // BUTTON HANDLERS
        // ============================================

        private void OnPlayClicked()
        {
            NavigateToPanel(characterPanel);

            // Refresh gold in character select panel
            // Controller might be on Canvas or parent, so find it globally
            var characterController = FindFirstObjectByType<CharacterSelectController>();
            if (characterController != null)
            {
                characterController.RefreshGoldAndUI();
            }
            else
            {
                Debug.LogError("[MainMenu] CRITICAL: CharacterSelectController NOT FOUND in scene!");
            }
        }

        private void OnShopClicked()
        {
            NavigateToPanel(shopPanel);
        }

        private void OnSettingsClicked()
        {
            NavigateToPanel(settingsPanel);

            // Refresh gold in settings panel
            // Controller might be on Canvas or parent, so find it globally
            var settingsController = FindFirstObjectByType<SettingsController>();
            if (settingsController != null)
            {
                settingsController.RefreshGold();
            }
            else
            {
                Debug.LogError("[MainMenu] CRITICAL: SettingsController NOT FOUND in scene!");
            }
        }

        private void OnQuitClicked()
        {
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
