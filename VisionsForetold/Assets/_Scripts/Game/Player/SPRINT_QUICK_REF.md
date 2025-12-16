# ?? Sprint System - Quick Reference

## Input Setup (Required!)

### Add to Input Actions:
```
Action: "Sprint"
Type: Button
Keyboard: Left Shift
Gamepad: Left Trigger (LT/L2)
```

### Connect in Code:
```csharp
public void OnSprint(InputAction.CallbackContext context)
{
    playerMovement.OnSprint(context);
}
```

## Default Settings

| Setting | Value | Result |
|---------|-------|--------|
| Sprint Speed | 1.8x | 80% faster |
| Max Stamina | 100 | Good balance |
| Drain Rate | 20/s | 5 seconds sprint |
| Regen Rate | 15/s | ~7 seconds recovery |
| Regen Delay | 1s | Quick recovery |
| Min Stamina | 10 | Prevents stutter |

## Controller Support

**Xbox:**
- Movement: Left Stick
- Sprint: Left Trigger (hold)
- Aim: Right Stick

**PlayStation:**
- Movement: Left Stick
- Sprint: L2 (hold)
- Aim: Right Stick

**Switch:**
- Movement: Left Stick
- Sprint: ZL (hold)
- Aim: Right Stick

## Quick Configurations

### Unlimited Sprint:
```
Require Stamina: false
Sprint Speed: 1.8x
```

### Limited Sprint:
```
Max Stamina: 50
Drain Rate: 30/s
Regen Rate: 10/s
```

### Fast Sprint:
```
Sprint Speed: 2.0x
Max Stamina: 150
Regen Rate: 25/s
```

## Public API

```csharp
// Check state
bool sprinting = playerMovement.IsSprinting;
float stamina = playerMovement.CurrentStamina;
float percent = playerMovement.StaminaPercentage;

// Control
playerMovement.SetSprintEnabled(true);
playerMovement.ModifyStamina(20f);
playerMovement.RestoreStamina();
```

## Animation Setup

```
Animator Parameters:
- IsSprinting (Bool)
- Speed (Float) - includes sprint multiplier
```

## Troubleshooting

| Issue | Solution |
|-------|----------|
| Not sprinting | Check Enable Sprint ? |
| No gamepad sprint | Bind Left Trigger to Sprint |
| No stamina regen | Wait for regen delay (1s) |
| Animation issues | Add IsSprinting parameter |

## Movement Restrictions

```
Forward: ? Always allowed
Strafe: ? Default allowed (canSprintStrafe)
Backwards: ? Default disabled (canSprintBackwards)
```

Change in Inspector Sprint Settings!

**Full joystick compatibility included!** ???
