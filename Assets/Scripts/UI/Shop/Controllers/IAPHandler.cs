using UnityEngine;
using UnityEngine.UI;
using TMPro;
using PixelVanguard.Services;
using PixelVanguard.UI.Animations;
using System.Threading.Tasks;

namespace PixelVanguard.UI.Shop
{
    public class IAPHandler
    {
        private readonly CachedSaveDataService cachedSave;
        private readonly IAdService adService;
        private readonly IIAPService iapService;
        private readonly Button iapBuyButton;
        private readonly TextMeshProUGUI iapButtonText;
        private readonly CoinRewardAnimator coinRewardAnimator;
        private readonly Transform goldIconTransform;
        private readonly TextMeshProUGUI goldText;
        
        private const int SPECIAL_OFFER_AD_TARGET = 20;
        private const int GOLD_PACK_REWARD = 29900;

        public IAPHandler(
            CachedSaveDataService cachedSave,
            IAdService adService,
            IIAPService iapService,
            Button iapBuyButton,
            TextMeshProUGUI iapButtonText,
            CoinRewardAnimator coinRewardAnimator,
            Transform goldIconTransform,
            TextMeshProUGUI goldText)
        {
            this.cachedSave = cachedSave;
            this.adService = adService;
            this.iapService = iapService;
            this.iapBuyButton = iapBuyButton;
            this.iapButtonText = iapButtonText;
            this.coinRewardAnimator = coinRewardAnimator;
            this.goldIconTransform = goldIconTransform;
            this.goldText = goldText;
        }

        public void HandleIAPButtonClick()
        {
            PurchaseIAP();
        }

        private async void PurchaseIAP()
        {
            if (iapService == null || !iapService.IsInitialized)
            {
                Debug.LogError("[IAPHandler] IAP service not initialized!");
                return;
            }

            // Disable button during purchase flow
            SetButtonState(Core.LocalizationManager.Get("ui.shop.ad_loading"), false);

            bool success = await iapService.PurchaseProduct(ProductIDs.GOLD_PACK_LARGE);

            if (success)
            {
                // Gold is granted by GooglePlayIAPService.OnPurchasePending → GrantGoldPack()
                // Play coin animation locally for UX feedback
                if (goldIconTransform != null && iapBuyButton != null && coinRewardAnimator != null)
                {
                    coinRewardAnimator.PlayCoinReward(
                        iapBuyButton.transform.position,
                        goldIconTransform,
                        GOLD_PACK_REWARD,
                        goldText,
                        onComplete: null
                    );
                }
            }
            else
            {
                Debug.LogWarning("[IAPHandler] IAP purchase failed or cancelled");
            }

            // Restore button state
            UpdateButton();
        }

        private void GrantGoldPackReward()
        {
            cachedSave.Data.totalGold += GOLD_PACK_REWARD;
            cachedSave.Save();

            if (goldIconTransform != null && iapBuyButton != null && coinRewardAnimator != null)
            {
                coinRewardAnimator.PlayCoinReward(
                    iapBuyButton.transform.position,
                    goldIconTransform,
                    GOLD_PACK_REWARD,
                    goldText,
                    onComplete: null
                );
            }
        }

        public void UpdateButton()
        {
            if (cachedSave.Data == null) return;

            if (iapService == null || !iapService.IsInitialized)
            {
                if (iapButtonText) iapButtonText.text = "---";
                return;
            }

            string price = iapService.GetLocalizedPrice(ProductIDs.GOLD_PACK_LARGE);
            if (iapButtonText) iapButtonText.text = price;
            if (iapBuyButton != null) iapBuyButton.interactable = true;
        }

        /// <summary>
        /// Helper to set button text and interactable state.
        /// </summary>
        private void SetButtonState(string text, bool interactable)
        {
            if (iapButtonText != null) iapButtonText.text = text;
            if (iapBuyButton != null) iapBuyButton.interactable = interactable;
        }
    }
}
