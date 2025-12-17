# ?? Animation Import Quick Fix - Player Falling Through Ground

## The Problem
Player falls through ground when using Mixamo/mocap animations.

## The Solution (3 Steps)

### Step 1: Select Animation
```
In Project window ? Click your animation FBX file
```

### Step 2: Fix Import Settings
```
Inspector ? Animation tab:

Root Transform Position (Y):
?? Bake Into Pose: ? ? CHECK THIS!
?? Based Upon: Original

Root Transform Position (XZ):
?? Bake Into Pose: ? ? CHECK THIS!
?? Based Upon: Center of Mass

Root Transform Rotation:
?? Bake Into Pose: ? ? CHECK THIS!
?? Based Upon: Original
```

### Step 3: Apply
```
Click "Apply" button at bottom
Test in Play Mode ?
```

## Why This Happens

**Animation contains Y position data:**
- Animation moves character up/down
- Character clips through ground
- Unity physics fights animation

**Solution:**
- "Bake Into Pose" removes Y movement
- Character stays at ground level
- Physics works correctly

## Quick Settings Reference

### ? Correct Settings (Prevents Falling)
```
Root Transform Position (Y):
Bake Into Pose: ? ? Must be checked!
```

### ? Wrong Settings (Causes Falling)
```
Root Transform Position (Y):
Bake Into Pose: ? ? This causes falling!
```

## For ALL Your Animations

**Select all FBX files at once:**
1. Hold Ctrl (or Cmd on Mac)
2. Click each animation file
3. Set Animation tab settings
4. Click Apply once

**Applies to all selected!**

## Mixamo Specific

### Download Settings:
```
Format: FBX for Unity
Skin: Without Skin
FPS: 30
```

### After Import:
```
1. Rig tab:
   - Animation Type: Humanoid
   - Avatar: Copy from Other Avatar
   - Source: Your character
   - Apply

2. Animation tab:
   - Root Transform Position (Y) ? Bake: ?
   - Apply
```

## Still Having Issues?

### Check:
```
? Animation ? Root Transform Position (Y) ? Bake Into Pose: ?
? Character has Rigidbody component
? Character has Collider component
? Ground has Collider component
? Rigidbody ? Use Gravity: ?
? Rigidbody ? Is Kinematic: ?
```

### If Scale is Wrong:
```
Mixamo characters are often 100x too large

Fix:
Select FBX ? Model tab ? Scale Factor: 0.01
```

## Common Animation Types

### Walk/Run/Idle:
```
Root Transform Position (Y): Bake ?
Root Transform Position (XZ): Bake ?
Root Transform Rotation: Bake ?
Loop Time: ?
```

### Attacks/Spells:
```
Root Transform Position (Y): Bake ?
Root Transform Position (XZ): Bake ?
Root Transform Rotation: Bake ?
Loop Time: ?
```

### Dodge/Roll:
```
Root Transform Position (Y): Bake ?
Root Transform Position (XZ): Bake ? (allows movement)
Root Transform Rotation: Bake ?
Loop Time: ?
```

## Your Code is Already Correct!

**PlayerMovement.cs already has:**
```csharp
animator.applyRootMotion = false;  ?
playerRigidbody.useGravity = true;  ?
```

**The issue is ONLY in animation import settings!**

## Visual Checklist

```
Animation Import:
? Bake Y position into pose
? Bake rotation into pose  
? Bake XZ position into pose (for most animations)
? Set Animation Type to Humanoid
? Copy avatar from your character
? Click Apply

Character Setup:
? Has Rigidbody
? Use Gravity enabled
? Not Kinematic
? Has Collider
? Animator has Controller

Result:
? Character stays on ground
? Animations play correctly
? No falling through floor
? No floating
? No spinning
```

## One-Line Fix

**For each animation FBX:**
```
Animation tab ? Root Transform Position (Y) ? Bake Into Pose ? ? Apply
```

**That's it!** ???

## Complete Guide

See `ANIMATION_IMPORT_FIX.md` for detailed explanations.
