# Shop System

**Status:** ✅ Implemented (v1.0 - Testing Phase)  
**Last Updated:** Dec 30, 2024

## Overview

Monetization system with stat upgrades, ad-based gold packs, and IAP. Features persistent progression and a details panel for item information.

---

## Components

### ShopController
**Path:** `Assets/Scripts/UI/Shop/ShopController.cs`

Main controller managing all shop functionality.

**Responsibilities:**
- 4 stat upgrade cards (Might, Vitality, Greaves, Magnet)
- 2 ad pack cards with progress tracking
- IAP special offer
- Async data persistence
- Real-time ad cooldown timer

**Services Used:**
- `ISaveService` - Player data persistence
- `IAdService` - Ad cooldown and display
- `IIAPService` - In-app purchases

---

### UpgradeCard
**Path:** `Assets/Scripts/UI/Shop/UpgradeCard.cs`

Individual stat upgrade UI card.

**Features:**
- Level display: "Level: 3 / 10"
- Effect text shows NEXT level bonus (e.g., "Damage +40%")
- Color-coded buy button (green = affordable, gray = can't afford)
- Clickable card area for details panel

**Stat Formulas:**
```
Might:    +10% damage per level
Vitality: +10 HP per level
Greaves:  +5% speed per level
Magnet:   +10% range per level

Cost: baseCost * (1.5 ^ currentLevel)
```

---

### AdPackCard
**Path:** `Assets/Scripts/UI/Shop/AdPackCard.cs`

Ad-based gold reward with progress tracking.

**States:**
- **Ready:** Shows "(3/5)" - ads remaining, green button
- **Cooldown:** Shows "42s" countdown, gray button

**Rewards:**
- Pack 1: 5 ads → 1,990 gold
- Pack 2: 10 ads → 4,990 gold

---

### DetailsPanel
**Path:** `Assets/Scripts/UI/Shop/DetailsPanel.cs`

Shows detailed item information when clicking cards.

**States:**
- Default: "Select upgrade to see details"
- Upgrade: Icon + name + description
- Gold Pack: Icon + title + reward info

---

## Service Architecture

### Ad Service (IAdService)
**Path:** `Assets/Scripts/Services/IAdService.cs`

```csharp
Task Initialize()
Task<bool> ShowRewardedAd()
bool CanWatchAd(string lastWatchedTime)
int GetCooldownRemainingSeconds(string lastWatchedTime)
```

**Current:** `PlaceholderAdService` (60s cooldown, always succeeds)  
**Production:** Replace with Unity Ads, AdMob, or Yandex SDK

---

### IAP Service (IIAPService)
**Path:** `Assets/Scripts/Services/IIAPService.cs`

```csharp
Task<bool> Initialize()
Task<bool> PurchaseProduct(string productId)
string GetLocalizedPrice(string productId)
bool IsProductAvailable(string productId)
```

**Product IDs:**
```csharp
const string GOLD_PACK_LARGE = "com.pixelvanguard.gold_large";
```

**Current:** `PlaceholderIAPService` ($4.99 placeholder)  
**Production:** Replace with Google Play, App Store, or Yandex SDK

---

## SaveData Extensions

**Path:** `Assets/Scripts/Services/SaveData.cs`

**New Fields:**
```csharp
public int adsWatchedForPack1 = 0;       // Progress (0-5)
public int adsWatchedForPack2 = 0;       // Progress (0-10)
public string lastAdWatchedTime = "";    // ISO 8601 timestamp
```

**Stat Storage:**
- Dictionary: `statLevels["might"]`, etc.
- Range: 0-10

---

## Critical Implementation Notes

### Execution Order Fix
Cards MUST initialize in `Awake()` to prevent race condition:

```csharp
private void Awake()
{
    SetupUpgradeCards();  // Before card Start()
}

private void Start()
{
    // Get services after cards ready
    LoadSaveData();
}
```

### Effect Text Logic
Shows NEXT level's bonus:
```csharp
int nextLevel = level + 1;
effectValue = nextLevel * 10;
```

### Ad Progress Display
Button format: `"(3/5)"` = 3 ads remaining

---

## Testing Checklist

- [x] Costs display correctly (not 0)
- [x] Effect text shows next level (not +0%)
- [x] Buy button color changes with gold
- [x] Ad progress shows (X/Y) format
- [x] Ad cooldown timer works
- [x] Gold awarded after completing pack
- [x] IAP shows price (not "---")
- [x] Details panel updates on click
- [ ] Data persists across restarts
- [ ] Real ad SDK integration
- [ ] Real IAP SDK integration

---

## Production TODO

- [ ] Replace `PlaceholderAdService` with real SDK
- [ ] Replace `PlaceholderIAPService` with real SDK
- [ ] Delete placeholder service files
- [ ] Create all shop UI sprites
- [ ] Test on mobile devices
- [ ] Verify ad cooldown on production
- [ ] Test IAP purchases on stores

---

## Related Files

- Main doc: [Shop System Documentation](file:///C:/Users/Honor/.gemini/antigravity/brain/4653feb0-0b4a-4e94-a2b5-0e036de540dc/shop_system_documentation.md)
- Bug fixes: [Session Bug Fixes](file:///C:/Users/Honor/.gemini/antigravity/brain/4653feb0-0b4a-4e94-a2b5-0e036de540dc/session_bugfixes_summary.md)
- Setup: [Shop Setup Guide](file:///C:/Users/Honor/.gemini/antigravity/brain/4653feb0-0b4a-4e94-a2b5-0e036de540dc/shop_final_setup.md)
