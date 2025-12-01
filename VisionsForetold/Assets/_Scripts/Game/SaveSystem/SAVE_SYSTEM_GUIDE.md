# Save System - Setup & Troubleshooting Guide

## Quick Setup Checklist

### ? Required Components

1. **SaveManager (Scene)**
   - Create empty GameObject named "SaveManager"
   - Add `SaveManager.cs` component
   - Check "Don't Destroy On Load" in Inspector
   - ? Should persist across scenes

2. **SaveStation (Interaction Points)**
   - Create GameObject where player should save (e.g., "SavePoint")
   - Add `SaveStation.cs` component
   - Add Collider component (Box/Sphere)
   - ? Set Collider to "Is Trigger"
   - Assign SaveStationMenu reference in Inspector
   - Assign Prompt UI (optional visual feedback)

3. **SaveStationMenu (UI Canvas)**
   - Create UI Canvas
   - Add `SaveStationMenu.cs` component
   - Assign all UI panel references:
     - Menu Panel
     - Save Panel
     - Skills Panel (optional)
     - Confirmation Dialog

4. **Player Setup**
   - Tag player GameObject as "Player"
   - Add `PlayerInput` component (New Input System)
   - Add `Health` component
   - Define "Interact" action in Input Actions

---

## Common Issues & Fixes

### ? "SaveManager instance not found!"

**Problem:** SaveManager doesn't exist or isn't initialized

**Solutions:**
1. Create GameObject with SaveManager component
2. Make sure it's marked "DontDestroyOnLoad"
3. Check for duplicate SaveManagers in scene
4. Ensure SaveManager.Awake() runs before SaveStation.Start()

**Quick Fix:**
```
Tools ? Save System ? Run Diagnostics
```

---

### ? "Cannot save when near station"

**Problem:** Interaction not triggering

**Root Causes:**

#### 1. Missing "Interact" Action
**Check:** Does your Input Actions asset have an "Interact" action?

**Fix:**
1. Open Input Actions asset
2. Add action named "Interact" (case-sensitive!)
3. Bind to keyboard key (E) and gamepad button (Y/Triangle)
4. Save and regenerate C# class

#### 2. Player Missing Components
**Check:** Does player have:
- PlayerInput component?
- Player tag?

**Fix:**
```csharp
// Select player GameObject
// Inspector ? Tag ? Player
// Add Component ? Player Input
```

#### 3. Collider Not Trigger
**Check:** SaveStation collider settings

**Fix:**
```
Select SaveStation GameObject
Inspector ? Collider component
? Is Trigger = TRUE
```

#### 4. SaveStationMenu Not Assigned
**Check:** SaveStation Inspector

**Fix:**
```
Select SaveStation GameObject
Inspector ? SaveStation component
UI References ? Save Station Menu = [Drag SaveStationMenu]
```

---

### ? "Save button does nothing"

**Problem:** UI not connected or SaveManager missing

**Diagnosis:**
```
1. Press Play
2. Open Save Station menu
3. Check Console for errors
4. Look for:
   - "SaveManager is null"
   - "Menu panel not assigned"
   - Button click logs
```

**Fixes:**

#### If "SaveManager is null":
1. Ensure SaveManager exists in scene
2. Check SaveManager.Instance is not null
3. Wait for SaveManager to initialize (happens in Awake)

#### If "Menu panel not assigned":
1. Select SaveStationMenu GameObject
2. Inspector ? Assign all UI references
3. Drag UI panels from hierarchy

#### If no button click logs:
1. Select button in hierarchy
2. Inspector ? Button component
3. Check OnClick() has listeners
4. Verify button is interactable (not grayed out)

---

### ? "Player data not saving"

**Problem:** CollectPlayerData() failing

**Check Console For:**
- "Player not found"
- "Health component not found"

**Fixes:**

#### Player not found:
```csharp
// Player GameObject MUST have tag "Player"
GameObject player = GameObject.FindGameObjectWithTag("Player");
```

#### Health not found:
```csharp
// Add Health component to player
// Or modify SaveManager.CollectPlayerData() to handle missing Health
```

---

### ? "Save file corrupted/not loading"

**Problem:** JSON deserialization error

**Diagnosis:**
```
Tools ? Save System ? Show Save Directory
```

**Fixes:**

1. **Delete corrupted save:**
```
Tools ? Save System ? Clear All Saves
```

2. **Check save file manually:**
- Open .json file in text editor
- Verify JSON is valid
- Look for missing braces/quotes

3. **Backup system:**
- Save system creates .backup files
- Rename .backup to .json to restore

---

## Optimization Tips

### Performance

#### 1. Reduce Save Frequency
```csharp
// SaveManager.cs
[SerializeField] private float autoSaveInterval = 600f; // 10 minutes instead of 5
```

#### 2. Async Saving (Advanced)
```csharp
// Use async file I/O for large saves
await File.WriteAllTextAsync(filePath, json);
```

#### 3. Compression
```csharp
// Compress JSON before saving
string compressed = CompressString(json);
```

### Memory

#### 1. Clear Old Saves
```csharp
// Limit number of saves
if (GetSaveCount() > maxSaves)
{
    DeleteOldestSave();
}
```

#### 2. Lazy Loading
```csharp
// Don't keep currentSaveData in memory when not needed
currentSaveData = null; // After saving
```

---

## Testing

### Quick Test Procedure

1. **Run Diagnostics:**
```
Tools ? Save System ? Run Diagnostics
```

2. **Test Save:**
- Enter Play mode
- Walk to SaveStation
- Press E (or Y on gamepad)
- Click "Save Game"
- Check Console for "? Game saved successfully"

3. **Test Load:**
- Exit Play mode
- Enter Play mode again
- Menu ? Load Game
- Select save slot
- Verify player position/health restored

4. **Check Save Files:**
```
Tools ? Save System ? Show Save Directory
```
- Verify .json files exist
- Open in text editor to inspect

---

## Debug Logging

### Enable Verbose Logging

All save system components have detailed logging. Check Console for:

**SaveManager:**
- `[SaveManager] Created save directory`
- `[SaveManager] Saving to slot X`
- `[SaveManager] ? Game saved successfully`

**SaveStation:**
- `[SaveStation] ? Player entered range`
- `[SaveStation] ? Subscribed to Interact action`
- `[SaveStation] ? Opened save station menu`

**SaveStationMenu:**
- `[SaveStationMenu] Menu opened successfully`
- `[SaveStationMenu] Save successful to slot X`

### No Logs Appearing?

Check Console filtering:
- Clear filters (top-right)
- Enable all log types (Info, Warning, Error)
- Check "Collapse" is off

---

## Advanced Troubleshooting

### Input System Not Working

**Check:**
1. Player has PlayerInput component
2. Input Actions asset exists
3. "Interact" action defined
4. Action Map "Player" exists
5. Action Map is enabled

**Test Input Directly:**
```csharp
// Add to Update() in SaveStation
if (Input.GetKeyDown(KeyCode.E))
{
    Debug.Log("E key pressed (old input)");
}

if (interactAction != null && interactAction.IsPressed())
{
    Debug.Log("Interact action pressed (new input)");
}
```

### Menu Not Responding

**Check:**
1. EventSystem exists in scene
2. Canvas has GraphicRaycaster
3. Buttons have Button component
4. Buttons are not blocked by other UI

**Test:**
```csharp
// In SaveStationMenu.Update()
if (Input.GetMouseButtonDown(0))
{
    Debug.Log($"Clicked at {Input.mousePosition}");
    Debug.Log($"Selected object: {eventSystem.currentSelectedGameObject}");
}
```

---

## Support

### Report Issues

Include in bug report:
1. Console logs (full trace)
2. SaveStation Inspector screenshot
3. SaveManager Inspector screenshot
4. Player Inspector screenshot
5. Steps to reproduce

### Run Full Diagnostic

```
Tools ? Save System ? Run Diagnostics
```

Copy entire console output and include in bug report.

---

## Summary: Most Common Fixes

| Issue | Quick Fix |
|-------|-----------|
| Can't interact with station | Check "Interact" action exists in Input Actions |
| Save button does nothing | Run diagnostics, check SaveManager exists |
| Player data not saving | Tag player as "Player", add Health component |
| Menu won't open | Assign SaveStationMenu reference in SaveStation |
| Saves disappear | Check save directory permissions |

**First Step for Any Issue:**
```
Tools ? Save System ? Run Diagnostics
```

This will identify 90% of setup problems!
