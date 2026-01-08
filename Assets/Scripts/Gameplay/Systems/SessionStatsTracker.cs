using UnityEngine;
using PixelVanguard.Core;

namespace PixelVanguard.Gameplay
{
    /// <summary>
    /// Responsibile for tracking session statistics (kills, gold, levels) 
    /// and updating the central SessionData storage.
    /// Extracted from GameManager to separate concerns.
    /// </summary>
    public class SessionStatsTracker : MonoBehaviour
    {
        private void OnEnable()
        {
            GameEvents.OnEnemyKilled += OnEnemyKilled;
            GameEvents.OnGoldCollected += OnGoldCollected;
            GameEvents.OnPlayerLevelUp += OnPlayerLevelUp;
            GameEvents.OnPlayerDeath += OnPlayerDeath;
        }

        private void OnDisable()
        {
            GameEvents.OnEnemyKilled -= OnEnemyKilled;
            GameEvents.OnGoldCollected -= OnGoldCollected;
            GameEvents.OnPlayerLevelUp -= OnPlayerLevelUp;
            GameEvents.OnPlayerDeath -= OnPlayerDeath;
        }

        private void Update()
        {
            // Track survival time if game is playing
            if (GameManager.Instance != null && GameManager.Instance.CurrentState == GameState.Playing)
            {
                if (SessionData.Instance != null)
                {
                    SessionData.Instance.survivalTime += Time.deltaTime;
                }
            }
        }

        private void OnEnemyKilled(int totalKills)
        {
            if (SessionData.Instance != null)
            {
                SessionData.Instance.killCount = totalKills;
            }
        }

        private void OnGoldCollected(int amount)
        {
            if (SessionData.Instance != null)
            {
                SessionData.Instance.goldCollected += amount;
            }
        }

        private void OnPlayerLevelUp()
        {
            if (SessionData.Instance != null)
            {
                SessionData.Instance.levelReached++;
            }
        }

        private void OnPlayerDeath()
        {
            if (SessionData.Instance != null)
            {
                SessionData.Instance.gameOverReason = GameOverReason.PlayerDied;
            }
        }
    }
}
