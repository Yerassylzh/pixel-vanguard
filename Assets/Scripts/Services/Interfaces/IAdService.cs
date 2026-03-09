using System;
using System.Threading.Tasks;

namespace PixelVanguard.Services
{
    /// <summary>
    /// Interface for rewarded ad services.
    /// Implement this for Unity Ads, AdMob, Yandex Ads, etc.
    /// </summary>
    public interface IAdService
    {
        /// <summary>
        /// Is a rewarded ad ready to show?
        /// </summary>
        bool IsRewardedAdReady();

        /// <summary>
        /// Show a rewarded ad.
        /// </summary>
        /// <returns>True if ad was watched and reward should be granted</returns>
        Task<bool> ShowRewardedAd();

        /// <summary>
        /// Show an interstitial ad (non-rewarded, typically at transitions).
        /// onComplete is called when the ad closes or fails (or immediately if no ad is ready).
        /// </summary>
        void ShowInterstitialAd(System.Action onComplete = null);

        /// <summary>
        /// Initialize the ad service (called by GameBootstrap).
        /// </summary>
        void Initialize();

        /// <summary>
        /// Check if ad cooldown period has passed (60 seconds).
        /// </summary>
        bool CanWatchAd(string lastWatchedTime);

        /// <summary>
        /// Get remaining cooldown time in seconds.
        /// Returns 0 if cooldown is over.
        /// </summary>
        int GetCooldownRemainingSeconds(string lastWatchedTime);
    }
}
