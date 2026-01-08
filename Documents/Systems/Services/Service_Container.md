# Service Architecture

**Location:** `Systems/Services/Service_Container.md`
**Pattern:** Service Locator (`ServiceLocator.cs`)

## 1. Philosophy
Platform-specific code (Ads, Saves, IAP) is hidden behind Interfaces.
*   **Core Game:** Calls `IAdService.ShowAd()`.
*   **Injector:** Decides if that means `AdMob` (Android) or `YandexSDK` (Web).

## 2. Core Interfaces
### `IAdService`
*   `ShowRewardedAd()`: Returns Task<bool>. True = Give Reward.
*   `ShowInterstitialAd()`: Fire and forget (automatic cooldowns).

### `ISaveService`
*   `LoadData()`: Returns `SaveData` object.
*   `SaveData()`: Writes to Disk/Cloud.
*   **Yandex:** Auto-syncs to Yandex Cloud Storage.

### `IIAPService`
*   `PurchaseProduct(id)`: Initiates payment flow.
*   `GetPrice(id)`: Returns localized string ("$0.99" or "50 YAN").

## 3. Bootstrap Flow
1.  **Scene:** `Bootstrap.unity` (First scene).
2.  **Platform Check:** `PlatformDetector.Detect()`.
3.  **Registration:** `ServiceLocator.Register(new YandexAdService())`.
4.  **Load:** `SaveService.LoadData()`.
5.  **Transition:** Load `MainMenu`.
