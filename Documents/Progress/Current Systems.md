# Current Systems

**Brief overview of implemented systems.**

---

## GameManager
- **Purpose:** Master game state controller
- **State:** Initializing → Playing → GameOver
- **Does:** Tracks time, kills, gold; pauses game on death/level-up
- **Events:** Listens to OnPlayerDeath, OnEnemyKilled, OnGoldCollected

---

## PlayerController
- **Purpose:** Player movement
- **Input:** New Input System (WASD, gamepad, touch)
- **Speed:** 5.0 units/sec (from CharacterData)
- **Does:** Applies velocity, flips sprite

---

## PlayerHealth
- **Purpose:** HP management
- **HP:** 100 (default)
- **Damage Cooldown:** 1 second
- **Does:** Takes damage from enemy contact, fires OnPlayerDeath when HP = 0

---

## EnemyAI
- **Purpose:** Make enemies chase player
- **Does:** Finds Player by tag, moves toward player
- **Speed:** From EnemyData.moveSpeed (should be 2.5-3.0)
- **Stops:** When game paused or enemy dead

---

## EnemyHealth
- **Purpose:** Enemy HP and loot
- **Does:** Takes damage, applies knockback, drops XP/gold (events)
- **Knockback:** Based on `weightResistance` (0-1)

---

## EnemySpawner
- **Purpose:** Spawn enemies continuously
- **Spawning:** At screen edges, outside camera view
- **Difficulty:** Scales with GameManager.GameTime (centralized)
- **Selection:** Weighted random based on EnemyData.spawnWeight
- **Limit:** Max 100 enemies on screen

---

## OrbitingWeapon
- **Purpose:** Player's weapon
- **Does:** Orbits player using sin/cos math, damages enemies on contact
- **Orbit:** Radius 2.0, speed 180°/sec
- **Damage:** From WeaponData, has cooldown between hits
- **Stops:** When game paused

---

## XPGem
- **Purpose:** XP pickup from dead enemies
- **Magnet:** Pulls toward player within 3 units
- **Collection:** Trigger collision with player
- **Speed:** 10 units/sec when pulled
- **Value:** Set by EnemyData.xpDrop

---

## Camera
- **Solution:** Cinemachine Virtual Camera (Unity built-in)
- **Follow:** Player with damping
- **No custom code needed**

---

## HUD
- **Purpose:** Visual feedback during gameplay
- **Displays:** HP bar, XP bar, level, timer, kill count
- **Updates:** Listens to GameEvents for real-time changes
- **XP Tracking:** Tracks XP locally, fires level-up event
- **Scaling:** XP requirement increases per level (1.2x multiplier)

---

## LevelUpPanel
- **Purpose:** Pause game and show upgrade choices
- **Trigger:** OnPlayerLevelUp event
- **Options:** 3 buttons (1 functional, 2 placeholders)
- **Upgrade:** Increases greatsword orbit speed by 30°/sec
- **Pauses:** Calls GameManager.PauseGame(), resumes with GameManager.ResumeGame()

---

## GameEvents
- **Purpose:** Decoupled communication
- **Pattern:** Publish-subscribe
- **Events:** OnPlayerDeath, OnEnemyKilled, OnXPGained, OnGoldCollected, etc.

---

## ServiceLocator
- **Purpose:** Dependency injection
- **Usage:** `ServiceLocator.Get<IAdService>()`
- **Status:** Ready, no services registered yet

---

## Data Models

**CharacterData:**
- Stats: HP, moveSpeed, baseDamageMultiplier
- Starter weapon, unlock requirements

**WeaponData:**
- Type, damage, cooldown, knockback
- `GetStatsForLevel(int)` for upgrades

**EnemyData:**
- HP, speed, damage, resistance
- Loot: XP, gold, potion drop chance
