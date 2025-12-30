# Pixel Vanguard - Changelog

**Recent major changes and updates to the project.**

---

## December 30, 2024

### Shop System Implementation ✅
**New Features:**
- Complete shop UI system with stat upgrades, ad packs, and IAP
- 4 stat upgrade cards: Might, Vitality, Greaves, Magnet
- 2 ad reward packs (5 ads → 1,990 gold, 10 ads → 4,990 gold)
- Special Offer IAP card ($4.99 placeholder)
- Details panel for item information
- Ad cooldown system (60 seconds)
- Persistent progress tracking

**Services Added:**
- `IAdService` + `PlaceholderAdService` (rewarded ads with cooldown)
- `IIAPService` + `PlaceholderIAPService` (in-app purchases)
- Service initialization in GameBootstrap

**SaveData Extensions:**
- `adsWatchedForPack1` / `adsWatchedForPack2` (progress tracking)
- `lastAdWatchedTime` (cooldown timestamp)

**Files Created:**
- `ShopController.cs` - Main shop logic
- `UpgradeCard.cs` - Stat upgrade UI
- `AdPackCard.cs` - Ad reward UI with cooldown
- `DetailsPanel.cs` - Item info display
- `IAdService.cs` + `PlaceholderAdService.cs`
- `IIAPService.cs` + `PlaceholderIAPService.cs`

**Documentation:**
- [Shop System.md](file:///c:/Users/Honor/Unity%20Games/Pixel%20Vanguard/Documents/Progress/Shop%20System.md) in Progress folder
- Complete technical docs in `.gemini/brain/` artifacts

---

### Bug Fixes (Critical)

**Crossbow Audio Bug:**
- **Fixed:** Firing sound played when no enemies present
- **Solution:** Moved `OnWeaponFired` trigger inside `Fire()` after target check
- **Files:** `WeaponBase.cs`, `AutoCrossbowWeapon.cs`, all weapon classes

**Shop Costs Showing 0:**
- **Fixed:** Unity execution order race condition
- **Solution:** Moved card initialization from `Start()` to `Awake()`
- **File:** `ShopController.cs`

**Effect Text Showing +0%:**
- **Fixed:** Displayed current level instead of next level bonus
- **Solution:** Changed formula to `(level + 1) * bonus`
- **File:** `UpgradeCard.cs`

**Service Initialization:**
- **Fixed:** IAP/Ad services not initialized before use
- **Solution:** Added `await service.Initialize()` in GameBootstrap
- **File:** `GameBootstrap.cs`

**Ad Progress Display:**
- **Fixed:** Showed total gold instead of ads remaining
- **Solution:** Changed format to `"(X/Y)"` progress display
- **File:** `AdPackCard.cs`

**Details:** See [session_bugfixes_summary.md](file:///C:/Users/Honor/.gemini/antigravity/brain/4653feb0-0b4a-4e94-a2b5-0e036de540dc/session_bugfixes_summary.md)

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
