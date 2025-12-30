using UnityEngine;
using PixelVanguard.Services;

namespace PixelVanguard.Core
{
    /// <summary>
    /// Persistent singleton that initializes core services on Awake.
    /// Place this in your Main Menu scene - it will persist across all scenes.
    /// </summary>
    public class GameBootstrap : MonoBehaviour
    {
        public static GameBootstrap Instance { get; private set; }

        private void Awake()
        {
            // Singleton pattern with DontDestroyOnLoad
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            Debug.Log("[GameBootstrap] Initializing core services...");

            // Initialize ServiceLocator
            InitializeServiceLocator();

            // Load and apply audio settings
            InitializeAudioSettings();

            Debug.Log("[GameBootstrap] ✅ Initialization complete!");
        }

        private async void InitializeServiceLocator()
        {
            if (ServiceLocator.Get<ISaveService>() == null)
            {
                var saveService = new PlayerPrefsSaveService();
                ServiceLocator.Register<ISaveService>(saveService);
                Debug.Log("[GameBootstrap] ✅ SaveService registered");
            }
            
            if (ServiceLocator.Get<IAdService>() == null)
            {
                var adService = new PlaceholderAdService();
                await adService.Initialize(); // Initialize before registering!
                ServiceLocator.Register<IAdService>(adService);
                Debug.Log("[GameBootstrap] ✅ PlaceholderAdService registered & initialized");
            }
            
            if (ServiceLocator.Get<IIAPService>() == null)
            {
                var iapService = new PlaceholderIAPService();
                await iapService.Initialize(); // Initialize before registering!
                ServiceLocator.Register<IIAPService>(iapService);
                Debug.Log("[GameBootstrap] ✅ PlaceholderIAPService registered & initialized");
            }
        }

        private void InitializeAudioSettings()
        {
            // Load saved settings
            float savedSFXVolume = GameSettings.SFXVolume;
            float savedMusicVolume = GameSettings.MusicVolume;

            Debug.Log($"[GameBootstrap] Audio settings - SFX: {savedSFXVolume:F2}, Music: {savedMusicVolume:F2}");

            // Apply when AudioManager becomes available
            StartCoroutine(ApplyAudioSettingsWhenReady(savedSFXVolume, savedMusicVolume));
        }

        private System.Collections.IEnumerator ApplyAudioSettingsWhenReady(float sfxVolume, float musicVolume)
        {
            // Wait for AudioManager to exist
            while (AudioManager.Instance == null)
            {
                yield return null;
            }

            // Apply volumes
            AudioManager.Instance.SetSFXVolume(sfxVolume);
            AudioManager.Instance.SetMusicVolume(musicVolume);

            Debug.Log("[GameBootstrap] ✅ Applied audio settings to AudioManager");
        }
    }
}
