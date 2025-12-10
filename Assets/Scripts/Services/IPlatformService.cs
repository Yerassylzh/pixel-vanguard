namespace PixelVanguard.Services
{
    /// <summary>
    /// Platform-agnostic interface for platform-specific features.
    /// Implementations: AndroidPlatformService, WebPlatformService
    /// </summary>
    public interface IPlatformService
    {
        /// <summary>
        /// Check if the current platform uses mobile touch input.
        /// </summary>
        bool IsMobileInput();

        /// <summary>
        /// Check if the current platform is web-based.
        /// </summary>
        bool IsWebPlatform();

        /// <summary>
        /// Get the detected platform type.
        /// </summary>
        PlatformType GetPlatformType();

        /// <summary>
        /// Set fullscreen mode (Web only, ignored on mobile).
        /// </summary>
        void SetFullscreen(bool enabled);
    }

    public enum PlatformType
    {
        Android,
        YandexWeb,
        WebDesktop,
        UnityEditor
    }
}
