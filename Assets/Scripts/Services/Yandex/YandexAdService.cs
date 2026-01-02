#if UNITY_WEBGL
using System;
using System.Threading.Tasks;
using UnityEngine;
using YG;

namespace PixelVanguard.Services
{
    /// <summary>
    /// Ad service implementation for Yandex Games (WebGL).
    /// Uses Yandex rewarded ads via PluginYG.
    /// </summary>
    public class YandexAdService : IAdService
    {
        private const int COOLDOWN_SECONDS = 60; // 1 minute cooldown
        private TaskCompletionSource<bool> rewardedAdTask;
        private const string REWARD_ID = "gold_reward";

        public async Task Initialize()
        {
            // Subscribe to Yandex ad events
            YG2.onRewardAdv += OnRewardReceived;
            YG2.onErrorRewardedAdv += OnAdError;
            YG2.onCloseRewardedAdv += OnAdClosed;

            await Task.CompletedTask;
        }

        public bool IsRewardedAdReady()
        {
            // Yandex handles ad availability internally
            return true;
        }

        public async Task<bool> ShowRewardedAd()
        {
            rewardedAdTask = new TaskCompletionSource<bool>();

            // Call Yandex rewarded ad
            YG2.RewardedAdvShow(REWARD_ID);

            // Wait for result (reward received or error)
            bool result = await rewardedAdTask.Task;
            return result;
        }

        public void ShowInterstitialAd()
        {            
            // Call Yandex interstitial ad
            // Plugin handles cooldown and frequency internally
            YG2.InterstitialAdvShow();
        }

        public bool CanWatchAd(string lastWatchedTime)
        {
            if (string.IsNullOrEmpty(lastWatchedTime))
            {
                return true; // Never watched before
            }

            try
            {
                DateTime lastWatch = DateTime.Parse(lastWatchedTime);
                TimeSpan timeSince = DateTime.Now - lastWatch;
                return timeSince.TotalSeconds >= COOLDOWN_SECONDS;
            }
            catch
            {
                return true; // Invalid timestamp, allow watching
            }
        }

        public int GetCooldownRemainingSeconds(string lastWatchedTime)
        {
            if (string.IsNullOrEmpty(lastWatchedTime))
            {
                return 0;
            }

            try
            {
                DateTime lastWatch = DateTime.Parse(lastWatchedTime);
                TimeSpan timeSince = DateTime.Now - lastWatch;
                int remaining = COOLDOWN_SECONDS - (int)timeSince.TotalSeconds;
                return Mathf.Max(0, remaining);
            }
            catch
            {
                return 0;
            }
        }

        // === Yandex Event Handlers ===

        private void OnRewardReceived(string rewardID)
        {            
            // CRITICAL: Restore timeScale as PluginYG pauses but doesn't unpause
            Time.timeScale = 1f;
            
            if (rewardID == REWARD_ID)
            {
                rewardedAdTask?.TrySetResult(true);
            }
        }

        private void OnAdError()
        {
            Debug.LogWarning("[YandexAdService] Ad error occurred");
            
            // CRITICAL: Restore timeScale even on error
            Time.timeScale = 1f;
            
            rewardedAdTask?.TrySetResult(false);
        }

        private void OnAdClosed()
        {            
            // CRITICAL: Restore timeScale when ad closes
            Time.timeScale = 1f;
            
            // If task not completed yet (user closed ad without watching), fail the task
            rewardedAdTask?.TrySetResult(false);
            Debug.Log("HERE");
        }
    }
}
#endif
