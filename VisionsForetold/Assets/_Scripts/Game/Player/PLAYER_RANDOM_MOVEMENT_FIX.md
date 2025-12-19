# ?? Player Moving Randomly - Fix Guide

## ? **Problem**

Player is moving randomly on its own without any input.

---

## ?? **Root Cause**

After attempting to set up ragdoll, your player likely has **extra Rigidbody components on bones** that are causing physics conflicts with the movement system.

**What's happening:**
1. Player has Rigidbodies on bone GameObjects (from ragdoll setup)
2. These Rigidbodies have physics enabled
3. They're receiving gravity and colliding with things
4. This causes the player to move erratically

---

## ? **Quick Fix - Remove Ragdoll Components**

### Step 1: Find Extra Rigidbodies

**In Unity Hierarchy:**
```
1. Select your Player GameObject
2. Expand Armature/Skeleton
3. Expand all bones (Hips, Spine, Arms, Legs, Head)
4. Look for Rigidbody components on ANY bone
```

**You'll likely see:**
- Rigidbody on Hips
- Rigidbody on Spine
- Rigidbody on LeftUpperLeg, RightUpperLeg
- Rigidbody on LeftUpperArm, RightUpperArm
- Rigidbody on Head
- Colliders on all these bones
- Character Joints connecting them

---

### Step 2: Remove All Ragdoll Components

**Method 1: Manual Removal (Recommended)**

**For EACH bone that has ragdoll components:**
```
1. Select the bone (e.g., Hips, Spine, etc.)
2. Inspector ? Find and remove:
   - Rigidbody component (click gear ? Remove Component)
   - Collider component (if it's not the main player collider)
   - Character Joint component
3. Repeat for ALL bones
```

**Bones to check:**
- Hips
- Spine (multiple if you have Spine1, Spine2, etc.)
- LeftUpperLeg, LeftLowerLeg
- RightUpperLeg, RightLowerLeg
- LeftUpperArm, LeftLowerArm
- RightUpperArm, RightLowerArm
- Head
- Any other bones with components

---

**Method 2: Using a Script (Faster)**

**Create a temporary script:**

```csharp
using UnityEngine;

public class RemoveRagdollComponents : MonoBehaviour
{
    [ContextMenu("Remove All Ragdoll Components")]
    public void RemoveRagdoll()
    {
        // Get all rigidbodies in children
        Rigidbody[] rigidbodies = GetComponentsInChildren<Rigidbody>();
        
        Debug.Log($"Found {rigidbodies.Length} Rigidbodies");
        
        foreach (var rb in rigidbodies)
        {
            // Don't destroy the main rigidbody on player root
            if (rb.gameObject != gameObject)
            {
                Debug.Log($"Removing Rigidbody from {rb.gameObject.name}");
                DestroyImmediate(rb);
            }
        }
        
        // Get all colliders in children (except main collider)
        Collider mainCollider = GetComponent<Collider>();
        Collider[] colliders = GetComponentsInChildren<Collider>();
        
        foreach (var col in colliders)
        {
            if (col != mainCollider && col.gameObject != gameObject)
            {
                Debug.Log($"Removing Collider from {col.gameObject.name}");
                DestroyImmediate(col);
            }
        }
        
        // Get all character joints
        CharacterJoint[] joints = GetComponentsInChildren<CharacterJoint>();
        
        Debug.Log($"Found {joints.Length} Character Joints");
        
        foreach (var joint in joints)
        {
            Debug.Log($"Removing Character Joint from {joint.gameObject.name}");
            DestroyImmediate(joint);
        }
        
        Debug.Log("Ragdoll components removed!");
    }
}
```

**Usage:**
```
1. Add this script to your Player GameObject
2. Right-click on the script in Inspector
3. Click "Remove All Ragdoll Components"
4. Check Console for confirmation
5. Remove the script after it's done
```

---

### Step 3: Verify Player Setup

**After removing ragdoll components, your player should have:**

**On Player Root:**
```
? Transform
? Rigidbody (only ONE, on the root)
? Collider (CapsuleCollider or similar)
? PlayerMovement script
? PlayerAttack script
? Health script
? Animator
? PlayerInput
```

**On Bones (should be CLEAN):**
```
? Transform ONLY
? No Rigidbody
? No Collider (except maybe main player collider)
? No Character Joint
```

---

## ?? **Verification Steps**

### Test 1: Check Components

**Select Player in Hierarchy:**
```
Inspector ? Should see:
- ONE Rigidbody on root
- ONE main Collider
- No Rigidbody on any bones
```

**Select any bone (Hips, Spine, etc.):**
```
Inspector ? Should see:
- Transform component ONLY
- No Rigidbody
- No Collider
- No Character Joint
```

---

### Test 2: Check Rigidbody Settings

**Select Player Root:**
```
Inspector ? Rigidbody:
? Use Gravity (if using gravity-based movement)
? Is Kinematic (should be unchecked for physics movement)
? Constraints:
  - Freeze Rotation X: ?
  - Freeze Rotation Y: ? (or ? depending on your setup)
  - Freeze Rotation Z: ?
  - Freeze Position: ??? (all unchecked)
```

---

### Test 3: Enter Play Mode

**After cleanup:**
```
1. Enter Play Mode
2. Don't touch any controls
3. Player should stay STILL
4. No random movement
5. No jittering
6. No spinning
```

**If player is still:**
? Success! Problem fixed!

**If player still moves:**
Continue to Advanced Troubleshooting below

---

## ?? **Advanced Troubleshooting**

### Issue 1: Player Still Moving

**Check Input System:**
```
1. Select Player
2. Inspector ? PlayerInput component
3. Check "Actions" asset
4. Look for unintended input bindings
```

**Check for stuck input:**
```
1. Add debug to PlayerMovement.cs OnMove():

public void OnMove(InputAction.CallbackContext context)
{
    movementInput = context.ReadValue<Vector2>();
    Debug.Log($"Movement Input: {movementInput}"); // Add this
    
    // ...rest of code
}

2. Enter Play Mode
3. Check Console
4. If you see non-zero values when not touching controls ? Input issue
```

---

### Issue 2: Player Jittering/Shaking

**Check collider conflicts:**
```
1. Select Player
2. Check if collider is colliding with floor correctly
3. Check Physics layers (Edit ? Project Settings ? Physics)
4. Make sure Player layer collides with Default
```

**Check Rigidbody constraints:**
```
Inspector ? Rigidbody ? Constraints:
- Freeze Rotation X, Y, Z: ??? (all checked)
- This prevents unwanted rotation
```

---

### Issue 3: Player Sliding

**Check if controller input has deadzone:**
```
PlayerMovement.cs should have:

[SerializeField] private float joystickDeadzone = 0.15f;

In OnMove():
if (movementInput.magnitude < joystickDeadzone)
{
    movementInput = Vector2.zero;
}
```

---

## ?? **Prevention Tips**

### Avoid Ragdoll Setup Issues:

**If you want ragdoll in the future:**
1. **Never** manually add Rigidbodies to bones
2. Use Unity's Ragdoll Wizard (GameObject ? 3D Object ? Ragdoll)
3. Set them to **kinematic** initially
4. Only enable on death

**For now:**
- Keep player simple
- No ragdoll components
- Just basic movement

---

## ?? **Expected State**

### Player GameObject Structure:

```
Player (Root)
?? Transform
?? Rigidbody (ONE, kinematic or not depending on setup)
?? Collider
?? PlayerMovement
?? PlayerAttack
?? Health
?? Animator
?? Armature
   ?? Hips (Transform ONLY)
   ?  ?? Spine (Transform ONLY)
   ?  ?? LeftUpperLeg (Transform ONLY)
   ?  ?? RightUpperLeg (Transform ONLY)
   ?? [More bones with Transform ONLY]
```

**No Rigidbodies on bones!**
**No Colliders on bones!**
**No Character Joints!**

---

## ? **Quick Checklist**

**To fix random movement:**

```
? Remove all Rigidbodies from bones
? Remove all Colliders from bones (except main)
? Remove all Character Joints
? Verify only ONE Rigidbody on player root
? Check Rigidbody constraints are set
? Test in Play Mode (player should stay still)
? Check Console for movement input debug
? Verify no input when not touching controls
```

---

## ?? **Most Common Fix**

**99% of the time, it's:**

**Rigidbodies on bones from failed ragdoll setup**

**Solution:**
1. Select each bone
2. Remove Rigidbody component
3. Remove Collider (if present)
4. Remove Character Joint (if present)
5. Test

**Done!**

---

## ?? **Summary**

**Problem:** Player moving randomly

**Cause:** Extra Rigidbody components on bones

**Solution:** Remove all ragdoll components from bones

**Prevention:** Don't add ragdoll without proper setup

**Expected Result:** Player stays still when no input is given

---

**Remove ragdoll components from bones and test!** ??

**Player should stop moving randomly!** ?

**Keep player structure simple!** ??
