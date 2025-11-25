# Echo Object Reveal System Guide

## Overview

The **EchoRevealSystem** makes entire GameObjects visible when the echolocation pulse intersects with them. Objects are revealed by clearing the fog overlay around them, creating a true sonar-like effect where objects "light up" when detected.

## How It Works

1. **Pulse Expands** - Echolocation pulse grows from player
2. **Edge Detection** - Raycasts at pulse edge detect colliding objects
3. **Object Revealed** - Fog clears around entire object bounds
4. **Visible Duration** - Object stays visible for configurable time
5. **Fade Out** - Fog gradually returns over object

## Quick Setup

### Step 1: Add Component
```
GameObject with EcholocationController
  ?? Add Component ? Echo Reveal System
```

### Step 2: Configure Detection
```
Detection Layers: Select which layers to detect
Raycasts Per Frame: 64 (higher = better coverage)
Detection Threshold: 3.0 (distance from pulse edge)
Reveal Duration: 5.0 (seconds objects stay visible)
```

### Step 3: Set Reveal Radius
```
Reveal Radius: 5.0
  - Objects smaller than this use this radius
  - Larger objects use their bounds size
  - Adjust based on your game scale
```

### Step 4: Press Play!
Objects hit by pulse will become visible through the fog!

## Key Differences from Intersection Detector

### EchoIntersectionDetector
- ? Highlights objects with emission glow
- ? Color-codes by object type
- ? Provides events for game logic
- ? Doesn't affect fog overlay

### EchoRevealSystem
- ? **Clears fog around entire object**
- ? **Makes whole object visible**
- ? **True sonar reveal effect**
- ? Fade in/out animations
- ? No color-coding (shows actual object materials)

## Configuration

### Detection Settings

| Setting | Description | Default |
|---------|-------------|---------|
| **Detection Layers** | Which layers to detect | Everything |
| **Raycasts Per Frame** | Detection resolution | 64 |
| **Detection Threshold** | Trigger distance from pulse edge | 3.0 |
| **Reveal Duration** | How long objects stay visible | 5.0s |
| **Detect 3D** | Enable vertical raycasting | false |
| **Vertical Ray Count** | Number of vertical rays | 5 |
| **Vertical Range** | Height range for 3D detection | 10 |

### Shader Communication

| Setting | Description | Default |
|---------|-------------|---------|
| **Max Revealed Objects** | Maximum simultaneous reveals | 50 |
| **Reveal Radius** | Minimum reveal radius | 5.0 |

### Debug

| Setting | Description | Default |
|---------|-------------|---------|
| **Show Debug Logs** | Log reveals to console | true |
| **Show Debug Gizmos** | Draw reveal spheres in scene | true |

## How Fog Clearing Works

The system sends object positions to the shader, which clears fog using this logic:

```hlsl
// For each revealed object:
1. Calculate distance from pixel to object center
2. If within object radius:
   - Clear fog completely at center
   - Smooth falloff at edges
   - Apply fade strength (for fade in/out)
3. Accumulate all reveals (max value wins)
```

### Reveal Formula
```hlsl
reveal = 1.0 - saturate(distToObject / objectRadius);
reveal = smoothstep(0.0, 1.0, reveal);  // Smooth edges
reveal *= revealStrength;  // Fade in/out over time
fogAlpha *= (1.0 - reveal);  // Clear fog
```

## Usage Examples

### Basic Usage
```csharp
using VisionsForetold.Game.Player.Echo;

// Component automatically works when attached
// No code needed for basic functionality!
```

### Query Revealed Objects
```csharp
public class EchoMonitor : MonoBehaviour
{
    private EchoRevealSystem revealer;

    void Start()
    {
        revealer = GetComponent<EchoRevealSystem>();
    }

    void Update()
    {
        int count = revealer.GetRevealedObjectCount();
        Debug.Log($"{count} objects currently revealed");

        List<GameObject> revealed = revealer.GetRevealedObjects();
        foreach (var obj in revealed)
        {
            Debug.Log($"Visible: {obj.name}");
        }
    }
}
```

### Clear All Reveals
```csharp
// Immediately hide all revealed objects
revealer.ClearAllReveals();
```

### Combine with Intersection Detector
```csharp
// Use both systems together!
// Reveal System: Makes objects visible
// Intersection Detector: Adds glow + events

void Start()
{
    var revealer = GetComponent<EchoRevealSystem>();
    var detector = GetComponent<EchoIntersectionDetector>();
    
    detector.OnObjectDetected += hit =>
    {
        Debug.Log($"Object revealed AND highlighted: {hit.hitObject.name}");
        
        // Add custom effects on top of reveal
        if (hit.objectType == EchoObjectType.Enemy)
        {
            PlayWarningSound();
        }
    };
}
```

## Visual Behavior

### Object Size Handling
- **Small Objects** (< Reveal Radius): Use fixed reveal radius
- **Large Objects** (> Reveal Radius): Use object bounds size
- **Calculation**: `radius = max(bounds.extents.magnitude, revealRadius)`

### Fade Animations

**Fade In** (0.5 seconds):
```
Object detected ? Fog clears gradually ? Fully visible
```

**Visible Phase** (Reveal Duration):
```
Object stays fully visible while revealed
```

**Fade Out** (1 second):
```
Timer expires ? Fog returns gradually ? Hidden again
```

### Multiple Overlapping Reveals
When multiple objects overlap:
- Shader uses **maximum reveal value**
- Fog clears to show all overlapping objects
- No visual conflicts or artifacts

## Performance Optimization

### Raycasts Per Frame
```csharp
// Low Performance (Mobile):
raycastsPerFrame = 32;

// Balanced (Default):
raycastsPerFrame = 64;

// High Precision (PC):
raycastsPerFrame = 128;
```

### Max Revealed Objects
```csharp
// Shader Performance Impact:
maxRevealedObjects = 25;  // Faster, fewer objects
maxRevealedObjects = 50;  // Default
maxRevealedObjects = 100; // Slower, more objects
```

**Note:** Shader loops through revealed objects every frame. Lower limit = better performance.

### 2D vs 3D Detection
```csharp
// 2D (Top-down/Isometric) - FASTER:
detect3D = false;
// Casts horizontal rays only

// 3D (Full 3D game) - SLOWER:
detect3D = true;
verticalRayCount = 5;  // More rays = slower
```

## Troubleshooting

### Objects Not Revealing

**Check:**
- [ ] Objects have Colliders
- [ ] Objects are on correct layers (Detection Layers)
- [ ] Detection Threshold is large enough (try 5.0)
- [ ] Raycasts Per Frame is high enough (try 128)
- [ ] EcholocationController is working (pulse visible)

### Fog Not Clearing

**Check:**
- [ ] Shader is "Custom/URP/Echolocation" (updated version)
- [ ] Fog Material is assigned to EcholocationController
- [ ] Max Revealed Objects > 0
- [ ] Reveal Radius > 0
- [ ] Check Console for shader errors

### Objects Disappear Too Fast

**Solution:**
```csharp
revealDuration = 10.0f;  // Increase from default 5.0s
```

### Objects Stay Revealed Forever

**Check:**
- [ ] Reveal Duration is not set to huge value (e.g., 9999)
- [ ] Update loop is running (no errors in console)

### Partial Object Visibility

**Cause:** Reveal Radius too small for large objects

**Solutions:**
1. Increase Reveal Radius:
   ```csharp
   revealRadius = 10.0f;
   ```

2. System auto-scales for large objects, so check object bounds:
   ```csharp
   // In Unity:
   Select object ? Add Mesh Renderer ? Check bounds size
   ```

### Performance Lag

**Solutions:**
1. Reduce raycasts:
   ```csharp
   raycastsPerFrame = 32;
   ```

2. Limit revealed objects:
   ```csharp
   maxRevealedObjects = 25;
   ```

3. Use layer masks to filter:
   ```csharp
   detectionLayers = LayerMask.GetMask("Default", "Enemy", "Item");
   ```

## Shader Implementation Details

### Arrays in Shader
The shader receives up to 50 revealed objects per frame:
- `_RevealPositions[50]` - Object centers (Vector4)
- `_RevealRadii[50]` - Object radii (float)
- `_RevealStrengths[50]` - Fade amounts (float, 0-1)

### Shader Loop
```hlsl
for (int i = 0; i < _RevealCount && i < 50; i++)
{
    // Calculate reveal for this object
    // Accumulate maximum reveal value
}

fogAlpha *= (1.0 - objectReveal);
```

This runs **every pixel, every frame**, so performance depends on:
- Number of revealed objects (_RevealCount)
- Screen resolution
- Maximum array size (50)

## Best Practices

### ? DO:
- Use layer masks to filter detection
- Adjust Reveal Radius for your game scale
- Tune Raycasts Per Frame for quality/performance balance
- Test with various object sizes
- Combine with EchoIntersectionDetector for events

### ? DON'T:
- Set Max Revealed Objects too high (>100)
- Use extremely high Raycasts Per Frame (>256)
- Forget to assign Detection Layers
- Leave Debug Logs enabled in production

## Integration Examples

### Stealth Game
```csharp
// Enemies stay visible longer
revealer.revealDuration = 8.0f;

// Combine with detector for alerts
detector.OnObjectDetected += hit =>
{
    if (hit.objectType == EchoObjectType.Enemy)
    {
        AlertGuards();
    }
};
```

### Puzzle Game
```csharp
// Short reveals force frequent scanning
revealer.revealDuration = 3.0f;

// High precision for small objects
revealer.raycastsPerFrame = 128;
revealer.revealRadius = 2.0f;
```

### Exploration Game
```csharp
// Long reveals for comfortable exploration
revealer.revealDuration = 10.0f;

// Large radius for big environments
revealer.revealRadius = 15.0f;
```

## Comparison Table

| Feature | EchoRevealSystem | EchoIntersectionDetector |
|---------|------------------|-------------------------|
| **Purpose** | Show objects through fog | Highlight + Events |
| **Visual Effect** | Clears fog overlay | Emission glow |
| **Object Visibility** | Full object revealed | Object highlighted |
| **Color Coding** | No (shows real materials) | Yes (type-specific) |
| **Events** | No | Yes (OnObjectDetected) |
| **Performance** | Shader loops (medium) | Material instances (low) |
| **Use Together?** | ? Yes! Complementary | ? Yes! Complementary |

## Recommended Setup

Use **BOTH** systems together for best results:

```
GameObject
?? EcholocationController (pulse + fog)
?? EchoRevealSystem (make objects visible)
?? EchoIntersectionDetector (highlight + events)
```

Benefits:
- Objects become **visible** (reveal system)
- AND get **highlighted** (intersection detector)
- AND trigger **game events** (intersection detector)

## Debugging Tools

### Console Logs
```
[EchoReveal] Revealed: Enemy_01 at (10, 0, 5) (radius: 3.2)
[EchoReveal] Hiding: Wall_03
```

### Scene Gizmos
- **Green Spheres** - Reveal radius around objects
- **Yellow Boxes** - Object bounds

### Inspector Monitoring
```csharp
// Add this to see live stats:
[ContextMenu("Show Reveal Stats")]
void ShowStats()
{
    Debug.Log($"Revealed: {revealer.GetRevealedObjectCount()}");
    foreach (var obj in revealer.GetRevealedObjects())
    {
        Debug.Log($"  - {obj.name}");
    }
}
```

## Advanced Customization

### Adjust Fade Times
Edit `EchoRevealSystem.cs`:
```csharp
// Line ~215:
float fadeIn = Mathf.Clamp01(timeAlive / 0.5f);  // Change 0.5
float fadeOut = Mathf.Clamp01(timeToDeath / 1f); // Change 1.0
```

### Change Distance Calculation
Edit shader to use 3D distance instead of 2D:
```hlsl
// Remove this line:
toObject.y = 0;

// For full 3D distance calculation
```

### Custom Reveal Shapes
Modify shader loop to use different shapes (box, cone, etc.)

---

**Created for VISIONSFORETOLD** - True sonar object revelation system.
