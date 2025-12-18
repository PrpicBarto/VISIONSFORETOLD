# ?? Animation System Architecture - Separation of Concerns

## Overview

The animation system now follows proper **separation of concerns** where:
- **PlayerMovement** = Animation trigger provider (how to trigger)
- **PlayerAttack** = Game logic controller (when to trigger)

---

## ? **Improved Architecture**

### PlayerMovement.cs - Animation Provider

**Responsibility:** Provide animation trigger methods

```csharp
// Just triggers animations - NO game logic checks
public void TriggerComboAttack(int comboStep)
{
    if (animator == null || animator.runtimeAnimatorController == null) return;
    
    animator.SetInteger(ComboStepHash, comboStep);
    
    switch (comboStep)
    {
        case 1: 
            animator.ResetTrigger(Attack1Hash);
            animator.SetTrigger(Attack1Hash);
            break;
        // etc...
    }
}
```

**Does:**
- ? Manages animator component
- ? Provides trigger methods
- ? Resets triggers to prevent issues
- ? Logs animation triggers for debugging

**Does NOT:**
- ? Check if player can attack
- ? Check dodge state
- ? Decide when to trigger animations
- ? Handle combat logic

---

### PlayerAttack.cs - Game Logic Controller

**Responsibility:** Decide when animations should play

```csharp
private void PerformMeleeAttack()
{
    // Game logic checks BEFORE triggering animation
    if (playerMovement != null && !playerMovement.IsDodging)
    {
        playerMovement.TriggerComboAttack(currentComboStep);
    }
    
    // Rest of attack logic (damage, etc.)
}
```

**Does:**
- ? Checks if player can attack
- ? Checks dodge state
- ? Decides when to trigger animations
- ? Handles all combat logic
- ? Manages cooldowns
- ? Deals damage

**Does NOT:**
- ? Directly access Animator component
- ? Manage animation parameters
- ? Handle animation state

---

## ?? **Call Flow**

### Melee Attack Example:

```
Player Input (Mouse Click)
    ?
PlayerAttack.PerformAttack()
    ?
PlayerAttack.PerformMeleeAttack()
    ?
CHECK: Is player dodging?
    ? NO
PlayerMovement.TriggerComboAttack(1)
    ?
Animator.SetTrigger(Attack1Hash)
    ?
Animation Plays!
```

### Bow Attack Example:

```
Player Input (Mouse Click)
    ?
PlayerAttack.PerformAttack()
    ?
PlayerAttack.PerformRangedAttack()
    ?
CHECK: Is player dodging?
    ? NO
PlayerMovement.TriggerAttackBow()
    ?
Animator.SetTrigger(AttackBowHash)
    ?
CHECK: Can fire arrow?
    ? YES
Fire Arrow Projectile
    ?
Animation + Projectile!
```

---

## ?? **Why This Is Better**

### Before (Wrong):
```csharp
// PlayerMovement.cs
public void TriggerAttack()
{
    if (!isDodging)  // ? PlayerMovement deciding game logic
    {
        animator.SetTrigger(AttackHash);
    }
}

// PlayerAttack.cs
private void PerformMeleeAttack()
{
    playerMovement.TriggerAttack();  // ? No control over when
}
```

**Problems:**
- ? PlayerMovement handles game logic (dodge check)
- ? PlayerAttack can't control animation timing
- ? Can't check other conditions (cooldowns, stamina, etc.)
- ? Violates single responsibility principle

---

### After (Correct):
```csharp
// PlayerMovement.cs
public void TriggerAttack()
{
    // ? Only handles animation triggering
    animator.SetTrigger(AttackHash);
}

// PlayerAttack.cs
private void PerformMeleeAttack()
{
    // ? PlayerAttack controls when animation plays
    if (playerMovement != null && !playerMovement.IsDodging)
    {
        playerMovement.TriggerAttack();
    }
}
```

**Benefits:**
- ? Clear separation of concerns
- ? PlayerAttack controls all combat logic
- ? Easy to add more checks (stamina, cooldowns, etc.)
- ? PlayerMovement is reusable for other systems
- ? Follows single responsibility principle

---

## ?? **Current Implementation**

### PlayerMovement.cs Methods:

**Animation Triggers (No Game Logic):**
```csharp
public void TriggerAttack()              // Generic attack
public void TriggerComboAttack(int step) // Combo system
public void TriggerAttackBow()           // Bow attack
public void TriggerSpellFireball()       // Fireball spell
public void TriggerSpellIce()            // Ice spell
public void TriggerHurt()                // Damage reaction
public void TriggerDash()                // Dash movement
public void ResetCombo()                 // Reset combo state
```

**Properties (For External Checks):**
```csharp
public bool IsDodging    // Check if dodging
public bool IsSprinting  // Check if sprinting
public bool IsRunning    // Check if at max speed
public bool IsLowHealth  // Check if health < 30%
```

---

### PlayerAttack.cs Checks:

**All combat methods now check:**
```csharp
if (playerMovement != null && !playerMovement.IsDodging)
{
    // Trigger animation
    playerMovement.TriggerComboAttack(step);
}
```

**Applied to:**
- ? PerformMeleeAttack() - Combo attacks
- ? PerformRangedAttack() - Bow attacks
- ? CastFireball() - Fireball spell
- ? CastIceBlast() - Ice spell

---

## ?? **Adding More Checks**

### Example: Adding Stamina Check

**PlayerAttack.cs:**
```csharp
private void PerformMeleeAttack()
{
    // Check multiple conditions
    if (playerMovement != null && 
        !playerMovement.IsDodging &&
        playerMovement.CurrentStamina >= attackStaminaCost)
    {
        playerMovement.TriggerComboAttack(currentComboStep);
        
        // Consume stamina
        playerMovement.ModifyStamina(-attackStaminaCost);
    }
    else
    {
        Debug.Log("Not enough stamina to attack!");
    }
    
    // Rest of attack logic...
}
```

**PlayerMovement doesn't need to change!**

---

### Example: Adding Attack State Check

**PlayerAttack.cs:**
```csharp
private void PerformRangedAttack()
{
    // Check if can perform action
    if (playerMovement != null && 
        !playerMovement.IsDodging &&
        !isCurrentlyAttacking &&
        Time.time >= lastAttackTime + attackCooldown)
    {
        playerMovement.TriggerAttackBow();
        isCurrentlyAttacking = true;
    }
    
    // Fire projectile...
}
```

---

## ?? **Best Practices**

### DO ?

**In PlayerMovement:**
```csharp
// Simple, focused methods
public void TriggerAttack()
{
    if (animator == null) return;
    animator.ResetTrigger(AttackHash);
    animator.SetTrigger(AttackHash);
}
```

**In PlayerAttack:**
```csharp
// All game logic checks
if (CanPerformAttack())
{
    playerMovement.TriggerAttack();
    DealDamage();
}

private bool CanPerformAttack()
{
    return playerMovement != null &&
           !playerMovement.IsDodging &&
           Time.time >= lastAttackTime + cooldown;
}
```

---

### DON'T ?

**In PlayerMovement:**
```csharp
// ? Don't check game logic
public void TriggerAttack()
{
    if (!isDodging &&  // ? Game logic check
        stamina >= 10)  // ? Combat logic check
    {
        animator.SetTrigger(AttackHash);
    }
}
```

**In PlayerAttack:**
```csharp
// ? Don't access Animator directly
private void PerformMeleeAttack()
{
    animator.SetTrigger(AttackHash);  // ? Bypass PlayerMovement
}
```

---

## ?? **Testing Your Changes**

### Verify Separation:

**1. Dodge During Attack:**
```
1. Start attacking
2. Press dodge mid-attack
3. Attack should NOT play while dodging ?
```

**2. Attack During Dodge:**
```
1. Start dodging
2. Press attack mid-dodge
3. Attack animation should NOT trigger ?
4. After dodge ends, can attack normally ?
```

**3. Rapid Attacks:**
```
1. Click attack rapidly
2. Animations should trigger properly ?
3. Cooldowns should be respected ?
```

---

## ?? **Architecture Summary**

```
???????????????????????????????????????????
?         PlayerAttack.cs                 ?
?  (Game Logic & Combat Controller)       ?
?                                         ?
?  - Checks game state                    ?
?  - Decides when to animate              ?
?  - Handles combat logic                 ?
?  - Manages cooldowns                    ?
?  - Deals damage                         ?
???????????????????????????????????????????
              ? Calls when appropriate
              ?
???????????????????????????????????????????
?       PlayerMovement.cs                 ?
?   (Animation Trigger Provider)          ?
?                                         ?
?  - Provides trigger methods             ?
?  - Manages Animator component           ?
?  - Resets triggers                      ?
?  - Updates animation parameters         ?
???????????????????????????????????????????
              ? Triggers
              ?
???????????????????????????????????????????
?         Animator Component              ?
?                                         ?
?  - Plays animations                     ?
?  - Manages state machine                ?
?  - Handles transitions                  ?
???????????????????????????????????????????
```

---

## Summary

**Key Changes:**
? PlayerMovement trigger methods now have NO game logic checks
? PlayerAttack checks `!playerMovement.IsDodging` before triggering
? Clean separation: PlayerAttack = when, PlayerMovement = how
? Easy to extend with more conditions (stamina, cooldowns, etc.)
? Follows Unity best practices and SOLID principles

**Benefits:**
- Clear, maintainable code
- Easy to debug
- Simple to extend
- Proper encapsulation
- Reusable components

**Your animation system now follows proper software architecture!** ???
