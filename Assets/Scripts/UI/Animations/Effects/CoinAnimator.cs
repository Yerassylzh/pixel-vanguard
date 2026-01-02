using UnityEngine;
using TMPro;
using DG.Tweening;
using System;

namespace PixelVanguard.UI.Animations
{
    /// <summary>
    /// Handles coin collection animations from world space to UI.
    /// Perfect for coin pickups and purchase feedback.
    /// </summary>
    public class CoinAnimator : MonoBehaviour
    {
        [Header("Coin Sprite")]
        [SerializeField] private Sprite coinSprite;

        [Header("Animation Settings")]
        [SerializeField] private float popUpHeight = 50f;
        [SerializeField] private float popUpDuration = 0.2f;
        [SerializeField] private float flyDuration = 0.5f;
        [SerializeField] private float targetPunchScale = 0.2f;

        private static CoinAnimator instance;
        private Transform poolParent;

        private void Awake()
        {
            instance = this;

            // Create pool parent
            poolParent = new GameObject("CoinAnimationPool").transform;
            poolParent.SetParent(transform);
        }

        /// <summary>
        /// Animate coin from world position to UI target.
        /// </summary>
        public static void AnimateCoinToTarget(Vector3 worldStartPos, Transform uiTarget, int goldAmount, Action onComplete = null)
        {
            if (instance == null)
            {
                Debug.LogWarning("[CoinAnimator] Instance not found!");
                onComplete?.Invoke();
                return;
            }

            instance.AnimateCoin(worldStartPos, uiTarget, goldAmount, onComplete);
        }

        /// <summary>
        /// Animate multiple coins with stagger.
        /// </summary>
        public static void AnimateMultipleCoins(Vector3 worldStartPos, Transform uiTarget, int coinCount, int goldPerCoin, float staggerDelay = 0.05f, Action onAllComplete = null)
        {
            if (instance == null)
            {
                Debug.LogWarning("[CoinAnimator] Instance not found!");
                onAllComplete?.Invoke();
                return;
            }

            int coinsCompleted = 0;

            for (int i = 0; i < coinCount; i++)
            {
                float delay = i * staggerDelay;

                DOVirtual.DelayedCall(delay, () =>
                {
                    instance.AnimateCoin(worldStartPos, uiTarget, goldPerCoin, () =>
                    {
                        coinsCompleted++;
                        if (coinsCompleted >= coinCount)
                        {
                            onAllComplete?.Invoke();
                        }
                    });
                });
            }
        }

        private void AnimateCoin(Vector3 worldStartPos, Transform uiTarget, int goldAmount, Action onComplete)
        {
            if (uiTarget == null)
            {
                Debug.LogWarning("[CoinAnimator] UI target is null!");
                onComplete?.Invoke();
                return;
            }

            // Create coin sprite
            GameObject coinObj = CreateCoinSprite(worldStartPos);

            if (coinObj == null)
            {
                onComplete?.Invoke();
                return;
            }

            RectTransform coinRect = coinObj.GetComponent<RectTransform>();
            Vector3 targetPos = uiTarget.position;

            Sequence seq = DOTween.Sequence();

            // 1. Pop up slightly
            Vector3 popUpPos = coinRect.position + Vector3.up * popUpHeight;
            seq.Append(coinRect.DOMove(popUpPos, popUpDuration).SetEase(Ease.OutQuad));

            // 2. Fly to target with curve
            seq.Append(coinRect.DOMove(targetPos, flyDuration).SetEase(Ease.InQuad));

            // 3. On complete
            seq.OnComplete(() =>
            {
                // Punch scale target
                if (uiTarget != null)
                {
                    UIAnimator.PunchScale(uiTarget, targetPunchScale, 0.3f);
                }

                // Note: Sound is handled by game events (GameEvents.TriggerGoldPickup)
                // when gold is actually collected, not by the animation itself

                // Destroy coin
                Destroy(coinObj);

                // Callback
                onComplete?.Invoke();
            });
        }

        private GameObject CreateCoinSprite(Vector3 worldPosition)
        {
            // Create UI element
            GameObject coinObj = new GameObject("AnimatedCoin");

            // Find canvas
            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas == null)
            {
                canvas = FindFirstObjectByType<Canvas>();
            }

            if (canvas == null)
            {
                Debug.LogError("[CoinAnimator] No canvas found!");
                Destroy(coinObj);
                return null;
            }

            coinObj.transform.SetParent(canvas.transform, false);

            // Add image component
            UnityEngine.UI.Image image = coinObj.AddComponent<UnityEngine.UI.Image>();
            
            if (coinSprite != null)
            {
                image.sprite = coinSprite;
            }
            else
            {
                // Fallback: create simple yellow circle
                image.color = new Color(1f, 0.84f, 0f); // Gold color
            }

            // Set size
            RectTransform rectTransform = coinObj.GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(32, 32);

            // Convert world position to screen position
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                Vector3 screenPos = mainCamera.WorldToScreenPoint(worldPosition);
                rectTransform.position = screenPos;
            }
            else
            {
                rectTransform.position = worldPosition;
            }

            return coinObj;
        }

        /// <summary>
        /// Animate coins from UI to item (for purchases).
        /// </summary>
        public static void AnimateCoinsFromUIToItem(Transform uiSource, Transform itemTarget, int coinCount, Action onAllComplete = null)
        {
            if (instance == null)
            {
                Debug.LogWarning("[CoinAnimator] Instance not found!");
                onAllComplete?.Invoke();
                return;
            }

            int coinsCompleted = 0;

            for (int i = 0; i < coinCount; i++)
            {
                float delay = i * 0.05f;

                DOVirtual.DelayedCall(delay, () =>
                {
                    instance.AnimateCoinUIToUI(uiSource, itemTarget, () =>
                    {
                        coinsCompleted++;
                        if (coinsCompleted >= coinCount)
                        {
                            onAllComplete?.Invoke();
                        }
                    });
                });
            }
        }

        private void AnimateCoinUIToUI(Transform uiSource, Transform uiTarget, Action onComplete)
        {
            if (uiSource == null || uiTarget == null)
            {
                onComplete?.Invoke();
                return;
            }

            // Create coin sprite at source position
            GameObject coinObj = new GameObject("AnimatedCoin");

            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas == null)
            {
                canvas = FindFirstObjectByType<Canvas>();
            }

            if (canvas == null)
            {
                Destroy(coinObj);
                onComplete?.Invoke();
                return;
            }

            coinObj.transform.SetParent(canvas.transform, false);

            UnityEngine.UI.Image image = coinObj.AddComponent<UnityEngine.UI.Image>();
            if (coinSprite != null)
            {
                image.sprite = coinSprite;
            }
            else
            {
                image.color = new Color(1f, 0.84f, 0f);
            }

            RectTransform rectTransform = coinObj.GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(32, 32);
            rectTransform.position = uiSource.position;

            // Animate to target
            rectTransform.DOMove(uiTarget.position, 0.5f)
                .SetEase(Ease.InQuad)
                .OnComplete(() =>
                {
                    Destroy(coinObj);
                    onComplete?.Invoke();
                });
        }
    }
}
