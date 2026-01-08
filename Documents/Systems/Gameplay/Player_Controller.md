# Player Controller System

**Location:** `Systems/Gameplay/Player_Controller.md`
**Code Path:** `Assets/Scripts/Gameplay/Player/`

## Components
The Player entity is composed of 4 specialized Monobehaviours:

### 1. `PlayerController.cs` (The Brain)
*   **Role:** Central Hub & Singleton Access.
*   **API:** `PlayerController.Instance.GetMoveSpeed()`.
*   **Responsibility:** Initializes other components, handles Global State (Pause/Resume).

### 2. `PlayerMovement.cs` (The Physics)
*   **Role:** Moves the `Rigidbody2D`.
*   **Logic:** `rb.velocity = Input * Speed`.
*   **Note:** Uses `FixedUpdate` for smooth physics interactions.

### 3. `PlayerInput.cs` (The Interface)
*   **Role:** Abstraction layer for Input.
*   **Logic:**
    *   **Mobile:** Reads `VirtualJoystick`.
    *   **Desktop:** Reads `InputSystem` (WASD/Arrows).
*   **State:** automatically blocks input during Pause/LevelUp.

### 4. `PlayerHealth.cs` (The Vitality)
*   **Role:** HP, Damage Taking, Death.
*   **Events:**
    *   Fires `GameEvents.OnPlayerDamaged`.
    *   Fires `GameEvents.OnPlayerDeath` via `GameManager`.
*   **IFrame:** Handles short invulnerability after hit.

## Integration
*   **Spawning:** `CharacterManager` instantiates specific Prefabs (Knight/Ranger) which contain these components.
*   **Upgrades:** `UpgradeManager` modifies `PlayerController` fields (Speed) or `PlayerHealth` fields (MaxHP) directly.
