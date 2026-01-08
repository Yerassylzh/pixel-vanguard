# GDD - Service Architecture
**Parent:** [Main Game Design Document](file:///c:/Users/Honor/Unity%20Games/Pixel%20Vanguard/Documents/Game%20Design%20Document%20-%20Main.md)

---

## Service Layer Overview

The Service Layer provides platform-agnostic interfaces for platform-specific functionality (ads, saves, input). This allows the core game code to remain platform-independent.

---

## Architecture Pattern: Service Locator

### Why Service Locator?

1. **Single Codebase**: Same game code runs on Android, Yandex Web, and Itch.io
2. **Runtime Detection**: Platform determined at startup, services injected dynamically
3. **Testability**: Can inject mock services for testing without platform dependencies

### Flow Diagram

```mermaid
graph TD
    A[Game Starts] --> B[Bootstrap Scene Loads]
    B --> C[PlatformDetector.Detect()]
    C --> D{Which Platform?}
    D -->|Android| E[Register AdMobService]
    D -->|Yandex Web| F[Register YandexAdService]
    D -->|Desktop/Itch| G[Register NoAdService]
    E --> H[Register PlayerPrefs SaveService]
    F --> I[Register YandexCloud SaveService]
    G --> H
    H --> J[ServiceLocator.IsReady = true]
    I --> J
    J --> K[Load Save Data]
    K --> L[Navigate to MainMenu]
```

---

## Core Services

### 1. IAdService

**Purpose:** Display ads and handle rewards (Rewarded + Interstitial)

**Methods:**

```csharp
interface IAdService {
    Task<bool> ShowRewardedAd();
    void ShowInterstitialAd();
    bool IsRewardedAdReady();
}
```

**Implementations:**

| Platform | Class | SDK |
|----------|-------|-----|
| WebGL (Yandex) | `YandexAdService` | PluginYG (`YG2.RewardedAdvShow`, `YG2.InterstitialAdvShow`) |
| Android/Editor | `PlaceholderAdService` | Returns instant success for testing |

**Integration Points (Jan 2026):**
1. **Shop:** Watch ads for gold packs (5 ads → 1,990g, 10 ads → 4,990g)
2. **Game Over:** Watch ad to revive mid-game
3. **Results:** Watch ad to double session gold
4. **Interstitial:** Automatic display at end of each game session

**Usage Example:**
```csharp
// Rewarded Ad (async/await pattern)
private async void OnWatchAdClicked() {
    var adService = ServiceLocator.Get<IAdService>();
    bool success = await adService.ShowRewardedAd();
    
    if (success) {
        // Grant reward (gold, revive, etc.)
        DoubleGold();
    }
}

// Interstitial Ad (fire-and-forget)
private void ShowInterstitialAd() {
    var adService = ServiceLocator.Get<IAdService>();
    adService.ShowInterstitialAd(); // Yandex handles cooldown
}
```

### 2. ISaveService

**Purpose:** Persist player data across sessions

**Methods:**

```csharp
interface ISaveService {
    void Initialize();
    SaveData LoadData();      // Synchronous (not async)
    void SaveData(SaveData data);
    bool IsCloudSaveAvailable();
}
```

**Data Structure:**
```csharp
[Serializable]
class SaveData {
    public int totalGold;
    public List<string> unlockedCharacterIDs;
    public string selectedCharacterID;
    public List<string> statLevelKeys;   // "might", "vitality", etc.
    public List<int> statLevelValues;    // 5, 3, etc.
    
    // Ad/IAP tracking
    public int adsWatchedForPack1;
    public int adsWatchedForPack2;
    public string lastAdWatchedTime;
    
    // High scores
    public int longestSurvivalTime;
    public int highestKillCount;
    public int highestLevelReached;
    public int mostGoldInRun;
}
```

**Implementations:**

| Platform | Class | Storage |
|----------|-------|---------|
| Android/Editor | `PlayerPrefsSaveService` | Local (PlayerPrefs, JSON) |
| WebGL (Yandex) | `YandexSaveService` | Yandex Cloud (`YG2.saves`) |

**Critical Fix (Jan 2026):**
```csharp
// PlayerPrefsSaveService - Always load fresh from disk
public SaveData LoadData() {
    // OLD (BUGGY): if (cachedData != null) return cachedData;
    // NEW (FIXED): Always read from PlayerPrefs
    string json = PlayerPrefs.GetString(SAVE_KEY);
    cachedData = JsonUtility.FromJson<SaveData>(json);
    return cachedData;
}
```

**Cloud Save Flow (Yandex):**
```
Load Data:
1. Always returns fresh YG2.saves (no caching)
2. Converts SavesYG → SaveData format
3. Ensures default characters unlocked

Save Data:
1. Convert SaveData → SavesYG format
2. Call YG2.SaveProgress() (uploads to cloud)
```

### 3. IIAPService (Jan 2026)

**Purpose:** Handle in-app purchases

**Methods:**

```csharp
interface IIAPService {
    Task<bool> PurchaseProduct(string productId);
    Task<string> GetProductPrice(string productId);
}
```

**Product Constants:**
```csharp
public static class ProductIDs {
    public const string GOLD_PACK_LARGE = "gold_pack";  // 29,900 gold
}
```

**Implementations:**

| Platform | Class | SDK |
|----------|-------|-----|
| WebGL (Yandex) | `YandexIAPService` | PluginYG (`YG2.BuyPayments`) |
| Android/Editor | `PlaceholderIAPService` | Instant success (testing) |

**Yandex IAP Flow:**
```csharp
// Purchase
public async Task<bool> PurchaseProduct(string productId) {
    var tcs = new TaskCompletionSource<bool>();
    
    YG2.onPurchaseSuccess += (id) => tcs.SetResult(true);
    YG2.onPurchaseFailed += (id) => tcs.SetResult(false);
    
    YG2.BuyPayments(productId);
    return await tcs.Task;
}
```

**Usage Example:**
```csharp
// In ShopController.cs
private async void OnIAPBuyClicked() {
    var iapService = ServiceLocator.Get<IIAPService>();
    bool success = await iapService.PurchaseProduct(ProductIDs.GOLD_PACK_LARGE);
    
    if (success) {
        // Award 29,900 gold
        saveData.totalGold += 29900;
        saveService.SaveData(saveData);
    }
}
```

---

### 4. IPlatformService

**Purpose:** Handle platform-specific input and screen settings

**Methods:**

```csharp
interface IPlatformService {
    bool IsMobileInput();
    bool IsWebPlatform();
    void SetFullscreen(bool enabled);
    InputType GetPreferredInputType();
}
```

**Implementations:**

| Platform | Class | Input Type |
|----------|-------|------------|
| Android | `AndroidPlatformService` | Touch |
| Yandex Mobile | `WebMobilePlatformService` | Touch (in browser) |
| Yandex Desktop | `WebDesktopPlatformService` | Mouse + Keyboard |

**Input Handling:**
```csharp
// In PlayerController.cs
void GetMovementInput() {
    var platform = ServiceLocator.Get<IPlatformService>();
    if (platform.IsMobileInput()) {
        // Use virtual joystick input
        moveDirection = Joystick.GetDirection();
    } else {
        // Use WASD/Arrow keys
        moveDirection = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
    }
}
```

---

## Bootstrap Process

**Script:** `Bootstrap.cs` (in Bootstrap scene)

**Execution Order:**

```
1. Awake() → Make DontDestroyOnLoad
2. Start() → Begin initialization
3. DetectPlatform() → Determine runtime environment
4. RegisterServices() → Inject platform-specific implementations
5. LoadInitialData() → Fetch save data via ISaveService
6. OnInitializationComplete() → Navigate to MainMenu
```

**Error Handling:**
- If save load fails: Create new default save data
- If ad service fails: Use NoAdService (game playable without ads)
- If critical service fails: Show error screen and retry option

---

## ServiceLocator Implementation

**Global Instance:**
```csharp
public static class ServiceLocator {
    private static Dictionary<Type, object> services = new();
    
    public static void Register<T>(T implementation) {
        services[typeof(T)] = implementation;
    }
    
    public static T Get<T>() {
        if (services.TryGetValue(typeof(T), out var service)) {
            return (T)service;
        }
        throw new Exception($"Service {typeof(T)} not registered!");
    }
    
    public static bool Has<T>() {
        return services.ContainsKey(typeof(T));
    }
}
```

**Thread Safety:**
- Services registered only during Bootstrap (single-threaded)
- All `Get<T>()` calls after Bootstrap complete (services immutable)

---

## Platform Detection Logic

**Script:** `PlatformDetector.cs`

**Detection Rules:**

```csharp
public enum PlatformType {
    Android,
    YandexWeb,
    WebDesktop,
    UnityEditor
}

public static PlatformType Detect() {
    #if UNITY_EDITOR
        return PlatformType.UnityEditor;
    #elif UNITY_ANDROID
        return PlatformType.Android;
    #elif UNITY_WEBGL
        // Check for Yandex SDK
        if (IsYandexEnvironment()) {
            return PlatformType.YandexWeb;
        }
        return PlatformType.WebDesktop;
    #else
        return PlatformType.UnityEditor;
    #endif
}

private static bool IsYandexEnvironment() {
    // Use JSLIB to check window.YaGames
    return Application.ExternalCall("checkYandexSDK") == "true";
}
```

---

## Service Configuration (ScriptableObject)

**Asset:** `ServiceConfiguration.asset`

**Purpose:** Map platforms to service implementations without hardcoding

```csharp
[CreateAssetMenu]
public class ServiceConfiguration : ScriptableObject {
    public AdServiceConfig adService;
    public SaveServiceConfig saveService;
}

[Serializable]
public class AdServiceConfig {
    public string androidAdMobID;
    public string yandexAppID;
    public int interstitialFrequency; // Show every N game sessions
}
```

**Bootstrap Usage:**
```csharp
[SerializeField] ServiceConfiguration config;

void RegisterServices() {
    var platform = PlatformDetector.Detect();
    
    IAdService adService = platform switch {
        PlatformType.Android => new AdMobService(config.adService.androidAdMobID),
        PlatformType.YandexWeb => new YandexAdService(config.adService.yandexAppID),
        _ => new NoAdService()
    };
    
    ServiceLocator.Register<IAdService>(adService);
}
```

---

## Testing Strategy

### Unit Tests (Services)

**Test with Mocks:**
```csharp
[Test]
public void TestSaveLoad() {
    var mockSaveService = new MockSaveService();
    ServiceLocator.Register<ISaveService>(mockSaveService);
    
    var saveData = new SaveData { totalGold = 1000 };
    await mockSaveService.SaveData(saveData);
    
    var loaded = await mockSaveService.LoadData();
    Assert.AreEqual(1000, loaded.totalGold);
}
```

### Integration Tests (Platform)

**Editor Play Mode:**
- Run in Editor → Should use UnityEditor platform services
- Verify ads don't actually show (mock service)
- Verify save works with PlayerPrefs

---

## Migration & Versioning

**Save Data Versioning:**
```csharp
[Serializable]
class SaveData {
    public int version = 1; // Increment when schema changes
    // ... rest of data
}
```

**Migration Logic:**
```csharp
SaveData LoadData() {
    var json = PlayerPrefs.GetString("SaveData");
    var data = JsonUtility.FromJson<SaveData>(json);
    
    if (data.version < 2) {
        // Migrate version 1 → 2
        data = MigrateV1ToV2(data);
    }
    
    return data;
}
```

---

## Future Services

**Potential Additions:**

1. **ILeaderboardService** - Submit scores to platform leaderboards
2. **IAchievementService** - Unlock achievements
3. **IAnalyticsService** - Track player behavior events
4. **ICloudService** - Explicit cloud save/load UI
