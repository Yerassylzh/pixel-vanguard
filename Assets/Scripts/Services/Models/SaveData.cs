using System;
using System.Collections.Generic;
using UnityEngine;

namespace PixelVanguard.Services
{
    /// <summary>
    /// Player save data structure. Serialized to JSON for persistence.
    /// Uses Lists instead of Dictionaries for Unity JsonUtility compatibility.
    /// </summary>
    [Serializable]
    public class SaveData
    {
        public int version = 1;
        public int totalGold;
        public List<string> unlockedCharacterIDs = new List<string>();
        public string selectedCharacterID = "knight"; // Default starter
        
        // Unity JsonUtility doesn't support Dictionary, use Lists
        public List<string> statLevelKeys = new List<string>();
        public List<int> statLevelValues = new List<int>();

        // High Scores (for leaderboards and main menu display)
        public int longestSurvivalTime = 0;   // Seconds
        public int highestKillCount = 0;
        public int highestLevelReached = 0;
        public int mostGoldInRun = 0;

        // Ad Tracking (for monetization)
        public int adsWatchedForPack1 = 0;  // 5 ads → 1990 gold
        public int adsWatchedForPack2 = 0;  // 10 ads → 4990 gold
        public string lastAdWatchedTime = "";  // ISO 8601 timestamp
        public bool adsRemoved = false;  // IAP: Remove Ads purchase

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

            // Free starter characters (always unlocked by default)
            data.unlockedCharacterIDs.Add("knight");
            data.unlockedCharacterIDs.Add("pyromancer");

            // Initialize shop permanent upgrade levels to 0
            data.SetStatLevel("vitality", 0);  // Max HP bonus
            data.SetStatLevel("might", 0);     // Damage bonus
            data.SetStatLevel("greaves", 0);   // Move speed bonus
            data.SetStatLevel("magnet", 0);    // Collection radius bonus
            // Note: "luck" removed - was redundant with Lucky Coins passive upgrade

            return data;
        }

        /// <summary>
        /// Get stat level by key.
        /// </summary>
        public int GetStatLevel(string key)
        {
            int index = statLevelKeys.IndexOf(key);
            return index >= 0 ? statLevelValues[index] : 0;
        }

        /// <summary>
        /// Set stat level by key.
        /// </summary>
        public void SetStatLevel(string key, int value)
        {
            int index = statLevelKeys.IndexOf(key);
            if (index >= 0)
            {
                statLevelValues[index] = value;
            }
            else
            {
                statLevelKeys.Add(key);
                statLevelValues.Add(value);
            }
        }

        /// <summary>
        /// Update high scores if current session beats them.
        /// Returns true if any new record was set.
        /// </summary>
        public bool UpdateHighScores(int survivalTime, int killCount, int levelReached, int goldInRun)
        {
            bool newRecord = false;

            if (survivalTime > longestSurvivalTime)
            {
                longestSurvivalTime = survivalTime;
                newRecord = true;
            }

            if (killCount > highestKillCount)
            {
                highestKillCount = killCount;
                newRecord = true;
            }

            if (levelReached > highestLevelReached)
            {
                highestLevelReached = levelReached;
                newRecord = true;
            }

            if (goldInRun > mostGoldInRun)
            {
                mostGoldInRun = goldInRun;
                newRecord = true;
            }

            return newRecord;
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
                version = 1;
            }

            // Ensure knight is always unlocked
            if (!unlockedCharacterIDs.Contains("knight"))
            {
                unlockedCharacterIDs.Add("knight");
            }

            // Ensure pyromancer is always unlocked (free starter)
            if (!unlockedCharacterIDs.Contains("pyromancer"))
            {
                unlockedCharacterIDs.Add("pyromancer");
            }

            // Ensure selected character is unlocked
            if (!unlockedCharacterIDs.Contains(selectedCharacterID))
            {
                selectedCharacterID = "knight";
            }
        }

        /// <summary>
        /// Check if a character is unlocked.
        /// </summary>
        public bool IsCharacterUnlocked(string characterId)
        {
            return unlockedCharacterIDs.Contains(characterId.ToLower());
        }

        /// <summary>
        /// Unlock a character (for purchase system).
        /// </summary>
        public void UnlockCharacter(string characterId)
        {
            string id = characterId.ToLower();
            if (!unlockedCharacterIDs.Contains(id))
            {
                unlockedCharacterIDs.Add(id);
            }
        }
    }
}
