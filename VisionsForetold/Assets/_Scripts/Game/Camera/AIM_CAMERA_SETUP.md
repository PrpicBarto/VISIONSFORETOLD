# ?? Aim-Based Dynamic Camera System - Setup Guide

## Overview

This system creates a dynamic camera that shifts to show what the player is aiming at, providing better visibility and immersion. Works seamlessly with both **mouse** and **gamepad** input.

## Features

? **Dynamic Camera Offset** - Camera shifts based on aim direction  
? **Look Ahead** - Shows more of what's in front of you  
? **Optional Zoom** - Zooms in when aiming far  
? **Visual Aim Indicator** - Shows exactly where you're pointing  
? **Mouse & Gamepad Support** - Works with both input methods  
? **Attack Mode Colors** - Indicator changes color per attack mode  
? **Smooth Transitions** - No jarring camera movements  

## Quick Setup (5 Minutes)

### Step 1: Add Camera Component

1. Find your **Cinemachine Virtual Camera** in Hierarchy
2. Add Component ? **Aim Offset Camera**
3. In Inspector:
   - Assign **Player Transform**
   - **Aim Target** (auto-found from PlayerMovement)
   - **Virtual Camera** (auto-assigned)
   - **Player Movement** reference

### Step 2: Add Visual Indicator

1. Select your **Player** GameObject
2. Add Component ? **Aim Indicator**
3. Components auto-find references
4. (Optional) Assign a custom indicator prefab

### Step 3: Test!

Play the game:
- **Move mouse** ? Camera shifts to show where you're aiming
- **Use gamepad stick** ? Same smooth camera movement
- **Aim far away** ? Camera shows more ahead
- **See indicator** ? Visual crosshair at aim point

## Detailed Configuration

### AimOffsetCamera Component

#### Camera Offset Settings

| Setting | Default | Description |
|---------|---------|-------------|
| **Max Camera Offset** | 3.0 | How far camera shifts (meters) |
| **Camera Smooth Speed** | 5.0 | Speed of camera movement |
| **Min Aim Distance** | 1.0 | Minimum aim distance to trigger |
| **Screen Offset Percentage** | 0.3 | Screen space offset (0-1) |

**Recommended Values:**
```
Subtle: Max Offset = 2.0, Smooth Speed = 3.0
Balanced: Max Offset = 3.0, Smooth Speed = 5.0 (default)
Aggressive: Max Offset = 5.0, Smooth Speed = 8.0
```

#### Vertical Offset

| Setting | Default | Description |
|---------|---------|-------------|
| **Vertical Aim Offset** | 0.5 | Up/down camera shift |
| **Use Vertical Offset** | ? | Enable vertical movement |

#### Dead Zone

| Setting | Default | Description |
|---------|---------|-------------|
| **Dead Zone** | 0.1 | Center area with no camera move (0-0.5) |

**Purpose**: Prevents jittery camera when aiming near player.

#### Zoom Settings

| Setting | Default | Description |
|---------|---------|-------------|
| **Enable Aim Zoom** | ? | Zoom when aiming far |
| **Aimed FOV** | 50° | Zoomed field of view |
| **Normal FOV** | 60° | Default field of view |
| **Zoom Distance Threshold** | 8.0 | Distance to trigger zoom |

**Use Cases:**
- **Disable** for fast-paced melee combat
- **Enable** for tactical ranged/sniper gameplay

#### Look Ahead

| Setting | Default | Description |
|---------|---------|-------------|
| **Enable Look Ahead** | ? | Show more in aim direction |
| **Look Ahead Distance** | 5.0 | How far to look ahead |

### AimIndicator Component

#### Visual Settings

| Setting | Default | Description |
|---------|---------|-------------|
| **Indicator Color** | White | Base indicator color |
| **Indicator Size** | 0.5 | Size in world units |
| **Hover Height** | 0.1 | Height above ground |

#### Behavior

| Setting | Default | Description |
|---------|---------|-------------|
| **Show Indicator** | ? | Display indicator |
| **Hide When Idle** | ? | Hide when not aiming |
| **Min Aim Distance** | 1.0 | Distance to show indicator |
| **Fade Distance** | 0.5 | Fade in/out distance |

#### Animation

| Setting | Default | Description |
|---------|---------|-------------|
| **Rotate Indicator** | ? | Spin animation |
| **Rotation Speed** | 90°/s | Rotation speed |
| **Pulse Indicator** | ? | Scale pulsing |
| **Pulse Speed** | 2.0 | Pulse frequency |
| **Pulse Amount** | 0.2 | Pulse intensity (0-1) |

#### Line Renderer

| Setting | Default | Description |
|---------|---------|-------------|
| **Show Aim Line** | ? | Line from player to aim |
| **Line Color** | Red | Line color |
| **Line Width** | 0.05 | Line thickness |

#### Attack Mode Colors

| Setting | Default | Description |
|---------|---------|-------------|
| **Use Attack Mode Colors** | ? | Color per attack mode |
| **Melee Color** | Red | Melee mode color |
| **Ranged Color** | Green | Ranged mode color |
| **Spell Color** | Cyan | Spell mode color |

## Visual Examples

### Camera Behavior

```
No Aim:
  Player ?????????? Camera (centered)

Aiming Right:
  Player ???? Aim
            ?
       Camera shifts right to show more

Aiming Far:
  Player ????????? Far Aim
       Camera zooms + shifts forward
```

### Indicator States

```
Idle: (hidden)
Close Aim: (faded)
Active Aim: ? (full opacity, rotating)
Melee Mode: ? (red)
Ranged Mode: ? (green)
Spell Mode: ? (cyan)
```

## Cinemachine Integration

### Using CinemachineFollow

The script automatically works with `CinemachineFollow`:
- Modifies `FollowOffset` to shift camera
- Smooth transitions
- Respects base offset

### Using CinemachinePositionComposer

Also works with `PositionComposer`:
- Uses `TargetOffset` for screen-space shifts
- Different feel but same concept

### Recommended Cinemachine Setup

```
Virtual Camera
?? CinemachineFollow
?  ?? Follow Offset: (0, 5, -8)
?  ?? Damping: (1, 1, 1)
?? AimOffsetCamera (YOUR NEW SCRIPT)
?  ?? Max Camera Offset: 3.0
?  ?? Camera Smooth Speed: 5.0
?  ?? Enable Look Ahead: ?
?? (Optional) Cinemachine Noise for shake
```

## Gameplay Tuning

### For Melee Combat (Close-range)

```csharp
AimOffsetCamera:
- Max Camera Offset: 2.0
- Smooth Speed: 8.0 (fast)
- Enable Look Ahead: ?
- Enable Aim Zoom: ? (no zoom)

AimIndicator:
- Show Indicator: ?
- Hide When Idle: ?
- Use Attack Mode Colors: ?
```

### For Ranged Combat (Shooter-style)

```csharp
AimOffsetCamera:
- Max Camera Offset: 4.0
- Smooth Speed: 4.0 (slower)
- Enable Look Ahead: ?
- Enable Aim Zoom: ?
- Aimed FOV: 45°

AimIndicator:
- Show Indicator: ?
- Show Aim Line: ?
- Rotate Indicator: ?
```

### For Tactical/Souls-like

```csharp
AimOffsetCamera:
- Max Camera Offset: 2.5
- Smooth Speed: 3.0 (very smooth)
- Dead Zone: 0.2 (larger)
- Enable Aim Zoom: ?

AimIndicator:
- Show Indicator: ? (no indicator)
- Hide When Idle: ?
```

## Advanced: Custom Indicator

### Create Custom Prefab

1. Create a GameObject with:
   - Quad/Sprite/3D Model
   - Material with transparency
2. Save as prefab
3. Assign to **Indicator Prefab** field

**Example Crosshair Setup:**
```
CrosshairPrefab
?? Quad (with crosshair texture)
?? Rotation: (90, 0, 0)
?? Material: Unlit/Transparent
```

## Performance

- **Lightweight**: Minimal overhead
- **No raycasts per frame**: Only for indicator ground detection
- **Optimized**: Smooth interpolation instead of instant updates
- **Scalable**: Works great even with many enemies

## Troubleshooting

### Camera Not Moving

**Check:**
1. `AimOffsetCamera` script is enabled
2. Aim Target is assigned (from PlayerMovement)
3. Virtual Camera is assigned
4. Player is actually aiming (move mouse/gamepad stick)

**Solution:**
- Increase `Max Camera Offset` to 5.0 for testing
- Disable `Dead Zone` temporarily

### Indicator Not Showing

**Check:**
1. `Show Indicator` is checked
2. `Hide When Idle` - Are you aiming far enough?
3. Min Aim Distance - Try setting to 0.5

**Solution:**
- Set `Min Aim Distance = 0`
- Uncheck `Hide When Idle`
- Check Debug Gizmos in Scene view

### Camera Too Jerky

**Increase:**
- `Camera Smooth Speed` to 10.0+
- `Dead Zone` to 0.2-0.3

### Camera Moves Too Much

**Decrease:**
- `Max Camera Offset` to 2.0
- `Look Ahead Distance` to 2.0

### Indicator Wrong Color

**Check:**
- `Use Attack Mode Colors` is enabled
- PlayerAttack component is on player
- Attack mode is changing (scroll wheel/input)

## Scripting API

### AimOffsetCamera

```csharp
// Get component
AimOffsetCamera camOffset = GetComponent<AimOffsetCamera>();

// Adjust offset at runtime
camOffset.SetMaxOffset(5.0f);

// Change smooth speed
camOffset.SetSmoothSpeed(10.0f);

// Toggle zoom
camOffset.SetAimZoomEnabled(true);

// Toggle look ahead
camOffset.SetLookAheadEnabled(false);

// Reset to base position
camOffset.ResetCamera();

// Get current offset from base
Vector3 offset = camOffset.GetCurrentOffset();
```

### AimIndicator

```csharp
// Get component
AimIndicator indicator = GetComponent<AimIndicator>();

// Show/hide
indicator.SetVisible(true);

// Change color
indicator.SetColor(Color.red);

// Change size
indicator.SetSize(1.0f);

// Toggle aim line
indicator.SetAimLineEnabled(true);
```

## Integration with Existing Systems

### With PlayerAttack

The indicator automatically detects attack mode changes and updates colors. No additional code needed!

### With PlayerMovement

Automatically uses the `AimTarget` from PlayerMovement. Everything is already integrated!

### With Cinemachine

Works alongside other Cinemachine components:
- Noise (camera shake)
- Damping
- Other modifiers

## Files Created

1. ? `AimOffsetCamera.cs` - Dynamic camera system
2. ? `AimIndicator.cs` - Visual aim indicator
3. ? `AIM_CAMERA_SETUP.md` - This guide

## Next Steps

1. **Test with different settings** - Find what feels best for your game
2. **Create custom indicator prefab** - Match your art style
3. **Adjust per game mode** - Different settings for different levels
4. **Add sound effects** - Indicator pulse sound, zoom sound, etc.

Enjoy your dynamic camera system! ????
