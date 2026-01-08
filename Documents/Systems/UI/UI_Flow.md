# UI Flow & Scenes

**Location:** `Systems/UI/UI_Flow.md`

## 1. Scene Hierarchy
1.  **Bootstrap:** (Invisible) Init services, load data.
2.  **MainMenu:** (Hub)
    *   **Play:** Go to Game.
    *   **Shop:** Upgrade stats overlay.
    *   **Barracks:** Character selection overlay.
3.  **GameScene:** (Core Loop)
    *   **HUD:** HP, XP, Joysticks.
    *   **PauseMenu:** Settings, Quit.
    *   **LevelUp:** 3-card selection (pauses game).
    *   **ReviveScreen:** "Watch Ad to Continue" (Last chance).
4.  **Results:** (Summary)
    *   Win/Loss state.
    *   "Double Gold" Ad offer.

## 2. Key UI Principles
*   **Anchoring:** All UI must stretch for Phones (Portrait/Landscape) and Desktop.
*   **Canvas Scaling:** Scale with Screen Size (Ref: 1920x1080).
*   **Y-Sort:** In-game UI (damage numbers) must sort correctly with sprites.

## 3. Monetization Hookpoints
*   **Main Menu:** Shop (IAP), Gold Packs (Ads).
*   **Revive Screen:** Revive (Ad).
*   **Results:** Double Gold (Ad).
