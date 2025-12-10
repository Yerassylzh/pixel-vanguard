# Quick Reference

**API cheat sheet.**

---

## File Structure

```
Scripts/
├── Core/           ServiceLocator, GameEvents
├── Services/       IAdService, ISaveService, IPlatformService, SaveData, NoAdService
├── Data/           CharacterData, WeaponData, EnemyData
└── Gameplay/       GameManager, PlayerController, PlayerHealth, EnemyHealth, EnemyAI
```

---

## Code Patterns

**Get Service:**
```csharp
var adService = ServiceLocator.Get<IAdService>();
```

**Fire Event:**
```csharp
GameEvents.TriggerPlayerDeath();
```

**Listen to Event:**
```csharp
void OnEnable() { GameEvents.OnPlayerDeath += HandleDeath; }
void OnDisable() { GameEvents.OnPlayerDeath -= HandleDeath; }
```

**RequireComponent:**
```csharp
[RequireComponent(typeof(Rigidbody2D))]
void Awake() { rb = GetComponent<Rigidbody2D>(); }
```

---

## Key APIs

**GameManager:**
```csharp
GameManager.Instance.CurrentState    // GameState enum
GameManager.Instance.PauseGame()
GameManager.Instance.EndGame(reason)
```

**PlayerHealth:**
```csharp
playerHealth.TakeDamage(float)
playerHealth.Heal(float)
playerHealth.GrantInvincibility(float)
```

**EnemyHealth:**
```csharp
enemyHealth.Initialize(EnemyData)
enemyHealth.TakeDamage(damage, dir, force)
enemyHealth.IsAlive               // bool
enemyHealth.ContactDamage         // float
```

**EnemyAI:**
```csharp
enemyAI.SetMoveSpeed(float)
```

**OrbitingWeapon:**
```csharp
orbitingWeapon.UpgradeToLevel(int)
```

---

## Events

```csharp
// Game Flow
OnGameStart, OnGamePause, OnGameResume, OnGameOver

// Player
OnPlayerHealthChanged(current, max)
OnPlayerDeath
OnPlayerLevelUp

// Combat
OnEnemyKilled(totalKills)
OnXPGained(amount)
OnGoldCollected(amount)

// Weapons (future)
OnWeaponEquipped(id), OnWeaponUpgraded(id, level)
```

---

## Common Operations

**Damage Enemy:**
```csharp
var enemy = hit.GetComponent<EnemyHealth>();
Vector2 dir = (enemy.transform.position - transform.position).normalized;
enemy.TakeDamage(damage, dir, knockbackForce);
```

**Find Player:**
```csharp
player = GameObject.FindGameObjectWithTag("Player").transform;
```

**Pause Game:**
```csharp
Time.timeScale = 0f;  // Pauses physics, movement, animations
```

---

## Important Values

- **Player Speed:** 5.0
- **Enemy Speed:** 2.5-3.0 (slower than player!)
- **Damage Cooldown:** 1.0 second
- **Default HP:** 100

---

## Setup Requirements

**Player GameObject:**
- Tag: "Player"
- Components: PlayerController, PlayerHealth, Rigidbody2D (auto-added)
- InputSystem_Actions assigned

**Enemy GameObject:**
- Tag: "Enemy"
- Components: EnemyAI, EnemyHealth, Rigidbody2D (auto-added)
- EnemyData asset assigned

**GameScene:**
- GameObject with GameManager script
