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
    /// Shows "Press to Start" text with fade animation.
    /// Waits for screen tap (not keyboard) before loading main menu.
    /// Uses New Input System via IPointerClickHandler.
    /// </summary>
    public class StartupSceneController : MonoBehaviour, IPointerClickHandler
    {
        [Header("References")]
        [SerializeField] private TextMeshProUGUI pressToStartText;
        [SerializeField] private CanvasGroup canvasGroup;

        [Header("Animation Settings")]
        [SerializeField] private float fadeDuration = 1f;
        [SerializeField] private float fadeInDelay = 0.5f;
        [SerializeField] private Ease fadeEase = Ease.InOutSine;

        [Header("Scene")]
        [SerializeField] private string mainMenuSceneName = "MainMenuScene";

        private bool hasStarted = false;
        private Tween pulseTween;

        private void Start()
        {
            // Set initial transparency
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
            }

            // Localize text
            if (pressToStartText != null)
            {
                pressToStartText.text = LocalizationManager.Get("ui.startup.press_to_start");
            }

            // Start fade-in animation with pulse loop
            StartFadeAnimation();
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

        private void StartFadeAnimation()
        {
            if (canvasGroup == null) return;

            // Fade in
            canvasGroup.DOFade(1f, fadeDuration)
                .SetEase(fadeEase)
                .SetDelay(fadeInDelay)
                .OnComplete(() =>
                {
                    // Start pulsing loop
                    StartPulseLoop();
                });
        }

        private void StartPulseLoop()
        {
            if (canvasGroup == null) return;

            // Pulse between 0.6 and 1.0 alpha
            pulseTween = canvasGroup.DOFade(0.3f, fadeDuration)
                .SetEase(Ease.InOutSine)
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
