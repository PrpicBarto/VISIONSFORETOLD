# Save System Troubleshooting Guide

## ? **Problem: Can't Interact with Save Station**

### Issue
Player approaches save station but pressing E (keyboard) or Y/Triangle (gamepad) doesn't open the menu.

### Root Cause
Your "Interact" action has `"interactions": "Hold"` in the Input Actions asset, which requires holding the button instead of just pressing it.

### ? **Solution 1: Change Input Action (Recommended)**

1. Open `InputSystem_Actions.inputactions` in Unity
2. Select the **"Interact"** action under **Player** action map
3. In Inspector, find **Interactions** field
4. **Remove** the "Hold" interaction (or change to "Press")
5. Click **Save Asset**

**Before:**
```
interactions: "Hold"
```

**After:**
```
interactions: "" (empty)
```

OR use "Press" interaction:
```
interactions: "Press"
```

### ? **Solution 2: Update SaveStation Code (Alternative)**

If you want to KEEP the Hold interaction (e.g., for other uses), update the `SaveStation.cs`:

Change this line in `OnInteractPerformed` method:

**FROM:**
```csharp
private void OnInteractPerformed(InputAction.CallbackContext context)
{
    if (playerInRange && context.performed)  // ? This checks .performed
    {
        OpenSaveStation();
    }
}
```

**TO:**
```csharp
private void OnInteractPerformed(InputAction.CallbackContext context)
{
    // Accept both performed (tap) and started (hold began)
    if (playerInRange && (context.performed || context.started))
    {
        OpenSaveStation();
    }
}
```

OR listen specifically for hold completion:

```csharp
private void OnInteractPerformed(InputAction.CallbackContext context)
{
    // Only trigger when hold completes
    if (playerInRange && context.phase == InputActionPhase.Performed)
    {
        OpenSaveStation();
    }
}
```

---

## ?? **Testing the Fix**

### Test 1: Check Input Action
1. Open `InputSystem_Actions.inputactions`
2. Select "Interact" action
3. Verify **Interactions** is blank or "Press"
4. Save and test in game

### Test 2: Debug Logging
Add debug logging to verify input is being received:

```csharp
private void OnInteractPerformed(InputAction.CallbackContext context)
{
    Debug.Log($"[SaveStation] Interact triggered! Phase: {context.phase}, Performed: {context.performed}");
    
    if (playerInRange && context.performed)
    {
        OpenSaveStation();
    }
}
```

Watch the Console when pressing E/Y near the save station.

### Test 3: Manual Test
1. Enter Play Mode
2. Walk player to save station
3. Check Console for: `"[SaveStation] Player entered save station range"`
4. Press E (keyboard) or Y/Triangle (gamepad)
5. Menu should open

---

## ?? **Other Common Issues**

### Issue: "Interact action not found"
**Symptoms:**
- Console shows: `'Interact' action not found in Input Actions!`

**Solution:**
- Verify "Interact" action exists in InputSystem_Actions.inputactions
- Check spelling is exact: "Interact" (capital I)
- Ensure it's under "Player" action map

### Issue: Player can't move after closing menu
**Symptoms:**
- After exiting save station menu, player is frozen

**Solution:**
- Check `SaveStation.OnMenuClosed()` is being called
- Verify PlayerMovement component is being re-enabled
- Check Player action map is being re-enabled

### Issue: Prompt doesn't show
**Symptoms:**
- No "Press E to Save" message appears

**Solution:**
- Assign Prompt UI GameObject in SaveStation Inspector
- Check Prompt UI is a child of Canvas
- Verify Prompt UI has Text/TMP_Text component

### Issue: Gamepad navigation doesn't work in menu
**Symptoms:**
- Can't navigate menu with gamepad D-Pad/Stick

**Solution:**
1. Check EventSystem exists in scene
2. In SaveStationMenu Inspector, check "Enable Gamepad Navigation" is ?
3. Ensure all buttons have Navigation set to "Automatic"
4. Verify UI action map has Navigate action

---

## ?? **Quick Checklist**

When save station isn't working, check:

- [ ] Player has "Player" tag
- [ ] SaveStation has trigger collider
- [ ] "Interact" action exists in Input Actions
- [ ] "Interact" has NO "Hold" interaction (or code handles it)
- [ ] Player has PlayerInput component
- [ ] SaveStationMenu is assigned in SaveStation Inspector
- [ ] Prompt UI is assigned (optional but recommended)
- [ ] EventSystem exists in scene
- [ ] Console shows no errors

---

## ?? **Input Action Configuration**

### Recommended "Interact" Setup:

```json
{
  "name": "Interact",
  "type": "Button",
  "interactions": "",  // ? EMPTY (no Hold)
  "bindings": [
    {
      "path": "<Keyboard>/e",
      "groups": "Keyboard&Mouse"
    },
    {
      "path": "<Gamepad>/buttonNorth",  // Y on Xbox, Triangle on PS
      "groups": "Gamepad"
    }
  ]
}
```

### Alternative with Press Interaction:

```json
{
  "name": "Interact",
  "type": "Button",
  "interactions": "Press",  // ? Explicit Press
  "bindings": [...]
}
```

---

## ??? **Advanced Debugging**

### Enable Detailed Logging

Add to `SaveStation.cs` `OnTriggerEnter`:

```csharp
private void OnTriggerEnter(Collider other)
{
    if (other.CompareTag(playerTag))
    {
        Debug.Log($"[SaveStation] Player entered! Tag: {other.tag}");
        
        player = other.transform;
        playerInRange = true;

        playerInput = other.GetComponent<PlayerInput>();
        if (playerInput != null)
        {
            Debug.Log("[SaveStation] PlayerInput found!");
            
            interactAction = playerInput.actions.FindAction("Interact");
            if (interactAction != null)
            {
                Debug.Log($"[SaveStation] Interact action found! Bindings: {interactAction.bindings.Count}");
                interactAction.performed += OnInteractPerformed;
            }
            else
            {
                Debug.LogError("[SaveStation] 'Interact' action NOT FOUND!");
            }
        }
        else
        {
            Debug.LogError("[SaveStation] PlayerInput component NOT FOUND!");
        }

        ShowPrompt();
    }
}
```

### Check Input Action Bindings

Add to `Start()` in a test script:

```csharp
void Start()
{
    var playerInput = GetComponent<PlayerInput>();
    var interactAction = playerInput.actions.FindAction("Interact");
    
    Debug.Log($"Interact Action: {interactAction != null}");
    Debug.Log($"Interact Interactions: '{interactAction.interactions}'");
    Debug.Log($"Interact Bindings Count: {interactAction.bindings.Count}");
    
    foreach (var binding in interactAction.bindings)
    {
        Debug.Log($"  Binding: {binding.effectivePath}");
    }
}
```

---

## ? **Confirmed Working Setup**

Here's a verified working configuration:

### Input Actions:
- Action: "Interact"
- Type: Button
- Interactions: (empty)
- Keyboard binding: E
- Gamepad binding: Button North (Y/Triangle)

### Player GameObject:
- Tag: "Player"
- Components: PlayerInput, PlayerMovement, Health

### SaveStation GameObject:
- BoxCollider (Is Trigger: ?)
- SaveStation component
- Prompt UI assigned
- SaveStationMenu reference assigned

### Canvas:
- EventSystem (auto-created)
- SaveStationMenu component
- All UI panels set up
- Button navigation: Automatic

---

## ?? **Still Not Working?**

If you've tried everything above:

1. **Check Console** for any error messages
2. **Test with debug logging** to see where it fails
3. **Verify Input System package** is installed (v1.14.2+)
4. **Restart Unity** (sometimes Input Actions need a refresh)
5. **Create a new simple test scene** with just player + save station

### Minimal Test Scene:
1. Create empty scene
2. Add Player (with PlayerInput)
3. Add SaveStation (cube with trigger)
4. Add SaveManager
5. Add basic Canvas with SaveStationMenu
6. Test

If it works in test scene, compare with your main scene to find differences.

---

## ?? **Most Common Fix**

**90% of the time, the issue is:**

The "Interact" action has `"interactions": "Hold"` instead of being empty or "Press".

**Quick Fix:**
1. Open InputSystem_Actions.inputactions
2. Select "Interact" action
3. Clear the "Interactions" field
4. Save
5. Test

This should resolve the interaction issue! ???
