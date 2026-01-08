# Character Selection System

**Status:** ✅ Complete  
**Last Updated:** Dec 31, 2024

---

## Overview

Complete character selection screen allowing players to view, purchase, and select characters. Integrates with shop upgrade system to display accurate stats.

---

## Components

### CharacterCard.cs
**Purpose:** Individual character card UI component

**Features:**
- Character icon + weapon icon display
- Name text display
- Locked/unlocked visual states (59% opacity for locked)
- Click event handling

**UI Structure:**
- Character Icon (Image)
- Weapon Icon (Image - bottom-right)
- Name Text (TextMeshProUGUI)
- Card Button (Button)

---

### CharacterDetailsPanel.cs
**Purpose:** Display character stats and weapon info

**Features:**
- Shows character portrait + weapon icon
- Displays weapon name
- **Applies shop upgrades to stats for unlocked characters**
- Shows base stats only for locked characters

**Stat Display:**
- `Health: 150 (100 +50)` - shows base + bonus
- `Speed: 6.0 (5.0 +20%)` - shows base + percentage
- `Damage: 1.30x (1.0x +30%)` - shows base + percentage

---

### CharacterSelectController.cs
**Purpose:** Main selection controller

**Key Responsibilities:**
- Creates character cards dynamically
- Manages character purchasing (gold deduction, unlocking)
- Handles character selection (persistence via SaveData)
- Controls 3-state action button (BUY/CONFIRM/PLAY)
- Fully synchronous for plugin compatibility

---

## Action Button States

| State | When | Display | Action |
|-------|------|---------|--------|
| **BUY** | Locked character | Price number + coin icon | Deducts gold, unlocks character |
| **CONFIRM** | Unlocked, not selected | "CONFIRM" text | Saves selection, switches to PLAY |
| **PLAY** | Currently selected | "PLAY" text | Loads GameScene |

---

## SaveData Extensions

**New Fields:**
```csharp
public List<string> unlockedCharacterIDs;
public string selectedCharacterID;
```

**Helper Methods:**
```csharp
bool IsCharacterUnlocked(string characterId)
void UnlockCharacter(string characterId)
```

**Default Unlocked:**
- Knight (free starter)
- Pyromancer (free starter)

---

## Shop Upgrade Integration

### Character Details Panel
Shows final stats with shop upgrades applied for unlocked characters:

**Health:**
```csharp
finalHP = baseHP + (vitalityLevel * 10);
```

**Speed:**
```csharp
finalSpeed = baseSpeed * (1f + greavesLevel * 0.05f);
```

**Damage:**
```csharp
finalDamage = baseDamage * (1f + mightLevel * 0.10f);
```

### In-Game Application
Shop upgrades automatically apply in GameScene:
- `PlayerHealth` loads Vitality/Might from SaveData
- `PlayerMovement` loads Greaves from SaveData
- Collectibles load Magnet from SaveData

---

## Navigation Flow

```
Main Menu
    ↓ PLAY
Character Selection
    ↓ PLAY          ↓ BACK
GameScene      Main Menu
```

**Integration:**
- `MainMenuManager.OnPlayClicked()` → Shows character selection panel
- `CharacterSelectController.OnBackClicked()` → Returns to main menu
- `CharacterSelectController.StartGame()` → Loads GameScene

---

## Synchronous Save Service

**Added to ISaveService:**
```csharp
SaveData LoadDataSync()
void SaveDataSync(SaveData data)
```

**Why:** Plugin compatibility (Yandex, etc.) that don't support async/await.

**Implementation:** `PlayerPrefsSaveService.cs`

---

## Unity Setup

### CharacterSelectController Inspector
- **Available Characters:** All CharacterData SOs (5 total)
- **Character Cards Container:** ScrollView Content Transform
- **Character Card Prefab:** CharacterCard prefab
- **Details Panel:** CharacterDetailsPanel component
- **Gold Text:** TextMeshProUGUI
- **Back Button:** Button
- **Action Button:** Button (parent)
  - Confirm Container (child with "CONFIRM" text)
  - Play Container (child with "PLAY" text)
  - Buy Container (child with price text + coin icon)
- **Buy Price Text:** TextMeshProUGUI in Buy Container

### CharacterManager Inspector (CRITICAL)
**Location:** GameScene Hierarchy

**Required Fields:**
- **Default Character:** Knight (fallback if SaveData fails)
- **All Characters:** Array of ALL 5 CharacterData assets
  - Knight
  - Pyromancer
  - Ranger
  - Santa
  - Zombie
  
**Why:** CharacterManager loads selected character from SaveData on game start. It searches through `allCharacters` array to find the character by ID. If this array is empty or not assigned, character selection persistence won't work.

**Note:** CharacterData assets are in `Assets/ScriptableObjects/Characters/` folder (not in Resources).

### CharacterCard Prefab
- Button component on root GameObject
- Character Icon (Image)
- Weapon Icon (Image - anchored bottom-right)
- Name Text (TextMeshProUGUI)

---

## Bug Fixes

### Bug #1: Gold Display Not Updating
**Issue:** Gold total in main menu didn't update after earning gold in GameScene or spending in shop/character selection.

**Fix:** Added `OnEnable()` to `MainMenuManager` and `CharacterSelectController` to reload SaveData and refresh gold display whenever panels become active.

```csharp
private void OnEnable()
{
    RefreshGoldDisplay(); // Reloads from SaveData
}
```

### Bug #2: Character Card Opacity Not Updating After Purchase
**Issue:** After purchasing a locked character, card remained at 59% opacity instead of changing to 100%.

**Fix:** Modified `PurchaseCharacter()` to find and re-initialize the purchased character's card with `isLocked = false`.

```csharp
// Update visual state after purchase
for (int i = 0; i < availableCharacters.Length && i < characterCards.Count; i++)
{
    if (availableCharacters[i].characterID == character.characterID)
    {
        characterCards[i].Initialize(character, false); // Now unlocked
        break;
    }
}
```

### Bug #3: Selected Character Not Persisting After App Restart
**Issue:** Selecting Santa, restarting game, pressing PLAY spawned Pyromancer (or default character) instead.

**Root Cause:** 
- `CharacterManager.SelectedCharacter` is a static variable
- Static variables reset to `null` when application restarts
- CharacterManager fell back to `defaultCharacter` when static was null

**Fix:** Modified `CharacterManager.Awake()` to:
1. Check if static `SelectedCharacter` is null
2. Load `selectedCharacterID` from SaveData
3. Search through Inspector-assigned `allCharacters` array
4. Find matching CharacterData by ID
5. Set `SelectedCharacter` to the loaded character

**Critical:** This requires **All Characters** array to be assigned in CharacterManager Inspector with all 5 CharacterData assets.

---

## Character Data Requirements

Each CharacterData ScriptableObject:
- `characterID` (string, lowercase)
- `displayName` (string)
- `portrait` (Sprite)
- `goldCost` (int)
- `starterWeapon` (WeaponData)
- `maxHealth` (float)
- `moveSpeed` (float)
- `baseDamageMultiplier` (float)
- `characterPrefab` (GameObject)

---

## Testing Checklist

- [x] Knight & Pyromancer unlocked by default
- [x] Locked characters at 59% opacity
- [x] BUY button shows price only
- [x] Purchase deducts gold correctly
- [x] CONFIRM → PLAY transition instant
- [x] PLAY loads GameScene
- [x] Selection persists after restart
- [x] Stats show shop upgrades for unlocked
- [x] Back button returns to main menu
- [x] All synchronous (no delays)

---

## Files

**Scripts:**
- `Assets/Scripts/UI/CharacterSelect/CharacterCard.cs`
- `Assets/Scripts/UI/CharacterSelect/CharacterDetailsPanel.cs`
- `Assets/Scripts/UI/CharacterSelect/CharacterSelectController.cs`

**Modified:**
- `Assets/Scripts/Services/SaveData.cs` (character tracking)
- `Assets/Scripts/Services/ISaveService.cs` (sync methods)
- `Assets/Scripts/Services/PlayerPrefsSaveService.cs` (sync impl)
- `Assets/Scripts/UI/MainMenu/MainMenuManager.cs` (navigation)

---

## Status: ✅ PRODUCTION READY
