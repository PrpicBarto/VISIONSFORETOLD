# ?? Fix: AttackBow Animation Only Plays Once

## The Problem
The AttackBow animation plays the first time you shoot, but subsequent shots don't trigger the animation.

## Root Cause
This is a **transition configuration issue** in your Animator Controller, not a code issue. The trigger is being consumed but the transition isn't set up to allow re-triggering.

---

## ? **Solution 1: Fix Animator Transitions (Most Common)**

### Check Your Transition Settings:

**Any State ? AttackBow:**

```
1. Open Animator window (Window ? Animation ? Animator)
2. Find transition: Any State ? AttackBow
3. Click the transition arrow
4. Inspector settings:

CRITICAL SETTINGS:
?? Has Exit Time: ? ? MUST BE UNCHECKED!
?? Can Transition To Self: ? ? This prevents re-triggering
?? Conditions: AttackBow (trigger)

If "Can Transition To Self" is checked:
? Uncheck it for Any State transition
```

**AttackBow ? Idle (or previous state):**

```
1. Find transition: AttackBow ? Idle (or Walk/Run)
2. Click the transition arrow
3. Inspector settings:

REQUIRED SETTINGS:
?? Has Exit Time: ? ? Should be checked
?? Exit Time: 0.85 - 0.95 ? Near end of animation
?? Transition Duration: 0.15 - 0.2
?? No conditions needed (exits automatically)

This allows animation to play fully then return to idle
```

---

## ? **Solution 2: Create State-Specific Transitions**

**Instead of using Any State, create explicit transitions:**

### From Idle:
```
Idle ? AttackBow:
?? Has Exit Time: ?
?? Transition Duration: 0.05
?? Conditions: AttackBow (trigger)
?? Can Transition To Self: not applicable
```

### From Walk:
```
Walk ? AttackBow:
?? Has Exit Time: ?
?? Transition Duration: 0.05
?? Conditions: AttackBow (trigger)
?? Can Transition To Self: not applicable
```

### From Run:
```
Run ? AttackBow:
?? Has Exit Time: ?
?? Transition Duration: 0.05
?? Conditions: AttackBow (trigger)
?? Can Transition To Self: not applicable
```

### Return Transition:
```
AttackBow ? Idle:
?? Has Exit Time: ?
?? Exit Time: 0.9
?? Transition Duration: 0.15
```

**Benefits:**
- More control over when attack can happen
- Prevents re-triggering issues
- Cleaner state machine

---

## ? **Solution 3: Reset Trigger Manually (Code Fix)**

**If transitions are correct but still having issues, manually reset trigger:**

Add to `PlayerMovement.cs`:

```csharp
public void TriggerAttackBow()
{
    if (animator == null) return;
    
    if (animator.runtimeAnimatorController == null)
    {
        Debug.LogWarning("[PlayerMovement] No AnimatorController assigned!");
        return;
    }
    
    if (!isDodging)
    {
        // Reset trigger first (in case it's still set)
        animator.ResetTrigger(AttackBowHash);
        
        // Then set it
        animator.SetTrigger(AttackBowHash);
        Debug.Log("[PlayerMovement] Triggered AttackBow animation");
    }
}
```

**Why this works:**
- ResetTrigger clears any lingering trigger state
- Ensures clean trigger each time
- Prevents Unity's trigger system from getting "stuck"

---

## ? **Solution 4: Use Animation Events to Reset**

**Add event at end of AttackBow animation:**

```
1. Window ? Animation ? Animation
2. Select AttackBow animation clip
3. Scrub to last frame (end of animation)
4. Add Event
5. Function: "OnAttackBowComplete"
```

**Add to PlayerMovement.cs:**

```csharp
// Called by animation event at end of bow attack
public void OnAttackBowComplete()
{
    Debug.Log("[PlayerMovement] AttackBow animation completed");
    
    // Reset trigger to ensure it can fire again
    if (animator != null)
    {
        animator.ResetTrigger(AttackBowHash);
    }
}
```

---

## ? **Solution 5: Check Transition Priority**

**If you have multiple transitions from AttackBow:**

```
AttackBow state has multiple exit transitions:
?? AttackBow ? Idle (Priority 0)
?? AttackBow ? Walk (Priority 1)
?? AttackBow ? AttackBow (if Can Transition To Self)

Make sure:
1. Only ONE transition should be active at a time
2. No circular transitions
3. Clear priority order
```

**Fix:**
```
1. Select AttackBow state
2. Inspector ? Transitions section
3. Reorder priorities (drag transitions)
4. Ensure clean exit path back to locomotion
```

---

## ?? **Step-by-Step Diagnostic**

### Test 1: Check Current Transition

```
1. Open Animator window
2. Enter Play Mode
3. Switch to Ranged mode
4. Attack once (works)
5. Try attack again (doesn't work?)
6. Watch Animator window:
   - Does AttackBow state highlight blue?
   - Does it transition back to Idle?
   - Does it get stuck?
```

**If stuck in AttackBow state:**
? Missing or wrong exit transition

**If never enters AttackBow state after first time:**
? "Can Transition To Self" issue or trigger not resetting

---

### Test 2: Check Trigger in Animator

```
1. Play Mode
2. Animator window open
3. Watch Parameters tab (right side)
4. Click to shoot arrow
5. Watch AttackBow trigger:
   - Does it light up briefly? ? Good
   - Does it stay lit? ? Stuck
   - Does nothing happen? ? Trigger not firing
```

---

### Test 3: Check Console

```
Look for:
[PlayerMovement] Triggered AttackBow animation

If you see this message but animation doesn't play:
? Animator transition issue (not code issue)

If you DON'T see this message:
? Code/input issue (check PlayerAttack.cs)
```

---

## ?? **Complete Fix Checklist**

### Animator Settings:
```
? Any State ? AttackBow:
  ? Has Exit Time: ?
  ? Can Transition To Self: ?
  ? Transition Duration: 0.05
  ? Condition: AttackBow trigger

? AttackBow ? Idle (or movement):
  ? Has Exit Time: ?
  ? Exit Time: 0.9
  ? Transition Duration: 0.15
  ? No conditions

? No circular transitions (AttackBow ? AttackBow)
? Clean exit path back to locomotion
```

### Code Verification:
```
? PlayerAttack.PerformRangedAttack() calls TriggerAttackBow()
? TriggerAttackBow() calls animator.SetTrigger(AttackBowHash)
? No errors in console
? AttackBow trigger exists in Animator parameters
```

### Animation Settings:
```
? AttackBow animation clip assigned to state
? Loop Time: ? (attacks shouldn't loop)
? Animation plays correctly when manually tested
```

---

## ?? **Quick Fix Code (Try This First)**

Add this improved version to `PlayerMovement.cs`:

```csharp
/// <summary>
/// Trigger bow/ranged attack animation
/// Called from PlayerAttack script when performing ranged attack
/// </summary>
public void TriggerAttackBow()
{
    if (animator == null) return;
    
    if (animator.runtimeAnimatorController == null)
    {
        Debug.LogWarning("[PlayerMovement] No AnimatorController assigned!");
        return;
    }
    
    if (!isDodging)
    {
        // Reset trigger to ensure clean state
        animator.ResetTrigger(AttackBowHash);
        
        // Small delay to ensure reset registers
        StartCoroutine(TriggerAttackBowDelayed());
    }
}

private System.Collections.IEnumerator TriggerAttackBowDelayed()
{
    yield return null; // Wait one frame
    
    if (animator != null)
    {
        animator.SetTrigger(AttackBowHash);
        Debug.Log("[PlayerMovement] Triggered AttackBow animation");
    }
}
```

**This ensures trigger is completely reset before setting it again.**

---

## ?? **Recommended Animator Setup**

### Best Practice for Repeatable Attacks:

**Option A: Direct Transitions (Cleanest)**

```
Idle ? AttackBow (AttackBow trigger)
Walk ? AttackBow (AttackBow trigger)
Run ? AttackBow (AttackBow trigger)

AttackBow ? Idle (Has Exit Time: ?, Exit Time: 0.9)

NO "Any State" transition for attacks
```

**Option B: Any State with Proper Settings**

```
Any State ? AttackBow:
?? Has Exit Time: ?
?? Can Transition To Self: ?  ? CRITICAL!
?? Transition Duration: 0.05
?? Condition: AttackBow (trigger)

AttackBow ? Idle:
?? Has Exit Time: ?
?? Exit Time: 0.9
?? Transition Duration: 0.15
```

---

## ?? **Most Likely Causes (In Order)**

### 1. Can Transition To Self = ? (Most Common)
**Fix:** Uncheck "Can Transition To Self" on Any State ? AttackBow

### 2. Missing Exit Transition
**Fix:** Create AttackBow ? Idle transition with Has Exit Time ?

### 3. Exit Time Too Early
**Fix:** Set Exit Time to 0.85 - 0.95 (near end of animation)

### 4. Trigger Not Resetting
**Fix:** Add animator.ResetTrigger() before SetTrigger()

### 5. Animation Stuck in Loop
**Fix:** Uncheck Loop Time on AttackBow animation clip

---

## ?? **Debug Output**

Add this debug version temporarily:

```csharp
public void TriggerAttackBow()
{
    Debug.Log($"[PlayerMovement] TriggerAttackBow called - animator exists: {animator != null}");
    
    if (animator == null) return;
    
    Debug.Log($"[PlayerMovement] Has controller: {animator.runtimeAnimatorController != null}");
    Debug.Log($"[PlayerMovement] Is dodging: {isDodging}");
    
    if (animator.runtimeAnimatorController == null) return;
    
    if (!isDodging)
    {
        // Check current state
        var currentState = animator.GetCurrentAnimatorStateInfo(0);
        Debug.Log($"[PlayerMovement] Current state: {currentState.IsName("AttackBow")}");
        
        animator.ResetTrigger(AttackBowHash);
        animator.SetTrigger(AttackBowHash);
        
        Debug.Log("[PlayerMovement] Triggered AttackBow animation");
    }
}
```

Run and check console for clues.

---

## Summary

**Most Common Fix:**
```
Animator ? Any State ? AttackBow transition:
Set "Can Transition To Self" to ? (unchecked)
```

**If that doesn't work:**
```
Add to code:
animator.ResetTrigger(AttackBowHash);
before
animator.SetTrigger(AttackBowHash);
```

**If still not working:**
```
Replace Any State transition with explicit:
Idle ? AttackBow
Walk ? AttackBow
Run ? AttackBow
```

**Your bow attacks will trigger every time!** ???
