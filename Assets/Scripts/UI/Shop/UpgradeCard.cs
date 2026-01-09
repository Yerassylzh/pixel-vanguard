using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

namespace PixelVanguard.UI
{
    /// <summary>
    /// Individual upgrade card (Might, Vitality, Greaves, Magnet).
    /// Simple design: Icon, Level text, Effect text, Buy button with cost.
    /// NO PROGRESS BAR - Just "Level: 3 / 10" text.
    /// </summary>
    public class UpgradeCard : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Image iconImage;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI levelText; // "Level: 3 / 10"
        [SerializeField] private TextMeshProUGUI effectText; // "Damage +30%"
        [SerializeField] private Button buyButton;
        [SerializeField] private TextMeshProUGUI buyButtonText; // "2256"
        [SerializeField] private Image coinIcon; // Small coin icon on button
        [SerializeField] private Button cardButton; // Button for whole card (to show details)

        [Header("Colors")]
        [SerializeField] private Color affordableColor = new Color(0.2f, 0.8f, 0.2f); // Green
        [SerializeField] private Color unaffordableColor = new Color(0.5f, 0.5f, 0.5f); // Gray
        [SerializeField] private Color maxLevelColor = new Color(0.7f, 0.7f, 0.7f); // Light gray

        // Card data
        private string statKey;
        private string displayName;
        private int baseCost;
        private string detailsDescription; // For details panel

        public int BaseCost => baseCost;
        public string StatName => statKey; // Expose for handlers
        public event Action OnPurchaseClicked;
        public event Action OnCardClicked; // When card itself is clicked (not buy button)

        private void Start()
        {
            buyButton.onClick.AddListener(() => OnPurchaseClicked?.Invoke());
            
            if (cardButton != null)
            {
                cardButton.onClick.AddListener(() => OnCardClicked?.Invoke());
            }
        }

        /// <summary>
        /// Initialize card with static data.
        /// </summary>
        public void Initialize(string key, string name, Sprite icon, int cost, string description)
        {
            statKey = key;
            displayName = name;
            baseCost = cost;
            detailsDescription = description;

            nameText.text = displayName;
            
            if (icon != null && iconImage != null)
            {
                iconImage.sprite = icon;
            }
        }

        /// <summary>
        /// Update card to reflect current level and affordability.
        /// </summary>
        public void UpdateCard(int currentLevel, int nextCost, bool canAfford)
        {
            // Update level text: "Level: 3 / 10"
            string levelLabel = Core.LocalizationManager.Get("ui.hud.level_full");
            levelText.text = $"{levelLabel}: {currentLevel} / 10";

            // Update effect text
            UpdateEffectText(currentLevel);

            // Update buy button
            if (currentLevel >= 10)
            {
                // Max level
                buyButtonText.text = "MAX";
                buyButton.interactable = false;
                
                if (buyButton.image != null)
                {
                    buyButton.image.color = maxLevelColor;
                }
            }
            else
            {
                // Show cost
                buyButtonText.text = nextCost.ToString();
                
                if (canAfford)
                {
                    // Affordable - green button
                    buyButton.interactable = true;
                    if (buyButton.image != null)
                    {
                        buyButton.image.color = affordableColor;
                    }
                }
                else
                {
                    // Unaffordable - gray button
                    buyButton.interactable = false;
                    if (buyButton.image != null)
                    {
                        buyButton.image.color = unaffordableColor;
                    }
                }
            }
        }

        private void UpdateEffectText(int level)
        {
            // Show NEXT level's effect (what you'll get when buying)
            int nextLevel = level + 1;
            int effectValue = 0;
            
            switch (statKey)
            {
                case "might":
                    effectValue = nextLevel * 10; // +10% damage per level
                    effectText.text = Core.LocalizationManager.Get("ui.shop.might.short");
                    break;
                case "vitality":
                    effectValue = nextLevel * 10; // +10 HP per level
                    effectText.text = Core.LocalizationManager.Get("ui.shop.vitality.short");
                    break;
                case "greaves":
                    effectValue = nextLevel * 5; // +5% speed per level
                    effectText.text = Core.LocalizationManager.Get("ui.shop.greaves.short");
                    break;
                case "magnet":
                    effectValue = nextLevel * 10; // +10% range per level
                    effectText.text = Core.LocalizationManager.Get("ui.shop.magnet.short");
                    break;
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
        /// Get display name for details panel.
        /// </summary>
        public string GetDisplayName()
        {
            return displayName;
        }

        private void OnDestroy()
        {
            buyButton.onClick.RemoveAllListeners();
            
            if (cardButton != null)
            {
                cardButton.onClick.RemoveAllListeners();
            }
        }
    }
}
