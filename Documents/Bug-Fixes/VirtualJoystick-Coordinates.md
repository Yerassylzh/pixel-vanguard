# VirtualJoystick Coordinate Conversion Fix

**Issue:** Joystick appears far above touch point  
**Date Fixed:** January 2026  
**Severity:** High (broken mobile controls)

[← Back to Architecture](../SYSTEM_ARCHITECTURE.md)

---

## The Problem

When player touches screen to move:
- ✅ Touch detected correctly
- ❌ **Joystick appears much higher than touch point**
- Joystick was offset by ~300-400 pixels upward

**Visual:** If player touches bottom-left, joystick appears in center-left.

---

## Root Cause

**Parent GameObject Anchor Configuration:**
- `VirtualJoystick` parent has **stretched anchors**
- Anchor Min: (0, 0)
- Anchor Max: (1, 0)

**The Issue:**
```csharp
// BROKEN: Convert screen touch to parent's local space
RectTransformUtility.ScreenPointToLocalPointInRectangle(
    parentRect,          // ❌ Parent has stretched anchors!
    eventData.position,
    camera,
    out localPos
);

joystickContainer.anchoredPosition = localPos;  // ❌ Wrong position!
```

When parent has stretched anchors, its local coordinate system is **distorted**. The conversion math assumes a non-stretched rect, producing incorrect coordinates.

---

## The Solution

**Bypass anchor stretching** by converting through canvas and using world position:

```csharp
// FIX: Convert via canvas (reliable coordinates)
Vector2 canvasPos;
RectTransformUtility.ScreenPointToLocalPointInRectangle(
    canvasRect,          // ✅ Canvas has no stretching
    eventData.position,
    canvas.worldCamera,
    out canvasPos
);

// Set world position (bypasses anchor system entirely)
joystickContainer.position = canvasRect.TransformPoint(canvasPos);
```

---

## Why This Works

### Step-by-Step:

1. **Screen Touch:** (500, 200) pixels
2. **Convert to Canvas Local:** Canvas has predictable coordinates (no stretching)
   - `canvasPos = (100, 50)` in canvas space
3. **Transform to World:** `TransformPoint()` converts canvas-local → world space
   - `worldPos = (-2.5f, -3.8f)` in Unity world
4. **Set World Position:** `joystickContainer.position = worldPos`
   - **Bypasses anchors entirely** - position is absolute in world

**Key Insight:** Setting `position` (world space) instead of `anchoredPosition` (anchor-relative) avoids all anchor stretching issues.

---

## Code Comparison

### Before (Broken)
```csharp
// Direct conversion to parent space
RectTransformUtility.ScreenPointToLocalPointInRectangle(
    parentRect,  // Stretched anchors cause distortion
    eventData.position,
    camera,
    out localPos
);
joystickContainer.anchoredPosition = localPos;  // Wrong!
```

### After (Fixed)
```csharp
// COORDINATE CONVERSION FIX
// Parent has stretched anchors (Min: 0,0 / Max: 1,0)
// Solution: Convert via canvas, then set world position

Vector2 canvasPos;
RectTransformUtility.ScreenPointToLocalPointInRectangle(
    canvasRect,  // Canvas is reliable (no stretching)
    eventData.position,
    canvas.worldCamera,
    out canvasPos
);

// Set world position to bypass anchor system
joystickContainer.position = canvasRect.TransformPoint(canvasPos);
```

---

## Unity UI Coordinate Spaces Explained

### Problem Space: Stretched Anchors

When a RectTransform has anchors at different positions (e.g., Min=(0,0), Max=(1,0)):
- Its **width** is defined as a percentage of parent width
- Its **local coordinates** are distorted by this stretching
- Direct conversion from screen → local produces incorrect results

### Solution Space: World Position

World space is **absolute** and unaffected by anchors:
- Canvas coordinates → reliable baseline
- `TransformPoint()` → converts to absolute world space
- Setting `position` → bypasses anchor system

---

## Testing Verification

**Before Fix:**
- Touch bottom-left corner (100, 100)
- Joystick appears at (~100, 450) ❌
- Offset of ~350 pixels upward

**After Fix:**
- Touch bottom-left corner (100, 100)
- Joystick appears at (100, 100) ✅
- Perfect centering on touch point

---

## Unity Inspector Requirements

For optimal centering, ensure:
- `JoystickContainer` **Pivot** = (0.5, 0.5)
- This makes the joystick center on the touch point

If Pivot is (0, 0), joystick's bottom-left will be at touch point instead of center.

---

**File Modified:**
- [VirtualJoystick.cs](file:///C:/Users/Honor/Unity%20Games/Pixel%20Vanguard/Assets/Scripts/UI/Gameplay/VirtualJoystick.cs)

**Last Updated:** 2026-01-02  
**Severity:** HIGH - Mobile controls were broken
