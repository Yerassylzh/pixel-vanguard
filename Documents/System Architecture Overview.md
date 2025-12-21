# System Architecture Overview
**Pixel Vanguard**

> **AI Context Hook:** Read this file first to understand how the entire game is wired. It replaces the old "Current Systems.md".

---

## 1. Project Structure (High Level)

- **Engine:** Unity 2022+ (2D URP)
- **Input:** New Input System (Actions Asset: `InputSystem_Actions`)
- **Pattern:** Component-based + Service Locator + Event Bus
- **Paradigm:** "Managers" control logic, "Controllers" handle view/input.

```
Assets/Scripts/
├── Core/           # System foundation (GameEvents, ServiceLocator, Platform)
├── Gameplay/       # Game logic (Player, Weapons, Enemies, Managers)
├── UI/             # UI Controllers (HUD, Menus, Joystick)
├── Data/           # ScriptableObjects (Item definitions, Stats)
├── Services/       # Backend interfaces (Save, Ads)
└── Utils/          # Helpers (FPS Setter)
```

---

## 2. Core Gameplay Loop

**Scene:** `GameScene` (No Bootstrap scene yet, starts directly).

1.  **Initialization:**
    *   `GameManager.Start()` initializes state.
    *   `CharacterManager` spawns the Player Prefab at `(0,0)`.
    *   `CameraController` (Cinemachine) locks onto Player.
2.  **Active Play (`GameState.Playing`):**
    *   **Player:** Moves via `PlayerController`.
    *   **Weapons:** `WeaponManager` iterates active weapons. Each `WeaponBase` handles its own auto-fire cooldowns.
    *   **Enemies:** `EnemySpawner` spawns prefabs at screen edges. `EnemyAI` moves them linearly toward player.
    *   **Loot:** Enemies drop `XPGem`. Player "magnet" pulls them in.
3.  **Progression (Level Up):**
    *   XP fills bar → `LevelUpManager` pauses game (`timeScale = 0`).
    *   **UI:** Display 3 random cards from `UpgradeManager`.
    *   **Action:** Player picks card → Upgrade applied → Game resumes.
4.  **End Game:**
    *   Player HP <= 0 → `GameManager` triggers `GameOver`.
    *   `GameOverScreen` shows stats. Restart reloads scene.

---

## 3. Vital Systems (The "Brain")

### A. GameManager
*   **Role:** The Boss.
*   **State Machine:** `Initializing` → `Playing` → `Paused` / `LevelUp` → `GameOver`.
*   **Time:** Tracks specialized `GameTime` (pauses when game pauses).

### B. ServiceLocator
*   **Role:** Service Registry.
*   **Usage:** `ServiceLocator.Get<IAdService>()`.
*   **Behavior:** Fail-fast. Throws exception if duplicate services registered.

### C. GameEvents (The "Nervous System")
*   **Role:** Decoupled Event Bus.
*   **Key Events:**
    *   `OnPlayerDeath`
    *   `OnEnemyKilled` (sends xp/gold amount)
    *   `OnXPGained`
    *   `OnLevelUp`
    *   `OnPlatformChanged` (Mobile vs Desktop)

### D. CharacterManager (New)
*   **Role:** Spawning & Setup.
*   **Job:** Instantiates selected character (ScriptableObject data) and assigns Cinemachine Target Group.

---

## 4. Weapon System

**Architecture:**
*   `WeaponBase` (Abstract): Handles Cooldowns, Damage Modifiers, and `Fire()` text.
*   `WeaponManager`: Holds List<WeaponBase>. Adds new ones via `AddWeapon()`.

**Current Weapons:**
1.  **Greatsword:** (VFX only) Toggles a "Shockwave" sprite on/off. Uses `AnimationCurve` for opacity.
2.  **Auto Crossbow:** (Projectile) Instantiates `ArrowProjectile` aimed at nearest neighbor.
3.  **Holy Water:** (Area) Throws flask → `DamagePuddle` (stay trigger).
4.  **Magic Orbitals:** (Physics) Rotating shield sprites with collision damage.

---

## 5. Input & Platform

**PlatformDetector:**
*   **Auto-Detects:** Mobile vs Desktop.
*   **Behavior:**
    *   **Mobile:** Enables `VirtualJoystick` Canvas. Shows UI Pause Button.
    *   **Desktop:** Hides Joystick. Enables WASD/Arrow keys. Shows "ESC" hint.

**Virtual Joystick:**
*   **Logic:** Floating anchor.
*   **Fix:** Uses **Parent-Space** coordinates to ensure it stays under finger precisely.

---

## 6. How to Add New Content (Cheat Sheet)

### Adding a New Weapon
1.  Create Script: `MyWeapon.cs` inheriting `WeaponBase`.
2.  Implement `Fire()`: Spawn projectile or enable Hitbox.
3.  Create ScriptableObject: `WeaponData` (Stats).
4.  Create Unlock Upgrade: `UpgradeData` (Type: NewWeapon).

### Adding a New Enemy
1.  Create ScriptableObject: `EnemyData` (HP, Speed, Prefab).
2.  Create Prefab: Sprite + `EnemyHealth` + `EnemyAI` + `BoxCollider2D`.
3.  Register: Add `EnemyData` to `EnemySpawner`'s list.

### Adding a New Character
1.  Create ScriptableObject: `CharacterData` (Stats, Starter Weapon).
2.  Add to `CharacterManager` list (when UI is ready).

---

## 7. Known Technical Limitations
*   **No Object Pooling (Yet):** Spawning/Destroying enemies causes GC alloc. Priority for future optimization.
*   **No Main Menu:** Scene starts directly into gameplay.
*   **Y-Axis Sorting:** Relies on Unity's "Transparency Sort Mode = Custom Axis (0,1,0)".

---
*Created: Dec 19, 2025*
