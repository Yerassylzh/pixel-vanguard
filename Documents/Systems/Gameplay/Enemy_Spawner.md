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

## 5. Fallback Spawn Position (NEW - 2026-01-09)

**Purpose**: Safe spawn point when all position validation attempts fail

**Inspector Field**:
- `fallbackSpawnPoint` (Transform) - Assign a safe position (e.g., center of play area)

**Logic**:
```csharp
private Vector3 GetRandomSpawnPosition()
{
    // Try 10 times to find valid position
    for (int attempt = 0; attempt < 10; attempt++)
    {
        Vector3 pos = CalculateSpawnPosition();
        if (IsValidSpawnPosition(pos)) // No colliders blocking
            return pos;
    }
    
    // All attempts failed - use fallback
    if (fallbackSpawnPoint != null)
    {
        Debug.LogWarning("All spawn attempts failed. Using fallback spawn point.");
        return fallbackSpawnPoint.position;
    }
    
    // No fallback - last resort
    Debug.LogWarning("No fallback configured. Spawning at potentially blocked position.");
    return CalculateSpawnPosition();
}
```

**Benefits**:
- Graceful degradation when spawn areas blocked
- Clear debugging warnings in Console
- Inspector-configurable (no code changes needed)

