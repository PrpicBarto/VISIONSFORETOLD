# ? Dash Animation System - Complete Guide

## Overview

Your player now has a **full dash system** integrated with animations! Dash is a fast, aggressive movement ability that's separate from dodge.

---

## ? **What's Been Added**

### New Features:
- ? **Dash ability** - Quick burst movement in any direction
- ?? **Dash animation** - Auto-triggers when dashing
- ?? **Cooldown system** - 2 second cooldown between dashes
- ?? **Stamina cost** - Uses 25 stamina per dash
- ??? **Optional invulnerability** - Can be invulnerable during dash
- ?? **Direction control** - Dash in movement direction or forward
- ?? **Ground check** - Optional requirement to be grounded

---

## ?? **Dash vs Dodge Comparison**

### Dodge (Roll):
```
Purpose: Defensive, evasive
Distance: 5 units
Duration: 0.4 seconds
Cooldown: 1 second
Animation: Dodge/Roll
Invulnerable: Yes
```

### Dash (NEW):
```
Purpose: Aggressive, repositioning
Distance: 8 units (longer!)
Duration: 0.3 seconds (faster!)
Cooldown: 2 seconds
Stamina Cost: 25
Animation: Dash
Invulnerable: Optional (default: No)
```

---

## ?? **How to Use Dash**

### Default Controls:

**Keyboard + Mouse:**
```
Dash: [Not bound by default - add to Input Actions]
Suggested: Mouse Button 4/5, Q, or E
```

**Gamepad:**
```
Dash: [Not bound by default - add to Input Actions]
Suggested: Right Stick Click (R3) or Face Button
```

---

## ?? **Setup Instructions**

### Step 1: Add Input Action

**In Unity Input System:**
```
1. Open your Input Actions asset
2. Find "Player" action map
3. Add new action: "Dash"
4. Action Type: Button
5. Add bindings:
   - Keyboard: Q (or your preference)
   - Gamepad: Right Stick Press (R3)
6. Save
```

### Step 2: Add Animator Parameter

**In Animator Controller:**
```
1. Open Animator window
2. Parameters tab (left side)
3. Click "+" ? Trigger
4. Name it: "Dash"
5. Save
```

### Step 3: Create Dash Animation State

**In Animator:**
```
1. Right-click in Animator grid
2. Create State ? Empty
3. Rename to: "Dash"
4. Inspector ? Motion field
5. Assign your dash animation clip
```

### Step 4: Setup Transitions

**From Any State ? Dash:**
```
Condition: Dash (Trigger)
Has Exit Time: ?
Can Transition To Self: ?
Transition Duration: 0.05
Priority: Normal (or High if you want dash to interrupt attacks)
```

**From Dash ? Idle:**
```
Has Exit Time: ?
Exit Time: 0.95
Transition Duration: 0.2
```

---

## ?? **Dash Settings (Inspector)**

### PlayerMovement Component:

**Dash Settings:**
```
Enable Dash: ?
Dash Distance: 8 (how far to dash)
Dash Duration: 0.3 (how long dash takes)
Dash Cooldown: 2.0 (seconds between dashes)
Dash Curve: Linear (or customize for speed variation)
Dash Stamina Cost: 25
Require Stamina For Dash: ?
Can Dash In Air: ? (must be grounded)
Invulnerable During Dash: ? (or ? for safety)
```

### Recommended Settings:

**For Aggressive Combat:**
```
Dash Distance: 8-10
Dash Duration: 0.25
Dash Cooldown: 1.5
Stamina Cost: 20
Invulnerable: ?
```

**For Safe Repositioning:**
```
Dash Distance: 6-8
Dash Duration: 0.3
Dash Cooldown: 2.5
Stamina Cost: 30
Invulnerable: ?
```

**For Speedrunning/Mobility:**
```
Dash Distance: 12
Dash Duration: 0.2
Dash Cooldown: 1.0
Stamina Cost: 15
Can Dash In Air: ?
```

---

## ?? **Animation Setup**

### Dash Animation Requirements:

**Animation should be:**
```
Duration: 0.3-0.5 seconds
Type: Quick forward lunge/burst
Loop: ? (no loop)
Root Motion: Baked into pose

Import Settings:
? Animation Type: Humanoid
? Loop Time: ? (unchecked!)
? Root Transform Rotation: Bake Into Pose ?
? Root Transform Position (Y): Bake Into Pose ?
? Root Transform Position (XZ): Bake Into Pose ?
? Apply
```

### Suggested Animation Types:
```
- Quick forward lunge
- Burst dash
- Speed boost animation
- Slide animation
- Blink/teleport animation
```

---

## ?? **How Dash Works**

### Dash Direction Logic:

**If moving (WASD pressed):**
```
Dashes in movement direction
W = Forward
S = Backward
A = Left
D = Right
W+D = Forward-Right diagonal
```

**If not moving (no input):**
```
Dashes forward (character's facing direction)
```

### Dash Behavior:

**During Dash:**
```
1. Sprint cancelled
2. Movement input ignored
3. Dash animation plays
4. Moves in dash direction
5. Stamina consumed
6. Cooldown starts
```

**After Dash:**
```
1. Returns to previous state (Idle or Run)
2. Movement control restored
3. Can attack, dodge, sprint, etc.
4. Stamina regenerates (after delay)
```

---

## ?? **Testing Dash**

### Basic Test:
```
1. Enter Play Mode
2. Stand still
3. Press Dash key (Q or configured)
4. Character dashes forward ?
5. Dash animation plays ?
6. Try to dash again immediately ? Cooldown message ?
7. Wait 2 seconds, dash again ? Works ?
```

### Movement Direction Test:
```
1. Press W (move forward)
2. Press Dash ? Dashes forward ?
3. Press A (move left)
4. Press Dash ? Dashes left ?
5. Press S+D (move back-right)
6. Press Dash ? Dashes back-right diagonal ?
```

### Stamina Test:
```
1. Check stamina bar (100/100)
2. Dash ? Stamina drops to 75 ?
3. Dash 3 more times ? Stamina at 0 ?
4. Try to dash ? "Not enough stamina" message ?
5. Wait for stamina to regenerate
6. Can dash again ?
```

---

## ?? **Public API**

### Check Dash State:

```csharp
PlayerMovement movement = GetComponent<PlayerMovement>();

// Check if currently dashing
if (movement.IsDashing)
{
    Debug.Log("Player is dashing!");
}

// Get remaining cooldown
float cooldown = movement.GetDashCooldownRemaining();
if (cooldown > 0)
{
    Debug.Log($"Dash on cooldown: {cooldown:F1}s");
}
```

### Manually Trigger Dash:

```csharp
// Dash in a specific direction (for abilities)
Vector3 direction = transform.forward; // or any direction
movement.PerformDashInDirection(direction);

// Cancel dash early (for interrupts)
movement.CancelDash();
```

---

## ?? **Integration Examples**

### Dash Attack Combo:

```csharp
public class DashAttackAbility : MonoBehaviour
{
    private PlayerMovement movement;
    private PlayerAttack attack;
    
    void Update()
    {
        // Dash + Attack for special move
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (!movement.IsDashing && !movement.IsDodging)
            {
                // Dash forward
                movement.PerformDashInDirection(transform.forward);
                
                // Queue attack after dash
                Invoke(nameof(PerformDashAttack), movement.dashDuration);
            }
        }
    }
    
    void PerformDashAttack()
    {
        // Trigger powerful attack after dash
        attack.PerformAttack();
    }
}
```

### Dash UI Display:

```csharp
public class DashUI : MonoBehaviour
{
    [SerializeField] private Image dashCooldownImage;
    [SerializeField] private Text dashText;
    
    private PlayerMovement movement;
    
    void Update()
    {
        float cooldown = movement.GetDashCooldownRemaining();
        
        if (cooldown > 0)
        {
            dashCooldownImage.fillAmount = cooldown / movement.dashCooldown;
            dashText.text = $"Dash: {cooldown:F1}s";
        }
        else
        {
            dashCooldownImage.fillAmount = 0;
            dashText.text = "Dash: Ready";
        }
    }
}
```

### Dash Trail Effect:

```csharp
public class DashEffects : MonoBehaviour
{
    [SerializeField] private TrailRenderer dashTrail;
    [SerializeField] private ParticleSystem dashParticles;
    
    private PlayerMovement movement;
    
    void Update()
    {
        if (movement.IsDashing)
        {
            // Enable effects during dash
            dashTrail.emitting = true;
            if (!dashParticles.isPlaying)
                dashParticles.Play();
        }
        else
        {
            // Disable effects
            dashTrail.emitting = false;
            if (dashParticles.isPlaying)
                dashParticles.Stop();
        }
    }
}
```

---

## ?? **Common Issues**

### Dash doesn't trigger:

**Check:**
```
? Enable Dash: ? (checked in Inspector)
? Input Action "Dash" exists
? Input Action bound to a key
? Dash parameter exists in Animator
? Dash animation assigned to Dash state
? Any State ? Dash transition exists
? Enough stamina (25+ required)
? Not on cooldown (wait 2 seconds)
? Grounded (if Can Dash In Air is ?)
```

### Dash animation doesn't play:

**Check:**
```
? Dash state has animation clip assigned
? Animator parameter "Dash" exists as Trigger
? Any State ? Dash transition configured
? Console shows "[PlayerMovement] Dash started!" message
```

### Dash too short/long:

**Adjust:**
```
Dash Distance: 8 ? Change to 6 (shorter) or 12 (longer)
Dash Duration: 0.3 ? Keep similar for consistent speed
```

### Dash uses too much stamina:

**Adjust:**
```
Dash Stamina Cost: 25 ? Lower to 15 or 20
OR
Require Stamina For Dash: ? (disable stamina requirement)
```

### Can't dash while moving:

**Check:**
```
Console for error messages
Make sure not currently dashing or dodging
Check ground detection if Can Dash In Air is ?
```

---

## ?? **Dash Curve Customization**

### Animation Curve Options:

**Linear (Default):**
```
Constant speed throughout dash
Good for: Consistent, predictable movement
```

**Ease Out:**
```
Fast start, slow end
Good for: Aggressive dash that slows to stop
```

**Ease In:**
```
Slow start, fast end
Good for: Building momentum
```

**Ease In-Out:**
```
Slow start, fast middle, slow end
Good for: Smooth, cinematic dashes
```

**Custom:**
```
In Inspector:
1. Click Dash Curve
2. Curve editor opens
3. Add/move keyframes
4. Create your own speed profile!
```

---

## ?? **Dash Statistics**

### Default Values:
```
Distance: 8 units
Duration: 0.3 seconds
Speed: 26.7 units/second (fast!)
Cooldown: 2 seconds
Stamina Cost: 25 (25% of max)
```

### Comparison to Other Movement:
```
Walk Speed: 5 units/second
Sprint Speed: 9 units/second (5 * 1.8)
Dodge Speed: 12.5 units/second
Dash Speed: 26.7 units/second (FASTEST!)
```

---

## ?? **Pro Tips**

**Dash Combos:**
```
- Dash ? Attack (gap closer)
- Dash ? Spell (reposition then cast)
- Attack ? Dash (hit and run)
- Dodge ? Dash (chain mobility)
```

**Stamina Management:**
```
- Don't spam dash (high stamina cost)
- Use for critical moments
- Combine with sprint for max mobility
- Save stamina for emergencies
```

**Tactical Uses:**
```
? Close distance to enemy
? Escape danger quickly
? Reposition during combat
? Dodge ground effects/AOEs
? Chase fleeing enemies
? Cross gaps/hazards
```

---

## ?? **Advanced: Dash Variants**

### Create Different Dash Types:

**Heavy Dash (Knockback):**
```csharp
public void PerformHeavyDash()
{
    // Dash with extra distance and knockback
    dashDistance = 12f;
    dashStaminaCost = 40f;
    
    // Add knockback on collision
    // Add damage on collision
}
```

**Quick Dash (Combat):**
```csharp
public void PerformQuickDash()
{
    // Short, fast dash for combat
    dashDistance = 5f;
    dashDuration = 0.2f;
    dashStaminaCost = 15f;
}
```

**Blink (Teleport):**
```csharp
public void PerformBlink()
{
    // Instant teleport (no animation)
    Vector3 targetPos = transform.position + dashDirection * dashDistance;
    transform.position = targetPos;
    
    // Play teleport effects
}
```

---

## Summary

**Your Dash System:**
- ? Fast, aggressive movement ability
- ?? Fully animated with auto-triggers
- ?? 2 second cooldown
- ?? 25 stamina cost
- ?? Direction-based (movement input or forward)
- ??? Optional invulnerability
- ?? Ground check optional
- ?? Easy to customize

**Setup Steps:**
1. Add "Dash" input action
2. Add "Dash" Animator parameter (Trigger)
3. Create Dash animation state
4. Setup Any State ? Dash transition
5. Configure settings in Inspector
6. Test!

**Your dash system is ready for fast-paced action!** ??
