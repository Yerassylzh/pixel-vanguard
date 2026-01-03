using UnityEngine;
using PixelVanguard.Services;

namespace PixelVanguard.Core
{
    /// <summary>
    /// Global settings manager for gameplay preferences.
    /// Uses platform-adaptive ISaveService for persistence.
    /// </summary>
    public class GameSettings
    {
        private readonly ISaveService saveService;
        private SaveData cachedSaveData;

        /// <summary>
        /// Should damage numbers be displayed?
        /// </summary>
        public bool ShowDamageNumbers
        {
            get => cachedSaveData?.showDamageNumbers ?? true;
            set
            {
                if (cachedSaveData != null)
                {
                    cachedSaveData.showDamageNumbers = value;
                    SaveSettings();
                }
            }
        }

        /// <summary>
        /// Should FPS counter be displayed?
        /// </summary>
        public bool ShowFPS
        {
            get => cachedSaveData?.showFPS ?? false;
            set
            {
                if (cachedSaveData != null)
                {
                    cachedSaveData.showFPS = value;
                    SaveSettings();
                }
            }
        }

        /// <summary>
        /// SFX volume (0-1 range).
        /// </summary>
        public float SFXVolume
        {
            get
            {
                if (cachedSaveData == null) return AudioManager.Instance?.DefaultSFXVolume ?? 0.67f;
                
                // If -1, use AudioManager default
                if (cachedSaveData.sfxVolume < 0f)
                    return AudioManager.Instance?.DefaultSFXVolume ?? 0.67f;
                
                return cachedSaveData.sfxVolume;
            }
            set
            {
                if (cachedSaveData != null)
                {
                    cachedSaveData.sfxVolume = Mathf.Clamp01(value);
                    SaveSettings();
                }
            }
        }

        /// <summary>
        /// Music volume (0-1 range).
        /// </summary>
        public float MusicVolume
        {
            get
            {
                if (cachedSaveData == null) return AudioManager.Instance?.DefaultMusicVolume ?? 0.67f;
                
                // If -1, use AudioManager default
                if (cachedSaveData.musicVolume < 0f)
                    return AudioManager.Instance?.DefaultMusicVolume ?? 0.67f;
                
                return cachedSaveData.musicVolume;
            }
            set
            {
                if (cachedSaveData != null)
                {
                    cachedSaveData.musicVolume = Mathf.Clamp01(value);
                    SaveSettings();
                }
            }
        }

        /// <summary>
        /// Current language ("en" or "ru").
        /// </summary>
        public string Language
        {
            get => cachedSaveData?.language ?? "en";
            set
            {
                if (cachedSaveData != null)
                {
                    cachedSaveData.language = value;
                    SaveSettings();
                }
            }
        }

        /// <summary>
        /// Constructor - loads settings from ISaveService.
        /// </summary>
        public GameSettings(ISaveService saveService)
        {
            this.saveService = saveService;
            LoadSettings();
        }

        /// <summary>
        /// Load settings from save data.
        /// </summary>
        private void LoadSettings()
        {
            cachedSaveData = saveService.LoadData();
            
            if (cachedSaveData == null)
            {
                Debug.LogWarning("[GameSettings] No save data found, creating default");
                cachedSaveData = SaveData.CreateDefault();
                SaveSettings();
            }
        }

        /// <summary>
        /// Save settings to persistent storage.
        /// </summary>
        private void SaveSettings()
        {
            if (cachedSaveData != null)
            {
                saveService.SaveData(cachedSaveData);
                cachedSaveData = saveService.LoadData();
            }
        }

        /// <summary>
        /// Reset all settings to defaults.
        /// </summary>
        public void ResetToDefaults()
        {
            if (cachedSaveData == null) return;

            cachedSaveData.showDamageNumbers = true;
            cachedSaveData.showFPS = false;
            cachedSaveData.sfxVolume = -1f; // Use AudioManager default
            cachedSaveData.musicVolume = -1f; // Use AudioManager default
            cachedSaveData.language = "en";
            
            SaveSettings();
        }

        /// <summary>
        /// Reload settings from storage (useful after external changes).
        /// </summary>
        public void Reload()
        {
            LoadSettings();
        }
    }
}
