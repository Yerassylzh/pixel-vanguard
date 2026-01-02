#if UNITY_WEBGL
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using YG;

namespace PixelVanguard.Services
{
    /// <summary>
    /// Save service implementation for Yandex Games (WebGL).
    /// Synchronizes between SaveData (local data model) and SavesYG (Yandex cloud storage).
    /// </summary>
    public class YandexSaveService : ISaveService
    {
        public void Initialize()
        {
            // YG2.saves is automatically loaded by PluginYG on game start
            // No explicit initialization needed
            Debug.Log("[YandexSaveService] Initialized - using Yandex cloud storage");
        }

        public bool IsCloudSaveAvailable()
        {
            // Yandex cloud saves are always available when using PluginYG
            return true;
        }

        public SaveData LoadData()
        {
            // YG2.saves is automatically loaded by PluginYG on game start
            return ConvertYandexToSaveData(YG2.saves);
        }

        public void SaveData(SaveData data)
        {
            SyncSaveDataToYandex(data);
            YG2.SaveProgress(); // Send to Yandex cloud (synchronous)
        }

        /// <summary>
        /// Convert Yandex SavesYG to local SaveData model.
        /// </summary>
        private SaveData ConvertYandexToSaveData(SavesYG yandexData)
        {
            var saveData = new SaveData
            {
                totalGold = yandexData.totalGold,
                unlockedCharacterIDs = new List<string>(yandexData.unlockedCharacterIDs ?? new List<string>()),
                selectedCharacterID = yandexData.selectedCharacterID ?? "knight",
                adsWatchedForPack1 = yandexData.adsWatchedForPack1,
                adsWatchedForPack2 = yandexData.adsWatchedForPack2,
                lastAdWatchedTime = yandexData.lastAdWatchedTime ?? "",
                
                // High Scores - map to SaveData field names
                longestSurvivalTime = (int)yandexData.longestTime,
                highestKillCount = yandexData.highestKills,
                highestLevelReached = yandexData.highestLevel,
                mostGoldInRun = yandexData.mostGold,
                
                // Monetization
                adsRemoved = yandexData.adsRemoved
            };

            // Convert Dictionary to SaveData's List format
            if (yandexData.statLevels != null)
            {
                foreach (var kvp in yandexData.statLevels)
                {
                    saveData.SetStatLevel(kvp.Key, kvp.Value);
                }
            }

            // Ensure all required stats exist
            string[] statNames = { "vitality", "might", "greaves", "magnet" };
            foreach (string statName in statNames)
            {
                if (saveData.GetStatLevel(statName) == 0 && !yandexData.statLevels.ContainsKey(statName))
                {
                    saveData.SetStatLevel(statName, 0);
                }
            }

            // Ensure default characters are unlocked
            if (saveData.unlockedCharacterIDs == null || saveData.unlockedCharacterIDs.Count == 0)
            {
                saveData.unlockedCharacterIDs = new List<string> { "knight", "pyromancer" };
            }

            return saveData;
        }

        /// <summary>
        /// Sync local SaveData to Yandex SavesYG.
        /// </summary>
        private void SyncSaveDataToYandex(SaveData data)
        {
            YG2.saves.totalGold = data.totalGold;
            YG2.saves.unlockedCharacterIDs = new List<string>(data.unlockedCharacterIDs ?? new List<string>());
            YG2.saves.selectedCharacterID = data.selectedCharacterID ?? "knight";
            YG2.saves.adsWatchedForPack1 = data.adsWatchedForPack1;
            YG2.saves.adsWatchedForPack2 = data.adsWatchedForPack2;
            YG2.saves.lastAdWatchedTime = data.lastAdWatchedTime ?? "";
            
            // Convert SaveData's List format to Dictionary
            YG2.saves.statLevels = new Dictionary<string, int>();
            if (data.statLevelKeys != null)
            {
                for (int i = 0; i < data.statLevelKeys.Count; i++)
                {
                    YG2.saves.statLevels[data.statLevelKeys[i]] = data.statLevelValues[i];
                }
            }
            
            // High Scores - map from SaveData field names
            YG2.saves.longestTime = data.longestSurvivalTime;
            YG2.saves.highestKills = data.highestKillCount;
            YG2.saves.highestLevel = data.highestLevelReached;
            YG2.saves.mostGold = data.mostGoldInRun;
            YG2.saves.adsRemoved = data.adsRemoved;
        }
    }
}
#endif
