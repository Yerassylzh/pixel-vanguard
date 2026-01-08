# Core Mechanics

**Genre:** Horde Survivor / Reverse Bullet Hell
**Core Loop:** `Spawn` → `Fight` → `XP` → `Level Up` → `Upgrade` → `Repeat`

## 1. Combat System
*   **Auto-Fire:** Weapons attack automatically based on their Cooldown/Range.
*   **Manual Movement:** Player controls positioning to dodge and kite enemies.
*   **Knockback:** All weapons apply physics force; enemies have mass.

## 2. Weapon Types
| Weapon | Type | Behavior |
| :--- | :--- | :--- |
| **Greatsword** | Melee | ~180° swing, high knockback, low range. |
| **Crossbow** | Ranged | Fires at nearest enemy, heavy single-target damage. |
| **Holy Water** | Area | Throws projectiles that create damaging puddles. |
| **Orbitals** | Defensive | Projectiles rotate around player, shielding from contact. |

## 3. Progression
*   **Session:** XP Gems drop from enemies. Level up grants 1 of 3 random Upgrades.
*   **Meta:** Gold drops from enemies/chests. Used in Shop for permanent stats.
*   **Scaling:** Enemy HP/Damage scales with `GameTime` (Minutes).

## 4. Design Pillars
1.  **Incremental Power:** Start weak, end screen-clearingly strong.
2.  **Meaningful Choice:** Weighted RNG ensures variety but rewards build planning.
3.  **Visual Clarity:** Damage numbers and hit flashes must not obscure the player.
