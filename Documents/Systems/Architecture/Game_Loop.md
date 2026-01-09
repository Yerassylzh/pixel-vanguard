# Game Loop & Architecture

**Location:** `Systems/Architecture/Game_Loop.md`
**Key Files:** `GameManager.cs`, `GameBootstrap.cs`, `GameEvents.cs`

## 1. Bootstrap Sequence
The game does not start in `GameScene`. It starts in `BootstrapScene`.
1.  **`GameBootstrap.cs`:**
    *   Initializes `ServiceLocator` (Ads, Saves).
    *   Loads `SaveData`.
    *   Loads `MainMenu` scene.

## 2. Global State Machine (`GameManager`)
**Singleton:** `GameManager.Instance`
**States:**
*   `Intro`: Animation playing.
*   `Playing`: Core loop active.
*   `Paused`: Input blocked, time scale 0.
*   `LevelUp`: Time scale 0, Upgrade Panel open.
*   `Revive`: "Watch Ad" screen active.
*   `GameOver`: Results screen active.

**Usage:**
```csharp
// Pausing
GameManager.Instance.PauseGame(); // Sets State=Paused, Time.timeScale=0

// Game Over
GameManager.Instance.TriggerGameOver(); // Sets State=GameOver, Fires OnGameOver
```

## 3. Event Bus (`GameEvents`)
Decoupled communication. Systems subscribe to these static events.
*   `OnEnemyKilled(int total)`: Updates XP, Stats.
*   `OnPlayerDamaged(float current, float max)`: Updates HUD.
*   `OnLevelUp(int level)`: Triggers Paused/LevelUp state.
*   `OnWaveComplete`: (Future use).

## 4. SessionData Lifecycle (NEW - 2026-01-09)

**Purpose**: Persistent GameObject that tracks runtime stats during gameplay  
**File**: `Gameplay/SessionData.cs`

**Lifecycle Pattern**:
```csharp
// Created in GameScene
void Awake()
{
    if (Instance != null && Instance != this)
    {
        Debug.LogWarning("Duplicate SessionData instance detected! Destroying duplicate.");
        Destroy(gameObject);
        return;
    }
    
    Instance = this;
    DontDestroyOnLoad(gameObject); // Persists across scenes
}
```

**Scene Transition Flow**:
1. **GameScene → ResultsScene**: SessionData persists (DontDestroyOnLoad)
   - ResultsScene reads `SessionData.Instance.goldEarned`, `enemiesKilled`, etc.
2. **ResultsScene → MainMenu**: SessionData destroyed in MainMenuManager.Start()
   ```csharp
   if (SessionData.Instance != null)
   {
       Destroy(SessionData.Instance.gameObject); // Clean slate
   }
   ```
3. **MainMenu → GameScene**: Fresh SessionData created

**Why Destroy in MainMenu?**  
Prevents duplicate instance warnings when returning to GameScene (which has SessionData in hierarchy).

**Tracked Data**:
- `goldEarned` - Gold collected this run
- `enemiesKilled` - Total kills
- `damageDealt` - Total damage
- `survivalTime` - Duration in seconds
- `characterUsed` - Character ID

