using UnityEngine;
using PixelVanguard.Data;

namespace PixelVanguard.Services
{
    /// <summary>
    /// Centralized cached save data manager.
    /// Provides a single source of truth for SaveData with automatic save-on-write.
    /// All systems should use this instead of managing their own cache.
    /// </summary>
    public class CachedSaveDataService
    {
        private readonly ISaveService saveService;
        private SaveData cache;

        /// <summary>
        /// Get the current save data. Loads if not cached.
        /// READONLY - do not modify directly. Use property setters instead.
        /// </summary>
        public SaveData Data
        {
            get
            {
                if (cache == null)
                {
                    cache = saveService.LoadData();
                    if (cache == null)
                    {
                        Debug.LogWarning("[CachedSaveDataService] No save data found, creating default");
                        cache = SaveData.CreateDefault();
                        Save();
                    }
                }
                return cache;
            }
        }

        public CachedSaveDataService(ISaveService saveService)
        {
            this.saveService = saveService;
        }

        /// <summary>
        /// Save current cache to persistent storage and refresh.
        /// Call this only when modifying Data directly (rare).
        /// Property setters call this automatically.
        /// </summary>
        public void Save()
        {
            if (cache != null)
            {
                saveService.SaveData(cache);
                cache = saveService.LoadData(); // Refresh to ensure consistency
            }
        }

        /// <summary>
        /// Force reload from storage. Useful after external changes.
        /// </summary>
        public void Reload()
        {
            cache = null; // Clear cache, will reload on next Data access
        }

        // ============================================
        // AUTO-SAVE PROPERTIES
        // ============================================
        // These properties automatically save when modified

        public string Language
        {
            get => Data.language;
            set
            {
                if (Data.language != value)
                {
                    Data.language = value;
                    Save();
                }
            }
        }

        public float MusicVolume
        {
            get => Data.musicVolume;
            set
            {
                Data.musicVolume = Mathf.Clamp01(value);
                Save();
            }
        }

        public float SFXVolume
        {
            get => Data.sfxVolume;
            set
            {
                Data.sfxVolume = Mathf.Clamp01(value);
                Save();
            }
        }

        public bool ShowDamageNumbers
        {
            get => Data.showDamageNumbers;
            set
            {
                Data.showDamageNumbers = value;
                Save();
            }
        }

        public bool ShowFPS
        {
            get => Data.showFPS;
            set
            {
                Data.showFPS = value;
                Save();
            }
        }

        public int TotalGold
        {
            get => Data.totalGold;
            set
            {
                Data.totalGold = value;
                Save();
            }
        }

        /// <summary>
        /// Add gold (convenience method with auto-save).
        /// </summary>
        public void AddGold(int amount)
        {
            Data.totalGold += amount;
            Save();
        }

        /// <summary>
        /// Spend gold. Returns true if successful, false if insufficient funds.
        /// </summary>
        public bool SpendGold(int amount)
        {
            if (Data.totalGold >= amount)
            {
                Data.totalGold -= amount;
                Save();
                return true;
            }
            return false;
        }
    }
}
