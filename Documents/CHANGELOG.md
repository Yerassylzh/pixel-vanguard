# Pixel Vanguard - Changelog

**Recent major changes and updates to the project.**

---

## January 13, 2026

### Gameplay & UX Improvements ✅

**Three professional features added to improve game balance and user experience:**

#### 1. Revive Limit System (Max 3 Per Session) 🎮

**Implementation:**
- Added `revivesUsed` counter and `CanRevive` property to `SessionData.cs`
- SessionData subscribes to `GameEvents.OnPlayerRevived` (event-driven architecture)
- GameOverScreen disables revive button GameObject when limit exceeded
- Counter resets each time GameScene loads

**Files Modified:**
- `SessionData.cs` - Added revive tracking with event subscription (OnPlayerRevived)
- `GameManager.cs` - Removed direct counter increment (now event-driven)
- `GameOverScreen.cs` - Hides revive button when `!CanRevive`

**Architecture:** Event-driven pattern - SessionData auto-increments on revive event, no tight coupling

#### 2. Ad Loading Visual Feedback 📱

**Problem:** Users with slow networks had no feedback while ads loaded
**Solution:** Loading/error states with localized text for better UX

**Implementation:**
- Loading state: Shows "Wait.." (EN) / "Секунду.." (RU) while ad loads
- Error state: Shows "No ad" (EN) / "Ошибка" (RU) for 2 seconds if ad fails
- State machine with lock prevents cooldown timer from overriding states

**Files Modified:**
- `AdPackCard.cs` - Added state machine (enum + isStateLocked guard)
- `GoldPackHandler.cs` - Shows loading before ShowRewardedAd(), error on failure
- `IAPHandler.cs` - Same loading/error states for Android special pack
- `TranslationPopulator.cs` - Added ui.shop.ad_loading and ui.shop.ad_error keys

**State Machine Design:**
```csharp
enum AdButtonState { Ready, Cooldown, Loading, Error }
private bool isStateLocked; // Prevents cooldown from overriding
```

**Critical Fix:** Multiple update sources override text
- `ShopUpdateCoroutine` calls `UpdateCooldown()` every second
- `OnGoldChanged` event triggers `RefreshCards()` → `UpdateProgress()`
- **Solution:** Both methods check `IsInTemporaryState` and return early if locked
- After delay: Unlock → normal updates work again

**Lock Guards Added:**
- ✅ `UpdateCooldown()` - Respects lock
- ✅ `UpdateProgress()` - Respects lock (critical for gold change events)

#### 3. Configurable Player Spawn Position 🎯

**Implementation:**
- Added `manualSpawnPosition` Vector2 field to CharacterManager
- Priority: Transform > Vector2 > (0,0,0)
- Allows spawn configuration via Inspector without creating GameObjects

**Files Modified:**
- `CharacterManager.cs` - Added Vector2 spawn field, updated SpawnPlayer() logic

---

### Service Architecture Refactoring 🔧

**Event-Driven Best Practices:**

#### IAdService.Initialize() Made Synchronous

**Reason:** Both YandexAdService and AdMobAdService only subscribe to events - no async work needed
**Change:** Removed `async Task` → now `void Initialize()`

**Files Modified:**
- `IAdService.cs` - Interface signature updated
- `YandexAdService.cs` - Removed async/await, just subscribes to events
- `AdMobAdService.cs` - Removed async/await
- `PlaceholderAdService.cs` - Removed async/await  
- `GameBootstrap.cs` - Removed await from Initialize() call

#### Event-Driven Revive Counter

**Before (Bad):** GameManager directly modified SessionData.revivesUsed++
**After (Good):** SessionData subscribes to GameEvents.OnPlayerRevived

**Benefits:**
- ✅ Decoupled - GameManager doesn't know about revivesUsed field
- ✅ Consistent - Matches patterns for gold, kills, XP (all use events)
- ✅ Scalable - Other systems can easily subscribe to same event
- ✅ Follows SOLID principles

---

### Debug Testing Features 🧪

**Added to all ad services for testing:**

```csharp
private const bool SIMULATE_SLOW_LOADING = false; // Test "Wait.." UI
private const int SIMULATED_LOAD_DELAY_MS = 3000;
private const bool SIMULATE_AD_FAILURE = false;   // Test "No ad" UI
```

**Purpose:** Simulate poor network conditions to test loading/error state UI
**Files:** YandexAdService.cs, AdMobAdService.cs, PlaceholderAdService.cs

---

### Files Modified Summary

| File | Changes |
|------|---------|
| SessionData.cs | Added revive counter with event subscription |
| GameManager.cs | Removed direct revive increment |
| GameOverScreen.cs | Hide button on revive limit |
| AdPackCard.cs | State machine with lock mechanism |
| GoldPackHandler.cs | Loading/error states + callback |
| IAPHandler.cs | Loading/error states for special pack |
| CharacterManager.cs | Manual spawn position field |
| All AdServices | Made Initialize() synchronous + debug flags |
| TranslationPopulator.cs | Added loading/error translation keys |

**Architecture Patterns:**
- ✅ State Machine (AdPackCard)
- ✅ Event-Driven (SessionData revive counter)
- ✅ Observer Pattern (GameEvents subscribers)
- ✅ Single Responsibility (state lock prevents timer conflicts)

---

## January 12, 2026

### Yandex IAP & Gold Display System Fixes ✅

**Critical fixes for In-App Purchase initialization and gold UI synchronization across panels:**

#### 1. IAP Service Initialization Fix 🐛

**Problem:** `YandexIAPService.Initialize()` wasn't completing fully - only first debug log appeared, price display showed "---", unconsumed purchases weren't processed.

**Root Cause:** 
- `GameBootstrap` was using "fire and forget" pattern: `_ = iapService.Initialize()`
- When `Initialize()` hit `await Task.Yield()`, execution yielded but never resumed (Task was discarded)
- ShopController subscription and ConsumePurchases logic never executed

**Solution:**
-Made `YandexIAPService.Initialize()` synchronous (returns `bool` instead of `Task<bool>`)
- Removed problematic `await Task.Yield()` and redundant ShopController subscription
- Updated `IIAPService` interface to match synchronous signature
- Updated `PlaceholderIAPService` to match
- Removed fire-and-forget pattern in `GameBootstrap.cs` (no longer needed since it's synchronous)

**Modified Files:**
- `YandexIAPService.cs` - Made Initialize() synchronous, removed Task.Yield() and redundant subscription
- `IIAPService.cs` - Changed Initialize() signature from `Task<bool>` to `bool`
- `PlaceholderIAPService.cs` - Updated Initialize() to match interface
- `GameBootstrap.cs` - Updated service initialization call

#### 2. Unconsumed Purchase Logic Moved 🔄

**Change:** Moved purchase consumption from YandexIAPService to ShopController for better separation of concerns.

**Implementation:**
- `ShopController.Awake()` now calls `YG2.ConsumePurchases(onPurchaseSuccess: true)`
- Ensures unconsumed purchases are processed when shop opens, not just at game startup
- Subscribes to `YG2.onPurchaseSuccess` event for gold pack purchases

**Modified Files:**
- `ShopController.cs` - Added Awake() with consume logic and event subscription

#### 3. Gold Display Synchronization - Event System ✅

**Problem:** Main Menu gold display didn't update after purchases/consumption until navigating to another panel and back.

**Solution:** Implemented centralized event-based system for automatic gold UI updates.

**Architecture:**
```csharp
// CachedSaveDataService.cs
public static event Action OnGoldChanged;

public void AddGold(int amount) {
    Data.totalGold += amount;
    Save();
    OnGoldChanged?.Invoke(); // Notify all subscribers
}
```

**Panel Integration:**
All UI panels now subscribe to `OnGoldChanged` in `Awake()`:
- `MainMenuManager.cs`
- `ShopController.cs` 
- `CharacterSelectController.cs`
- `SettingsController.cs`

**Benefits:**
- Gold updates instantly across **all panels** when changed anywhere
- No manual refresh calls needed
- Decoupled architecture - panels don't need to know about each other
- Event unsubscription in `OnDestroy()` prevents memory leaks

**Modified Files:**
- `CachedSaveDataService.cs` - Added OnGoldChanged event, fires in AddGold(), SpendGold(), TotalGold setter
- `MainMenuManager.cs` - Subscribe/unsubscribe to event
- `ShopController.cs` - Subscribe/unsubscribe, CoinRewardAnimator handles text updates
- `CharacterSelectController.cs` - Subscribe/unsubscribe, added RefreshGoldDisplay() method
- `SettingsController.cs` - Subscribe/unsubscribe to event

#### 4. Coin Reward Animation Integration ✨

**Design Decision:** Keep animation simple - let `CoinRewardAnimator` handle gold text updates during animation, event system updates other panels.

**Flow:**
1. Purchase succeeds → `cachedSave.AddGold(29900)` called
2. `OnGoldChanged` fires → All panels (Main Menu, Settings, Character Select) update immediately
3. If shop is open → `CoinRewardAnimator` plays coin flight animation with text count-up
4. Animation completes → Final state already synchronized via event

**Result:**  Smooth visual feedback with automatic cross-panel synchronization.

#### 5. Canvas Resolution Setter Tool 🛠️

**Created:** Unity Editor tool for configuring Canvas Scaler settings across scenes.

**Features:**
- Platform-specific settings (Android vs WebGL)
- Reference resolution and match width/height configuration
- Apply to current scene or all scenes in build settings
- Ensures consistent UI scaling across platforms

**File Created:**
- `Editor/CanvasResolutionSetter.cs`

---

### Technical Highlights

**Async Initialization Pattern (What NOT to Do):**
```csharp
// ❌ WRONG - Fire and forget
_ = service.Initialize(); // Execution stops at first await

// ✅ RIGHT - Made synchronous
bool success = service.Initialize(); // No await needed
```

**Event-Based Gold Updates:**
```csharp
// Awake in all UI panels
Services.CachedSaveDataService.OnGoldChanged += RefreshGoldDisplay;

// OnDestroy in all UI panels  
Services.CachedSaveDataService.OnGoldChanged -= RefreshGoldDisplay;
```

---

## January 3, 2026


### Startup Scene Interaction Improvements ✅

**Enhanced user experience for faster scene transitions:**

#### Input Detection Upgrade
- **New Input System Integration** - Replaced legacy `Input` class with New Input System
  - Uses `Mouse.current.leftButton.wasPressedThisFrame` for mouse clicks
  - Uses `Touchscreen.current.primaryTouch.press.wasPressedThisFrame` for touch input
  - Fully compatible with existing mobile and desktop builds

#### Click Anywhere Functionality
- **Problem:** Players could only click on the "Press to Start" text to proceed
- **Solution:** Added `Update()` method to detect clicks/taps anywhere on screen
- **Benefit:** Improved UX - entire screen is now interactive

#### Instant Scene Loading
- **Problem:** Scene loaded only after fade-out animation completed (~1+ seconds)
- **Solution:** Removed fade-out wait, loads main menu immediately on click
- **Benefit:** Faster transition, more responsive feel

**Modified Files:**
- `StartupSceneController.cs` - New Input System integration, instant loading

---

## January 2, 2026

### UI Animation System ✅

**Complete professional UI animation framework for main menu, shop, and gameplay:**

#### Core Animation Infrastructure
- **DOTween Integration** - Hardware-accelerated UI tweening throughout
- **CoinRewardAnimator** - Configurable coin spawning for purchases
  - Multiple coins with staggered spawn (0.08s interval)
  - Two-stage animation: pop-up → bezier flight to target
  - Professional non-stacking pulse on arrival
  - Optional text count-up integration
  - Overlay canvas creation (sortOrder 1000) for z-ordering
  - Resolution-independent coin sizing (32px)
- **MainMenuIntroAnimator** - Vampire Survivors-style intro
  - Scale-out effect (zooms from 1.5x → 1x)
  - Sequential UI element fade-in with configurable delays
  - Play-once-per-session logic (skips on return to menu)
  - Supports both UI Images and SpriteRenderer backgrounds
  - Manual scale/position configuration for pivot compensation
  - Integrated one-shot music playback

**Files Created:**
- `CoinRewardAnimator.cs` - Shop/Results coin reward animations
- `MainMenuIntroAnimator.cs` - Main menu intro sequence

**Modified Files:**
- `ShopController.cs` - Integrated coin animations for IAP + ad rewards
- `MenuNavigationController.cs` - Fixed panel overlap (instant hide before slide)
- `AnimatedButton.cs` - Fixed pause menu buttons (unscaled time)
- `AudioManager.cs` - Added `PlayOneShotMusic()` for non-looping audio

---

#### Bug Fixes & Polish

**1. Panel Navigation Overlap** 🐛
- **Problem:** Both panels visible during transitions creating visual overlap
- **Solution:** Instant hide old panel, smoothly show new panel (no simultaneous animation)
- **File:** `MenuNavigationController.cs`

**2. Pause Menu Button Animation** 🐛
- **Problem:** Buttons didn't animate when `Time.timeScale = 0`
- **Solution:** All tweens now use `.SetUpdate(true)` for unscaled time
- **File:** `AnimatedButton.cs`

**3. Coin Rendering Z-Order** 🐛
- **Problem:** Coins rendered behind shop popup panels
- **Solution:** Created dedicated overlay Canvas (sortOrder 1000)
- **File:** `CoinRewardAnimator.cs`

**4. Target Icon Scale Stacking** 🐛
- **Problem:** Multiple coins caused gold icon scale amplification
- **Solution:** Kill previous pulse tween before starting new one
- **Result:** Fast, clean pulses without visual artifacts
- **File:** `CoinRewardAnimator.cs`

**5. Intro Music Looping** 🐛
- **Problem:** 7-second intro music looped forever
- **Solution:** Created `PlayOneShotMusic()` in AudioManager
- **Files:** `AudioManager.cs`, `MainMenuIntroAnimator.cs`

**6. Coin Size Screen-Dependent** 🐛
- **Problem:** Coin size varied across different resolutions
- **Solution:** Changed from `Vector2` to `float coinSizePixels` (resolution-independent)
- **File:** `CoinRewardAnimator.cs`

---

#### AudioManager Enhancements

**New Methods:**
```csharp
// One-shot music (intros, stingers)
public void PlayOneShotMusic(AudioClip musicClip)

// Looping music (backgrounds)
public void PlayMusic(AudioClip musicClip) // Now explicitly sets loop = true
```

**Usage:**
- Intro music: `AudioManager.Instance.PlayOneShotMusic(introClip)`
- Background music: `AudioManager.Instance.PlayMusic(bgMusicClip)`

---

#### Technical Highlights

**Non-Stacking Pulse Pattern:**
```csharp
Tween activePulseTween = null;  // Single tracked tween
activePulseTween?.Kill();        // Stop previous animation
activePulseTween = transform.DOScale(...); // Start new pulse
```

**Play-Once-Per-Session Pattern:**
```csharp
private static bool hasShownIntro = false;  // Static flag persists
if (hasShownIntro)
    SkipIntro();  // Return to menu
else
    PlayIntro();  // First launch
```

**Overlay Canvas Architecture:**
- Dedicated `CoinOverlayCanvas` with sortOrder 1000
- Ensures coins always render on top of all UI
- Auto-created and reused across animations

---

#### Integration Points

**Shop:**
- IAP purchase (29,900 gold) → 10 coins fly to gold icon
- Ad pack 1 completion (1,990 gold) → 5 coins
- Ad pack 2 completion (4,990 gold) → 5 coins
- Gold text counts up as coins arrive

**Main Menu:**
- First app launch → Full intro animation plays
- Return from game → Instant skip to menu
- Background scales out (1.5x → 1x) over 5-7 seconds
- UI elements fade in sequentially (0.15s stagger)

---

#### Unity Editor Setup Required

**Shop Coin Animations (3 min):**
1. Add `CoinRewardAnimator` component to Canvas
2. Assign to ShopController
3. Assign gold icon transform reference

**Main Menu Intro (10 min):**
1. Add `MainMenuIntroAnimator` to Canvas
2. Add `CanvasGroup` to UI elements (title, buttons)
3. Assign background transform
4. Assign UI elements array in order
5. Generate 5-7s intro music using AI (prompt provided in docs)
6. Import and assign intro music clip

**Documentation:**
- [intro_animation_guide.md](file:///C:/Users/Honor/.gemini/antigravity/brain/9ec9c9e6-88a1-4484-89fb-b334dbf7be1d/intro_animation_guide.md)
- [setup_instructions.md](file:///C:/Users/Honor/.gemini/antigravity/brain/9ec9c9e6-88a1-4484-89fb-b334dbf7be1d/setup_instructions.md)
- [walkthrough.md](file:///C:/Users/Honor/.gemini/antigravity/brain/9ec9c9e6-88a1-4484-89fb-b334dbf7be1d/walkthrough.md)

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
