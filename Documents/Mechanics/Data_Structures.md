# Data Structures

**Architecture:** ScriptableObject-based Data-Driven Design.
**Location:** `Assets/Scripts/Data/`

## 1. Core Data Models

### CharacterData (`CD_`)
Defines a playable hero.
*   **Stats:** `MaxHealth`, `MoveSpeed`, `DamageMultiplier`.
*   **Loadout:** `StarterWeapon` (WeaponData ref).
*   **Identity:** `Prefab`, `Portrait`, `Name`.

### WeaponData (`WD_`)
Defines a weapon's base statistics.
*   **Stats:** `BaseDamage`, `Cooldown`, `KnockbackForce`.
*   **Visuals:** `ProjectilePrefab`, `FireSound`.
*   **Behavior:** `WeaponType` Enum (Melee, Ranged, etc.).

### EnemyData (`ED_`)
Defines an enemy type.
*   **Spawn:** `SpawnWeight` (Rarity), `MinGameTime` (When it starts appearing).
*   **Combat:** `Health`, `Damage`, `MoveSpeed`.
*   **Visuals:** `Prefab`, `Scale`.

### UpgradeData (`UD_`)
Defines a level-up bonus.
*   **Type:** `StatBoost` (Passive) or `WeaponUnlock`.
*   **Weights:** `Common`, `Rare`, `Epic` probability weights.
*   **Prerequisites:** List of other `UpgradeData` required before this appears.

## 2. Validation
*   **Naming:** All assets must use prefixes (`CD_`, `WD_`, `ED_`).
*   **Immutability:** Data should not be changed at runtime (clone if needed).
