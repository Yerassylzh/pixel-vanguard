# Audio System

**Location:** `Systems/Gameplay/Audio_System.md`
**Code Path:** `Assets/Scripts/Core/AudioManager.cs`

## 1. Overview
The `AudioManager` is a Singleton responsible for all sound playback. It decouples gameplay from audio assets using the Event Bus.

## 2. Architecture
*   **Singleton:** `AudioManager.Instance` (Persists across scenes).
*   **Event-Driven:** Listens to `GameEvents` (e.g., `OnPlayerShoot`) rather than being called directly.
*   **Data:** `SFXLibrary` (ScriptableObject) maps Enum/Strings to AudioClips.

## 3. Key Refactoring (Jan 2026)
*   **Remvoed Reflection:** Previously used Reflection to find `WeaponData`; now uses public `Weapon.Data` property.
*   **Automatic Cleanup:** Unsubscribes from events `OnDisable` to prevent memory leaks.
*   **Performance:** Uses `AudioSource` pooling (via `PlayOneShot`) for high-frequency sounds.

## 4. Usage
```csharp
// 1. Define Event in GameEvents.cs
public static event Action OnGoldCollected;

// 2. Trigger in Gameplay
GameEvents.OnGoldCollected?.Invoke();

// 3. Audio Manager Responses
void OnEnable() {
    GameEvents.OnGoldCollected += PlayGoldSound;
}
```
