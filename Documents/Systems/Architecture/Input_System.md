# Input System

**Location:** `Systems/Architecture/Input_System.md`
**Key Files:** `PlayerInput.cs`, `VirtualJoystick.cs`, `PlatformDetector.cs`

## 1. Abstraction Layer
The game supports both Desktop (Keyboard) and Mobile (Touch) via a unified interface in `PlayerInput.cs`.

## 2. Platform Detection
`PlatformDetector.cs` determines `PlatformType` at runtime:
*   **Editor:** Uses Keyboard + Mouse (can simulate mobile).
*   **Android:** Uses Touch (Joystick).
*   **WebGL:** Checks `Global.deviceType` (Yandex SDK).
    *   *Desktop Browser:* Keyboard.
    *   *Mobile Browser:* Touch.

## 3. Virtual Joystick
**Location:** `UI/VirtualJoystick.cs`
*   **Behavior:** Appears on touch (Dynamic) or fixed (Static).
*   **Normalization:** Output is normalized Vector2 (-1 to 1).
*   **Coordinate Fix:** (Jan 2026) Corrected checks to use `RectTransformUtility.ScreenPointToLocalPointInRectangle`.

## 4. Usage
```csharp
// In PlayerController
Vector2 input;
if (PlatformDetector.IsMobile) {
    input = virtualJoystick.Direction;
} else {
    input = playerInputActions.Move.ReadValue<Vector2>();
}
```
