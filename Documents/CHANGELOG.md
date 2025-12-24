# Pixel Vanguard - Changelog

**Recent major changes and updates to the project.**

---

## December 24, 2024

### Refactoring: Player System Split
**Breaking Changes:**
- Split `PlayerController` into 4 separate components:
  - `PlayerController` - Singleton reference management
  - `PlayerMovement` - Rigidbody2D movement logic
  - `PlayerInput` - New Input System integration (WASD + Virtual Joystick)
  - `PlayerHealth` - HP management and damage multiplier

**Impact:** Improved separation of concerns, better testability

---

### Removed: EnemyDamageUtility
**Deleted:** `Assets/Scripts/Gameplay/Weapons/EnemyDamageUtility.cs`  
**Reason:** Redundant wrapper around `EnemyHealth.TakeDamage()`

**Affected Files:**
- GreatswordWeapon.cs
- AutoCrossbowWeapon.cs (ArrowProjectile)
- HolyWaterWeapon.cs (DamagePuddle)
- MagicOrbitalsWeapon.cs (OrbitalBall)

All weapons now call `EnemyHealth.TakeDamage()` directly.

---

### Added: Passive Upgrades Integration
**New Systems:**
- **Lifesteal** - Stored in `PlayerHealth.lifestealPercent`
- **Gold Bonus** - Multiplier stored in `PlayerHealth.goldBonusMultiplier`  
- **Magnet Range** - Increases XPGem/GoldCoin/HealthPotion magnet range

**Status:** Storage implemented, partial integration (magnet working)

---

### Balance Changes

#### Knockback Values (Dec 24)
- Greatsword: 5 → 100
- AutoCrossbow: 3 → 45
- HolyWater: 0 → 20
- MagicOrbitals: 3 → 50
- Force type: `ForceMode2D.Impulse` → `Force`

#### Enemy Drop Rates (Dec 20)
- Skeleton: Gold 4%→25%, Potion 100%→3%
- Crawler: Gold 100%→30%, Potion 5%→4%
- Goblin: Gold 24%→35%, Potion 5%
- Ghost: Gold 28%→40%, Potion 5%→6%
- ArmoredOrc: Gold 100%→80%, Potion 5%→8%
- Slime: Gold 100%→60%, Potion 5%→10%
- Boss: Potion 50%→25%

#### Character Stats (Dec 20)
- Knight move speed: 3.5 → 5
- HolyWater cooldown: 8s → 3s
- MagicOrbitals cooldown: 11s → 0.5s (continuous)

---

## December 20-24, 2024

### Upgrade System Overhaul
**Added:** 18 weapon-specific + passive upgrades
- Greatsword: Mirror Slash, Executioner's Edge, Berserker Fury
- AutoCrossbow: Dual/Triple Shot, Piercing Bolts
- HolyWater: Radius, HP Scaling, Duration
- MagicOrbitals: Expanded Orbit, Overcharged Spheres
- Passives: Lifesteal, Magnet, Lucky Coins (max 3)

**Features:**
- Rarity weighting system (Epic/Rare/Uncommon/Common)
- Duplicate prevention via HashSet tracking
- Weapon-specific filtering (only show if weapon equipped)
- Multi-targeting for AutoCrossbow (unique enemies per arrow)
- HP scaling for Holy Water (6% of enemy max HP)

---

### Bug Fixes

#### Player Animation (Dec 24)
- **Fixed:** Vertical-only movement now preserves last facing direction
- **Before:** Player defaulted to facing right when moving up/down
- **After:** Player maintains previous horizontal direction

#### Magic Orbitals (Dec 24)
- **Fixed:** Double-spawning at normal speed (race condition)
- **Solution:** Set `isActive = true` immediately instead of after animation

#### Greatsword (Dec 24)  
- **Fixed:** Weapon not firing (missing `isMirror` field)
- **Fixed:** Auto-find for SpriteRenderer and Collider2D components

#### AutoCrossbow (Dec 24)
- **Fixed:** Initial cooldown prevented immediate firing
- **Solution:** Removed `cooldownTimer = cooldown` initialization in WeaponBase

---

## Pending Work

### Integration Required
- Lifesteal: Apply healing on weapon hits
- Gold Bonus: Apply multiplier in `EnemyHealth.DropLoot()`

### Unity Setup Needed
- Create 18 UpgradeData ScriptableObjects
- Assign weapon prefabs in WeaponManager Inspector

---

**For detailed system documentation, see `SYSTEM_ARCHITECTURE.md`**
