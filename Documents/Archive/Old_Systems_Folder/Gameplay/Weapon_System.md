# Weapon System

**Location:** `Gameplay/Weapons/`

## Architecture
*   `WeaponBase` (Abstract): Handles Cooldowns, Stats, and Event Firing.
*   `WeaponData` (ScriptableObject): Stores Damage, Speed, and Prefab refs.

## Inheritance
```mermaid
graph TD
    A[WeaponBase] --> B[ProjectileWeapon]
    A --> C[MeleeWeapon]
    A --> D[OrbitalWeapon]
    B --> E[Crossbow]
    C --> F[Greatsword]
    D --> G[FireballOrbit]
```

## Adding a New Weapon
1.  Create `NewWeapon.cs` inheriting from `WeaponBase`.
2.  Implement `Fire()` method.
3.  Create `WeaponData` asset in `Resources/Data`.
4.  Expose `public Data.WeaponData Data => weaponData;` (Standard).
