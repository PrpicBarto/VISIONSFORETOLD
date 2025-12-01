# Save System - Quick Start

## ?? Getting Started in 5 Minutes

### Step 1: Run Diagnostics (30 seconds)
```
Unity Menu ? Tools ? Save System ? Run Diagnostics
```

This will tell you exactly what's missing!

### Step 2: Fix Any Issues (2-3 minutes)

Based on diagnostics output:

#### ? SaveManager not found
1. Create empty GameObject: "SaveManager"
2. Add component: `SaveManager`
3. (Optional) Add component: `SaveSystemMonitor` for runtime debugging

#### ? No SaveStations found
1. Create GameObject where player should save: "SaveStation"
2. Add component: `SaveStation`
3. Add component: `Box Collider` or `Sphere Collider`
4. Check "Is Trigger" on collider
5. Assign references in Inspector

#### ? Player not found
1. Select your player GameObject
2. Tag ? "Player"
3. Add component: `PlayerInput` (if using New Input System)
4. Ensure player has `Health` component

#### ? 'Interact' action not found
1. Open your Input Actions asset
2. Create action: "Interact"
3. Bind to: E (keyboard) + Y/Triangle (gamepad)
4. Save asset

### Step 3: Connect UI (1-2 minutes)

1. **Create UI Canvas** (if you don't have one)
   - Right-click Hierarchy ? UI ? Canvas

2. **Add SaveStationMenu Script**
   - Select Canvas
   - Add Component ? SaveStationMenu

3. **Create UI Panels**
   - Create child panels for:
     - Main Menu (buttons: Save, Skills, Exit)
     - Save Panel (input field, dropdown, buttons)
     - Confirmation Dialog (text, yes/no buttons)

4. **Assign References**
   - Select Canvas
   - Inspector ? SaveStationMenu component
   - Drag panels and buttons to corresponding fields

5. **Link SaveStation to Menu**
   - Select SaveStation GameObject
   - Inspector ? SaveStation component
   - Drag Canvas to "Save Station Menu" field

### Step 4: Test (1 minute)

1. Press Play
2. Walk to SaveStation
3. Press E (or Y on gamepad)
4. Menu should open!
5. Click "Save Game"
6. Check Console for "? Game saved successfully"

---

## ?? Runtime Debugging

### Press F12 to toggle on-screen monitor

Shows:
- SaveManager status
- Current save info
- Player position/health
- Number of save files

### Check Console Logs

Look for these symbols:
- ? = Success
- ? = Error (fix this!)
- ? = Warning (might work, but fix recommended)

---

## ?? Common First-Time Issues

### "Menu doesn't open when I press E"

**Quick Fix:**
1. `Tools ? Save System ? Run Diagnostics`
2. Look for "? 'Interact' action not found"
3. Add "Interact" action to Input Actions asset
4. Bind to E key

### "Save button does nothing"

**Quick Fix:**
1. Check Console for errors
2. Verify SaveManager exists in scene
3. Ensure all UI references are assigned

### "Player position not saved"

**Quick Fix:**
1. Tag player as "Player"
2. Add Health component to player

---

## ?? Save File Location

To see your save files:
```
Tools ? Save System ? Show Save Directory
```

Location: `%AppData%/../LocalLow/[YourCompany]/[YourGame]/Saves/`

---

## ?? Reset During Development

To clear all saves:
```
Tools ? Save System ? Clear All Saves
```

? This deletes ALL save files! Use only for testing.

---

## ?? Full Documentation

- **Setup Guide:** `Assets/_Scripts/Game/SaveSystem/SAVE_SYSTEM_GUIDE.md`
- **Optimization Summary:** `Assets/_Scripts/Game/SaveSystem/OPTIMIZATION_SUMMARY.md`

---

## ? Quick Validation

After setup, this should work:

1. ? SaveManager exists in scene
2. ? Player tagged as "Player"
3. ? SaveStation has collider (Is Trigger = true)
4. ? SaveStation ? SaveStationMenu assigned
5. ? "Interact" action exists in Input Actions
6. ? Press E near SaveStation ? Menu opens
7. ? Click Save ? Console shows success
8. ? Save file created in directory

---

## ?? Still Having Issues?

1. **Run Diagnostics:**
   ```
   Tools ? Save System ? Run Diagnostics
   ```

2. **Check Console:**
   - Enable all log types (Info, Warning, Error)
   - Look for ? error messages

3. **Enable Runtime Monitor:**
   - Add `SaveSystemMonitor` component to SaveManager
   - Press F12 in Play mode
   - Check for ? errors on screen

4. **Read Full Guide:**
   - Open `SAVE_SYSTEM_GUIDE.md`
   - Find your specific issue
   - Follow troubleshooting steps

---

## ?? Success Checklist

- [ ] Diagnostics show all ? checks
- [ ] Can open save menu (Press E)
- [ ] Can save game (Click button)
- [ ] Console shows "? Game saved successfully"
- [ ] Save file exists in directory
- [ ] Can load game (restores player position/health)

**All checked? You're done!** ??

---

## Next Steps

### Add More Save Data

Edit `SaveManager.CollectPlayerData()`:
```csharp
// Add inventory
currentSaveData.inventory = player.GetComponent<Inventory>().GetItems();

// Add quests
currentSaveData.quests = QuestManager.Instance.GetActiveQuests();

// Add custom data
currentSaveData.customData = yourData;
```

### Customize UI

- Design your own save menu panels
- Add save slot previews
- Show save file dates/playtime
- Add confirmation dialogs

### Advanced Features

- Multiple save slots
- Auto-save on checkpoints
- Cloud save integration
- Save file encryption

Refer to `SAVE_SYSTEM_GUIDE.md` for detailed instructions!
