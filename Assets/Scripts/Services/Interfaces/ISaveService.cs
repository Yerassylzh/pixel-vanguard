using System.Threading.Tasks;

namespace PixelVanguard.Services
{
    /// <summary>
    /// Platform-agnostic interface for save data persistence.
    /// All operations are synchronous since both PlayerPrefs and Yandex saves are immediate.
    /// Implementations: PlayerPrefsSaveService (Android/Desktop), YandexSaveService (Yandex WebGL)
    /// </summary>
    public interface ISaveService
    {
        /// <summary>
        /// Initialize the save service. Call during Bootstrap.
        /// </summary>
        void Initialize();

        /// <summary>
        /// Load player save data. Returns default data if no save exists.
        /// </summary>
        SaveData LoadData();

        /// <summary>
        /// Save player data. Handles both local and cloud saves.
        /// </summary>
        void SaveData(SaveData data);

        /// <summary>
        /// Check if cloud save is available (Yandex: true, PlayerPrefs: false).
        /// </summary>
        bool IsCloudSaveAvailable();
    }
}
