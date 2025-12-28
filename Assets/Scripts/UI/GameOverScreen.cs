using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using PixelVanguard.Core;
using PixelVanguard.Gameplay;

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

            Debug.Log("[GameOverScreen] Game Over - Revive or Quit?");
        }

        private void OnReviveClicked()
        {
            Debug.Log("[GameOverScreen] Revive clicked");

            // TODO: Watch ad (placeholder for now)
            // AdManager.ShowRewardedAd(() => { RevivePlayer(); });

            // For now, revive immediately
            RevivePlayer();
        }

        private void RevivePlayer()
        {
            Debug.Log("[GameOverScreen] Player revived!");

            // Hide panel
            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(false);
            }

            // Tell GameManager to revive player
            if (GameManager.Instance != null)
            {
                GameManager.Instance.RevivePlayer();
            }
        }

        private void OnQuitClicked()
        {
            Debug.Log("[GameOverScreen] Quit clicked - loading stats scene");

            // Hide this panel
            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(false);
            }

            // TODO: Load dedicated game over stats scene
            // For now, log stats and return to main menu
            if (GameManager.Instance != null && GameManager.Instance.CurrentSession != null)
            {
                var session = GameManager.Instance.CurrentSession;
                Debug.Log($"[GameOverScreen] Stats - Time: {session.GetFormattedTime()}, Kills: {session.killCount}, Level: {session.levelReached}, Gold: {session.goldCollected}");
            }

            // Restore time scale
            Time.timeScale = 1f;

            // TODO: Replace with actual stats scene loading
            // SceneManager.LoadScene("GameOverStatsScene");
            
            // For now, reload game scene
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}
