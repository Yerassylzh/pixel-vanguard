using UnityEngine;
using DG.Tweening;

namespace PixelVanguard.UI.Animations
{
    /// <summary>
    /// Component for popup windows with scale/bounce animations.
    /// Perfect for level-up panels, confirmation dialogs, etc.
    /// </summary>
    public class AnimatedPopup : MonoBehaviour
    {
        [Header("Animation Settings")]
        [SerializeField] private float showDuration = 0.5f;
        [SerializeField] private float hideDuration = 0.3f;
        [SerializeField] private bool useUnscaledTime = false; // For popups during pause
        [SerializeField] private bool fadeBackground = true;

        [Header("Initial State")]
        [SerializeField] private bool startHidden = true;

        private CanvasGroup canvasGroup;
        private Tween currentTween;
        private Vector3 originalScale;

        private void Awake()
        {
            originalScale = transform.localScale;

            // Ensure CanvasGroup exists
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null && fadeBackground)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }

            // Apply initial state
            if (startHidden)
            {
                gameObject.SetActive(false);
                transform.localScale = Vector3.zero;
            }
        }

        private void OnDestroy()
        {
            currentTween?.Kill();
        }

        /// <summary>
        /// Show popup with bounce animation.
        /// </summary>
        public void Show(System.Action onComplete = null)
        {
            currentTween?.Kill();

            currentTween = UIAnimator.ShowPopup(
                transform,
                showDuration,
                useUnscaledTime,
                onComplete
            );
        }

        /// <summary>
        /// Hide popup with reverse animation.
        /// </summary>
        public void Hide(System.Action onComplete = null)
        {
            currentTween?.Kill();

            currentTween = UIAnimator.HidePopup(
                transform,
                hideDuration,
                useUnscaledTime,
                () =>
                {
                    gameObject.SetActive(false);
                    onComplete?.Invoke();
                }
            );
        }

        /// <summary>
        /// Show immediately without animation.
        /// </summary>
        public void ShowImmediate()
        {
            currentTween?.Kill();
            gameObject.SetActive(true);
            transform.localScale = originalScale;
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1f;
                canvasGroup.interactable = true;
                canvasGroup.blocksRaycasts = true;
            }
        }

        /// <summary>
        /// Hide immediately without animation.
        /// </summary>
        public void HideImmediate()
        {
            currentTween?.Kill();
            gameObject.SetActive(false);
            transform.localScale = Vector3.zero;
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
                canvasGroup.interactable = false;
            }
        }

        /// <summary>
        /// Check if popup is currently visible.
        /// </summary>
        public bool IsVisible()
        {
            return gameObject.activeSelf;
        }
    }
}
