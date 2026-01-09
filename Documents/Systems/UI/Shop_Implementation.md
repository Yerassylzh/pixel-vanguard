# Shop Implementation

**Location**: `Systems/UI/Shop_Implementation.md`  
**Code Path**: `Assets/Scripts/UI/Shop/`  
**Last Updated**: 2026-01-09

---

## 1. Architecture: Composition Pattern

`ShopController` acts as a coordinator, delegating to 4 specialized handlers:

| Handler | Purpose | Lines |
|---------|---------|-------|
| **UpgradeShopHandler** | Stat upgrades (Might, Vitality, Greaves, Magnet) | ~170 |
| **GoldPackHandler** | Ad pack watching & rewards | ~160 |
| **IAPHandler** | Platform-specific IAP & special offers | ~160 |
| **ShopUIHandler** | UI management & navigation | ~70 |

**Total**: ~690 lines across 5 files (vs 683 in old monolithic version)  
**Benefit**: Single responsibility, easy to test, platform-agnostic design

---

## 2. Upgrade Shop

**Handler**: `UpgradeShopHandler`

**Stats**: Might (damage), Vitality (health), Greaves (speed), Magnet (pickup radius)

**Cost Formula**: `baseCost * 1.15^currentLevel`  
- Might base: 100 gold
- Vitality base: 80 gold  
- Greaves base: 120 gold
- Magnet base: 60 gold

**Flow**: User clicks BUY → Check gold → Deduct → Increment stat level → Apply to UpgradeManager → Save → Refresh UI

---

## 3. Gold Pack Ads

**Handler**: `GoldPackHandler`

**Packs**:
- Pack 1: Watch 5 ads → 1990 gold
- Pack 2: Watch 10 ads → 4990 gold

**Features**:
- Tracks progress per pack (e.g., "3/5" displayed on card)
- 60-second cooldown after each ad (shared across packs)
- Resets to 0/X after completing pack
- Coin animation plays when reward granted

**Timer**: `ShopUpdateCoroutine` runs every 1 second to update cooldown displays

---

## 4. IAP / Special Offer

**Handler**: `IAPHandler`

**Platform Differences**:

| Platform | Offer | Mechanism |
|----------|-------|-----------|
| **WebGL** | Large Gold Pack (29900) | Yandex IAP with real money |
| **Android** | Special Ad Offer (29900) | Watch 20 ads |

**Button States**:
- WebGL: Shows Yandex price (e.g., "₽99")
- Android (ready): Shows progress (e.g., "15/20")
- Android (cooldown): Shows timer (e.g., "45s")

---

## 5. UI Components

**Cards**:
- `UpgradeCard`: Shows stat, level, cost, effect preview
- `AdPackCard`: Shows reward amount, progress, cooldown timer
- `DetailsPanel`: Displays full information on click

**Events**:
- Card click → Show details panel
- Buy/Watch button → Execute purchase/ad logic

---

## 6. Service Integration

**CachedSaveDataService**:
- `SpendGold(amount)` - Validates and deducts
- `Data.statLevels` - Persistent upgrade levels
- `Data.adsWatchedForPack1/Pack2/SpecialPack` - Progress tracking
- `Data.lastAdWatchedTime` - ISO 8601 timestamp for cooldown

**IAdService**:
- `ShowRewardedAd()` - Returns Task<bool>
- `CanWatchAd(lastWatchTime)` - 60s cooldown check
- `GetCooldownRemainingSeconds()` - For timer display

**IIAPService** (WebGL only):
- `PurchaseProduct(productId)` - Yandex payment flow
- `GetLocalizedPrice(productId)` - For button text

**UpgradeManager**:
- `ApplyStatUpgrade(statName, newLevel)` - Applies boost to gameplay

---

## 7. Cooldown Timer System

**File**: `Controllers/ShopUpdateCoroutine.cs`

**Purpose**: Static coroutine runs every 1 second to update UI

**Updates**:
- Gold pack cards: Show remaining cooldown seconds
- IAP button: Update text based on cooldown/progress
- Stops when shop closes

**Started by**: `ShopController.Start()`

---

## 8. Localization

All shop text uses `LocalizationManager.Get(key)`:
- Stat names/descriptions
- Gold pack descriptions  
- Button labels

**Language Change**: `LocalizationManager.OnLanguageChanged` event triggers all handlers to refresh text

---

## 9. Platform-Specific Notes

**WebGL (Yandex)**:
- Uses `YG2.Purchase()` for IAP
- Consumed purchases handled via `YG2.onPurchaseSuccess` callback
- IAP button shows Yandex price

**Android**:
- IAP card becomes "20-ad special offer"
- No real money purchases
- Ad cooldown timer shared with regular packs

---

## 10. Event Flow Examples

### Upgrade Purchase
```
Click BUY on Might card
→ UpgradeShopHandler.PurchaseUpgrade("might")
→ CachedSaveDataService.SpendGold(cost)
→ Increment statLevels["might"]
→ UpgradeManager.ApplyStatUpgrade("might", newLevel)
→ Save & refresh UI
```

### Complete Ad Pack
```
Click WATCH on 5-ad pack (at 4/5 progress)
→ GoldPackHandler.WatchAd(1)
→ IAdService.ShowRewardedAd()
→ Ad completes successfully
→ adsWatchedForPack1 = 5 (completed!)
→ Grant 1990 gold
→ CoinRewardAnimator plays
→ Reset adsWatchedForPack1 = 0
→ Save & refresh
```

---

## 11. Testing Checklist

- [ ] Purchase each upgrade (Might, Vitality, Greaves, Magnet)
- [ ] Complete both ad packs (5-ad, 10-ad)
- [ ] Test IAP/special offer (platform-specific)
- [ ] Verify cooldown timer updates
- [ ] Test details panel for all cards
- [ ] Change language, verify text updates
- [ ] Test back button navigation

---

## 12. Design Benefits

**Why Composition?**
- Each handler has single responsibility
- Easy to unit test (plain C# classes)
- Platform logic isolated in `IAPHandler`
- No 683-line god object

**Maintainability**: ⭐⭐⭐⭐⭐  
**Testability**: ⭐⭐⭐⭐⭐  
**Platform Support**: WebGL (Yandex), Android (AdMob), extensible
