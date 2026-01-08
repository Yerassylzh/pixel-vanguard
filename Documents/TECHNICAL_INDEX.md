# Technical Index

**Code Quality Goal:** Clean, Decoupled, Event-Driven.

---

## 🗺️ System Map

### 1. [Mechanics (Design Rules)](Mechanics/)
*   [Combat Formulas](Mechanics/Combat_Formulas.md) (Damage, Armor, Crit)
*   [Economy](Mechanics/Economy.md) (Gold, Prices)
*   [Progression](Mechanics/Progression.md) (XP Curve, Unlocks)
*   [Wave Logic](Mechanics/Wave_Logic.md) (Spawn rates)
*   [Audio Design](Mechanics/Audio_Design.md) (SFX Guidelines/Triggers)

### 2. [Systems (Implementation)](Systems/)
#### 🏗️ [Architecture](Systems/Architecture/)
*   **[Game Loop](Systems/Architecture/Game_Loop.md):** `GameManager`, States, Bootstrapping.
*   **[Input System](Systems/Architecture/Input_System.md):** Cross-platform abstraction.
*   **Service Locator:** `ServiceLocator.cs` (Global access)
*   **Event Bus:** `GameEvents.cs` (Decoupled communication)

#### ⚔️ [Gameplay](Systems/Gameplay/)
*   **[Player Controller](Systems/Gameplay/Player_Controller.md):** Movement & State
*   **[Weapon System](Systems/Gameplay/Weapon_System.md):** Inheritance hierarchy (`WeaponBase`)
*   **[Enemy Spawner](Systems/Gameplay/Enemy_Spawner.md):** *Refactored Jan 2026*
*   **[Audio System](Systems/Gameplay/Audio_System.md):** *Refactored Jan 2026*

#### ☁️ [Services](Systems/Services/)
*   **Save System:** `ISaveService` (JSON/Cloud)
*   **IAP/Ads:** `YandexIAPService`, `YandexAdService`

#### 🖥️ [UI & UX](Systems/UI/)
*   **Animation:** DOTween-based transitions
*   **Localization:** `LocalizationManager` (EN/RU)

---

## 🛠️ Code Standards
1.  **No Singletons for Logic:** Use `ServiceLocator` for systems.
2.  **Event-Driven:** `PlayerHealth` fires `OnDeath`; `GameManager` listens. Do not couple them directly.
3.  **Data-Driven:** All stats (Damage, Speed) live in `ScriptableObjects` (`Data/`), not Monobehaviours.
