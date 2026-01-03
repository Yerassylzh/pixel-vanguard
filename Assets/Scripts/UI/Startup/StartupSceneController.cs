using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using TMPro;
using DG.Tweening;
using PixelVanguard.Core;

namespace PixelVanguard.UI.Startup
{
    /// <summary>
    /// Startup scene controller for Yandex Games audio unlock.
    /// Shows "Press to Start" text with scale-in animation and pulse effect.
    /// Waits for screen tap (not keyboard) before loading main menu.
    /// Uses New Input System for global input detection.
    /// </summary>
    public class StartupSceneController : MonoBehaviour, IPointerClickHandler
    {
        [Header("References")]
        [SerializeField] private TextMeshProUGUI pressToStartText;

        [Header("Animation Settings")]
        [SerializeField] [Range(1.1f, 2f)] private float pulseScale = 1.12f; // Pulse: 1x ↔ this value
        [SerializeField] private float pulseDuration = 1f;
        [SerializeField] private float pulseDelay = 0.5f;

        [Header("Scene")]
        [SerializeField] private string mainMenuSceneName = "MainMenuScene";

        private bool hasStarted = false;
        private Tween pulseTween;

        private void Start()
        {
            // Localize text
            if (pressToStartText != null)
            {
                pressToStartText.text = LocalizationManager.Get("ui.startup.press_to_start");
            }

            // Start pulsing immediately after delay
            StartPulse();
        }

        private void Update()
        {
            // Detect mouse click or screen tap anywhere using New Input System
            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                OnScreenTapped();
            }
            else if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
            {
                OnScreenTapped();
            }
        }

        /// <summary>
        /// IPointerClickHandler - Detects mouse clicks and screen taps.
        /// </summary>
        public void OnPointerClick(PointerEventData eventData)
        {
            OnScreenTapped();
        }

        private void StartPulse()
        {
            if (pressToStartText == null) return;

            // Set initial scale to normal
            pressToStartText.transform.localScale = Vector3.one;

            // Start pulsing: 1x ↔ pulseScale
            pulseTween = pressToStartText.transform.DOScale(pulseScale, pulseDuration)
                .SetEase(Ease.InOutSine)
                .SetDelay(pulseDelay)
                .SetLoops(-1, LoopType.Yoyo);
        }

        private void OnScreenTapped()
        {
            if (hasStarted) return;
            hasStarted = true;

            // Kill pulse animation
            pulseTween?.Kill();

            // Load main menu immediately (don't wait for fade out)
            LoadMainMenu();
        }

        private void LoadMainMenu()
        {
            SceneManager.LoadScene(mainMenuSceneName);
        }

        private void OnDestroy()
        {
            // Cleanup tweens
            pulseTween?.Kill();
            DOTween.Kill(this);
        }
    }
}
