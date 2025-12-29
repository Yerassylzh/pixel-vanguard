using UnityEngine;

namespace PixelVanguard.Core
{
    /// <summary>
    /// Global settings manager for gameplay preferences.
    /// Persists via PlayerPrefs.
    /// </summary>
    public static class GameSettings
    {
        // PlayerPrefs keys
        private const string PREF_SHOW_DAMAGE_NUMBERS = "ShowDamageNumbers";
        private const string PREF_SHOW_FPS = "ShowFPS";
        private const string PREF_MASTER_VOLUME = "MasterVolume";
        private const string PREF_SFX_VOLUME = "SFXVolume";
        private const string PREF_MUSIC_VOLUME = "MusicVolume";
        private const string PREF_LANGUAGE = "Language";

        // Cached values (loaded once at game start)
        private static bool? _showDamageNumbers;
        private static bool? _showFPS;

        /// <summary>
        /// Should damage numbers be displayed?
        /// </summary>
        public static bool ShowDamageNumbers
        {
            get
            {
                if (!_showDamageNumbers.HasValue)
                {
                    _showDamageNumbers = PlayerPrefs.GetInt(PREF_SHOW_DAMAGE_NUMBERS, 1) == 1;
                }
                return _showDamageNumbers.Value;
            }
            set
            {
                _showDamageNumbers = value;
                PlayerPrefs.SetInt(PREF_SHOW_DAMAGE_NUMBERS, value ? 1 : 0);
                PlayerPrefs.Save();
            }
        }

        /// <summary>
        /// Should FPS counter be displayed?
        /// </summary>
        public static bool ShowFPS
        {
            get
            {
                if (!_showFPS.HasValue)
                {
                    _showFPS = PlayerPrefs.GetInt(PREF_SHOW_FPS, 0) == 1;
                }
                return _showFPS.Value;
            }
            set
            {
                _showFPS = value;
                PlayerPrefs.SetInt(PREF_SHOW_FPS, value ? 1 : 0);
                PlayerPrefs.Save();
            }
        }

        /// <summary>
        /// Master volume (0-1 range).
        /// </summary>
        public static float MasterVolume
        {
            get => PlayerPrefs.GetFloat(PREF_MASTER_VOLUME, 1f);
            set
            {
                PlayerPrefs.SetFloat(PREF_MASTER_VOLUME, Mathf.Clamp01(value));
                PlayerPrefs.Save();
            }
        }

        /// <summary>
        /// SFX volume (0-1 range).
        /// </summary>
        public static float SFXVolume
        {
            get => PlayerPrefs.GetFloat(PREF_SFX_VOLUME, 0.4f); // Default: 0.4
            set
            {
                PlayerPrefs.SetFloat(PREF_SFX_VOLUME, Mathf.Clamp01(value));
                PlayerPrefs.Save();
            }
        }

        /// <summary>
        /// Music volume (0-1 range).
        /// </summary>
        public static float MusicVolume
        {
            get => PlayerPrefs.GetFloat(PREF_MUSIC_VOLUME, 0.5f); // Default: 0.5
            set
            {
                PlayerPrefs.SetFloat(PREF_MUSIC_VOLUME, Mathf.Clamp01(value));
                PlayerPrefs.Save();
            }
        }

        /// <summary>
        /// Current language ("en" or "ru").
        /// </summary>
        public static string Language
        {
            get => PlayerPrefs.GetString(PREF_LANGUAGE, "en");
            set
            {
                PlayerPrefs.SetString(PREF_LANGUAGE, value);
                PlayerPrefs.Save();
            }
        }

        /// <summary>
        /// Reset all settings to defaults.
        /// </summary>
        public static void ResetToDefaults()
        {
            ShowDamageNumbers = true;
            ShowFPS = false;
            MasterVolume = 1f;
            SFXVolume = 1f;
            MusicVolume = 0.7f;
            Language = "en";
        }
    }
}
