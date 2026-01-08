# Enemy Spawner System

**Status:** Refactored (Jan 2026)
**Location:** `Gameplay/Enemies/EnemySpawner.cs`

## Overview
Manages infinite enemy waves using a "Vampire Survivors" style logic (Screen Edge Spawning).

## Logic Flow
1.  **Timer:** Checks `spawnInterval`.
2.  **Edge Selection:** Picks a random screen edge (Top, Bottom, Left, Right).
3.  **Positioning:** Calculates a point just outside the camera view.
4.  **Validation:** `Physics2D.OverlapCircle` ensures the spot is valid (not inside a wall).
5.  **Spawning:** Instantiates enemy from `EnemyData` pool based on current Game Time.

## Refactoring Notes
*   **Legacy Code Removed:** Placeholder sprite generation and fallback logic were deleted.
*   **Optimization:** Uses Object Pooling (TODO) and FIFO queue to cap max enemies.
