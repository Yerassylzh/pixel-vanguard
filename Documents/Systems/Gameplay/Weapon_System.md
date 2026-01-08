# Weapon System

**Location:** `Systems/Gameplay/Weapon_System.md`
**Code Path:** `Assets/Scripts/Gameplay/Weapons/`

## 1. Architecture
Polymorphic inheritance structure allows easy addition of new weapon types.

*   `WeaponBase` (Abstract Monobehaviour)
    *   `AutoCrossbow` (Projectiles)
    *   `Greatsword` (Melee Hitbox)
    *   `OrbitalShield` (Rotating Projectiles)
    *   `HolyWater` (Area of Effect)

## 2. Key Components
*   **WeaponData (SO):** Stores stats (`Damage`, `Speed`, `Cooldown`).
*   **WeaponBase:** Handles `Cooldown` timer and `Level` management.
*   **Projectiles:** Independent objects that handle their own collision (`OnTriggerEnter2D`).

## 3. Targeting
*   **`TargetingUtility.GetNearestEnemy()`**: Efficiently finds closest target using `Physics2D.OverlapCircleNonAlloc`.
*   **Auto-Fire:** Weapons fire automatically when `Cooldown <= 0` and a target is in range.

## 4. Upgrades provides
*   `WeaponManager` applies upgrades.
*   **Stat:** Direct modification (`weapon.Data.baseDamage *= 1.1f`).
*   **Evolution:** (Future) Swapping `WeaponData` for Evolved variant.
