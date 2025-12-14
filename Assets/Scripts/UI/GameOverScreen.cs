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
    /// Shows session stats and provides restart option.
    /// </summary>
    public class GameOverScreen : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject gameOverPanel;
        [SerializeField] private TextMeshProUGUI survivalTimeText;
        [SerializeField] private TextMeshProUGUI killCountText;
        [SerializeField] private TextMeshProUGUI levelText;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button mainMenuButton;

        private void Start()
        {
            // Hide initially
            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(false);
            }

            // Setup button listeners
            if (restartButton != null) restartButton.onClick.AddListener(Restart);
            if (mainMenuButton != null) mainMenuButton.onClick.AddListener(ReturnToMainMenu);
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
            if (GameManager.Instance == null || GameManager.Instance.CurrentSession == null)
            {
                Debug.LogWarning("[GameOverScreen] GameManager or CurrentSession not found!");
                return;
            }

            var session = GameManager.Instance.CurrentSession;

            // Display stats
            if (survivalTimeText != null)
            {
                survivalTimeText.text = $"Time: {session.GetFormattedTime()}";
            }

            if (killCountText != null)
            {
                killCountText.text = $"Kills: {session.killCount}";
            }

            if (levelText != null)
            {
                levelText.text = $"Level: {session.levelReached}";
            }

            // Show panel
            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(true);
            }

            Debug.Log($"[GameOverScreen] Game Over - Time: {session.GetFormattedTime()}, Kills: {session.killCount}, Level: {session.levelReached}");
        }

        private void Restart()
        {
            // Restore time scale
            Time.timeScale = 1f;

            // Reload current scene
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        private void ReturnToMainMenu()
        {
            // TODO: Load main menu scene
            Debug.Log("[GameOverScreen] Main Menu not implemented yet - restarting instead");
            
            // For now, just restart
            Restart();
        }
    }
}
