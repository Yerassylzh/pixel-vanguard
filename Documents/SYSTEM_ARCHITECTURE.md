# Pixel Vanguard - System Architecture Overview

**Purpose:** Quick context recovery for AI/developers returning to project  
**Last Updated:** 2025-12-20

---

## 🎮 Game Core

**Genre:** Horde Survivor / Action Roguelite (Vampire Survivors-like)  
**Platform:** Unity 2D (Desktop + Mobile)  
**Architecture:** Service Locator + Event-Driven + ScriptableObject Data

---

## 📊 Complete System Map

### Core Loop
```
Spawn → Fight → Collect XP → Level Up → Choose Upgrade → Repeat → Die → Stats → Restart
```

### Systems (9 Core)
1. **Player** - Movement, health, singleton reference
2. **Characters** - Selection, stat loading, prefab spawning via CharacterManager
3. **Weapons** - 4 types, auto-fire, upgrades via WeaponManager
4. **Enemies** - AI chase, spawning, health, XP drops
5. **Progression** - XP collection, leveling, upgrade selection
6. **UI** - HUD, level-up panel, pause, game over
7. **Input** - Platform-aware (WASD/arrows + joystick)
8. **Services** - Save/load, platform detection, ads (架構)
9. **Camera** - Cinemachine follow

---

## 🗡️ WEAPON SYSTEM (Most Complex)

### 4 Weapon Types

| Weapon | Type Enum | Script | Behavior |
|--------|-----------|--------|----------|
| Greatsword | `Greatsword` | GreatswordWeapon.cs | Periodic 360° swing (2.5s cooldown) |
| AutoCrossbow | `Crossbow` | AutoCrossbowWeapon.cs | Fires arrows at enemies |
| HolyWater | `HolyWater` | HolyWaterWeapon.cs | Spawns "Sanctified Ground" fire zone (area denial) |
| MagicOrbitals | `MagicOrbitals` | MagicOrbitalsWeapon.cs | Shields orbit continuously |

### Weapon Architecture
```
WeaponBase (abstract)
├── Properties: damage, cooldown, knockback, weaponData
├── Methods: Fire() [abstract], UpgradeToLevel(), IncreaseDamage(), IncreaseAttackSpeed()
├── Singleton ref: PlayerController.Instance.transform
│
├── GreatswordWeapon
│   └── State machine: Idle → Swing → Idle
├── AutoCrossbowWeapon
│   └── Spawns ArrowProjectile prefabs
├── HolyWaterWeapon
│   └── Spawns DamagePuddle prefabs
└── MagicOrbitalsWeapon
    └── Continuous orbit calculation
```

### WeaponManager (Orchestrator)
```csharp
// Located on: Player GameObject
// Max weapons: 4 simultaneous
// Auto-equips: Greatsword on start

Key Methods:
- EquipWeapon(WeaponData) → Instantiate + track
- UpgradeWeapon(weaponID) → Level up specific weapon
- GetEquippedWeapons() → List<WeaponInstance>

Prefab Mapping (via enum):
- Greatsword → greatswordPrefab
- MagicOrbitals → magicOrbitalsPrefab
- Crossbow → autoCrossbowPrefab
- HolyWater → holyWaterPrefab
```

### WeaponData (ScriptableObjects)
```
Location: Assets/ScriptableObjects/Weapons/
Files: Greatsword.asset, AutoCrossbow.asset, HolyWater.asset, MagicOrbitals.asset

Structure:
- weaponID: string (lowercase identifier)
- displayName: string (UI display)
- type: WeaponType enum
- baseDamage, cooldown, knockback
- baseDuration (for timed effects like HolyWater)
- baseTickRate (for DoT weapons)

Note: NO per-weapon upgrades! All upgrades are universal.
```

---

## 🎯 UPGRADE SYSTEM

### UpgradeManager
```csharp
// Uses: WeaponManager to apply to ALL equipped weapons
// NOT weapon-specific anymore!

Upgrade Types:
1. PlayerMoveSpeed → Multiplies player.moveSpeed
2. PlayerMaxHP → Adds to max health
3. WeaponAttackSpeed → Calls weapon.IncreaseAttackSpeed() for ALL
4. WeaponDamage → Calls weapon.IncreaseDamage() for ALL
5. NewWeapon → Equips a new weapon (max 4 total)

Flow:
Level Up → GetRandomUpgrades(3) → Player picks → ApplyUpgrade()
```

### Key Fix (2025-12-14):
❌ **OLD:** Only upgraded Greatsword  
✅ **NEW:** Loops through `weaponManager.GetEquippedWeapons()` and upgrades ALL

---

## 👤 PLAYER SYSTEM

### PlayerController
```csharp
// Singleton: PlayerController.Instance (accessed by weapons)
// Input: New Input System
// Movement: WASD/Arrows (desktop) + Floating joystick (mobile)
// Speed: Upgradeable via reflection (no public setter yet)

State Checking:
- Respects GameManager.CurrentState
- Blocks input when paused
```

### PlayerHealth
```csharp
// HP management with events
// Public API: IncreaseMaxHealth(int amount)
// Event: OnPlayerDeath → GameManager triggers game over
```

---

## 🎭 CHARACTER SYSTEM

### CharacterManager
```csharp
// Static access to selected character
CharacterManager.SelectedCharacter → CharacterData
CharacterManager.SpawnedPlayer → GameObject

// Spawns player prefab from CharacterData.characterPrefab
// Validates: tags, layers, components, sprite sorting
// Auto-assigns: Cinemachine camera target
```

### CharacterData (ScriptableObjects)
```
Location: Assets/ScriptableObjects/Characters/

Structure:
- characterID: string (unique identifier)
- displayName: string (UI display)
- Max HP, Move Speed, Damage Multiplier
- Starter Weapon: WeaponData reference
- Character Prefab: GameObject with all player components
- Unlock Type: FreeStarter / Gold / Ads
```

### Stat Loading
```csharp
PlayerController.LoadCharacterStats() → sets moveSpeed
PlayerHealth.LoadCharacterStats() → sets maxHealth
WeaponManager.Start() → equips starterWeapon
WeaponBase.GetFinalDamage() → applies baseDamageMultiplier
```

---

## 👾 ENEMY SYSTEM

### EnemyAI
```csharp
// Chases: PlayerController.Instance.transform
// Movement: Simple Vector2.MoveTowards
// Tag: "Enemy" (required for weapon collision)
```

### EnemySpawner
```csharp
// Spawns: At screen edges (off-camera)
// Difficulty: Scales over time (more frequent, more HP)
// Continuous: Every spawnInterval seconds
```

### EnemyHealth
```csharp
// Damage: TakeDamage(damage, knockback direction, knockback force)
// Death: Spawns XP gem
// Tag: "Enemy" (required)
```

---

## 📈 PROGRESSION

### XPGem
```csharp
// Magnet: Attracted to player when in range
// Collection: Triggers GameEvents.TriggerXPGained()
// Visual: All gems identical (cyan square) - NO size/color differentiation by value
// Note: Future enhancement - differentiate by size/particles/color based on XP amount
```

### Loot Drops
```
✅ XP Gems: Fully implemented (left-up offset, magnet pickup)
✅ Gold Drops: Fully implemented (right-up offset, magnet pickup, chance-based)
✅ Health Potions: Fully implemented (prefab, chance-based, smart pickup)
```

### Leveling
```
XP Required = level * 10
Level up → Pause game → Show LevelUpPanel → Select upgrade → Resume
```

---

## 🎨 UI SYSTEM

### HUD
```csharp
// Shows: HP bar, XP bar, Level, Timer, Kill count
// Updates: Via event listeners
// Platform: Visible on all platforms
```

###LevelUpPanel
```csharp
// Shows: 3 random upgrade cards
// Pauses: Game while selecting
// Cards: Generated from UpgradeManager.GetRandomUpgrades()
```

### PauseMenu
```csharp
// Toggle: ESC (desktop) or button (mobile)
// Platform-aware: Button visibility
// State: Uses GameManager.SetPaused(true/false)
```

### GameOverScreen
```csharp
// Shows: Final stats (time, kills, XP)
// Data: From GameSession
// Actions: Restart scene
```

---

## 🎮 INPUT SYSTEM

### Platform Detection
```csharp
// PlatformDetector singleton
// Auto-detects: Mobile vs Desktop
// Force modes: AlwaysMobile, AlwaysDesktop (for testing)
```

### VirtualJoystick
```csharp
// Type: Floating (appears where touched)
// Raycast blocking: Ignores UI elements
// Visibility: Mobile only
```

---

## 💾 SAVE SYSTEM (Architecture Only)

### ISaveService
```csharp
// Interface for platform-specific saves
// Implementations: PlayerPrefsSaveService (local), YandexSaveService (cloud)

SaveData structure:
- High scores
- Stat levels (Dictionary → List conversion for Unity JSON)
- Ad watch progress
```

### GameSession
```csharp
// Runtime stats: playTime, totalKills, totalXPGained
// NOT persistent (resets each run)
```

---

## 🏗️ ARCHITECTURE PATTERNS

### 1. Service Locator
```csharp
ServiceLocator.Get<ISaveService>()
ServiceLocator.Get<IPlatform Service>()
```

### 2. Singleton
```csharp
PlayerController.Instance
GameManager.Instance
PlatformDetector.Instance
```

### 3. ScriptableObject Data
```csharp
WeaponData, UpgradeData, EnemyData (all create via Assets menu)
```

### 4. Event System
```csharp
PlayerHealth.OnPlayerDeath
PlayerController.OnLevelUp
```

---

## 📁 CRITICAL FILE LOCATIONS

### Scripts
```
Assets/Scripts/
├── Core/
│   ├── ServiceLocator.cs
│   └── GameManager.cs
├── Data/
│   ├── WeaponData.cs (+ enum WeaponType)
│   └── UpgradeData.cs (+ enum UpgradeType)
├── Gameplay/
│   ├── Player/
│   │   ├── PlayerController.cs (SINGLETON)
│   │   └── PlayerHealth.cs
│   ├── Weapons/
│   │   ├── WeaponBase.cs (ABSTRACT)
│   │   ├── GreatswordWeapon.cs
│   │   ├── AutoCrossbowWeapon.cs
│   │   ├── HolyWaterWeapon.cs
│   │   ├── MagicOrbitalsWeapon.cs
│   │   ├── ArrowProjectile.cs
│   │   ├── DamagePuddle.cs
│   │   └── WeaponManager.cs
│   ├── EnemyAI.cs
│   ├── EnemyHealth.cs
│   ├── EnemySpawner.cs
│   ├── UpgradeManager.cs
│   └── XPGem.cs
└── UI/
    ├── HUD.cs
    ├── LevelUpPanel.cs
    ├── PauseMenu.cs
    └── GameOverScreen.cs

Assets/Shaders/
├── SpriteReveal.shader     - Horizontal/Vertical clip reveal (Greatsword)
└── RadialReveal.shader     - Center-outward clip reveal (Holy Water)
```

### Assets
```
Assets/ScriptableObjects/
├── Weapons/ (4 assets)
│   ├── Greatsword.asset
│   ├── AutoCrossbow.asset
│   ├── HolyWater.asset
│   └── MagicOrbitals.asset
└── Upgrades/ (4 assets)
    ├── SwiftFeet.asset
    ├── VitalityBoost.asset
    ├── SpinningFury.asset
    └── SharpBlade.asset
```

---

## 🔧 KEY IMPLEMENTATION DETAILS

### Tags Required
- Player: `"Player"`
- Enemies: `"Enemy"`
- NO tags needed for weapons/projectiles

### Sorting Layers (Order 0 unless specified)
1. **Background**
2. **Ground** (-1 to 2)
3. **Shadows** (NEW)
4. **Ground Effects** (Puddles)
5. **Enemies**
6. **Player**
7. **Collectibles** (XP, Gold)
8. **Flying Objects** (Trees)
9. **Weapons** (Arrows=5, Orbitals=8, Slash=10)
10. **Effects** (VFX)
11. **UI**

### Prefabs Structure
```
Assets/Prefabs/Weapons/
├── GreatswordWeapon.prefab (has sprite, collider, script)
├── MagicOrbitalsWeapon.prefab (has sprite, collider, script)
├── AutoCrossbowWeapon.prefab (empty, just script)
├── HolyWaterWeapon.prefab (empty, just script)
├── ArrowProjectile.prefab (sprite, rigidbody, collider)
└── DamagePuddle.prefab (sprite, collider)
```

---

## 🐛 COMMON ISSUES & FIXES

### "PlayerController.Instance not found"
**Fix:** Player must have PlayerController component with Awake() setting Instance

### "Missing Script on prefab"
**Fix:** Assign correct weapon script (GreatswordWeapon, AutoCrossbowWeapon, etc.)

### "Weapons don't spawn"
**Fix:** WeaponManager needs all 4 prefabs assigned + 4 WeaponData assets in Available Weapons

### "Upgrades only affect Greatsword"
**Fix:** ✅ FIXED - Now uses WeaponManager.GetEquippedWeapons() loop

### "Collisions not working"
**Fix:** Ensure "Is Trigger" checked on weapon/projectile colliders

---

## 📊 IMPLEMENTATION STATUS

### ✅ Complete (Core Gameplay)
- Player movement & health
- **Character system** (selection, stat loading, spawning)
- 4 weapon types (all functional)
- Enemy AI & spawning
- XP & leveling system
- Upgrade system (4 types)
- HUD, pause, game over
- Platform-aware input
- Camera follow (Cinemachine auto-assignment)

### 🔨 Code Only (Unity Setup Needed)
- Character variety (create 3 CharacterData assets)
- Enemy visuals (sprites/animations pending import)

### ⏳ Not Started
- Main menu scene
- Persistent upgrades / meta-progression
- Achievement system
- Sound/Music
- Map bounds
- Visual polish (VFX/particles)


---

## 🎓 DESIGN PHILOSOPHY

### Weapon System
- **Type = Behavior:** Enum determines which script/prefab to use
- **Data-Driven:** All stats in WeaponData ScriptableObjects
- **Inheritance:** WeaponBase provides common functionality
- **Manager Pattern:** WeaponManager orchestrates all weapons

### Upgrade System
- **Universal:** Weapon upgrades apply to ALL equipped weapons
- **Fair:** No weapon-specific bias
- **Scalable:** Adding weapons doesn't require UpgradeManager changes

### Input
- **Platform-Aware:** Auto-detects and switches
- **State-Respecting:** Checks GameManager.CurrentState before accepting input
- **Flexible:** Can force platform for testing

---

## 🚀 NEXT STEPS (When Resuming)

1. **Unity Setup:** Complete weapon prefab/asset assignment in WeaponManager
2. **Testing:** Test all 4 weapons + upgrades
3. **Enemy Variety:** Create 2-3 more enemy types
4. **Main Menu:** Scene + UI
5. **Polish:** VFX, SFX, particles

---

## 📝 RECENT MAJOR CHANGES

### 2025-12-14 Session
1. **Weapon Naming:** Standardized all to Greatsword, AutoCrossbow, HolyWater, MagicOrbitals
2. **WeaponType Enum:** Changed from behavior-based (OrbitingMelee) to weapon-specific (Greatsword)
3. **Greatsword Behavior:** Redesigned from continuous orbit to periodic swing
4. **UpgradeManager Fix:** Now upgrades ALL weapons, not just Greatsword
5. **WeaponBase API:** Added IncreaseDamage(), IncreaseAttackSpeed(), IncreaseKnockback()
### 2025-12-21 Session
1. **Animation System:**
   - Player: `IsMoving` + `FacingRight` (4 states)
   - Enemy: `FacingRight` only (2 states), direction-based (fixes knockback)
2. **Loot System:**
   - Implemented GoldCoin spawning with magnet
   - Added offset system to prevent XP/Gold overlap
   - Added `goldDropChance` configuration
3. **Sorting Layers:**
   - Defined 11-layer hierarchy including Shadows, Ground Effects, Collectibles
4. **Enemy Spawning:**
   - Added validation to prevent spawning in isolated/blocked areas
5. **Health Potions:**
   - Implemented `HealthPotion.cs` with smart pickup (only if damaged)
   - Refactored loot system to use Prefabs



---

**This document is the MASTER REFERENCE for understanding the entire system!**
