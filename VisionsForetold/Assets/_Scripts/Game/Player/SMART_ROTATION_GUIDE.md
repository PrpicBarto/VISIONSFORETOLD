# ?? Smart Rotation System - Movement + Combat Guide

## Overview

Your player now has an **intelligent rotation system** that automatically switches between:
- **Movement Rotation** - Rotates towards movement direction when exploring
- **Combat Rotation** - Rotates towards aim target when attacking/shooting

---

## ? **How It Works**

### Automatic Rotation Modes:

**When NOT Attacking:**
```
Player moves ? Rotates towards movement direction
W = Face forward
S = Face backward
A = Face left
D = Face right
W+D = Face forward-right diagonal

Natural, intuitive movement!
```

**When Attacking/Shooting:**
```
Player attacks ? Automatically rotates to aim target
Stays facing aim target for 0.5 seconds after attack
Allows strafing while keeping enemy in sight

Tactical combat positioning!
```

---

## ?? **Behavior Examples**

### Exploration:
```
1. Press W ? Run forward, facing forward
2. Press A ? Strafe left, rotate to face left
3. Press S ? Backpedal, rotate to face backward
4. Natural 3rd person movement!
```

### Combat:
```
1. Move with WASD ? Face movement direction
2. Aim at enemy with mouse
3. Click attack ? Instantly rotate to face enemy
4. Keep attacking ? Stay facing enemy
5. Stop attacking ? After 0.5s, return to movement rotation
6. Can strafe around enemy while shooting!
```

### Advanced Combat:
```
1. Run forward (W)
2. Aim at enemy to your right
3. Attack ? Character rotates to face enemy
4. Keep holding W ? Run forward but face enemy
5. Circle strafe around enemy while attacking!
```

---

## ?? **Settings (Inspector)**

### PlayerMovement Component:

**Rotation Behavior:**
```
Always Rotate Towards Aim: ?/? (legacy setting, usually ?)
Rotate Towards Movement When No Aim: ? (legacy)
Rotate To Movement By Default: ? ? NEW! Enable smart rotation
Attack Rotation Duration: 0.5 ? How long to aim after attack
Min Aim Distance: 0.1
Rotation Speed: 720 (degrees per second)
```

### Recommended Settings:

**For Action Combat (Recommended):**
```
Rotate To Movement By Default: ?
Attack Rotation Duration: 0.5
Always Rotate Towards Aim: ?
Rotation Speed: 720
```

**For Traditional Top-Down (Old Behavior):**
```
Rotate To Movement By Default: ?
Always Rotate Towards Aim: ?
Attack Rotation Duration: 0.5
```

**For Precise Shooting:**
```
Rotate To Movement By Default: ?
Attack Rotation Duration: 1.0 (longer aim time)
Rotation Speed: 900 (faster rotation)
```

---

## ?? **Rotation Logic**

### Priority System:

```
1. Is Attacking? ? Rotate to Aim Target
2. Just Attacked? (within 0.5s) ? Rotate to Aim Target
3. Moving? ? Rotate to Movement Direction
4. Standing Still? ? Keep current rotation
```

### Attack State Tracking:

**Automatically tracked for:**
```
? Melee attacks (all 3 combo hits)
? Bow attacks
? Fireball spell
? Ice spell
? Any attack that triggers animation
```

**Duration:**
```
Attacking State: Active during attack animation
Post-Attack: Active for 0.5 seconds after attack
Total Time: ~1-1.5 seconds of aim rotation per attack
```

---

## ?? **Code Integration**

### Automatic (Already Done):

All attack triggers automatically set attacking state:

```csharp
// When you call these, rotation is handled automatically:
playerMovement.TriggerComboAttack(1);  // Melee
playerMovement.TriggerAttackBow();     // Bow
playerMovement.TriggerSpellFireball(); // Fireball
playerMovement.TriggerSpellIce();      // Ice
```

### Manual Control (Advanced):

```csharp
// Manually control attack rotation state
PlayerMovement movement = GetComponent<PlayerMovement>();

// Enter attack rotation mode
movement.SetAttackingState(true);

// Exit attack rotation mode
movement.SetAttackingState(false);

// Check if in attack rotation mode
if (movement.IsInAttackRotationMode())
{
    Debug.Log("Currently rotating to aim target");
}
```

---

## ?? **Usage Examples**

### Check Rotation Mode:

```csharp
void Update()
{
    if (playerMovement.IsAttacking)
    {
        // Currently attacking - facing aim target
        Debug.Log("Combat rotation active");
    }
    else
    {
        // Not attacking - facing movement direction
        Debug.Log("Movement rotation active");
    }
}
```

### Custom Attack with Rotation:

```csharp
public void PerformSpecialAttack()
{
    // Trigger attack rotation
    playerMovement.SetAttackingState(true);
    
    // Your attack logic
    Debug.Log("Special attack!");
    
    // Rotation will automatically return to movement after 0.5s
}
```

### Extended Aim Duration:

```csharp
public void PerformChargedAttack()
{
    // Enter attack state
    playerMovement.SetAttackingState(true);
    
    // Perform attack
    DoChargedAttack();
    
    // Keep aiming for longer
    StartCoroutine(ExtendAimTime(2.0f));
}

IEnumerator ExtendAimTime(float duration)
{
    yield return new WaitForSeconds(duration);
    playerMovement.SetAttackingState(false);
}
```

---

## ?? **Behavior Details**

### Movement Rotation:

**Rotates to match movement input:**
```
Forward (W): 0° (facing forward)
Right (D): 90° (facing right)
Backward (S): 180° (facing backward)
Left (A): 270° (facing left)
Forward-Right (W+D): 45° (diagonal)
```

**Rotation Speed:**
```
Default: 720 degrees/second
Time to 180° turn: 0.25 seconds
Smooth, responsive rotation
```

### Attack Rotation:

**Rotates to aim target:**
```
Mouse position ? Raycasts to world
Aim target placed at hit point
Character rotates to face target
Ignores Y-axis (flat rotation)
```

**Transition:**
```
Movement ? Attack: Instant
Attack ? Movement: 0.5 second delay
Smooth blending between modes
```

---

## ?? **Customization**

### Adjust Rotation Timing:

**In Inspector:**
```
Attack Rotation Duration: 0.5
- 0.0 = Instant return to movement rotation
- 0.5 = Default, good balance
- 1.0 = Longer aim time, better for precise shooting
- 2.0 = Very long aim, for sniping
```

**In Code:**
```csharp
// Access the setting
float duration = playerMovement.attackRotationDuration;

// Change at runtime
playerMovement.attackRotationDuration = 1.0f; // Longer aim
```

### Adjust Rotation Speed:

**In Inspector:**
```
Rotation Speed: 720
- 360 = Slow, cinematic
- 720 = Default, responsive
- 1080 = Fast, arcade-style
- 1800 = Very fast, instant-feeling
```

---

## ?? **Animation Integration**

### Works With All Animations:

**Movement Animations:**
```
Idle, Run, Walk
? Face movement direction
? Natural locomotion
```

**Attack Animations:**
```
Attack1, Attack2, Attack3 (Combo)
AttackBow, SpellFireball, SpellIce
? Face aim target
? Track enemy during attack
```

**Special Animations:**
```
Dodge, Dash
? Use dash/dodge direction
? Rotation maintained
```

---

## ?? **Common Questions**

### Why doesn't character rotate to aim when not attacking?

**Answer:**
```
By design! New system rotates to movement by default.
Only rotates to aim when attacking.

If you want always aim:
Rotate To Movement By Default: ?
Always Rotate Towards Aim: ?
```

### Character keeps rotating after attack:

**Answer:**
```
Working as intended - 0.5 second aim duration.
To change:
Attack Rotation Duration: 0.0 (instant return)
OR
Attack Rotation Duration: 0.2 (shorter)
```

### Want to face aim target while moving without attacking:

**Use manual control:**
```csharp
// Enter aim mode
playerMovement.SetAttackingState(true);

// Move around while facing aim
// ...

// Exit aim mode
playerMovement.SetAttackingState(false);
```

---

## ?? **Comparison: Old vs New**

### Old System (Always Aim):
```
? Character always faces aim target
? Awkward movement (backpedaling while facing forward)
? No natural exploration
? Good for twin-stick shooters
```

### New System (Smart Rotation):
```
? Natural movement rotation
? Combat rotation when needed
? Best of both worlds
? Perfect for action RPGs
? Strafe around enemies while shooting
```

---

## ?? **Best Practices**

### For Action Combat:
```
1. Enable "Rotate To Movement By Default"
2. Set Attack Rotation Duration: 0.5
3. Rotation Speed: 720-900
4. Let system handle rotation automatically
```

### For Twin-Stick Style:
```
1. Disable "Rotate To Movement By Default"
2. Enable "Always Rotate Towards Aim"
3. Manual attack state control if needed
```

### For Hybrid Approach:
```
1. Enable "Rotate To Movement By Default"
2. Longer Attack Rotation Duration: 1.0-2.0
3. Use manual SetAttackingState() for abilities
```

---

## ?? **Pro Tips**

**Strafe Shooting:**
```
1. Aim at enemy
2. Attack (character faces enemy)
3. Strafe left/right with A/D
4. Character moves sideways while facing enemy
5. Perfect for combat positioning!
```

**Quick Turn:**
```
1. Running forward
2. Press S (backward)
3. Character quickly turns 180°
4. Now running backward
5. Great for retreating!
```

**Combat Movement:**
```
1. Circle enemy with A/D
2. Attack when ready
3. Character auto-aims
4. Keep circling while attacking
5. Advanced combat technique!
```

---

## ?? **Debugging**

### Check Rotation Mode:

**Console Logs:**
```
[PlayerMovement] Entered attacking state - rotating to aim target
[PlayerMovement] Exited attacking state - rotating to movement direction
```

**In Code:**
```csharp
Debug.Log($"Is Attacking: {playerMovement.IsAttacking}");
Debug.Log($"In Attack Rotation: {playerMovement.IsInAttackRotationMode()}");
Debug.Log($"Time Since Attack: {Time.time - playerMovement.lastAttackTime}");
```

### Visual Debug:

**Gizmos show:**
```
Red Ray: Aim direction (to aim target)
Green Ray: Forward direction
Yellow Ray: When sprinting
Cyan Ray: When dashing
```

---

## Summary

**Your Smart Rotation System:**
- ?? Rotates to movement direction by default
- ?? Automatically switches to aim rotation when attacking
- ?? Stays aimed for 0.5s after attack
- ?? Fully automatic, no manual control needed
- ?? Highly customizable via Inspector
- ? Perfect for action RPG combat

**Key Settings:**
```
Rotate To Movement By Default: ?
Attack Rotation Duration: 0.5
Rotation Speed: 720
```

**Behavior:**
```
Not Attacking ? Face movement direction (natural)
Attacking ? Face aim target (combat)
Post-Attack ? Face aim for 0.5s, then movement
```

**Result:**
Natural exploration movement + Precise combat aiming = Perfect!

**Your rotation system intelligently adapts to player actions!** ???
