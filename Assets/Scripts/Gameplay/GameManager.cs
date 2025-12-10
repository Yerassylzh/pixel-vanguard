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

        [Header("Debug")]
        [SerializeField] private bool enableDebugLogs = true;

        public GameState CurrentState => currentState;
        public float GameTime => gameTime;
        public int KillCount => currentKillCount;
        public int GoldCollected => currentGoldCollected;

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
            }
        }

        private void Initialize()
        {
            Log("Initializing GameManager...");

            // Reset session data
            gameTime = 0f;
            currentKillCount = 0;
            currentGoldCollected = 0;

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
        }

        private void OnGoldCollected(int amount)
        {
            currentGoldCollected += amount;
        }

        private void OnPlayerDeath()
        {
            Log("Player died!");

            // TODO: In the future, check if ReviveManager wants to offer a revive
            // For now, end the game immediately
            EndGame(Core.GameOverReason.PlayerDied);
        }

        private void OnPlayerLevelUp()
        {
            SetState(GameState.LevelUp);
            Time.timeScale = 0f; // Pause for card selection
            Log("Level Up - Game Paused for card selection");
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
