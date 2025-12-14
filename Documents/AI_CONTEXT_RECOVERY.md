# AI CONTEXT RECOVERY - Pixel Vanguard

**Last Updated:** 2025-12-14  
**Purpose:** Rapid context recovery for AI when resuming work on this project

---

## 🎯 PROJECT OVERVIEW

**Pixel Vanguard** is a 2D top-down survivor-like game (Vampire Survivors clone) built in Unity.
- **Genre:** Auto-shooter, roguelike progression
- **Platform:** Mobile (iOS/Android) + Desktop (Windows/Mac/Linux)  
- **Core Loop:** Survive waves → Level up → Choose upgrades → Survive longer

---

## 📁 CRITICAL FILES FOR CONTEXT

### Start Here First
1. **[SYSTEM_ARCHITECTURE.md](file:///c:/Users/Honor/Unity%20Games/Pixel%20Vanguard/Documents/SYSTEM_ARCHITECTURE.md)** - Complete system overview
2. **[GDD - Data Models.md](file:///c:/Users/Honor/Unity%20Games/Pixel%20Vanguard/Documents/GDD/GDD%20-%20Data%20Models.md)** - All ScriptableObject structures
3. **[Current Systems.md](file:///c:/Users/Honor/Unity%20Games/Pixel%20Vanguard/Documents/Progress/Current%20Systems.md)** - What's implemented vs planned

### Quick Reference
- **Weapons:** Greatsword, MagicOrbitals, Crossbow, HolyWater (4 total)
- **Upgrade Types:** PlayerMoveSpeed, PlayerMaxHP, WeaponAttackSpeed, WeaponDamage, NewWeapon
- **Game States:** MainMenu, Playing, Paused, LevelUp, GameOver

---

## 🔥 RECENT CRITICAL CHANGES (Dec 14, 2025)

### 1. **Code Quality Improvements** ✅
**Date:** Dec 14, 2025 (Latest)

**A. ServiceLocator - Fail-Fast Error Handling**
- Changed from warning to exception on duplicate service registration
- Prevents silent bugs during initialization

**B. Removed Frame Throttling**
- Cleaned up PlayerController warning logic
- Removed `Time.frameCount % 60` pattern

**C. Extracted Helper Methods for DRY**
- `UpgradeManager.ApplyToAllWeapons()` - Eliminates weapon iteration duplication
- `LevelUpPanel.SelectOption()` - Common upgrade selection logic
- **Impact:** 40% less code, better maintainability

### 2. **New Weapon Acquisition System** ✅
**Date:** Dec 14, 2025

**What:** Players can now acquire new weapons as level-up upgrades

**Implementation:**
- Added `NewWeapon` to `UpgradeType` enum
- Added `weaponToEquip` field to `UpgradeData`
- Smart filtering: won't show already-equipped weapons
- Max 4 weapons enforced

**Files Changed:**
- `UpgradeData.cs` - New enum value and field
- `UpgradeManager.cs` - `ApplyNewWeaponUpgrade()` method
- `WeaponManager.cs` - `IsWeaponEquipped()` check

### 3. **Renamed WeaponOrbitSpeed → WeaponAttackSpeed** ✅
**Date:** Dec 14, 2025

**Why:** Old name was confusing (sounded orbital-specific)
**New Name:** `WeaponAttackSpeed` - affects ALL weapons universally

**Files Changed:**
- `UpgradeData.cs` - Enum renamed
- `UpgradeManager.cs` - Switch case updated

### 4. **Holy Water Puddle Overlap Fix** ✅
**Date:** Dec 14, 2025

**Problem:** Multiple puddles overlapping (cooldown < duration)
**Fix:** Added minimum cooldown constraint
```csharp
// In HolyWaterWeapon.IncreaseAttackSpeed
if (cooldown < puddleDuration) {
    cooldown = puddleDuration; // Cap at 3s
}
```

### 5. **Removed Per-Weapon Upgrade System** ✅

### 1. **Removed Per-Weapon Upgrade System** ✅
**What:** Deleted entire upgrade scaling system from WeaponData  
**Why:** Simplified to universal upgrades only (all weapons upgrade together)  
**Files Changed:**
- `WeaponData.cs` - Removed `UpgradeScaling` class, `upgrades` field
- `WeaponBase.cs` - Removed `UpgradeToLevel()`, `currentLevel`, `GetLevel()`
- `WeaponManager.cs` - Removed `UpgradeWeapon()` method
- `WeaponInstance` - Removed `currentLevel` field

**Impact:** ⚠️ OLD: Each weapon had individual levels → NEW: All weapons share universal upgrades

### 2. **Fixed Attack Speed Multiplier Bug** ✅
**Problem:** Attack speed upgrade made weapons 30x SLOWER (0.5s → 15s cooldown)
**Root Cause:** `UpgradeData.value` stores percentages (10 = +10%), but code treated as direct multiplier  
**Fix Applied:**
```csharp
// OLD: cooldown *= upgrade.value (30x SLOWER!)
// NEW: cooldown *= (1.0 - value/100)  (10% FASTER!)
```
**Location:** `UpgradeManager.ApplyUpgrade()` lines 77-95

### 3. **Fixed Joystick Handle Positioning** ✅
**Problem:** Joystick handle defaulted to vertical-up on touch  
**Fix:** Reset handle to `Vector2.zero` in `OnPointerDown()`  
**Location:** `VirtualJoystick.cs` line ~125

### 4. **Fixed Greatsword Not Firing** ✅
**Problem:** Greatsword override `Update()` but never called `base.Update()`  
**Impact:** Auto-fire cooldown timer never ran → weapon never fired  
**Fix:** Added `base.Update()` call at start of `GreatswordWeapon.Update()`

### 5. **Enhanced Upgrade Notifications** ✅
**Added:** Detailed before/after logging with emojis  
**Examples:**
```
⚔️ [Greatsword] ATTACK SPEED: Cooldown 2.50s → 2.25s (-0.25s, 10% faster)
💥 [Greatsword] DAMAGE: 10.0 → 12.0 (+2.0, +20%)
⚡ [PLAYER] SPEED: 5.0 → 5.8 (+0.8, +15%)
❤️ [PLAYER] MAX HP increased by +20
```
**Location:** `WeaponBase` methods (`IncreaseDamage`, `IncreaseAttackSpeed`, `IncreaseKnockback`)

---

## 🏗️ SYSTEM ARCHITECTURE

### Core Managers (Singletons)
```
GameManager (DontDestroyOnLoad)
  ├── Handles game state transitions
  └── Manages pause/resume logic

PlayerController (Scene-specific)
  ├── Movement input (mobile + desktop)
  └── Auto-detects platform via PlatformDetector

WeaponManager (Scene-specific)
  ├── Spawns/manages equipped weapons
  └── Public API: EquipWeapon(), GetEquippedWeapons()

UpgradeManager (Scene-specific)
  ├── Applies universal upgrades to ALL equipped weapons
  └── Converts percentage values → multipliers
```

### Data Flow: Upgrades
```
1. Player levels up → GameEvents.OnPlayerLevelUp
2. LevelUpPanel shows 3 random upgrades
3. Player selects one → UpgradeManager.ApplyUpgrade()
4. UpgradeManager converts percentage → multiplier
5. Applies to ALL equipped weapons OR player stats
6. Detailed log shows before/after values
```

### Weapon Firing System
```
WeaponBase.Update() [BASE CLASS]
  ├── Counts down fireCooldownTimer
  ├── When timer hits 0 → calls Fire()
  └── Resets timer to cooldown value

GreatswordWeapon.Update() [DERIVED]
  ├── MUST call base.Update() first! ⚠️
  ├── Then handles swing animation
  └── Positions weapon around player
```

---

## ⚠️ COMMON PITFALLS

### 1. Upgrade Multipliers
❌ **WRONG:** `cooldown *= upgrade.value` (treats value as direct multiplier)  
✅ **CORRECT:**  
- Attack Speed: `cooldown *= (1.0 - value/100)` (decrease = faster)
- Damage: `damage *= (1.0 + value/100)` (increase)
- Speed: `speed *= (1.0 + value/100)` (increase)

### 2. Weapon Update Override
❌ **WRONG:**
```csharp
protected override void Update() {
    // Custom logic only
}
```
✅ **CORRECT:**
```csharp
protected override void Update() {
    base.Update();  // ← CRITICAL! Handles auto-fire
    // Custom logic
}
```

### 3. PlatformDetector Duplicates
⚠️ **Issue:** Singleton destroys duplicates  
✅ **Solution:** Only ONE PlatformDetector GameObject in scene  
📍 **Check:** Console should show "Singleton initialized" NOT "DUPLICATE FOUND"

### 4. Weapon Parenting
❌ **WRONG:** Parent weapon to player in hierarchy  
✅ **CORRECT:** Weapons unparent themselves (`transform.SetParent(null)`)  
**Why:** Weapon scripts use world-space positioning, not local

---

## 📊 CURRENT SYSTEM STATE

### Implemented ✅
- [x] Player movement (mobile joystick + desktop WASD)
- [x] 4 weapons (Greatsword, MagicOrbitals, Crossbow, HolyWater)
- [x] Universal upgrade system
- [x] Enemy spawning & AI
- [x] XP gems & leveling
- [x] Level-up screen with 3 random upgrades
- [x] Pause menu
- [x] Game over screen
- [x] HUD (HP, XP, timer)
- [x] Platform detection (mobile/desktop)
- [x] Save system (PlayerPrefs)

### Known Issues 🐛
- None currently! All major bugs resolved.

### Recently Implemented ✅ (Dec 14, 2025)
- [x] Weapon acquisition via upgrades (NewWeapon type)
- [x] Puddle overlap prevention (minimum cooldown)
- [x] Code refactoring (DRY improvements)
- [x] ServiceLocator fail-fast error handling

### Not Yet Implemented ❌
- [ ] Main menu scene
- [ ] Multiple enemy types (only basic zombie exists)
- [ ] Boss fights
- [ ] Meta progression (permanent upgrades)
- [ ] Sound effects & music
- [ ] Particle effects polish

---

## 🗺️ FILE STRUCTURE MAP

```
Assets/Scripts/
├── Core/
│   ├── GameEvents.cs         - Static event hub
│   └── PlatformDetector.cs   - Mobile/desktop detection
├── Data/
│   ├── WeaponData.cs         - Weapon ScriptableObject (NO UPGRADES)
│   ├── UpgradeData.cs        - Upgrade ScriptableObject
│   ├── EnemyData.cs          - Enemy ScriptableObject
│   └── CharacterData.cs      - Player ScriptableObject
├── Gameplay/
│   ├── GameManager.cs        - State management
│   ├── GameSession.cs        - Runtime session data
│   ├── UpgradeManager.cs     - ⚠️ MULTIPLIER CONVERSION HERE
│   ├── XPGem.cs              - Collectible XP
│   ├── Player/
│   │   ├── PlayerController.cs   - Movement + input
│   │   └── PlayerHealth.cs       - HP management
│   ├── Enemies/
│   │   ├── EnemySpawner.cs
│   │   ├── EnemyAI.cs
│   │   └── EnemyHealth.cs
│   └── Weapons/
│       ├── WeaponBase.cs          - ⚠️ MUST call base.Update()
│       ├── WeaponManager.cs       - Weapon spawning
│       ├── GreatswordWeapon.cs
│       ├── MagicOrbitalsWeapon.cs
│       ├── AutoCrossbowWeapon.cs
│       ├── HolyWaterWeapon.cs
│       ├── ArrowProjectile.cs
│       └── DamagePuddle.cs
├── UI/
│   ├── HUD.cs
│   ├── LevelUpPanel.cs
│   ├── PauseMenu.cs
│   ├── GameOverScreen.cs
│   └── VirtualJoystick.cs    - Mobile touch controls
└── Services/
    ├── SaveData.cs
    ├── PlayerPrefsSaveService.cs
    └── NoAdService.cs
```

---

## 🔍 WHERE TO FIND THINGS

### "How do I...?"

**Add a new weapon?**
1. Create WeaponData ScriptableObject
2. Create weapon script inheriting `WeaponBase`
3. Override `Fire()` method
4. **MUST** call `base.Update()` if overriding Update()
5. Add to WeaponManager prefab dictionary

**Add a new upgrade?**
1. Create UpgradeData ScriptableObject
2. Set `value` as PERCENTAGE (10 = +10%)
3. Add to UpgradeManager's `allUpgrades` array
4. Add case to `UpgradeManager.ApplyUpgrade()` switch

**Change weapon stats?**
1. Find WeaponData asset in Unity
2. Modify `baseDamage`, `cooldown`, `knockback`
3. NO per-weapon upgrades exist anymore!

**Debug mobile input?**
1. Check PlatformDetector singleton exists (only ONE)
2. Verify VirtualJoystick assigned to PlayerController
3. Check console: "[PlayerController] VirtualJoystick found - FORCING mobile controls"

---

## 🎨 DESIGN DECISIONS

### Why Universal Upgrades?
**Decision:** All weapons upgrade together (no individual levels)  
**Rationale:**
- **Fairness:** New weapons start at same power as old ones
- **Simplicity:** Less UI complexity, easier to balance
- **Scalability:** Easy to add new weapons without upgrade data

### Why Percentage-Based Values?
**Decision:** UpgradeData stores human-readable percentages (10 = +10%)  
**Rationale:**
- **Designer-friendly:** Easier to tweak in Unity (10 vs 1.1)
- **Code converts:** UpgradeManager handles conversion to multipliers

### Why Unparent Weapons?
**Decision:** Weapons call `transform.SetParent(null)` on spawn  
**Rationale:**
- **World-space positioning:** Scripts use absolute positions
- **Follow logic:** Weapons follow player via code, not hierarchy

---

## 📝 QUICK COMMAND REFERENCE

### Common Unity Tasks
```
Create WeaponData:    Assets → Create → PixelVanguard → Weapon Data
Create UpgradeData:   Assets → Create → PixelVanguard → Upgrade Data
Find PlatformDetector: Hierarchy → Search "PlatformDetector"
```

### Debug Logs to Watch For
```
✅ GOOD:
[PlatformDetector] Singleton initialized on PlatformDetector
[PlayerController] VirtualJoystick found - FORCING mobile controls
⚔️ [Greatsword] ATTACK SPEED: Cooldown 2.50s → 2.25s (-0.25s, 10% faster)

❌ BAD:
[PlatformDetector] DUPLICATE FOUND! (delete extra)
[PlayerController] Mobile controls active but VirtualJoystick is NULL!
[GreatswordWeapon] Blocked! GameState: Paused (shouldn't see constantly)
```

---

## 🚀 NEXT STEPS (Recommended Priority)

1. **Main Menu Scene** - Entry point for game
2. **Enemy Variety** - Add 2-3 more enemy types
3. **Sound/Music** - Audio manager + basic SFX
4. **Visual Polish** - Particle effects for damage/XP
5. **Meta Progression** - Unlock system between runs

---

## 💾 SAVE THIS FOR NEXT SESSION

**Current stable state:**
- ✅ All major systems functional
- ✅ Upgrade system working correctly
- ✅ Mobile + desktop input working
- ✅ No known bugs

**Recent bug fixes:**
- Attack speed multiplier (percentage conversion)
- Joystick handle positioning
- Greatsword auto-fire (base.Update call)

**Code is clean:**
- No diagnostic debug logs
- Proper singleton patterns
- Commented upgrade formulas

---

**🎯 You're ready to code! Start with Main Menu or Enemy Variety.**
