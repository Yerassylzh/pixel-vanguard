# Features Specification

## 1. Gameplay Loop & Session Logic

> [!NOTE]
> **Current Implementation Status**
> - **XP Gems:** Fully implemented (cyan squares). All values work correctly.
> - **Gold Coins:** Event fires (`TriggerGoldCollected`) but NO visual coin spawns yet.
> - **Health Potions:** Not implemented (TODO).

1. **Start:** Player enters map with **1 Starter Weapon** (default: Greatsword from CharacterData).
2. **Combat:** Player kites enemies. Weapons auto-fire independently.
3. **Loot:**
   - **XP Gems (Blue):** Fill level-up bar
   - **Gold Coins (Yellow):** Add to end-game reward
   - **Health Potions (Red):** Restore HP
4. **Level Up Event:**
   - Game pauses when XP bar fills
   - **3 Random Upgrades** shown:
     - **New Weapon:** Acquire additional weapon (max 4 total)
     - **Attack Speed:** Reduce ALL weapon cooldowns (universal)
     - **Weapon Damage:** Increase ALL weapon damage (universal)
     - **Player Stats:** Max HP, Move Speed, Knockback
5. **End Game:**
   - "Run Complete" or "You Died"
   - Summary shows total Gold earned
   - Option: "Watch Ad to Double Gold"

---

## 2. Character & Enemy Roster

### The Heroes

Unlocked via **Gold**.

1. **The Knight:** Balanced stats. Starts with Greatsword.
2. **The Pyromancer:** High Area Damage, Low HP. Starts with Holy Water.
3. **The Ranger:** Fast movement. Starts with Auto-Crossbow.

### The Enemies

> [!WARNING]
> **Current:** Only 1 placeholder enemy (red square). System ready for multiple types via EnemyData ScriptableObjects.

1. **Skeleton Grunt:** Slow, weak, numerous
2. **The Crawler:** Fast chaser (humanoid crawl animation)
3. **Armored Orc:** High HP, knockback resistant
4. **The Abomination (Boss):** Huge sprite, telegraphed charge attacks

---

## 3. Weaponry

**Max Weapons:** 4 simultaneous (start with 1)  
**Auto-Fire:** All weapons fire automatically based on `cooldown` stat  
**Universal Upgrades:** All weapons share upgrade bonuses

### Greatsword - Grand Cleave
- **Behavior:** Horizontal slash (Left/Right based on movement direction)
- **VFX:** SpriteReveal shader with opacity fade curve
- **Mechanics:** 
  - No physical rotation; visual-only effect
  - Multi-hit tracking (HashSet prevents duplicate hits per swing)
  - Direction-aware aiming
- **Stats:** High knockback, 2.5s base cooldown
- **Upgrades:** Faster attacks, more damage

### AutoCrossbow - Firework Bolt
- **Behavior:** Fires arrows at nearest enemy (15m range)
- **Projectile:** Spinning arrow sprite with pierce capability
- **Mechanics:**
  - Smart targeting (finds nearest alive enemy)
  - Multi-shot support (spread pattern)
  - Pierce count (arrow survives X hits)
- **Stats:** Medium damage, 1.0s base cooldown
- **Upgrades:** Multi-shot (1→2→3 arrows), pierce through enemies

### HolyWater - Sanctified Ground
- **Behavior:** Spawns blue fire damage zone at random offset from player
- **VFX:** RadialReveal shader (center-outward expansion)
- **Animation Phases:**
  1. Rune expands (`fadeInDuration` 0.5s)
  2. Fire particles ignite
  3. DoT damage every `tickRate` (0.5s)
  4. Fire stops, rune shrinks (`fadeOutDuration` 0.5s)
  5. Destroy
- **Mechanics:**
  - Tracks enemies in zone (HashSet)
  - Continuous damage to all enemies inside
  - No projectile/flask (instant spawn)
- **Stats:** DoT damage, 3s duration, 3s cooldown
- **Upgrades:** Longer duration, more damage, multiple zones

### Magic Orbitals - Ethereal Shields
- **Behavior:** 3 balls orbit player with animated radius
- **Animation:**
  - Radius expands 0→targetRadius (`fadeInDuration` 0.5s)
  - Balls orbit for `baseDuration` (5s)
  - Radius shrinks targetRadius→0 (`fadeOutDuration` 0.5s)
  - Destroy and respawn cycle
- **Mechanics:**
  - Per-enemy damage cooldown (Dictionary tracking)
  - Damages ALL enemies touched simultaneously
  - Ball size constant (only radius animates)
  - Visibility delays prevent overlap at spawn/despawn
- **Stats:** Per-ball damage, 0.5s damage interval per enemy
- **Upgrades:** More balls (3→4→5), longer orbit duration

---

## 4. Main Menu & Metagame (The Shop)

### Tab A: "The Armory" (Stats)
Permanent upgrades to base stats:
1. **Vitality:** Increases Max HP
2. **Might:** Increases Base Damage %
3. **Greaves:** Increases Movement Speed
4. **Magnet:** Increases XP collection radius
5. **Luck:** Increases Critical Hit chance

### Tab B: "The Barracks" (Characters)
- Locked character cards
- "Unlock for 10,000 Gold"

### Tab C: "The Treasury" (Gold Store)
- **IAP:** Buy 50,000 Gold ($4.99)
- **Ad:** Watch video → Get 500 Gold

---

## 5. Monetization Points (Ads)

1. **Treasury Ad:** Proactive gold grinding
2. **Multiplier Ad:** Game Over screen only ("Double Your Gold")
3. **"Desperation" Reroll:** Level-up upgrade reroll for build optimization