using UnityEngine;
using System;
using System.Threading.Tasks;

namespace PixelVanguard.Services
{
    /// <summary>
    /// Placeholder ad service for testing.
    /// Replace with real implementation (Unity Ads, AdMob, Yandex).
    /// </summary>
    public class PlaceholderAdService : IAdService
    {
        private const int COOLDOWN_SECONDS = 60; // 1 minute cooldown

        public bool IsRewardedAdReady()
        {
            // Placeholder: always ready
            return true;
        }

        public async Task<bool> ShowRewardedAd()
        {            
            // Simulate ad loading delay
            await Task.Delay(500);
            
            // Placeholder: always succeeds
            return true;
        }

        public void ShowInterstitialAd()
        {
            // Placeholder: instant, no reward
        }

        public async Task Initialize()
        {
            await Task.CompletedTask;
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
    }
}
