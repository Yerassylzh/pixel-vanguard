# PluginYG Focus Pause Fix

**Issue:** Game freezes after watching revive ad  
**Date Fixed:** January 2026  
**Severity:** Critical (game-breaking)

[← Back to Architecture](../SYSTEM_ARCHITECTURE.md)

---

## The Problem

After watching a rewarded ad to revive:
- ✅ `Time.timeScale = 1f` (confirmed in logs)
- ✅ `GameState = Playing` (confirmed in logs)
- ❌ **Game remains frozen** (player can't move, enemies don't move, timer stopped)
- ✅ UI buttons work
- ✅ Audio plays

**Confusing symptom:** `Time.timeScale` was correct, but nothing moved!

---

## Root Cause Discovery

### PluginYG Focus Pause Behavior

From [PluginYG documentation](https://max-games.ru/plugin-yg/doc/reward-ad/?en):

When a rewarded ad shows, PluginYG:
1. **Pauses Unity's entire game loop** (not just `Time.timeScale`)
2. Shows the ad
3. **Doesn't automatically unpause** when ad completes
4. Fires callback (`OnRewardReceived`)

**The Issue:**
```csharp
// GameOverScreen.cs (BROKEN FLOW)
bool success = await adService.ShowRewardedAd();
if (success)
{
    RevivePlayer();  // ❌ Unity STILL PAUSED at this point!
                     // Time.timeScale = 1f has no effect
}
```

Even though the callback fired and we set `Time.timeScale = 1f`, Unity's game loop was still paused by PluginYG's Focus Pause system.

---

## The Solution

**Delay revive logic by 1 frame** to allow Unity's game loop to fully restore:

```csharp
// GameOverScreen.cs (FIXED)
private async void OnReviveClicked()
{
    bool success = await adService.ShowRewardedAd();
    
    if (success)
    {
        // CRITICAL: Wait for Unity to restore focus after PluginYG unpause
        StartCoroutine(RevivePlayerNextFrame());
    }
}

private IEnumerator RevivePlayerNextFrame()
{
    // CRITICAL WORKAROUND: PluginYG Focus Pause Issue
    // PluginYG pauses Unity's entire game loop when showing ads.
    // Even after ad callback fires, Unity remains paused until next frame.
    // This delay ensures Unity's game loop is fully restored before RevivePlayer().
    yield return new WaitForEndOfFrame();
    
    RevivePlayer();  // ✅ Now executes after Unity is fully unpaused
}
```

---

## Why This Works

**Execution Timeline:**

1. **Ad shows** → PluginYG pauses Unity game loop
2. **Ad completes** → PluginYG fires `OnRewardReceived` callback
3. **await completes** → `ShowRewardedAd()` returns (Unity STILL paused)
4. **`StartCoroutine()` called** → Queued for next frame
5. **PluginYG unpauses Unity** → Game loop restores before next frame
6. **Next frame starts** → Coroutine executes
7. **`WaitForEndOfFrame()`** → Ensures full restoration
8. **`RevivePlayer()` executes** → `Time.timeScale = 1f` now takes effect!

---

## Files Modified

### 1. GameOverScreen.cs
```csharp
// Added 1-frame delay coroutine
private IEnumerator RevivePlayerNextFrame()
{
    yield return new WaitForEndOfFrame();
    RevivePlayer();
}
```

### 2. GameManager.cs
```csharp
public void RevivePlayer()
{
    if (currentState != GameState.Reviving) return;
    
    Core.GameEvents.TriggerPlayerRevived();
    ResumeGame(); // Triggers OnGameResume event
}

public void ResumeGame()
{
    SetState(GameState.Playing);
    Time.timeScale = 1f;
    Core.GameEvents.TriggerGameResume();  // ✅ Unblocks VirtualJoystick
}
```

### 3. YandexAdService.cs
```csharp
private void OnRewardReceived(string rewardID)
{
    Time.timeScale = 1f;  // Restore timeScale (even though Unity is paused)
    if (rewardID == REWARD_ID)
        rewardedAdTask?.TrySetResult(true);
}

private void OnAdError()
{
    Time.timeScale = 1f;  // Also restore on error
    rewardedAdTask?.TrySetResult(false);
}

private void OnAdClosed()
{
    Time.timeScale = 1f;  // And on manual close
    rewardedAdTask?.TrySetResult(false);
}
```

---

## Testing Verification

**Before Fix:**
```
[GameManager] Time.timeScale set to 1f - actual value: 1  ✅
[GameManager] State set to Playing                        ✅
[GameManager] PlayerRevived event triggered               ✅
[GameManager] GameResume event triggered                  ✅

❌ Game still frozen (Unity game loop paused)
```

**After Fix:**
```
[GameOverScreen] Ad watched successfully
[Wait 1 frame - Unity game loop restores]
[PlayerHealth] Player revived with full HP                ✅
[GameManager] Game Resumed                                ✅

✅ Game resumes normally, all systems active
```

---

## Key Learnings

1. **`Time.timeScale` is not everything** - Unity's game loop can be paused separately
2. **PluginYG Focus Pause is enabled by default** - browsers pause games during ads
3. **Frame timing matters** - Sometimes you must wait for external systems to complete
4. **Event-driven systems need proper triggers** - `OnGameResume` was critical for unblocking

---

**Files Changed:**
- [GameOverScreen.cs](file:///C:/Users/Honor/Unity%20Games/Pixel%20Vanguard/Assets/Scripts/UI/Gameplay/GameOverScreen.cs)
- [GameManager.cs](file:///C:/Users/Honor/Unity%20Games/Pixel%20Vanguard/Assets/Scripts/Gameplay/GameManager.cs)
- [YandexAdService.cs](file:///C:/Users/Honor/Unity%20Games/Pixel%20Vanguard/Assets/Scripts/Services/Yandex/YandexAdService.cs)

**Last Updated:** 2026-01-02  
**Severity:** CRITICAL - Game-breaking bug resolved
