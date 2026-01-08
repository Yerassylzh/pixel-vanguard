using UnityEngine;

namespace PixelVanguard.Services
{
    /// <summary>
    /// Factory for creating platform-specific service implementations.
    /// Uses conditional compilation to select appropriate services for WebGL (Yandex) or Android/Editor.
    /// </summary>
    public static class PlatformServiceFactory
    {
        /// <summary>
        /// Create save service for current platform.
        /// WebGL: YandexSaveService (cloud saves)
        /// Android/Other: PlayerPrefsSaveService (local saves)
        /// </summary>
        public static ISaveService CreateSaveService()
        {
#if UNITY_WEBGL
            return new YandexSaveService();
#else
            return new PlayerPrefsSaveService();
#endif
        }

        /// <summary>
        /// Create ad service for current platform.
        /// WebGL: YandexAdService (Yandex Ads)
        /// Android/Other: PlaceholderAdService (ready for Unity Ads integration)
        /// </summary>
        public static IAdService CreateAdService()
        {
#if UNITY_WEBGL
            return new YandexAdService();
#elif UNITY_ANDROID
            return new AdMobAdService();
#else
            return new PlaceholderAdService();
#endif
        }

        /// <summary>
        /// Create IAP service for current platform.
        /// WebGL: YandexIAPService (Yandex Payments)
        /// Android/Other: PlaceholderIAPService (ready for Google Play Billing integration)
        /// </summary>
        public static IIAPService CreateIAPService()
        {
#if UNITY_WEBGL
            return new YandexIAPService();
#else
            return new PlaceholderIAPService();
#endif
        }

        /// <summary>
        /// Create language provider for current platform.
        /// WebGL: YandexLanguageProvider (Yandex language detection)
        /// Android/Other: DefaultLanguageProvider (PlayerPrefs)
        /// </summary>
        public static ILanguageProvider CreateLanguageProvider()
        {
#if UNITY_WEBGL
            return new YandexLanguageProvider();
#else
            return new DefaultLanguageProvider();
#endif
        }
    }
}
