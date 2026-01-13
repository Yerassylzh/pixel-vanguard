using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

namespace PixelVanguard.UI
{
    /// <summary>
    /// Ad pack card for watching ads to earn gold.
    /// 
    /// STATE MACHINE PATTERN:
    /// - Uses state enum (Ready/Cooldown/Loading/Error) with lock mechanism
    /// - Lock prevents external updates from overriding temporary states
    /// - Critical: ShopUpdateCoroutine calls UpdateCooldown() every 1s
    /// - Critical: OnGoldChanged event triggers RefreshCards() → UpdateProgress()
    /// - Without lock: These would immediately overwrite loading/error text
    /// - With lock: All update methods check IsInTemporaryState() and return early
    /// 
    /// EXTERNAL UPDATE SOURCES (must respect lock):
    /// 1. UpdateCooldown() - Called every 1s by ShopUpdateCoroutine
    /// 2. UpdateProgress() - Called by RefreshCards() on gold change event
    /// </summary>
    public class AdPackCard : MonoBehaviour
    {
        /// <summary>
        /// Button states with priority hierarchy.
        /// </summary>
        public enum AdButtonState
        {
            Ready,      // Can watch ad, shows "(X/Y)"
            Cooldown,   // Timer active, shows "Xs"
            Loading,    // Ad loading, shows "Wait.." (LOCKED - cannot be overridden)
            Error       // Ad failed, shows "No ad" (LOCKED - cannot be overridden)
        }
        
        private AdButtonState currentState = AdButtonState.Ready;
        private bool isStateLocked = false; // Prevents external updates from overriding temporary states
        [Header("UI References")]
        [SerializeField] private Image iconImage;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI rewardText;
        [SerializeField] private Button watchButton;
        [SerializeField] private TextMeshProUGUI watchButtonText;
        [SerializeField] private Image buttonIcon; // Coin or TV icon on button
        [SerializeField] private Button cardButton; // Button for whole card (to show details)

        [Header("Colors")]
        [SerializeField] private Color readyColor = new Color(0.2f, 0.8f, 0.2f); // Green
        [SerializeField] private Color cooldownColor = new Color(0.5f, 0.5f, 0.5f); // Gray

        /// <summary>
        /// Helper property to check if state is locked.
        /// Temporary states (Loading/Error) lock to prevent external updates.
        /// </summary>
        private bool IsInTemporaryState => isStateLocked;

        private int requiredAds;
        private int rewardAmount;
        private string detailsDescription; // For details panel
        public event Action OnWatchClicked;
        public event Action OnCardClicked; // When card itself is clicked

        private void Start()
        {
            watchButton.onClick.AddListener(() => OnWatchClicked?.Invoke());
            
            if (cardButton != null)
            {
                cardButton.onClick.AddListener(() => OnCardClicked?.Invoke());
            }
        }

        /// <summary>
        /// Initialize card with static data.
        /// </summary>
        public void Initialize(int required, int reward, Sprite icon, Action onWatchCallback, string description)
        {
            requiredAds = required;
            rewardAmount = reward;
            OnWatchClicked = onWatchCallback;
            detailsDescription = description;

            titleText.text = $"WATCH {required} ADS";
            rewardText.text = $"{reward} Coins";

            if (icon != null && iconImage != null)
            {
                iconImage.sprite = icon;
            }
        }

        /// <summary>
        /// Update button to show ads remaining.
        /// Called by RefreshCards() when gold changes (via OnGoldChanged event).
        /// </summary>
        public void UpdateProgress(int adsWatched, int totalGold)
        {
            if (IsInTemporaryState) return; // Don't override loading/error states
            
            // Button shows progress: "AD (2/5)"
            int remaining = requiredAds - adsWatched;
            
            // Format: "AD (0/5)"
            watchButtonText.text = $"({adsWatched}/{requiredAds})";
        }

        /// <summary>
        /// Update state based on cooldown.
        /// Called every 1 second by ShopUpdateCoroutine.
        /// </summary>
        /// <param name="remainingSeconds">0 if ready, >0 if on cooldown</param>
        public void UpdateCooldown(int remainingSeconds)
        {
            if (IsInTemporaryState) return; // Don't override loading/error states
            
            if (remainingSeconds > 0)
            {
                currentState = AdButtonState.Cooldown;
                watchButtonText.text = $"{remainingSeconds}s";
                watchButton.interactable = false;
                
                if (watchButton.image != null)
                {
                    watchButton.image.color = cooldownColor;
                }
            }
            else
            {
                currentState = AdButtonState.Ready;
                watchButton.interactable = true;
                
                if (watchButton.image != null)
                {
                    watchButton.image.color = readyColor;
                }
            }
        }

        /// <summary>
        /// Get description for details panel.
        /// </summary>
        public string GetDescription()
        {
            return detailsDescription;
        }

        /// <summary>
        /// Get icon for details panel.
        /// </summary>
        public Sprite GetIcon()
        {
            return iconImage != null ? iconImage.sprite : null;
        }

        /// <summary>
        /// Get title for details panel.
        /// </summary>
        public string GetTitle()
        {
            return titleText != null ? titleText.text : "";
        }

        /// <summary>
        /// Show loading state and lock to prevent cooldown override.
        /// </summary>
        public void SetLoadingState()
        {
            currentState = AdButtonState.Loading;
            isStateLocked = true; // Lock state
            watchButton.interactable = false;
            watchButtonText.text = Core.LocalizationManager.Get("ui.shop.ad_loading");
        }

        /// <summary>
        /// Show error state ("No ad" or "Ошибка") for 2 seconds.
        /// </summary>
        public void SetErrorState()
        {
            currentState = AdButtonState.Error;
            isStateLocked = true; // Keep locked
            watchButtonText.text = Core.LocalizationManager.Get("ui.shop.ad_error");
            watchButton.interactable = false;
            StartCoroutine(UnlockAfterDelay(2f, null));
        }
        
        /// <summary>
        /// Show error state with callback - used by GoldPackHandler.
        /// </summary>
        public void SetErrorStateWithCallback(System.Action onComplete)
        {
            currentState = AdButtonState.Error;
            isStateLocked = true; // Keep locked
            watchButtonText.text = Core.LocalizationManager.Get("ui.shop.ad_error");
            watchButton.interactable = false;
            StartCoroutine(UnlockAfterDelay(2f, onComplete));
        }

        /// <summary>
        /// Unlock state after delay and restore normal state.
        /// </summary>
        private System.Collections.IEnumerator UnlockAfterDelay(float delay, System.Action onComplete)
        {
            yield return new WaitForSeconds(delay);
            isStateLocked = false; // Unlock - cooldown can update again
            currentState = AdButtonState.Ready;
            watchButton.interactable = true;
            onComplete?.Invoke(); // Callback refreshes card to restore progress text
        }

        private void OnDestroy()
        {
            watchButton.onClick.RemoveAllListeners();
            
            if (cardButton != null)
            {
                cardButton.onClick.RemoveAllListeners();
            }
        }
    }
}
