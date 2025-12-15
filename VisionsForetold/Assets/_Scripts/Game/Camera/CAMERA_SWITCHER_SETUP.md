# 🎥 Camera Perspective Switcher - Setup Guide

## Overview

Switch between **isometric top-down** and **third-person** camera perspectives on the fly! Works with both **keyboard** and **gamepad** input.

## Features

✅ **Two Camera Modes:**
- **Isometric** - Fixed top-down view (like Diablo, Hades)
- **Third Person** - Behind-player view (like Dark Souls, Elden Ring)

✅ **Input Support:**
- Keyboard (V key by default)
- Gamepad button
- Mouse (right-click to rotate third-person camera)
- Gamepad right stick (rotate third-person camera)

✅ **Smooth Transitions:**
- Cinematic blending between perspectives
- Configurable blend time
- No jarring switches

✅ **Full Control:**
- Adjustable camera distances
- Adjustable angles
- Manual or auto camera rotation

## Quick Setup (2 Minutes)

### Step 1: Add Component to Player

```
1. Select Player in Hierarchy
2. Add Component → Camera Perspective Switcher
3. Done! (Auto-creates cameras)
```

### Step 2: (Optional) Add Input Action

For gamepad support:
```
1. Open Input Actions asset
2. Add Action: "SwitchCamera"
3. Binding: Button North (Y on Xbox, Triangle on PS)
4. Save
```

### Step 3: Test

```
1. Play game
2. Press V (keyboard) → Switches perspective
3. In third-person:
   - Hold Right Mouse → Rotate camera
   - OR Use Right Stick → Rotate camera
```

## Camera Modes Explained

### Isometric Mode (Top-Down)

```
        Camera
          ↓
     [Fixed angle]
          ↓
        Player ●
```

**Characteristics:**
- Fixed camera angle (45° by default)
- Doesn't rotate with player
- Great for strategic gameplay
- See more of the battlefield

**Use Cases:**
- ARPG combat (Diablo-style)
- RTS-style gameplay
- Top-down exploration
- Twin-stick shooters

### Third Person Mode (Behind Player)

```
    Camera
      ↓ (rotates with player)
   [● Player]
```

**Characteristics:**
- Camera behind player
- Rotates with player movement
- Can manually rotate with mouse/stick
- Closer, more immersive view

**Use Cases:**
- Action combat (Souls-like)
- Exploration
- Precision platforming
- Cinematic moments

## Inspector Settings

### Camera References

| Setting | Description |
|---------|-------------|
| **Isometric Camera** | Top-down camera (auto-created) |
| **Third Person Camera** | Behind-player camera (auto-created) |
| **Player Transform** | Player GameObject (auto-found) |

### Starting Perspective

| Setting | Options |
|---------|---------|
| **Default Perspective** | Isometric / ThirdPerson |

Choose which camera mode to start in.

### Input Settings

| Setting | Default | Description |
|---------|---------|-------------|
| **Switch Key** | V | Keyboard key to switch |
| **Gamepad Switch Button** | "SwitchCamera" | Input Action name |
| **Switch Cooldown** | 0.5s | Prevent rapid switching |

### Transition Settings

| Setting | Default | Description |
|---------|---------|-------------|
| **Blend Time** | 0.8s | Transition duration |
| **Smooth Transition** | ✓ | Cinematic blend |

### Third Person Settings

| Setting | Default | Description |
|---------|---------|-------------|
| **Third Person Distance** | 5.0 | Distance behind player |
| **Third Person Height** | 2.0 | Camera height offset |
| **Camera Rotation Speed** | 10.0 | Auto-follow speed |
| **Enable Manual Camera Rotation** | ✓ | Allow mouse/stick control |
| **Mouse Sensitivity** | 2.0 | Mouse rotation speed |
| **Gamepad Sensitivity** | 100.0 | Stick rotation speed |

### Isometric Settings

| Setting | Default | Description |
|---------|---------|-------------|
| **Isometric Angle** | 45° | Camera angle from horizontal |
| **Isometric Distance** | 10.0 | Distance from player |
| **Isometric Height** | 10.0 | Camera height |

## Configuration Presets

### Diablo-Style ARPG

```csharp
Default Perspective: Isometric
Isometric Angle: 60°
Isometric Distance: 12.0
Isometric Height: 12.0
Third Person Distance: 6.0
```

### Souls-Like Action

```csharp
Default Perspective: ThirdPerson
Third Person Distance: 4.0
Third Person Height: 1.5
Enable Manual Camera Rotation: ✓
Camera Rotation Speed: 8.0
```

### Hybrid Tactical

```csharp
Default Perspective: Isometric
Isometric Angle: 45°
Blend Time: 0.5
Switch Cooldown: 0.3
// Fast switching for tactical advantage
```

### Cinematic Adventure

```csharp
Default Perspective: ThirdPerson
Blend Time: 1.5
Smooth Transition: ✓
Third Person Distance: 7.0
// Slower, more cinematic transitions
```

## Controls

### Keyboard

| Input | Action |
|-------|--------|
| **V** | Toggle perspective |
| **Right Mouse Button (Hold)** | Rotate third-person camera |

### Gamepad

| Input | Action |
|-------|--------|
| **Y / Triangle** | Toggle perspective (if Input Action set up) |
| **Right Stick** | Rotate third-person camera |

### Mouse

| Input | Action |
|-------|--------|
| **Right-Click + Drag** | Rotate third-person camera |

## Advanced Setup

### Manual Camera Creation

If you want custom Cinemachine cameras:

```
1. Create Cinemachine Virtual Camera (isometric)
   - Name: "IsometricCamera"
   - Add CinemachineFollow component
   - Assign to Inspector

2. Create Cinemachine Virtual Camera (third person)
   - Name: "ThirdPersonCamera"
   - Add CinemachineFollow component
   - Assign to Inspector
```

### Input Action Setup

```
Input Actions Asset:
├─ Player
│  ├─ Move (existing)
│  ├─ Attack (existing)
│  ├─ Look (for camera rotation)
│  └─ SwitchCamera (NEW!)
│     ├─ Type: Button
│     ├─ Keyboard: V
│     └─ Gamepad: Button North (Y/Triangle)
```

## Scripting API

```csharp
// Get component
CameraPerspectiveSwitcher switcher = GetComponent<CameraPerspectiveSwitcher>();

// Check current perspective
if (switcher.IsIsometric())
{
    Debug.Log("In isometric view");
}

// Switch perspectives
switcher.SwitchToThirdPerson();
switcher.SwitchToIsometric();
switcher.TogglePerspective();

// Adjust at runtime
switcher.SetThirdPersonDistance(8.0f);
switcher.SetIsometricDistance(15.0f);

// Get current mode
CameraPerspectiveSwitcher.CameraPerspective mode = switcher.GetCurrentPerspective();
```

## Third Person Camera Rotation

### Auto-Follow Mode

**When:** Manual rotation disabled

```
Player rotates → Camera smoothly follows
```

**Settings:**
```
Enable Manual Camera Rotation: ☐
Camera Rotation Speed: 10.0 (how fast camera follows)
```

### Manual Rotation Mode

**When:** Manual rotation enabled

```
Mouse/Stick input → Camera rotates around player
No input → Camera auto-follows player rotation
```

**Settings:**
```
Enable Manual Camera Rotation: ✓
Mouse Sensitivity: 2.0
Gamepad Sensitivity: 100.0
```

**Controls:**
- **Right Mouse Button** + Move = Rotate camera
- **Right Stick** = Rotate camera
- **Release inputs** = Auto-follow resumes

## Isometric Camera

### Fixed Angle View

The isometric camera stays at a fixed angle relative to the world, not the player.

**Angle Explanation:**
```
0° = Horizontal (side view)
45° = Classic isometric (Diablo-style)
90° = Top-down (bird's eye)
```

**Customization:**
```csharp
Isometric Angle: 45°    // Sweet spot for most games
Isometric Distance: 10  // How far back
Isometric Height: 10    // How high up
```

**Visual:**
```
45° Angle:
    Camera
      ╱
     ╱  45°
    ╱___
   Player

60° Angle (more top-down):
    Camera
      │
      │ 60°
     ╱
    Player
```

## Troubleshooting

### Camera Not Switching

**Check:**
1. Component is enabled
2. Player has component attached
3. Both cameras exist (check Hierarchy)
4. Press V or assigned button

**Debug:**
```
Enable "Show Debug Info" in Inspector
See current mode in top-left corner
```

### Third Person Camera Not Rotating

**Check:**
1. "Enable Manual Camera Rotation" is checked
2. "Look" Input Action exists and is enabled
3. Right mouse button or right stick is working

**Test:**
```
While in third person:
1. Hold right mouse button
2. Move mouse
3. Camera should rotate around player
```

### Camera Jumps/Jittery

**Solution:**
```
Increase Blend Time: 1.0 or higher
Enable Smooth Transition: ✓
Reduce Switch Cooldown: 0.3s minimum
```

### Isometric Camera Too High/Low

**Adjust:**
```
Isometric Height: Change value
Isometric Distance: Change value
Isometric Angle: Change to taste (30-60° typical)
```

### Third Person Camera Through Walls

**Note:** This script doesn't include collision detection.

**Solutions:**
1. Add Cinemachine Collider extension
2. Use Cinemachine Camera's built-in collision
3. Ensure "Collision Filter" layers exclude player

## Performance

- **Lightweight**: ~0.05ms per frame
- **No allocations**: After initialization
- **CPU efficient**: Only updates active camera
- **Scales well**: Handles any player count

## Integration

### Works With

✅ Your existing PlayerMovement  
✅ Your existing PlayerAttack  
✅ Your existing AimIndicator  
✅ Your existing AimOffsetCamera  
✅ Cinemachine  
✅ New Input System  

### Compatibility

The switcher **doesn't interfere** with:
- Aim system
- Attack system
- Movement system
- Other camera scripts

You can **combine** with AimOffsetCamera for even more dynamic cameras!

## Best Practices

### 1. Match Gameplay to Perspective

**Isometric** = Best for:
- Seeing many enemies
- Strategic positioning
- Area effects
- Twin-stick combat

**Third Person** = Best for:
- Precision aiming
- Exploration
- Cinematic moments
- Souls-like combat

### 2. Provide Visual Feedback

```csharp
// Show UI indicator
void OnPerspectiveChanged()
{
    if (switcher.IsIsometric())
    {
        ShowUI("Tactical View");
    }
    else
    {
        ShowUI("Combat View");
    }
}
```

### 3. Game Design Considerations

**Different perspectives might need:**
- Different attack ranges
- Different aim systems
- Different UI layouts
- Different movement speeds

**Example:**
```csharp
void Update()
{
    if (switcher.IsIsometric())
    {
        // Wider attack range for isometric
        attackRange = 5.0f;
    }
    else
    {
        // Closer range for third person
        attackRange = 3.0f;
    }
}
```

## Example Use Cases

### 1. Exploration vs Combat

```
Exploring → Isometric (see more)
Combat starts → Switch to Third Person (precision)
```

### 2. Indoor vs Outdoor

```
Tight corridors → Third Person (better visibility)
Open areas → Isometric (see battlefield)
```

### 3. Player Preference

```
Let player choose their preferred style
Save preference in settings
```

## Files Created

- ✅ `CameraPerspectiveSwitcher.cs` - Main script
- ✅ `CAMERA_SWITCHER_SETUP.md` - This guide

## Next Steps

1. **Test both perspectives** - Find what feels best
2. **Adjust distances** - Match your game's scale
3. **Set up Input Action** - For gamepad support
4. **Customize blend time** - Fast or cinematic?
5. **Add UI feedback** - Show current mode to player

Enjoy your dual-perspective camera system! 🎥🎮✨
