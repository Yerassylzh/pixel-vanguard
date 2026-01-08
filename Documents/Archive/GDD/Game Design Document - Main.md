# Game Design Document - Main

**Game Title:** Pixel Vanguard  
**Genre:** Horde Survivor / Action Rogue lite (Vampire Survivors-style)  
**Platform:** Unity 2D (Desktop + Mobile)  
**Target:** Casual action players

---

## Core Concept

**Elevator Pitch:** Survive waves of pixel-art enemies by unlocking weapons and stacking upgrades in this auto-attack roguelite.

**Core Loop:**
```
Spawn → Kill Enemies → Collect XP → Level Up → Choose Upgrade → Get Stronger → Repeat → Eventually Die → See Stats
```

**Session Duration:** 10-30 minutes per run

---

## Key Features

### 1. Auto-Combat System
- Weapons fire automatically (no aim required)
- Player focuses on positioning and dodging
- Mobile-friendly (virtual joystick)

### 2. Weapon Variety (4 Types)
- **Greatsword:** Melee ~180° swing with visual slash
- **Auto Crossbow:** Ranged multi-target arrows
- **Holy Water:** Area denial puddles with DoT
- **Magic Orbitals:** Orbiting shield balls

### 3. Deep Upgrade System (18 Types)
- **Universal:** Speed, HP, Damage, Attack Speed (infinite stacking)
- **New Weapons:** Unlock up to 4 weapons
- **Weapon-Specific:** 10 unique upgrades (Mirror Slash, Dual Shot, etc.)
- **Passives:** Lifesteal, Magnet, Lucky Coins (max 3)

### 4. Progression Systems
- XP leveling (level × 10 XP required)
- Gold collection (future shop)
- Weighted rarity upgrades (Common → Epic)

---

## Design Pillars

### 1. **Incremental Power Fantasy**
- Start weak, become godlike
- Repeatable stats enable infinite scaling
- Synergistic upgrades (Dual + Triple + Pierce crossbow)

### 2. **Meaningful Choices**
- Weighted randomness ensures variety
- Prerequisites create upgrade paths
- Passive limits force trade-offs

### 3. **Visual Satisfaction**
- Shader-based weapon reveals
- Knockback on every hit
- Screen-filling late-game power

### 4. **Accessibility**
- One-finger mobile control
- Auto-targeting weapons
- Clear visual feedback

---

## Monetization (Future)

**Free-to-Play Model:**
- Core game free
- Optional ads for continue/double rewards
- No pay-to-win mechanics

---

## Target Audience

**Primary:** Casual mobile gamers (ages 18-35)  
**Secondary:** PC roguelite fans

**Appeal:**
- Easy to learn, hard to master
- Short sessions fit mobile lifestyle
- Satisfying power progression
- Retro pixel art aesthetic

---

## Unique Selling Points

1. **4-Weapon Limit** - Forces strategic choices
2. **Prerequisite System** - Adds progression depth
3. **HP Scaling Damage** - Makes upgrades relevant vs bosses
4. **Multi-Target Arrows** - Rewards positioning

---

## Technical Highlights

- Component-based architecture (easy to extend)
- ScriptableObject-driven balance (designer-friendly)
- Modular upgrade system (4-class separation)
- Event-driven UI (decoupled)

---

## Future Expansion

**Phase 2:**
- Shop system (spend gold)
- More characters (3-5 total)
- Boss enemies
- Meta progression (permanent upgrades)

**Phase 3:**
- Multiple maps
- Daily challenges
- Leaderboards
- Achievements

**Phase 4:**
- Multiplayer co-op
- More weapons (target 8-10)
- Prestige system
