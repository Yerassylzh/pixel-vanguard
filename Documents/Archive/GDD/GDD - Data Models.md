# GDD - Data Models (Future Vision)

**Purpose:** Planned data architecture for full game scope

---

## ScriptableObject Philosophy

**Why ScriptableObjects:**
- Designer-friendly (no code changes for tuning)
- Version control friendly (merge-able assets)
- Memory efficient (single instance, many references)
- Runtime immutable (prevents accidental changes)

---

## Planned Data Models

### CharacterData
**Purpose:** Playable character statistics and configuration

**Planned Fields:**
```csharp
public class CharacterData : ScriptableObject
{
    [Header("Identity")]
    public string characterName;
    public string description;
    public Sprite portrait;
    public GameObject prefab;
    
    [Header("Stats")]
    public float maxHealth;
    public float moveSpeed;
    public float baseDamageMultiplier;
    
    [Header("Starting Equipment")]
    public WeaponData starterWeapon;
    
    // FUTURE EXPANSION
    [Header("Special Abilities")]
    public AbilityData uniqueAbility; // Character-specific skill
    public PassiveData[] startingPassives; // Built-in bonuses
    
    [Header("Unlock Requirements")]
    public bool isUnlockedByDefault;
    public int requiredGold; // For shop unlock
    public Achievement requiredAchievement;
}
```

**Planned Characters (5 total):**
- Knight (current) - Balanced, Greatsword starter
- Ranger - Fast, Crossbow starter
- Mage - High damage, Orbitals starter
- Cleric - Healer, Holy Water starter
- Berserker - Glass cannon, lifesteal boost

---

### WeaponData
**Purpose:** Weapon statistics and behavior configuration

**Planned Fields:**
```csharp
public class WeaponData : ScriptableObject
{
    [Header("Identity")]
    public string weaponID; // Unique identifier
    public string displayName;
    public string description;
    public Sprite icon;
    public WeaponType type;
    
    [Header("Stats")]
    public float baseDamage;
    public float cooldown;
    public float knockback;
    
    [Header("Visuals")]
    public GameObject prefab;
    public RuntimeAnimatorController animator;
    public AudioClip fireSound;
    
    // FUTURE: Evolve system
    [Header("Evolution")]
    public WeaponData evolvedForm; // Ultimate upgrade
    public UpgradeData[] requiredUpgrades; // Prerequisites
}
```

**Planned Weapon Evolution:**
- Greatsword → Executioner's Blade (lifesteal + AoE)
- Crossbow → Ballista (massive damage, slow)
- Holy Water → Divine Wrath (instant explosions)
- Orbitals → Supernova (expanding blast)

---

### EnemyData
**Purpose:** Enemy statistics and behavior

**Planned Expansion:**
```csharp
public class EnemyData : ScriptableObject
{
    // Current fields stay...
    
    // FUTURE ADDITIONS
    [Header("Boss Mechanics")]
    public bool isBoss;
    public BossPhase[] phases; // Multi-phase bosses
    public float[] phaseHealthThresholds;
    
    [Header("Special Behaviors")]
    public AIType aiType; // CHASE, RANGED, TELEPORT, SUMMON
    public GameObject projectilePrefab; // For ranged enemies
    public EnemyData summonedEnemy; // For summoner enemies
    
    [Header("Rare Variants")]
    public EnemyData eliteVariant; // Stronger version
    public float eliteSpawnChance; // % to spawn elite instead
}
```

**Planned Enemy Types (15 total):**
- Basic: Skeleton, Crawler, Goblin, Ghost, Slime (current)
- Armored: ArmoredOrc (current), Knight, Shieldbearer
- Ranged: Archer, Mage, Necromancer
- Special: Summoner, Teleporter, Charger
- Bosses: 2-3 unique bosses

---

### UpgradeData
**Purpose:** Upgrade effects and metadata

**Planned Evolution System:**
```csharp
public class UpgradeData : ScriptableObject
{
    // Current fields stay...
    
    // FUTURE: Upgrade tiers
    [Header("Tier System")]
    public int tier; // 1-3
    public UpgradeData nextTier; // Tier 2 → Tier 3
    
    [Header("Synergies")]
    public UpgradeData[] synergyUpgrades; // Highlighted if owned
    public string synergyDescription; // "With Dual Shot: ..."
    
    [Header("Mutations")]
    public MutationData[] possibleMutations; // Random modifiers
}
```

**Planned Upgrade Categories:**
- Tier 1: Current upgrades
- Tier 2: Evolved versions (Triple → Quad Shot)
- Tier 3: Ultimate forms (Transcendent abilities)
- Mutations: Random powerful modifiers (risk/reward)

---

## Future Data Models

### MetaProgressionData
**Purpose:** Permanent upgrades between runs

```csharp
public class MetaUpgradeData : ScriptableObject
{
    public string upgradeName;
    public int goldCost;
    public int maxLevel;
    
    public enum MetaType
    {
        StartingGold,
        StartingXP,
        BonusHealth,
        BonusDamage,
        ReviveToken,
        UnlockCharacter,
        UnlockWeapon
    }
    
    public MetaType type;
    public float valuePerLevel;
}
```

### ChallengeData
**Purpose:** Daily/weekly challenges

```csharp
public class ChallengeData : ScriptableObject
{
    public string challengeName;
    public string description;
    public Sprite icon;
    
    public ChallengeType type; // SURVIVE_TIME, KILL_COUNT, NO_DAMAGE
    public float targetValue;
    public RewardData reward;
}
```

### MapData
**Purpose:** Different arenas/biomes

```csharp
public class MapData : ScriptableObject
{
    public string mapName;
    public Sprite preview;
    public GameObject scenePrefab;
    
    public EnemyData[] enemyPool; // Biome-specific enemies
    public float difficultyMultiplier;
    public AudioClip bgm;
}
```

---

## Extensibility Principles

**For Future Features:**
1. Don't delete fields - mark `[Obsolete]` instead
2. Use `List<>` for variable-length data
3. Add `[Header("Future")]` for planned expansions
4. Keep backward compatibility for save files

**Naming Conventions:**
- Prefix: `CD_` CharacterData, `WD_` WeaponData, etc.
- Example: `CD_Knight`, `WD_Greatsword`, `ED_Skeleton`

---

## Data Validation

**Editor Scripts (Planned):**
- Validate unique IDs
- Check circular references
- Ensure all prefabs assigned
- Balance testing tools

**Runtime Validation:**
- Log warnings for missing data
- Graceful fallbacks (default values)
- Debug mode: Show data in inspector
