# Player System

**Status:** ✅ Fully Implemented (Refactored Dec 2024)  
**Location:** `Assets/Scripts/Gameplay/Player/`  
**Components:** PlayerController, PlayerMovement, PlayerInput, PlayerHealth

[← Back to Architecture](../SYSTEM_ARCHITECTURE.md)

---

## Overview

The player system uses **component composition** for separation of concerns. Each component has a focused responsibility.

```
Player GameObject
├─ PlayerController    - Singleton reference, GameManager integration
├─ PlayerMovement      - Rigidbody2D physics movement
├─ PlayerInput         - Input handling (WASD + Mobile Joystick)
└─ PlayerHealth        - HP management, damage, passive effects
```

---

## PlayerController

**Purpose:** Singleton reference and speed management

```csharp
// Provides global access to player
public static PlayerController Instance { get; private set; }

// Speed API for upgrades
public float GetMoveSpeed();
public void SetMoveSpeed(float speed);
```

**Key Features:**
- Singleton pattern for easy access
- Integrates with GameManager for pause/resume
- Central speed modification point for upgrades

---

## PlayerMovement

**Purpose:** Rigidbody2D-based movement logic

```csharp
void FixedUpdate()
{
    Vector2 movement = playerInput.GetMovementInput();
    rb.linearVelocity = movement * moveSpeed;
}
```

**Features:**
- Physics-based movement
- Diagonal movement normalization
- Respects game state (pauses during level-up)

---

## PlayerInput

**Purpose:** Platform-aware input handling

```csharp
Vector2 GetPlatformInput()
{
    // Mobile: Use virtual joystick
    if (platformDetector.IsMobile())
        return virtualJoystick.Direction;
    
    // Desktop: New Input System
    return currentInput;
}
```

**Features:**
- Unity's New Input System (WASD, Arrows, Gamepad)
- Virtual Joystick for mobile
- Runtime platform switching
- State-aware (blocks during pause/levelup/gameover)

**Integration:**
- Checks `GameManager.CurrentState != GameState.Playing`
- Uses `PlatformDetector.IsMobile()` for input sourcing

---

## PlayerHealth

**Purpose:** HP management and passive upgrade storage

```csharp
// HP tracking
private int currentHealth;
private int maxHealth;

// Passive upgrade storage
public float characterDamageMultiplier = 1f;  // Might upgrade
public float lifestealPercent = 0f;           // Lifesteal
public float goldBonusMultiplier = 1f;         // Lucky Coins
```

**Features:**
- Damage cooldown (invincibility frames)
- Stores passive upgrade values
- Triggers `OnPlayerDeath` event → Game Over
- Listens to `OnPlayerRevived` event → Restores full HP

---

## Character System Integration

### CharacterData (ScriptableObject)
Defines character stats loaded at spawn:
- Move Speed
- Max HP
- Damage Multiplier
- Starter Weapon

### CharacterManager
- Spawns selected character at runtime
- Configures camera follow (Cinemachine)
- Validates tags/layers/components

**Example (Knight):**
- Move Speed: 5.0
- Max HP: 100
- Damage Multiplier: 1.0x
- Starter Weapon: Greatsword

---

## Event Integration

**Listens to:**
- `OnGamePause` → Block input (PlayerInput)
- `OnGameResume` → Unblock input (PlayerInput)
- `OnGameOver` → Block input  (PlayerInput)
- `OnPlayerRevived` → Restore full HP (PlayerHealth)

**Triggers:**
- `OnPlayerDeath` → Game Over sequence

---

## Code Locations

| Component | File |
|-----------|------|
| PlayerController | [PlayerController.cs](file:///C:/Users/Honor/Unity%20Games/Pixel%20Vanguard/Assets/Scripts/Gameplay/Player/PlayerController.cs) |
| PlayerMovement | [PlayerMovement.cs](file:///C:/Users/Honor/Unity%20Games/Pixel%20Vanguard/Assets/Scripts/Gameplay/Player/PlayerMovement.cs) |
| PlayerInput | [PlayerInput.cs](file:///C:/Users/Honor/Unity%20Games/Pixel%20Vanguard/Assets/Scripts/Gameplay/Player/PlayerInput.cs) |
| PlayerHealth | [PlayerHealth.cs](file:///C:/Users/Honor/Unity%20Games/Pixel%20Vanguard/Assets/Scripts/Gameplay/Player/PlayerHealth.cs) |
| CharacterManager | [CharacterManager.cs](file:///C:/Users/Honor/Unity%20Games/Pixel%20Vanguard/Assets/Scripts/Core/CharacterManager.cs) |

---

**Last Updated:** 2026-01-02  
**Related:** [Upgrade System](Upgrade-System.md), [Localization](Localization-System.md)
