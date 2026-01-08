# Localization System

**Status:** ✅ Fully Implemented  
**Languages:** English (EN), Russian (RU)  
**Platform:** Yandex Games auto-detection + Manual toggle

[← Back to Architecture](../SYSTEM_ARCHITECTURE.md)

---

## 3-Layer Architecture

```
┌──────────────────────────┐
│  ILanguageProvider       │ ← Platform abstraction
│  (YandexLanguageProvider │ ← WebGL: Reads YG.lang
│   DefaultLanguageProvider)│ ← Android/Editor: PlayerPrefs
└────────────┬─────────────┘
             │
┌────────────▼─────────────┐
│  LocalizationManager     │ ← Static singleton
│  - Initialize()          │
│  - Get(key)              │
│  - SwitchLanguage()      │
└────────────┬─────────────┘
             │
┌────────────▼─────────────┐
│  TranslationData (SO)    │ ← ScriptableObject
│  - List<LocalizedString> │
│  - Get(key, lang)        │
└──────────────────────────┘
```

---

## Core Components

### LocalizationManager

```csharp
// Static singleton - globally accessible
LocalizationManager.Get("ui.shop.title") → "Shop" or "Магазин"
LocalizationManager.SwitchLanguage("ru")   → Triggers UI refresh
```

**Features:**
- **Fallback Chain:** Requested lang → English → "[Missing: key]"
- **Event-Driven:** `OnLanguageChanged` refreshes all UI
- **Platform-Agnostic:** Uses `ILanguageProvider` interface

---

### ILanguageProvider

Platform-specific language detection:

**YandexLanguageProvider (WebGL):**
- Reads `YG.YG2.lang` from PluginYG
- Auto-detects user's browser language
- Saves to Yandex cloud storage

**DefaultLanguageProvider (Editor/Android):**
- Reads  `Application.systemLanguage`
- Saves to `PlayerPrefs`
- Defaults to English for unsupported languages

---

### TranslationData (ScriptableObject)

Contains all translation keys:

```csharp
[System.Serializable]
public class LocalizedString
{
    public string key;           // "ui.shop.title"
    public string englishValue;  // "Shop"
    public string russianValue;  // "Магазин"
}
```

**Populated by:** `TranslationPopulator.cs` (Editor script)  
**Location:** `Data/Translations.asset`

---

## Translation Key Convention

```
<scope>.<category>.<item>[.<property>]

Examples:
ui.shop.title          → "Shop"
ui.shop.might.name     → "Might"  
ui.shop.might.desc     → "Increases all weapon damage"
upgrade.lifesteal.name → "Lifesteal"
upgrade.lifesteal.desc → "10% healing on hit"
```

---

## Usage Patterns

### 1. Static UI (LocalizedText Component)

```csharp
// Attach to TextMeshProUGUI
// Automatically refreshes on language change

[SerializeField] private string localizationKey = "ui.shop.title";

void OnEnable()
{
    LocalizationManager.OnLanguageChanged += Refresh;
    Refresh();
}

void Refresh()
{
    textComponent.text = LocalizationManager.Get(localizationKey);
}
```

### 2. Dynamic UI (Code)

```csharp
// ShopController.cs example
mightCard.Initialize(
    "might",
    LocalizationManager.Get("ui.shop.might.name"),  // Dynamic
    mightIcon,
    100,
    LocalizationManager.Get("ui.shop.might.desc")
);
```

### 3. UpgradeData Localization

```csharp
// UpgradeData ScriptableObject
[Header("Localization")]
public string localizationKey;  // "upgrade.lifesteal"

// LevelUpPanel.cs
string name = !string.IsNullOrEmpty(upgrade.localizationKey)
    ? LocalizationManager.Get($"{upgrade.localizationKey}.name")
    : upgrade.upgradeName;  // Fallback for old data
```

---

## Language Switching Flow

```
User clicks Language Toggle Button
    └─► LocalizationManager.SwitchLanguage("ru")
         ├─► Update _currentLanguage
         ├─► ILanguageProvider.SwitchLanguage("ru")
         │    └─► Save to Yandex cloud or PlayerPrefs
         └─► Trigger OnLanguageChanged event
              └─► All LocalizedText components refresh
                   └─► UI updates instantly
```

---

## Translation Coverage

✅ **Fully Localized:**
- Main Menu (Play, Shop, Settings, Quit)
- Shop System (Upgrades, Gold Packs, IAP)
- Character Selection
- Settings Panel
- HUD (Level, "New Level!")
- Game Over Screen
- Results Screen
- In-Game Upgrades (via UpgradeData)

---

## Adding New Translations

1. Add keys to `TranslationPopulator.cs`
2. Run `Tools → Localization → Populate Translations`
3. Assign keys to UI:
   - Static: `LocalizedText` component
   - Dynamic: `LocalizationManager.Get(key)` in code
4. Test language switching

---

## Platform Detection Integration

```csharp
// PlatformDetector.cs - Yandex Games
#if UNITY_WEBGL
if (YG.YG2.envir.deviceType == "mobile")
    return PlatformType.WebMobile;
else if (YG.YG2.envir.isDesktop)
    return PlatformType.Desktop;
#endif
```

**Benefits:**
- Accurate mobile vs desktop detection
- Touch controls auto-enable for mobile browsers
- Virtual joystick visibility controlled by platform

---

## Code Locations

| Component | File |
|-----------|------|
| LocalizationManager | [LocalizationManager.cs](file:///C:/Users/Honor/Unity%20Games/Pixel%20Vanguard/Assets/Scripts/Core/LocalizationManager.cs) |
| ILanguageProvider | [ILanguageProvider.cs](file:///C:/Users/Honor/Unity%20Games/Pixel%20Vanguard/Assets/Scripts/Services/Interfaces/ILanguageProvider.cs) |
| YandexLanguageProvider | [YandexLanguageProvider.cs](file:///C:/Users/Honor/Unity%20Games/Pixel%20Vanguard/Assets/Scripts/Services/Yandex/YandexLanguageProvider.cs) |
| DefaultLanguageProvider | [DefaultLanguageProvider.cs](file:///C:/Users/Honor/Unity%20Games/Pixel%20Vanguard/Assets/Scripts/Services/Placeholder/DefaultLanguageProvider.cs) |
| TranslationData | [TranslationData.cs](file:///C:/Users/Honor/Unity%20Games/Pixel%20Vanguard/Assets/Scripts/Data/TranslationData.cs) |
| LocalizedText | [LocalizedText.cs](file:///C:/Users/Honor/Unity%20Games/Pixel%20Vanguard/Assets/Scripts/UI/LocalizedText.cs) |

---

**Last Updated:** 2026-01-02  
**Related:** [Service Architecture](Service-Architecture.md), [UI System](UI-System.md)
