## 1. Gameplay Loop & Session Logic

1. **Start:** Player enters map with **1 Starter Weapon** (Default: Greatsword).
    
2. **Combat:** Player kites enemies. Weapons auto-fire.
    
3. **Loot:**
    
    - **XP Gems (Blue):** Fill the Level Up bar.
        
    - **Gold Coins (Yellow):** Added to end-game reward.
        
    - **Health Potions (Red):** Restore HP.
        
4. **The Level Up Event:**
    
    - When the XP bar fills, the game pauses.
        
    - The player is presented with **3 Cards**.
        
    - **Card Types:**
        
        - **New Weapon:** (e.g., "Get Auto-Crossbow").
            
        - **Upgrade Weapon:** (e.g., "Greatsword Lvl 2 
            
            ```
            →→
            ```
            
             Lvl 3").
            
        - **Stat Boost:** (e.g., "Increase Might by 10%").
            
5. **End Game:**
    
    - "Run Complete" or "You Died."
        
    - Summary screen shows total Gold earned.
        
    - Option: "Watch Ad to Double Gold."
        

## 2. Character & Enemy Roster

### The Heroes (The "Barracks")

Unlocked via **Gold**.

1. **The Knight:** Balanced stats. Starts with Greatsword.
    
2. **The Pyromancer:** High Area Damage, Low HP. Starts with Molotov.
    
3. **The Ranger:** Fast movement. Starts with Auto-Crossbow.
    

### The Enemies (The "Horde")

1. **Skeleton Grunt:** Slow, weak, numerous.
    
2. **The Crawler (Fast Chaser):** Technical Note: Uses a humanoid "Zombie Crawl" animation to fit the Mixamo workflow. Moves very fast on all fours.
    
3. **Armored Orc:** High HP. Hard to knock back.
    
4. **The Abomination (Boss):** Huge sprite. Telegraphed charge attacks.
    

## 3. Weaponry

Players start with 1, but can hold up to 4 weapons simultaneously.

1. **Greatsword:**
    
    - Periodic 360°swing around the player every 2.5s. High knockback.
        
    - Upgrade: Attacks faster (lower cooldown) and deals more damage.
        
2. **AutoCrossbow:**
    
    - Fires arrows at the nearest enemy automatically.
        
    - Upgrade: Fires more arrows (Double shot → Triple shot) and pierces through enemies.
        
3. **HolyWater:**
    
    - Throws a flask that creates a damaging puddle on the floor.
        
    - Upgrade: Puddle lasts longer and grows larger.
        
4. **MagicOrbitals:**
    
    - Shields that continuously rotate around the player to block enemies.
        
    - Upgrade: Adds more shields (1 → 2 → 3).
        

## 4. Main Menu & Metagame (The Shop)

The Shop is split into two distinct tabs to organize content.

### Tab A: "The Armory" (Stats)

Permanent upgrades to the character's base stats.

1. **Vitality:** Increases Max HP.
    
2. **Might:** Increases Base Damage % (Essential for late game).
    
3. **Greaves:** Increases Movement Speed (Essential for dodging bosses).
    
4. **Magnet:** Increases the radius at which you attract XP Gems.
    
    - Why this matters: If you don't have a Magnet, you have to run into the swarm to get XP, which is dangerous. High Magnet lets you collect XP from a safe distance.
        
5. **Luck:** Increases Critical Hit chance.
    

### Tab B: "The Barracks" (Characters)

- Cards showing the locked characters.
    
- Button: "Unlock for 10,000 Gold".
    

### Tab C: "The Treasury" (Gold Store)

- **IAP:** Buy 50,000 Gold ($4.99).
    
- **Ad:** Watch Video 
    
    ```
    →→
    ```
    
     Get 500 Gold.
    

## 5. Monetization Points (Ads)

1. **The Treasury Ad:** Player proactively goes to the shop and watches ads to grind Gold for a new character.
    
2. **The Multiplier Ad:** Appears **only** at the Game Over screen. "Watch Ad to Double your Run Gold." (Very popular with players).
    
3. **The "Desperation" Reroll:** Appears during Level Up. If the player gets 3 items they don't want, they can watch an ad to shuffle the deck. Used by players trying to build specific "God Builds."