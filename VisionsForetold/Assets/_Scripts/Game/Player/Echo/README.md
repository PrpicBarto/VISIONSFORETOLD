# Echolocation Fog of War System

A visual fog-of-war system that reveals areas through expanding pulse waves, creating an echolocation/sonar effect.

## Overview

This system creates a dark fog overlay that covers the game world. Periodic pulse waves expand from the player, temporarily revealing the environment as they pass through. The fog gradually returns after each pulse.

## Files

### Core System
- **`EcholocationController.cs`** - Main controller script that manages the fog plane and pulse system
- **`Echolocation.shader`** - URP shader that renders the fog and pulse effects

### Alternative (Not Currently Used)
- **`EcholocationRenderFeature.cs`** - Post-processing render feature (kept for reference)
  - This was the original approach but had issues with black screens
  - The current system uses a fog plane overlay instead

## How It Works

### Fog Plane Approach (Current)
1. Creates a large transparent quad positioned above/at the player
2. The quad uses a shader that renders fog based on distance from player
3. Pulses expand outward, revealing areas inside the pulse radius
4. Fog gradually returns after pulse passes

### Key Features
- ? **Pulse Waves** - Expanding circles that reveal the world
- ? **Permanent Reveal** - Small area around player stays visible
- ? **Fog Fade-back** - Revealed areas gradually return to fog
- ? **Edge Glow** - Bright ring marks the pulse boundary
- ? **Auto-pulse** - Configurable automatic pulsing
- ? **Manual Control** - Can trigger pulses via code

## Setup Instructions

### 1. Create the Material
1. Right-click in Project window ? **Create > Material**
2. Name it `EcholocationFogMaterial`
3. Set shader to **Custom/URP/Echolocation**
4. Configure fog settings in material properties

### 2. Add Controller to Scene
1. Create empty GameObject or attach to Player
2. Add **EcholocationController** component
3. Assign the material to **Fog Material** field
4. Assign **Player** transform (or leave empty to auto-find)

### 3. Press Play!
The system will automatically:
- Create a fog plane
- Start pulsing every 2.5 seconds
- Follow the player around

## Configuration

### Pulse Settings
| Setting | Description | Default |
|---------|-------------|---------|
| **Pulse Speed** | How fast pulse expands (units/sec) | 20 |
| **Max Pulse Radius** | Maximum distance pulse reaches | 40 |
| **Pulse Interval** | Time between auto-pulses | 2.5s |
| **Pulse Width** | Width of visible pulse ring | 5 |
| **Auto Pulse** | Enable automatic pulsing | true |

### Fog Settings
| Setting | Description | Default |
|---------|-------------|---------|
| **Fog Color** | Color of the fog | Very dark blue |
| **Fog Density** | Opacity (0=transparent, 1=solid) | 0.95 |
| **Permanent Reveal Radius** | Area around player that stays visible | 15 |
| **Reveal Duration** | How long fog takes to return | 3s |
| **Reveal Fade Curve** | Animation curve for fog return | Ease In/Out |

### Visual Settings
| Setting | Description | Default |
|---------|-------------|---------|
| **Edge Color** | Color of pulse ring glow | Cyan-blue |
| **Edge Intensity** | Brightness of pulse glow | 3 |

### Fog Plane Configuration
| Setting | Description | Default |
|---------|-------------|---------|
| **Plane Size** | Size of fog plane (X, Y, Z) | 200x1x200 |
| **Plane Height Offset** | Height above player | 0 (ground) |

## Usage Examples

### Manual Pulse Trigger
```csharp
EcholocationController echo = GetComponent<EcholocationController>();
echo.TriggerPulse();
```

### Enable/Disable System
```csharp
echo.SetEnabled(false); // Turn off
echo.SetEnabled(true);  // Turn on
```

### Control Auto-Pulsing
```csharp
echo.StopAutoPulse();  // Stop automatic pulses
echo.StartAutoPulse(); // Resume automatic pulses
```

### Check System State
```csharp
if (echo.IsPulsing)
{
    float radius = echo.CurrentPulseRadius;
    Debug.Log($"Pulse active at radius: {radius}");
}

float timeToNext = echo.TimeUntilNextPulse;
```

## Shader Properties

The shader exposes these properties (set automatically by controller):

| Property | Type | Description |
|----------|------|-------------|
| `_FogColor` | Color | Color of the fog |
| `_FogDensity` | Float | Fog opacity (0-1) |
| `_PulseCenter` | Vector3 | World position of pulse origin |
| `_PulseRadius` | Float | Current pulse radius |
| `_PulseWidth` | Float | Width of pulse ring |
| `_PulseIntensity` | Float | Pulse strength (0-1) |
| `_RevealRadius` | Float | Permanent reveal area size |
| `_RevealFade` | Float | Fog fade-back amount (0-1) |
| `_EdgeColor` | Color | Pulse ring glow color |
| `_EdgeIntensity` | Float | Pulse ring brightness |

## Troubleshooting

### Fog Not Visible
- Check **Plane Height Offset** - should be at or slightly above player height
- Increase **Fog Density** to 1.0 temporarily to test
- Check camera **Culling Mask** includes fog plane layer

### Objects Not Covered by Fog
- Fog plane at ground level only covers ground-level objects
- For tall objects, increase **Plane Height Offset**
- Or position plane above all objects

### Pulse Not Expanding From Player
- Make sure **Player** transform is assigned
- Check Console for warnings about player not found
- Verify player has "Player" tag or assign manually

### Performance Issues
- Reduce **Plane Size** if covering too large an area
- Increase **Pulse Interval** for less frequent pulses
- Disable **Show Debug** in production

## Technical Details

### Distance Calculation
The shader calculates distance in the **XZ plane only** (horizontal), ignoring vertical (Y) distance. This is perfect for top-down and isometric games.

```hlsl
// Project pixel position to ground plane at player height
float3 pixelPosOnGround = worldPos;
pixelPosOnGround.y = playerPos.y;

// Calculate horizontal distance
float dist = length(pixelPosOnGround - playerPos);
```

### Fog Rendering
- Uses **transparent quad** with alpha blending
- Renders in **Transparent+100** queue (on top of most objects)
- No depth writing to avoid Z-fighting

### Performance
- Single draw call for fog plane
- Efficient shader with minimal calculations
- Material properties updated once per frame

## Customization Ideas

### True Sonar Effect
```csharp
permanentRevealRadius = 0f;  // No permanent reveal
fogDensity = 1.0f;           // Complete darkness
edgeIntensity = 5f;          // Bright pulse ring only
```

### Atmospheric Fog
```csharp
permanentRevealRadius = 20f; // Large visible area
fogColor = new Color(0.3f, 0.3f, 0.4f, 0.7f); // Lighter fog
fogDensity = 0.7f;           // Semi-transparent
```

### Fast Scan
```csharp
pulseSpeed = 40f;            // Fast expansion
pulseInterval = 1f;          // Frequent pulses
maxPulseRadius = 60f;        // Large coverage
```

## Version History

### Current Version
- Fog plane overlay approach (reliable, no black screens)
- Full fog-of-war behavior (world hidden by default)
- Distance calculation fixed for player-centered pulses
- Comprehensive documentation and debugging tools

### Previous Approaches
1. **Post-processing (RenderGraph)** - Had black screen issues in Unity 6
2. **Multi-layer fog planes** - Too complex, reverted to single plane
3. **Global shader properties** - Simplified to material properties

## Credits

Created for VISIONSFORETOLD game project.
Uses Unity's Universal Render Pipeline (URP).

## License

Part of the VISIONSFORETOLD project.
