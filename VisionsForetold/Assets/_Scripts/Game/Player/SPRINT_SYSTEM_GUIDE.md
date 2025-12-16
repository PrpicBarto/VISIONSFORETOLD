# ?? Sprint System with Joystick Compatibility - Complete Guide

## Features Added

Your PlayerMovement now includes:
- ? **Sprint System** - Hold button to sprint
- ? **Full Joystick/Gamepad Support** - Works perfectly with controllers
- ? **Stamina System** - Optional stamina drain/regeneration
- ? **Movement Restrictions** - Control sprint direction (forward/strafe/backwards)
- ? **Animation Integration** - IsSprinting parameter for animator
- ? **Smooth Transitions** - Seamless sprint start/stop
- ? **Input Detection** - Auto-detects keyboard vs gamepad

## ?? Input Setup (Important!)

### Add Sprint Action to Input Actions Asset

```
1. Open your Input Actions asset
2. Find or create "Player" Action Map
3. Add new Action:
   - Name: "Sprint"
   - Action Type: Button (Value)
   - Control Type: Button

4. Add Bindings:
   
   Keyboard:
   - Path: <Keyboard>/leftShift
   - Or: <Keyboard>/space (alternative)
   
   Gamepad (Xbox):
   - Path: <Gamepad>/leftTrigger
   - Or: <Gamepad>/leftShoulder (LB)
   
   Gamepad (PlayStation):
   - Path: <DualShockGamepad>/leftTrigger (L2)
   - Or: <DualShockGamepad>/leftShoulder (L1)
```

### Connect to PlayerMovement

```csharp
// In your Input Actions script or PlayerInput component:

public void OnSprint(InputAction.CallbackContext context)
{
    playerMovement.OnSprint(context);
}
```

## ?? Inspector Settings

### Sprint Settings

| Setting | Default | Description |
|---------|---------|-------------|
| **Enable Sprint** | ? | Master toggle for sprint system |
| **Sprint Speed Multiplier** | 1.8 | Speed multiplier when sprinting (1.8 = 80% faster) |
| **Require Stamina** | ? | Enable stamina system |
| **Max Stamina** | 100 | Maximum stamina |
| **Stamina Drain Rate** | 20 | Stamina used per second while sprinting |
| **Stamina Regen Rate** | 15 | Stamina restored per second when not sprinting |
| **Stamina Regen Delay** | 1.0s | Delay before stamina starts regenerating |
| **Min Stamina To Sprint** | 10 | Minimum stamina needed to start sprinting |
| **Can Sprint Backwards** | ? | Allow sprinting while moving backwards |
| **Can Sprint Strafe** | ? | Allow sprinting while strafing left/right |

### Input Detection

| Setting | Default | Description |
|---------|---------|-------------|
| **Input Switch Delay** | 0.1s | Time to switch between input methods |
| **Joystick Deadzone** | 0.15 | Deadzone for joystick (0.15 = 15% deadzone) |

## ?? How It Works

### Sprint Mechanics

**Activation:**
- Hold Left Shift (Keyboard) or Left Trigger (Gamepad)
- Must be moving to sprint
- Requires minimum stamina (if enabled)

**Deactivation:**
- Release sprint button
- Stop moving
- Run out of stamina

**Speed:**
```
Normal Speed: moveSpeed (default 5)
Sprint Speed: moveSpeed × sprintSpeedMultiplier (5 × 1.8 = 9)
```

### Stamina System

**Drain:**
```
While Sprinting:
- Drains at staminaDrainRate per second (default 20/s)
- At default settings: 100 stamina = 5 seconds of sprint
```

**Regeneration:**
```
When Not Sprinting:
1. Wait for staminaRegenDelay (default 1s)
2. Regenerate at staminaRegenRate per second (default 15/s)
3. At default settings: Full stamina restored in ~7 seconds
```

**Minimum Stamina:**
- Must have `minStaminaToSprint` (default 10) to start sprinting
- Can continue sprinting until stamina reaches 0
- Prevents "stutter sprinting"

### Movement Restrictions

**Forward Sprint (Default):**
```
Can Sprint: Moving forward (W, Up, Joystick up)
Cannot Sprint: Moving backwards (S, Down, Joystick down)
```

**Strafe Sprint (Optional):**
```
Enable: canSprintStrafe = true
Allows: Sprinting while moving left/right (A/D, Left/Right, Joystick left/right)
```

**Backwards Sprint (Optional):**
```
Enable: canSprintBackwards = true
Allows: Sprinting while moving backwards (not recommended for realism)
```

## ?? Joystick Compatibility

### Full Gamepad Support

**Movement:**
- Left Stick: Movement with deadzone
- Analog support: Smooth acceleration based on stick position
- Sprint works at any movement speed

**Sprint:**
- Left Trigger (LT/L2): Hold to sprint
- Or Left Shoulder (LB/L1): Alternative binding
- Works exactly like keyboard hold

**Aiming:**
- Right Stick: Aim direction
- Aim while sprinting: Full support
- Camera follows smoothly

### Deadzone Handling

```csharp
// Joystick Deadzone (default 0.15 = 15%)
if (movementInput.magnitude < joystickDeadzone)
{
    movementInput = Vector2.zero; // Ignore small movements
}
else
{
    // Remap to full range
    float remapped = (magnitude - deadzone) / (1 - deadzone);
    movementInput = movementInput.normalized * remapped;
}
```

**Result:**
- No stick drift issues
- Smooth movement from center
- Full analog range preserved

### Input Detection

**Auto-Switch Between Inputs:**
```
Keyboard/Mouse Active:
- Sprint: Left Shift
- Aim: Mouse position

Gamepad Active:
- Sprint: Left Trigger
- Aim: Right Stick

Switches seamlessly based on last input!
```

## ?? Animation Integration

### Animator Parameters

Add these to your Animator Controller:

```
Parameter: IsSprinting
Type: Bool
Usage: Blend between walk/run/sprint animations

Parameter: Speed
Type: Float
Usage: Includes sprint multiplier (0 = idle, 1 = walk, 1.8 = sprint)
```

### Animation Setup Example

```
Animator Controller:
?? Locomotion (Blend Tree)
?  ?? Idle (Speed = 0)
?  ?? Walk (Speed = 1)
?  ?? Run/Sprint (Speed > 1.5)
?
?? Sprint Transition
?  Condition: IsSprinting = true
?  Transition to sprint animation
```

### Code Example

```csharp
// Automatically handled by PlayerMovement
animator.SetBool("IsSprinting", isSprinting);
animator.SetFloat("Speed", currentAnimationSpeed); // Includes sprint multiplier
```

## ?? Stamina UI Example

```csharp
using UnityEngine;
using UnityEngine.UI;

public class StaminaUI : MonoBehaviour
{
    [SerializeField] private Image staminaBar;
    [SerializeField] private PlayerMovement playerMovement;
    
    private void Update()
    {
        if (playerMovement != null && staminaBar != null)
        {
            staminaBar.fillAmount = playerMovement.StaminaPercentage;
            
            // Optional: Change color based on stamina
            if (playerMovement.StaminaPercentage < 0.2f)
            {
                staminaBar.color = Color.red; // Low stamina
            }
            else if (playerMovement.StaminaPercentage < 0.5f)
            {
                staminaBar.color = Color.yellow; // Medium stamina
            }
            else
            {
                staminaBar.color = Color.green; // Full stamina
            }
        }
    }
}
```

## ?? Public API

### Properties

```csharp
// Check sprint state
bool isSprinting = playerMovement.IsSprinting;

// Get stamina info
float currentStamina = playerMovement.CurrentStamina;
float maxStamina = playerMovement.MaxStamina;
float staminaPercent = playerMovement.StaminaPercentage; // 0-1
```

### Methods

```csharp
// Enable/disable sprint system
playerMovement.SetSprintEnabled(true);

// Modify stamina
playerMovement.ModifyStamina(20f);  // Add 20 stamina
playerMovement.ModifyStamina(-10f); // Remove 10 stamina

// Restore full stamina
playerMovement.RestoreStamina();
```

## ?? Configuration Presets

### Realistic (Default)

```
Sprint Speed Multiplier: 1.8
Max Stamina: 100
Drain Rate: 20/s (5 seconds of sprint)
Regen Rate: 15/s (~7 seconds to full)
Regen Delay: 1s
Can Sprint Backwards: false
Can Sprint Strafe: true
```

### Arcade

```
Sprint Speed Multiplier: 2.0 (faster)
Max Stamina: 150 (more stamina)
Drain Rate: 10/s (15 seconds of sprint)
Regen Rate: 25/s (fast recovery)
Regen Delay: 0.5s (quick recovery)
Can Sprint Backwards: true
Can Sprint Strafe: true
```

### No Stamina

```
Require Stamina: false
Sprint Speed Multiplier: 1.8
Can Sprint Backwards: false
Can Sprint Strafe: true
Result: Unlimited sprint
```

### Limited Sprint

```
Sprint Speed Multiplier: 1.5 (slower sprint)
Max Stamina: 50 (less stamina)
Drain Rate: 30/s (1.67 seconds of sprint)
Regen Rate: 10/s (slow recovery)
Regen Delay: 2s (long delay)
Can Sprint Backwards: false
Can Sprint Strafe: false (forward only)
```

## ?? Troubleshooting

### Sprint Not Working

**Check:**
1. Enable Sprint: ? (in Inspector)
2. Input Action: "Sprint" action added
3. Input Binding: Left Shift / Left Trigger bound
4. Stamina: Check if out of stamina
5. Movement: Must be moving to sprint

**Test:**
```
- Hold Left Shift while moving forward
- Check IsSprinting property in Inspector (should be true)
- Check CurrentStamina (should be draining)
```

### Gamepad Sprint Not Working

**Check:**
1. Left Trigger bound to "Sprint" action
2. Joystick Deadzone: Not too high (default 0.15)
3. Movement input: Left stick moving beyond deadzone
4. Input System: Package installed and active

**Test:**
```
- Push left stick forward (move)
- Hold left trigger (sprint)
- Check console for input detection
```

### Stamina Not Regenerating

**Check:**
1. Stamina Regen Rate: > 0
2. Stamina Regen Delay: Not too long
3. Not sprinting: Must stop sprinting to regen
4. Wait time: Must wait for delay before regen starts

**Fix:**
```
- Release sprint button completely
- Wait for staminaRegenDelay (default 1s)
- Stamina should start regenerating
```

### Animation Not Playing

**Check:**
1. IsSprinting parameter: Added to Animator Controller
2. Animator: Assigned in Inspector
3. Animation: Sprint animation set up in controller
4. Transitions: Conditions using IsSprinting = true

**Test:**
```
- Start sprinting
- Check IsSprinting in Animator window
- Check Speed parameter (should be > 1.8)
```

## ?? Best Practices

### Game Design

**For Action Games:**
```
- High sprint speed (2.0x)
- Moderate stamina (75-100)
- Fast regeneration
- Sprint in all directions
```

**For Survival Games:**
```
- Moderate sprint speed (1.5x)
- Limited stamina (50-75)
- Slow regeneration
- Forward sprint only
```

**For Stealth Games:**
```
- Low sprint speed (1.3x)
- High stamina (100-150)
- Fast regeneration
- Sprint disables stealth
```

### Performance

**Optimized Settings:**
- Joystick Deadzone: 0.15 (prevents jitter)
- Stamina checks: Every frame (necessary)
- Input detection: Cached (efficient)
- Animation updates: Smoothed (no jitter)

### Player Feedback

**Visual:**
- Stamina bar UI
- Sprint trail effect (particles)
- Camera FOV increase when sprinting
- Character lean forward animation

**Audio:**
- Breathing sounds when low stamina
- Footstep speed increase
- Sprint start/stop sounds

**Haptic (Gamepad):**
- Light vibration when stamina low
- Pulse when sprint starts/stops

## ?? Quick Setup Checklist

- [ ] Input Action "Sprint" created
- [ ] Left Shift bound (keyboard)
- [ ] Left Trigger bound (gamepad)
- [ ] Sprint settings configured
- [ ] Stamina values adjusted
- [ ] IsSprinting parameter in Animator
- [ ] Sprint animations set up
- [ ] Tested with keyboard
- [ ] Tested with gamepad
- [ ] UI for stamina (optional)
- [ ] Sound effects added (optional)

## ?? Summary

**What You Got:**
- ? Hold-to-sprint system
- ? Full gamepad/joystick support
- ? Stamina system with regeneration
- ? Movement direction restrictions
- ? Smooth input detection
- ? Animation integration
- ? Public API for external control
- ? Deadzone handling for controllers
- ? Auto-switching between input methods

**Controller Bindings:**
- Left Stick: Movement
- Right Stick: Aiming
- Left Trigger: Sprint
- Works exactly like keyboard!

**Your sprint system is production-ready with full joystick support!** ?????
