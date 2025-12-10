using System;
using System.Threading.Tasks;

namespace PixelVanguard.Services
{
    /// <summary>
    /// Platform-agnostic interface for ad services.
    /// Implementations: AdMobService (Android), YandexAdService (Web), NoAdService (Desktop/Fallback)
    /// </summary>
    public interface IAdService
    {
        /// <summary>
        /// Check if a rewarded ad is ready to show.
        /// </summary>
        bool IsRewardedAdReady();

        /// <summary>
        /// Show a rewarded ad. Callback returns true if user watched the ad completely.
        /// </summary>
        /// <param name="onComplete">Called with success status when ad finishes or fails</param>
        void ShowRewardedAd(Action<bool> onComplete);

        /// <summary>
        /// Check if an interstitial ad is ready to show.
        /// </summary>
        bool IsInterstitialAdReady();

        /// <summary>
        /// Show an interstitial ad. Callback called when ad finishes or fails.
        /// </summary>
        /// <param name="onComplete">Called when ad closes</param>
        void ShowInterstitialAd(Action onComplete);

        /// <summary>
        /// Initialize the ad service. Call during Bootstrap.
        /// </summary>
        Task Initialize();
    }
}
