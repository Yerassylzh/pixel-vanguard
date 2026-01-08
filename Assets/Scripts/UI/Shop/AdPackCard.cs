using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

namespace PixelVanguard.UI
{
    /// <summary>
    /// Ad pack card for watching ads to earn gold.
    /// Simple design: Icon, reward text, button showing ads remaining OR cooldown timer.
    /// NO PROGRESS BAR - Button shows everything.
    /// </summary>
    public class AdPackCard : MonoBehaviour
    {
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
        /// </summary>
        public void UpdateProgress(int adsWatched, int totalGold)
        {
            // Button shows progress: "AD (2/5)"
            int remaining = requiredAds - adsWatched;
            
            // Format: "AD (0/5)"
            watchButtonText.text = $"({adsWatched}/{requiredAds})";
        }

        /// <summary>
        /// Update state based on cooldown.
        /// </summary>
        /// <param name="remainingSeconds">0 if ready, >0 if on cooldown</param>
        public void UpdateCooldown(int remainingSeconds)
        {
            if (remainingSeconds > 0)
            {
                // COOLDOWN - Show timer
                watchButtonText.text = $"{remainingSeconds}s";
                watchButton.interactable = false;
                
                if (watchButton.image != null)
                {
                    watchButton.image.color = cooldownColor;
                }
            }
            else
            {
                // READY - Show gold (button will be updated via UpdateProgress)
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
