using System;
using System.Collections.Generic;
using UnityEngine;

namespace PixelVanguard.Services
{
    /// <summary>
    /// Player save data structure. Serialized to JSON for persistence.
    /// </summary>
    [Serializable]
    public class SaveData
    {
        public int version = 1;
        public int totalGold;
        public List<string> unlockedCharacterIDs = new List<string>();
        public string selectedCharacterID = "knight"; // Default starter
        public Dictionary<string, int> statLevels = new Dictionary<string, int>();
        public Dictionary<string, int> adWatchProgress = new Dictionary<string, int>();

        /// <summary>
        /// Create a default save for new players.
        /// </summary>
        public static SaveData CreateDefault()
        {
            var data = new SaveData
            {
                totalGold = 0,
                selectedCharacterID = "knight"
            };

            // Knight is always unlocked by default
            data.unlockedCharacterIDs.Add("knight");

            // Initialize stat levels to 0
            data.statLevels["might"] = 0;
            data.statLevels["vitality"] = 0;
            data.statLevels["greaves"] = 0;
            data.statLevels["magnet"] = 0;
            data.statLevels["luck"] = 0;

            return data;
        }

        /// <summary>
        /// Validate and migrate old save data if needed.
        /// </summary>
        public void Validate()
        {
            // Migration logic for future version updates
            if (version < 1)
            {
                // Migrate from version 0 to 1
                Debug.Log("[SaveData] Migrating from version 0 to 1");
                version = 1;
            }

            // Ensure knight is always unlocked
            if (!unlockedCharacterIDs.Contains("knight"))
            {
                unlockedCharacterIDs.Add("knight");
            }

            // Ensure selected character is unlocked
            if (!unlockedCharacterIDs.Contains(selectedCharacterID))
            {
                selectedCharacterID = "knight";
            }
        }
    }
}
