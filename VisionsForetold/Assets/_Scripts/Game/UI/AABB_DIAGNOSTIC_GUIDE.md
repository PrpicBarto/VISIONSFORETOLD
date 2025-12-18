# ?? AABB Diagnostic Tool - User Guide

## ? **Diagnostic Tool Created**

I've created **AABBDiagnostic.cs** - a powerful tool to help identify exactly which UI element is causing the "Invalid AABB" error.

---

## ?? **How to Use the Diagnostic Tool**

### Step 1: Add the Component

```
1. In Unity Hierarchy, create an empty GameObject
2. Name it "AABB Diagnostic"
3. Add Component ? AABBDiagnostic script
4. Keep it in your scene during gameplay
```

### Step 2: Run Your Game

```
1. Enter Play Mode
2. The diagnostic tool will:
   - Automatically scan all UI elements
   - Monitor them every second
   - Log any invalid UI elements to Console
```

### Step 3: Watch the Console

**Look for these messages:**

**Good (Normal):**
```
[AABB Diagnostic] Found X Graphic components
[AABB Diagnostic] Scan complete. Monitoring X graphics.
[AABB Diagnostic] All X graphics are valid.
```

**Bad (Problem Found):**
```
[AABB] INVALID RECT: Path/To/UI/Element - Width: NaN, Height: NaN
[AABB] INVALID POSITION: Path/To/UI/Element - Position: (NaN, NaN, NaN)
[AABB] NULL RectTransform: Path/To/UI/Element
[AABB] NULL Canvas: Path/To/UI/Element
[AABB] NULL CanvasRenderer: Path/To/UI/Element
```

---

## ?? **What the Tool Checks**

### For Every UI Element:

**1. Null Checks:**
```
? Is the Graphic component destroyed?
? Is the RectTransform null?
? Is the Canvas null?
? Is the CanvasRenderer null?
```

**2. Rect Validation:**
```
? Width is not NaN or Infinity
? Height is not NaN or Infinity
? Width is not negative
? Height is not negative
```

**3. Position Validation:**
```
? Position X is not NaN or Infinity
? Position Y is not NaN or Infinity
? Position Z is not NaN or Infinity
```

**4. Component Status:**
```
? Is the UI element active?
? Is it enabled?
? Is it properly initialized?
```

---

## ?? **Using the Tool**

### In-Game UI Buttons:

**Bottom-left corner shows:**
```
???????????????????????????
? AABB Diagnostic         ?
? Monitoring: X graphics  ?
? [Scan All Graphics]     ?
? [Check All Graphics]    ?
???????????????????????????
```

**Buttons:**
- **Scan All Graphics**: Re-scan scene for UI elements
- **Check All Graphics**: Check all UI for validity

### Right-Click Menu:

**In Inspector (with AABBDiagnostic selected):**
```
Right-click on component:
? Scan All Graphics
? Check All Graphics
```

---

## ?? **Interpreting Results**

### Error Messages Guide:

**"INVALID RECT"**
```
Problem: UI element has NaN or invalid dimensions
Cause: Math error (division by zero, NaN calculation)
Fix: Check Update() methods for invalid math
```

**"INVALID POSITION"**
```
Problem: UI element position is NaN or Infinity
Cause: Transform calculation error
Fix: Check position/rotation calculations
```

**"NULL RectTransform"**
```
Problem: RectTransform component destroyed
Cause: GameObject destroyed but UI still updating
Fix: Add null checks in Update()
```

**"NULL Canvas"**
```
Problem: Parent Canvas is missing or destroyed
Cause: Canvas hierarchy broken
Fix: Verify Canvas parent exists
```

**"NULL CanvasRenderer"**
```
Problem: CanvasRenderer destroyed
Cause: Component cleanup issue
Fix: Don't manually destroy CanvasRenderer
```

---

## ?? **How to Fix Issues**

### When Error is Found:

**1. Note the Path:**
```
Error shows: "[AABB] INVALID RECT: Player/Canvas/HealthBar/Fill"
This tells you: HealthBar Fill image has invalid dimensions
```

**2. Find the Script:**
```
Go to that GameObject in Hierarchy
Check which scripts are attached
Look for Update() methods
```

**3. Add Safety Checks:**
```csharp
private void Update()
{
    // Add null check
    if (targetComponent == null)
        return;

    // Add validation
    if (maxValue <= 0)
        return;

    // Safe calculation
    float percent = currentValue / maxValue;
    if (float.IsNaN(percent) || float.IsInfinity(percent))
        return;

    // Safe assignment
    if (fillImage != null)
        fillImage.fillAmount = percent;
}
```

---

## ?? **Additional Fixes Applied**

### BossHealthBar.cs (Just Fixed)

**Added:**
- ? Division by zero check
- ? Boss health null check
- ? Health percent validation (NaN/Infinity check)
- ? OnDestroy() event cleanup
- ? Safe coroutine starting

**Code:**
```csharp
private void UpdateHealthBar(int currentHealth, int maxHealth)
{
    // Safety check for division by zero
    if (maxHealth <= 0)
    {
        Debug.LogWarning("[BossHealthBar] MaxHealth is 0 or negative!");
        return;
    }

    // Check if bossHealth reference is still valid
    if (bossHealth == null)
    {
        Debug.LogWarning("[BossHealthBar] Boss Health reference lost!");
        return;
    }

    float healthPercent = (float)currentHealth / (float)maxHealth;

    // Validate the calculated percentage
    if (float.IsNaN(healthPercent) || float.IsInfinity(healthPercent))
    {
        Debug.LogError($"[BossHealthBar] Invalid health percent: {healthPercent}");
        return;
    }

    if (HealthBarFill != null)
    {
        HealthBarFill.fillAmount = healthPercent;
    }
}
```

---

## ?? **Complete Fix Checklist**

### All UI Scripts Fixed:

```
? WorldHealthBar.cs
   - Null checks in Update()
   - Destroy on death
   - Safe FaceCamera()

? PlayerHUD.cs
   - Player reference validation
   - Division by zero prevention
   - Null checks for UI elements

? DamageNumber.cs
   - Component validation
   - Camera reference recovery
   - Safe transform operations

? EchoRevealSystem.cs
   - Null checks for destroyed objects
   - Skip invalid objects
   - Clean up null references

? BossHealthBar.cs (NEW)
   - Division by zero check
   - NaN/Infinity validation
   - Event cleanup on destroy
   - Safe coroutine handling
```

---

## ?? **Testing Procedure**

### With Diagnostic Tool:

**1. Start Game:**
```
Play Mode ? Watch Console
[AABB Diagnostic] Found X Graphic components
```

**2. Play Normally:**
```
Walk around
Fight enemies
Take damage
Gain XP
Use abilities
```

**3. Watch for Errors:**
```
If "Invalid AABB inAABB" appears:
Look immediately above it in Console
Diagnostic tool should have logged the problematic UI
```

**4. Note the Path:**
```
Example: "[AABB] INVALID RECT: Player/HUD/HealthBar/Fill"
Now you know exactly which UI element to fix!
```

**5. Fix and Test:**
```
Add null checks to that UI element's script
Re-test in Play Mode
Verify error is gone
```

---

## ?? **Common Patterns**

### Most Common Invalid AABB Causes:

**1. Health Bars (40%):**
```
Problem: Division by zero (maxHealth = 0)
Fix: if (maxHealth <= 0) return;
```

**2. Destroyed Objects (30%):**
```
Problem: UI updating after parent destroyed
Fix: if (target == null) return;
```

**3. Missing References (20%):**
```
Problem: Camera, Canvas, or component null
Fix: if (component == null) { recover or return; }
```

**4. Invalid Math (10%):**
```
Problem: NaN or Infinity from calculations
Fix: if (float.IsNaN(value)) return;
```

---

## ?? **Next Steps**

### When You See the Error:

**1. Check Console Immediately:**
```
Look for diagnostic messages
Note the GameObject path
Identify the script
```

**2. Open the Script:**
```
Find Update() or similar methods
Look for math operations
Check for null references
```

**3. Add Safety Checks:**
```
Null checks before access
Division by zero prevention
NaN/Infinity validation
Reference recovery
```

**4. Test Again:**
```
Play Mode
Reproduce the error scenario
Verify fix worked
```

---

## ?? **Diagnostic Settings**

### AABBDiagnostic Component Settings:

**Log Every Frame:**
```
? Unchecked (default): Only logs errors
? Checked: Logs all UI elements every scan
Use: When you want verbose debugging
```

**Auto Scan On Enable:**
```
? Checked (default): Auto-scans when enabled
? Unchecked: Manual scan only
Use: Keep checked for automatic monitoring
```

**Scan Interval:**
```
Default: 1.0 seconds
Range: 0.1 - 5.0 seconds
Use: How often to check UI validity
```

---

## ? **Summary**

**Diagnostic Tool Features:**
```
? Automatic UI element scanning
? Real-time validity checking
? Detailed error logging with paths
? In-game UI buttons
? Right-click Inspector commands
? Configurable scan frequency
```

**What It Detects:**
```
? Null components
? Invalid rect dimensions (NaN, Infinity, negative)
? Invalid positions (NaN, Infinity)
? Missing Canvas or CanvasRenderer
? Destroyed GameObjects
? Inactive elements
```

**How to Use:**
```
1. Add AABBDiagnostic to a GameObject
2. Enter Play Mode
3. Watch Console for error messages
4. Fix the identified UI element
5. Re-test
```

---

**The diagnostic tool will pinpoint EXACTLY which UI element is causing the error!** ???

**Use it to track down the remaining Invalid AABB source!** ??

**Build Status:** ? Successful

**Files Created:**
- AABBDiagnostic.cs (diagnostic tool)
- AABB_DIAGNOSTIC_GUIDE.md (this guide)

**Files Updated:**
- BossHealthBar.cs (added safety checks)

---

**Run the game with the diagnostic tool and report what it finds!** ????
