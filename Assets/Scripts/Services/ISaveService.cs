using System.Threading.Tasks;

namespace PixelVanguard.Services
{
    /// <summary>
    /// Platform-agnostic interface for save data persistence.
    /// Implementations: PlayerPrefsSaveService (Android/Desktop), YandexCloudSaveService (Yandex Web)
    /// </summary>
    public interface ISaveService
    {
        /// <summary>
        /// Load player save data. Returns default data if no save exists.
        /// </summary>
        Task<SaveData> LoadData();

        /// <summary>
        /// Save player data. Should handle both local and cloud saves if available.
        /// </summary>
        Task SaveData(SaveData data);

        /// <summary>
        /// Check if cloud save is available (Yandex only).
        /// </summary>
        bool IsCloudSaveAvailable();

        /// <summary>
        /// Initialize the save service. Call during Bootstrap.
        /// </summary>
        Task Initialize();
    }
}
