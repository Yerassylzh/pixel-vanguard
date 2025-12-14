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
- **Purpose:** Player movement with platform-aware input
- **Desktop Input:** WASD + Arrow keys via Input Actions
- **Mobile Input:** VirtualJoystick (touch)
- **State Management:** Blocks input when paused/level-up/game-over
- **Speed:** 5.0 units/sec, diagonal normalized (no speed boost)
- **Platform Switching:** Responds to OnPlatformChanged event

---

## VirtualJoystick
- **Purpose:** Touch controls for mobile
- **Type:** Floating (appears at touch, hides when released)
- **State Blocking:** Disabled during pause/level-up/game-over
- **Raycast Management:** Disables raycastTarget when blocked (allows pause menu clicks)
- **Output:** Normalized direction vector (-1 to 1)
- **Auto-hide:** Invisible on desktop, responds to platform changes
- **Handle Range:** 50 units (configurable)

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

## Weapon System
- **Purpose:** Multi-weapon auto-fire combat
- **Types:** 4 weapon types, up to 4 equipped simultaneously
  - **Greatsword**: Periodic 360° swing attack (melee)
  - **Auto Crossbow**: Fires arrows at nearest enemy (projectile)
  - **Holy Water**: Throws flask creating damage puddle (area denial)
  - **Magic Orbitals**: Orbiting shields that damage on contact
- **Auto-fire:** All weapons fire automatically based on cooldown
- **Upgrades:** Damage, attack speed, knockback modifiers
- **Acquisition:** Obtain new weapons via level-up upgrades (max 4)

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

## UpgradeManager
- **Purpose:** Select and apply upgrades on level up
- **Selection:** Random 3 from pool of UpgradeData assets
- **Smart Filtering:** Won't show already-equipped weapons or when max (4) reached
- **Types:** Move speed, max HP, weapon attack speed, weapon damage, new weapon
- **Application:** Applies to player stats OR all equipped weapons
- **Data-driven:** ScriptableObject-based upgrade definitions

---

## LevelUpPanel
- **Purpose:** Pause game and show upgrade choices
- **Trigger:** OnPlayerLevelUp event
- **Options:** 3 random upgrades from UpgradeManager
- **Display:** Shows name + description on buttons
- **Pauses:** Calls GameManager.PauseGame(), resumes with GameManager.ResumeGame()

---

## GameOverScreen
- **Purpose:** Display session results when player dies
- **Trigger:** OnGameOver event from GameManager
- **Stats Displayed:** Survival time, kill count, level reached
- **Actions:** Restart button (reloads scene), main menu button (placeholder)
- **Session Data:** Reads from GameManager.CurrentSession

---

## PlatformDetector
- **Purpose:** Detect and manage platform type
- **Modes:** AutoDetect, AlwaysMobile (default), AlwaysDesktop, ForceSpecific
- **Types:** Desktop, Native Mobile, Web Mobile
- **Runtime Switching:** ForcePlatform() method triggers OnPlatformChanged event
- **Singleton:** Global access via Instance
- **Testing:** Set to AlwaysMobile for development, AutoDetect for production

---

## PauseMenu
- **Purpose:** Platform-aware pause with proper input management
- **Desktop:** ESC key only (Input Actions + Keyboard fallback), no pause button
- **Mobile:** Pause button visible, ESC disabled
- **Platform Switching:** Responds to OnPlatformChanged event
- **Uses:** GameManager.PauseGame/ResumeGame

---

## GameEvents
- **Purpose:** Decoupled communication
- **Pattern:** Publish-subscribe
- **Events:** OnPlayerDeath, OnEnemyKilled, OnXPGained, OnGoldCollected, etc.

---

## ServiceLocator
- **Purpose:** Dependency injection
- **Usage:** `ServiceLocator.Get<IAdService>()`
- **Error Handling:** Throws exception on duplicate registration (fail-fast)
- **Status:** Ready, no services registered yet

---

## Data Models

**CharacterData:**
- Stats: HP, moveSpeed, baseDamageMultiplier
- Starter weapon, unlock requirements

**WeaponData:**
- Type, damage, cooldown, knockback
- **No per-weapon upgrades** (universal only)

**EnemyData:**
- HP, speed, damage, resistance
- Loot: XP, gold, potion drop chance
