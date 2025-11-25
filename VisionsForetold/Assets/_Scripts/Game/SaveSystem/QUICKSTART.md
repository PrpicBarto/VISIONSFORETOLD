# Save System Quick Start Guide

## 5-Minute Setup

### 1. Create Save Manager (1 minute)
```
1. GameObject ? Create Empty ? Name: "SaveManager"
2. Add Component ? Save Manager
3. Done! (It will persist across scenes automatically)
```

### 2. Create Save Station (2 minutes)
```
1. Create a Cube (or your custom model)
2. Add Component ? Save Station
3. In Collider ? Check "Is Trigger" ?
4. Tag nearby area trigger or use existing collider range
```

### 3. Create Basic UI (2 minutes)
```
1. Right-click Hierarchy ? UI ? Canvas
2. Add panels for Main Menu, Save Panel, Skills Panel
3. Add component Save Station Menu to Canvas
4. Link SaveStation to this menu in Inspector
5. Ensure EventSystem exists (created automatically with Canvas)
```

## Input System Setup

### Required Input Action
Your Input Actions asset MUST have an "Interact" action in the "Player" action map:

**In InputSystem_Actions.inputactions:**
```
Action Map: "Player"
  ?? Action: "Interact"
      Type: Button
      Bindings:
        - Keyboard: E
        - Gamepad: Button North (Y on Xbox, Triangle on PlayStation)
```

### How It Works
- **Keyboard:** Press **E** to interact with save station
- **Gamepad:** Press **Y/Triangle** to interact with save station
- **Menu Navigation:**
  - Keyboard: Mouse/Arrow Keys
  - Gamepad: D-Pad/Left Stick + A/Cross to select

## Minimal Working Example

### Simple Save Station Setup
```
GameObject: SaveStation
??? BoxCollider (IsTrigger: true)
??? SaveStation Component
    ??? Interaction Range: 3
    ??? Player Tag: "Player"
    ??? Prompt UI: (your UI element)
    ??? Save Station Menu: (your menu)
```

### Code Example: Manual Save
```csharp
using VisionsForetold.Game.SaveSystem;

public class QuickSaveTest : MonoBehaviour
{
    void Update()
    {
        // Press F5 to quick save (keyboard only)
        if (Input.GetKeyDown(KeyCode.F5))
        {
            SaveManager.Instance.SaveGame(0, "Quick Save");
        }

        // Press F9 to quick load (keyboard only)
        if (Input.GetKeyDown(KeyCode.F9))
        {
            SaveManager.Instance.LoadGame(0);
        }
    }
}
```

## Testing

### Test Save (Keyboard):
1. Play game
2. Approach save station
3. Press **E** to interact
4. Use **Mouse** or **Arrow Keys** to navigate menu
5. Click/Press **Enter** on "Save"
6. Enter name, select slot
7. Confirm save

### Test Save (Gamepad):
1. Play game
2. Approach save station
3. Press **Y/Triangle** to interact
4. Use **D-Pad/Left Stick** to navigate menu
5. Press **A/Cross** to select "Save"
6. Navigate with **D-Pad**, select with **A/Cross**
7. Confirm save

### Test Load:
1. Quit game
2. Play game again
3. In main menu, call:
   ```csharp
   SaveManager.Instance.LoadGame(0);
   ```
4. You should spawn at saved position

## Common Issues & Fixes

| Issue | Fix |
|-------|-----|
| Can't interact with station | Check Player tag is "Player" |
| Menu doesn't open | Assign SaveStationMenu in Inspector |
| Save doesn't work | Check SaveManager exists in scene |
| No prompt shows | Assign Prompt UI GameObject |
| Input not working (Keyboard) | Check Input Action "Interact" exists (bound to E) |
| Input not working (Gamepad) | Check Input Action "Interact" exists (bound to Button North) |
| Can't navigate menu | Ensure EventSystem exists in scene |
| Gamepad navigation broken | Check "Enable Gamepad Navigation" in SaveStationMenu |

## Input Action Requirements

### Player Action Map
```
Interact:
  - Type: Button
  - Keyboard: <Keyboard>/e
  - Gamepad: <Gamepad>/buttonNorth (Y/Triangle)
```

### UI Action Map (Auto-handled by Unity)
```
Navigate:
  - Keyboard: <Keyboard>/upArrow, downArrow, etc.
  - Gamepad: <Gamepad>/leftStick, dpad
Submit:
  - Keyboard: <Keyboard>/enter
  - Gamepad: <Gamepad>/buttonSouth (A/Cross)
Cancel:
  - Keyboard: <Keyboard>/escape
  - Gamepad: <Gamepad>/buttonEast (B/Circle)
```

## What Gets Saved?

### By Default:
- ? Player Position
- ? Player Rotation
- ? Player Health
- ? Current Scene
- ? Save Name & Date
- ? Skill Data Structure

### To Add More:
Edit `SaveBase.cs` and add fields to `SaveData` class, then update `SaveManager` to collect/apply that data.

## UI Setup Requirements

### Required UI Elements:
1. **EventSystem** (auto-created with Canvas)
   - Ensure it has `StandaloneInputModule` component
   - For gamepad, this handles navigation automatically

2. **Canvas** with SaveStationMenu component
   - All buttons must be **Unity UI Buttons** (not TextMeshPro buttons)
   - Buttons should have **Navigation** set to "Automatic"

3. **Main Menu Panel:**
   - Save Button (Button component)
   - Skills Button (Button component)
   - Exit Button (Button component)

4. **Save Panel:**
   - Save Name Input Field (TMP_InputField)
   - Slot Dropdown (TMP_Dropdown)
   - Confirm Save Button
   - Cancel Button
   - Status Text (TMP_Text)

5. **Skills Panel:**
   - Close Button

6. **Confirmation Dialog:**
   - Message Text
   - Yes Button
   - No Button

## Gamepad Navigation Setup

### Button Navigation Settings:
For each button in Inspector:
```
Navigation:
  - Mode: Automatic (or Explicit if you want custom flow)
  - Visualize: Shows navigation connections
```

### Testing Gamepad Navigation:
1. Open menu with gamepad
2. You should see first button highlighted
3. Use D-Pad or Left Stick to move between buttons
4. Press A/Cross to select
5. Press B/Circle to go back/cancel

## Next Steps

1. Customize UI appearance
2. Add your skill tree to Skills Panel
3. Implement inventory saving (if applicable)
4. Add quest/progression saving
5. Test with both keyboard and gamepad in all scenes
6. Add save file encryption (for release)

## Files Included

- `SaveBase.cs` - Data structures
- `SaveManager.cs` - Main save system
- `SaveStation.cs` - Interaction component (New Input System)
- `SaveStationMenu.cs` - UI controller (Gamepad support)
- `README.md` - Full documentation
- `QUICKSTART.md` - This guide

## Input System Integration

The save system is fully integrated with Unity's New Input System:

- ? **Keyboard & Mouse** - E to interact, mouse to navigate
- ? **Gamepad** - Y/Triangle to interact, D-Pad/Stick to navigate
- ? **Action Maps** - Automatically switches between Player and UI maps
- ? **Event System** - Proper gamepad selection handling
- ? **Navigation** - Automatic button-to-button navigation

That's it! Your save system now works with both Keyboard and Gamepad! ????
