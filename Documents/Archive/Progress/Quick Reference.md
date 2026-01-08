# Quick Reference - Common Tasks

**Purpose:** Fast lookup for frequent operations

## File Locations

**Player:** `Assets/Scripts/Gameplay/Player/`  
**Weapons:** `Assets/Scripts/Gameplay/Weapons/`  
**Enemies:** `Assets/Scripts/Gameplay/Enemies/`  
**Upgrades:** `Assets/Scripts/Gameplay/Upgrades/`  
**Data:** `Assets/Scripts/Data/`  
**ScriptableObjects:** `Assets/ScriptableObjects/`

## Adding New Upgrade

1. **Create ScriptableObject:**
   - Right-click in `/ScriptableObjects/Upgrades/`
   - Create → PixelVanguard → UpgradeData
   - Set: name, description, icon, type, value, weaponToEquip, rarityWeight

2. **Add to UpgradeType Enum** (if new type):
   ```csharp
   // Assets/Scripts/Data/UpgradeData.cs
   public enum UpgradeType { ..., YourNewType }
   ```

3. **Implement in UpgradeApplicator:**
   ```csharp
   // Assets/Scripts/Gameplay/Upgrades/UpgradeApplicator.cs
   case Data.UpgradeType.YourNewType:
       ApplyYourUpgrade(upgrade.value);
       break;
   ```

4. **Add Validation** (if needed):
   ```csharp
   // Assets/Scripts/Gameplay/Upgrades/UpgradeValidator.cs
   case UpgradeType.YourNewType:
       if (!someCondition) return false;
       break;
   ```

5. **Assign in Inspector:**
   - Add to `UpgradeManager.allUpgrades[]` array

## Adding New Weapon

1. **Create WeaponData:**
   - Right-click in `/ScriptableObjects/Weapons/`
   - Create → PixelVanguard → WeaponData
   - Set stats (damage, cooldown, knockback, etc.)

2. **Create Script:**
   ```csharp
   public class YourWeapon : WeaponBase
   {
       protected override void Fire() {
           // Implementation
       }
   }
   ```

3. **Create Prefab:**
   - Add script to GameObject
   - Assign sprite, collider, etc.
   - Save as prefab in `/Prefabs/Weapons/`

4. **Register in WeaponManager:**
   - Add to `WeaponType` enum
   - Add prefab field: `[SerializeField] private GameObject yourWeaponPrefab;`
   - Add case in `InstantiateWeaponByType()`

## Adding New Enemy

1. **Create EnemyData:**
   - Right-click in `/ScriptableObjects/Enemies/`
   - Create → PixelVanguard → EnemyData
   - Set: stats, drop rates, spawn weight, prefab

2. **Create Prefab:**
   - Add: SpriteRenderer, Rigidbody2D, Collider2D
   - Add: EnemyAI, EnemyHealth, EnemyAnimationController
   - Tag: "Enemy"
   - Layer: "Enemy"

3. **Assign in EnemySpawner:**
   - Add to `enemyTypes[]` array

## Modifying Balance

**Player Stats:**
- `/ScriptableObjects/Characters/Knight.asset`
- Modify: maxHealth, baseMove Speed, baseDamageMultiplier

**Weapon Stats:**
- `/ScriptableObjects/Weapons/[WeaponName].asset`
- Modify: damage, cooldown, knockback, projectileSpeed, etc.

**Enemy Stats:**
- `/ScriptableObjects/Enemies/[EnemyName].asset`
- Modify: maxHealth, moveSpeed, damage, weightResistance, drop rates

**Upgrade Values:**
- `/ScriptableObjects/Upgrades/[UpgradeName].asset`
- Modify: value, rarityWeight

## Testing Specific Systems

**Test Upgrades:**
- Enable debug: Check `UpgradeManager` logs in Console
- Force specific upgrade: Temporarily make it 1000 weight

**Test Weapons:**
- Modify cooldown to 0.1s for rapid testing
- Increase damage to 9999 for one-shot testing

**Test Spawning:**
- `EnemySpawner.spawnInterval` = 0.1f for stress test
- `EnemySpawner.maxEnemies` = 10 for performance test

## Common Queries

**Get Player Instance:**
```csharp
PlayerController player = PlayerController.Instance;
```

**Get All Equipped Weapons:**
```csharp
var weapons = FindObjectOfType<WeaponManager>().GetEquippedWeapons();
```

**Check Game State:**
```csharp
bool isPlaying = GameManager.Instance.CurrentState == GameState.Playing;
```

**Fire Event:**
```csharp
Core.GameEvents.TriggerXPGained(50);
```

**Access Selected Character:**
```csharp
var character = Core.CharacterManager.SelectedCharacter;
```
