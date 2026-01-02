using System.Threading.Tasks;
using UnityEngine;
using SaveDataType = PixelVanguard.Services.SaveData;

namespace PixelVanguard.Services
{
    /// <summary>
    /// Local save service using Unity PlayerPrefs.
    /// Use for: Desktop, Android, iOS builds.
    /// Data stored on device only.
    /// </summary>
    public class PlayerPrefsSaveService : ISaveService
    {
        private const string SAVE_KEY = "PixelVanguard_SaveData";
        private SaveDataType cachedData;

        public void Initialize()
        {
        }

        public SaveDataType LoadData()
        {
            // IMPORTANT: Always load from disk to get latest changes
            // Don't use cache - it causes stale data when switching between scenes
            
            if (PlayerPrefs.HasKey(SAVE_KEY))
            {
                string json = PlayerPrefs.GetString(SAVE_KEY);
                try
                {
                    cachedData = JsonUtility.FromJson<SaveDataType>(json);
                    cachedData.Validate();
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"[PlayerPrefsSaveService] Failed to parse save data: {e.Message}");
                    cachedData = SaveDataType.CreateDefault();
                }
            }
            else
            {
                cachedData = SaveDataType.CreateDefault();
            }

            return cachedData;
        }

        public void SaveData(SaveDataType data)
        {
            if (data == null)
            {
                Debug.LogError("[PlayerPrefsSaveService] Attempted to save null data!");
                return;
            }

            cachedData = data;
            string json = JsonUtility.ToJson(data, true);
            PlayerPrefs.SetString(SAVE_KEY, json);
            PlayerPrefs.Save();
        }

        public bool IsCloudSaveAvailable()
        {
            // PlayerPrefs is local only
            return false;
        }
    }
}
