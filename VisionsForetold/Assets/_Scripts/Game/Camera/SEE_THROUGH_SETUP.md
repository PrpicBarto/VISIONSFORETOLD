# ??? See-Through System - Setup Guide

## Overview

Makes objects between the camera and player/enemies **transparent** so you never lose sight of important characters. Essential for third-person games!

## Features

? **Automatic Detection** - Finds obstructing objects automatically  
? **Smooth Transitions** - Fades in/out gracefully  
? **Custom Shader** - Professional see-through effect  
? **Configurable** - Adjust colors, transparency, detection  
? **Performance Friendly** - Minimal overhead  
? **Works with Any Objects** - Walls, trees, rocks, etc.  

## Quick Setup (3 Minutes)

### Step 1: Create the Material

```
1. Right-click in Project ? Create ? Material
2. Name it: "SeeThroughMaterial"
3. Shader dropdown ? Custom ? SeeThrough
4. Adjust colors if desired (optional)
```

### Step 2: Add to Camera or Player

```
1. Select your Main Camera (or Player GameObject)
2. Add Component ? See Through System
3. In Inspector:
   - Target: Drag your Player
   - See Through Material: Drag "SeeThroughMaterial"
   - Done!
```

### Step 3: Test

```
1. Play game
2. Move camera so objects are between camera and player
3. Objects should become transparent!
```

## How It Works

### Obstruction Detection

```
Camera ----[Check]---? Player
           ?
      Object in way?
           ?
    Make transparent!
```

**Process:**
1. Every 0.1s, checks line from camera to player
2. Finds all objects blocking the view
3. Applies see-through shader
4. When object moves away, restores original material

### Visual Example

**Before:**
```
Camera ? [WALL] ? ? Player (can't see!)
```

**After:**
```
Camera ? [????] ? ? Player (see-through wall!)
```

## Inspector Settings

### References

| Setting | Description |
|---------|-------------|
| **Target** | Object to keep visible (Player) |
| **Main Camera** | Camera to check from (auto-finds) |
| **See Through Material** | Material with see-through shader |

### See-Through Settings

| Setting | Default | Description |
|---------|---------|-------------|
| **See Through Color** | Light Blue | Color when transparent |
| **Transparency Amount** | 0.5 | How see-through (0-1) |

**Color Examples:**
- Light Blue (default): Sci-fi, modern
- Light Green: Nature, fantasy
- Light Pink: Magical, stylized
- White: Clean, minimal

### Detection Settings

| Setting | Default | Description |
|---------|---------|-------------|
| **Obstruction Layers** | Everything | Which layers to check |
| **Check Radius** | 0.5 | Detection sphere size |
| **Check Interval** | 0.1s | How often to check |
| **Transition Time** | 0.2s | Fade speed |

### Advanced Settings

| Setting | Default | Description |
|---------|---------|-------------|
| **Use Sphere Cast** | ? | Better for large objects |
| **Sphere Radius** | 0.1 | Spherecast size |
| **Ignore Tags** | Player, Enemy | Don't make these transparent |

## Shader Properties

### In Material Inspector

When you select the SeeThroughMaterial:

| Property | Purpose |
|----------|---------|
| **Main Color** | Base object color |
| **Base (RGB)** | Object's texture |
| **See-Through Color** | Color when transparent |
| **See-Through Amount** | Transparency intensity |

**Adjust these for different effects!**

## Configuration Examples

### Subtle See-Through (Realistic)

```csharp
See Through Color: (0.8, 0.8, 0.8, 0.3) // Light gray
Transparency Amount: 0.3
Transition Time: 0.3s
```

**Effect:** Objects barely fade, natural look

### Strong See-Through (Stylized)

```csharp
See Through Color: (0.5, 1.0, 1.0, 0.7) // Bright cyan
Transparency Amount: 0.7
Transition Time: 0.1s
```

**Effect:** Objects very transparent, game-like

### Hologram Style

```csharp
See Through Color: (0.2, 0.6, 1.0, 0.8) // Electric blue
Transparency Amount: 0.8
Transition Time: 0.05s
```

**Effect:** Futuristic hologram appearance

### Ghost Style

```csharp
See Through Color: (1.0, 1.0, 1.0, 0.5) // White ghost
Transparency Amount: 0.6
Transition Time: 0.4s
```

**Effect:** Ghostly, slow fade

## Layer Setup (Recommended)

### Create Layers

```
1. Edit ? Project Settings ? Tags and Layers
2. Add layers:
   - Environment (walls, buildings)
   - Obstacles (rocks, trees)
   - Decorations (props, furniture)
```

### Assign to Objects

```
1. Select walls/buildings
2. Inspector ? Layer ? Environment
3. Repeat for other objects
```

### Configure See-Through System

```
Obstruction Layers: 
? Environment
? Obstacles
? Decorations
? Player
? Enemy
? UI
```

**Why?** Only makes environment transparent, not characters!

## Common Scenarios

### For Third-Person Camera

```
Attach to: Main Camera
Target: Player
Check Interval: 0.1s
Transition Time: 0.2s
```

**Use Case:** Standard third-person game

### For Isometric Camera

```
Attach to: Main Camera
Target: Player
Check Interval: 0.15s (less frequent)
Use Sphere Cast: ?
Sphere Radius: 0.3
```

**Use Case:** Top-down ARPG

### For Multiple Targets

**Option 1:** Multiple Systems
```
Create SeeThroughSystem for each important character
Each targets different GameObject
```

**Option 2:** Switch Target
```csharp
// In your code
seeThroughSystem.SetTarget(currentBoss.transform);
```

**Use Case:** Boss fights, escort missions

## Performance Optimization

### For Large Scenes

```
Check Interval: 0.2s (check less often)
Obstruction Layers: Only important layers
Transition Time: 0.3s (slower = smoother)
```

### For Small Scenes

```
Check Interval: 0.05s (very responsive)
Use Sphere Cast: ?
Transition Time: 0.1s (snappy)
```

### For Many Objects

```
Obstruction Layers: Be specific!
Ignore Tags: Add more tags to ignore
Use Sphere Cast: ? (raycast is faster)
```

## Troubleshooting

### Objects Not Becoming Transparent

**Check:**
1. See Through Material is assigned
2. Material uses Custom/SeeThrough shader
3. Target is assigned (usually Player)
4. Objects are on Obstruction Layers

**Test:**
```
Enable "Show Debug Rays" in Inspector
Yellow line should draw from camera to player
```

### Objects Stay Transparent

**Check:**
1. Transition Time isn't too long
2. Objects are moving away from camera line
3. No errors in Console

**Fix:**
```
Reduce Transition Time to 0.1s
Check Console for material errors
```

### Wrong Objects Becoming Transparent

**Solution:**
```
Adjust Obstruction Layers
Add tags to Ignore Tags list
Reduce Sphere Radius
```

### Performance Issues

**Optimize:**
```
Increase Check Interval (0.2s+)
Use fewer Obstruction Layers
Disable Use Sphere Cast
```

### Shader Not Found

**Fix:**
```
1. Check shader file: Assets/_Shaders/SeeThrough.shader
2. Reimport shader (right-click ? Reimport)
3. In Material: Shader ? Custom ? SeeThrough
```

**Fallback:**
The system auto-falls back to Standard shader if custom shader missing.

## Advanced Usage

### Scripting API

```csharp
// Get component
SeeThroughSystem seeThrough = GetComponent<SeeThroughSystem>();

// Change target at runtime
seeThrough.SetTarget(newTarget.transform);

// Adjust color
seeThrough.SetSeeThroughColor(Color.cyan);

// Adjust transparency
seeThrough.SetTransparency(0.8f);

// Enable/disable
seeThrough.SetEnabled(true);
```

### Dynamic Switching

```csharp
void OnCameraSwitch(CameraPerspective perspective)
{
    SeeThroughSystem seeThrough = camera.GetComponent<SeeThroughSystem>();
    
    if (perspective == CameraPerspective.ThirdPerson)
    {
        // Enable for third person
        seeThrough.SetEnabled(true);
        seeThrough.SetTransparency(0.6f);
    }
    else
    {
        // Disable for isometric
        seeThrough.SetEnabled(false);
    }
}
```

### Multiple Targets

```csharp
// Create list of important targets
List<Transform> importantTargets = new List<Transform>
{
    player.transform,
    mainBoss.transform,
    escortNPC.transform
};

// Check all targets
foreach (Transform target in importantTargets)
{
    // Custom obstruction check
    if (IsObstructed(camera.position, target.position))
    {
        MakePathTransparent(camera.position, target.position);
    }
}
```

## Shader Customization

### Custom Colors Per Material

Edit `SeeThrough.shader`:

```hlsl
Properties
{
    _SeeThroughColor ("See-Through Color", Color) = (1, 0.5, 0.5, 0.5)
    // Change default color here
}
```

### Different Transparency Styles

**Additive Blending (Glow):**
```hlsl
Blend One One  // Instead of SrcAlpha OneMinusSrcAlpha
```

**Multiplicative (Darken):**
```hlsl
Blend DstColor Zero
```

**Softer Edges:**
```hlsl
float fresnel = pow(1.0 - saturate(dot(i.worldNormal, viewDir)), 2.0);
// Change 3.0 to 2.0 for softer edge
```

## Best Practices

### 1. Layer Organization

```
? Use layers for obstruction detection
? Keep player/enemies on separate layers
? Don't make UI transparent
? Don't use "Everything" for obstruction
```

### 2. Performance

```
? Check interval: 0.1-0.2s is good
? Use specific layers
? Limit sphere radius
? Don't check every frame
? Don't make everything transparent
```

### 3. Visual Quality

```
? Match see-through color to game's palette
? Use subtle transparency (0.3-0.5)
? Smooth transitions (0.2-0.3s)
? Don't use harsh colors
? Don't make fully invisible
```

### 4. Game Design

```
? Essential for third-person games
? Great for indoor scenes
? Helpful for cluttered environments
? May not need for isometric views
? Disable in cutscenes if needed
```

## Integration

### With Camera Switcher

```csharp
void OnPerspectiveChanged(CameraPerspective newPerspective)
{
    SeeThroughSystem seeThrough = camera.GetComponent<SeeThroughSystem>();
    
    // Only enable for third person
    seeThrough.enabled = (newPerspective == CameraPerspective.ThirdPerson);
}
```

### With Player Movement

```csharp
// Already works automatically!
// SeeThroughSystem finds Player by tag
// Tracks player movement automatically
```

### With Multiple Cameras

```csharp
// Add SeeThroughSystem to each camera
// Each can have different settings
ThirdPersonCamera: Transparency = 0.6
IsometricCamera: Transparency = 0.3
```

## Visual Examples

### Indoor Scene

```
Camera inside building ? Many walls ? Player behind walls
Result: Walls become transparent, player always visible
```

### Forest

```
Camera in forest ? Trees in way ? Player behind tree
Result: Trees fade, player visible through foliage
```

### Dungeon

```
Camera in corridor ? Pillars blocking view ? Player behind pillar
Result: Pillar transparent, player never lost
```

## Files Created

- ? `SeeThrough.shader` - Custom shader
- ? `SeeThroughSystem.cs` - Management script
- ? `SEE_THROUGH_SETUP.md` - This guide

## Quick Start Checklist

- [ ] Create SeeThroughMaterial
- [ ] Assign Custom/SeeThrough shader
- [ ] Add SeeThroughSystem to camera
- [ ] Assign Player as target
- [ ] Assign material to system
- [ ] Test with objects blocking view
- [ ] Adjust colors and transparency
- [ ] Set up layers (optional)

## Next Steps

1. **Create the material** with see-through shader
2. **Add system to camera** with player as target
3. **Test** by putting objects between camera and player
4. **Adjust colors** to match your game's style
5. **Configure layers** for better control
6. **Optimize** check interval and settings

Never lose sight of your player again! ????
