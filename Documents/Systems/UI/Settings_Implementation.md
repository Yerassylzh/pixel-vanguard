# Settings Implementation

**Location**: `Systems/UI/Settings_Implementation.md`  
**Code Path**: `Assets/Scripts/UI/Settings/`  
**Last Updated**: 2026-01-09

---

## 1. Auto-Save Pattern

**Design**: All changes apply and save immediately (no "Apply" button)

**Benefits**:
- Modern UX (instant feedback)
- Single source of truth (GameSettings service)
- No complex revert logic needed
- Simpler code (no pending state variables)

---

## 2. Managed Settings

| Setting | Type | Default | Storage |
|---------|------|---------|---------|
| Music Volume | Slider (0-1) | 0.67 | GameSettings |
| SFX Volume | Slider (0-1) | 0.67 | GameSettings |
| Language | Dropdown | "en" | GameSettings |
| Show FPS | Toggle | true | GameSettings |
| Show Damage Numbers | Toggle | true | GameSettings |
| Remove Ads | Button (Android) | - | CachedSaveDataService |

---

## 3. Auto-Save Flow

**Example: Music Volume Change**
```
User drags slider
→ OnMusicVolumeChanged(value)
→ AudioManager.SetMusicVolume(value) [instant apply]
→ GameSettings.SetMusicVolume(value)
→ GameSettings.Save() [instant persist]
```

**Key Point**: Each change triggers immediate apply + save

---

## 4. Service Integration

**GameSettings** (IGameSettings):
- Stores all settings in persistent storage
- Uses ISaveService internally (Yandex Cloud / PlayerPrefs)
- Separate from player SaveData (different JSON file)

**AudioManager**:
- Settings applied directly on change
- SFX slider plays preview sound on drag

**LocalizationManager**:
- Language change triggers global `OnLanguageChanged` event
- All UI text updates automatically

**FPS Display** / **Damage Numbers**:
- Toggles show/hide immediately
- Components check settings before displaying

---

## 5. Platform-Specific Features

**Remove Ads Button**:
- **WebGL**: Hidden (ads not used)
- **Android**: Shows cost (4990 gold)
  - Before purchase: Shows "REMOVE ADS"
  - After purchase: Shows "PURCHASED" (disabled)

**Purchase Flow**:
```
Click Remove Ads button
→ IAdService.PurchaseRemoveAds()
→ Deduct 4990 gold
→ Set cachedSave.Data.adsRemoved = true
→ Save & update button state
```

---

## 6. Lifecycle

**Start()**: 
- Get services from ServiceLocator
- Subscribe to UI events (sliders, toggles, buttons)
- Hide Remove Ads button on WebGL
- Apply saved audio settings to AudioManager

**OnEnable()**: 
- Refresh UI with current values
- Null check for early lifecycle (may run before Start)

**RefreshUI()**: 
- Load values using `SetValueWithoutNotify` (avoids triggering callbacks)
- Updates all sliders, toggles, dropdown

**OnDestroy()**: 
- Remove all event listeners

---

## 7. Localization

**LocalizedText Components**: Automatically update when language changes

**Keys Used**:
- `ui.settings.music`
- `ui.settings.sfx`
- `ui.settings.language`
- `ui.settings.show_fps`
- `ui.settings.show_damage`
- `ui.settings.remove_ads` / `ui.settings.ads_removed`

**Flow**: User selects language → GameSettings saves → LocalizationManager fires event → All text updates

---

## 8. Why No "Apply" Button?

**Old Pattern Problems**:
- Audio previews immediately, but nothing else does (inconsistent)
- Users confused about when changes take effect
- Complex revert logic on Back button
- Doubled memory (7 pending* variables)

**New Pattern Benefits**:
- Predictable: Every change saves instantly
- Modern standard (Steam, mobile games all use instant save)
- Cleaner code: No pending state management
- Better UX: Immediate visual feedback

---

## 9. Testing Checklist

- [ ] Adjust music/SFX volume, verify AudioManager updates
- [ ] Toggle FPS/damage, verify in-game display updates
- [ ] Change language, verify all text updates
- [ ] Purchase Remove Ads (Android), verify button state
- [ ] Close and reopen settings, verify persistence

---

## 10. Code Metrics

**File Size**: ~260 lines  
**Complexity**: ⭐⭐⭐ (Low-Medium)  
**Services Used**: GameSettings, CachedSaveDataService, IAdService  
**Events**: 7 UI callbacks + 1 localization event

**Recommendation**: This pattern should be the **standard for all settings screens** in the project.
