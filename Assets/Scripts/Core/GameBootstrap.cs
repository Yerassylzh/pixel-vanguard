using UnityEngine;
using PixelVanguard.Services;
using PixelVanguard.Data;

namespace PixelVanguard.Core
{
    /// <summary>
    /// Persistent singleton that initializes core services on Awake.
    /// Place this in your Main Menu scene - it will persist across all scenes.
    /// </summary>
    [DefaultExecutionOrder(-100)] // Ensure this runs before other scripts
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

            // Initialize ServiceLocator
            InitializeServiceLocator();

            // Load and apply audio settings
            InitializeAudioSettings();
        }

        private void InitializeServiceLocator()
        {
            // === Save Service ===
            if (!ServiceLocator.Has<ISaveService>())
            {
                var saveService = PlatformServiceFactory.CreateSaveService();
                saveService.Initialize();
                ServiceLocator.Register<ISaveService>(saveService);
            }

            // === Ad Service ===
            if (!ServiceLocator.Has<IAdService>())
            {
                var adService = PlatformServiceFactory.CreateAdService();
                _ = adService.Initialize(); // Fire and forget
                ServiceLocator.Register<IAdService>(adService);
            }

            // === IAP Service ===
            if (!ServiceLocator.Has<IIAPService>())
            {
                var iapService = PlatformServiceFactory.CreateIAPService();
                _ = iapService.Initialize(); // Fire and forget
                ServiceLocator.Register<IIAPService>(iapService);
            }

            // === Localization System ===
            InitializeLocalization();
        }

        private void InitializeLocalization()
        {
            // Create and initialize language provider
            var languageProvider = PlatformServiceFactory.CreateLanguageProvider();
            languageProvider.Initialize();

            // Load translation data from Resources
            var translationData = Resources.Load<TranslationData>("Translations");

            if (translationData == null)
            {
                Debug.LogError("[GameBootstrap] ❌ Translations.asset not found in Resources folder!");
                Debug.LogError("[GameBootstrap] Please create it via: Assets → Create → Localization → Translation Data");
                return;
            }

            // Initialize LocalizationManager
            LocalizationManager.Initialize(languageProvider, translationData);
            
            // === Game Settings Service ===
            // Must be initialized after ISaveService and before AudioManager is used
            if (!ServiceLocator.Has<GameSettings>())
            {
                var saveService = ServiceLocator.Get<ISaveService>();
                var gameSettings = new GameSettings(saveService);
                ServiceLocator.Register<GameSettings>(gameSettings);
            }
        }

        private void InitializeAudioSettings()
        {
            // Get GameSettings service
            var gameSettings = ServiceLocator.Get<GameSettings>();
            
            if (gameSettings == null)
            {
                Debug.LogWarning("[GameBootstrap] GameSettings not initialized yet!");
                return;
            }

            // Apply when AudioManager becomes available
            StartCoroutine(ApplyAudioSettingsWhenReady(gameSettings));
        }

        private System.Collections.IEnumerator ApplyAudioSettingsWhenReady(GameSettings gameSettings)
        {
            // Wait for AudioManager to exist
            while (AudioManager.Instance == null)
            {
                yield return null;
            }

            // Apply volumes from GameSettings
            AudioManager.Instance.SetSFXVolume(gameSettings.SFXVolume);
            AudioManager.Instance.SetMusicVolume(gameSettings.MusicVolume);
        }
    }
}
