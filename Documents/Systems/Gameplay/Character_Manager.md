# Character Manager System

**Location:** `Systems/Gameplay/Character_Manager.md`
**Code Path:** `Assets/Scripts/Core/CharacterManager.cs`

## 1. Responsibilities
*   **Spawning:** Instantiates the correct Prefab at Game Start.
*   **Persistence:** Remembers selection between sessions (`SaveService`).
*   **Camera:** Links `CinemachineVirtualCamera` to the new Player instance.

## 2. Character Selection UI
**Location:** `UI/CharacterSelect/`
*   **Carousel:** Displays unlocked and locked characters.
*   **Purchase Logic:**
    *   **Gold:** Deducts gold from SaveData.
    *   **Ads:** Tracks "Ads Watched" count (e.g., 2/5).
*   **Stat Preview:** Shows *final* stats including Shop Upgrades (e.g., "Speed: 5.0 (+20%)").

## 3. Data Flow
1.  **Menu:** `SelectedCharacterID` saved to JSON.
2.  **Scene Load:** `GameManager` starts.
3.  **Bootstrap:** `CharacterManager` reads ID -> Finds `CharacterData` -> Instantiates Prefab.
