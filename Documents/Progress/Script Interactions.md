# Script Interactions - System Communication

**Purpose:** How different scripts communicate

## Core Communication Patterns

### 1. Events (Decoupled)

**GameEvents.cs** - Static event bus for system-wide notifications:
```csharp
// Triggers
GameEvents.TriggerPlayerHealthChanged(currentHP, maxHP);
GameEvents.TriggerXPGained(amount);
GameEvents.TriggerPlayerDeath();
GameEvents.TriggerEnemyKilled(enemyData);

// Listeners
GameEvents.OnPlayerHealthChanged += UpdateHealthBar;
GameEvents.OnXPGained += AddXP;
GameEvents.OnPlayerDeath += ShowGameOver;
```

**Usage:**
- UI updates (HP bar, XP bar)
- Game state changes (death → game over)
- Stat tracking (kills, damage dealt)

### 2. Event-Driven (Interface Events)

**IDamageable Interface** - Component-level events for damage feedback:
```csharp
// Interface
public interface IDamageable
{
    event Action<float, Vector3> OnDamaged;
    event Action<float, Vector3> OnHealed;
    bool IsAlive { get; }
}

// Implementation (EnemyHealth, PlayerHealth)
public event Action<float, Vector3> OnDamaged;
OnDamaged?.Invoke(damage, transform.position);

// Subscribers (DamageFlash, DamageNumberListener)
private void OnEnable()
{
    damageable.OnDamaged += HandleDamage;
}
```

**Benefits:**
- Complete decoupling (health ↔ VFX independent)
- Easy to add/remove feedback systems
- No manual triggering required
- Testable in isolation

**Usage:**
- Damage flash effects
- Floating damage numbers
- Hit sounds (future)
- Screen shake (future)

### 3. Direct References (Performance-Critical)

**Singletons:**
- `PlayerController.Instance` - Accessed by weapons for positioning
- `GameManager.Instance` - Game state queries (`CurrentState`)
- `CharacterManager.SelectedCharacter` - Character data access

**Component Queries:**
- Weapons find player via `FindObjectOfType<PlayerController>()`
- Stored as cached reference in `Awake()`

### 3. ScriptableObject Data Flow

```
CharacterData → CharacterManager → Player Components
WeaponData → WeaponManager → WeaponBase
EnemyData → EnemySpawner → Enemy Components
UpgradeData → UpgradeManager → Player/Weapons
```

**Benefits:** Designer-friendly tuning, no code changes for balance

## System-to-System Communication

### Player → Weapons
- Weapons access `PlayerController.Instance.transform` for positioning
- Weapons call `GetFinalDamage()` which queries `CharacterManager.SelectedCharacter.baseDamageMultiplier`

### Upgrades → Player
- `UpgradeApplicator` calls `PlayerController.SetMoveSpeed()`
- `UpgradeApplicator` calls `PlayerHealth.IncreaseMaxHealth()`

### Upgrades → Weapons
- `UpgradeApplicator` queries `WeaponManager.GetEquippedWeapons()`
- Calls `weapon.IncreaseDamage()` or weapon-specific methods
- For NewWeapon: `WeaponManager.EquipWeapon(weaponData)`

### Weapons → Enemies
- Direct collision detection (`OnTriggerStay2D`)
- Direct method call: `enemyHealth.TakeDamage(damage, knockbackDir, force)`
- No utility classes - weapons call enemy methods directly

### Enemies → Player
- Collision: `OnCollisionStay2D` → `PlayerHealth.TakeDamage()`
- Loot drops: Instantiate collectibles on death

### Collectibles → Player
- Trigger: `OnTriggerEnter2D` detects player tag
- Event: `GameEvents.TriggerXPGained(amount)`
- Direct: Health potion calls `PlayerHealth.Heal(amount)`

### UI → Game Systems
- `LevelUpPanel` calls `UpgradeManager.GetRandomUpgrades()`
- Player selects → calls `UpgradeManager.ApplyUpgrade(upgrade)`
- `HUD` subscribes to `GameEvents` for updates

## Dependency Flow

```
GameManager (Top Level)
    └─► Player Systems
        ├─► WeaponManager → Weapons
        │       └─► Enemies
        └─► UpgradeManager → Player/Weapons
            ├─► UpgradeTracker
            ├─► UpgradeValidator → WeaponManager
            └─► UpgradeApplicator → All systems

Services (ServiceLocator)
    ├─► SaveService (any system)
    ├─► AdService (UI)
    └─► PlatformService (Input)
```

## Anti-Patterns Avoided

**❌ NO:** Central "Manager of Managers"  
**✅ YES:** Decoupled via events + selective singletons

**❌ NO:** God objects  
**✅ YES:** Single responsibility (Player split into 4 components, UpgradeManager split into 4 classes)

**❌ NO:** Tight coupling  
**✅ YES:** Dependency injection where appropriate (UpgradeApplicator receives references)

## Cross-Scene Communication

**Not Applicable:** Single-scene game (GameScene only)  
**Future:** If multiple scenes added, use `DontDestroyOnLoad` + events
