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
