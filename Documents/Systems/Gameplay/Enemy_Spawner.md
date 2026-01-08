# Enemy Spawner System

**Location:** `Systems/Gameplay/Enemy_Spawner.md`
**Code Path:** `Assets/Scripts/Gameplay/Enemies/EnemySpawner.cs`

## 1. Overview
Manages the lifecycle of enemies: Spawning, Scaling, and Despawning.

## 2. Spawning Logic
*   **Edge Spawning:** Enemies spawn just outside the camera viewport.
*   **Algorithm:**
    1.  Pick Random Edge (Top/Bottom/Left/Right).
    2.  Calculate World Position.
    3.  Check Physics Overlap (Verify position is valid).
    4.  Instantiate Prefab.

## 3. Difficulty Scaling
*   **Input:** `GameManager.Instance.GameTime` (Minutes).
*   **Data:** `EnemyData` has `minSpawnTime`.
*   **Logic:**
    *   At 0:00 -> Only Skeltons.
    *   At 2:00 -> Add Orcs.
    *   At 5:00 -> Add Ghosts.

## 4. Refactoring (Jan 2026)
*   **Clean Code:** Removed `CreatePlaceholderEnemy` and legacy fallback logic. The system now strictly requires `EnemyData.prefab` to be assigned.
*   **Optimization:** Added direct references to `GameEvents` for kill tracking.
