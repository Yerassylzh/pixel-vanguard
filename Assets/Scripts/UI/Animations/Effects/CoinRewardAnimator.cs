using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System;

namespace PixelVanguard.UI.Animations
{
    /// <summary>
    /// Configurable coin reward animator.
    /// Spawns multiple coins that fly to a target with stagger, pulses target, and counts up text.
    /// Use this in Shop, Results, or any scene that needs coin reward feedback.
    /// </summary>
    public class CoinRewardAnimator : MonoBehaviour
    {
        [Header("Coin Settings")]
        [SerializeField] private Sprite coinSprite;
        [SerializeField] private float coinSizePixels = 32f; // Size in pixels at reference resolution
        [SerializeField] private int coinsPerReward = 5; // How many coins to spawn
        [SerializeField] private float spawnInterval = 0.08f; // Delay between each coin spawn
        
        [Header("Animation Settings")]
        [SerializeField] private float popUpHeight = 30f;
        [SerializeField] private float popUpDuration = 0.2f;
        [SerializeField] private float flyDuration = 0.5f;
        [SerializeField] private float targetPulseStrength = 0.2f;
        
        [Header("Audio")]
        [SerializeField] private bool playSoundOnArrival = false; // Sound handled by events
        
        private Transform poolParent;

        private void Awake()
        {
            // Create pool parent for organization
            poolParent = new GameObject("CoinAnimationPool").transform;
            poolParent.SetParent(transform);
        }

        /// <summary>
        /// Play coin reward animation.
        /// </summary>
        public void PlayCoinReward(
            Vector3 sourcePos, 
            Transform targetTransform, 
            int totalGold,
            TextMeshProUGUI goldText = null,
            Action onComplete = null)
        {
            if (targetTransform == null)
            {
                Debug.LogWarning("[CoinRewardAnimator] Target transform is null!");
                onComplete?.Invoke();
                return;
            }

            int coinsToSpawn = CalculateCoinCount(totalGold);
            int goldPerCoin = Mathf.CeilToInt((float)totalGold / coinsToSpawn);
            int coinsCompleted = 0;
            int currentGoldValue = goldText != null ? int.Parse(goldText.text) : 0;
            Vector3 targetOriginalScale = targetTransform.localScale;

            // Track active pulse tween to prevent stacking
            Tween activePulseTween = null;

            for (int i = 0; i < coinsToSpawn; i++)
            {
                float delay = i * spawnInterval;
                
                DOVirtual.DelayedCall(delay, () =>
                {
                    AnimateSingleCoin(
                        sourcePos,
                        targetTransform,
                        goldPerCoin,
                        () =>
                        {
                            coinsCompleted++;

                            // Update text
                            if (goldText != null)
                            {
                                int newValue = currentGoldValue + (coinsCompleted * goldPerCoin);
                                newValue = Mathf.Min(newValue, currentGoldValue + totalGold);
                                goldText.text = newValue.ToString();
                            }

                            // Fast, non-stacking pulse
                            activePulseTween?.Kill(); // Kill previous pulse
                            float pulseScale = 1f + targetPulseStrength;
                            activePulseTween = targetTransform.DOScale(targetOriginalScale * pulseScale, 0.1f)
                                .SetEase(Ease.OutQuad)
                                .SetUpdate(true)
                                .OnComplete(() =>
                                {
                                    // Return to normal
                                    targetTransform.DOScale(targetOriginalScale, 0.15f)
                                        .SetEase(Ease.InOutQuad)
                                        .SetUpdate(true);
                                });

                            // Final coin - smooth return to original
                            if (coinsCompleted >= coinsToSpawn)
                            {
                                DOVirtual.DelayedCall(0.3f, () =>
                                {
                                    if (targetTransform != null)
                                    {
                                        targetTransform.DOScale(targetOriginalScale, 0.2f)
                                            .SetEase(Ease.OutQuad)
                                            .SetUpdate(true);
                                    }
                                    onComplete?.Invoke();
                                }).SetUpdate(true);
                            }
                        }
                    );
                }).SetUpdate(true);
            }
        }

        /// <summary>
        /// Calculate dynamic coin count based on reward amount.
        /// Uses logarithmic-style scaling for pleasing visual feedback.
        /// 1990 gold → ~7 coins, 4990 gold → ~13 coins, 29900 gold → ~24 coins
        /// </summary>
        private int CalculateCoinCount(int goldAmount)
        {
            // Use a logarithmic-ish formula for smooth scaling
            // Base: 7 coins at ~2000 gold, 13 coins at ~5000 gold, 24 coins at ~30000 gold
            float logValue = Mathf.Log10(goldAmount + 1); // +1 to avoid log(0)
            int coinCount = Mathf.RoundToInt(logValue * 14.5f - 40.5f);
            
            // Clamp to reasonable range (5-30 coins)
            coinCount = Mathf.Clamp(coinCount, 5, 30);
            
            return coinCount;
        }

        private void AnimateSingleCoin(Vector3 sourcePos, Transform target, int goldValue, Action onComplete)
        {
            // Create coin sprite
            GameObject coinObj = CreateCoinSprite(sourcePos);
            if (coinObj == null)
            {
                onComplete?.Invoke();
                return;
            }

            RectTransform coinRect = coinObj.GetComponent<RectTransform>();
            Vector3 targetPos = target.position;

            Sequence seq = DOTween.Sequence();

            // 1. Pop up slightly
            Vector3 popUpPos = coinRect.position + Vector3.up * popUpHeight;
            seq.Append(coinRect.DOMove(popUpPos, popUpDuration).SetEase(Ease.OutQuad));

            // 2. Fly to target with curve
            seq.Append(coinRect.DOMove(targetPos, flyDuration).SetEase(Ease.InQuad));

            // 3. Scale out before destroying
            seq.Append(coinRect.DOScale(Vector3.zero, 0.15f).SetEase(Ease.InBack));

            // 4. On complete
            seq.OnComplete(() =>
            {
                Destroy(coinObj);
                onComplete?.Invoke();
            });
        }

        private GameObject CreateCoinSprite(Vector3 uiPosition)
        {
            // Create UI element
            GameObject coinObj = new GameObject("AnimatedCoin");

            // Find or create overlay canvas (renders on top of everything)
            Canvas overlayCanvas = GetOverlayCanvas();
            if (overlayCanvas == null)
            {
                Debug.LogError("[CoinRewardAnimator] Could not create overlay canvas!");
                Destroy(coinObj);
                return null;
            }

            coinObj.transform.SetParent(overlayCanvas.transform, false);

            // Add image component
            Image image = coinObj.AddComponent<Image>();
            
            if (coinSprite != null)
            {
                image.sprite = coinSprite;
            }
            else
            {
                // Fallback: create simple yellow circle
                image.color = new Color(1f, 0.84f, 0f); // Gold color
            }

            // Set size - use fixed pixel size for consistency across resolutions
            RectTransform rectTransform = coinObj.GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(coinSizePixels, coinSizePixels);
            rectTransform.position = uiPosition;

            return coinObj;
        }

        /// <summary>
        /// Get or create an overlay canvas that renders on top of everything.
        /// </summary>
        private Canvas GetOverlayCanvas()
        {
            // Check if overlay already exists
            Canvas[] allCanvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
            foreach (Canvas c in allCanvases)
            {
                if (c.gameObject.name == "CoinOverlayCanvas")
                {
                    return c;
                }
            }

            // Create new overlay canvas
            GameObject overlayObj = new GameObject("CoinOverlayCanvas");
            Canvas overlayCanvas = overlayObj.AddComponent<Canvas>();
            overlayCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            overlayCanvas.sortingOrder = 1000; // Very high to render on top

            // Add graphic raycaster (optional, coins don't need interaction)
            overlayObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();

            // Don't destroy on load if you want it persistent
            // DontDestroyOnLoad(overlayObj);

            return overlayCanvas;
        }

        private void OnDestroy()
        {
            DOTween.Kill(this);
        }
    }
}
