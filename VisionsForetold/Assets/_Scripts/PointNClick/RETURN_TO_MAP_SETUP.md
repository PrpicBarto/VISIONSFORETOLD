# ??? Return to Map Button Setup Guide

## Quick Setup for Point & Click Scenes

### Step 1: Add SceneConnectionManager to Your Scene
1. Create an empty GameObject in your Point & Click scene
2. Name it: `SceneConnectionManager`
3. Add Component ? `Scene Connection Manager`
4. In Inspector:
   - Assign your **Scene Connection Data** asset
   - Check **Show Return UI** ?
   - Set **Return Key** to your preference (default: M)

### Step 2: Create Return to Map Button (UI)

#### Option A: Add to Existing Close Button
1. Select your Close Button in the UI
2. Add Component ? **Return To Map Button**
3. Configure settings:
   - ? **Enable Escape Key** (ESC to close)
   - ? **Enable Gamepad Input** (B/Circle to close)
   - Optional: Assign **Cancel Action** from your Input Actions

#### Option B: Create New Button
1. Right-click in Hierarchy ? UI ? Button
2. Name it: `ReturnToMapButton`
3. Position it where you want (e.g., top-right corner)
4. Add Component ? **Return To Map Button**
5. Style the button (text: "Back to Map" or "×")

### Step 3: Setup Input Actions (For Gamepad)

**If you want gamepad support:**

1. Open your Input Actions asset (e.g., `InputSystem_Actions`)
2. Find or create an action called **"Cancel"** or **"Back"**
3. Bind it to:
   - **Gamepad B Button** (Xbox)
   - **Gamepad Circle Button** (PlayStation)
4. Save the Input Actions
5. In your **Return To Map Button**:
   - Drag the Input Action into **Cancel Action** slot

## Controls

### Keyboard
- **ESC** = Return to map (if enabled)
- **M** = Return to map (via SceneConnectionManager)

### Mouse
- **Click Close Button** = Return to map

### Gamepad
- **B/Circle Button** = Return to map (if Cancel Action is assigned)
- **Start Button** = Return to map (via SceneConnectionManager)

## Multiple Close Buttons?

If you have multiple close buttons (e.g., pause menu, inventory, etc.):

1. Add **Return To Map Button** component to each
2. They'll all work with the same inputs
3. Or disable certain inputs per button:
   ```
   Pause Menu Close Button:
   - Enable Escape Key: ?
   - Enable Gamepad Input: ?
   
   Main UI Close Button:
   - Enable Escape Key: ?
   - Enable Gamepad Input: ?
   ```

## Example Hierarchy

```
YourPointClickScene
??? SceneConnectionManager (Empty GameObject)
??? Canvas
?   ??? CloseButton
?   ?   ??? Button (component)
?   ?   ??? ReturnToMapButton (component) ? Add this!
?   ??? OtherUI...
??? GameObjects...
```

## Scene Connection Data Setup

Make sure you have a **SceneConnectionData** asset:

1. Right-click in Project ? Create ? Map System ? Scene Connection Data
2. Add scene connections:
   - **Scene Name**: "YourPointClickScene"
   - **Return Map Scene**: "MapScene"
   - **Unlocked Areas**: (areas to unlock after visiting)
3. Assign this asset to **SceneConnectionManager**

## Testing

1. **Play your Point & Click scene**
2. **Test inputs:**
   - Press **ESC** ? Should return to map
   - Click **Close Button** ? Should return to map
   - Press **B on gamepad** ? Should return to map (if configured)
3. **Check console:**
   - Should see: `[SceneConnectionManager] Returning to map: MapScene`

## Troubleshooting

### Button Does Nothing
- Check SceneConnectionManager is in the scene
- Check Scene Connection Data is assigned
- Check "Return Map Scene" is set correctly

### ESC Key Not Working
- Check "Enable Escape Key" is checked
- Make sure no other script is consuming ESC input

### Gamepad Not Working
- Assign **Cancel Action** in Inspector
- Make sure Input Action is enabled
- Check Input Action has gamepad binding

### "No connection data" Error
- Create a Scene Connection Data asset
- Assign it to SceneConnectionManager
- Add your scene to the connections list

## Advanced: Custom Return Logic

If you need custom logic before returning (e.g., save progress):

```csharp
using VisionsForetold.PointNClick;

public class CustomReturnButton : MonoBehaviour
{
    private ReturnToMapButton returnButton;
    
    void Start()
    {
        returnButton = GetComponent<ReturnToMapButton>();
        
        // Disable default return
        returnButton.enabled = false;
        
        // Add custom listener
        GetComponent<Button>().onClick.AddListener(OnCustomReturn);
    }
    
    void OnCustomReturn()
    {
        // Your custom logic here
        SaveProgress();
        
        // Then return to map
        returnButton.TriggerReturn();
    }
    
    void SaveProgress()
    {
        // Save game, etc.
    }
}
```

## Files Created
- ? `ReturnToMapButton.cs` - Button component for returning to map
- ? `SceneConnectionManager.cs` - Updated (removed Input Manager conflicts)

You're all set! ?????
