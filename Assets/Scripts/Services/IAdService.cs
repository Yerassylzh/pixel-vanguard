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
        /// <param name="onAdWatched">Callback when ad is successfully watched</param>
        /// <param>
        /// <param name="onAdFailed">Callback when ad fails to show or is skipped</param>
        Task<bool> ShowRewardedAd();

        /// <summary>
        /// Initialize the ad service (called by GameBootstrap).
        /// </summary>
        Task Initialize();

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
