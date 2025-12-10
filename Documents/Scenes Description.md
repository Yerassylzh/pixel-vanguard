## 1. SCENE: Bootstrap (The Invisible Scene)

**Purpose:** This is the very first scene the game loads. It has no gameplay. Its job is to figure out "Where am I?" and "Who is playing?"

- **Visual Layout:**
    
    - **Background:** Black.
        
    - **Center:** Your Studio Logo (Fade in/Fade out).
        
    - **Bottom:** A simple "Loading..." progress bar.
        
- **Key Functionality:**
    
    1. **Platform Check:** Scripts determine if the game is running on Android, Web Desktop, or Web Mobile.
        
    2. **Service Injection:** Based on the platform, it initializes the correct Ad Provider and Save System.
        
    3. **Data Load:** It pulls the player's Save JSON (Total Gold, Unlocked Characters) from disk or cloud.
        
    4. **Transition:** Once data is loaded (usually < 2 seconds), it automatically changes scene to MainMenu.
        

---

## 2. SCENE: MainMenu (The Hub)

**Purpose:** The visual hook. It needs to look expensive to sell the "High Quality" vibe immediately.

- **Visual Layout:**
    
    - **Background:** A scrolling parallax image of a dark fantasy landscape (ruins, fog).
        
    - **Center Stage:** The currently selected Hero (Idle Animation). Note: Use your high-quality pre-rendered sprite here, scaled up 2x so we see the details.
        
    - **Top Right:** "Wallet" Widget (Icon of Gold Coin + Amount).
        
- **UI Elements (CanvasLayer):**
    
    - **Play Button:** Huge, glowing, bottom center.
        
    - **Shop Button:** Icon of a Bag/Anvil.
        
    - **Settings Button:** Gear Icon (Top Left).
        
- **The Shop Popup (Panel):**
    
    - Overlay: When clicked, this covers 90% of the screen.
        
    - **Tab 1 (Barracks):** Horizontal scroll of Characters. Locks/Unlocks based on Gold.
        
    - **Tab 2 (Armory):** A grid of upgrade icons (Vitality, Might, Luck). Clicking one shows cost and "Buy" button.
        
    - **Tab 3 (Treasury):**
        
        - Button 1: "Watch Ad for +500 Gold".
            
        - Button 2: "Buy 10k Gold ($0.99)".
            
- **Key Logic:**
    
    - When the player changes the character in the Shop, the "Center Stage" sprite in the background must update immediately.
        

---

## 3. SCENE: GameScene (The Core Loop)

**Purpose:** This is where 99% of the game happens. It is the "Engine Room."

- **Node Hierarchy (Mental Map):**
    
    - **Map Layer:**
        
        - **Infinite TileMap:** A system that repeats the ground texture (Grass/Dirt) infinitely as the player moves.
            
    - **Gameplay Layer (Y-Sort Enabled):**
        
        - Crucial: This layer must have "Y-Sort" enabled. This means if the Player is below an Enemy on the Y-axis, the Player draws on top of the Enemy. This creates the 2.5D depth.
            
        - **Projectiles Container:** (Bullets, Arrows).
            
        - **XP Gems Container:** (Little floating crystals).
            
        - **Enemies Container:** (All active monsters).
            
        - **Player:** The Hero node. Contains the Sprite, CollisionShape, and Camera2D.
            
        - **Floating Text Manager:** Invisible container that spawns "15 DMG" text popups.
            
    - **Managers (Non-Visual):**
        
        - **EnemySpawner:** Logic that decides "It is minute 2:00, spawn 50 skeletons at the edge of the screen."
            
        - **Director:** Tracks time (00:00) and difficulty scaling.
            

---

## 4. SCENE: HUD (Heads-Up Display)

**Purpose:** The User Interface overlay that sits on top of the GameScene.

- **Visual Layout:**
    
    - **Top Left:** HP Bar (Red, fills horizontally).
        
    - **Top Center:** Experience Bar (Blue/Yellow, spans the whole top width). Shows Level number (e.g., "LVL 5").
        
    - **Top Right:** Kill Counter (Skull Icon: 142) and Timer (02:45).
        
    - **Bottom Left (Mobile Only):** Virtual Joystick area. Invisible, but shows a UI knob when touched.
        
    - **Center Screen (Hidden by default):** The **Level Up Selection** Panel.
        
- **The Level Up Panel:**
    
    - Appears when Game Pauses.
        
    - **Three Vertical Cards:** Each card has an Icon, Name (e.g., "Holy Water"), and Description ("Creates a damaging zone").
        
    - **Button:** "Watch Ad to Reroll" (Small button below the cards).
        
- **Key Logic:**
    
    - Must have "Anchors" set up correctly so it looks good on a wide PC monitor AND a tall phone screen.
        

---

## 5. SCENE: Results (Game Over)

**Purpose:** The emotional conclusion and the monetization checkpoint.

- **Visual Layout:**
    
    - **Header:** Big Text. "VICTORY" (Gold color) or "DEFEATED" (Red color).
        
    - **Middle:** A summary box.
        
        - Time Survived: 12:34
            
        - Enemies Killed: 1,502
            
        - Gold Found: 350
            
    - **The "Ad Hook" (Flashy Button):**
        
        - "WATCH AD TO DOUBLE GOLD (Get 700!)"
            
        - Logic: If clicked, plays ad 
            
            ```
            →→
            ```
            
             adds 700 to save. If skipped, adds 350.
            
    - **Bottom:** "Home" (House Icon) and "Retry" (Rotate Icon).
        
- **Key Logic:**
    
    - This scene must communicate with the SaveSystem to write the new Gold balance to the disk permanently.
        
    - If Interstitial Ads are enabled, the ad usually plays before this scene fades in, or immediately after pressing "Home."

## 5. UI PANEL: CharacterSelector (The Barracks)

**Purpose:** This is where the player swipes through heroes, reads their stats, and spends Gold/Ads to unlock them.

- **Access:** Opened by clicking a "Swap Hero" or "Barracks" button on the Main Menu.
    
- **Visual Layout:**
    
    - **Background:** Darkened/Blurred version of the Main Menu background (Modal feel).
        
    - **Center:** **The Carousel.** A horizontal list of character cards.
        
        - Selected Card: Large, in the middle. Shows the high-res 3D render of the Hero.
            
        - Side Cards: Smaller, faded out (showing the next/previous heroes).
            
    - **Bottom Info Panel (Dynamic):**
        
        - **Header:** Character Name (e.g., "The Pyromancer") and Class Icon.
            
        - **Stats Block:** Simple bars or text. "Speed: High", "HP: Low".
            
        - **Starting Weapon:** Icon of the weapon they start with (e.g., Molotov).
            
    - **Action Button (The most important part):**
        
        - State A (Owned): Green button saying **"SELECT"**.
            
        - State B (Locked - Gold): Yellow button saying **"BUY: 10,000 G"**.
            
        - State C (Locked - Ads): Blue button saying **"WATCH ADS (2/5)"**.
            
- **Navigation:** A "Back" arrow in the top left to return to the Main Menu.
    

---

## 6. UI PANEL: Shop (The Upgrades)

**Purpose:** Where players spend Gold to get permanent stat boosts.

- **Access:** Opened by the "Shop" button on Main Menu.
    
- **Layout Structure:**
    
    - **Top Bar:** "TOTAL GOLD: 12,500" (Must always be visible so players know what they can afford).
        
    - **Tab System:**
        
        - **Tab 1: Stats (The Armory)**
            
        - **Tab 2: Gold (The Treasury)**
            
- **Tab 1 Content (Grid Container):**
    
    - A grid of **Upgrade Slots**.
        
    - **The Slot Design:**
        
        - **Icon:** (e.g., A Flexed Arm for "Might").
            
        - **Pips:** Little dots showing progress (e.g., 3/10 lit up).
            
        - **Button:** "+" Sign.
            
    - **Selection Logic:** Clicking a slot updates a "Detail View" at the bottom of the screen.
        
        - Detail View: "MIGHT (Level 3 -> 4). Increases base damage by 10%. Cost: 500 Gold."
            
        - Buy Button: Big button at the bottom. Disables if Gold is insufficient.
            
- **Tab 2 Content (Gold Buying):**
    
    - **Vertical List:**
        
        - **"Small Sack":** 5,000 Gold ($0.99).
            
        - **"Big Chest":** 50,000 Gold ($4.99).
            
        - **"Daily Grant":** 500 Gold (Watch Video Icon).
            

---

## 7. UI PANEL: PauseMenu (In-Game)

**Purpose:** Stops the chaos, allows quitting or tweaking settings.

- **Access:** Small "Pause" (||) button in the top-right of the HUD during gameplay.
    
- **Visual Layout:**
    
    - **Overlay:** Semi-transparent black (dim the game).
        
    - **Center Box:**
        
        - **Header:** "PAUSED".
            
        - **Stats Summary:** "Current Level: 12", "Gold Collected: 85".
            
        - **Current Equipment:** Row of icons showing what weapons you have (so players remember their build).
            
    - **Buttons (Vertical Column):**
        
        - **RESUME:** Closes panel.
            
        - **SETTINGS:** Toggles SFX/Music sliders.
            
        - **GIVE UP / HOME:** **Warning:** This needs a confirmation popup ("Are you sure? You will lose progress" - or "You will keep Gold but lose the run").
            

---

## 8. UI PANEL: ReviveScreen (The Second Chance)

**Purpose:** Extremely high-value monetization screen. Appears **before** the Game Over screen.

- **Trigger:** When Player HP reaches 0.
    
- **Logic:**
    
    - Pause game immediately.
        
    - Check: Has the player already revived this run? (Limit 1).
        
    - If No -> Show Screen.
        
    - If Yes -> Go straight to Game Over.
        
- **Visual Layout:**
    
    - **Background:** Red vignette / heartbeat effect.
        
    - **Center Text:** "YOU DIED!"
        
    - **Countdown:** A large timer counting down: 5... 4... 3...
        
    - **The Hero Button (Center):**
        
        - **"REVIVE (50% HP)"** accompanied by a **Video Ad Icon**.
            
    - **The Cancel Button (Bottom):**
        
        - "No thanks, let me die."
            
- **Flow:**
    
    - If Player clicks "Revive": Show Ad -> Wait for close -> Heal Player -> Remove ReviveScreen -> Give 3 seconds of invincibility -> Resume Game.
        
    - If Timer hits 0 or "No thanks" clicked: Go to Results (Game Over) scene.


https://aistudio.google.com/prompts/1vj1YPn1up7iepFTZrgeBQobt9C2YAgxP
