using UnityEngine;

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
                    Debug.Log($"[PlatformDetector] Auto-detected: {CurrentPlatform}");
                    break;

                case PlatformMode.AlwaysMobile:
                    CurrentPlatform = PlatformType.NativeMobile;
                    Debug.Log($"[PlatformDetector] Forced: Mobile (default for testing)");
                    break;

                case PlatformMode.AlwaysDesktop:
                    CurrentPlatform = PlatformType.Desktop;
                    Debug.Log($"[PlatformDetector] Forced: Desktop");
                    break;

                case PlatformMode.ForceSpecific:
                    CurrentPlatform = forcedPlatform;
                    Debug.Log($"[PlatformDetector] Forced: {forcedPlatform}");
                    break;
            }
        }

        private PlatformType AutoDetectPlatform()
        {
            // Check if running on mobile device
            if (Application.isMobilePlatform)
            {
                // Check if WebGL on mobile browser
                if (Application.platform == RuntimePlatform.WebGLPlayer)
                {
                    return PlatformType.WebMobile;
                }
                // Native mobile app
                return PlatformType.NativeMobile;
            }

            // Desktop (Windows, Mac, Linux, WebGL on desktop)
            return PlatformType.Desktop;
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
            Debug.Log($"[PlatformDetector] Runtime force: {platform}");
            
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
