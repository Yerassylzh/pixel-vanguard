using UnityEngine;
using PixelVanguard.Core;

namespace PixelVanguard.Gameplay
{
    /// <summary>
    /// Singleton that holds current session data.
    /// Persists between scenes using DontDestroyOnLoad.
    /// Used to pass stats from GameScene to ResultsScene.
    /// </summary>
    public class SessionData : MonoBehaviour
    {
        public static SessionData Instance { get; private set; }

        // === CURRENT RUN STATISTICS ===
        
        /// <summary>
        /// Total time survived in seconds.
        /// </summary>
        public float survivalTime;

        /// <summary>
        /// Total enemies killed this run.
        /// </summary>
        public int killCount;

        /// <summary>
        /// Total gold collected this run (before any multipliers).
        /// </summary>
        public int goldCollected;

        /// <summary>
        /// Highest level reached this run.
        /// </summary>
        public int levelReached;

        /// <summary>
        /// Reason for game over (victory, player died, timeout, etc).
        /// </summary>
        public GameOverReason gameOverReason;

        private void Awake()
        {
            // Singleton pattern with DontDestroyOnLoad
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Debug.LogWarning("[SessionData] Duplicate instance detected, destroying...");
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Reset session data for a new run.
        /// Call this when starting a new game.
        /// </summary>
        public void ResetSession()
        {
            survivalTime = 0f;
            killCount = 0;
            goldCollected = 0;
            levelReached = 1;
            gameOverReason = GameOverReason.PlayerDied;
        }

        /// <summary>
        /// Get formatted time string (MM:SS).
        /// </summary>
        public string GetFormattedTime()
        {
            int minutes = Mathf.FloorToInt(survivalTime / 60f);
            int seconds = Mathf.FloorToInt(survivalTime % 60f);
            return $"{minutes:00}:{seconds:00}";
        }

        /// <summary>
        /// Get formatted time string with hours (HH:MM:SS).
        /// </summary>
        public string GetFormattedTimeWithHours()
        {
            int hours = Mathf.FloorToInt(survivalTime / 3600f);
            int minutes = Mathf.FloorToInt((survivalTime % 3600f) / 60f);
            int seconds = Mathf.FloorToInt(survivalTime % 60f);
            
            if (hours > 0)
                return $"{hours:00}:{minutes:00}:{seconds:00}";
            else
                return $"{minutes:00}:{seconds:00}";
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }
    }
}
