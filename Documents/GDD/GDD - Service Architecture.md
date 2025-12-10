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

**Purpose:** Display ads and handle rewards

**Methods:**

```csharp
interface IAdService {
    bool IsRewardedAdReady();
    void ShowRewardedAd(Action<bool> onComplete);
    bool IsInterstitialAdReady();
    void ShowInterstitialAd(Action onComplete);
}
```

**Implementations:**

| Platform | Class | SDK |
|----------|-------|-----|
| Android | `AdMobService` | Google AdMob |
| Yandex Web | `YandexAdService` | Yandex Ads SDK |
| Desktop/Itch | `NoAdService` | Returns false, never shows ads |

**Usage Example:**
```csharp
// In ResultsController.cs
void OnDoubleGoldButtonClicked() {
    var adService = ServiceLocator.Get<IAdService>();
    if (adService.IsRewardedAdReady()) {
        adService.ShowRewardedAd(success => {
            if (success) goldAmount *= 2;
            SaveGoldAndReturn();
        });
    }
}
```

### 2. ISaveService

**Purpose:** Persist player data across sessions

**Methods:**

```csharp
interface ISaveService {
    Task<SaveData> LoadData();
    Task SaveData(SaveData data);
    bool IsCloudSaveAvailable();
}
```

**Data Structure:**
```csharp
[Serializable]
class SaveData {
    public int totalGold;
    public List<string> unlockedCharacters;
    public Dictionary<string, int> statLevels; // "Might" → 5
    public string selectedCharacter;
}
```

**Implementations:**

| Platform | Class | Storage |
|----------|-------|---------|
| Android | `PlayerPrefsSaveService` | Local (PlayerPrefs) |
| Yandex Web | `YandexCloudSaveService` | Yandex Cloud |
| Desktop/Itch | `PlayerPrefsSaveService` | Local (PlayerPrefs) |

**Cloud Save Flow (Yandex):**
```
Load Data:
1. Check if user is logged in to Yandex
2. If yes: Fetch from cloud
3. If no or failed: Fallback to PlayerPrefs local save
4. Return merged data

Save Data:
1. Save to PlayerPrefs (immediate)
2. If cloud available: Upload to cloud (async)
```

### 3. IPlatformService

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
