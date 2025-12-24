# GDD - Data Models Reference
**Parent:** [Main Game Design Document](file:///c:/Users/Honor/Unity%20Games/Pixel%20Vanguard/Documents/Game%20Design%20Document%20-%20Main.md)

---

## Overview

Pixel Vanguard uses **ScriptableObjects** for all game data. This allows designers to tweak balance without touching code.

---

## Character Data

**Asset Type:** `CharacterData.asset`

**Schema:**

```csharp
[CreateAssetMenu(fileName = "Character", menuName = "PixelVanguard/Character")]
public class CharacterData : ScriptableObject {
    [Header("Identity")]
    public string characterID; // e.g., "knight"
    public string displayName; // e.g., "The Knight"
    public Sprite portrait; // For menus
    public Sprite idleSprite; // For MainMenu background
    
    [Header("Base Stats")]
    public float maxHealth = 100f;
    public float moveSpeed = 5f;
    public float baseDamageMultiplier = 1f;
    
    [Header("Starting Loadout")]
    public WeaponData starterWeapon;
    
    [Header("Unlock Requirement")]
    public UnlockType unlockType; // Gold, Ads, FreeStarter
    public int goldCost; // If unlockType == Gold
    public int adWatchCount; // If unlockType == Ads
}

public enum UnlockType {
    FreeStarter, // Always unlocked (Knight)
    Gold,
    Ads
}
```

**Example Asset:**

| Field | Value |
|-------|-------|
| characterID | `"knight"` |
| displayName | `"The Knight"` |
| maxHealth | `100` |
| moveSpeed | `5` |
| starterWeapon | `OrbitalGreatsword.asset` |
| unlockType | `FreeStarter` |

---

## Character Roster

**Total Characters:** 5

| Character | ID | HP | Speed | Damage Mult | Starter Weapon | Unlock Type | Cost/Requirement |
|-----------|----|----|-------|-------------|----------------|-------------|------------------|
| **The Knight** | `knight` | 100 | 5.0 | 1.0x | Greatsword | FreeStarter | Always unlocked |
| **The Pyromancer** | `pyromancer` | 80 | 5.0 | 1.2x | HolyWater | FreeStarter | Always unlocked |
| **The Ranger** | `ranger` | 100 | 6.5 | 1.0x | AutoCrossbow | Gold | 1500 gold |
| **The Zombie** | `zombie` | 130 | 4.0 | 0.9x | Greatsword | Gold | 2500 gold |
| **Santa Claus** | `santa` | 90 | 6.0 | 1.1x | MagicOrbitals | Ads | Watch 10 ads |

**Design Philosophy:**
- **Knight**: Balanced starter (HP: 100, Speed: 5.0, Damage: 1.0x)
- **Pyromancer**: Glass cannon (Low HP, High Damage)
- **Ranger**: Speed demon (Fast movement, standard damage)
- **Zombie**: Tank (High HP, Slow, Reduced damage)
- **Santa**: Agile support (Fast, Gift-themed orbitals)

---

## Weapon Data

**Asset Type:** `WeaponData.asset`

**Schema:**

```csharp
[CreateAssetMenu(fileName = "Weapon", menuName = "PixelVanguard/Weapon")]
public class WeaponData : ScriptableObject {
    [Header("Identity")]
    public string weaponID; // e.g., "greatsword"
    public string displayName; // e.g., "Orbiting Greatsword"
    public string description; // For card display
    public Sprite icon;
    
    [Header("Behavior Type")]
    public WeaponType type;
    
    [Header("Base Stats (Level 1)")]
    public float baseDamage = 10f;
    public float cooldown = 1f; // Seconds between attacks
    public float knockback = 5f;
    
    [Header("Upgrade Scaling")]
    public UpgradeScaling[] upgrades; // Level 2, 3, 4, etc.
    
    [Header("Prefab")]
    public GameObject prefab; // Visual projectile/effect
}

public enum WeaponType {
    Greatsword,      // Periodic swing attack
    MagicOrbitals,   // Continuous orbit shields
    Crossbow,        // Fires arrows
    HolyWater        // Throws puddle flask/Molotov
}

[Serializable]
public class UpgradeScaling {
    public int level; // 2, 3, 4...
    public float damageMultiplier = 1.2f; // +20% damage
    public float cooldownReduction = 0.9f; // -10% cooldown
    public string specialUpgrade; // Description of special effect
}
```

---

## Weapon Roster

**Max Equipped:** 4 weapons simultaneously  
**Starting Weapon:** Orbiting Greatsword (always)

### 1. Greatsword

**Type:** Greatsword  
**Script:** GreatswordWeapon.cs  
**Behavior:** Periodic 360° swing attack  
**Attack Pattern:** Rests at side → Swings → Returns to rest  
**Damage:** 15 (high)  
**Cooldown:** 2.5s (between swings)  
**Knockback:** 100 (high)

| Level | Damage | Cooldown | Special Effect |
|-------|--------|----------|----------------|
| 1 | 15 | 1.5s | Basic 180° arc |
| 2 | 18 (+20%) | 1.3s | Wider 240° arc |
| 3 | 23 (+25%) | 1.1s | Full 360° coverage |
| 4 | 30 (+30%) | 0.9s | Double spin speed |

**Upgrade Path:** Faster attacks → Wider coverage → Full circle protection

---

### 2. AutoCrossbow

**Type:** Crossbow  
**Script:** AutoCrossbowWeapon.cs  
**Projectile:** ArrowProjectile.cs  
**Behavior:** Fires arrows at nearest enemy  
**Damage:** 10 (medium)  
**Cooldown:** 1.0s  
**Knockback:** 45 (medium)

| Level | Damage | Cooldown | Arrows | Pierce |
|-------|--------|----------|--------|--------|
| 1 | 10 | 1.0s | 1 | 0 |
| 2 | 12 (+20%) | 1.0s | 2 (Double shot) | 0 |
| 3 | 15 (+25%) | 0.8s | 2 | 1 enemy |
| 4 | 20 (+33%) | 0.8s | 3 (Triple shot) | 2 enemies |

**Upgrade Path:** More arrows → Pierce enemies → Triple shot

---

### 3. HolyWater

**Type:** HolyWater  
**Script:** HolyWaterWeapon.cs  
**Puddle:** DamagePuddle.cs  
**Behavior:** Throws flask creating damage puddle  
**Damage:** 5/tick (DoT)  
**Cooldown:** 3.0s  
**Knockback:** 20

| Level | Damage/Tick | Duration | Puddle Radius | Tick Rate |
|-------|-------------|----------|---------------|-----------|
| 1 | 5 | 3s | 1.5m | 0.5s |
| 2 | 7 (+40%) | 4s | 2.0m | 0.5s |
| 3 | 10 (+43%) | 5s | 2.5m | 0.4s |
| 4 | 15 (+50%) | 6s | 3.0m | 0.3s |

**Upgrade Path:** Longer duration → Larger puddle → Faster damage ticks

---

### 4. MagicOrbitals

**Type:** MagicOrbitals  
**Script:** MagicOrbitalsWeapon.cs  
**Behavior:** Shields continuously orbit player  
**Damage:** 8 (low-medium)  
**Cooldown:** 0.5s (continuous damage)  
**Knockback:** 50 (medium)

| Level | Damage | Shield Count | Orbit Speed | Orbit Radius |
|-------|--------|--------------|-------------|--------------|
| 1 | 8 | 1 | 90°/s | 2.0m |
| 2 | 10 (+25%) | 2 | 90°/s | 2.0m |
| 3 | 13 (+30%) | 3 | 120°/s | 2.2m |
| 4 | 18 (+38%) | 3 | 150°/s | 2.5m |

**Upgrade Path:** More shields (1→2→3) → Faster rotation → Wider coverage

---

## Weapon System Rules

**Acquisition:**
- Player starts with **Orbiting Greatsword** (always equipped)
- Can hold up to **4 weapons simultaneously**
- New weapons offered via level-up cards
- Cannot unequip weapons during a run

**Auto-Fire:**
- All weapons fire automatically based on individual cooldowns
- No player input required (mobile-friendly)
- Weapons operate independently

**Upgrade Priority:**
- If player has < 4 weapons: Level-up offers new weapon cards
- If player has weapons: Level-up offers upgrade cards for existing weapons
- Each weapon maxes at Level 4

---

## Enemy Data

**Asset Type:** `EnemyData.asset`

**Schema:**

```csharp
[CreateAssetMenu(fileName = "Enemy", menuName = "PixelVanguard/Enemy")]
public class EnemyData : ScriptableObject {
    [Header("Identity")]
    public string enemyID; // e.g., "skeleton"
    public string displayName;
    public string enemyName; // Added Dec 24 - Name property
    
    [Header("Stats")]
    public float maxHealth = 20f;
    public float moveSpeed = 3f;
    public float contactDamage = 5f;
    public float weightResistance = 0f; // 0-1, higher = harder to knock back
   
    [Header("Loot Drops")]
    public int xpDrop = 5;
    public int goldDrop = 1;
    public float healthPotionDropChance = 0.05f; // 5%
    
    [Header("Loot Prefabs (ADDED DEC 24)")]
    public GameObject xpGemPrefab;
    public GameObject goldCoinPrefab;
    public GameObject healthPotionPrefab;
    
    [Header("Spawn Config")]
    public int spawnWeight = 10; // Higher = more common
    public int minGameTime = 0; // Seconds, when this enemy starts spawning
    
    [Header("Visuals")]
    public GameObject prefab;
    public RuntimeAnimatorController animator;
}
```

## Enemy Roster

| Enemy | ID | HP | Speed | Damage | XP | Gold % | Potion % | Weight | Resistance | Min Time |
| :--- | :--- | :--- | :--- | :--- | :--- | :--- | :--- | :--- | :--- | :--- |
| **Skeleton Grunt** | `skeleton_grunt` | 10 | 1.25 | 5 | 5 | 25% | 3% | 10 | 0.0 | 0:00 |
| **Crawler** | `crawler` | 15 | 1.6 | 8 | 8 | 30% | 4% | 5 | 0.1 | 1:00 |
| **Goblin** | `goblin` | 25 | 2.2 | 7 | 5 | 35% | 5% | 10 | 0.0 | 1:30 |
| **The Ghost** | `ghost` | 30 | 1.5 | 5 | 12 | 40% | 6% | 10 | 0.0 | 2:30 |
| **Armored Orc** | `armored_orc` | 80 | 1.2 | 10 | 15 | 80% | 8% | 3 | 0.7 | 2:00 |
| **The Abomination (Boss)** | `abomination` | 200 | 1.75 | 20 | 50 | 100% | 25% | 1 | 0.9 | 3:00 |
| **Slime** | `slime` | 80 | 1.7 | 5 | 20 | 60% | 10% | 10 | 0.0 | 4:30 |

---

## Stat Upgrade Data

**Asset Type:** `StatUpgradeData.asset`

**Purpose:** Permanent meta-progression upgrades in the shop

**Schema:**

```csharp
[CreateAssetMenu(fileName = "StatUpgrade", menuName = "PixelVanguard/StatUpgrade")]
public class StatUpgradeData : ScriptableObject {
    [Header("Identity")]
    public string statID; // e.g., "might"
    public string displayName; // "Might"
    public string description; // "Increases base damage"
    public Sprite icon;
    
    [Header("Progression")]
    public int maxLevel = 10;
    public StatUpgradeLevel[] levels;
}

[Serializable]
public class StatUpgradeLevel {
    public int level; // 1, 2, 3...
    public int goldCost; // Gets more expensive each level
    public float effectValue; // e.g., +10% damage
}
```

**Example Stat: Might**

| Level | Cost | Effect |
|-------|------|--------|
| 1 | 500 | +10% damage |
| 2 | 1000 | +10% damage |
| 3 | 1500 | +10% damage |
| ... | ... | ... |
| 10 | 10000 | +10% damage |

**Total at Level 10:** +100% damage (2x multiplier)

**All Stats:**

| Stat ID | Display Name | Effect |
|---------|--------------|--------|
| `might` | Might | +10% base damage per level |
| `vitality` | Vitality | +10 max HP per level |
| `greaves` | Greaves | +5% move speed per level |
| `magnet` | Magnet | +0.5 pickup radius per level |
| `luck` | Luck | +2% crit chance per level |

---

## Card Data (Dynamic)

**Not a ScriptableObject** - Generated at runtime

**Schema:**

```csharp
[Serializable]
public class CardData {
    public CardType type;
    public Sprite icon;
    public string title;
    public string description;
    public object payload; // WeaponData, StatUpgradeData, or UpgradeScaling
}

public enum CardType {
    NewWeapon,    // Unlock a weapon
    UpgradeWeapon, // Level up existing weapon
    StatBoost     // Temporary in-run stat boost
}
```

**Example Cards:**

| Type | Title | Description | Payload |
|------|-------|-------------|---------|
| NewWeapon | "Auto-Crossbow" | "Fires arrows at nearest enemy" | `CrossbowWeaponData` |
| UpgradeWeapon | "Greatsword Lv. 2" | "+20% damage, wider arc" | `UpgradeScaling(level=2)` |
| StatBoost | "Might +10%" | "Increases damage for this run" | `StatType.Might, value=0.1` |

**Card Generation Logic:**
```
Available Pool:
1. For each equipped weapon → Add upgrade card (if not max level)
2. If < 4 weapons equipped → Add random weapon from pool
3. Always add 2-3 stat boost cards

Selection:
- Randomly pick 3 from available pool
- No duplicates
```

---

## Save Data

**Not a ScriptableObject** - Serialized JSON

**Schema:**

```csharp
[Serializable]
public class SaveData {
    public int version = 1;
    public int totalGold;
    public List<string> unlockedCharacterIDs;
    public string selectedCharacterID;
    public Dictionary<string, int> statLevels; // "might" → 5
    public Dictionary<string, int> adWatchProgress; // "pyromancer" → 2/5
}
```

**Storage:**
- **Android:** PlayerPrefs → `Application.persistentDataPath/save.json`
- **Yandex:** Cloud save + PlayerPrefs fallback
- **Desktop:** PlayerPrefs

---

## Difficulty Scaling Data

**Asset Type:** `DifficultyConfig.asset`

**Purpose:** Control enemy spawn rates over time

**Schema:**

```csharp
[CreateAssetMenu(fileName = "DifficultyConfig", menuName = "PixelVanguard/DifficultyConfig")]
public class DifficultyConfig : ScriptableObject {
    public DifficultyPhase[] phases;
}

[Serializable]
public class DifficultyPhase {
    public int startTimeSeconds; // When this phase begins
    public int enemiesPerWave; // How many spawn per wave
    public float spawnInterval; // Seconds between waves
    public EnemyWeightOverride[] enemyWeights; // Adjust spawn weights
}

[Serializable]
public class EnemyWeightOverride {
    public string enemyID;
    public int weight; // Override default spawn weight
}
```

**Example Config:**

| Phase | Start Time | Enemies/Wave | Interval | Weights |
|-------|------------|--------------|----------|---------|
| 1 | 0:00 | 10 | 5s | Skeleton: 100 |
| 2 | 1:00 | 20 | 4s | Skeleton: 70, Crawler: 30 |
| 3 | 3:00 | 40 | 3s | Skeleton: 50, Crawler: 30, Orc: 20 |
| 4 | 5:00+ | 60 | 2s | All enemies + Boss |

---

## Asset Organization

**Folder Structure:**

```
Assets/
├── ScriptableObjects/
│   ├── Characters/
│   │   ├── Knight.asset
│   │   ├── Pyromancer.asset
│   │   ├── Ranger.asset
│   │   ├── Zombie.asset
│   │   └── Santa.asset
│   ├── Weapons/
│   │   ├── OrbitalGreatsword.asset
│   │   ├── AutoCrossbow.asset
│   │   ├── HolyWater.asset
│   │   └── MagicOrbitals.asset
│   ├── Enemies/
│   │   ├── Skeleton.asset
│   │   ├── Crawler.asset
│   │   ├── ArmoredOrc.asset
│   │   └── Abomination.asset
│   ├── Stats/
│   │   ├── Might.asset
│   │   ├── Vitality.asset
│   │   ├── Greaves.asset
│   │   ├── Magnet.asset
│   │   └── Luck.asset
│   └── Configs/
│       ├── DifficultyConfig.asset
│       └── ServiceConfiguration.asset
```

---

## Data Validation

**Inspector Validation:**

All ScriptableObjects have `OnValidate()` to catch errors:

```csharp
void OnValidate() {
    if (string.IsNullOrEmpty(weaponID)) {
        Debug.LogError($"WeaponData missing weaponID!", this);
    }
    
    if (baseDamage <= 0) {
        Debug.LogWarning($"WeaponData {weaponID} has 0 damage!", this);
    }
}
```

**Editor Tools:**

Custom inspector buttons:
- "Test Spawn Enemy" - Instantiate in editor
- "Preview Upgrade Path" - Show stat progression table
