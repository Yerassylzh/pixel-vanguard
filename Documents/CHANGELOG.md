# Pixel Vanguard - Changelog

**Recent major changes and updates to the project.**

---

## January 1, 2026 (Evening)

### Yandex Ads Integration - Production Ready ✅

**Interstitial Ads:**
- Implemented `IAdService.ShowInterstitialAd()` interface method
- `YandexAdService.cs`: Uses `YG2.InterstitialAdvShow()` with automatic cooldown
- `PlaceholderAdService.cs`: Logs placeholder for non-WebGL builds
- `ResultsController.cs`: Shows interstitial ad at end of each game session

**Rewarded Ads - Multiple Touchpoints:**
1. **Shop System** (Existing): Watch ad for gold packs
2. **Game Over Revive** (NEW): Watch ad to revive and continue playing
   - `GameOverScreen.cs`: Integrated `IAdService.ShowRewardedAd()`
   - Players must watch ad successfully to revive (no free revives)
3. **Results Screen** (Existing): Watch ad to double session gold earnings

**Implementation Details:**
- Async/await pattern for ad completion
- Success callbacks trigger rewards (gold, revive, etc.)
- Platform-adaptive: Real ads on WebGL, instant success in Editor/Android
- No simulation - leverages PluginYG's automatic environment detection

---

### IAP System Finalization ✅

**Product ID Update:**
- Changed from `"gold_pack_29900"` to `"gold_pack"` per Yandex requirements
- Updated `ProductIDs.GOLD_PACK_LARGE` constant
- `ShopController.cs`: Uses new product ID in purchase flow
- `YandexIAPService.cs`: Already configured for Yandex Payments API

**Purchase Flow:**
- Shop → IAP "Special Offer" card → `YG2.BuyPayments("gold_pack")`
- Awards 29,900 gold on successful purchase
- Event-driven callbacks: `onPurchaseSuccess`, `onPurchaseFailed`

---

### Critical Bug Fix: Gold Display Persistence 🐛

**Problem:**
Gold display showed as 0 when navigating between Main Menu, Character Selection, and Settings after making shop purchases.

**Root Cause:**
`PlayerPrefsSaveService.LoadData()` was returning cached data instead of reading fresh from disk:
```csharp
// OLD (BUGGY):
if (cachedData != null)
    return cachedData;  // Stale data!

// NEW (FIXED):
// Always load from PlayerPrefs
PlayerPrefs.GetString(SAVE_KEY)
```

**Solution:**
1. **PlayerPrefsSaveService.cs**: Removed cache check - always loads from disk
2. **ShopController.cs**: Added `OnEnable()` to reload data when panel shown
3. **CharacterSelectController.cs**: Added `OnEnable()` + public `RefreshGoldAndUI()`
4. **SettingsController.cs**: Added public `RefreshGold()` method
5. **MainMenuManager.cs**: 
   - Calls `RefreshGoldDisplay()` when returning to main menu
   - Uses `FindFirstObjectByType<T>()` to find controllers (robust against hierarchy changes)
   - Explicitly refreshes gold when opening Character/Settings panels

**Modified Files:**
- `PlayerPrefsSaveService.cs` - Always load from disk
- `YandexSaveService.cs` - Already correct (no caching)
- `MainMenuManager.cs` - Added refresh calls + FindFirstObjectByType
- `ShopController.cs` - Added OnEnable
- `CharacterSelectController.cs` - Added OnEnable + public refresh method
- `SettingsController.cs` - Added public refresh method

**Debug Logging:**
Added comprehensive logs for troubleshooting:
- `[SaveService] Loaded from disk - Gold: X`
- `[SaveService] Saved to disk - Gold: X`
- `[CharacterSelect] Updating gold text to: X`
- `[Settings] Refreshing Gold Display: X`

---

### Save Service Synchronization Fixes ✅

**Removed Async Methods:**
All `await LoadData()` and `await SaveData()` calls were incorrect (methods are synchronous).

**Files Updated:**
- `CharacterManager.cs`
- `CharacterSelectController.cs`
- `ShopController.cs`
- `ResultsController.cs`
- `MainMenuManager.cs`
- `SettingsController.cs`
- `GoldCoin.cs`
- `PlayerMovement.cs`
- `PlayerHealth.cs`

**Changes:**
- Removed `await` keywords from all save operations
- Changed `async void` to `void` where appropriate
- Maintained synchronous call pattern: `saveService.LoadData()`, `saveService.SaveData(data)`

---

### Documentation Updates 📚

**Created:**
- `yandex_integration_guide.md` - Complete Yandex Games setup guide
  - Yandex Console configuration (Cloud Saves, Ads, Payments)
  - PluginYG module setup
  - Testing procedures (Editor, Sandbox, Production)
  - Troubleshooting guide

**Architecture Benefits:**
- Platform-adaptive services work seamlessly
- WebGL builds use Yandex services automatically
- Android/Editor builds use local services
- Zero code changes needed when switching platforms

---

## January 1, 2026 (Morning)

### Platform-Adaptive Services ✅
**New Features:**
- Automatic platform detection for WebGL vs Android builds
- Yandex Games integration for WebGL (cloud saves, rewarded ads, IAP)
- Conditional compilation using `#if UNITY_WEBGL` macros
- Zero code changes for Android builds (existing services preserved)

**Architecture:**
- `PlatformServiceFactory` - Central factory for service creation
- Conditional compilation prevents unused code in final builds
- No runtime platform checks - all resolved at compile time

**WebGL (Yandex Games):**
- `SavesYG.Partial.cs` - Cloud save structure for PluginYG
- `YandexSaveService` - Syncs SaveData ↔ Yandex cloud
- `YandexAdService` - Rewarded ads via YG2.RewardedAdvShow()
- `YandexIAPService` - ✅ **Yandex Payments fully integrated**
  - Uses `YG2.BuyPayments(productId)` for purchases
  - Event-based callbacks: `onPurchaseSuccess`, `onPurchaseFailed`
  - Retrieves prices from Yandex catalog via `YG2.PurchaseByID()`
  - Logs all available products on initialization

**Android / Editor:**
- `PlayerPrefsSaveService` - Local saves (unchanged)
- `PlaceholderAdService` - Ready for Unity Ads/AdMob
- `PlaceholderIAPService` - Ready for Google Play Billing

**Files Created:**
- `PlatformServiceFactory.cs` - Platform detection and service creation
- `SavesYG.Partial.cs` - Yandex cloud save data structure
- `YandexSaveService.cs` - Yandex cloud save implementation
- `YandexAdService.cs` - Yandex rewarded ads implementation
- `YandexIAPService.cs` - Yandex IAP placeholder

**Modified Files:**
- `GameBootstrap.cs` - Uses factory pattern for service registration

**Requirements:**
- PluginYG must be installed for WebGL builds
- Yandex Games console configuration (cloud saves, ads, payments)

**Documentation:**
- [walkthrough.md](file:///C:/Users/Honor/.gemini/antigravity/brain/1262dfee-5e5a-46c7-8875-7b0077e9411a/walkthrough.md) - Complete implementation walkthrough
- [implementation_plan.md](file:///C:/Users/Honor/.gemini/antigravity/brain/1262dfee-5e5a-46c7-8875-7b0077e9411a/implementation_plan.md) - Technical design

---

## December 31, 2024

### Character Selection System ✅
**New Features:**
- Complete character selection screen with gold-based unlock system
- Visual feedback for locked/unlocked characters (59% opacity for locked)
- Dynamic 3-state action button (BUY/CONFIRM/PLAY)
- Character details panel showing stats + shop upgrades applied
- Main menu integration (Play → Character Selection → GameScene)
- Synchronous save/load for plugin compatibility

**Files Created:**
- `CharacterCard.cs` - Individual character card UI
- `CharacterDetailsPanel.cs` - Character stats + weapon display
- `CharacterSelectController.cs` - Main selection controller

**SaveData Extensions:**
- `unlockedCharacterIDs` (List<string>)
- `selectedCharacterID` (string)
- `IsCharacterUnlocked()` / `UnlockCharacter()` helper methods
- Default unlocked: Knight + Pyromancer

**ISaveService Extensions:**
- `LoadDataSync()` - Synchronous save loading
- `SaveDataSync()` - Synchronous save writing
- Implemented in `PlayerPrefsSaveService.cs`

**Navigation Flow:**
```
Main Menu → PLAY
         ↓
Character Selection → BACK (Main Menu)
         ↓ PLAY
    GameScene
```

**Documentation:**
- [character_selection_system_documentation.md](file:///C:/Users/Honor/.gemini/antigravity/brain/4653feb0-0b4a-4e94-a2b5-0e036de540dc/character_selection_system_documentation.md) - Complete technical docs

---

### Shop Upgrades Integration ✅
**Applied shop upgrades to gameplay:**
- **Vitality** → `PlayerHealth`: +10 HP per level
- **Might** → `PlayerHealth`: +10% damage per level  
- **Greaves** → `PlayerMovement`: +5% speed per level
- **Magnet** → Collectibles (`XPGem`, `GoldCoin`, `HealthPotion`): +10% range per level

**Modified Files:**
- `PlayerHealth.cs` - Async stat loading with Vitality/Might upgrades
- `PlayerMovement.cs` - Async stat loading with Greaves upgrade
- `XPGem.cs`, `GoldCoin.cs`, `HealthPotion.cs` - Async magnet upgrade application

**Character Details Panel:**
- Shows base stats for locked characters
- Shows final stats (base + shop upgrades) for unlocked characters
- Format: `Health: 150 (100 +50)` shows base + bonus

---

### Bug Fixes ✅

**Gold Display Issues:**
- Fixed main menu gold not updating after earning/spending
- Fixed character selection gold not updating when panel re-opens
- Solution: Added `OnEnable()` refresh to both controllers

**Character Card Opacity:**
- Fixed purchased character remaining at 59% opacity after unlock
- Solution: Re-initialize card with `isLocked = false` after purchase

**Character Selection Persistence (CRITICAL):**
- Fixed selected character not spawning after app restart
- Issue: Static `CharacterManager.SelectedCharacter` reset to null on restart
- Solution: CharacterManager loads `selectedCharacterID` from SaveData on Awake
- **Requirement:** CharacterManager must have `All Characters` array assigned in Inspector

**Modified Files:**
- `MainMenuManager.cs` - Added OnEnable gold refresh
- `CharacterSelectController.cs` - Added OnEnable gold refresh + card opacity update after purchase
- `CharacterManager.cs` - Added SaveData loading + All Characters array field

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
