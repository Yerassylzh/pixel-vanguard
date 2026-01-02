# Pixel Vanguard - System Architecture

**Purpose:** Master technical reference index  
**Last Updated:** 2026-01-02  
**Status:** Production-ready with localization, Yandex integration, and critical bug fixes  
**Code Quality:** A (92/100) - Professional-grade architecture

> 💡 **Quick Start:** This is the index. Start with [Player System](Systems/Player-System.md) → [Weapon System](Systems/Weapon-System.md) → [Upgrade System](Systems/Upgrade-System.md)

---

## 🎮 Game Overview

**Genre:** Horde Survivor / Action Roguelite (Vampire Survivors-like)  
**Platform:** Unity 2D (Desktop + Mobile)  
**Architecture:** Component-based + Service Locator + Event Bus + ScriptableObject Data

### Core Loop
```
Spawn → Fight → Collect XP → Level Up → Choose Upgrade → Repeat → Die → Stats → Restart
```

### Technology Stack
- **Engine:** Unity 2022+ (2D URP)
- **Input:** New Input System + Virtual Joystick (mobile)
- **Camera:** Cinemachine Virtual Camera
- **Pattern:** Service Locator for cross-cutting concerns

---

## 📊 10 Core Systems

> Each system has its own detailed documentation page. Click to read more.

1. **[Player System](Systems/Player-System.md)** - Movement, health, input, animations (4 components)
2. **[Character System](Systems/Character-System.md)** - Selection, stat loading, spawning
3. **[Weapon System](Systems/Weapon-System.md)** - 4 types, auto-fire, upgrade system
4. **[Enemy System](Systems/Enemy-System.md)** - AI, wave spawning, loot drops
5. **[Progression System](Systems/Progression-System.md)** - XP/Gold collection, leveling
6. **[Upgrade System](Systems/Upgrade-System.md)** - 18 types with rarity weighting
7. **[UI System](Systems/UI-System.md)** - HUD, menus, platform-aware joystick
8. **[Service Architecture](Systems/Service-Architecture.md)** - Save/load, ads, IAP, platform detection
9. **[Camera System](Systems/Camera-System.md)** - Cinemachine player follow
10. **[Localization System](Systems/Localization-System.md)** - Multi-language (EN/RU), platform-aware

---

## 🐛 Critical Bug Fixes

> **Recent:** Two critical fixes implemented (Jan 2026)

- **[PluginYG Focus Pause Fix](Bug-Fixes/PluginYG-Focus-Pause.md)** - Revive freeze resolution
-  **[VirtualJoystick Coordinate Fix](Bug-Fixes/VirtualJoystick-Coordinates.md)** - Touch positioning accuracy

---

## 🎯 Design Patterns

### Singletons
- **GameManager** - Game state machine
- **PlayerController** - Player reference
- **CharacterManager** - Character spawning
- **LocalizationManager** - Translation management

### ScriptableObjects
- All configuration data (Characters, Weapons, Enemies, Upgrades, Translations)
- Enables designer-friendly tuning without code changes

### Events
- `GameEvents` static class for decoupled communication
- UI updates via events (no direct references)

### Code Style
- **Null-safe:** Use `?.` operator
- **Early returns:** Avoid deep nesting
- **Protected fields:** Weapon stats (inheritance-friendly)
- **XML docs:** On public APIs

---

## 📁 Quick File Reference

**Core Systems:**  [`Core/`](file:///C:/Users/Honor/Unity%20Games/Pixel%20Vanguard/Assets/Scripts/Core)
- GameBootstrap, ServiceLocator, LocalizationManager, PlatformDetector, GameEvents

**Data (ScriptableObjects):** [`Data/`](file:///C:/Users/Honor/Unity%20Games/Pixel%20Vanguard/Assets/Scripts/Data)
- CharacterData, WeaponData, UpgradeData, EnemyData, TranslationData

**Gameplay:** [`Gameplay/`](file:///C:/Users/Honor/Unity%20Games/Pixel%20Vanguard/Assets/Scripts/Gameplay)
- Player/, Weapons/, Enemies/, Upgrades/, GameManager, XPGem, GoldCoin, HealthPotion

**UI:** [`UI/`](file:///C:/Users/Honor/Unity%20Games/Pixel%20Vanguard/Assets/Scripts/UI)
- HUD, LevelUpPanel, GameOverScreen, VirtualJoystick, Shop/

**Services:** [`Services/`](file:///C:/Users/Honor/Unity%20Games/Pixel%20Vanguard/Assets/Scripts/Services)
- Interfaces/, Yandex/, Placeholder/, PlatformServiceFactory

---

## 📖 Additional Documentation

| Topic | Document |
|-------|----------|
| Recent Changes | [CHANGELOG.md](CHANGELOG.md) |
| Feature Specs | [Features Specification.md](Features%20Specification.md) |
| Data Models | [GDD/GDD - Data Models.md](GDD/GDD%20-%20Data%20Models.md) |
| Implementation Status | [Progress/Implementation Status.md](Progress/Implementation%20Status.md) |

---

**Last Updated:** 2026-01-02  
**Maintainer:** Development Team  
**Code Review:** [See comprehensive analysis](file:///C:/Users/Honor/.gemini/antigravity/brain/1262dfee-5e5a-46c7-8875-7b0077e9411a/comprehensive_code_review.md)
