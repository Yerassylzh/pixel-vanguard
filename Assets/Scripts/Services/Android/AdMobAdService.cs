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

        // private const string REWARDED_AD_UNIT_ID = "ca-app-pub-3940256099942544/5224354917"; 
        // private const string INTERSTITIAL_AD_UNIT_ID = "ca-app-pub-3940256099942544/1033173712";

        // === DEBUG: Simulate Poor Network ===
        private const bool SIMULATE_SLOW_LOADING = false; // Set true to test "Wait.." UI
        private const int SIMULATED_LOAD_DELAY_MS = 3000; // 3 second delay
        private const bool SIMULATE_AD_FAILURE = false;   // Set true to test "No ad" UI


        private RewardedAd _rewardedAd;
        private InterstitialAd _interstitialAd;
        private TaskCompletionSource<bool> _rewardedAdTcs;
        private System.Action _interstitialOnComplete;

        public void Initialize()
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
        }

        public bool IsRewardedAdReady()
        {
            return _rewardedAd != null && _rewardedAd.CanShowAd();
        }

        public async Task<bool> ShowRewardedAd()
        {
            // === DEBUG: Simulate slow loading ===
            if (SIMULATE_SLOW_LOADING)
            {
                Debug.LogWarning($"[AdMobAdService] DEBUG: Simulating {SIMULATED_LOAD_DELAY_MS}ms ad load delay");
                await Task.Delay(SIMULATED_LOAD_DELAY_MS);
            }
            
            // === DEBUG: Simulate ad failure ===
            if (SIMULATE_AD_FAILURE)
            {
                Debug.LogWarning("[AdMobAdService] DEBUG: Simulating ad failure");
                await Task.Delay(1000); // Simulate failed load attempt
                return false;
            }
            
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

        public void ShowInterstitialAd(System.Action onComplete = null)
        {
            // === AdMob only: respect IAP "Remove Ads" purchase ===
            if (AreAdsRemoved())
            {
                Debug.Log("[AdMob] Ads removed — skipping interstitial.");
                onComplete?.Invoke();
                return;
            }

            if (_interstitialAd != null && _interstitialAd.CanShowAd())
            {
                _interstitialOnComplete = onComplete;
                _interstitialAd.Show();
                // onComplete is called in OnAdFullScreenContentClosed / Failed
            }
            else
            {
                Debug.LogWarning("[AdMob] Interstitial ad not ready. Proceeding without ad.");
                onComplete?.Invoke();
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
                var cb = _interstitialOnComplete;
                _interstitialOnComplete = null;
                cb?.Invoke();
            };
            ad.OnAdFullScreenContentFailed += (AdError error) =>
            {
                Debug.LogError($"[AdMob] Interstitial ad failed to show: {error}");
                LoadInterstitialAd();
                var cb = _interstitialOnComplete;
                _interstitialOnComplete = null;
                cb?.Invoke();
            };
        }

        /// <summary>
        /// Check if the player has purchased "Remove Ads" via IAP.
        /// Reads from CachedSaveDataService (same source of truth used everywhere).
        /// </summary>
        private bool AreAdsRemoved()
        {
            var cachedSave = Core.ServiceLocator.TryGet<CachedSaveDataService>(out var svc)
                ? svc : null;
            return cachedSave?.Data?.adsRemoved == true;
        }

        public bool CanWatchAd(string lastWatchedTime)
        {
            return GetCooldownRemainingSeconds(lastWatchedTime) <= 0;
        }

        public int GetCooldownRemainingSeconds(string lastWatchedTime)
        {
            int cooldownSeconds = 60;

            if (string.IsNullOrEmpty(lastWatchedTime)) return 0;
            if (DateTime.TryParse(lastWatchedTime, out DateTime lastTime))
            {
                var diff = DateTime.Now - lastTime;
                if (diff.TotalSeconds < cooldownSeconds)
                {
                    return cooldownSeconds - (int)diff.TotalSeconds;
                }
            }
            return 0;
        }
    }
}
#endif
