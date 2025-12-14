using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using PixelVanguard.Core;
using PixelVanguard.Gameplay;

namespace PixelVanguard.UI
{
    /// <summary>
    /// Platform-aware pause menu.
    /// Desktop: ESC key only, no pause button
    /// Mobile: Pause button visible, ESC disabled
    /// </summary>
    public class PauseMenu : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject pausePanel;
        [SerializeField] private Button pauseButton; // Mobile only
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button mainMenuButton;

        [Header("Input (Optional)")]
        [SerializeField] private InputActionAsset inputActions;

        private bool isPaused = false;
        private InputAction pauseAction;
        private bool useInputActions = false;

        private void Start()
        {
            if (pausePanel != null)
            {
                pausePanel.SetActive(false);
            }

            // Setup button listeners
            if (resumeButton != null) resumeButton.onClick.AddListener(Resume);
            if (mainMenuButton != null) mainMenuButton.onClick.AddListener(ReturnToMainMenu);
            if (pauseButton != null) pauseButton.onClick.AddListener(Pause);

            SetupInputActions();
            ConfigurePlatformControls();
        }

        private void OnEnable()
        {
            GameEvents.OnPlatformChanged += OnPlatformChanged;
        }

        private void OnDisable()
        {
            GameEvents.OnPlatformChanged -= OnPlatformChanged;
        }

        private void OnPlatformChanged(PlatformType newPlatform)
        {
            ConfigurePlatformControls();
        }

        private void SetupInputActions()
        {
            if (inputActions == null)
            {
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
            // Desktop-only: ESC key fallback if InputActions not configured
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
            if (pauseAction != null)
            {
                pauseAction.performed -= OnPauseInput;
            }
        }

        private void OnPauseInput(InputAction.CallbackContext context)
        {
            // Desktop only
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
            if (PlatformDetector.Instance == null)
            {
                Debug.LogWarning("[PauseMenu] PlatformDetector not found! Defaulting to desktop.");
                if (pauseButton != null) pauseButton.gameObject.SetActive(false);
                return;
            }

            // Mobile: Show pause button
            if (PlatformDetector.Instance.IsMobile())
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
            if (GameManager.Instance == null) return;
            if (GameManager.Instance.CurrentState != GameState.Playing) return;

            isPaused = true;
            GameManager.Instance.PauseGame();

            if (pausePanel != null)
            {
                pausePanel.SetActive(true);
            }
        }

        public void Resume()
        {
            if (GameManager.Instance == null) return;

            isPaused = false;
            GameManager.Instance.ResumeGame();

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
