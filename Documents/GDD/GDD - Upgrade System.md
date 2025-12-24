# GDD - Upgrade System
**Parent:** [Main Game Design Document](file:///c:/Users/Honor/Unity%20Games/Pixel%20Vanguard/Documents/GDD/Game%20Design%20Document%20-%20Main.md)

---

## Overview

The upgrade system allows players to enhance their character and weapons during a run via level-up cards. Players choose from 3 random upgrades each time they level up, creating build diversity and strategic depth.

**Total Upgrades:** 19 unique upgrade paths
- **10 weapon-specific** upgrades
- **3 passive skills**
- **1 stat boost**
- **5 universal/new weapon** upgrades (existing system)

---

## Universal Upgrades (Existing System)

These upgrades affect ALL equipped weapons or the player character:

| Upgrade Type | Effect | Formula |
|--------------|--------|---------|
| **Weapon Attack Speed** | Reduces all weapon cooldowns by X% | `cooldown *= (1.0 - value/100)` |
| **Weapon Damage** | Increases all weapon damage by X% | `damage *= (1.0 + value/100)` |
| **Player Max HP** | Increases player maximum health | `maxHP += value` |
| **Player Move Speed** | Increases player movement speed by X% | `speed *= (1.0 + value/100)` |
| **New Weapon** | Equips additional weapon (max 4 total) | Instantiates weapon prefab from `WeaponData` |

---

## Weapon-Specific Upgrades

### Greatsword (3 upgrades)

| Upgrade | Effect | Implementation |
|---------|--------|----------------|
| **Mirror Slash** | Spawns 2nd greatsword on opposite side | Create second `GreatswordWeapon` instance at 180° rotation |
| **Executioner's Edge** | +50% damage, +100% knockback (this weapon only) | Multiply `damage` by 1.5, `knockback` by 2.0 |
| **Berserker Fury** | -30% cooldown (this weapon only) | Multiply `cooldown` by 0.7 |

**Technical Notes:**
- Greatsword is sprite-based 180° horizontal slash
- Cannot modify arc width (fixed sprite animation)
- Mirror Slash creates independent weapon instance

---

### AutoCrossbow (3 upgrades)

| Upgrade | Effect | Implementation |
|---------|--------|----------------|
| **Dual Crossbows** | Fire 2 arrows simultaneously at different enemies | Spawn 2 `ArrowProjectile` instances, each targets `FindNearestEnemy()` independently |
| **Piercing Bolts** | Arrows pierce +1 enemy | Increment `ArrowProjectile.pierceCount` by 1 |
| **Triple Barrage** | Fire 3 arrows simultaneously | Spawn 3 arrows. **Requires Player Level 15+** |

**Balance:** Triple Barrage is gated to prevent early-game dominance.

---

### Holy Water (3 upgrades)

| Upgrade | Effect | Implementation |
|---------|--------|----------------|
| **Sanctified Expansion** | Puddle radius +40% | Multiply `DamagePuddle` collider radius by 1.4 |
| **Burning Touch** | Damage scales +6% of enemy max HP | `finalDamage = baseDamage + (enemyMaxHP × 0.06)` per tick |
| **Eternal Flame** | Puddle lasts 2x duration (4s → 8s) | Multiply `baseDuration` by 2.0 |

**Why Scaling?** Makes Holy Water always relevant:
- vs Skeleton (10 HP): 5 + 0.6 = **5.6 damage/tick**
- vs Boss (200 HP): 5 + 12 = **17 damage/tick**

---

### Magic Orbitals (2 upgrades)

| Upgrade | Effect | Implementation |
|---------|--------|----------------|
| **Expanded Orbit** | Orbit radius +40% | Increase `targetRadius` from 2.0m to 2.8m |
| **Overcharged Spheres** | Each orbital does +30% damage | Multiply damage of each `OrbitalBall` by 1.3 |

**Note:** Magic Orbitals spawn with **3 balls by default**. Upgrades enhance effectiveness, not quantity.

---

## Passive Skills

Permanent buffs that **don't take weapon slots**. Max **3 passives per run**.

| Passive | Effect | Unlock Level |
|---------|--------|--------------|
| **Lifesteal** | Heal 3% of damage dealt | Level 5+ |
| **Magnet** | Pickup radius +50% | Level 3+ |
| **Lucky Coins** | Gold drops give +40% more gold | Level 6+ |

**Logic:** 15% chance to appear in level-up pool if unlock condition met.

---

## Stat Boosts

Temporary in-run bonuses. Stackable up to **5 times**.

| Stat | Effect Per Stack | Max Stacks | Notes |
|------|------------------|------------|-------|
| **Might** | +10% damage (character multiplier) | 5 (= +50%) | Stacks **multiplicatively** with Weapon Damage |

**Why only Might?** `PlayerMaxHP` and `PlayerMoveSpeed` already exist as universal upgrades. Might is unique as a character-level damage multiplier.

**Damage Calculation Example:**
```
Base Weapon Damage: 100
Weapon Damage Upgrade (+20%): 100 × 1.2 = 120
Might (3 stacks, +30%): 120 × 1.3 = 156 total damage
```

---

## Upgrade Selection Logic

### Card Pool Generation

**Weights:**
- Weapon-Specific Upgrades: **30%** (if weapon equipped)
- Universal Upgrades: **40%**
- Passive Skills: **15%** (if unlock level met)
- Stat Boosts: **15%**

**Filtering Rules:**
1. Remove "New Weapon" if player has 4 weapons
2. Remove weapon-specific upgrades if weapon not equipped
3. Prevent duplicate cards in same selection
4. Check unlock gates (e.g., Triple Barrage requires Level 15+)

---

## Unlock Gates

To prevent overpowered early builds:

| Upgrade | Requirement |
|---------|-------------|
| **Mirror Slash** | Greatsword equipped for 3+ minutes |
| **Dual Crossbows** | AutoCrossbow equipped for 2+ minutes |
| **Triple Barrage** | AutoCrossbow equipped + **Player Level 15+** |
| **Burning Touch** | Holy Water equipped for 5+ minutes |
| **Expanded Orbit** | Magic Orbitals equipped for 4+ minutes |

**Implementation:** Track weapon equip time using `Time.time` on weapon instantiation.

---

## Data Structure

### UpgradeData ScriptableObject

```csharp
[CreateAssetMenu(fileName = \"Upgrade\", menuName = \"PixelVanguard/Upgrade Data\")]
public class UpgradeData : ScriptableObject {
    [Header(\"Identity\")]
    public string upgradeName;
    public string description;
    public Sprite icon;
    
    [Header(\"Upgrade Type\")]
    public UpgradeType type;
    
    [Header(\"Effect\")]
    public float value; // Amount (e.g., 50 for +50% damage)
    
    [Header(\"Weapon (only for NewWeapon type)\")]
    public WeaponData weaponToEquip;
}

public enum UpgradeType {
    // Existing
    PlayerMoveSpeed,
    PlayerMaxHP,
    WeaponAttackSpeed,
    WeaponDamage,
    NewWeapon,
    
    // Greatsword
    GreatswordMirrorSlash,
    GreatswordDamageBoost,
    GreatswordCooldownBoost,
    
    // Crossbow
    CrossbowDualShot,
    CrossbowTripleShot,
    CrossbowPierce,
    
    // Holy Water
    HolyWaterRadius,
    HolyWaterScaling,
    HolyWaterDuration,
    
    // Magic Orbitals
    OrbitalsExpandedOrbit,
    OrbitalsOverchargedSpheres,
    
    // Passives
    PassiveLifesteal,
    PassiveMagnet,
    PassiveLuckyCoin,
    
    // Stat
    StatMight
}
```

---

## Implementation Notes

### UpgradeManager Changes

```csharp
// Track passive skill count (max 3)
private int passiveSkillCount = 0;

// Track stat boost stacks (max 5 per stat)
private Dictionary<UpgradeType, int> statStacks = new();

// Track weapon equip time for unlock gates
private Dictionary<WeaponType, float> weaponEquipTimes = new();

public void ApplyUpgrade(UpgradeData upgrade) {
    switch (upgrade.type) {
        // Existing cases...
        
        case UpgradeType.GreatswordMirrorSlash:
            ApplyMirrorSlash();
            break;
            
        case UpgradeType.CrossbowDualShot:
            ApplyCrossbowMultiShot(2);
            break;
            
        case UpgradeType.HolyWaterScaling:
            ApplyHolyWaterScaling(0.06f); // 6% scaling
            break;
            
        case UpgradeType.PassiveLifesteal:
            if (passiveSkillCount < 3) {
                ApplyLifesteal(0.03f); // 3% lifesteal
                passiveSkillCount++;
            }
            break;
            
        case UpgradeType.StatMight:
            if (GetStatStack(UpgradeType.StatMight) < 5) {
                ApplyMightStack(0.10f); // +10% damage
                IncrementStatStack(UpgradeType.StatMight);
            }
            break;
    }
}
```

---

## Balancing Philosophy

1. **Early Power Surge** - Universal upgrades (Attack Speed, Damage) provide immediate payoff
2. **Mid-Game Build** - Weapon-specific upgrades create specialized builds (15-30 minutes)
3. **Late-Game Scaling** - HP-based scaling (Holy Water) keeps weapons relevant
4. **Strategic Choice** - Passives vs Stats vs Weapon Upgrades create meaningful decisions

**Target difficulty curve:** Player should survive ~15 minutes on first run, 25+ with optimal builds.
