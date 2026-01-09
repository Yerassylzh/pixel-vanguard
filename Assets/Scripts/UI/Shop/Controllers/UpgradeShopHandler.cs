using UnityEngine;
using PixelVanguard.Core;
using PixelVanguard.Services;

namespace PixelVanguard.UI.Shop
{
    /// <summary>
    /// Handles upgrade shop logic for stat upgrades (Might, Vitality, Greaves, Magnet).
    /// Extracted from ShopController for better separation of concerns.
    /// </summary>
    public class UpgradeShopHandler
    {
        private readonly CachedSaveDataService cachedSave;
        private readonly DetailsPanel detailsPanel;
        
        // Upgrade cards
        private readonly UpgradeCard mightCard;
        private readonly UpgradeCard vitalityCard;
        private readonly UpgradeCard greavesCard;
        private readonly UpgradeCard magnetCard;
        
        // Icons
        private readonly Sprite mightIcon;
        private readonly Sprite vitalityIcon;
        private readonly Sprite greavesIcon;
        private readonly Sprite magnetIcon;

        public UpgradeShopHandler(
            CachedSaveDataService cachedSave,
            DetailsPanel detailsPanel,
            UpgradeCard mightCard,
            UpgradeCard vitalityCard,
            UpgradeCard greavesCard,
            UpgradeCard magnetCard,
            Sprite mightIcon,
            Sprite vitalityIcon,
            Sprite greavesIcon,
            Sprite magnetIcon)
        {
            this.cachedSave = cachedSave;
            this.detailsPanel = detailsPanel;
            this.mightCard = mightCard;
            this.vitalityCard = vitalityCard;
            this.greavesCard = greavesCard;
            this.magnetCard = magnetCard;
            this.mightIcon = mightIcon;
            this.vitalityIcon = vitalityIcon;
            this.greavesIcon = greavesIcon;
            this.magnetIcon = magnetIcon;
        }

        public void Initialize()
        {
            mightCard.Initialize("might", 
                LocalizationManager.Get("ui.shop.might.name"), 
                mightIcon, 100, 
                LocalizationManager.Get("ui.shop.might.desc"));
                
            vitalityCard.Initialize("vitality", 
                LocalizationManager.Get("ui.shop.vitality.name"), 
                vitalityIcon, 80, 
                LocalizationManager.Get("ui.shop.vitality.desc"));
            
            greavesCard.Initialize("greaves", 
                LocalizationManager.Get("ui.shop.greaves.name"), 
                greavesIcon, 120, 
                LocalizationManager.Get("ui.shop.greaves.desc"));
            
            magnetCard.Initialize("magnet", 
                LocalizationManager.Get("ui.shop.magnet.name"), 
                magnetIcon, 60, 
                LocalizationManager.Get("ui.shop.magnet.desc"));
        }

        public void SubscribeEvents(System.Action<string> onPurchaseCallback)
        {
            mightCard.OnPurchaseClicked += () => onPurchaseCallback("might");
            vitalityCard.OnPurchaseClicked += () => onPurchaseCallback("vitality");
            greavesCard.OnPurchaseClicked += () => onPurchaseCallback("greaves");
            magnetCard.OnPurchaseClicked += () => onPurchaseCallback("magnet");

            mightCard.OnCardClicked += () => ShowCardDetails(mightCard);
            vitalityCard.OnCardClicked += () => ShowCardDetails(vitalityCard);
            greavesCard.OnCardClicked += () => ShowCardDetails(greavesCard);
            magnetCard.OnCardClicked += () => ShowCardDetails(magnetCard);
        }

        public void RefreshAllCards()
        {
            RefreshCard(mightCard, "might");
            RefreshCard(vitalityCard, "vitality");
            RefreshCard(greavesCard, "greaves");
            RefreshCard(magnetCard, "magnet");
        }

        private void RefreshCard(UpgradeCard card, string statKey)
        {
            int currentLevel = cachedSave.Data.GetStatLevel(statKey);
            int cost = CalculateCost(card.BaseCost, currentLevel);
            bool canAfford = cachedSave.Data.totalGold >= cost;
            
            card.UpdateCard(currentLevel, cost, canAfford);
        }

        private int CalculateCost(int baseCost, int currentLevel)
        {
            return Mathf.RoundToInt(baseCost * Mathf.Pow(1.5f, currentLevel));
        }

        public bool PurchaseUpgrade(string statName)
        {
            UpgradeCard card = GetCardForStat(statName);
            if (card == null)
            {
                Debug.LogError($"[UpgradeShopHandler] Unknown stat: {statName}");
                return false;
            }

            int currentLevel = cachedSave.Data.GetStatLevel(statName);
            int cost = CalculateCost(card.BaseCost, currentLevel);

            if (cachedSave.Data.totalGold < cost)
            {
                Debug.LogWarning($"[UpgradeShopHandler] Not enough gold for {statName}. Need {cost}, have {cachedSave.Data.totalGold}");
                return false;
            }

            cachedSave.Data.totalGold -= cost;
            cachedSave.Data.SetStatLevel(statName, currentLevel + 1);
            cachedSave.Save();

            RefreshCard(card, statName);

            Debug.Log($"[UpgradeShopHandler] Purchased {statName} level {currentLevel + 1} for {cost} gold");
            return true;
        }

        private void ShowCardDetails(UpgradeCard card)
        {
            if (detailsPanel != null)
            {
                detailsPanel.ShowUpgradeDetails(
                    card.GetIcon(),
                    card.GetDisplayName(),
                    card.GetDescription()
                );
            }
        }

        private UpgradeCard GetCardForStat(string statName)
        {
            return statName switch
            {
                "might" => mightCard,
                "vitality" => vitalityCard,
                "greaves" => greavesCard,
                "magnet" => magnetCard,
                _ => null
            };
        }

        public void UpdateLocalization()
        {
            Initialize();
            RefreshAllCards();
        }
    }
}
