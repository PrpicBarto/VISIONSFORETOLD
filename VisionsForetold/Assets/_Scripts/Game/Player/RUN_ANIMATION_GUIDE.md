# ?? Run Animation System Guide

## Overview

The Run animation is automatically triggered when the player moves at maximum speed (Speed > 1.2). This creates smooth transitions between walking and running based on player velocity.

## How It Works

### Speed Threshold System

```
Speed Value ? Animation State
0.0        ? Idle
0.1 - 1.0  ? Walk
1.2 - 1.8  ? Run (max speed)
```

**Speed Calculation:**
- Base speed: `movementInput.magnitude` (0-1)
- With Sprint: `movementInput.magnitude * sprintSpeedMultiplier` (0-1.8)

### Automatic Detection

The system automatically sets the `IsRunning` bool based on:
```csharp
bool isRunning = isMoving && currentAnimationSpeed > 1.2f;
```

**When IsRunning becomes true:**
- Player is moving (input magnitude > 0.1)
- Current animation speed exceeds 1.2
- This typically happens when:
  - Sprinting at full speed
  - Moving at maximum velocity
  - Pushing movement stick fully forward

## Animator Setup

### Method 1: Using IsRunning Bool (Recommended)

**Parameters Needed:**
```
IsMoving   (Bool)  - Player is moving
IsRunning  (Bool)  - Player is running at max speed
Speed      (Float) - Current speed value
```

**States:**
```
?? Idle
?? Walk
?? Run
```

**Transitions:**
```
Idle ? Walk
Condition: IsMoving == true
Has Exit Time: false
Transition Duration: 0.1s

Walk ? Run
Condition: IsRunning == true
Has Exit Time: false
Transition Duration: 0.15s

Run ? Walk
Condition: IsRunning == false
Has Exit Time: false
Transition Duration: 0.15s

Walk ? Idle
Condition: IsMoving == false
Has Exit Time: false
Transition Duration: 0.1s

Run ? Idle
Condition: IsMoving == false
Has Exit Time: false
Transition Duration: 0.2s
```

### Method 2: Using Blend Tree (Advanced)

**Create a 1D Blend Tree using Speed parameter:**

```
Movement Blend Tree (Parameter: Speed)
?? 0.0   ? Idle
?? 0.5   ? Walk (slow)
?? 1.0   ? Walk (full speed)
?? 1.2   ? Run (start of run)
?? 1.8   ? Run (max sprint)

Settings:
- Blend Type: 1D
- Parameter: Speed
- Automatic Thresholds: Off (manual control)
```

**Advantages:**
- Smooth transitions between all speeds
- No hard transitions
- Natural speed ramping

**Disadvantages:**
- Requires more animation clips at different speeds
- More complex to set up

## Integration with Sprint System

### Sprint Speed Multiplier

Your current settings:
```csharp
moveSpeed = 5f;
sprintSpeedMultiplier = 1.8f;

// Base walk speed: 5 units/s
// Sprint speed: 9 units/s (5 * 1.8)
```

### Animation Speed Mapping

```
Player State              Speed Value    Animation
Standing Still            0.0            Idle
Walking (50% input)       0.5            Walk
Walking (100% input)      1.0            Walk
Sprinting (50% input)     0.9            Walk
Sprinting (100% input)    1.8            Run
```

### Adjusting Run Threshold

If you want run to trigger earlier/later, modify the threshold in `UpdateAnimations()`:

```csharp
// Current (run at 67% of max sprint):
bool isRunning = isMoving && currentAnimationSpeed > 1.2f;

// Start running earlier (run at 50% of max sprint):
bool isRunning = isMoving && currentAnimationSpeed > 0.9f;

// Start running later (run only at full sprint):
bool isRunning = isMoving && currentAnimationSpeed > 1.5f;
```

## Public API

### Check if Player is Running

```csharp
PlayerMovement movement = GetComponent<PlayerMovement>();

if (movement.IsRunning)
{
    // Player is running at max speed
    Debug.Log("Player is running!");
}
```

### Use Cases

**Footstep Audio:**
```csharp
public class FootstepAudio : MonoBehaviour
{
    [SerializeField] private AudioClip walkFootstep;
    [SerializeField] private AudioClip runFootstep;
    
    private PlayerMovement movement;
    private AudioSource audioSource;
    
    void Start()
    {
        movement = GetComponent<PlayerMovement>();
        audioSource = GetComponent<AudioSource>();
    }
    
    // Called from animation event
    void PlayFootstep()
    {
        AudioClip clip = movement.IsRunning ? runFootstep : walkFootstep;
        audioSource.PlayOneShot(clip);
    }
}
```

**Dust Particle System:**
```csharp
public class MovementDust : MonoBehaviour
{
    [SerializeField] private ParticleSystem walkDust;
    [SerializeField] private ParticleSystem runDust;
    
    private PlayerMovement movement;
    
    void Update()
    {
        if (movement.IsRunning)
        {
            walkDust.Stop();
            if (!runDust.isPlaying) runDust.Play();
        }
        else if (movement.IsSprinting || movement.IsMoving)
        {
            runDust.Stop();
            if (!walkDust.isPlaying) walkDust.Play();
        }
        else
        {
            walkDust.Stop();
            runDust.Stop();
        }
    }
}
```

**Stamina Drain Modifier:**
```csharp
public class StaminaManager : MonoBehaviour
{
    [SerializeField] private float runStaminaDrain = 25f;
    [SerializeField] private float sprintStaminaDrain = 20f;
    
    private PlayerMovement movement;
    
    void Update()
    {
        if (movement.IsRunning)
        {
            // Higher drain when running at max speed
            DrainStamina(runStaminaDrain * Time.deltaTime);
        }
        else if (movement.IsSprinting)
        {
            // Normal sprint drain
            DrainStamina(sprintStaminaDrain * Time.deltaTime);
        }
    }
}
```

## Animation Events

Add these to your Run animation for perfect timing:

**Run Cycle:**
```
Frame 0:   RunStart (optional)
Frame 8:   FootLeft
Frame 16:  FootRight
Frame 24:  Loop back to frame 8
```

**In Your Script:**
```csharp
public class PlayerMovement : MonoBehaviour
{
    // Called from animation events
    public void OnRunFootLeft()
    {
        // Play footstep sound
        // Spawn dust particle
        // Camera shake (subtle)
    }
    
    public void OnRunFootRight()
    {
        // Play footstep sound
        // Spawn dust particle
        // Camera shake (subtle)
    }
}
```

## Fine-Tuning

### Smooth Speed Transitions

Adjust `animationSmoothTime` for smoother/snappier transitions:

```csharp
[SerializeField] private float animationSmoothTime = 0.1f;

// Smoother (takes longer to reach full speed):
animationSmoothTime = 0.2f;

// Snappier (reaches full speed quickly):
animationSmoothTime = 0.05f;
```

### Run Animation Speed

Match your animation playback speed to movement speed:

**In Animator Controller:**
```
Run State:
- Speed Multiplier: 1.0 (normal)
- Or use Speed parameter: Set Motion Time from Speed parameter

If run looks too slow:
- Speed Multiplier: 1.2

If run looks too fast:
- Speed Multiplier: 0.8
```

### Sprint vs Run Distinction

If you want separate sprint and run animations:

```csharp
// In PlayerMovement.cs
bool isRunning = isMoving && currentAnimationSpeed > 1.0f && currentAnimationSpeed < 1.5f;
bool isSprinting = isMoving && currentAnimationSpeed >= 1.5f;

animator.SetBool(IsRunningHash, isRunning);
animator.SetBool(IsSprintingHash, isSprinting);
```

**Then in Animator:**
```
Walk (Speed 0.5-1.0)
  ?
Run (Speed 1.0-1.5)
  ?
Sprint (Speed 1.5-1.8)
```

## Testing Checklist

### ? Basic Functionality
```
? Idle plays when standing still
? Walk plays when moving normally
? Run plays when moving at max speed
? Smooth transition Walk ? Run
? Smooth transition Run ? Walk
? Run returns to Idle correctly
```

### ? Sprint Integration
```
? Run triggers when sprinting
? Sprint stamina drains correctly
? Run stops when stamina depletes
? Speed value reflects sprint multiplier
```

### ? Edge Cases
```
? Run plays with gamepad at full stick
? Run plays with keyboard (WASD held)
? Run works while strafing
? Run works in all movement directions
? Run respects dodge interruption
```

## Common Issues

### Run Not Triggering

**Check:**
1. `IsRunning` bool exists in Animator
2. Transition Walk ? Run exists
3. Transition condition: `IsRunning == true`
4. Sprint is working (stamina available)
5. Movement input is at maximum (1.0)

**Debug:**
```csharp
void Update()
{
    Debug.Log($"Speed: {currentAnimationSpeed}, IsRunning: {IsRunning}");
}
```

### Run Triggers Too Early/Late

**Adjust threshold:**
```csharp
// Current
bool isRunning = isMoving && currentAnimationSpeed > 1.2f;

// Your adjustment (find sweet spot)
bool isRunning = isMoving && currentAnimationSpeed > 1.0f;  // Earlier
bool isRunning = isMoving && currentAnimationSpeed > 1.5f;  // Later
```

### Jerky Walk-Run Transitions

**Solutions:**
1. Increase transition duration (0.2-0.3s)
2. Use blend tree instead of hard transitions
3. Adjust `animationSmoothTime` to 0.15-0.2
4. Enable "Blend" on transition curves

### Run Animation Looks Wrong

**Check:**
1. Animation clip imported correctly
2. Animation loops properly
3. Root motion disabled (if using transform movement)
4. Animation speed multiplier set correctly

**Fix Animation Speed:**
```
If run looks like fast-forward:
? Reduce Speed Multiplier to 0.7-0.8

If run looks like slow-motion:
? Increase Speed Multiplier to 1.2-1.3
```

## Summary

**Key Points:**
- ? Run animation auto-triggers at Speed > 1.2
- ? Works with sprint system automatically
- ? Smooth transitions via lerp
- ? Public `IsRunning` property for external checks
- ? Compatible with blend trees and bool states
- ? Easily adjustable threshold

**Setup Steps:**
1. Add `IsRunning` bool to Animator
2. Create Run state
3. Add Walk ? Run transitions
4. Import run animation clip
5. Test at full speed
6. Adjust threshold if needed

**Your run animation system is ready for max-speed movement!** ????
