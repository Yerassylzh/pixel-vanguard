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
    OrbitingMelee, // Greatsword, Magic Orbitals
    Projectile,    // Crossbow
    AreaDenial     // Molotov
}

[Serializable]
public class UpgradeScaling {
    public int level; // 2, 3, 4...
    public float damageMultiplier = 1.2f; // +20% damage
    public float cooldownReduction = 0.9f; // -10% cooldown
    public string specialUpgrade; // "Adds 1 more orbital"
}
```

**Example Asset (Greatsword):**

| Level | Damage Mult | Cooldown | Special |
|-------|-------------|----------|---------|
| 1 | 1.0x | 1.0s | Base |
| 2 | 1.2x | 0.9s | Wider arc |
| 3 | 1.5x | 0.8s | +10% knockback |
| 4 | 2.0x | 0.7s | Double swing |

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
    
    [Header("Stats")]
    public float maxHealth = 20f;
    public float moveSpeed = 3f;
    public float contactDamage = 5f;
    public float weightResistance = 0f; // 0-1, higher = harder to knock back
    
    [Header("Loot")]
    public int xpDrop = 5;
    public int goldDrop = 1;
    public float healthPotionDropChance = 0.05f; // 5%
    
    [Header("Spawn Config")]
    public int spawnWeight = 10; // Higher = more common
    public int minGameTime = 0; // Seconds, when this enemy starts spawning
    
    [Header("Visuals")]
    public GameObject prefab;
    public RuntimeAnimatorController animator;
}
```

**Example Enemy Roster:**

| Enemy | HP | Speed | XP | Weight | Min Time |
|-------|----|----|----|----|----------|
| Skeleton | 20 | 3 | 5 | 10 | 0:00 |
| Crawler | 15 | 6 | 8 | 15 | 1:00 |
| Armored Orc | 50 | 2 | 15 | 5 | 3:00 |
| Abomination (Boss) | 500 | 1 | 100 | 2 | 5:00 |

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
│   │   └── Ranger.asset
│   ├── Weapons/
│   │   ├── OrbitalGreatsword.asset
│   │   ├── AutoCrossbow.asset
│   │   ├── Molotov.asset
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
