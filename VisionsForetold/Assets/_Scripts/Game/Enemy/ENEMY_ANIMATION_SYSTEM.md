# ?? Enemy Animation System - Complete Guide

## Overview

All enemies now have a **unified animation system** with idle, run, attack, and death animations built into the BaseEnemy class!

---

## ? **What's Been Implemented**

### BaseEnemy Features:
- ?? **Automatic Animation Updates** - Updates every frame
- ?? **Run Animation** - Plays when moving
- ?? **Idle Animation** - Plays when standing still
- ?? **Attack Animation** - Triggered by attacks
- ?? **Death Animation** - Plays when enemy dies
- ?? **Speed Parameter** - Smooth speed transitions (0-1)
- ?? **Toggle System** - Can disable animations per enemy

---

## ?? **Required Animator Parameters**

### Add These to ALL Enemy Animator Controllers:

```
Bools:
+ IsMoving ? True when enemy is moving

Floats:
+ Speed ? Movement speed (0.0 to 1.0 normalized)

Triggers:
+ Attack ? Triggers attack animation
+ Death ? Triggers death animation
```

---

## ?? **Animator Setup (All Enemies)**

### Step 1: Create Animator Controller

**For each enemy type:**
```
1. Right-click in Project ? Create ? Animator Controller
2. Name it: "[EnemyName]Animator" (e.g., "GhoulAnimator")
3. Drag onto enemy prefab
```

---

### Step 2: Add Parameters

**Open Animator window:**
```
Parameters tab:
1. + ? Bool ? "IsMoving"
2. + ? Float ? "Speed"
3. + ? Trigger ? "Attack"
4. + ? Trigger ? "Death"
```

---

### Step 3: Create Animation States

**Required states:**
```
1. Idle (default state, orange)
2. Run
3. Attack
4. Death
```

**Create them:**
```
Right-click ? Create State ? Empty
Rename each state accordingly
```

---

### Step 4: Assign Animation Clips

**For each state:**
```
Click state ? Inspector ? Motion field ? Drag animation clip
```

---

### Step 5: Setup Transitions

**Movement Transitions:**

**Idle ? Run:**
```
Condition: IsMoving == true
Has Exit Time: ?
Transition Duration: 0.15
```

**Run ? Idle:**
```
Condition: IsMoving == false
Has Exit Time: ?
Transition Duration: 0.15
```

**Attack Transitions:**

**Any State ? Attack:**
```
Condition: Attack (Trigger)
Has Exit Time: ?
Can Transition To Self: ?
Transition Duration: 0.05
```

**Attack ? Idle:**
```
Has Exit Time: ?
Exit Time: 0.9
Transition Duration: 0.15
```

**Death Transitions:**

**Any State ? Death:**
```
Condition: Death (Trigger)
Has Exit Time: ?
Can Transition To Self: ?
Transition Duration: 0.05
Priority: Highest
```

**Death ? (No Return):**
```
Death is final state
No transition out needed
```

---

## ?? **Animation Import Settings**

### For ALL Enemy Animations:

**Select animation FBX ? Inspector:**

```
Rig Tab:
- Animation Type: Humanoid (or Generic)
- Avatar: Create From This Model (or Copy)
- Apply

Animation Tab:
Loop Time Settings:
  ? Idle (loop)
  ? Run (loop)
  ? Attack (no loop)
  ? Death (no loop)

- Loop Pose: ? (for looping animations)
- Root Transform Rotation: Bake Into Pose ?
- Root Transform Position (Y): Bake Into Pose ?
- Root Transform Position (XZ): Bake Into Pose ?
- Apply
```

---

## ?? **How It Works**

### Automatic System:

**BaseEnemy handles everything:**
```csharp
protected virtual void Update()
{
    // AI behavior
    UpdateBehavior(distanceToPlayer);
    
    // Animations updated automatically
    if (useAnimations)
    {
        UpdateAnimations();
    }
}
```

**No manual animation code needed in child classes!**

---

## ?? **Animation States by Behavior**

### Idle State:
```
Enemy waiting/patrolling
NavMeshAgent velocity < 0.1
IsMoving: false
Speed: 0.0
Animation: Idle loops
```

### Chasing State:
```
Enemy detected player
NavMeshAgent moving toward target
IsMoving: true
Speed: 0.0 ? 1.0 (smooth)
Animation: Idle ? Run (smooth blend)
```

### Attacking State:
```
Enemy in attack range
Attack method called
Attack trigger fires
Animation: Current ? Attack
After attack: Attack ? Idle
```

### Death State:
```
Health reaches 0
OnDeath() called
Death trigger fires
Animation: Current ? Death
Agent disabled
Enemy stops all behavior
```

---

## ?? **Implemented Enemies**

### ? Ghoul (Updated)
```
Animations: Idle, Run, Attack
Types: Basic, Strong, Fast
Attack: Melee bite
Cooldown: 1.5 seconds
```

### ? Wraith (Updated)
```
Animations: Idle, Run, Attack
Types: Aggressive, Ranged
Attack: Rapid claws or Magic explosion
Cooldown: 0.3 seconds (rapid)
```

### ?? Need Update (Same Pattern)

**For each enemy, add to their attack methods:**
```csharp
// In attack method
TriggerAttackAnimation();

// Example:
private void PerformAttack()
{
    TriggerAttackAnimation(); // ? Add this
    
    // Your damage code...
    Health playerHealth = player.GetComponent<Health>();
    if (playerHealth != null)
    {
        playerHealth.TakeDamage(damage);
    }
}
```

---

## ?? **Inspector Settings**

### BaseEnemy Component (All Enemies):

**Animation Settings:**
```
Animation Smooth Time: 0.1
- Lower (0.05) = Snappier
- Higher (0.2) = Smoother

Use Animations: ?
- Check to enable animations
- Uncheck to disable (for testing)
```

---

## ?? **Speed Parameter Usage**

### What It Does:

**Normalized Speed (0-1):**
```
0.0 = Idle (not moving)
0.5 = Half speed (slowing down/speeding up)
1.0 = Full speed (max movement)
```

### Uses:

**Can be used for:**
- Blend trees (walk ? run)
- Speed variations per enemy type
- Animation speed multiplier
- Visual feedback

**Example: Fast Ghoul**
```
Move Speed: 4.5
Animation: Run plays at 1.2x speed (faster)
```

---

## ?? **Enemy-Specific Animation Speeds**

### Adjusting Per Enemy Type:

**In Animator (per enemy):**
```
1. Click Run state
2. Inspector ? Speed: 1.0
3. Adjust per type:
   - Basic: 1.0 (normal)
   - Fast: 1.2 (faster run)
   - Strong/Slow: 0.8 (slower run)
```

**Examples:**

**Ghoul:**
```
Basic: Run Speed 1.0
Strong: Run Speed 0.9 (heavier)
Fast: Run Speed 1.3 (aggressive)
```

**Wraith:**
```
Aggressive: Run Speed 1.1 (quick)
Ranged: Run Speed 0.9 (cautious)
```

---

## ?? **Complete Setup Checklist**

### For Each Enemy Type:

```
Animator Setup:
? Create AnimatorController
? Add parameters: IsMoving, Speed, Attack, Death
? Create states: Idle, Run, Attack, Death
? Assign animation clips
? Setup transitions (see above)
? Import animations correctly

Enemy Prefab:
? Assign Animator component
? Assign AnimatorController
? Verify BaseEnemy references set
? Test in Play Mode

Code (If Custom Attack):
? Call TriggerAttackAnimation() in attack method
? Ensure base.Update() is called
? Don't override UpdateAnimations() unless needed
```

---

## ?? **Child Class Implementation**

### Minimal Implementation (Recommended):

```csharp
public class YourEnemy : BaseEnemy
{
    protected override void Awake()
    {
        base.Awake(); // Always call base
        // Your initialization
    }
    
    protected override void Update()
    {
        base.Update(); // Always call base - handles animations
        // Your custom update code (if any)
    }
    
    protected override void UpdateBehavior(float distanceToPlayer)
    {
        // Your AI logic
        if (distanceToPlayer > attackRange)
        {
            agent.SetDestination(player.position);
        }
        else
        {
            agent.SetDestination(transform.position);
            TryAttack();
        }
    }
    
    private void TryAttack()
    {
        if (CanAttack())
        {
            TriggerAttackAnimation(); // ? Use base class method
            
            // Your damage logic
            DamagePlayer();
        }
    }
}
```

**That's it! Animations handled automatically!**

---

## ?? **Advanced: Custom Animation Override**

### If You Need Custom Animation Logic:

```csharp
protected override void UpdateAnimations()
{
    // Call base for standard behavior
    base.UpdateAnimations();
    
    // Add custom animation parameters
    if (animator != null)
    {
        animator.SetBool("IsFlying", isFlying);
        animator.SetFloat("HealthPercent", health.HealthPercentage);
    }
}
```

---

## ?? **Common Issues**

### Animations don't play:

**Check:**
```
? Animator component assigned
? AnimatorController assigned
? Animation clips assigned to states
? Parameters exist and spelled correctly
? Transitions configured
? useAnimations = true (default)
? base.Update() called in child class
```

---

### Enemy slides without animation:

**Debug:**
```csharp
// Add to child class temporarily
protected override void UpdateAnimations()
{
    base.UpdateAnimations();
    Debug.Log($"{name} - IsMoving: {agent.velocity.magnitude > 0.1f}, Speed: {currentAnimationSpeed}");
}
```

**Check:**
```
? NavMeshAgent working
? Agent on NavMesh
? IsMoving parameter updating
? Run animation assigned
? Idle ? Run transition works
```

---

### Attack animation doesn't play:

**Check:**
```
? TriggerAttackAnimation() called
? Attack parameter is Trigger (not Bool)
? Any State ? Attack transition exists
? Has Exit Time unchecked on Any ? Attack
? Can Transition To Self unchecked
? Attack animation assigned
```

---

### Death animation doesn't play:

**Check:**
```
? Health component connected
? OnDeath event fires
? Death parameter exists
? Any State ? Death transition exists
? Death animation assigned
? Death transition priority is high
```

---

## ?? **Animation Recommendations**

### Ideal Animation Lengths:

**Idle:**
```
Duration: 2-4 seconds
Loop: ?
Type: Breathing, idle motion
```

**Run:**
```
Duration: 0.5-1.0 second cycle
Loop: ?
Type: Running/floating motion
```

**Attack:**
```
Duration: 0.5-1.0 seconds
Loop: ?
Type: Swipe, bite, cast, etc.
Damage: Middle of animation
```

**Death:**
```
Duration: 1.0-2.0 seconds
Loop: ?
Type: Collapse, disintegrate, fade
No return transition
```

---

## ?? **Console Debug Messages**

### What You'll See:

**Normal operation:**
```
(Animations update silently)
```

**On attack:**
```
Ghoul (Basic) bit player for 15 damage!
```

**On death:**
```
(Enemy stops all behavior, death animation plays)
```

**If errors:**
```
Check that all references are assigned
Verify AnimatorController exists
```

---

## ?? **Enemy Animation Matrix**

### Standard Enemies:

| Enemy | Idle | Run | Attack | Death | Special |
|-------|------|-----|--------|-------|---------|
| Ghoul | ? | ? | Bite | ? | 3 types |
| Wraith | ? | ? | Claw/Magic | ? | Rapid/Ranged |
| Skeleton | ? | ? | Sword | ? | - |
| Zombie | ? | ? | Grab | ? | - |
| Ghost | ? | Float | Possess | ? | Flying |

**All use same parameter setup!**

---

## ?? **Pro Tips**

**Animation Speed:**
```
Fast enemies: Animation Speed 1.2-1.5
Normal enemies: Animation Speed 1.0
Slow enemies: Animation Speed 0.7-0.9
```

**Smooth Transitions:**
```
Animation Smooth Time: 0.1 (default)
Lower for responsive, snappy feel
Higher for smooth, natural motion
```

**Attack Timing:**
```
Damage should occur mid-animation
Attack cooldown > animation length
Prevents spam, looks professional
```

**Death Cleanup:**
```
Disable collider on death
Disable agent on death (done)
Destroy GameObject after death animation
Or: Fade out and pool
```

**Visual Polish:**
```
Add particle effects on attack
Add sound effects per animation
Add hit reactions
Add footstep sounds to run
Makes enemies feel alive!
```

---

## Summary

**Your Enemy Animation System:**
```
? BaseEnemy handles all animations
? Idle, Run, Attack, Death for all enemies
? Smooth speed transitions
? Automatic state management
? Easy to extend per enemy
? Unified parameter system
? Toggle per enemy if needed
? Professional, polished behavior
```

**Required Setup Per Enemy:**
```
1. Create Animator Controller
2. Add 4 parameters (IsMoving, Speed, Attack, Death)
3. Create 4 states (Idle, Run, Attack, Death)
4. Assign animation clips
5. Setup transitions
6. Test in Play Mode
```

**Child Class Requirements:**
```
1. Call base.Awake()
2. Call base.Update()
3. Call TriggerAttackAnimation() when attacking
4. That's it!
```

**Result:**
Professional enemy AI with smooth, responsive animations across all enemy types! ???????

**Your unified enemy animation system is complete and ready to use!**
