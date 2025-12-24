# Pixel Vanguard - System Architecture

**Purpose:** Master technical reference for AI/developers  
**Last Updated:** 2024-12-24  
**Status:** Core systems complete, documentation consolidated

> 💡 **Quick Start:** Read Core Loop → Player System → Weapon System → Upgrade System

---

## 🎮 Game Overview

**Genre:** Horde Survivor / Action Roguelite (Vampire Survivors-like)  
**Platform:** Unity 2D (Desktop + Mobile)  
**Architecture:** Component-based + Service Locator + Event Bus + ScriptableObject Data

### Core Loop
```
Spawn → Fight → Collect XP → Level Up → Choose Upgrade → Repeat → Die → Stats → Restart
```

### Technology Stack
- **Engine:** Unity 2022+ (2D URP)
- **Input:** New Input System + Virtual Joystick (mobile)
- **Camera:** Cinemachine Virtual Camera
- **Pattern:** Service Locator for cross-cutting concerns

---

## 📊 System Architecture

### 9 Core Systems
1. **Player** - Movement, health, input, animations (4 components)
2. **Characters** - Selection, stat loading, spawning  
3. **Weapons** - 4 types, auto-fire, upgrade system
4. **Enemies** - AI, waves spawning, loot drops
5. **Progression** - XP/Gold collection, leveling
6. **Upgrades** - 18 types with rarity weighting
7. **UI** - HUD, menus, platform-aware joystick
8. **Services** - Save/load, platform detection
9. **Camera** - Cinemachine player follow

---

## 👤 PLAYER SYSTEM (REFACTORED DEC 24)

### Architecture: Split into 4 Components

#### PlayerController (Singleton)
```csharp
// Purpose: Central reference point for weapons and systems
// Location: Assets/Scripts/Gameplay/Player/PlayerController.cs

public static PlayerController Instance { get; private set; }
public Transform transform; // Accessed by weapons for positioning
```

**Responsibilities:**
- Provides singleton instance
- Coordinates other player components
- Houses player transform reference

---

#### PlayerMovement
```csharp
// Purpose: Rigidbody2D-based movement logic
// Location: Assets/Scripts/Gameplay/Player/PlayerMovement.cs

private Rigidbody2D rb;
private float moveSpeed;

void FixedUpdate()
{
    Vector2 movement = playerInput.GetMovementInput();
    rb.linearVelocity = movement * moveSpeed;
}
```

**Responsibilities:**
- Rigidbody2D physics movement
- Speed modifiers from upgrades
- Diagonal movement normalization

---

#### PlayerInput
```csharp
// Purpose: Platform-aware input handling
// Location: Assets/Scripts/Gameplay/Player/PlayerInput.cs

private PlayerInputActions inputActions; // New Input System
private VirtualJoystick virtualJoystick;
private PlatformDetector platformDetector;

Vector2 GetMovementInput()
{
   if (platformDetector.IsMobile()) 
        return virtualJoystick.Direction;
    return inputActions.Player.Move.ReadValue<Vector2>();
}
```

**Responsibilities:**
- New Input System integration (WASD/Arrows)
- Virtual Joystick for mobile
- Platform detection and switching
- State-aware input (blocks during pause/levelup)

---

#### PlayerHealth
```csharp
// Purpose: HP management and passive upgrade storage
// Location: Assets/Scripts/Gameplay/Player/PlayerHealth.cs

private int currentHealth;
private int maxHealth;

// Passive upgrade storage
public float characterDamageMultiplier = 1f; // Might upgrade
public float lifestealPercent = 0f;         // Lifesteal
public float goldBonusMultiplier = 1f;       // Lucky Coins
```

**Responsibilities:**
- HP tracking and damage cooldown
- Stores passive upgrade values
- Triggers OnPlayerDeath event

---

## 🗡️ WEAPON SYSTEM

### 4 Weapon Types

| Weapon | Script | Behavior |
|--------|--------|----------|
| Greatsword | GreatswordWeapon.cs | Periodic 360° swing with shader reveal |
| AutoCrossbow | AutoCrossbowWeapon.cs | Fires arrows at unique targets |
| HolyWater | HolyWaterWeapon.cs | Spawns DoT damage puddles |
| MagicOrbitals | MagicOrbitalsWeapon.cs | Shields orbit player continuously |

### WeaponBase (Abstract Parent)
```csharp
// All weapons inherit from this base class

protected float damage;      // Base damage (from WeaponData)
protected float cooldown;     // Fire rate
protected float knockback;    // Knockback force

// Auto-fire system
protected virtual void Update()
{
    if (cooldownTimer <= 0f) {
        Fire();
        cooldownTimer = cooldown;
    }
    cooldownTimer -= Time.deltaTime;
}

// Upgrade API
public virtual void IncreaseDamage(float multiplier);
public virtual void IncreaseAttackSpeed(float multiplier);
public virtual void IncreaseKnockback(float multiplier);

// Damage calculation (includes character multiplier)
protected float GetFinalDamage()
{
    float characterMultiplier = CharacterManager.SelectedCharacter.baseDamageMultiplier;
    characterMultiplier *= PlayerHealth.characterDamageMultiplier; // Might upgrade
    return damage * characterMultiplier;
}
```

**Key Pattern:** All weapons call `EnemyHealth.TakeDamage()` directly

---

### WeaponManager (Orchestrator)
```csharp
// Location: Player GameObject
// Manages up to 4 equipped weapons simultaneously

Key Methods:
- EquipWeapon(WeaponData) → Instantiate + track
- UpgradeWeapon(weaponID) → Level up specific weapon  
- GetEquippedWeapons() → List<WeaponInstance>
- IsWeaponEquipped(WeaponType) → bool
```

---

## 🎯 UPGRADE SYSTEM

### 18 Upgrade Types

#### Universal (4)
- **Move Speed** - Increases player movement
- **Max HP** - Increases health pool
- **Weapon Damage** - Boosts ALL weapons
- **Attack Speed** - Faster cooldowns for ALL weapons

#### New Weapons (4)
- Greatsword, AutoCrossbow, HolyWater, MagicOrbitals

#### Greatsword (3)
- **Mirror Slash** - Spawns 2nd sword at 180°
- **Executioner's Edge** - +50% damage
- **Berserker Fury** - -30% cooldown

#### AutoCrossbow (3)
- **Dual Shot** - Fires 2 arrows at unique targets
- **Triple Barrage** - Fires 3 arrows
- **Piercing Bolts** - Arrows pierce through enemies

#### HolyWater (3)
- **Sanctified Expansion** - +40% puddle radius
- **Burning Touch** - +6% of enemy max HP as damage
- **Eternal Flame** - 2x puddle duration

#### MagicOrbitals (2)
- **Expanded Orbit** - +50% orbit radius
- **Overcharged Spheres** - +100% damage

#### Passive (3, max 3 selected)
- **Lifesteal** - 10% healing on hit (stored, not integrated yet)
- **Magnet** - +100% XP/Gold/Potion pickup range
- **Lucky Coins** - +80% gold from enemies (stored, not applied yet)

---

### Rarity System
```csharp
Rarity Weight Distribution:
- Common:    100 weight (~53% chance)
- Uncommon:   50 weight (~26% chance)  
- Rare:       25 weight (~13% chance)
- Epic:       10 weight (~5% chance)
- Passive:    30 weight (~16% chance)

Selection Algorithm:
1. Filter valid upgrades (not already applied, weapon ownership)
2. Calculate totalWeight = sum(upgrade.rarityWeight)
3. Pick random value [0, totalWeight)
4. Return first upgrade where accumulatedWeight > randomValue
```

---

### UpgradeManager (Implementation)
```csharp
// Filters and applies upgrades to ALL equipped weapons

Key Flow:
1. GetRandomUpgrades(3) → Returns 3 weighted, validated upgrades
2. ApplyUpgrade(UpgradeData) → Distributes to correct system
3. Tracks applied upgrades (prevents duplicates)

Weapon-Specific Pattern:
if (weaponScript is AutoCrossbowWeapon crossbow) {
    crossbow.SetMultiShot(arrowCount);
}
```

---

## 👾 ENEMY SYSTEM

### EnemySpawner
- Spawns at screen edges based on game time
- Weighted spawn rates per enemy type
- Uses `EnemyData` ScriptableObjects

### EnemyAI
- Simple chase behavior (moves toward player)
- Direct vector pathfinding (no nav mesh)
- Pauses when game paused or dead

### EnemyHealth
```csharp
// Health, knockback, loot drops

void TakeDamage(float damage, Vector2 knockbackDir, float knockbackForce)
{
    currentHealth -= damage;
    ApplyKnockback(knockbackDir, knockbackForce);
    if (currentHealth <= 0) {
        DropLoot();
        Destroy(gameObject);
    }
}

float actualKnockback = knockbackForce * (1f - enemyData.weightResistance);
rb.AddForce(knockbackDirection * actualKnockback, ForceMode2D.Force);
```

### EnemyData (ScriptableObject)
```csharp
public class EnemyData : ScriptableObject
{
    [Header("Stats")]
    public int maxHealth;
    public float moveSpeed;
    public float damage;
    public float weightResistance; // 0-1, knockback resistance
    
    [Header("Loot")]
    public float xpDropAmount;
    public float goldDropChance;
    public float healthPotionDropChance;
    
    [Header("Loot Prefabs")]
    public GameObject xpGemPrefab;
    public GameObject goldCoinPrefab;
    public GameObject healthPotionPrefab;
    
    [Header("Spawning")]
    public int spawnWeight;
    public float minGameTimeSeconds;
    public GameObject prefab;
}
```

---

## 🌟 PROGRESSION SYSTEM

### XP & Leveling
```csharp
XP Required = level * 10
Level 1: 10 XP
Level 2: 20 XP
Level 3: 30 XP
...
```

### Collectibles

**XPGem**
- Magnet range: 3f default, upgradeable
- Auto-collects on proximity
- Grants XP based on enemy type

**GoldCoin**
- Magnet range: 3f default
- Grants gold for shop (future feature)
- Drop rates vary by enemy

**HealthPotion**
- Smart pickup: Only if damaged
- Magnet range: 2f default
- Restores 25 HP

---

## 🎨 Passive Upgrades Integration

### Storage Locations
```csharp
// PlayerHealth.cs
public float characterDamageMultiplier = 1f; // Might upgrade
public float lifestealPercent = 0f;          // Lifesteal
public float goldBonusMultiplier = 1f;       // Lucky Coins

// XPGem.cs / GoldCoin.cs / HealthPotion.cs
private float magnetRange = 3f;              // Magnet upgrade
```

### Integration Status
✅ **Magnet** - Fully integrated (increases pickup range)  
⚠️ **Lifest eal** - Stored, needs integration in weapon hit events  
⚠️ **Gold Bonus** - Stored, needs application in `EnemyHealth.DropLoot()`

---

## 🎮 INPUT SYSTEM

### Platform Detection
```csharp
PlatformDetector.IsMobile() → bool

Desktop: WASD + Arrow keys (New Input System)
Mobile:  Virtual Joystick (floating anchor)
```

### Virtual Joystick
- Appears on first touch
- Parent-space coordinates
- Auto-hides on release
- Disables during pause/levelup

---

## 🏗️ Service Architecture

### ServiceLocator Pattern
```csharp
ServiceLocator.Register<ISaveService>(new PlayerPrefsSaveService());
var saveService = ServiceLocator.Get<ISaveService>();
```

**Available Services:**
- `ISaveService` - Player prefs save/load
- `IAdService` - Ad integration (NoAdService default)
- `IPlatformService` - Platform-specific features

---

## 📁 File Structure

```
Assets/Scripts/
├── Core/ (4 files)
│   ├── CharacterManager.cs
│   ├── GameEvents.cs
│   ├── PlatformDetector.cs
│   └── ServiceLocator.cs
│
├── Data/ (4 ScriptableObject definitions)
│   ├── CharacterData.cs
│   ├── EnemyData.cs
│   ├── UpgradeData.cs
│   └── WeaponData.cs
│
├── Gameplay/ (26 files)
│   ├── Player/ (4 - REFACTORED DEC 24)
│   │   ├── PlayerController.cs (Singleton)
│   │   ├── PlayerMovement.cs (NEW)
│   │   ├── PlayerInput.cs (NEW)
│   │   ├── PlayerHealth.cs
│   │   └── PlayerAnimationController.cs
│   │
│   ├── Weapons/ (10)
│   │   ├── WeaponBase.cs
│   │   ├── WeaponManager.cs
│   │   ├── GreatswordWeapon.cs
│   │   ├── AutoCrossbowWeapon.cs
│   │   ├── HolyWaterWeapon.cs
│   │   ├── MagicOrbitalsWeapon.cs
│   │   ├── ArrowProjectile.cs
│   │   ├── DamagePuddle.cs
│   │   ├── OrbitalBall.cs
│   │   ├── ShaderHelper.cs
│   │   └── TargetingUtility.cs
│   │
│   ├── Enemies/ (4)
│   │   ├── EnemyAI.cs
│   │   ├── EnemyHealth.cs
│   │   ├── EnemySpawner.cs
│   │   └── EnemyAnimationController.cs
│   │
│   ├── GameManager.cs
│   ├── GameSession.cs
│   ├── UpgradeManager.cs
│   ├── XPGem.cs
│   ├── GoldCoin.cs
│   └── HealthPotion.cs
│
├── UI/ (5 files)
│   ├── HUD.cs
│   ├── LevelUpPanel.cs
│   ├── PauseMenu.cs
│   ├── GameOverScreen.cs
│   └── VirtualJoystick.cs
│
├── Services/ (6 files)
│   ├── Interfaces: IAdService, ISaveService, IPlatformService
│   └── Implementations: SaveData, PlayerPrefsSaveService, NoAdService
│
└── Utils/ (1 file)
    └── AutoFPS60Setter.cs
```

---

## 🎯 Design Patterns & Conventions

### Singletons
- **GameManager** - Game state machine
- **PlayerController** - Player reference
- **CharacterManager** - Character spawning

### ScriptableObjects
- All configuration data (Characters, Weapons, Enemies, Upgrades)
- Enables designer-friendly tuning

### Events
- `GameEvents` static class for decoupled communication
- UI updates via events (no direct references)

### Code Style
- **Null-safe:** Use `?.` operator
- **Early returns:** Avoid deep nesting
- **Protected fields:** Weapon stats (inheritance-friendly)
- **XML docs:** On public APIs

---

## 📖 Related Documentation

| Topic | Document |
|-------|----------|
| Recent Changes | [CHANGELOG.md](CHANGELOG.md) |
| Feature Specs | [Features Specification.md](Features%20Specification.md) |
| Data Models | [GDD - Data Models.md](GDD/GDD%20-%20Data%20Models.md) |
| Upgrade Details | [GDD - Upgrade System.md](GDD/GDD%20-%20Upgrade%20System.md) |
| Implementation Status | [Progress/Implementation Status.md](Progress/Implementation%20Status.md) |
| Current Systems | [Progress/Current Systems.md](Progress/Current%20Systems.md) |

---

**Last Updated:** 2024-12-24  
**Maintainer:** Development Team  
**Status:** Production-ready, pending passive upgrade integration
