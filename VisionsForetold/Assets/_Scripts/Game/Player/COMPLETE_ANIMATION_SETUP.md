# ?? Complete Animation System Setup - 3-Hit Combo + All Animations

## Overview

Your animation system now supports:
- **3-Hit Sword Combo** with individual animations and pauses
- **Run Animation** at max speed
- **Bow Attack** with proper re-triggering
- **Spell Casting** (Fireball & Ice)
- **All Movement States** (Idle, Walk, Run, Walk_Hurt)
- **Damage & Dodge** animations

---

## ??? **3-Hit Combo System**

### Animator Parameters

Add these to your Animator Controller:

#### Integers:
```
ComboStep ? Int (0-3)
  - 0 = No combo active
  - 1 = First attack
  - 2 = Second attack
  - 3 = Third attack (finisher)
```

#### Triggers:
```
Attack1 ? Trigger (First hit)
Attack2 ? Trigger (Second hit)
Attack3 ? Trigger (Third hit - Finisher)
Attack ? Trigger (Generic fallback)
```

### Animator States & Transitions

**Create 3 Attack States:**

```
States:
?? Attack1 (First sword swing)
?? Attack2 (Second sword swing)
?? Attack3 (Third sword swing - big finisher)
```

**Transitions:**

**From Idle/Walk/Run ? Attack1:**
```
Condition: Attack1 (Trigger)
Has Exit Time: ?
Transition Duration: 0.05
```

**Attack1 ? Attack2:**
```
Condition: Attack2 (Trigger)
Has Exit Time: ?
Exit Time: 0.8 (near end of Attack1)
Transition Duration: 0.1
```

**Attack2 ? Attack3:**
```
Condition: Attack3 (Trigger)
Has Exit Time: ?
Exit Time: 0.8 (near end of Attack2)
Transition Duration: 0.1
```

**Return to Idle:**
```
Attack1 ? Idle:
?? Has Exit Time: ?
?? Exit Time: 0.95
?? Transition Duration: 0.2

Attack2 ? Idle:
?? Has Exit Time: ?
?? Exit Time: 0.95
?? Transition Duration: 0.2

Attack3 ? Idle:
?? Has Exit Time: ?
?? Exit Time: 0.98 (let finisher complete)
?? Transition Duration: 0.3
```

### How It Works

**Player attacks:**
1. **First Click** ? Attack1 plays ? Small pause at end
2. **Second Click** (within 1.5s) ? Attack2 plays ? Small pause
3. **Third Click** (within 1.5s) ? Attack3 plays ? Big finisher!

**If player stops clicking:**
- Attack returns to idle after brief pause
- Combo resets after 1.5 seconds
- Next attack starts from Attack1 again

### Animation Timing Recommendations

**For proper "stop between attacks" feel:**

```
Attack1 Animation:
?? Duration: 0.5-0.7 seconds
?? Swing: Frames 0-30
?? Recovery: Frames 30-42 (pause/ready)
?? Exit Time: 0.8 (allows combo continuation)

Attack2 Animation:
?? Duration: 0.6-0.8 seconds
?? Swing: Frames 0-35
?? Recovery: Frames 35-48 (pause/ready)
?? Exit Time: 0.8

Attack3 Animation:
?? Duration: 0.8-1.2 seconds (longer, more dramatic)
?? Wind-up: Frames 0-15
?? Big Swing: Frames 15-45
?? Impact: Frame 30 (add camera shake!)
?? Recovery: Frames 45-60
?? Exit Time: 0.98 (almost complete)
```

---

## ?? **Run Animation**

### Animator Parameters

```
IsRunning ? Bool (auto-managed)
Speed ? Float (0-1.8)
```

### Setup

**Option A: Bool-Based (Simpler)**

```
Walk State
  ? (IsRunning == true)
Run State
  ? (IsRunning == false)
Walk State
```

**Option B: Blend Tree (Smoother)**

```
Movement Blend Tree (Parameter: Speed)
?? 0.0 ? Idle
?? 0.5 ? Walk (slow)
?? 1.0 ? Walk (normal)
?? 1.2 ? Run (transition)
?? 1.8 ? Run (full sprint)
```

### Transitions

```
Walk ? Run:
?? Condition: IsRunning == true (or Speed > 1.2)
?? Has Exit Time: ?
?? Transition Duration: 0.15

Run ? Walk:
?? Condition: IsRunning == false (or Speed < 1.2)
?? Has Exit Time: ?
?? Transition Duration: 0.15
```

**Triggers automatically when:**
- Player sprints (Hold Shift + Move)
- Speed exceeds 1.2
- Returns to walk when sprint released

---

## ?? **Bow Attack Animation**

### Animator Parameters

```
AttackBow ? Trigger
```

### Setup

**From Any State ? AttackBow:**

```
Has Exit Time: ?
Can Transition To Self: ? ? IMPORTANT!
Transition Duration: 0.05
Condition: AttackBow (Trigger)
```

**AttackBow ? Idle:**

```
Has Exit Time: ?
Exit Time: 0.9
Transition Duration: 0.15
```

**Animation Timing:**

```
Bow Attack Animation:
?? Draw bow: Frames 0-10
?? Aim: Frames 10-15
?? Release: Frame 15 ? Add animation event here
?? Follow-through: Frames 15-25
?? Return: Frames 25-30

Animation Event at Frame 15:
Function: "OnBowRelease"
(Spawn arrow at exact moment)
```

### Re-Trigger Fix

The code now includes automatic trigger reset:
```csharp
animator.ResetTrigger(AttackBowHash);
animator.SetTrigger(AttackBowHash);
```

This ensures bow attack works every time!

---

## ?? **Spell Animations**

### Animator Parameters

```
SpellFireball ? Trigger
SpellIce ? Trigger
```

### Setup

**From Any State ? SpellFireball:**

```
Has Exit Time: ?
Can Transition To Self: ?
Transition Duration: 0.05
Condition: SpellFireball (Trigger)
```

**From Any State ? SpellIce:**

```
Has Exit Time: ?
Can Transition To Self: ?
Transition Duration: 0.05
Condition: SpellIce (Trigger)
```

**Return to Idle:**

```
Spell ? Idle:
?? Has Exit Time: ?
?? Exit Time: 0.85
?? Transition Duration: 0.2
```

### Animation Timing

```
Fireball Animation:
?? Wind-up: Frames 0-10
?? Cast: Frame 15 ? Add animation event
?? Hold: Frames 15-20
?? Release: Frames 20-25
?? Return: Frames 25-35

Ice Spell Animation:
?? Gather energy: Frames 0-12
?? Cast: Frame 18 ? Add animation event
?? Burst: Frames 18-25
?? Return: Frames 25-40

Animation Event:
Function: "OnSpellCast"
(Spawn projectile at exact cast point)
```

---

## ?? **Complete Animator Parameters List**

### Bools:
```
IsMoving ? Player is moving
IsSprinting ? Player is sprinting (Shift held)
IsRunning ? Player at max speed (Speed > 1.2)
IsLowHealth ? Health below 30%
```

### Floats:
```
Speed ? Movement speed (0-1.8)
HealthPercentage ? Current health (0-1)
```

### Integers:
```
ComboStep ? Current combo step (0-3)
```

### Triggers:
```
Attack ? Generic attack (fallback)
Attack1 ? Combo attack 1
Attack2 ? Combo attack 2
Attack3 ? Combo attack 3 (finisher)
AttackBow ? Bow/ranged attack
SpellFireball ? Fireball spell
SpellIce ? Ice spell
Hurt ? Taking damage
Dodge ? Dodge roll
Dash ? Dash movement
```

---

## ?? **Complete State Machine Structure**

### Base Layer:

```
?? Idle (default)
?
?? Locomotion
?  ?? Walk
?  ?? Run (IsRunning or Speed > 1.2)
?  ?? Walk_Hurt (IsLowHealth)
?
?? Combat
?  ?? Attack1 (first combo hit)
?  ?? Attack2 (second combo hit)
?  ?? Attack3 (third combo hit - finisher)
?  ?? AttackBow (ranged attack)
?  ?? SpellFireball
?  ?? SpellIce
?
?? Reactions
?  ?? Hurt (damage taken)
?  ?? Dodge (dodge roll)
?
?? Special
   ?? Dash
```

---

## ?? **Animation Clips Needed**

### Movement (Loop: ?):
```
? idle.fbx
? walk.fbx
? run.fbx (at max speed)
? walk_hurt.fbx (injured walk)
```

### Combat (Loop: ?):
```
? attack1.fbx (first sword swing)
? attack2.fbx (second sword swing)
? attack3.fbx (big finisher swing)
? attack_bow.fbx (bow shoot)
? spell_fireball.fbx (cast fireball)
? spell_ice.fbx (cast ice)
```

### Reactions (Loop: ?):
```
? hurt.fbx (damage reaction)
? dodge.fbx (dodge roll)
? dash.fbx (quick dash)
```

---

## ?? **Animation Import Settings**

### For ALL Animations:

```
Rig Tab:
?? Animation Type: Humanoid
?? Avatar Definition: Copy from Other Avatar
?? Source: [Your Character Avatar]

Animation Tab:
?? Root Transform Rotation: Bake Into Pose ?
?? Root Transform Position (Y): Bake Into Pose ?
?? Root Transform Position (XZ): Bake Into Pose ?
?? Resample Curves: ?
```

### Specific Settings:

**Walk/Run/Idle:**
```
Loop Time: ?
Loop Pose: ?
```

**Attacks/Spells:**
```
Loop Time: ?
Root Transform Position (XZ): Bake ?
```

**Dodge/Dash:**
```
Loop Time: ?
Root Transform Position (XZ): Bake ? (if you want movement)
```

---

## ?? **Testing Checklist**

### Movement:
```
? Idle plays when standing
? Walk plays when moving slowly
? Run plays when sprinting at full speed
? Walk_Hurt plays when health < 30%
? Smooth transitions between all states
```

### Combat:
```
? Attack1 plays on first click
? Attack2 plays on second click (within 1.5s)
? Attack3 plays on third click (big finisher!)
? Combo resets if player stops clicking
? Each attack has visible pause/recovery
? AttackBow plays every time (not just once!)
? Spells play when cast
```

### Timing:
```
? Small pause after Attack1 before returning to idle
? Small pause after Attack2
? Longer, dramatic pause after Attack3 finisher
? Bow arrow spawns at correct animation frame
? Spells cast at correct animation frame
```

---

## ?? **Advanced: Animation Events**

### Add These for Perfect Timing:

**Attack1 Animation:**
```
Frame 20: OnAttackHit1
Frame 42: OnAttackComplete1
```

**Attack2 Animation:**
```
Frame 22: OnAttackHit2
Frame 48: OnAttackComplete2
```

**Attack3 Animation:**
```
Frame 30: OnAttackHit3 (big impact!)
Frame 25: OnCameraShake (shake on wind-up)
Frame 60: OnAttackComplete3
```

**AttackBow Animation:**
```
Frame 15: OnBowRelease (spawn arrow)
```

**Spell Animations:**
```
Frame 15-18: OnSpellCast (spawn projectile)
```

### In PlayerAttack.cs, add:

```csharp
// Called by animation events
public void OnAttackHit1()
{
    Debug.Log("Attack 1 connected!");
    // Damage already dealt, but can add effects here
}

public void OnAttackHit2()
{
    Debug.Log("Attack 2 connected!");
}

public void OnAttackHit3()
{
    Debug.Log("Attack 3 FINISHER connected!");
    // Add screen shake, particle effects, etc.
}

public void OnBowRelease()
{
    Debug.Log("Arrow released!");
    // Already handled in PerformRangedAttack
}

public void OnSpellCast()
{
    Debug.Log("Spell cast!");
    // Projectile already spawned
}
```

---

## ?? **Recommended Animation Speeds**

### In Animator States:

```
Idle: 1.0
Walk: 1.0
Run: 1.0-1.1 (slightly faster feels better)

Attack1: 1.3-1.5 (snappy first hit)
Attack2: 1.2-1.4 (good flow)
Attack3: 1.0-1.2 (let finisher feel powerful)

AttackBow: 1.2-1.3 (quick shots)
SpellFireball: 1.2
SpellIce: 1.1

Hurt: 1.2-1.5 (quick reaction)
Dodge: 1.0 (controlled by code)
```

---

## ?? **Troubleshooting**

### Combo doesn't chain:
```
Check:
? Exit Time on Attack1 ? Attack2 is ~0.8
? Attack2 trigger fires before Attack1 completes
? comboWindow in PlayerAttack.cs is 1.5 seconds
```

### Attacks feel too fast/slow:
```
Adjust:
- Animator state Speed values
- Animation clip trimming (Start/End frames)
- Transition durations
```

### Bow only fires once:
```
Check:
? Any State ? AttackBow: Can Transition To Self = ?
? Code includes ResetTrigger (already added)
? AttackBow ? Idle transition exists
```

### Run doesn't trigger:
```
Check:
? Sprint working (Shift key)
? Speed parameter reaches > 1.2
? IsRunning transitions exist
```

---

## ?? **Quick Reference Table**

| Animation | Type | Loop | Speed | Exit Time |
|-----------|------|------|-------|-----------|
| Idle | State | ? | 1.0 | - |
| Walk | State | ? | 1.0 | - |
| Run | State | ? | 1.0 | - |
| Attack1 | Trigger | ? | 1.4 | 0.8 |
| Attack2 | Trigger | ? | 1.3 | 0.8 |
| Attack3 | Trigger | ? | 1.1 | 0.98 |
| AttackBow | Trigger | ? | 1.3 | 0.9 |
| SpellFireball | Trigger | ? | 1.2 | 0.85 |
| SpellIce | Trigger | ? | 1.1 | 0.85 |
| Hurt | Trigger | ? | 1.4 | 0.9 |
| Dodge | Trigger | ? | 1.0 | 1.0 |

---

## Summary

**Your complete animation system is now ready with:**

? 3-Hit Combo with pauses between attacks
? Run animation at max speed
? Bow attack that re-triggers properly
? Spell casting animations
? All movement states (idle, walk, run, walk_hurt)
? Damage and dodge animations
? Automatic trigger resets for reliability
? Proper state machine structure

**Next Steps:**
1. Import/assign all animation clips
2. Set up Animator Controller with all parameters
3. Create states and transitions as described
4. Test combo system (1-2-3 hits)
5. Test all other animations
6. Add animation events for perfect timing
7. Fine-tune speeds and timings

**Your combat will feel responsive, fluid, and satisfying!** ?????????
