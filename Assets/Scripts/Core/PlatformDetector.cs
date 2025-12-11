using UnityEngine;

namespace PixelVanguard.Core
{
    /// <summary>
    /// Detects and stores current platform type.
    /// For development: manually assignable in Inspector.
    /// For production: auto-detects at runtime.
    /// </summary>
    public class PlatformDetector : MonoBehaviour
    {
        public static PlatformDetector Instance { get; private set; }

        [Header("Platform Override (Dev)")]
        [SerializeField] private bool useManualOverride = true;
        [SerializeField] private PlatformType manualPlatform = PlatformType.Desktop;

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
            if (useManualOverride)
            {
                CurrentPlatform = manualPlatform;
                Debug.Log($"[PlatformDetector] Manual override: {CurrentPlatform}");
            }
            else
            {
                CurrentPlatform = DetectPlatform();
                Debug.Log($"[PlatformDetector] Auto-detected: {CurrentPlatform}");
            }
        }

        private PlatformType DetectPlatform()
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
    }

    public enum PlatformType
    {
        Desktop,        // PC (Windows/Mac/Linux) or WebGL on desktop browser
        NativeMobile,   // iOS/Android native app
        WebMobile       // WebGL on mobile browser
    }
}
