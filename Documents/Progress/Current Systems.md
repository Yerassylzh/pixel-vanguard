# Current Systems - Implementation Status

**Purpose:** Quick reference for what's implemented and how systems work

## ✅ Player Systems

**Components (4):**
- `PlayerController` - Singleton, GameManager integration, speed getters/setters
- `PlayerMovement` - Rigidbody2D velocity-based movement
- `PlayerInput` - New Input System + Virtual Joystick (desktop WASD, mobile joystick)
- `PlayerHealth` - HP tracking, passive effects storage (lifesteal%, goldBonus%), damage cooldown

**Character System:**
- `CharacterData` (ScriptableObject) - Stats (moveSpeed, maxHP, damageMultiplier), starter weapon
- `CharacterManager` - Spawns selected character at runtime, configures Cinemachine follow

**Current Balance:**
- Knight: Speed 5.0, HP 100, Damage 1.0x, Starter: Greatsword

## ✅ Weapon Systems

**4 Weapon Types:**
- **Greatsword** - 360° periodic swing with shader fill animation, knockback 100
- **AutoCrossbow** - Multi-target arrows (Dual/Triple upgrade), pierce support, knockback 45
- **HolyWater** - DoT puddles with HP scaling, knockback 20
- **MagicOrbitals** - Continuous orbit shields, knockback 50

**WeaponBase:**
- Auto-fire system (cooldown-based `Update()`)
- Upgrade API: `IncreaseDamage(multiplier)`, `IncreaseAttackSpeed(multiplier)`
- `GetFinalDamage()` - Applies character damage multiplier
- Cloning support (`CopyStatsFrom()`, `isClone` flag)

**WeaponManager:**
- Max 4 equipped weapons simultaneously
- `EquipWeapon(weaponData)` - Instantiates prefab, tracks instance
- `SpawnMirrorGreatsword()` - For Mirror Slash upgrade (spawns synchronized copy)
- `GetEquippedWeapons()` - Returns list for upgrade application

## ✅ Upgrade System

**Architecture (Refactored Dec 2024):**
```
UpgradeManager (Orchestrator)
    ├─► UpgradeTracker (State)
    ├─► UpgradeValidator (Validation)
    └─► UpgradeApplicator (Effects)
```

**18 Upgrade Types:**
- 4 Repeatable: Speed, HP, Damage, Attack Speed
- 4 Weapons: Greatsword, Crossbow, HolyWater, Orbitals
- 10 Weapon-Specific: Mirror Slash, Dual/Triple Shot, etc.
- 3 Passives (max 3): Lifesteal, Magnet, Lucky Coins

**Features:**
- Weighted rarity selection
- Repeatable stats (infinite)
- Prerequisite support (Triple requires Dual)
- Passive limit (max 3 total)
- Weapon-specific filtering

## ✅ Enemy Systems

**Components:**
- `EnemySpawner` - Wave-based spawning at screen edges, difficulty scaling
- `EnemyAI` - Simple chase behavior (moves toward player)
- `EnemyHealth` -  HP, knockback resistance, loot drops (XP, gold, potions)
- `EnemyData` (ScriptableObject) - Stats, drop rates, spawn weights

**6 Enemy Types:**
Skeleton, Crawler, Goblin, Ghost, ArmoredOrc, Slime

**Drop Rates (Adjusted Dec 2024):**
- XP: Always drops (amount varies by enemy)
- Gold: 25%-80% depending on enemy type
- Health Potion: 3%-10% depending on enemy type

**Difficulty Scaling:**
- `difficultyIncreaseRate: 0.1f` (10% faster spawns per minute)
- `maxSpawnRate: 10f` (caps at 10x speed)

## ✅ Progression Systems

**XP & Leveling:**
- Formula: `XP Required = level × 10`
- `LevelUpPanel` - Shows 3 random upgrades
- `XPGem` - Magnet range 3f (upgradeable), auto-collect

**Gold:**
- `GoldCoin` - Magnet range 3f, drops from enemies
- Future: Shop system

**Health Potions:**
- Smart pickup: Only if damaged
- Magnet range 2f
- Restores 25 HP
- No magnet pull if player at max HP

## ✅ Input Systems

**Platform Detection:**
- `PlatformDetector.IsMobile()` - True if mobile build
- Desktop: WASD + Arrow keys (New Input System)
- Mobile: Virtual Joystick (floating, appears on touch)

**Joystick Behavior:**
- Disabled during pause/levelup
- Parent-space coordinates
- Auto-hides on release

## ✅ Camera System

**Cinemachine Integration:**
- Virtual Camera follows spawned player
- Graceful fallback if Cinemachine not installed
- Auto- configured by `CharacterManager`

## ✅ Service Architecture

**ServiceLocator Pattern:**
- `ISaveService` - PlayerPrefs save/load
- `IAdService` - Ad integration (NoAdService default)
- `IPlatformService` - Platform-specific features

## ⚠️ Partial Integration

**Lifesteal:**
- Storage: `UpgradeTracker` → exposed via `UpgradeManager.GetLifestealPercent()`
- Integration: Needs weapon hit events to apply healing

**Gold Bonus:**
- Storage: `UpgradeTracker` → exposed via `UpgradeManager.GetGoldBonusPercent()`
- Integration: Needs application in `EnemyHealth.DropLoot()`

## 📁 File Structure

```
Assets/Scripts/
├── Core/ - CharacterManager, GameEvents, PlatformDetector, ServiceLocator
├── Data/ - ScriptableObject definitions (4 types)
├── Gameplay/
│   ├── Player/ - PlayerController, PlayerMovement, PlayerInput, PlayerHealth, PlayerAnimationController
│   ├── Weapons/ - WeaponBase, WeaponManager, 4 weapon types, projectiles, utilities
│   ├── Enemies/ - EnemyAI, EnemyHealth, EnemySpawner, EnemyAnimationController
│   ├── Upgrades/ - UpgradeManager, UpgradeTracker, UpgradeValidator, UpgradeApplicator
│   └── Collectibles - XPGem, GoldCoin, HealthPotion, GameManager, GameSession
├── UI/ - HUD, LevelUpPanel, PauseMenu, GameOverScreen, VirtualJoystick
├── Services/ - Interfaces + Implementations
└── Utils/ - AutoFPS60Setter
```

## Design Patterns

**Singletons:** GameManager, PlayerController, CharacterManager  
**ScriptableObjects:** All configuration data (designer-friendly)  
**Events:** `GameEvents` static class for decoupled communication  
**Service Locator:** Cross-cutting concerns (save, ads, platform)

## Code Conventions

- Null-safe operators (`?.`)
- Early returns (avoid deep nesting)
- Protected fields in weapon classes (inheritance-friendly)
- XML docs on public APIs
