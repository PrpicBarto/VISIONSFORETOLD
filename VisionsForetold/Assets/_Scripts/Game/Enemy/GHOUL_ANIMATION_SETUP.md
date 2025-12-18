# ?? Ghoul Animation Setup Guide

## Overview

The Ghoul enemy now has a complete animation system with **attack, idle, and run** animations!

---

## ? **What's Been Added**

### Code Features:
- ?? **Animator Integration** - Full animator support
- ?? **Run Animation** - Plays when chasing player
- ?? **Idle Animation** - Plays when standing still
- ?? **Attack Animation** - Triggers when attacking player
- ?? **Speed Parameter** - Smooth speed blending (0-1)
- ?? **Automatic Updates** - Animations update based on AI state

---

## ?? **Animation Parameters**

### Required Animator Parameters:

**Add these to your Ghoul Animator Controller:**

```
Bools:
+ IsMoving ? True when ghoul is moving

Floats:
+ Speed ? Movement speed (0.0 to 1.0 normalized)

Triggers:
+ Attack ? Triggers attack animation
```

---

## ?? **Animator Setup**

### Step 1: Create Animator Controller

**In Unity:**
```
1. Right-click in Project window
2. Create ? Animator Controller
3. Name it: "GhoulAnimator"
4. Drag onto Ghoul prefab
```

---

### Step 2: Add Parameters

**Open Animator window (Ctrl+9):**

```
Parameters tab (left side):
1. Click "+" ? Bool ? Name: "IsMoving"
2. Click "+" ? Float ? Name: "Speed"
3. Click "+" ? Trigger ? Name: "Attack"
```

---

### Step 3: Create Animation States

**In Animator window:**

```
States needed:
1. Idle (default state)
2. Run
3. Attack

Create them:
1. Right-click ? Create State ? Empty
2. Rename to "Idle"
3. Repeat for "Run" and "Attack"
```

---

### Step 4: Assign Animation Clips

**For each state:**

```
Idle State:
1. Click Idle state
2. Inspector ? Motion field
3. Drag your idle animation here

Run State:
1. Click Run state
2. Inspector ? Motion
3. Drag your run animation here

Attack State:
1. Click Attack state
2. Inspector ? Motion
3. Drag your attack animation here
```

---

### Step 5: Setup Transitions

**Movement Transitions:**

**Idle ? Run:**
```
Condition: IsMoving == true
Has Exit Time: ? (unchecked)
Transition Duration: 0.15
```

**Run ? Idle:**
```
Condition: IsMoving == false
Has Exit Time: ? (unchecked)
Transition Duration: 0.15
```

**Attack Transitions:**

**Any State ? Attack:**
```
Condition: Attack (Trigger)
Has Exit Time: ? (unchecked)
Can Transition To Self: ? (unchecked)
Transition Duration: 0.05
```

**Attack ? Idle:**
```
Has Exit Time: ? (checked)
Exit Time: 0.9
Transition Duration: 0.15
No conditions needed
```

---

## ?? **Animation Import Settings**

### For ALL Ghoul Animations:

**Select animation FBX ? Inspector:**

```
Rig Tab:
- Animation Type: Humanoid (or Generic)
- Avatar: Copy from Other Avatar (if Humanoid)
- Apply

Animation Tab:
- Loop Time: 
  ? For Idle (loop)
  ? For Run (loop)
  ? For Attack (no loop)
  
- Loop Pose: ? (for looping animations)

- Root Transform Rotation: Bake Into Pose ?
- Root Transform Position (Y): Bake Into Pose ?
- Root Transform Position (XZ): Bake Into Pose ?

- Apply
```

---

## ?? **How It Works**

### Idle State:
```
Ghoul standing still or waiting
IsMoving: false
Speed: 0.0
Animation: Idle plays (loop)
```

### Chasing Player:
```
Ghoul detects player, starts chasing
IsMoving: true
Speed: 0.0 ? 1.0 (smooth transition)
Animation: Idle ? Run (smooth blend)
```

### Attacking:
```
Ghoul in attack range
Attack trigger fires
Animation: Run/Idle ? Attack
After attack: Attack ? Idle
Cooldown before next attack
```

---

## ?? **Behavior Flow**

### Spawn:
```
Ghoul spawns ? Idle animation plays
Waiting for player...
```

### Detect Player:
```
Player enters detection range
Ghoul starts moving toward player
IsMoving = true ? Run animation plays
```

### Chase:
```
Continuously moving toward player
Run animation loops
Speed parameter updates based on velocity
```

### Reach Player:
```
Within attack range
Stops moving
IsMoving = false ? Transitions to Idle
Waits for attack cooldown
```

### Attack:
```
Attack cooldown ready
Attack trigger fires
Attack animation plays
Damage dealt to player
```

### After Attack:
```
Attack animation completes
Returns to Idle
If player still in range: Attack again (after cooldown)
If player moved away: Chase again (Run animation)
```

---

## ?? **Inspector Settings**

### Ghoul Component:

**Animation Section:**
```
Animator: [Drag GhoulAnimator here]
Animation Smooth Time: 0.1
- Lower (0.05) = Snappier transitions
- Higher (0.2) = Smoother transitions
```

---

## ?? **Animation Speed Parameter**

### What It Does:

**Normalized Speed (0.0 to 1.0):**
```
0.0 = Standing still (Idle)
0.5 = Half speed (slower run)
1.0 = Full speed (normal run)

Smoothly blends between values
Natural acceleration/deceleration
```

### Uses:

**Can be used for:**
- Blend trees (walk ? run transition)
- Speed variations (slow/fast ghouls)
- Animation speed multiplier
- Visual feedback of movement

---

## ?? **Ghoul Type Animations**

### Speed Differences:

**Basic Ghoul:**
```
Move Speed: 3.0
Run animation plays at normal speed
Standard chase behavior
```

**Strong Ghoul:**
```
Move Speed: 2.5
Run animation plays slightly slower
Heavier, more deliberate movement
Optional: Adjust animation speed to 0.9x
```

**Fast Ghoul:**
```
Move Speed: 4.5
Run animation plays faster
Quick, aggressive movement
Optional: Adjust animation speed to 1.2x
```

### Adjusting Animation Speed:

**In Animator:**
```
1. Click Run state
2. Inspector ? Speed: 1.0
3. Adjust per ghoul type:
   - Basic: 1.0
   - Strong: 0.9 (slower)
   - Fast: 1.2 (faster)
```

---

## ?? **Testing**

### Test Idle:
```
1. Place Ghoul in scene (no player nearby)
2. Enter Play Mode
3. Check: Idle animation plays? ?
4. Ghoul stands still? ?
```

### Test Run:
```
1. Place Player in scene
2. Move player near Ghoul (detection range)
3. Check:
   - Ghoul starts moving? ?
   - Run animation plays? ?
   - Smooth transition from Idle? ?
   - Continues running while chasing? ?
```

### Test Attack:
```
1. Let Ghoul reach player (attack range)
2. Check:
   - Ghoul stops moving? ?
   - Attack animation plays? ?
   - Damage dealt? ?
   - Returns to Idle after? ?
   - Attacks again after cooldown? ?
```

### Test Chase ? Attack ? Chase:
```
1. Ghoul chases player (Run)
2. Player stops, ghoul reaches (Attack)
3. Player moves away during cooldown
4. Ghoul chases again (Run)
5. All transitions smooth? ?
```

---

## ?? **Advanced: Blend Trees**

### Optional: Walk-Run Blend Tree

**If you have both walk and run animations:**

**Create Blend Tree:**
```
1. Delete Run state
2. Create State ? From New Blend Tree
3. Rename to "Movement"
4. Double-click to enter
5. Blend Type: 1D
6. Parameter: Speed
7. Add motions:
   - 0.0: Idle
   - 0.5: Walk
   - 1.0: Run
```

**Result:**
- Speed 0.0-0.5: Blends Idle ? Walk
- Speed 0.5-1.0: Blends Walk ? Run
- Smooth transitions at all speeds

---

## ?? **Animation Recommendations**

### Ideal Animation Lengths:

**Idle:**
```
Duration: 2-4 seconds
Loop: ?
Type: Breathing, looking around
Feel: Menacing but passive
```

**Run:**
```
Duration: 0.5-1.0 second per cycle
Loop: ?
Type: Aggressive sprint/shamble
Feel: Threatening, determined
```

**Attack:**
```
Duration: 0.6-1.0 seconds
Loop: ?
Type: Bite, claw swipe
Feel: Fast, brutal
```

---

## ?? **Common Issues**

### Animations don't play:

**Check:**
```
? Animator component assigned on Ghoul
? AnimatorController assigned to Animator
? Animation clips assigned to states
? Parameters exist (IsMoving, Speed, Attack)
? Transitions exist (Idle ? Run, Any ? Attack)
```

---

### Ghoul slides without animation:

**Check:**
```
? IsMoving parameter updating?
? Speed parameter updating?
? Run state has animation assigned?
? Transition from Idle to Run works?
```

**Debug:**
```csharp
// Add to UpdateAnimations():
Debug.Log($"IsMoving: {isMoving}, Speed: {currentAnimationSpeed}");
```

---

### Attack animation doesn't play:

**Check:**
```
? Attack parameter is Trigger (not Bool)
? Any State ? Attack transition exists
? Attack animation assigned to Attack state
? Has Exit Time unchecked on Any ? Attack
? Can Transition To Self unchecked
```

---

### Animation stuck:

**Check:**
```
? Attack ? Idle transition has Exit Time checked
? Exit Time around 0.9 (near end of animation)
? All states have return path to Idle or Run
```

---

## ?? **Console Debug Messages**

### What You Should See:

**During chase:**
```
(No special messages, animations update automatically)
```

**During attack:**
```
Ghoul (Basic) bit player for 15 damage!
```

**If you see errors about animator:**
```
Check that Animator component exists
Check that AnimatorController is assigned
```

---

## ?? **Quick Setup Checklist**

```
Animator Setup:
? Create GhoulAnimator controller
? Add parameters: IsMoving (Bool), Speed (Float), Attack (Trigger)
? Create states: Idle, Run, Attack
? Assign animation clips to states
? Create transitions (see above)
? Set Loop Time for Idle and Run
? Uncheck Loop Time for Attack

Ghoul Setup:
? Assign Animator component
? Assign GhoulAnimator controller
? Test in Play Mode

Animation Import:
? Import animations with correct settings
? Humanoid or Generic rig
? Bake Root Transforms
? Loop Time for looping animations
```

---

## ?? **Pro Tips**

**Animation Speed:**
```
Fast Ghoul: Run animation Speed: 1.2
Strong Ghoul: Run animation Speed: 0.9
Matches their movement speed personality!
```

**Attack Timing:**
```
Attack cooldown (1.5s) should be longer than animation
Allows attack to fully play out
Prevents animation spam
```

**Smooth Transitions:**
```
Animation Smooth Time: 0.1
Lower for snappy response
Higher for smooth, natural flow
```

**Visual Polish:**
```
Add attack impact effect
Add footstep sounds to run animation
Add growl sound to attack animation
Makes animations feel alive!
```

---

## Summary

**Your Ghoul Now Has:**
```
? Idle animation (standing)
? Run animation (chasing)
? Attack animation (biting)
? Smooth transitions
? Speed-based blending
? Automatic state management
? Three variants (Basic, Strong, Fast)
```

**Required Setup:**
```
1. Create Animator Controller
2. Add 3 parameters (IsMoving, Speed, Attack)
3. Create 3 states (Idle, Run, Attack)
4. Assign animation clips
5. Setup transitions
6. Test!
```

**Result:**
Professional enemy AI with smooth, responsive animations! ???

**Your Ghoul animations are ready to use!**
