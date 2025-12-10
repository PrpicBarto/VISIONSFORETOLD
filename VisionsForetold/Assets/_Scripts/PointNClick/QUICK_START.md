# ?? Quick Start: Close Button ? Return to Map

## 1. Setup Scene Manager (Do Once Per Scene)

**Create the manager:**
```
1. Right-click in Hierarchy ? Create Empty
2. Name: "SceneConnectionManager"
3. Add Component ? "Scene Connection Manager"
4. Assign your Scene Connection Data asset
```

## 2. Make Close Button Return to Map

**For EXISTING close buttons:**
```
1. Select your Close Button
2. Add Component ? "Return To Map Button"
3. Done! It now works with:
   - Mouse clicks ?
   - ESC key ?
   - Gamepad B/Circle (if you set up Input Action)
```

**Button settings in Inspector:**
```
Return To Map Button Component:
?? Enable Escape Key: ? (ESC works)
?? Enable Gamepad Input: ? (Controller works)
?? Cancel Action: [Optional - assign Input Action]
?? Return Sound: [Optional - audio clip]
?? Show Debug: ? (for testing)
```

## 3. Gamepad Support (Optional but Recommended)

**Setup Input Action:**
```
1. Find: InputSystem_Actions asset
2. Add Action: "Cancel" or "UI Cancel"
3. Binding ? Gamepad ? B Button (Xbox) / Circle (PS)
4. Save
5. Drag action into Close Button ? Cancel Action slot
```

**OR use existing UI Cancel action:**
```
Most Input Action assets already have UI/Cancel
Just assign that to your button!
```

## Controls Summary

| Input | Action |
|-------|--------|
| **Mouse** | Click Close Button |
| **Keyboard** | ESC key |
| **Gamepad** | B / Circle Button |
| **Keyboard** | M key (via SceneConnectionManager) |

## Visual Example

### Before:
```
[Close Button]
?? Button (component)
   ?? OnClick() ? (nothing)
```

### After:
```
[Close Button]
?? Button (component)
?? ReturnToMapButton (component) ? NEW!
   ?? Enable Escape Key: ?
   ?? Enable Gamepad Input: ?
   ?? Cancel Action: UI/Cancel
```

## Test It

1. **Play the scene**
2. **Try each input:**
   - Click the button
   - Press ESC
   - Press B on controller
3. **Should load MapScene**

## Common Issues

**"No SceneConnectionManager found"**
? Add SceneConnectionManager GameObject to scene

**ESC does nothing**
? Check "Enable Escape Key" is ticked

**Gamepad does nothing**  
? Assign Cancel Action from Input Actions

**Wrong scene loads**
? Check Scene Connection Data ? Return Map Scene

## That's It!

Your close buttons now work with mouse, keyboard, AND gamepad! ??

For full details, see: `RETURN_TO_MAP_SETUP.md`
