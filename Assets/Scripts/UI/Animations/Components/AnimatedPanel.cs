using UnityEngine;
using DG.Tweening;

namespace PixelVanguard.UI.Animations
{
    /// <summary>
    /// Component for panels that slide in/out with animations.
    /// Automatically ensures required components (RectTransform, CanvasGroup).
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class AnimatedPanel : MonoBehaviour
    {
        [Header("Animation Settings")]
        [SerializeField] private SlideDirection showDirection = SlideDirection.Right;
        [SerializeField] private SlideDirection hideDirection = SlideDirection.Right;
        [SerializeField] private float animationDuration = 0.3f;
        [SerializeField] private bool disableOnHide = true;

        [Header("Initial State")]
        [SerializeField] private bool startHidden = false;

        private RectTransform rectTransform;
        private CanvasGroup canvasGroup;
        private Tween currentTween;
        private Vector2 originalPosition;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            
            // Ensure CanvasGroup exists
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }

            // Save original position
            originalPosition = rectTransform.anchoredPosition;

            // Apply initial state
            if (startHidden)
            {
                gameObject.SetActive(false);
            }
        }

        private void OnDestroy()
        {
            currentTween?.Kill();
        }

        /// <summary>
        /// Show panel with slide animation.
        /// </summary>
        public void Show(System.Action onComplete = null)
        {
            currentTween?.Kill();

            // Reset position to original before showing
            rectTransform.anchoredPosition = originalPosition;

            currentTween = UIAnimator.ShowPanel(
                gameObject,
                showDirection,
                animationDuration,
                onComplete
            );
        }

        /// <summary>
        /// Hide panel with slide animation.
        /// </summary>
        public void Hide(System.Action onComplete = null)
        {
            currentTween?.Kill();

            currentTween = UIAnimator.HidePanel(
                gameObject,
                hideDirection,
                animationDuration,
                () =>
                {
                    if (disableOnHide)
                    {
                        gameObject.SetActive(false);
                    }
                    // Reset position after hiding
                    rectTransform.anchoredPosition = originalPosition;
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
            rectTransform.anchoredPosition = originalPosition;
            gameObject.SetActive(true);
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
            if (disableOnHide)
            {
                gameObject.SetActive(false);
            }
            rectTransform.anchoredPosition = originalPosition;
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
                canvasGroup.interactable = false;
            }
        }

        /// <summary>
        /// Check if panel is currently visible.
        /// </summary>
        public bool IsVisible()
        {
            return gameObject.activeSelf && (canvasGroup == null || canvasGroup.alpha > 0.5f);
        }
    }
}
