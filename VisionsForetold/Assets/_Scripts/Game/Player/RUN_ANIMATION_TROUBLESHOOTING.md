# ?? Run Animation Troubleshooting Guide

## Problem: Run Animation Doesn't Play

Your code is set up correctly, but the run animation still isn't playing. Let's diagnose the issue step by step.

---

## ?? **Step 1: Check Animator Parameter**

### Verify IsRunning Parameter Exists

**In Unity:**
```
1. Select your Player GameObject
2. Open Animator window (Ctrl + 9)
3. Look at Parameters tab (left side)
4. Check if "IsRunning" exists as a Bool parameter
```

**If IsRunning is missing:**
```
Click "+" button ? Bool ? Name it "IsRunning"
```

**If IsRunning exists but is a different type:**
```
Delete it and recreate as Bool
```

---

## ?? **Step 2: Check Animation State Exists**

### Verify Run State is Created

**In Animator window:**
```
1. Look for a state called "Run"
2. If it doesn't exist, right-click ? Create State ? Empty
3. Rename it to "Run"
4. Assign your run animation clip to it
```

**To assign animation:**
```
1. Click "Run" state
2. Inspector ? Motion field
3. Drag your run.fbx animation clip here
```

---

## ?? **Step 3: Check Transitions Exist**

### Verify Walk ? Run Transitions

**You need TWO transitions:**

**Walk ? Run:**
```
1. Right-click Walk state
2. Make Transition
3. Click Run state
4. Click the transition arrow
5. Inspector settings:
   - Conditions: + IsRunning ? true
   - Has Exit Time: ? (unchecked!)
   - Transition Duration: 0.15
```

**Run ? Walk:**
```
1. Right-click Run state
2. Make Transition
3. Click Walk state
4. Click the transition arrow
5. Inspector settings:
   - Conditions: + IsRunning ? false
   - Has Exit Time: ? (unchecked!)
   - Transition Duration: 0.15
```

**Common Mistake:** Having only ONE transition instead of both!

---

## ?? **Step 4: Test Speed Values**

### Add Debug Logging

**Temporarily add this to PlayerMovement.cs UpdateAnimations():**

```csharp
private void UpdateAnimations()
{
    if (animator == null) return;

    bool isMoving = movementInput.magnitude > 0.1f && !isDodging;
    float targetSpeed = isMoving ? movementInput.magnitude : 0f;
    
    if (isSprinting)
    {
        targetSpeed *= sprintSpeedMultiplier;
    }

    currentAnimationSpeed = Mathf.Lerp(currentAnimationSpeed, targetSpeed, Time.deltaTime / animationSmoothTime);
    bool isRunning = isMoving && currentAnimationSpeed > 1.2f;

    // ADD THIS DEBUG CODE
    if (isSprinting)
    {
        Debug.Log($"Sprint Active! Speed: {currentAnimationSpeed:F2}, IsRunning: {isRunning}, TargetSpeed: {targetSpeed:F2}");
    }
    
    // Rest of your code...
    animator.SetBool(IsMovingHash, isMoving);
    animator.SetFloat(SpeedHash, currentAnimationSpeed);
    animator.SetBool(IsSprintingHash, isSprinting);
    animator.SetBool(IsRunningHash, isRunning);
    
    // ... rest
}
```

**Test and watch Console:**
```
1. Enter Play Mode
2. Hold Shift + W (sprint forward)
3. Watch Console output
4. Check if Speed reaches > 1.2
```

**Expected Output:**
```
Sprint Active! Speed: 0.85, IsRunning: False, TargetSpeed: 1.80
Sprint Active! Speed: 1.05, IsRunning: False, TargetSpeed: 1.80
Sprint Active! Speed: 1.25, IsRunning: True, TargetSpeed: 1.80  ? Should see True!
```

---

## ?? **Step 5: Check Sprint Settings**

### Verify Sprint is Working

**Check PlayerMovement Inspector:**
```
Sprint Settings:
? Enable Sprint: ? (must be checked!)
? Sprint Speed Multiplier: 1.8 (or higher)
? Require Stamina: Check if you have stamina
? Min Stamina To Sprint: Lower if needed
```

**Test Sprint:**
```
1. Enter Play Mode
2. Press and HOLD Shift
3. Press W to move forward
4. Character should move faster
```

**If sprint doesn't work at all:**
```
- Check Input Actions: "Sprint" action exists
- Check key binding: Left Shift bound to Sprint
- Check stamina: Enough stamina available
```

---

## ?? **Step 6: Check Animator State Machine**

### Watch Animator in Play Mode

**While playing:**
```
1. Keep Animator window open
2. Hold Shift + W
3. Watch which state is active (blue highlight)
4. Does it stay on Walk? ? Transition issue
5. Does it go to Run? ? Animation clip issue
```

**If stuck on Walk:**
```
Problem: Transition not triggering
Solution: Check Step 3 (transitions)
```

**If Run state is active but no animation:**
```
Problem: No animation clip assigned
Solution: Check Step 2 (assign run.fbx)
```

---

## ?? **Step 7: Check Animation Clip**

### Verify Run Animation Import

**Select your run.fbx:**
```
Inspector ? Animation tab:
? Loop Time: ? (must be checked!)
? Loop Pose: ?
? Root Transform Rotation: Bake Into Pose ?
? Root Transform Position (Y): Bake Into Pose ?
? Root Transform Position (XZ): Bake Into Pose ?
? Click Apply!
```

**If animation is too short/long:**
```
Animation tab:
- Start: 0 (or adjust)
- End: 60 (or adjust to your clip length)
- Apply
```

---

## ?? **Step 8: Common Issues & Fixes**

### Issue 1: Speed Never Reaches 1.2

**Symptom:** Debug shows Speed stuck at 1.0 or below

**Cause:** Not actually sprinting or multiplier too low

**Fix:**
```csharp
// Option A: Lower the run threshold
bool isRunning = isMoving && currentAnimationSpeed > 0.9f; // Lower threshold

// Option B: Increase sprint multiplier
sprintSpeedMultiplier = 2.0f; // In Inspector
```

---

### Issue 2: Speed Lerps Too Slowly

**Symptom:** Debug shows Speed increasing very slowly

**Cause:** animationSmoothTime too high

**Fix:**
```
PlayerMovement Inspector:
Animation Settings ? Animation Smooth Time: 0.05 (lower = faster)
```

---

### Issue 3: IsRunning Bool Not Updating

**Symptom:** Console shows IsRunning always False

**Cause:** Animator parameter not linked

**Fix:**
```
1. Delete IsRunning parameter from Animator
2. Recreate it (+ ? Bool ? "IsRunning")
3. Make sure spelling matches exactly: "IsRunning"
```

---

### Issue 4: Walk ? Run Transition Has Exit Time

**Symptom:** Delay before run starts

**Cause:** "Has Exit Time" is checked

**Fix:**
```
Walk ? Run transition:
- Has Exit Time: ? (MUST BE UNCHECKED!)
- Transition Duration: 0.15
```

---

### Issue 5: Using Speed Parameter Instead of IsRunning

**If you want to use Speed instead of IsRunning bool:**

**Option A: Use Speed with threshold:**
```
Walk ? Run transition:
- Condition: Speed ? Greater ? 1.2
- Has Exit Time: ?
```

**Option B: Use Blend Tree (recommended):**
```
1. Create 1D Blend Tree
2. Parameter: Speed
3. Add motions:
   - 0.0: Idle
   - 1.0: Walk
   - 1.2: Run
4. Automatic blending between animations
```

---

## ?? **Quick Diagnostic Checklist**

**Run through this list:**

```
Animator Setup:
? IsRunning Bool parameter exists
? Run state exists with animation assigned
? Walk ? Run transition exists
? Run ? Walk transition exists
? Both transitions have Has Exit Time: ?
? Walk ? Run condition: IsRunning == true
? Run ? Walk condition: IsRunning == false

Sprint Settings:
? Enable Sprint: ?
? Sprint Speed Multiplier: 1.8 or higher
? Enough stamina to sprint
? Sprint key binding works (Shift)

Animation Settings:
? Animation Smooth Time: 0.1 or lower
? Run animation clip assigned
? Run animation Loop Time: ?
? Animation imported correctly

Code Check:
? UpdateAnimations sets IsRunning correctly
? Threshold is 1.2 (or lower if needed)
? No errors in Console
```

---

## ?? **Step-by-Step Fix Process**

### Fix #1: Ensure Parameter Exists

**Do this first:**
```
1. Open Animator window
2. Parameters tab ? Look for "IsRunning"
3. If missing: + ? Bool ? "IsRunning"
4. Save scene
5. Test again
```

---

### Fix #2: Create Proper Transitions

**If transitions missing or wrong:**
```
1. Delete any existing Walk ? Run transitions
2. Right-click Walk ? Make Transition ? Run
3. Set: IsRunning == true, Has Exit Time: ?
4. Right-click Run ? Make Transition ? Walk
5. Set: IsRunning == false, Has Exit Time: ?
6. Save and test
```

---

### Fix #3: Lower Run Threshold (Temporary Test)

**To quickly test if threshold is the issue:**

**In PlayerMovement.cs line 724:**
```csharp
// Change from:
bool isRunning = isMoving && currentAnimationSpeed > 1.2f;

// To (temporarily):
bool isRunning = isMoving && currentAnimationSpeed > 0.5f;
```

**Test:**
```
1. Sprint forward
2. Run should trigger almost immediately
3. If it works: Threshold was too high
4. Adjust back to comfortable value (0.8 - 1.0)
```

---

### Fix #4: Use Alternative Approach - Speed Parameter

**If IsRunning bool still doesn't work, use Speed directly:**

**In Animator:**
```
1. Walk ? Run transition
2. Remove IsRunning condition
3. Add condition: Speed ? Greater ? 1.2
4. Run ? Walk transition
5. Add condition: Speed ? Less ? 1.2
```

**This uses the Speed float parameter directly instead of IsRunning bool.**

---

## ?? **Testing Procedure**

**Follow these exact steps:**

```
1. Enter Play Mode
2. Press W (walk forward) ? Walk animation plays
3. Keep holding W
4. Press and HOLD Shift
5. Character speeds up
6. After ~0.5 seconds ? Run animation should play
7. Release Shift
8. After ~0.5 seconds ? Walk animation returns
```

**What to watch:**
```
Console (if debug enabled):
- Speed should increase above 1.2
- IsRunning should become True

Animator window:
- Walk state should be blue (active)
- After sprinting, Run state should turn blue
- On releasing sprint, Walk state returns
```

---

## ?? **Most Likely Causes**

Based on common issues:

**1. IsRunning parameter doesn't exist (50% of cases)**
```
Fix: Add Bool parameter "IsRunning" in Animator
```

**2. No Walk ? Run transition (30% of cases)**
```
Fix: Create transition with IsRunning == true condition
```

**3. Has Exit Time is checked (15% of cases)**
```
Fix: Uncheck "Has Exit Time" on Walk ? Run transition
```

**4. Sprint not working (5% of cases)**
```
Fix: Check sprint settings, stamina, key binding
```

---

## ?? **Advanced Debugging**

### Watch Parameter Values in Real-Time

**In Animator window (Play Mode):**
```
1. Click Parameters tab
2. Sprint forward
3. Watch these values change:
   - Speed: Should go from ~1.0 to ~1.8
   - IsRunning: Should change to True when Speed > 1.2
   - IsMoving: Should be True
   - IsSprinting: Should be True
```

**If IsRunning stays False:**
- Speed not reaching 1.2
- Threshold too high
- Code not executing

**If IsRunning becomes True but no animation:**
- No Run state
- No Walk ? Run transition
- No animation clip assigned

---

## Summary

**Most Common Fix:**
```
1. Add "IsRunning" Bool parameter to Animator ?
2. Create Walk ? Run transition (IsRunning == true) ?
3. Create Run ? Walk transition (IsRunning == false) ?
4. Uncheck "Has Exit Time" on both transitions ?
5. Assign run animation clip to Run state ?
```

**Test:**
```
Hold Shift + W ? Should see run animation!
```

**If still not working, add debug logging to see actual Speed values and adjust threshold accordingly.**

**Your run animation will work!** ?????
