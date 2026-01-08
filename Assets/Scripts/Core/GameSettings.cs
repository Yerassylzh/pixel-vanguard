using UnityEngine;
using PixelVanguard.Services;

namespace PixelVanguard.Core
{
    /// <summary>
    /// Global settings manager for gameplay preferences.
    /// Uses centralized CachedSaveDataService for persistence.
    /// </summary>
    public class GameSettings
    {
        private readonly CachedSaveDataService cachedSave;
        private readonly IAudioConfig audioConfig;

        // Interface for audio defaults (to avoid dependency on AudioManager singleton)
        public interface IAudioConfig
        {
            float DefaultSFXVolume { get; }
            float DefaultMusicVolume { get; }
        }

        // Default implementation using AudioManager
        private class AudioManagerConfig : IAudioConfig
        {
            public float DefaultSFXVolume => AudioManager.Instance?.DefaultSFXVolume ?? 0.67f;
            public float DefaultMusicVolume => AudioManager.Instance?.DefaultMusicVolume ?? 0.67f;
        }

        /// <summary>
        /// Should damage numbers be displayed?
        /// </summary>
        public bool ShowDamageNumbers
        {
            get => cachedSave.ShowDamageNumbers;
            set => cachedSave.ShowDamageNumbers = value;  // Auto-saves
        }

        /// <summary>
        /// Should FPS counter be displayed?
        /// </summary>
        public bool ShowFPS
        {
            get => cachedSave.ShowFPS;
            set => cachedSave.ShowFPS = value;  // Auto-saves
        }

        /// <summary>
        /// SFX volume (0-1 range).
        /// </summary>
        public float SFXVolume
        {
            get
            {
                float volume = cachedSave.SFXVolume;
                // If -1, use default
                if (volume < 0f)
                    return audioConfig.DefaultSFXVolume;
                return volume;
            }
            set => cachedSave.SFXVolume = Mathf.Clamp01(value);  // Auto-saves
        }

        /// <summary>
        /// Music volume (0-1 range).
        /// </summary>
        public float MusicVolume
        {
            get
            {
                float volume = cachedSave.MusicVolume;
                // If -1, use default
                if (volume < 0f)
                    return audioConfig.DefaultMusicVolume;
                return volume;
            }
            set => cachedSave.MusicVolume = Mathf.Clamp01(value);  // Auto-saves
        }

        /// <summary>
        /// Current language ("en" or "ru").
        /// </summary>
        public string Language
        {
            get => cachedSave.Language;
            set
            {
                if (cachedSave.Language != value)
                {
                    cachedSave.Language = value;  // Auto-saves
                    LocalizationManager.SetLanguage(value);  // Notify UI to refresh
                }
            }
        }

        /// <summary>
        /// Constructor - uses centralized cache service.
        /// </summary>
        public GameSettings(CachedSaveDataService cachedSave, IAudioConfig audioConfig = null)
        {
            this.cachedSave =cachedSave;
            this.audioConfig = audioConfig ?? new AudioManagerConfig();
        }

        /// <summary>
        /// Reset all settings to defaults.
        /// </summary>
        public void ResetToDefaults()
        {
            cachedSave.ShowDamageNumbers = true;
            cachedSave.ShowFPS = false;
            cachedSave.SFXVolume = -1f; // Use AudioManager default
            cachedSave.MusicVolume = -1f; // Use AudioManager default
            cachedSave.Language = "en";
        }

        /// <summary>
        /// Reload settings from storage (useful after external changes).
        /// </summary>
        public void Reload()
        {
            cachedSave.Reload();
        }
    }
}
