# ??? Attack Movement Lock System - Complete Guide

## Overview

Your player now **stops moving during attacks** for realistic, grounded combat!

---

## ? **What's Been Added**

### New Features:
- ?? **Movement Lock** - Player can't move during attacks
- ? **Automatic Unlock** - Movement restored after attack
- ?? **Configurable Durations** - Set lock time per attack type
- ?? **Velocity Stop** - Smooth halt when entering attack
- ?? **Toggle System** - Can disable if needed

---

## ?? **How It Works**

### Before (Movement During Attacks):
```
Click Attack ? Animation plays
Player can still move with WASD
Character slides while attacking
Looks unrealistic ?
```

### After (Locked Movement):
```
Click Attack ? Animation plays
Movement input ignored
Player locked in place
Character performs grounded attack
Looks professional! ?

After 0.6 seconds ? Movement unlocked
Can move again ?
```

---

## ?? **Settings (Inspector)**

### PlayerMovement Component:

**Attack Movement Lock Settings:**
```
Lock Movement During Attacks: ?
- Check to enable movement lock
- Uncheck to allow movement during attacks

Melee Attack Duration: 0.6 seconds
- How long player is locked for sword attacks
- Should match your melee animation length

Bow Attack Duration: 0.8 seconds
- How long player is locked for bow attacks
- Includes draw time + release

Spell Cast Duration: 0.7 seconds
- How long player is locked for spells
- Includes cast animation
```

---

## ?? **Attack Types & Durations**

### Melee Attacks (Sword):
```
Duration: 0.6 seconds (default)

Sequence:
1. Click attack
2. Player stops moving
3. Sword swing animation (0.6s)
4. Movement unlocked
5. Can move/attack again
```

**Adjust if:**
- Animation is longer: Increase duration (0.7-0.8)
- Animation is shorter: Decrease duration (0.4-0.5)
- Want faster combat: Decrease duration

---

### Bow Attacks:
```
Duration: 0.8 seconds (default)

Sequence:
1. Click attack
2. Player stops moving
3. Bow draw animation
4. Arrow fires (after 0.5s projectile delay)
5. Bow release
6. Movement unlocked (0.8s total)
7. Can move again
```

**Adjust if:**
- Slow bow: Increase duration (1.0-1.2)
- Quick bow: Decrease duration (0.6-0.7)

---

### Spell Casting:
```
Duration: 0.7 seconds (default)

Sequence:
1. Click cast
2. Player stops moving
3. Cast animation
4. Spell fires (after 0.3s projectile delay)
5. Cast completes
6. Movement unlocked (0.7s total)
7. Can move again
```

**Adjust if:**
- Long cast: Increase duration (0.9-1.0)
- Quick cast: Decrease duration (0.5-0.6)
- Instant cast: Very low (0.3-0.4)
```

---

## ?? **Key Features**

### Smooth Velocity Stop:

**When entering attack:**
```csharp
// Horizontal velocity set to 0
velocity.x = 0;
velocity.z = 0;
// Gravity still works (y velocity preserved)
velocity.y = unchanged;
```

**Result:**
- Player stops smoothly
- No sliding
- Can still fall if in air
- Natural, grounded feel

---

### Automatic Unlock:

**No manual management needed:**
```
Attack triggered ? Lock duration set ? Waits ? Auto-unlocks
```

**Benefits:**
- Can't forget to unlock
- Always consistent
- No stuck states
- Reliable system

---

### Per-Attack Customization:

**Different lock times for different attacks:**
```
Melee: 0.6s (quick slashes)
Bow: 0.8s (draw and release)
Spell: 0.7s (casting time)
```

**Easy to adjust per weapon type later!**

---

## ?? **Adjusting Duration**

### Method 1: Match Animation Length

**Steps:**
```
1. Open Animator window
2. Click attack animation state
3. Note animation length (e.g., 0.65 seconds)
4. Set duration slightly longer (0.7 seconds)
5. Test in Play Mode
6. Adjust if needed
```

**Formula:**
```
Lock Duration = Animation Length + 0.05-0.1 seconds
(Slight buffer for smooth feel)
```

---

### Method 2: Feel-Based Tuning

**Steps:**
```
1. Set initial guess (0.6 for melee)
2. Enter Play Mode
3. Attack multiple times
4. Does it feel too long? Decrease by 0.1
5. Does it feel too short? Increase by 0.1
6. Repeat until perfect
```

**Sweet Spots:**
```
Too Short (<0.4s): Feels rushed, can move mid-swing
Just Right (0.5-0.7s): Grounded, weighty, professional
Too Long (>1.0s): Feels sluggish, unresponsive
```

---

## ?? **Behavior Details**

### What Happens When Attacking:

**1. Attack Triggered:**
```
playerMovement.TriggerComboAttack(1)
? isAttacking = true
? Horizontal velocity = 0
? Movement input ignored
```

**2. During Attack:**
```
Player tries to press WASD ? Ignored
CanMove() returns false
No movement applied
Character stays in place
Animation plays normally
```

**3. After Duration:**
```
0.6 seconds pass
? isAttacking = false
? Movement input accepted
? Can move freely again
```

---

### What Still Works:

**During attack lock:**
```
? Rotation (faces aim target)
? Animation plays
? Projectiles fire (with delay)
? Damage dealt
? Gravity applies
? Can be hit
? Movement input (locked)
? Dodge (if during attack)
? Sprint (movement locked)
```

---

## ?? **Advanced Usage**

### Combo Attacks:

**Each combo hit locks separately:**
```
Attack 1 (0.6s lock):
Click ? Lock ? Animation ? Unlock

Attack 2 (0.6s lock):
Click ? Lock ? Animation ? Unlock

Attack 3 (0.6s lock):
Click ? Lock ? Animation ? Unlock
```

**Total combo time: ~1.8 seconds**
**Player locked for each individual hit**

---

### Rapid Attacks:

**Spam clicking:**
```
Click 1 ? Lock starts (0.6s)
Click 2 (0.3s later) ? Resets lock (0.6s from now)
Click 3 (0.3s later) ? Resets lock again

Result: Locked until last attack finishes
Works as intended! ?
```

---

### Movement Buffering:

**Holding WASD during attack:**
```
Attack starts ? WASD held
Movement blocked during attack
Attack ends ? Immediately starts moving
No input lag!
```

**Natural feel, instant response after unlock!**

---

## ?? **Customization Options**

### Disable Lock (Allow Movement):

**If you want old behavior:**
```
PlayerMovement Inspector:
Lock Movement During Attacks: ? (uncheck)

Result:
- Can move during attacks
- Old sliding behavior
- Good for fast-paced arcade games
```

---

### Weapon-Specific Durations:

**If you add different weapon types:**

```csharp
[Header("Weapon-Specific Lock Durations")]
[SerializeField] private float swordAttackDuration = 0.6f;
[SerializeField] private float daggerAttackDuration = 0.4f; // Faster
[SerializeField] private float hammerAttackDuration = 0.9f; // Slower

public void TriggerSwordAttack()
{
    SetAttackingStateWithDuration(swordAttackDuration);
}

public void TriggerDaggerAttack()
{
    SetAttackingStateWithDuration(daggerAttackDuration);
}
```

---

### Charge Attacks (Longer Lock):

**For charged/heavy attacks:**

```csharp
public void TriggerChargedAttack(float chargeTime)
{
    // Lock for charge time + attack animation
    float totalDuration = chargeTime + meleeAttackDuration;
    SetAttackingStateWithDuration(totalDuration);
}

// Usage:
TriggerChargedAttack(1.5f); // 1.5s charge + 0.6s attack = 2.1s lock
```

---

## ?? **Common Issues**

### Player stuck, can't move:

**Symptom:**
```
Attack once, then permanently can't move
```

**Cause:**
```
Attack duration too long or coroutine failed
```

**Fix:**
```
1. Check attack durations aren't crazy high (>5.0)
2. Test if toggle works (disable lock)
3. Add debug: Debug.Log(isAttacking) in Update()
4. Should see: true ? false after duration
```

---

### Can move too soon:

**Symptom:**
```
Animation still playing but can move
Looks unfinished
```

**Fix:**
```
Increase attack duration:
Melee: 0.6 ? 0.7 or 0.8
Match your animation length!
```

---

### Can't move for too long:

**Symptom:**
```
Animation done but still locked
Feels sluggish
```

**Fix:**
```
Decrease attack duration:
Melee: 0.6 ? 0.5 or 0.4
Should unlock right when animation ends
```

---

### Movement input feels delayed:

**Symptom:**
```
Press WASD after attack
Slight delay before moving
```

**Cause:**
```
This is normal! Attack must finish first
```

**Adjust:**
```
Lower attack durations for snappier feel
Or: Accept small delay for grounded combat
```

---

## ?? **Recommended Settings**

### Fast-Paced Action:
```
Lock Movement During Attacks: ?
Melee Attack Duration: 0.4-0.5
Bow Attack Duration: 0.6-0.7
Spell Cast Duration: 0.5-0.6

Result: Quick, responsive, arcade-style
```

---

### Realistic Combat:
```
Lock Movement During Attacks: ?
Melee Attack Duration: 0.6-0.7
Bow Attack Duration: 0.8-1.0
Spell Cast Duration: 0.7-0.9

Result: Weighty, grounded, tactical
```

---

### Souls-Like:
```
Lock Movement During Attacks: ?
Melee Attack Duration: 0.7-0.9
Bow Attack Duration: 1.0-1.2
Spell Cast Duration: 0.8-1.0

Result: Committed attacks, high risk/reward
```

---

### Arcade/Mobile:
```
Lock Movement During Attacks: ? (disabled)

Result: Can move during attacks
Fast-paced, casual feel
```

---

## ?? **Testing**

### Test Melee Lock:

```
1. Enter Play Mode
2. Switch to Melee mode
3. Click to attack
4. Immediately try to move (WASD)
5. Check:
   - Can't move? ?
   - Animation plays? ?
   - After ~0.6s, can move again? ?
```

---

### Test Bow Lock:

```
1. Switch to Ranged mode
2. Click to shoot
3. Try to move during draw
4. Check:
   - Can't move? ?
   - Bow draws? ?
   - Arrow fires? ?
   - After ~0.8s, can move? ?
```

---

### Test Spell Lock:

```
1. Switch to Spell mode
2. Click to cast
3. Try to move during cast
4. Check:
   - Can't move? ?
   - Spell animation plays? ?
   - Projectile fires? ?
   - After ~0.7s, can move? ?
```

---

### Test Combo:

```
1. Attack combo (click 3 times)
2. Try to move between hits
3. Check:
   - Locked during each hit? ?
   - Unlocks between hits? ?
   - Combo flows naturally? ?
```

---

## ?? **Pro Tips**

**Match Your Animation:**
```
Play animation in slow-mo
Note when character should be able to move again
Set duration to match that point
Perfect sync!
```

**Buffer for Combo Flow:**
```
Unlock slightly before animation ends
Allows next input to queue
Smoother combo chains
Example: 0.65s animation ? 0.6s lock
```

**Visual Feedback:**
```
Add ground effect when attacking
Dust, impact particles
Makes lock feel intentional
Player understands they're committed
```

---

## ?? **Console Messages**

**What you'll see:**
```
[PlayerMovement] Triggered Attack1 (Combo 1/3) - Movement locked
[PlayerMovement] Entered attacking state - movement locked, rotating to aim target
... (0.6 seconds pass)
[PlayerMovement] Exited attacking state - movement unlocked
```

**If you see these, system is working correctly! ?**

---

## Summary

**What You Got:**
```
? Movement lock during attacks
? Separate durations per attack type
? Automatic unlock after animation
? Smooth velocity stop
? Toggle to disable if needed
? Professional, grounded combat!
```

**Default Durations:**
```
Melee (Sword): 0.6 seconds
Bow (Ranged): 0.8 seconds
Spell (Magic): 0.7 seconds
```

**Settings:**
```
Inspector ? PlayerMovement ? Attack Movement Lock Settings
Adjust durations to match your animations
Toggle on/off as needed
```

**Result:**
Committed, weighty attacks that feel professional! ??????

**Your player now stops moving during attacks for realistic combat!** ????
