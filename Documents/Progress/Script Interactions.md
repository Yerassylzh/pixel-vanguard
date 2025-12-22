# Script Interactions

**How scripts communicate.**

---

## Core Flow

```
ServiceLocator (singleton, no deps)
GameEvents (static, no deps)
GameManager (listens to events, controls time)
```

---

## Player Systems

```mermaid
graph LR
    PlayerController[PlayerController] --> PlayerHealth
    PlayerHealth -->|OnPlayerDeath| GameManager
    PlayerHealth -->|OnHealthChanged| HUD[HUD - future]
    PlayerController -.->|uses| InputActions[InputSystem_Actions]
```

**Communication:**
- PlayerHealth fires events, doesn't call GameManager directly
- GameManager listens and reacts (pause game, end game)

---

## Enemy Systems

```mermaid
graph LR
    EnemyAI[EnemyAI] --> EnemyHealth
    EnemyAI --> EnemyAnimationController
    EnemyHealth -->|OnEnemyKilled| GameManager
    EnemyHealth -->|OnXPGained| LevelUpManager
    EnemyAI -.->|finds| Player[Player tag]
```

**Communication:**
- EnemyAI finds Player by tag (GameObject.FindGameObjectWithTag)
- EnemyHealth reads EnemyData.moveSpeed
- Events used for XP, gold, kill count

---

## Event Flow

**Player Death:**
1. `PlayerHealth.Die()` → fires `OnPlayerDeath`
2. `GameManager.OnPlayerDeath()` → calls `EndGame()`
3. `GameManager.EndGame()` → sets `Time.timeScale = 0`

**Enemy Death:**
1. `EnemyHealth.Die()` → fires `OnEnemyKilled`, `OnXPGained`, `OnGoldCollected`
2. `GameManager` updates kill count
3. (Future) LevelUpManager tracks XP

---

## Dependencies

| Script | Depends On |
|--------|-----------|
| PlayerController | Rigidbody2D, InputActions, PlayerAnimationController |
| PlayerHealth | PlayerController |
| EnemyAI | EnemyHealth, Player (tag) |
| EnemyHealth | Rigidbody2D, EnemyData |
| EnemyAnimationController | Animator, EnemyAI |
| WeaponManager | WeaponData inputs, Projectile prefabs |
| GameManager | GameEvents |

---

## Weapon Integrations
- Spawner calls `EnemyHealth.Initialize(EnemyData)`
- Weapons find enemies → call `EnemyHealth.TakeDamage()`
- Knockback direction: `(enemy.pos - weapon.pos).normalized`
