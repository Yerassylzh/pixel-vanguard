# Service Architecture

**Location**: `Systems/Services/Service_Container.md`  
**Pattern**: Service Locator (`ServiceLocator.cs`)  
**Last Updated**: 2026-01-09

---

## 1. Philosophy

Platform-specific code (Ads, Saves, IAP) hidden behind interfaces. Core game never depends on platform implementations.

**Benefits**: Cross-platform, testable, maintainable

---

## 2. Service Locator Pattern

**Core Class**: `Core/ServiceLocator.cs`

**API**:
- `ServiceLocator.Register<T>(implementation)` - Register service
- `ServiceLocator.Get<T>()` - Retrieve service (returns null if not found)

**Registration**: Happens once in `GameBootstrap.Awake()`

---

## 3. Core Interfaces

### 3.1 IAdService
**Purpose**: Show ads and handle rewards

**Implementations**: `YandexAdService` (WebGL), `AdMobAdService` (Android)

**Key Methods**:
- `ShowRewardedAd()` - Returns Task<bool>, true if user watched
- `ShowInterstitialAd()` - Fire-and-forget
- `CanWatchAd(lastWatchTime)` - 60s cooldown check
- `GetCooldownRemainingSeconds()` - For timer display
- `PurchaseRemoveAds()` - Android only (4990 gold)

---

### 3.2 ISaveService
**Purpose**: Persist player data to disk/cloud

**Implementations**: `YandexSaveService` (WebGL cloud), `PlayerPrefsSaveService` (Android local)

**Key Methods**:
- `LoadData()` - Returns SaveData object
- `SaveData(data)` - Writes to storage

**SaveData Fields**:
- `totalGold`, `statLevels`, `selectedCharacterID`, `unlockedCharacters`
- Shop tracking: `adsWatchedForPack1/2/SpecialPack`, `lastAdWatchedTime`
- Purchase: `adsRemoved`

---

### 3.3 IIAPService
**Purpose**: Handle in-app purchases (WebGL only)

**Implementation**: `YandexIAPService`

**Key Methods**:
- `IsInitialized` - Check if SDK ready
- `PurchaseProduct(productId)` - Returns Task<bool>
- `GetLocalizedPrice(productId)` - For button text (e.g., "₽99")
- `ConsumePurchases()` - Yandex requirement (call on startup)

**Product IDs**: `ProductIDs.GOLD_PACK_LARGE` (29900 gold)

---

### 3.4 IGameSettings
**Purpose**: Store and persist game settings

**Implementation**: `GameSettings.cs` (uses ISaveService internally)

**Settings**: Music/SFX volume, language, showFPS, showDamageNumbers

**Storage**: Separate JSON file from player SaveData

---

##4. Specialized Services

### 4.1 CachedSaveDataService
**Type**: Concrete class (not interface)

**Purpose**: In-memory cache of SaveData with helper methods

**Key Features**:
- Caches SaveData after first load
- Helper methods: `SpendGold(amount)`, `CanAffordUpgrade()`
- Auto-refreshes cache after save (Yandex cloud sync)

**Why Refresh After Save?**: Yandex Cloud Save may sync data from other devices

---

## 5. Bootstrap Flow

```
Unity loads Bootstrap scene
→ GameBootstrap.Awake()
  → RegisterPlatformServices() (IAdService, ISaveService, IIAPService)
  → RegisterCoreServices() (CachedSaveDataService, IGameSettings)
→ IAdService.Initialize()
→ ISaveService.LoadData() → CachedSaveDataService caches
→ IIAPService.ConsumePurchases() (Yandex only)
→ Load MainMenu scene
```

---

## 6. Platform-Specific Implementations

| Service | WebGL | Android |
|---------|-------|---------|
| **Ads** | YandexAdService (Yandex SDK) | AdMobAdService (Unity Ads) |
| **Save** | YandexSaveService (Cloud) | PlayerPrefsSaveService (Local) |
| **IAP** | YandexIAPService | ❌ Uses ad-based purchases |
| **Settings** | GameSettings | GameSettings |

---

## 7. Service Access Patterns

**MonoBehaviour Components**:
```csharp
void Start()
{
    cachedSave = ServiceLocator.Get<CachedSaveDataService>();
    adService = ServiceLocator.Get<IAdService>();
    // Always null-check!
}
```

**Plain C# Classes (Handlers)**:
```csharp
public GoldPackHandler(IAdService adService, ...)
{
    this.adService = adService; // Dependency injection
}
```

---

## 8. Current Services Summary

| Service | Interface | Purpose |
|---------|-----------|---------|
| Ad Service | IAdService | Show ads, manage cooldowns |
| Save Service | ISaveService | Persist player data |
| IAP Service | IIAPService | In-app purchases (WebGL only) |
| Settings | IGameSettings | Game settings (audio, UI) |
| Cached Save | ❌ (concrete) | In-memory cache + helpers |

---

## 9. Adding New Services

**Steps**:
1. Define interface (e.g., `IAnalyticsService`)
2. Create platform implementations
3. Register in `GameBootstrap.RegisterPlatformServices()`
4. Use via `ServiceLocator.Get<T>()`

---

## 10. Best Practices

✅ **DO**:
- Register services once in GameBootstrap
- Use interfaces for platform-specific code
- Null-check services after Get<T>()
- Cache service references in Start()

❌ **DON'T**:
- Call Get<T>() in Update() loops
- Register different implementations after initialization
- Create circular dependencies
- Mix ServiceLocator with FindObjectByType

---

## Summary

Service Locator provides:
- Platform abstraction via interfaces
- Dependency injection for testability
- Centralized registration
- Runtime flexibility for mocking/testing

All platform-specific code isolated in service implementations.
