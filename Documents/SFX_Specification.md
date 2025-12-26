# SFX Specification - Pixel Vanguard

**Style:** Retro 8-bit/16-bit arcade style (similar to Vampire Survivors)  
**No Human Voices:** Pure sound effects and music only

---

## Combat SFX

### Player Weapons

**Greatsword Swing**
- **Type:** Medium-pitched "whoosh" with metallic ring
- **Variation:** 3-4 variations to avoid repetition
- **Volume:** Medium-loud
- **Pitch:** Slightly randomized (±5%)
- **Example:** Sword whoosh + light "shink" sound
- **Trigger:** On each swing animation start

**Auto Crossbow Fire**
- **Type:** Sharp "twang" + arrow whistle
- **Variation:** 2-3 variations
- **Volume:** Medium
- **Pitch:** Randomized (±8%)
- **Example:** Bowstring snap + projectile whoosh
- **Trigger:** On arrow spawn

**Holy Water Throw**
- **Type:** Glass bottle whoosh + splash
- **Variation:** 2 variations
- **Volume:** Medium
- **Pitch:** Randomized (±5%)
- **Example:** Bottle whistle → glass shatter → liquid splash
- **Trigger:** On flask spawn

**Holy Water DoT Tick**
- **Type:** Sizzle/burning sound
- **Variation:** Subtle continuous sizzle
- **Volume:** Low-medium (not overwhelming)
- **Pitch:** Constant
- **Example:** Acid/fire burn effect
- **Trigger:** On each damage tick in puddle

**Magic Orbitals Hit**
- **Type:** Magic "zap" or energy pulse
- **Variation:** 3-4 variations
- **Volume:** Medium
- **Pitch:** Higher pitch, randomized (±10%)
- **Example:** Electric spark + ethereal chime
- **Trigger:** On enemy collision

---

### Enemy Damage

**Enemy Hit (Light)**
- **Type:** Soft impact thud
- **Variation:** 4-5 variations
- **Volume:** Medium-low
- **Pitch:** Randomized (±10%)
- **Example:** Fleshy impact
- **Trigger:** When enemy takes <30% max HP damage

**Enemy Hit (Heavy)**
- **Type:** Heavier impact with crunch
- **Variation:** 3-4 variations
- **Volume:** Medium
- **Pitch:** Lower, randomized (±8%)
- **Example:** Bone crack + impact
- **Trigger:** When enemy takes ≥30% max HP damage

**Enemy Death**
- **Type:** Defeat sound (pop/poof/dissolve)
- **Variation:** 5-6 variations per enemy type
- **Volume:** Medium
- **Pitch:** Randomized per enemy (±12%)
- **Example:** 
  - Skeleton: Bone rattle collapse
  - Ghost: Ethereal dissipate
  - Slime: Wet splat
**Trigger:** On enemy death

**Boss Death**
- **Type:** Dramatic explosion/collapse
- **Variation:** 1-2 variations
- **Volume:** Loud
- **Pitch:** Deep, no randomization
- **Example:** Electronic explosion + rumble
- **Trigger:** On boss defeat

---

### Player Damage

**Player Hit**
- **Type:** Damage grunt (electronic, not human)
- **Variation:** 3-4 variations
- **Volume:** Medium-loud
- **Pitch:** Randomized (±7%)
- **Example:** Synth damage tone (downward pitch sweep)
- **Trigger:** On player TakeDamage()

**Player Death**
- **Type:** Defeat jingle (sad/dramatic)
- **Variation:** 1 version
- **Volume:** Loud
- **Pitch:** Constant
- **Example:** Descending arpeggio + explosion
- **Trigger:** On player death

---

## Progression SFX

### Collectibles

**XP Gem Pickup**
- **Type:** Pleasant chime/ding
- **Variation:** 3 variations (small/medium/large gems)
- **Volume:** Medium
- **Pitch:** Higher for larger gems
- **Example:** Crystal chime, higher pitch = more XP
- **Trigger:** On XP collection

**Gold Coin Pickup**
- **Type:** Coin clink/jingle
- **Variation:** 2-3 variations
- **Volume:** Medium
- **Pitch:** Metallic, randomized (±5%)
- **Example:** Retro coin sound
- **Trigger:** On gold collection

**Health Potion Pickup**
- **Type:** Gulp + sparkle
- **Variation:** 1-2 variations
- **Volume:** Medium
- **Pitch:** Upward sweep
- **Example:** Magic restoration tone
- **Trigger:** On health pickup

**Magnet Pull**
- **Type:** Subtle attraction hum
- **Variation:** 1 looping sound
- **Volume:** Low
- **Pitch:** Constant hum with slight wobble
- **Example:** Electromagnetic pull
- **Trigger:** While collectibles are being pulled

---

### Level Up System

**Level Up**
- **Type:** Triumphant fanfare
- **Variation:** 1-2 variations
- **Volume:** Loud
- **Pitch:** Bright, ascending
- **Example:** Victory jingle (2-3 notes)
- **Trigger:** On level up

**Upgrade Select (Hover)**
- **Type:** Soft UI beep
- **Variation:** 1 version
- **Volume:** Low
- **Pitch:** Higher pitch
- **Example:** Menu cursor move
- **Trigger:** On upgrade card hover

**Upgrade Confirm**
- **Type:** Power-up sound
- **Variation:** 2-3 variations based on rarity
- **Volume:** Medium-loud
- **Pitch:** 
  - Common: Standard
  - Rare: Slightly higher
  - Epic: Highest + extra reverb
- **Example:** Power-up arpeggio
- **Trigger:** On upgrade selection

**New Weapon Unlock**
- **Type:** Special unlock jingle
- **Variation:** 1 version
- **Volume:** Loud
- **Pitch:** Triumphant
- **Example:** Item get fanfare (Zelda-style)
- **Trigger:** On NewWeapon upgrade

---

## UI SFX

**Button Click**
- **Type:** Click/tap sound
- **Variation:** 1-2 variations
- **Volume:** Medium
- **Pitch:** Sharp
- **Example:** Menu select
- **Trigger:** On any button press

**Button Hover**
- **Type:** Soft tick
- **Variation:** 1 version
- **Volume:** Low
- **Pitch:** High
- **Example:** Menu hover
- **Trigger:** On button hover

**Panel Open**
- **Type:** Whoosh/slide in
- **Variation:** 1-2 variations
- **Volume:** Medium
- **Pitch:** Upward sweep
- **Example:** UI panel appear
- **Trigger:** On pause/level-up panel open

**Panel Close**
- **Type:** Whoosh/slide out
- **Variation:** 1 version
- **Volume:** Medium
- **Pitch:** Downward sweep
- **Example:** UI panel dismiss
- **Trigger:** On panel close

**Error/Invalid Action**
- **Type:** Buzzer/negative beep
- **Variation:** 1 version
- **Volume:** Medium
- **Pitch:** Low, dissonant
- **Example:** Denied action
- **Trigger:** Invalid input

---

## Ambient/Atmospheric

**Wave Spawn Warning**
- **Type:** Rising tension sound
- **Variation:** 1 version
- **Volume:** Medium
- **Pitch:** Rising sweep
- **Example:** Danger approach alarm
- **Trigger:** Before large enemy wave

**Timer Warning (1 minute left)**
- **Type:** Urgent beeping
- **Variation:** 1 looping version
- **Volume:** Medium-loud
- **Pitch:** Pulsing
- **Example:** Warning siren
- **Trigger:** At specific time milestones

---

## Music

### Background Music

**Main Menu Theme**
- **Style:** Mysterious, inviting
- **Tempo:** Moderate (100-120 BPM)
- **Mood:** Heroic but calm
- **Loop:** Seamless 1-2 minute loop
- **Instruments:** Chiptune synths, light drums

**Gameplay Theme (Early Game)**
- **Style:** Upbeat, adventurous
- **Tempo:** Fast (140-160 BPM)
- **Mood:** Energetic, hopeful
- **Loop:** Seamless 2-3 minute loop
- **Instruments:** Driving bass, melodic leads, percussion
- **Layering:** Add intensity over time

**Gameplay Theme (Late Game)**
- **Style:** Intense, epic
- **Tempo:** Very fast (160-180 BPM)
- **Mood:** Chaotic, powerful
- **Loop:** Seamless 2-3 minute loop
- **Instruments:** Heavy bass, multiple synth layers, intense drums
- **Trigger:** After 10+ minutes or when player is overpowered

**Boss Music** (Future)
- **Style:** Dramatic, threatening
- **Tempo:** Variable (starts slow, builds to fast)
- **Mood:** Tense, epic
- **Loop:** Non-looping intro → looping battle phase

**Game Over Music**
- **Style:** Sad, reflective
- **Tempo:** Slow (60-80 BPM)
- **Mood:** Defeat but not depressing
- **Loop:** Plays once, ~10-15 seconds

**Victory Music**
- **Style:** Triumphant, celebratory
- **Tempo:** Moderate-fast (120-140 BPM)
- **Mood:** Victorious
- **Loop:** Plays once, ~15-20 seconds

---

## Audio Mixing Guidelines

**Volume Hierarchy:**
1. Music: Base layer (60-70% volume)
2. Important SFX (player damage, level up): 90-100%
3. Combat SFX (weapons, enemy hits): 70-80%
4. Collectible SFX: 60-70%
5. UI SFX: 50-60%

**Ducking:**
- Music should duck (reduce by 20%) during:
  - Level up screen
  - Important upgrades
  - Player death

**Spatial Audio:**
- Not required (2D game)
- All sounds play at same stereo position

**Polyphony Limits:**
- Max 32 simultaneous sounds
- Priority: Player damage > Weapons > Enemy hits > Collectibles > UI

---

## Asset Sources (Recommendations)

**Free Resources:**
- **Freesound.org** - Large library, requires attribution
- **OpenGameArt.org** - CC0 and CC-BY assets
- **SFXR/Bfxr** - Generate custom retro SFX
- **ChipTone** - Chiptune SFX generator

**Paid Resources:**
- **Humble Bundle** (Game Audio bundles)
- **itch.io** - Indie SFX packs
- **Sonniss Game Audio Bundle** (annual free releases)

**Music:**
- **Kevin MacLeod** (incompetech.com) - Free with attribution
- **Purple Planet** - Free background music
- **Bosca Ceoil** - Create custom chiptunes

---

## Implementation Notes

**AudioManager Singleton:**
- Centralized SFX playback
- Volume control per category
- Pooling for repeated sounds (enemy hits, weapon fire)

**Randomization:**
- Pitch variation prevents repetition fatigue
- Volume slight variation (±5%) for realism

**Performance:**
- Pre-load combat SFX (no streaming)
- Stream music tracks
- Limit max instances per sound (e.g., max 10 sword swings playing simultaneously)
