using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using PixelVanguard.Core;
using PixelVanguard.Gameplay;

namespace PixelVanguard.UI
{
    /// <summary>
    /// Pause menu with platform-aware controls.
    /// Desktop: ESC key to pause (new Input System), no UI button
    /// Mobile: UI pause button, no keyboard
    /// </summary>
    public class PauseMenu : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject pausePanel;
        [SerializeField] private Button pauseButton; // Mobile only
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button mainMenuButton;

        [Header("Input (Optional)")]
        [SerializeField] private InputActionAsset inputActions; // Optional: assign same as PlayerController

        private bool isPaused = false;
        private InputAction pauseAction;
        private bool useInputActions = false;

        private void Start()
        {
            // Hide pause panel initially
            if (pausePanel != null)
            {
                pausePanel.SetActive(false);
            }

            // Setup button listeners
            if (resumeButton != null) resumeButton.onClick.AddListener(Resume);
            if (mainMenuButton != null) mainMenuButton.onClick.AddListener(ReturnToMainMenu);
            if (pauseButton != null) pauseButton.onClick.AddListener(Pause);

            // Try to setup Input Actions (optional)
            SetupInputActions();

            // Show/hide pause button based on platform
            ConfigurePlatformControls();
        }

        private void SetupInputActions()
        {
            // Try assigned asset first
            if (inputActions == null)
            {
                // Try to load from Resources as fallback
                inputActions = Resources.Load<InputActionAsset>("InputSystem_Actions");
            }

            if (inputActions != null)
            {
                var uiMap = inputActions.FindActionMap("UI");
                if (uiMap != null)
                {
                    pauseAction = uiMap.FindAction("Pause");
                    if (pauseAction != null)
                    {
                        pauseAction.Enable();
                        pauseAction.performed += OnPauseInput;
                        useInputActions = true;
                        Debug.Log("[PauseMenu] Using Input Actions for pause");
                        return;
                    }
                }
            }

            // Fallback: use Keyboard.current
            Debug.Log("[PauseMenu] InputActions not configured, using Keyboard.current fallback");
            useInputActions = false;
        }

        private void Update()
        {
            // Fallback: Direct keyboard check if InputActions not configured
            if (!useInputActions && PlatformDetector.Instance != null && PlatformDetector.Instance.IsDesktop())
            {
                if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
                {
                    if (isPaused)
                    {
                        Resume();
                    }
                    else
                    {
                        Pause();
                    }
                }
            }
        }

        private void OnDestroy()
        {
            // Unsubscribe from input
            if (pauseAction != null)
            {
                pauseAction.performed -= OnPauseInput;
            }
        }

        private void OnPauseInput(InputAction.CallbackContext context)
        {
            // Only on desktop
            if (PlatformDetector.Instance != null && PlatformDetector.Instance.IsDesktop())
            {
                if (isPaused)
                {
                    Resume();
                }
                else
                {
                    Pause();
                }
            }
        }

        private void ConfigurePlatformControls()
        {
            if (Core.PlatformDetector.Instance == null)
            {
                Debug.LogWarning("[PauseMenu] PlatformDetector not found! Defaulting to desktop.");
                if (pauseButton != null) pauseButton.gameObject.SetActive(false);
                return;
            }

            // Mobile: Show pause button
            if (Core.PlatformDetector.Instance.IsMobile())
            {
                if (pauseButton != null)
                {
                    pauseButton.gameObject.SetActive(true);
                }
            }
            // Desktop: Hide pause button (use ESC key)
            else
            {
                if (pauseButton != null)
                {
                    pauseButton.gameObject.SetActive(false);
                }
            }
        }

        public void Pause()
        {
            if (Gameplay.GameManager.Instance == null) return;
            if (Gameplay.GameManager.Instance.CurrentState != GameState.Playing) return;

            isPaused = true;
            Gameplay.GameManager.Instance.PauseGame();

            if (pausePanel != null)
            {
                pausePanel.SetActive(true);
            }
        }

        public void Resume()
        {
            if (Gameplay.GameManager.Instance == null) return;

            isPaused = false;
            Gameplay.GameManager.Instance.ResumeGame();

            if (pausePanel != null)
            {
                pausePanel.SetActive(false);
            }
        }

        private void ReturnToMainMenu()
        {
            // TODO: Load main menu scene
            Debug.Log("[PauseMenu] Return to Main Menu - not implemented yet");
            
            // For now, just unpause
            Resume();
        }
    }
}
