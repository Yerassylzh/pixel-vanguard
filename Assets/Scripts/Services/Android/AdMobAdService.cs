#if UNITY_ANDROID
using UnityEngine;
using GoogleMobileAds.Api;
using System;
using System.Threading.Tasks;

namespace PixelVanguard.Services
{
    /// <summary>
    /// AdMob implementation for Android.
    /// Requires Google Mobile Ads SDK.
    /// </summary>
    public class AdMobAdService : IAdService
    {
        // Replace these with your actual Ad Unit IDs from AdMob Console
        // These are official Google Test IDs
        private const string REWARDED_AD_UNIT_ID = "ca-app-pub-4326973674601582/2935167324"; 
        private const string INTERSTITIAL_AD_UNIT_ID = "ca-app-pub-4326973674601582/9876497111";

        private RewardedAd _rewardedAd;
        private InterstitialAd _interstitialAd;
        private TaskCompletionSource<bool> _rewardedAdTcs;

        public async Task Initialize()
        {
            var requestConfiguration = new RequestConfiguration
            {
                TagForChildDirectedTreatment =
                    TagForChildDirectedTreatment.Unspecified,

                TagForUnderAgeOfConsent =
                    TagForUnderAgeOfConsent.Unspecified
            };  

            MobileAds.SetRequestConfiguration(requestConfiguration);

            // Initialize the Google Mobile Ads SDK.
            MobileAds.Initialize(initStatus =>
            {
                Debug.Log($"[AdMob] Initialized: {initStatus}");
                // Pre-load ads after initialization
                LoadRewardedAd();
                LoadInterstitialAd();
            });

            await Task.CompletedTask;
        }

        public bool IsRewardedAdReady()
        {
            return _rewardedAd != null && _rewardedAd.CanShowAd();
        }

        public async Task<bool> ShowRewardedAd()
        {
            if (_rewardedAd != null && _rewardedAd.CanShowAd())
            {
                _rewardedAdTcs = new TaskCompletionSource<bool>();

                _rewardedAd.Show((Reward reward) =>
                {
                    Debug.Log($"[AdMob] User earned reward: {reward.Amount} {reward.Type}");
                    _rewardedAdTcs.TrySetResult(true);
                });

                bool result = await _rewardedAdTcs.Task;
                
                // Reload for next time
                LoadRewardedAd();
                
                return result;
            }
            else
            {
                Debug.LogWarning("[AdMob] Rewarded ad not ready.");
                return false;
            }
        }

        public void ShowInterstitialAd()
        {
            if (_interstitialAd != null && _interstitialAd.CanShowAd())
            {
                _interstitialAd.Show();
                // Reload happens in event handler
            }
            else
            {
                Debug.LogWarning("[AdMob] Interstitial ad not ready.");
            }
        }

        private void LoadRewardedAd()
        {
            if (_rewardedAd != null)
            {
                _rewardedAd.Destroy();
                _rewardedAd = null;
            }

            Debug.Log("[AdMob] Loading Rewarded Ad...");
            var adRequest = new AdRequest();

            RewardedAd.Load(REWARDED_AD_UNIT_ID, adRequest,
                (RewardedAd ad, LoadAdError error) =>
                {
                    if (error != null || ad == null)
                    {
                        Debug.LogError($"[AdMob] Rewarded ad failed to load: {error}");
                        return;
                    }

                    Debug.Log($"[AdMob] Rewarded ad loaded: {ad.GetResponseInfo()}");
                    _rewardedAd = ad;
                    RegisterRewardedAdEvents(_rewardedAd);
                });
        }

        private void LoadInterstitialAd()
        {
            if (_interstitialAd != null)
            {
                _interstitialAd.Destroy();
                _interstitialAd = null;
            }

            Debug.Log("[AdMob] Loading Interstitial Ad...");
            var adRequest = new AdRequest();

            InterstitialAd.Load(INTERSTITIAL_AD_UNIT_ID, adRequest,
                (InterstitialAd ad, LoadAdError error) =>
                {
                    if (error != null || ad == null)
                    {
                        Debug.LogError($"[AdMob] Interstitial ad failed to load: {error}");
                        return;
                    }

                    Debug.Log($"[AdMob] Interstitial ad loaded: {ad.GetResponseInfo()}");
                    _interstitialAd = ad;
                    RegisterInterstitialAdEvents(_interstitialAd);
                });
        }

        private void RegisterRewardedAdEvents(RewardedAd ad)
        {
            ad.OnAdFullScreenContentClosed += () =>
            {
                Debug.Log("[AdMob] Rewarded ad closed.");
                // If closed without reward, ensure task completes
                if (_rewardedAdTcs != null && !_rewardedAdTcs.Task.IsCompleted)
                {
                    _rewardedAdTcs.TrySetResult(false);
                }
                LoadRewardedAd();
            };
            ad.OnAdFullScreenContentFailed += (AdError error) =>
            {
                Debug.LogError($"[AdMob] Rewarded ad failed to show: {error}");
                 if (_rewardedAdTcs != null && !_rewardedAdTcs.Task.IsCompleted)
                {
                    _rewardedAdTcs.TrySetResult(false);
                }
                LoadRewardedAd();
            };
        }

        private void RegisterInterstitialAdEvents(InterstitialAd ad)
        {
            ad.OnAdFullScreenContentClosed += () =>
            {
                Debug.Log("[AdMob] Interstitial ad closed.");
                LoadInterstitialAd();
            };
            ad.OnAdFullScreenContentFailed += (AdError error) =>
            {
                Debug.LogError($"[AdMob] Interstitial ad failed to show: {error}");
                LoadInterstitialAd();
            };
        }

        public bool CanWatchAd(string lastWatchedTime)
        {
            return GetCooldownRemainingSeconds(lastWatchedTime) <= 0;
        }

        public int GetCooldownRemainingSeconds(string lastWatchedTime)
        {
            if (string.IsNullOrEmpty(lastWatchedTime)) return 0;
            if (DateTime.TryParse(lastWatchedTime, out DateTime lastTime))
            {
                var diff = DateTime.Now - lastTime;
                if (diff.TotalSeconds < 60)
                {
                    return 60 - (int)diff.TotalSeconds;
                }
            }
            return 0;
        }
    }
}
#endif
