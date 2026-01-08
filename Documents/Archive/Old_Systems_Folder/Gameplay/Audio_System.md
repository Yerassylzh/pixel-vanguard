# Audio System

**Status:** Refactored (Jan 2026)
**Location:** `Core/AudioManager.cs`

## Overview
The audio system is a persistent Singleton that handles Music and SFX. It uses an **Event-Driven** architecture to decouple itself from gameplay logic.

## Key Refactors
*   **No Reflection:** Accesses `WeaponData` via public property.
*   **Automatic Cleanup:** Unsubscribes from all events on scene change.

## Usage
### Play Sound
Do not call `AudioManager` directly. Fire an event instead:
```csharp
// GOOD: Decoupled
GameEvents.TriggerGoldCollected(10); 

// BAD: Coupled
AudioManager.Instance.PlaySFX(goldClip);
```

### Add New Sound
1.  Add `AudioClip` field to `SFXLibrary` ScriptableObject.
2.  Add event to `GameEvents.cs`.
3.  Subscribe in `AudioManager.OnEnable()`.
