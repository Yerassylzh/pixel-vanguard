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

        public async Task Initialize()
        {
            Debug.Log("[PlayerPrefsSaveService] Initialized");
            await Task.CompletedTask;
        }

        public async Task<SaveDataType> LoadData()
        {
            if (cachedData != null)
            {
                return cachedData;
            }

            if (PlayerPrefs.HasKey(SAVE_KEY))
            {
                string json = PlayerPrefs.GetString(SAVE_KEY);
                try
                {
                    cachedData = JsonUtility.FromJson<SaveDataType>(json);
                    cachedData.Validate();
                    Debug.Log("[PlayerPrefsSaveService] Loaded save data");
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"[PlayerPrefsSaveService] Failed to parse save data: {e.Message}");
                    cachedData = SaveDataType.CreateDefault();
                }
            }
            else
            {
                Debug.Log("[PlayerPrefsSaveService] No save found, creating default");
                cachedData = SaveDataType.CreateDefault();
            }

            await Task.CompletedTask;
            return cachedData;
        }

        public async Task SaveData(SaveDataType data)
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

            Debug.Log("[PlayerPrefsSaveService] Save data written to PlayerPrefs");
            await Task.CompletedTask;
        }

        public bool IsCloudSaveAvailable()
        {
            // PlayerPrefs is local only
            return false;
        }

        // Explicit interface implementation to match ISaveService
        async Task<SaveData> ISaveService.LoadData() => await LoadData();
        async Task ISaveService.SaveData(SaveData data) => await SaveData(data);
    }
}
