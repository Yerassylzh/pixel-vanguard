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

        [Header("Session Data")]
        [SerializeField] private float gameTime = 0f;
        [SerializeField] private int currentKillCount = 0;
        [SerializeField] private int currentGoldCollected = 0;
        [SerializeField] private int currentLevel = 1;

        [Header("Debug")]
        [SerializeField] private bool enableDebugLogs = true;

        public GameState CurrentState => currentState;
        public float GameTime => gameTime;
        public int KillCount => currentKillCount;
        public int GoldCollected => currentGoldCollected;
        public int Level => currentLevel;
        public GameSession CurrentSession { get; private set; }

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
            Core.GameEvents.OnEnemyKilled += OnEnemyKilled;
            Core.GameEvents.OnGoldCollected += OnGoldCollected;
            Core.GameEvents.OnPlayerDeath += OnPlayerDeath;
            Core.GameEvents.OnPlayerLevelUp += OnPlayerLevelUp;
        }

        private void OnDisable()
        {
            Core.GameEvents.OnEnemyKilled -= OnEnemyKilled;
            Core.GameEvents.OnGoldCollected -= OnGoldCollected;
            Core.GameEvents.OnPlayerDeath -= OnPlayerDeath;
            Core.GameEvents.OnPlayerLevelUp -= OnPlayerLevelUp;
        }

        private void Start()
        {
            Initialize();
        }

        private void Update()
        {
            if (currentState == GameState.Playing)
            {
                gameTime += Time.deltaTime;
                
                // Update SessionData
                if (SessionData.Instance != null)
                {
                    SessionData.Instance.survivalTime = gameTime;
                }
            }
        }

        private void Initialize()
        {
            Log("Initializing GameManager...");

            // Create new game session
            CurrentSession = new GameSession();

            // Reset session data
            gameTime = 0f;
            currentKillCount = 0;
            currentGoldCollected = 0;
            currentLevel = 1; // Reset level to 1

            // Initialize SessionData singleton
            if (SessionData.Instance != null)
            {
                SessionData.Instance.ResetSession();
                Log("SessionData initialized and reset");
            }

            // Transition to playing state
            SetState(GameState.Playing);
            Core.GameEvents.TriggerGameStart();

            Log("Game Started!");
        }

        public void PauseGame()
        {
            if (currentState == GameState.Playing)
            {
                SetState(GameState.Paused);
                Time.timeScale = 0f;
                Core.GameEvents.TriggerGamePause();
                Log("Game Paused");
            }
        }

        public void ResumeGame()
        {
            if (currentState == GameState.Paused || currentState == GameState.LevelUp)
            {
                SetState(GameState.Playing);
                Time.timeScale = 1f;
                Core.GameEvents.TriggerGameResume();
                Log("Game Resumed");
            }
        }

        public void EndGame(Core.GameOverReason reason)
        {
            if (currentState == GameState.GameOver) return;

            SetState(GameState.GameOver);
            Time.timeScale = 0f; // Pause game
            Core.GameEvents.TriggerGameOver(reason);
            Log($"Game Over: {reason}");
        }

        private void SetState(GameState newState)
        {
            currentState = newState;
        }

        private void OnEnemyKilled(int totalKills)
        {
            currentKillCount = totalKills;
            
            // Update SessionData
            if (SessionData.Instance != null)
            {
                SessionData.Instance.killCount = totalKills;
            }
        }

        private void OnGoldCollected(int amount)
        {
            currentGoldCollected += amount;
            
            // Update SessionData
            if (SessionData.Instance != null)
            {
                SessionData.Instance.goldCollected = currentGoldCollected;
            }
        }

        private void OnPlayerDeath()
        {
            Log("Player died!");

            // Finalize session stats
            FinalizeSession();

            // Update SessionData with final stats
            if (SessionData.Instance != null)
            {
                SessionData.Instance.gameOverReason = Core.GameOverReason.PlayerDied;
            }

            // Transition to Reviving state (offers revive option)
            SetState(GameState.Reviving);
            Time.timeScale = 0f; // Pause for revive decision
            Core.GameEvents.TriggerGameOver(Core.GameOverReason.PlayerDied);

            // TODO: If player doesn't revive within X seconds, load Results Scene
            // For now, GameOverScreen handles the "Quit" button which can load Results Scene
            // StartCoroutine(LoadResultsSceneAfterTimeout(10f));
        }

        private void FinalizeSession()
        {
            if (CurrentSession == null) return;

            CurrentSession.survivalTime = gameTime;
            CurrentSession.killCount = currentKillCount;
            CurrentSession.goldCollected = currentGoldCollected;
            CurrentSession.levelReached = currentLevel;
        }

        private void OnPlayerLevelUp()
        {
            currentLevel++;
            
            // Update SessionData
            if (SessionData.Instance != null)
            {
                SessionData.Instance.levelReached = currentLevel;
            }
            
            SetState(GameState.LevelUp);
            Time.timeScale = 0f; // Pause for card selection
            Log($"Level Up - Now Level {currentLevel}");
        }

        /// <summary>
        /// Revive player with full health and resume game.
        /// Called from GameOverScreen when player watches ad.
        /// </summary>
        public void RevivePlayer()
        {
            if (currentState != GameState.Reviving)
            {
                Debug.LogWarning("[GameManager] Cannot revive - not in Reviving state");
                return;
            }

            Log("Player revived!");

            // Resume game
            SetState(GameState.Playing);
            Time.timeScale = 1f;

            // Trigger revive event (PlayerHealth will restore to full)
            Core.GameEvents.TriggerPlayerRevived();
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
