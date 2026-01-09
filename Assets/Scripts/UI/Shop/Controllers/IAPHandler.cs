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
#if UNITY_ANDROID
            WatchSpecialAd();
#else
            PurchaseIAP();
#endif
        }

        private async void PurchaseIAP()
        {
            if (iapService == null || !iapService.IsInitialized)
            {
                Debug.LogError("[IAPHandler] IAP service not initialized!");
                return;
            }

            bool success = await iapService.PurchaseProduct(ProductIDs.GOLD_PACK_LARGE);

            if (success)
            {
                GrantGoldPackReward();
            }
            else
            {
                Debug.LogWarning("[IAPHandler] IAP purchase failed or cancelled");
            }
        }

        private async void WatchSpecialAd()
        {
            if (adService == null)
            {
                Debug.LogError("[IAPHandler] AdService missing!");
                return;
            }

            if (!adService.CanWatchAd(cachedSave.Data.lastAdWatchedTime))
            {
                int remaining = adService.GetCooldownRemainingSeconds(cachedSave.Data.lastAdWatchedTime);
                Debug.LogWarning($"[IAPHandler] Ad cooldown: {remaining}s remaining");
                return;
            }

            bool success = await adService.ShowRewardedAd();

            if (success)
            {
                cachedSave.Data.adsWatchedForSpecialPack++;
                cachedSave.Data.lastAdWatchedTime = System.DateTime.Now.ToString("o");

                if (cachedSave.Data.adsWatchedForSpecialPack >= SPECIAL_OFFER_AD_TARGET)
                {
                    cachedSave.Data.adsWatchedForSpecialPack = 0;
                    GrantGoldPackReward();
                }

                cachedSave.Save();
                UpdateButton();
            }
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

#if UNITY_ANDROID
            if (iapButtonText != null)
            {
                if (adService != null)
                {
                    int remaining = adService.GetCooldownRemainingSeconds(cachedSave.Data.lastAdWatchedTime);
                    if (remaining > 0)
                    {
                        iapButtonText.text = $"{remaining}s";
                        if (iapBuyButton != null) iapBuyButton.interactable = false;
                        return;
                    }
                }

                iapButtonText.text = $"{cachedSave.Data.adsWatchedForSpecialPack}/{SPECIAL_OFFER_AD_TARGET}";
                if (iapBuyButton != null) iapBuyButton.interactable = true;
            }
#else
            if (iapService == null || !iapService.IsInitialized)
            {
                if (iapButtonText) iapButtonText.text = "---";
                return;
            }

            string price = iapService.GetLocalizedPrice(ProductIDs.GOLD_PACK_LARGE);
            if (iapButtonText) iapButtonText.text = price;
#endif
        }
    }
}
