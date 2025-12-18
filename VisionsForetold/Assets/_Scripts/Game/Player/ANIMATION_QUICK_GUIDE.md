# ?? Quick Setup Guide - 3-Hit Combo + All Animations

## ? What's New

### Code Changes (Already Applied):
- ? 3-Hit combo system with individual triggers
- ? Automatic trigger resets for all animations
- ? ComboStep integer parameter support
- ? Attack1, Attack2, Attack3 triggers
- ? Run animation auto-triggers at Speed > 1.2
- ? All animations properly integrated

---

## ?? Quick Setup Steps

### 1. Add Animator Parameters

**Open your Animator Controller and add:**

```
Bools:
+ IsMoving
+ IsSprinting
+ IsRunning
+ IsLowHealth

Floats:
+ Speed
+ HealthPercentage

Integers:
+ ComboStep

Triggers:
+ Attack1 (first combo hit)
+ Attack2 (second combo hit)
+ Attack3 (third combo hit - finisher)
+ AttackBow
+ SpellFireball
+ SpellIce
+ Hurt
+ Dodge
+ Dash
```

### 2. Create Animation States

```
Movement:
- Idle
- Walk
- Run
- Walk_Hurt

Combat:
- Attack1 (assign first attack animation)
- Attack2 (assign second attack animation)
- Attack3 (assign third attack/finisher animation)
- AttackBow
- SpellFireball
- SpellIce

Reactions:
- Hurt
- Dodge
```

### 3. Setup Combo Transitions

**Critical for 3-hit combo:**

```
Any State ? Attack1:
- Condition: Attack1 (Trigger)
- Has Exit Time: ?
- Transition Duration: 0.05

Attack1 ? Attack2:
- Condition: Attack2 (Trigger)
- Has Exit Time: ?
- Exit Time: 0.8 ? Allows pause, then combo
- Transition Duration: 0.1

Attack2 ? Attack3:
- Condition: Attack3 (Trigger)
- Has Exit Time: ?
- Exit Time: 0.8
- Transition Duration: 0.1

Attack1/2/3 ? Idle:
- Has Exit Time: ?
- Exit Time: 0.95
- Transition Duration: 0.2
```

### 4. Setup Run Transition

```
Walk ? Run:
- Condition: IsRunning == true (or Speed > 1.2)
- Has Exit Time: ?
- Transition Duration: 0.15

Run ? Walk:
- Condition: IsRunning == false (or Speed < 1.2)
- Has Exit Time: ?
- Transition Duration: 0.15
```

### 5. Setup Other Transitions

```
Any State ? AttackBow:
- Condition: AttackBow (Trigger)
- Has Exit Time: ?
- Can Transition To Self: ? ? IMPORTANT!
- Transition Duration: 0.05

Any State ? SpellFireball/SpellIce:
- Same settings as AttackBow

Any State ? Hurt:
- Condition: Hurt (Trigger)
- Has Exit Time: ?
- Can Transition To Self: ? ? Can be hit multiple times
- Transition Duration: 0.05
- Priority: High

All attacks ? Idle:
- Has Exit Time: ?
- Exit Time: 0.85-0.95
- Transition Duration: 0.15
```

---

## ?? Animation Import Settings

**For EVERY animation:**

```
1. Select animation FBX
2. Inspector ? Rig tab:
   - Animation Type: Humanoid
   - Avatar: Copy from Other Avatar
   - Source: Your character
   - Apply

3. Inspector ? Animation tab:
   - Root Transform Rotation: Bake Into Pose ?
   - Root Transform Position (Y): Bake Into Pose ?
   - Root Transform Position (XZ): Bake Into Pose ?
   
4. For looping animations only (walk, run, idle):
   - Loop Time: ?
   - Loop Pose: ?
   
5. Apply
```

---

## ?? How It Works

### 3-Hit Combo:
```
Click 1: Attack1 ? Short pause ? Return to idle OR...
Click 2 (within 1.5s): Attack2 ? Short pause ? Return OR...
Click 3 (within 1.5s): Attack3 ? BIG FINISHER! ? Return

If you stop clicking, combo resets after 1.5 seconds
Next attack starts from Attack1 again
```

### Run:
```
Walk normally ? Walk animation
Sprint (Shift + Move) ? Speed increases
Speed reaches 1.2 ? Automatically switches to Run animation
Release Sprint ? Speed decreases ? Back to Walk
```

### Bow Attack:
```
Click to shoot ? AttackBow plays
Shoots every time (fixed re-trigger issue!)
Arrow spawns when animation triggers
```

### Spells:
```
Cast spell ? Animation plays
Projectile spawns automatically
Works every time
```

---

## ? Testing

### Test Combo:
```
1. Enter Play Mode
2. Switch to Melee mode
3. Click once ? See Attack1
4. Wait 2 seconds ? Combo resets
5. Click, wait 0.5s, click again ? Attack1 ? Attack2
6. Click three times quickly ? Attack1 ? Attack2 ? Attack3 (FINISHER!)
```

### Test Run:
```
1. Move with WASD ? Walk animation
2. Hold Shift + W ? Speed increases ? Run animation plays!
3. Release Shift ? Walk animation returns
```

### Test Bow:
```
1. Scroll to Ranged mode
2. Click to shoot ? AttackBow plays + arrow spawns
3. Click again ? AttackBow plays again (works every time!)
4. Keep clicking ? Keeps shooting
```

### Test Spells:
```
1. Scroll to Spell mode
2. Click to cast ? Spell animation + projectile
3. Wait for cooldown
4. Cast again ? Works every time
```

---

## ?? Common Issues

### Combo doesn't chain:
**Fix:** Check Exit Time on Attack1 ? Attack2 transition (should be ~0.8)

### Run doesn't play:
**Fix:** 
- Check IsRunning transitions exist
- Verify Sprint key is working (Shift)
- Check Speed parameter reaches > 1.2

### Bow only shoots once:
**Fix:** 
- Any State ? AttackBow: Can Transition To Self = ?
- Code already includes ResetTrigger (done!)

### Attacks feel too slow:
**Fix:** 
- Animator ? Attack states ? Speed: 1.3-1.5
- Or trim animation clip Start/End frames

### Animations facing wrong direction:
**Fix:**
- Root Transform Rotation ? Based Upon: Body Orientation

---

## ?? Animation Speeds

**Recommended speeds for responsive combat:**

```
Attack1: 1.4 (fast first hit)
Attack2: 1.3 (good flow)
Attack3: 1.1 (let finisher feel powerful)
AttackBow: 1.3 (quick shots)
SpellFireball: 1.2
SpellIce: 1.1
Hurt: 1.4 (quick reaction)
Run: 1.0-1.1
```

---

## ?? Animation Clips Needed

```
Movement:
? idle.fbx (loop)
? walk.fbx (loop)
? run.fbx (loop) ? MAX SPEED
? walk_hurt.fbx (loop)

Combo:
? attack1.fbx (first sword swing)
? attack2.fbx (second sword swing)
? attack3.fbx (BIG finisher swing)

Ranged:
? attack_bow.fbx (bow shoot)

Spells:
? spell_fireball.fbx
? spell_ice.fbx

Reactions:
? hurt.fbx (damage)
? dodge.fbx (dodge roll)
```

---

## ?? Pro Tips

**For best combo feel:**
- Attack1: 0.5-0.7 seconds
- Attack2: 0.6-0.8 seconds
- Attack3: 0.8-1.2 seconds (longer, more dramatic)

**Exit Time sweet spot:**
- 0.8 = Good for chaining combos
- 0.95 = Good for returning to idle

**Transition Duration:**
- 0.05 = Instant (combat starts)
- 0.1-0.15 = Smooth (combo chains)
- 0.2-0.3 = Soft (returning to idle)

---

## Summary

**Everything is ready to go!**

? Code: Complete
? Combo System: Implemented
? Run Animation: Auto-triggers
? Bow Re-trigger: Fixed
? Spells: Working
? All triggers: Auto-reset

**Just need to:**
1. Add parameters to Animator Controller
2. Create states with your animation clips
3. Setup transitions (follow guide above)
4. Test and adjust speeds

**Your combat system will feel AMAZING!** ???

See `COMPLETE_ANIMATION_SETUP.md` for full detailed guide.
