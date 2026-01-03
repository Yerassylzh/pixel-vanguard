using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System;

namespace PixelVanguard.UI.Animations
{
    /// <summary>
    /// Central animation utility for all UI animations.
    /// Provides static methods for consistent, reusable animations.
    /// </summary>
    public static class UIAnimator
    {
        // ============================================
        // PANEL ANIMATIONS
        // ============================================

        /// <summary>
        /// Animate panel appearing with specified direction.
        /// </summary>
        public static Tween ShowPanel(GameObject panel, SlideDirection direction, float duration = 0.3f, Action onComplete = null)
        {
            if (panel == null) return null;

            RectTransform rect = panel.GetComponent<RectTransform>();
            CanvasGroup canvasGroup = panel.GetComponent<CanvasGroup>();

            if (rect == null)
            {
                Debug.LogWarning($"[UIAnimator] {panel.name} missing RectTransform!");
                panel.SetActive(true);
                onComplete?.Invoke();
                return null;
            }

            // Ensure panel is active
            panel.SetActive(true);

            // Setup canvas group
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1f;
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = true;
            }

            // Calculate start position
            Vector2 startPos = GetOffscreenPosition(rect, direction);
            Vector2 endPos = Vector2.zero; // Always animate to center (0,0)

            rect.anchoredPosition = startPos;

            // Animate to center
            Sequence seq = DOTween.Sequence();
            seq.Append(rect.DOAnchorPos(endPos, duration).SetEase(Ease.OutCubic));

            seq.OnComplete(() =>
            {
                if (canvasGroup != null)
                {
                    canvasGroup.interactable = true;
                }
                onComplete?.Invoke();
            });

            return seq;
        }

        /// <summary>
        /// Animate panel hiding with specified direction.
        /// </summary>
        public static Tween HidePanel(GameObject panel, SlideDirection direction, float duration = 0.3f, Action onComplete = null)
        {
            if (panel == null) return null;

            RectTransform rect = panel.GetComponent<RectTransform>();
            CanvasGroup canvasGroup = panel.GetComponent<CanvasGroup>();

            if (rect == null)
            {
                Debug.LogWarning($"[UIAnimator] {panel.name} missing RectTransform!");
                panel.SetActive(false);
                onComplete?.Invoke();
                return null;
            }

            // Disable interaction immediately
            if (canvasGroup != null)
            {
                canvasGroup.interactable = false;
            }

            Vector2 endPos = GetOffscreenPosition(rect, direction);

            Sequence seq = DOTween.Sequence();
            
            // Fade out immediately to prevent overlap (mobile fix)
            if (canvasGroup != null)
            {
                seq.Join(canvasGroup.DOFade(0f, duration * 0.3f));
            }
            
            // Slide out
            seq.Append(rect.DOAnchorPos(endPos, duration).SetEase(Ease.InCubic));

            seq.OnComplete(() =>
            {
                panel.SetActive(false);
                // Reset alpha for next show
                if (canvasGroup != null)
                {
                    canvasGroup.alpha = 1f;
                }
                onComplete?.Invoke();
            });

            return seq;
        }

        /// <summary>
        /// Get offscreen position based on slide direction.
        /// </summary>
        private static Vector2 GetOffscreenPosition(RectTransform rect, SlideDirection direction)
        {
            Canvas canvas = rect.GetComponentInParent<Canvas>();
            float screenWidth = canvas != null ? canvas.GetComponent<RectTransform>().rect.width : Screen.width;
            float screenHeight = canvas != null ? canvas.GetComponent<RectTransform>().rect.height : Screen.height;

            Vector2 currentPos = rect.anchoredPosition;

            switch (direction)
            {
                case SlideDirection.Right:
                    return new Vector2(screenWidth, currentPos.y);
                case SlideDirection.Left:
                    return new Vector2(-screenWidth, currentPos.y);
                case SlideDirection.Up:
                    return new Vector2(currentPos.x, screenHeight);
                case SlideDirection.Down:
                    return new Vector2(currentPos.x, -screenHeight);
                default:
                    return currentPos;
            }
        }

        // ============================================
        // POPUP ANIMATIONS
        // ============================================

        /// <summary>
        /// Show popup with bounce scale effect.
        /// </summary>
        public static Tween ShowPopup(Transform popup, float duration = 0.5f, bool useUnscaledTime = false, Action onComplete = null)
        {
            if (popup == null) return null;

            popup.gameObject.SetActive(true);
            popup.localScale = Vector3.zero;

            CanvasGroup canvasGroup = popup.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = true;
            }

            Sequence seq = DOTween.Sequence();

            // Fade in canvas group
            if (canvasGroup != null)
            {
                seq.Join(canvasGroup.DOFade(1f, duration * 0.3f));
            }

            // Scale with bounce
            seq.Append(popup.DOScale(Vector3.one, duration)
                .SetEase(Ease.OutBack));

            if (useUnscaledTime)
            {
                seq.SetUpdate(true);
            }

            seq.OnComplete(() =>
            {
                if (canvasGroup != null)
                {
                    canvasGroup.interactable = true;
                }
                onComplete?.Invoke();
            });

            return seq;
        }

        /// <summary>
        /// Hide popup with reverse scale effect.
        /// </summary>
        public static Tween HidePopup(Transform popup, float duration = 0.3f, bool useUnscaledTime = false, Action onComplete = null)
        {
            if (popup == null) return null;

            CanvasGroup canvasGroup = popup.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                canvasGroup.interactable = false;
            }

            Sequence seq = DOTween.Sequence();

            // Fade out
            if (canvasGroup != null)
            {
                seq.Join(canvasGroup.DOFade(0f, duration));
            }

            // Scale down
            seq.Join(popup.DOScale(Vector3.zero, duration)
                .SetEase(Ease.InBack));

            if (useUnscaledTime)
            {
                seq.SetUpdate(true);
            }

            seq.OnComplete(() =>
            {
                popup.gameObject.SetActive(false);
                onComplete?.Invoke();
            });

            return seq;
        }

        // ============================================
        // VALUE ANIMATIONS
        // ============================================

        /// <summary>
        /// Animate number value with count-up effect.
        /// </summary>
        public static Tween AnimateValue(TextMeshProUGUI text, int fromValue, int toValue, float duration = 0.5f, Action onComplete = null)
        {
            if (text == null) return null;

            return DOVirtual.Int(fromValue, toValue, duration, (value) =>
            {
                text.text = value.ToString();
            }).OnComplete(() => onComplete?.Invoke());
        }

        /// <summary>
        /// Animate float value with count-up effect and formatting.
        /// </summary>
        public static Tween AnimateValue(TextMeshProUGUI text, float fromValue, float toValue, float duration = 0.5f, string format = "F1", Action onComplete = null)
        {
            if (text == null) return null;

            return DOVirtual.Float(fromValue, toValue, duration, (value) =>
            {
                text.text = value.ToString(format);
            }).OnComplete(() => onComplete?.Invoke());
        }

        // ============================================
        // BUTTON ANIMATIONS
        // ============================================

        /// <summary>
        /// Animate button press (scale down).
        /// </summary>
        public static Tween AnimateButtonPress(Transform button, float scale = 0.95f, float duration = 0.1f)
        {
            if (button == null) return null;
            return button.DOScale(scale, duration).SetEase(Ease.OutQuad);
        }

        /// <summary>
        /// Animate button release (scale back to normal or hover).
        /// </summary>
        public static Tween AnimateButtonRelease(Transform button, float scale = 1f, float duration = 0.1f)
        {
            if (button == null) return null;
            return button.DOScale(scale, duration).SetEase(Ease.OutQuad);
        }

        /// <summary>
        /// Animate button hover (scale up).
        /// </summary>
        public static Tween AnimateButtonHover(Transform button, float scale = 1.1f, float duration = 0.2f)
        {
            if (button == null) return null;
            return button.DOScale(scale, duration).SetEase(Ease.OutQuad);
        }

        // ============================================
        // EFFECTS
        // ============================================

        /// <summary>
        /// Pulse effect - scale up and down repeatedly.
        /// </summary>
        public static Tween Pulse(Transform target, float intensity = 0.1f, int loops = 1, float duration = 0.5f)
        {
            if (target == null) return null;

            Vector3 originalScale = target.localScale;
            Vector3 pulseScale = originalScale * (1f + intensity);

            return target.DOScale(pulseScale, duration * 0.5f)
                .SetEase(Ease.OutQuad)
                .SetLoops(loops * 2, LoopType.Yoyo)
                .OnComplete(() => target.localScale = originalScale);
        }

        /// <summary>
        /// Shake effect - shake position.
        /// </summary>
        public static Tween Shake(Transform target, float strength = 10f, float duration = 0.3f, int vibrato = 10)
        {
            if (target == null) return null;

            if (target is RectTransform rectTransform)
            {
                return rectTransform.DOShakeAnchorPos(duration, strength, vibrato);
            }
            else
            {
                return target.DOShakePosition(duration, strength, vibrato);
            }
        }

        /// <summary>
        /// Punch scale effect - quick scale bump.
        /// </summary>
        public static Tween PunchScale(Transform target, float strength = 0.2f, float duration = 0.3f)
        {
            if (target == null) return null;
            return target.DOPunchScale(Vector3.one * strength, duration, 5, 0.5f);
        }

        // ============================================
        // FADE ANIMATIONS
        // ============================================

        /// <summary>
        /// Fade in canvas group.
        /// </summary>
        public static Tween FadeIn(CanvasGroup canvasGroup, float duration = 0.3f, Action onComplete = null)
        {
            if (canvasGroup == null) return null;

            canvasGroup.gameObject.SetActive(true);
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = true;

            return canvasGroup.DOFade(1f, duration)
                .OnComplete(() =>
                {
                    canvasGroup.interactable = true;
                    onComplete?.Invoke();
                });
        }

        /// <summary>
        /// Fade out canvas group.
        /// </summary>
        public static Tween FadeOut(CanvasGroup canvasGroup, float duration = 0.3f, bool disableOnComplete = true, Action onComplete = null)
        {
            if (canvasGroup == null) return null;

            canvasGroup.interactable = false;

            return canvasGroup.DOFade(0f, duration)
                .OnComplete(() =>
                {
                    if (disableOnComplete)
                    {
                        canvasGroup.gameObject.SetActive(false);
                    }
                    onComplete?.Invoke();
                });
        }

        // ============================================
        // UTILITY
        // ============================================

        /// <summary>
        /// Kill all tweens on a target.
        /// </summary>
        public static void KillTweens(Transform target)
        {
            if (target != null)
            {
                target.DOKill();
            }
        }

        /// <summary>
        /// Stagger animate multiple elements with delay between each.
        /// </summary>
        public static Sequence StaggerAnimate(Transform[] elements, Func<Transform, Tween> animationFunc, float staggerDelay = 0.1f)
        {
            Sequence seq = DOTween.Sequence();

            for (int i = 0; i < elements.Length; i++)
            {
                if (elements[i] == null) continue;

                Tween tween = animationFunc(elements[i]);
                if (tween != null)
                {
                    seq.Insert(i * staggerDelay, tween);
                }
            }

            return seq;
        }
    }
}
