# Shop & IAP System

**Location:** `Systems/UI/Shop_Implementation.md`
**Code Path:** `Assets/Scripts/UI/Shop/`

## 1. Components
*   **`ShopController`:** Orchestrates the UI. Initializes cards.
*   **`UpgradeCard`:** Displays individual stat upgrade (Level, Cost, Effect).
*   **`CoinRewardAnimator`:** Visual feedback when purchasing.

## 2. Integration
*   **Save System:** Reads/Writes `Gold` and `StatLevels`.
*   **IAP Service:** Calls `PurchaseProduct` for Gold Packs.
*   **Ad Service:** Calls `ShowRewardedAd` for free Gold.

## 3. Persistence
Shop upgrades are permanent.
*   `SaveData.statLevels["might"] = 5`
*   `SaveData.statLevels["vitality"] = 2`

## 4. UI Flow
1.  **Open:** Main Menu -> Shop Button.
2.  **Interact:** Click Upgrade Card -> Update Details Panel.
3.  **Buy:** Click Buy -> Check Gold -> Deduct Gold -> Increment Level -> Save.
