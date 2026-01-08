# Upgrade System - Technical Implementation

**Architecture:** 4-class modular design (Dec 2024 refactor)  
**Total Upgrades:** 18 types (4 repeatable, 4 weapons, 10 weapon-specific, 3 passives)

## Architecture

```
UpgradeManager (218 lines) - Orchestration
    ├─► UpgradeTracker (118 lines) - State tracking
    ├─► UpgradeValidator (173 lines) - Validation logic
    └─► UpgradeApplicator (389 lines) - Effect implementation
```

### Class Responsibilities

**UpgradeManager:** Initializes components, coordinates `GetRandomUpgrades()` and `ApplyUpgrade()`, exposes public API  
**UpgradeTracker:** Tracks applied upgrades (HashSet), equipped weapons (HashSet), passive count (0-3), passive effects (lifesteal %, gold %)  
**UpgradeValidator:** Validates repeatability, ownership, prerequisites (Triple needs Dual), passive limits  
**UpgradeApplicator:** Applies effects to player/weapons via switch statement, organized by category

## Upgrade Types

### Repeatable (Infinite)
- **Move Speed:** +20% per selection (multiplies `PlayerController.moveSpeed`)
- **Max HP:** +10 per selection (calls `PlayerHealth.IncreaseMaxHealth()`)
- **Weapon Damage:** +% all weapons (`weapon.IncreaseDamage(multiplier)`)
- **Attack Speed:** -% cooldown all weapons (`weapon.IncreaseAttackSpeed(multiplier)`)

### New Weapons (One-Time Per Weapon)
- Greatsword, AutoCrossbow, HolyWater, MagicOrbitals
- Tracked by `weaponID` in `equippedWeaponIDs` HashSet
- Filtered if already equipped
- Max 4 weapons total

### Weapon-Specific (One-Time)

**Greatsword:**
- Mirror Slash - Spawns 2nd sword firing opposite direction
- Executioner's Edge - +50% damage
- Berserker Fury - -30% cooldown

**AutoCrossbow:**
- Dual Shot - Fires 2 arrows at unique targets
- **Triple Barrage - Fires 3 arrows (requires Dual first)**
- Piercing Bolts - +1 pierce count (`crossbow.IncrementPierce()`)

**HolyWater:**
- Sanctified Expansion - +40% radius (`holyWater.MultiplyPuddleRadius(1.4f)`)
- Burning Touch - +6% enemy max HP as damage (`holyWater.SetHPScaling(0.06f)`)
- Eternal Flame - 2x duration (`holyWater.UpgradeDuration(1.0f)`)

**MagicOrbitals:**
- Expanded Orbit - +40% orbit radius (affects next spawn)
- Overcharged Spheres - +30% damage (immediate)

### Passives (Max 3 Total)
- **Lifesteal:** +3% per selection (stored in `UpgradeTracker`, exposed via `GetLifestealPercent()`)
- **Magnet:** +50% pickup range (multiplies `XPGem.magnetRange` and `GoldCoin.magnetRange`)
- **Lucky Coins:** +40% gold (stored in `UpgradeTracker`, exposed via `GetGoldBonusPercent()`)

## Validation Flow

```
GetRandomUpgrades(3)
    → Filter allUpgrades array
        → For each: UpgradeValidator.IsUpgradeValid()
            → Repeatable stat? Always valid
            → tracker.HasUpgrade()? Filter out
            → Requires weapon? Check IsWeaponEquipped()
            → Has prerequisite? Check tracker.HasUpgrade()
            → Passive limit? Check tracker.GetPassiveSkillCount() < 3
    → SelectWeightedRandom(validUpgrades)
    → Return 3 upgrades

ApplyUpgrade(upgrade)
    → applicator.ApplyUpgrade(upgrade)
        → Switch on upgrade.type → Call specific method
    → If non-repeatable: tracker.TrackUpgrade(type)
```

## Rarity System

**Weight Distribution:**
- Common (100): Repeatable stats
- Uncommon (50): Most weapon-specific upgrades, new weapons
- Rare (25): Crossbow Triple, Berserker Fury, Holy Water Duration
- Epic (10): Mirror Slash, Burning Touch, Overcharged Spheres
- Passive (30): Lifesteal, Magnet, Lucky Coins

**Selection:** Weighted random - higher weight = higher chance  
**Formula:** `totalWeight = sum(rarityWeights)`, pick random in `[0, totalWeight)`, return first upgrade where `accumulatedWeight > randomValue`

## Key Methods

**UpgradeManager:**
- `GetRandomUpgrades(count)` - Returns filtered, weighted selection
- `ApplyUpgrade(upgrade)` - Applies effects and tracks state
- `GetLifestealPercent()` - Public accessor for combat systems
- `GetGoldBonusPercent()` - Public accessor for loot systems

**UpgradeValidator:**
- `IsUpgradeValid(upgrade)` - Returns true if upgrade can be selected
- `IsWeaponEquipped(weaponType)` - Checks if weapon type is equipped

**UpgradeApplicator:**
- `ApplyUpgrade(upgrade)` - Main entry point, routes to specific handlers
- 20+ private methods for each upgrade type

## Integration Points

- `PlayerController` - Speed modifications
- `PlayerHealth` - HP increases, passive effect storage
- `WeaponManager` - Weapon unlocking, mirror greatsword spawning
- `WeaponBase` - IncreaseDamage(), IncreaseAttackSpeed()
- Weapon-specific classes - Special upgrade methods (SetMultiShot, IncrementPierce, etc.)
- `XPGem` / `GoldCoin` - Magnet radius field access

## Starter Weapon Tracking

On `Start()`, `UpgradeManager` initializes `UpgradeTracker` with equipped starter weapons:
```csharp
foreach (var weapon in weaponManager.GetEquippedWeapons())
    tracker.TrackWeapon(weapon.weaponData.weaponID);
```

This prevents starter weapon from appearing as selectable upgrade.

## Triple Crossbow Prerequisite

`UpgradeValidator` checks:
```csharp
case UpgradeType.CrossbowTripleShot:
    if (!tracker.HasUpgrade(UpgradeType.CrossbowDualShot))
        return false; // Must have Dual first
```

Ensures upgrade progression: Dual → Triple
