# Upgrade System Formulas

**Location:** `Mechanics/Upgrade_Formulas.md`
**Code Path:** `Assets/Scripts/Gameplay/Upgrades/`

## 1. Categories
| Category | Count | Behavior |
| :--- | :--- | :--- |
| **Universal** | 4 | Speed, HP, Damage. Infinite repeatability. |
| **Weapon Unlock** | 4 | Grants new weapon (Greatsword, Crossbow, etc). One-time. |
| **Weapon Spec** | 10 | Modifies specific weapon (e.g. "Mirror Slash"). One-time. |
| **Passive** | 3 | Lifesteal, Magnet, Money. Limited to 3 slots. |

## 2. Rarity Weights
Higher rarity = Lower probability but better stats.
*   **Common (100):** +10% Damage
*   **Rare (25):** +25% Damage
*   **Epic (10):** +50% Damage

## 3. Prerequisite Logic
Upgrades form a tree structure.
`Dual Shot` → *requires* → `Crossbow Lv1`
`Triple Shot` → *requires* → `Dual Shot`

## 4. Balance Curve
*   **Early Game (0-5m):** Focus on Weapon Unlocks.
*   **Mid Game (5-15m):** Focus on Synergies/Modifers.
*   **Late Game (15m+):** Focus on Infinite Stat Stacking (Attack Speed / Damage).
