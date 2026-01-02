using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;

namespace PixelVanguard.UI.Animations
{
    /// <summary>
    /// Auto-animating button component.
    /// Adds hover, press, and release animations to any button.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class AnimatedButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        [Header("Animation Settings")]
        [SerializeField] private float hoverScale = 1.1f;
        [SerializeField] private float pressScale = 0.95f;
        [SerializeField] private float animationDuration = 0.2f;
        [SerializeField] private bool enableHoverAnimation = true;
        [SerializeField] private bool enablePressAnimation = true;

        [Header("Audio (Optional)")]
        [SerializeField] private bool playHoverSound = false;
        [SerializeField] private bool playClickSound = false;

        private Button button;
        private Vector3 originalScale;
        private bool isHovering = false;
        private Tween currentTween;

        private void Awake()
        {
            button = GetComponent<Button>();
            originalScale = transform.localScale;
        }

        private void OnDestroy()
        {
            // Kill any active tweens
            currentTween?.Kill();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!enableHoverAnimation || !button.interactable) return;

            isHovering = true;
            currentTween?.Kill();
            currentTween = transform.DOScale(originalScale * hoverScale, animationDuration)
                .SetEase(Ease.OutQuad)
                .SetUpdate(true); // Works during pause

            if (playHoverSound && Core.AudioManager.Instance != null)
            {
                // Use button click for hover sound (no separate hover sound in AudioManager)
                Core.AudioManager.Instance.PlayButtonClick();
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (!enableHoverAnimation || !button.interactable) return;

            isHovering = false;
            currentTween?.Kill();
            currentTween = transform.DOScale(originalScale, animationDuration)
                .SetEase(Ease.OutQuad)
                .SetUpdate(true); // Works during pause
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!enablePressAnimation || !button.interactable) return;

            currentTween?.Kill();
            currentTween = transform.DOScale(originalScale * pressScale, animationDuration * 0.5f)
                .SetEase(Ease.OutQuad)
                .SetUpdate(true); // Works during pause

            if (playClickSound && Core.AudioManager.Instance != null)
            {
                Core.AudioManager.Instance.PlayButtonClick();
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (!enablePressAnimation || !button.interactable) return;

            currentTween?.Kill();
            
            // Return to hover state if still hovering, otherwise return to original
            float targetScale = isHovering ? hoverScale : 1f;
            currentTween = transform.DOScale(originalScale * targetScale, animationDuration * 0.5f)
                .SetEase(Ease.OutQuad)
                .SetUpdate(true); // Works during pause
        }

        /// <summary>
        /// Manually trigger a pulse animation.
        /// </summary>
        public void Pulse(float intensity = 0.2f, int loops = 1)
        {
            UIAnimator.Pulse(transform, intensity, loops);
        }

        /// <summary>
        /// Reset to original scale.
        /// </summary>
        public void ResetScale()
        {
            currentTween?.Kill();
            transform.localScale = originalScale;
        }
    }
}
