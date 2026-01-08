using UnityEngine;

namespace PixelVanguard.Gameplay
{
    /// <summary>
    /// Master game state controller for GameScene.
    /// Manages game states, time scaling, and high-level game flow.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Game State")]
        [SerializeField] private GameState currentState = GameState.Initializing;

        [Header("Debug")]
        [SerializeField] private bool enableDebugLogs = true;

        public GameState CurrentState => currentState;
        // Properties redirected to SessionData for convenience/backwards compatibility if needed
        public float GameTime => SessionData.Instance != null ? SessionData.Instance.survivalTime : 0f;
        public int KillCount => SessionData.Instance != null ? SessionData.Instance.killCount : 0;
        public int GoldCollected => SessionData.Instance != null ? SessionData.Instance.goldCollected : 0;
        public int Level => SessionData.Instance != null ? SessionData.Instance.levelReached : 1;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void OnEnable()
        {
            Core.GameEvents.OnPlayerDeath += OnPlayerDeath;
            Core.GameEvents.OnPlayerLevelUp += OnPlayerLevelUp;
        }

        private void OnDisable()
        {
            Core.GameEvents.OnPlayerDeath -= OnPlayerDeath;
            Core.GameEvents.OnPlayerLevelUp -= OnPlayerLevelUp;
        }

        private void Start()
        {
            Initialize();
        }

        private void Update()
        {
            // Logic moved to SessionStatsTracker
        }

        private void Initialize()
        {
            if (SessionData.Instance != null)
            {
                SessionData.Instance.ResetSession();
            }
            else
            {
                Debug.LogError("[GameManager] SessionData.Instance is NULL! Stats will not be recorded.");
            }

            // Ensure LifestealSystem exists
            if (GetComponent<LifestealSystem>() == null)
            {
                gameObject.AddComponent<LifestealSystem>();
            }

            if (GetComponent<SessionStatsTracker>() == null)
            {
                gameObject.AddComponent<SessionStatsTracker>();
            }

            SetState(GameState.Playing);
            Core.GameEvents.TriggerGameStart();
        }

        public void PauseGame()
        {
            if (currentState == GameState.Playing)
            {
                SetState(GameState.Paused);
                Time.timeScale = 0f;
                Core.GameEvents.TriggerGamePause();
            }
        }

        public void ResumeGame()
        {
            SetState(GameState.Playing);
            Time.timeScale = 1f;
            Core.GameEvents.TriggerGameResume();
        }

        public void EndGame(Core.GameOverReason reason)
        {
            if (currentState == GameState.GameOver) return;

            SetState(GameState.GameOver);
            Time.timeScale = 0f; // Pause game
            Core.GameEvents.TriggerGameOver(reason);
        }

        private void SetState(GameState newState)
        {
            currentState = newState;
        }



        private void OnPlayerDeath()
        {
            SetState(GameState.Reviving);
            Time.timeScale = 0f; // Pause for revive decision
            Core.GameEvents.TriggerGameOver(Core.GameOverReason.PlayerDied);
        }

        private void OnPlayerLevelUp()
        {
            SetState(GameState.LevelUp);
            Time.timeScale = 0f; // Pause for card selection
        }

        /// <summary>
        /// Revive player with full health and resume game.
        /// Called from GameOverScreen when player watches ad.
        /// </summary>
        public void RevivePlayer()
        {
            if (currentState != GameState.Reviving)
            {
                Debug.LogWarning($"[GameManager] Cannot revive - not in Reviving state (current: {currentState})");
                return;
            }

            Core.GameEvents.TriggerPlayerRevived();
            ResumeGame();
        }

        private void Log(string message)
        {
            if (enableDebugLogs)
            {
                Debug.Log($"[GameManager] {message}");
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }

            // Restore time scale when exiting GameScene
            Time.timeScale = 1f;
        }
    }

    public enum GameState
    {
        Initializing,  // Loading and setup
        Playing,       // Active combat
        Paused,        // Pause menu open
        LevelUp,       // Card selection screen
        Reviving,      // Revive screen shown
        GameOver       // Run complete
    }
}
