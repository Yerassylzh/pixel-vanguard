using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;
using PixelVanguard.Core;
using PixelVanguard.Gameplay;
using PixelVanguard.Services;

namespace PixelVanguard.UI
{
    /// <summary>
    /// Game Over screen displayed when player dies.
    /// Offers revive (with ad) or quit to stats screen.
    /// </summary>
    public class GameOverScreen : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject gameOverPanel;
        [SerializeField] private Button reviveButton;
        [SerializeField] private Button quitButton;

        private void Start()
        {
            // Hide initially
            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(false);
            }

            // Setup button listeners
            if (reviveButton != null) reviveButton.onClick.AddListener(OnReviveClicked);
            if (quitButton != null) quitButton.onClick.AddListener(OnQuitClicked);
        }

        private void OnEnable()
        {
            GameEvents.OnGameOver += OnGameOver;
        }

        private void OnDisable()
        {
            GameEvents.OnGameOver -= OnGameOver;
        }

        private void OnGameOver(GameOverReason reason)
        {
            ShowGameOver();
        }

        private void ShowGameOver()
        {
            // Show panel
            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(true);
            }

        }

        private async void OnReviveClicked()
        {

            var adService = ServiceLocator.Get<Services.IAdService>();
            if (adService == null)
            {
                Debug.LogError("[GameOverScreen] AdService not found! Cannot show revive ad.");
                return;
            }

            // Show rewarded ad
            bool success = await adService.ShowRewardedAd();

            if (success)
            {
                
                // CRITICAL: Delay revive to next frame to allow PluginYG Focus Pause to fully restore
                StartCoroutine(RevivePlayerNextFrame());
            }
            else
            {
                Debug.LogWarning("[GameOverScreen] Ad failed or was cancelled - player not revived");
            }
        }
        
        private IEnumerator RevivePlayerNextFrame()
        {
            // CRITICAL WORKAROUND: PluginYG Focus Pause Issue
            // PluginYG pauses Unity's entire game loop when showing ads (not just Time.timeScale).
            // Even after ad callback fires, Unity remains paused until next frame.
            // This delay ensures Unity's game loop is fully restored before RevivePlayer() executes.
            // See: https://max-games.ru/plugin-yg/doc/reward-ad/?en (Focus Pause section)
            yield return new WaitForEndOfFrame();
            
            RevivePlayer();
        }

        private void RevivePlayer()
        {
            // Hide panel AFTER game state is restored
            gameOverPanel.SetActive(false);
            
            // Tell GameManager to revive player FIRST (it restores timeScale and state)
            GameManager.Instance.RevivePlayer();   
        }

        private void OnQuitClicked()
        {
            // Hide this panel
            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(false);
            }

            // Restore time scale for scene transition
            Time.timeScale = 1f;

            // YANDEX REQUIREMENT: Show interstitial ad BEFORE scene loads (no frame delays)
            ShowInterstitialAdBeforeResults();

            // Load Results Scene
            SceneManager.LoadScene("ResultsScene");
        }

        /// <summary>
        /// Show interstitial ad immediately before loading results scene.
        /// Called synchronously to avoid frame delays per Yandex requirements.
        /// </summary>
        private void ShowInterstitialAdBeforeResults()
        {
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
        }
    }
}
