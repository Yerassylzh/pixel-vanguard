# GDD - Upgrade System Design

**Purpose:** Design specification for upgrade mechanics

---

## Design Goals

1. **Infinite Scaling:** Repeatable stats prevent upgrade pool exhaustion
2. **Strategic Depth:** Weapon-specific upgrades + prerequisites
3. **Build Variety:** 18 upgrade types create unique combinations
4. **Balanced RNG:** Weighted rarity prevents frustration

---

## Upgrade Categories

### Universal (Always Available)
- Move Speed, Max HP, Weapon Damage, Attack Speed
- Infinite repeatability
- Provides reliable power growth

### Weapon Unlocks (One-Time)
- 4 weapons, max 4 equipped
- Each unlock buffs entire arsenal
- Synergy potential

### Weapon-Specific (One-Time)
- 10 unique upgrades
- Requires weapon equipped (filtered otherwise)
- Creates specialization paths

### Passives (Limited to 3)
- Lifesteal, Magnet, Lucky Coins
- Scarcity creates meaningful choice
- Can't stack same passive

---

## Rarity Design

**Weight Distribution:**
| Rarity | Weight | Frequency | Purpose |
|--------|--------|-----------|---------|
| Common | 100 | ~53% | Repeatable stats (consistency) |
| Uncommon | 50 | ~26% | Most weapon upgrades |
| Rare | 25 | ~13% | Powerful weapon upgrades |
| Epic | 10 | ~5% | Game-changing upgrades |
| Passive | 30 | ~16% | Utility upgrades |

**Design Rationale:**
- Commons dominate = consistent power growth
- Epics are exciting but rare = memorable moments
- Passives slightly biased = encourages diversity

---

## Prerequisite System

**Current:** Triple Crossbow requires Dual first

**Goals:**
- Creates upgrade progression paths
- Prevents overpowered early-game combos
- Adds unlock satisfaction

**Future Expansion:**
- Multi-tier trees (Normal → Dual → Triple → Quad?)
- Cross-weapon prerequisites (Mirror Slash + Dual Crossbows → Special Combo?)

---

## Balance Philosophy

### Power Curve Target
- **Minute 1-5:** Survive, unlock first weapon
- **Minute 5-10:** Comfortable, 2-3 weapons, some upgrades
- **Minute 10-20:** Powerful, 3-4 weapons, synergies online
- **Minute 20+:** Godmode, screen-filling carnage

### Anti-Frustration Design
- Repeatable stats always available (never "dead" level-up)
- Weight system biases toward useful upgrades
- Passive limit prevents analysis paralysis

### Scaling Considerations
- Repeatable stats use multiplicative scaling (+20%, not +5 flat)
- HP scaling damage keeps upgrades relevant vs high-HP enemies
- Attack speed has diminishing returns (cooldown can't go below threshold)

---

## Upgrade Unlock Conditions

**Current System:**
- Weapon-specific: Check if weapon equipped
- Passive: Check count < 3
- NewWeapon: Check not already equipped
- Prerequisites: Check specific upgrade applied

**Design Space:**
- Time-gated upgrades (appear after X minutes)
- Kill-count gates (unlock after 100 kills)
- Combo unlocks (get A+B to unlock C)

---

## Visual Design

**Level-Up Panel:**
- 3 upgrade cards
- Icon + Name + Description
- Rarity color coding
- Hover/tap: Show detailed stats

**Upgrade Card Anatomy:**
- **Icon:** Visual representation
- **Name:** Short, punchy (e.g., "Mirror Slash")
- **Description:** Clear effect ("Spawn 2nd greatsword")
- **Stats:** Numerical impact ("+50% damage")

---

## Future Enhancements

### Planned
- Upgrade rerolls (spend gold to refresh options)
- Upgrade previews (see next 3 levels)
- Upgrade locking (bank one upgrade for later)

### Considered
- Negative upgrades (risk-reward)
- Upgrade fusion (combine 2 weak → 1 strong)
- Character-specific upgrades
- Curse system (powerful but costly)

---

## Technical Notes

**Implementation:** 4-class modular system
- Track er (state)
- Validator (filtering)
- Applicator (effects)
- Manager (orchestration)

**Benefits:**
-  Easy to add new upgrades (edit Applicator only)
- Testable in isolation
- Clear separation of concerns
