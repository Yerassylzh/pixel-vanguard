using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using PixelVanguard.Core;
using PixelVanguard.Gameplay;
using PixelVanguard.Services;
using System.Threading.Tasks;

namespace PixelVanguard.UI
{
    /// <summary>
    /// Controls the Results Scene (Game Over screen).
    /// Displays session stats, handles ad for gold doubling, and saves progress.
    /// Supports shadow text via arrays (Index 0 = Primary, Index 1 = Shadow).
    /// </summary>
    public class ResultsController : MonoBehaviour
    {
        [Header("UI References - Title")]
        [Tooltip("Index 0 = Primary text, Index 1 = Shadow")]
        [SerializeField] private TextMeshProUGUI[] titleTexts; // Array for primary + shadow

        [Header("UI References - Stats Panel")]
        [Tooltip("Index 0 = Primary text, Index 1 = Shadow")]
        [SerializeField] private TextMeshProUGUI[] timeTexts;
        [Tooltip("Index 0 = Primary text, Index 1 = Shadow")]
        [SerializeField] private TextMeshProUGUI[] killsTexts;
        [Tooltip("Index 0 = Primary text, Index 1 = Shadow")]
        [SerializeField] private TextMeshProUGUI[] levelTexts;
        [Tooltip("Index 0 = Primary text, Index 1 = Shadow")]
        [SerializeField] private TextMeshProUGUI[] goldTexts;

        [Header("UI References - Buttons")]
        [SerializeField] private Button watchAdButton;
        [Tooltip("Index 0 = Primary text, Index 1 = Shadow")]
        [SerializeField] private TextMeshProUGUI[] watchAdButtonTexts; // Array for primary + shadow
        [SerializeField] private Button mainMenuButton;

        [Header("UI References - New Record Badge (Optional)")]
        [SerializeField] private GameObject newRecordBadge;

        [Header("Colors")]
        [SerializeField] private Color victoryColor = new Color(1f, 0.84f, 0f, 0.835f); // Gold (255, 215, 0, 213)
        [SerializeField] private Color defeatColor = new Color(0.54f, 0f, 0f, 0.835f);  // Dark red (138, 0, 0, 213)

        private int baseGold;
        private bool goldDoubled = false;
        private bool dataSaved = false;

        private void Start()
        {
            // Ensure ServiceLocator is initialized (fallback for testing scene directly)
            if (ServiceLocator.Get<ISaveService>() == null)
            {
                Debug.LogWarning("[ResultsController] ServiceLocator not initialized, initializing now...");
                var saveService = new PlayerPrefsSaveService();
                ServiceLocator.Register<ISaveService>(saveService);
            }

            // Restore time scale (in case game was paused)
            Time.timeScale = 1f;

            // Display session stats
            DisplayStats();

            // Show interstitial ad at end of game
            ShowInterstitialAd();

            // Setup button listeners
            SetupButtons();
        }

        /// <summary>
        /// Show interstitial ad when results screen opens (end of game).
        /// Skip if ads have been removed via IAP.
        /// </summary>
        private void ShowInterstitialAd()
        {
            // Check if ads have been removed
            var saveService = ServiceLocator.Get<ISaveService>();
            if (saveService != null)
            {
                var saveData = saveService.LoadData();
                if (saveData.adsRemoved)
                {
                    return;
                }
            }

            var adService = ServiceLocator.Get<IAdService>();
            if (adService != null)
            {
                adService.ShowInterstitialAd();
            }
            else
            {
                Debug.LogWarning("[ResultsController] AdService not found - skipping interstitial ad");
            }
        }

        private void DisplayStats()
        {
            // Ensure SessionData exists
            if (SessionData.Instance == null)
            {
                Debug.LogError("[ResultsController] SessionData not found! Cannot display stats.");
                SetDefaultStats();
                return;
            }

            var session = SessionData.Instance;

            // Set title and icon based on game over reason
            bool isVictory = session.gameOverReason == GameOverReason.Victory;

            SetTextArray(titleTexts, isVictory ? 
                Core.LocalizationManager.Get("ui.results.victory") : 
                Core.LocalizationManager.Get("ui.results.defeated"));
            SetColorArray(titleTexts, isVictory ? victoryColor : defeatColor);

            // Display session stats
            SetTextArray(timeTexts, $"{session.GetFormattedTime()}");
            SetTextArray(killsTexts, $"{session.killCount}");
            SetTextArray(levelTexts, $"{session.levelReached}");

            baseGold = session.goldCollected;
            SetTextArray(goldTexts, $"{baseGold}");

            // Update ad button text
            int doubledAmount = baseGold * 2;
            string adButtonText = Core.LocalizationManager.GetFormatted("ui.results.watch_ad_bonus", doubledAmount.ToString("N0"));
            SetTextArray(watchAdButtonTexts, adButtonText);

            // Check for new records
            CheckNewRecords();
        }

        private void CheckNewRecords()
        {
            // Load save data to check high scores
            var saveService = ServiceLocator.Get<ISaveService>();
            if (saveService == null)
            {
                Debug.LogWarning("[ResultsController] ISaveService not found, skipping high score check");
                return;
            }

            var saveData = saveService.LoadData();

            int survivalTime = Mathf.FloorToInt(SessionData.Instance.survivalTime);
            bool isNewRecord = saveData.UpdateHighScores(
                survivalTime,
                SessionData.Instance.killCount,
                SessionData.Instance.levelReached,
                baseGold
            );

            // Show "NEW RECORD!" badge if applicable
            if (isNewRecord && newRecordBadge != null)
            {
                newRecordBadge.SetActive(true);
            }
        }

        private void SetDefaultStats()
        {
            SetTextArray(titleTexts, "GAME OVER");
            SetColorArray(titleTexts, defeatColor);
            SetTextArray(timeTexts, "00:00");
            SetTextArray(killsTexts, "0");
            SetTextArray(levelTexts, "1");
            SetTextArray(goldTexts, "0");
            baseGold = 0;
        }

        private void SetupButtons()
        {
            if (watchAdButton != null)
            {
                watchAdButton.onClick.AddListener(OnWatchAdClicked);
            }

            if (mainMenuButton != null)
            {
                mainMenuButton.onClick.AddListener(OnMainMenuClicked);
            }
        }

        private async void OnWatchAdClicked()
        {
            var adService = ServiceLocator.Get<IAdService>();
            if (adService == null)
            {
                Debug.LogError("[ResultsController] AdService not found!");
                return;
            }

            // Show rewarded ad
            bool success = await adService.ShowRewardedAd();

            if (success)
            {
                // Grant doubled gold
                DoubleGold();
            }
            else
            {
                Debug.LogWarning("[ResultsController] Ad failed or was cancelled");
            }
        }

        private void DoubleGold()
        {
            if (goldDoubled)
            {
                Debug.LogWarning("[ResultsController] Gold already doubled!");
                return;
            }

            goldDoubled = true;
            int finalGold = baseGold * 2;

            // Update display with visual feedback
            SetTextArray(goldTexts, $"{finalGold}");

            // Disable ad button and update text
            if (watchAdButton != null)
            {
                watchAdButton.interactable = false;
                SetTextArray(watchAdButtonTexts, Core.LocalizationManager.Get("ui.notification.gold_doubled"));
            }

            // Auto-save with doubled gold
            SaveProgress(finalGold);
        }

        private void SaveProgress(int goldAmount)
        {
            if (dataSaved)
            {
                Debug.LogWarning("[ResultsController] Data already saved!");
                return;
            }

            var saveService = ServiceLocator.Get<ISaveService>();
            if (saveService == null)
            {
                Debug.LogError("[ResultsController] ISaveService not found! Cannot save progress.");
                return;
            }

            // Load current save data
            var saveData = saveService.LoadData();

            // Add gold
            int oldGold = saveData.totalGold;
            saveData.totalGold += goldAmount;

            // Update high scores (already done in CheckNewRecords, but ensures it's saved)
            int survivalTime = Mathf.FloorToInt(SessionData.Instance.survivalTime);
            saveData.UpdateHighScores(
                survivalTime,
                SessionData.Instance.killCount,
                SessionData.Instance.levelReached,
                goldAmount
            );

            // Save to disk
            saveService.SaveData(saveData);

            dataSaved = true;
        }

        private void OnMainMenuClicked()
        {
            // Ensure data is saved before leaving
            if (!dataSaved)
            {
                SaveProgress(goldDoubled ? baseGold * 2 : baseGold);
            }

            ContinueToMenu();
        }

        private void ContinueToMenu()
        {
            // Reset session data
            if (SessionData.Instance != null)
            {
                SessionData.Instance.ResetSession();
            }

            SceneManager.LoadScene("MainMenuScene");
        }

        /// <summary>
        /// Helper: Set text on all elements in array (primary + shadow).
        /// </summary>
        private void SetTextArray(TextMeshProUGUI[] textArray, string value)
        {
            if (textArray == null || textArray.Length == 0) return;

            foreach (var textElement in textArray)
            {
                if (textElement != null)
                {
                    textElement.text = value;
                }
            }
        }

        /// <summary>
        /// Helper: Set color on all elements in array (primary + shadow).
        /// </summary>
        private void SetColorArray(TextMeshProUGUI[] textArray, Color color)
        {
            if (textArray == null || textArray.Length == 0) return;

            foreach (var textElement in textArray)
            {
                if (textElement != null)
                {
                    textElement.color = color;
                }
            }
        }

        private void OnDestroy()
        {
            // Clean up button listeners
            if (watchAdButton != null) watchAdButton.onClick.RemoveAllListeners();
            if (mainMenuButton != null) mainMenuButton.onClick.RemoveAllListeners();
        }
    }
}
