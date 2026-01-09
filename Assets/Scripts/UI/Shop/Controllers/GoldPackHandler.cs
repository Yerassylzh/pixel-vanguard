using UnityEngine;
using TMPro;
using PixelVanguard.Core;
using PixelVanguard.Services;
using PixelVanguard.UI.Animations;
using System.Threading.Tasks;
using System;

namespace PixelVanguard.UI.Shop
{
    public class GoldPackHandler
    {
        private readonly CachedSaveDataService cachedSave;
        private readonly IAdService adService;
        private readonly AdPackCard adPack1Card;
        private readonly AdPackCard adPack2Card;
        private readonly Sprite adPack1Icon;
        private readonly Sprite adPack2Icon;
        private readonly CoinRewardAnimator coinRewardAnimator;
        private readonly Transform goldIconTransform;
        private readonly TextMeshProUGUI goldText;

        public GoldPackHandler(
            CachedSaveDataService cachedSave,
            IAdService adService,
            AdPackCard adPack1Card,
            AdPackCard adPack2Card,
            Sprite adPack1Icon,
            Sprite adPack2Icon,
            CoinRewardAnimator coinRewardAnimator,
            Transform goldIconTransform,
            TextMeshProUGUI goldText)
        {
            this.cachedSave = cachedSave;
            this.adService = adService;
            this.adPack1Card = adPack1Card;
            this.adPack2Card = adPack2Card;
            this.adPack1Icon = adPack1Icon;
            this.adPack2Icon = adPack2Icon;
            this.coinRewardAnimator = coinRewardAnimator;
            this.goldIconTransform = goldIconTransform;
            this.goldText = goldText;
        }

        public void Initialize()
        {
            adPack1Card.Initialize(5, 1990, adPack1Icon, () => WatchAd(1),
                LocalizationManager.Get("ui.shop.ad_pack.desc_5"));
            adPack2Card.Initialize(10, 4990, adPack2Icon, () => WatchAd(2),
                LocalizationManager.Get("ui.shop.ad_pack.desc_10"));
        }

        public void SubscribeCardClickEvents(Action<AdPackCard> onCardClickCallback)
        {
            adPack1Card.OnCardClicked += () => onCardClickCallback(adPack1Card);
            adPack2Card.OnCardClicked += () => onCardClickCallback(adPack2Card);
        }

        public void RefreshCards()
        {
            adPack1Card.UpdateProgress(cachedSave.Data.adsWatchedForPack1, cachedSave.Data.totalGold);
            adPack2Card.UpdateProgress(cachedSave.Data.adsWatchedForPack2, cachedSave.Data.totalGold);
        }

        public async void WatchAd(int packNumber)
        {
            if (adService == null)
            {
                Debug.LogError("[GoldPackHandler] AdService missing!");
                return;
            }

            if (!adService.CanWatchAd(cachedSave.Data.lastAdWatchedTime))
            {
                int remaining = adService.GetCooldownRemainingSeconds(cachedSave.Data.lastAdWatchedTime);
                Debug.LogWarning($"[GoldPackHandler] Ad cooldown: {remaining}s remaining");
                return;
            }

            bool success = await adService.ShowRewardedAd();

            if (success)
            {
                int goldEarned = 0;
                Transform sourceTransform = null;

                if (packNumber == 1)
                {
                    cachedSave.Data.adsWatchedForPack1++;

                    if (cachedSave.Data.adsWatchedForPack1 >= 5)
                    {
                        goldEarned = 1990;
                        cachedSave.Data.totalGold += goldEarned;
                        cachedSave.Data.adsWatchedForPack1 = 0;
                        sourceTransform = adPack1Card.transform;
                    }
                }
                else if (packNumber == 2)
                {
                    cachedSave.Data.adsWatchedForPack2++;

                    if (cachedSave.Data.adsWatchedForPack2 >= 10)
                    {
                        goldEarned = 4990;
                        cachedSave.Data.totalGold += goldEarned;
                        cachedSave.Data.adsWatchedForPack2 = 0;
                        sourceTransform = adPack2Card.transform;
                    }
                }

                cachedSave.Data.lastAdWatchedTime = DateTime.Now.ToString("o");
                cachedSave.Save();

                if (goldEarned > 0 && goldIconTransform != null && sourceTransform != null && coinRewardAnimator != null)
                {
                    coinRewardAnimator.PlayCoinReward(
                        sourceTransform.position,
                        goldIconTransform,
                        goldEarned,
                        goldText,
                        onComplete: () => RefreshCards() // Refresh after animation
                    );
                }
                
                // Always refresh cards to show updated progress
                RefreshCards();
            }
            else
            {
                Debug.LogWarning("[GoldPackHandler] Ad failed or cancelled");
            }
        }

        public void UpdateCooldown(int remainingSeconds)
        {
            // Update both cards with cooldown state
            adPack1Card.UpdateCooldown(remainingSeconds);
            adPack2Card.UpdateCooldown(remainingSeconds);
            
            // When cooldown ends, refresh progress
            if (remainingSeconds == 0)
            {
                RefreshCards();
            }
        }

        public void UpdateLocalization()
        {
            Initialize();
            RefreshCards();
        }
    }
}
