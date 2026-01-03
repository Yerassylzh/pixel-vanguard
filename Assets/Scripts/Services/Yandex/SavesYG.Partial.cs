#if UNITY_WEBGL
// This file defines the SavesYG partial class for Yandex Games cloud saves.
// It mirrors the SaveData structure for synchronization between local data model and Yandex cloud.
using System.Collections.Generic;

namespace YG
{
    /// <summary>
    /// Yandex Games cloud save data structure.
    /// This partial class extends SavesYG from PluginYG.
    /// All fields here will be saved to Yandex cloud storage.
    /// </summary>
    public partial class SavesYG
    {
        // === Stats ===
        public int totalGold = 0;
        public Dictionary<string, int> statLevels = new Dictionary<string, int>
        {
            { "vitality", 0 },
            { "might", 0 },
            { "greaves", 0 },
            { "magnet", 0 }
        };

        // === Characters ===
        public List<string> unlockedCharacterIDs = new List<string> { "knight", "pyromancer" };
        public string selectedCharacterID = "knight";

        // === Ad Packs ===
        public int adsWatchedForPack1 = 0;
        public int adsWatchedForPack2 = 0;
        public string lastAdWatchedTime = "";

        // === High Scores ===
        public float longestTime = 0f;
        public int highestKills = 0;
        public int highestLevel = 0;
        public int mostGold = 0;

        // === Monetization ===
        public bool adsRemoved = false;  // IAP Remove Ads

        // Game Settings (platform-adaptive)
        public bool showDamageNumbers = true;
        public bool showFPS = false;
        public float sfxVolume = -1f;  // -1 = use AudioManager default
        public float musicVolume = -1f; // -1 = use AudioManager default
        public string language = "en";
    }
}
#endif
