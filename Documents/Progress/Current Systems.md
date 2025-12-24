# Current Systems

**Implemented system overview - concise technical reference**

---

## Core Managers

### GameManager
- Master game state controller (Singleton, DontDestroyOnLoad)
- **States:** Initializing → Playing → Paused → LevelUp → GameOver
- **Tracks:** Time, kills, gold
- **Events:** Listens to OnPlayerDeath, OnEnemyKilled, OnGoldCollected

### PlayerController
- Platform-aware movement with auto-detection
- **Desktop:** WASD + Arrow keys (Input Actions)
- **Mobile:** VirtualJoystick (floating touch)
- **Speed:** 5.0 units/sec, diagonal normalized
- **State Blocking:** Pauses input during level-up/game-over

### WeaponManager
- Manages up to 4 equipped weapons simultaneously
- **Spawning:** Instantiates weapon prefabs, initializes with WeaponData
- **API:** `EquipWeapon(WeaponData)`, `GetEquippedWeapons()`, `IsWeaponEquipped(WeaponType)`

### UpgradeManager
- Applies universal upgrades to ALL equipped weapons + player stats
- **Formula Conversion:**
  - Attack Speed: `cooldown *= (1.0 - value/100)` (10% = faster)
  - Damage: `damage *= (1.0 + value/100)` (10% = +10% dmg)
  - Speed: `speed *= (1.0 + value/100)`
- **Method:** `ApplyUpgrade(UpgradeData)` distributes to weapons or player

---

## Player Systems (REFACTORED DEC 24)

### PlayerController (Singleton)
- Central reference point for weapons and systems
- **Access:** `PlayerController.Instance.transform`
- **Coordinates:** Other player components

### PlayerMovement
- Rigidbody2D-based movement logic  
- **Speed:** 5.0 units/sec default, upgradeable
- **Diagonal normalized:** Prevents faster diagonal movement
- **State blocking:** Pauses during level-up/game-over

### PlayerInput
- New Input System integration
- **Desktop:** WASD + Arrow keys (PlayerInputActions)
- **Mobile:** VirtualJoystick (floating touch)
- **Platform detection:** Auto-switches based on `PlatformDetector.IsMobile()`

### PlayerHealth
- **HP:** 100 default (from CharacterData.maxHealth)
- **Damage Cooldown:** 1 second
- **Passive Storage:** lifestealPercent, goldBonusMultiplier, characterDamageMultiplier
- **Triggers:** OnPlayerDeath when HP ≤ 0

### VirtualJoystick
- Floating touch controls (appears at touch, hides on release)
- **Range:** 50 units (configurable)
- **State Management:** Auto-disables during pause/level-up
- **Platform:** Invisible on desktop, responds to platform changes

---

## Weapon System

### Universal Architecture
- **WeaponBase** - Abstract base class
  - Auto-fire system (cooldown timer in Update())
  - Stats: `damage`, `cooldown`, `knockback` (duration/tickRate are weapon-specific)
  - `GetFinalDamage()` applies character damage multiplier
  - Direct damage: All weapons call `EnemyHealth.TakeDamage()` directly

### Utility Classes
- **TargetingUtility** - Finds unique enemy targets for multi-shot weapons
- **ShaderHelper** - Creates reveal material instances for VFX

### Weapon Implementations

**Greatsword** - Grand Cleave (Melee Slash)
- Horizontal slash VFX (Left/Right based on PlayerController movement)
- SpriteReveal shader with AnimationCurve opacity fade
- Multi-hit tracking (HashSet<int> prevents duplicate kills per swing)
- No physical rotation; visual-only effect

**Auto Crossbow** - Firework Bolt (Projectile)
- Finds nearest enemy via Physics2D.OverlapCircleAll (15m range)
- Spawns spinning arrow projectile with pierce capability
- Multi-shot support (spread pattern)
- ArrowProjectile handles movement, lifetime, pierce count

**Holy Water** - Sanctified Ground (Area Denial)
- Spawns blue fire zone at random offset (3.5m radius)
- RadialReveal shader (center-outward expansion)
- Animation: Rune expands → Fire particles → DoT → Rune shrinks → Destroy
- DamagePuddle tracks enemies in HashSet, applies DoT every `tickRate`

**Magic Orbitals** - Ethereal Shields (Orbital)
- Spawns 3 balls (OrbitalBall instances)
- Radius animation: 0 → targetRadius → 0 (unified AnimateRadius coroutine)
- Per-enemy damage cooldown (Dictionary<int, float>)
- Damages ALL touched enemies simultaneously (no global cooldown)
- Visibility delays (Invoke) prevent overlap at spawn/despawn

---

## Enemy Systems

### EnemyAI
- Simple chase behavior (moves toward player via normalized delta)
- **Speed:** From EnemyData.moveSpeed (typically 2.5-3.0)
- **Pathfinding:** Direct vector (no nav mesh)
- **Pauses:** When game paused or dead

### EnemyHealth
- **HP Management:** Takes damage, applies knockback, triggers death
- **Knockback:** Scaled by `weightResistance` (0-1, heavier = less knockback)
- **Loot:** Fires events OnEnemyKilled (XP, gold) via GameEvents

### EnemySpawner
- Continuous spawning at screen edges (outside camera view)
- **Difficulty Scaling:** Spawn rate increases with GameManager.GameTime
- **Selection:** Weighted random based on EnemyData.spawnWeight
- **Limit:** Max 100 enemies on screen

---

## Loot & Progression

### XPGem
- Magnet behavior (pulls toward player within 3 units)
- **Speed:** 10 units/sec when attracted
- **Value:** Set by EnemyData.xpDrop
- **Collection:** OnTriggerEnter2D with player

### Level-Up Flow
1. XP bar fills → `GameEvents.OnPlayerLevelUp` fires
2. `LevelUpPanel` shows 3 random upgrades (no duplicates, smart filtering)
3. Player selects → `UpgradeManager.ApplyUpgrade(UpgradeData)`
4. Multiplier conversion applied → All weapons + player updated

---

## UI Systems

### HUD
- Real-time display: HP bar, XP bar, level, timer, kill count
- Event-driven updates (subscribes to GameEvents)

### LevelUpPanel
- Shows 3 random UpgradeData cards
- **Filtering:** Excludes already-equipped weapons
- **Smart Selection:** Prevents duplicate cards

### PauseMenu / GameOverScreen
- Standard overlay panels
- State transitions via GameManager.SetState()

---

## Technical Notes

### Platform Detection
- PlatformDetector Singleton (MUST have only 1 in scene)
- Auto-detects Mobile/Desktop/Editor
- Fires OnPlatformChanged event
- Desktop: Hides joystick, enables Input Actions

### Save System
- ServiceLocator pattern (fail-fast on duplicates)
- PlayerPrefs-based persistence
- SaveData model (serializable)

### Common Pitfalls
1. **Weapon Update Override:** MUST call `base.Update()` first
2. **Upgrade Multipliers:** Use conversion formulas (value/100)
3. **Weapon Parenting:** Weapons unparent themselves (world-space positioning)
4. **Singleton Duplicates:** Only ONE PlatformDetector/GameManager allowed
