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
        
        // === DEBUG: Simulate Poor Network ===
        private const bool SIMULATE_SLOW_LOADING = false; // Set true to test "Wait.." UI
        private const int SIMULATED_LOAD_DELAY_MS = 3000; // 3 second delay
        private const bool SIMULATE_AD_FAILURE = false;   // Set true to test "No ad" UI

        public bool IsRewardedAdReady()
        {
            // Placeholder: always ready
            return true;
        }

        public async Task<bool> ShowRewardedAd()
        {            
            // === DEBUG: Simulate slow loading ===
            if (SIMULATE_SLOW_LOADING)
            {
                Debug.LogWarning($"[PlaceholderAdService] DEBUG: Simulating {SIMULATED_LOAD_DELAY_MS}ms ad load delay");
                await Task.Delay(SIMULATED_LOAD_DELAY_MS);
            }
            
            // === DEBUG: Simulate ad failure ===
            if (SIMULATE_AD_FAILURE)
            {
                Debug.LogWarning("[PlaceholderAdService] DEBUG: Simulating ad failure");
                await Task.Delay(1000);
                return false;
            }
            
            // Simulate ad loading delay
            await Task.Delay(500);
            
            // Placeholder: always succeeds (unless debug failure enabled)
            return true;
        }

        public void ShowInterstitialAd()
        {
            // Placeholder: instant, no reward
        }

        public void Initialize()
        {
            Debug.Log("[PlaceholderAdService] Initialized (Editor/Test mode)");
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
