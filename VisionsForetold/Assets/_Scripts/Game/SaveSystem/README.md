# Save System Documentation

## Overview

A comprehensive save system for VISIONSFORETOLD that allows players to save their game progress at designated save stations. The system includes menu UI for saving, viewing skills, and exiting save stations.

## Components

### 1. **SaveBase.cs** - Data Structures
- `SaveData` - Main save data class containing all player information
- `SkillData` - Player skills and progression data

### 2. **SaveManager.cs** - Save/Load Manager (Singleton)
Handles all save and load operations with the following features:
- Multiple save slots (default: 3)
- Auto-save functionality (optional)
- JSON-based save format
- Persistent data storage

### 3. **SaveStation.cs** - Interactive Save Points
- Place on objects in your scene where players can save
- Trigger-based interaction
- Supports both Legacy and New Input System
- Visual feedback when player is in range

### 4. **SaveStationMenu.cs** - UI Menu System
- Main menu with three options: Save, Skills, Exit
- Save panel for naming and slot selection
- Skills panel (to be expanded with your skill system)
- Confirmation dialogs for overwriting saves

## Setup Instructions

### Step 1: Create Save Manager
1. Create empty GameObject: `SaveManager`
2. Add component: `SaveManager`
3. Configure settings:
   - Max Save Slots: 3 (default)
   - Auto Save: Enable/Disable
   - Auto Save Interval: 300s (5 minutes)

### Step 2: Create Save Station
1. Create GameObject (e.g., Cube, special model) in your scene
2. Add `SaveStation` component
3. Add Collider component (will be set to trigger automatically)
4. Assign references:
   - Prompt UI (Canvas with prompt text)
   - Save Station Menu reference
5. Configure interaction range (default: 3 units)

### Step 3: Create UI

#### A. Create Save Station Menu Canvas
```
Canvas (SaveStationMenu)
??? MainMenu Panel
?   ??? Save Button
?   ??? Skills Button
?   ??? Exit Button
??? Save Panel
?   ??? Save Name Input Field
?   ??? Slot Dropdown
?   ??? Confirm Save Button
?   ??? Cancel Button
?   ??? Status Text
??? Skills Panel
?   ??? Skill Content Area
?   ??? Close Button
??? Confirmation Dialog
    ??? Message Text
    ??? Yes Button
    ??? No Button
```

#### B. Setup SaveStationMenu Component
1. Add `SaveStationMenu` component to Canvas
2. Assign all UI references in Inspector
3. Set default save slot (0-2)

### Step 4: Connect Save Station to UI
1. Select SaveStation GameObject
2. In Inspector, assign:
   - Prompt UI ? Your interaction prompt
   - Save Station Menu ? The SaveStationMenu component

## Usage

### For Designers

**Placing Save Stations:**
1. Duplicate existing save station prefab
2. Position in scene
3. Adjust interaction range as needed
4. Test by walking up to it and pressing E

**Visual Customization:**
- Assign Active Visual GameObject for highlight effects
- Adjust Highlight Color
- Modify Pulse Speed for glow effect

### For Programmers

**Saving Game Programmatically:**
```csharp
// Get save manager
SaveManager saveManager = SaveManager.Instance;

// Save to slot 0 with custom name
saveManager.SaveGame(0, "Boss Defeated Save");

// Check if save exists
bool exists = saveManager.DoesSaveExist(0);

// Load game from slot
saveManager.LoadGame(0);

// Delete save
saveManager.DeleteSave(0);
```

**Accessing Current Save Data:**
```csharp
SaveData currentSave = SaveManager.Instance.GetCurrentSaveData();
if (currentSave != null)
{
    Debug.Log($"Current save: {currentSave.saveName}");
    Debug.Log($"Player level: {currentSave.skills.level}");
}
```

**Extending Save Data:**
To add new data to saves, edit `SaveBase.cs`:

```csharp
[Serializable]
public class SaveData
{
    // Existing fields...
    
    // Add new fields
    public int currency;
    public string[] inventoryItems;
    public QuestData[] completedQuests;
}
```

Then update `SaveManager.cs` to collect/apply this data:

```csharp
// In CollectPlayerData():
currentSaveData.currency = GetPlayerCurrency();
currentSaveData.inventoryItems = GetPlayerInventory();

// In ApplyPlayerData():
SetPlayerCurrency(currentSaveData.currency);
SetPlayerInventory(currentSaveData.inventoryItems);
```

**Save Events:**
Subscribe to save system events:

```csharp
private void OnEnable()
{
    SaveManager.Instance.OnGameSaved += OnGameSaved;
    SaveManager.Instance.OnGameLoaded += OnGameLoaded;
    SaveManager.Instance.OnSaveError += OnSaveError;
}

private void OnGameSaved(SaveData data)
{
    Debug.Log($"Game saved: {data.saveName}");
    // Show "Game Saved" notification
}

private void OnGameLoaded(SaveData data)
{
    Debug.Log($"Game loaded: {data.saveName}");
    // Initialize systems with loaded data
}

private void OnSaveError(string error)
{
    Debug.LogError($"Save error: {error}");
    // Show error message to player
}
```

## File Structure

```
Assets/_Scripts/Game/SaveSystem/
??? SaveBase.cs           - Save data classes
??? SaveManager.cs        - Save/load manager
??? SaveStation.cs        - Interactive save point
??? SaveStationMenu.cs    - UI menu controller
??? README.md            - This file
```

## Save File Location

Saves are stored in:
- **Windows:** `%USERPROFILE%\AppData\LocalLow\<CompanyName>\<GameName>\Saves\`
- **Mac:** `~/Library/Application Support/<CompanyName>/<GameName>/Saves/`
- **Linux:** `~/.config/unity3d/<CompanyName>/<GameName>/Saves/`

Files are named: `save_0.json`, `save_1.json`, `save_2.json`

## Features

### Current Features
- ? Multiple save slots (3 by default)
- ? Auto-save functionality
- ? Interactive save stations
- ? Save/Load game state
- ? Player position and health saving
- ? Scene persistence
- ? Skills data structure
- ? Overwrite confirmation
- ? Both input systems supported

### To Be Implemented
- ? Skill tree UI integration
- ? Inventory system integration
- ? Quest system integration
- ? Save file encryption
- ? Cloud save support
- ? Save file migration/versioning

## Troubleshooting

### Save Station Not Working
1. Check Player has "Player" tag
2. Ensure SaveStation has trigger collider
3. Verify Interaction Range is large enough
4. Check SaveStationMenu is assigned

### Can't Save/Load
1. Check SaveManager exists in scene (DontDestroyOnLoad)
2. Verify save directory permissions
3. Check Console for error messages
4. Ensure SaveManager.Instance is not null

### UI Not Showing
1. Verify Canvas is set up correctly
2. Check all UI references in Inspector
3. Ensure UI is child of Canvas
4. Check Canvas render mode

### Input Not Working
1. If using New Input System:
   - Verify "Interact" action exists
   - Check Player has PlayerInput component
2. If using Legacy Input:
   - Check correct key code is set (default: E)

## Best Practices

1. **Always test saves before building:**
   - Test saving in different scenarios
   - Test loading after quitting
   - Test all save slots

2. **Add save version for future updates:**
   ```csharp
   public int saveVersion = 1;
   ```

3. **Validate loaded data:**
   - Check for null values
   - Verify data ranges
   - Handle corrupted saves gracefully

4. **Backup saves during development:**
   - Keep backup of test saves
   - Document save structure changes

5. **Test auto-save:**
   - Ensure it doesn't interrupt gameplay
   - Test performance impact
   - Verify save integrity

## Example Scene Setup

```
Scene Hierarchy:
??? SaveManager (DontDestroyOnLoad)
??? Player
??? SaveStation_01
?   ??? Prompt UI
??? SaveStation_02
?   ??? Prompt UI
??? UI
    ??? SaveStationMenuCanvas
        ??? MainMenu
        ??? SavePanel
        ??? SkillsPanel
        ??? ConfirmationDialog
```

## Support

For issues or questions:
- Check Console for error messages
- Verify all components are properly assigned
- Review this README
- Check code comments for detailed explanations

