# Economy & Shop System

**Location:** `Mechanics/Economy.md`
**Code Path:** `Assets/Scripts/UI/Shop/`

## 1. Currencies
*   **Gold (Soft Currency):**
    *   **Source:** Drops from enemies, Ad Packs, IAP.
    *   **Sink:** Permanent Stat Upgrades in Main Menu Shop.
    *   **Persistence:** Saved to `SaveService` (JSON/Cloud).

## 2. Shop Structure
The Shop allows permanent progression ("Meta-Progression").

### Stat Upgrades
Infinite scaling costs: `Cost = Base * (1.5 ^ Level)`.
1.  **Might:** +Damage %
2.  **Vitality:** +Max HP
3.  **Greaves:** +Move Speed %
4.  **Magnet:** +Pickup Range %

### Gold Packs (Monetization)
1.  **Ad Packs:** "Watch 5 Ads to get 2000 Gold". Tracks progress persistently.
2.  **IAP Packs:** Direct purchase via platform payment (Yandex/Google).

## 3. Technical Implementation
*   **`ShopController`:** UI Manager for the shop scene.
*   **`UpgradeCard`:** UI Element for a single stat. Calculates its own cost/effect.
*   **Services:**
    *   `IIAPService`: Handles real-money transactions.
    *   `IAdService`: Handles rewarded video callbacks.
