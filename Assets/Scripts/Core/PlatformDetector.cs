using UnityEngine;
using YG;


namespace PixelVanguard.Core
{
    /// <summary>
    /// Detects and manages current platform type.
    /// Defaults to Mobile for testing.
    /// Production: Auto-detects platform at runtime.
    /// </summary>
    public class PlatformDetector : MonoBehaviour
    {
        public static PlatformDetector Instance { get; private set; }

        [Header("Platform Configuration")]
        [SerializeField] private PlatformMode mode = PlatformMode.AutoDetect;
        [SerializeField] private PlatformType forcedPlatform = PlatformType.NativeMobile;

        public PlatformType CurrentPlatform { get; private set; }

        private void Awake()
        {
            // Singleton
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Determine platform
            DeterminePlatform();
        }

        private void DeterminePlatform()
        {
            switch (mode)
            {
                case PlatformMode.AutoDetect:
                    CurrentPlatform = AutoDetectPlatform();
                    break;

                case PlatformMode.AlwaysMobile:
                    CurrentPlatform = PlatformType.NativeMobile;
                    break;

                case PlatformMode.AlwaysDesktop:
                    CurrentPlatform = PlatformType.Desktop;
                    break;

                case PlatformMode.ForceSpecific:
                    CurrentPlatform = forcedPlatform;
                    break;
            }
        }

        private PlatformType AutoDetectPlatform()
        {
#if UNITY_WEBGL
            // For WebGL builds, use PluginYG to detect platform
            try
            {
                if (YG2.envir.isDesktop)
                {
                    return PlatformType.Desktop;
                }
                else
                {
                    return PlatformType.WebMobile;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[PlatformDetector] Failed to access PluginYG platform detection: {e.Message}. Falling back to Unity detection.");
                // Fallback to Unity's built-in detection
                return Application.isMobilePlatform ? PlatformType.WebMobile : PlatformType.Desktop;
            }
#else
            return PlatformType.NativeMobile;
#endif
        }

        public bool IsMobile()
        {
            return CurrentPlatform == PlatformType.NativeMobile || CurrentPlatform == PlatformType.WebMobile;
        }

        public bool IsDesktop()
        {
            return CurrentPlatform == PlatformType.Desktop;
        }

        /// <summary>
        /// Force platform change at runtime (for testing only).
        /// </summary>
        public void ForcePlatform(PlatformType platform)
        {
            CurrentPlatform = platform;

            // Notify listeners to refresh UI/input
            GameEvents.TriggerPlatformChanged(platform);
        }
    }

    public enum PlatformMode
    {
        AutoDetect,      // Production: Auto-detect at runtime
        AlwaysMobile,    // Testing: Always mobile (default)
        AlwaysDesktop,   // Testing: Always desktop
        ForceSpecific    // Testing: Use forcedPlatform value
    }

    public enum PlatformType
    {
        Desktop,        // PC (Windows/Mac/Linux) or WebGL on desktop browser
        NativeMobile,   // iOS/Android native app
        WebMobile       // WebGL on mobile browser
    }
}
