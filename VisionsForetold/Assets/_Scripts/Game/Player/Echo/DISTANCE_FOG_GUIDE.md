# Distance-Based Fog Density Guide

## Overview

The echolocation fog now features **distance-based density** that makes fog **denser the further it is from the player**. This creates a more realistic and atmospheric effect where nearby areas are slightly visible, but distant areas are completely obscured.

## How It Works

### Distance Gradient
```
Player Position ? Light fog (min density)
      ?
   Distance
      ?
Far from Player ? Heavy fog (max density)
```

### Formula
```hlsl
distanceRatio = saturate(distance / falloffDistance)
density = minDensity + (distanceRatio² × (maxDensity - minDensity))
fogAlpha *= density
```

**Key Points:**
- Uses **quadratic falloff** (distanceRatio²) for dramatic effect
- **Smooth transition** from light to heavy fog
- **Configurable** min/max density and falloff distance

## Configuration

### Inspector Settings

Located in `EcholocationController ? Distance-Based Fog Density`:

| Setting | Description | Default | Range |
|---------|-------------|---------|-------|
| **Fog Distance Falloff** | Distance at which fog reaches maximum density | 100 | 1+ |
| **Fog Min Density** | Fog opacity near player | 0.3 | 0-1 |
| **Fog Max Density** | Fog opacity far from player | 1.0 | 0-1 |

### Quick Presets

#### Light Gradient (Subtle)
```csharp
fogDistanceFalloff = 150f;
fogMinDensity = 0.5f;
fogMaxDensity = 0.9f;
```
Effect: Gentle transition, still visible at distance

#### Medium Gradient (Default)
```csharp
fogDistanceFalloff = 100f;
fogMinDensity = 0.3f;
fogMaxDensity = 1.0f;
```
Effect: Balanced visibility and atmosphere

#### Heavy Gradient (Dramatic)
```csharp
fogDistanceFalloff = 50f;
fogMinDensity = 0.1f;
fogMaxDensity = 1.0f;
```
Effect: Rapid fog increase, very limited visibility

#### Extreme Gradient (Horror)
```csharp
fogDistanceFalloff = 30f;
fogMinDensity = 0.0f;
fogMaxDensity = 1.0f;
```
Effect: Almost no fog near player, pitch black at distance

## Visual Examples

### Before (Uniform Fog)
```
Player [====================] Far Distance
       Same fog everywhere
```

### After (Distance-Based)
```
Player [??????????????????] Far Distance
Clear ? Light ? Dense ? Opaque
```

## Tuning Guide

### Fog Distance Falloff

**What it controls:** How far you can see before fog reaches maximum

```csharp
// Short range (claustrophobic):
fogDistanceFalloff = 30f;

// Medium range (balanced):
fogDistanceFalloff = 100f;

// Long range (open):
fogDistanceFalloff = 200f;
```

### Fog Min Density

**What it controls:** How clear areas near the player are

```csharp
// Very clear near player:
fogMinDensity = 0.0f;  // Almost no fog

// Slightly obscured:
fogMinDensity = 0.3f;  // Light haze

// Still foggy:
fogMinDensity = 0.6f;  // Visible but foggy
```

### Fog Max Density

**What it controls:** How opaque distant areas are

```csharp
// Still see through:
fogMaxDensity = 0.7f;  // Translucent

// Mostly blocked:
fogMaxDensity = 0.9f;  // Nearly opaque

// Complete blackout:
fogMaxDensity = 1.0f;  // Totally opaque
```

## Interaction with Other Systems

### Permanent Reveal Radius
The permanent reveal area **overrides** distance-based fog:
```
Near player (within reveal radius):
  - Fog cleared by permanent reveal
  - Distance-based density doesn't apply here

Outside reveal radius:
  - Distance-based density takes effect
  - Fog gradually increases with distance
```

### Pulse Reveals
When pulse expands:
1. Distance fog still applies to base layer
2. Pulse reveal **clears** fog inside pulse
3. Creates dramatic "wave of visibility" effect

### Object Reveals (EchoRevealSystem)
Revealed objects clear fog locally:
- Distance fog still present around them
- Creates "islands of visibility" in dense fog
- More dramatic when fog is very dense

## Game-Specific Recommendations

### Horror Game
```csharp
fogDistanceFalloff = 40f;   // Very short
fogMinDensity = 0.2f;       // Some fog even close
fogMaxDensity = 1.0f;       // Total darkness far
```
Result: Oppressive, claustrophobic atmosphere

### Stealth Game
```csharp
fogDistanceFalloff = 80f;   // Medium
fogMinDensity = 0.4f;       // Limited near vision
fogMaxDensity = 0.95f;      // Almost blind at distance
```
Result: Strategic visibility management

### Exploration Game
```csharp
fogDistanceFalloff = 150f;  // Long
fogMinDensity = 0.3f;       // Clear near player
fogMaxDensity = 0.85f;      // Can see shapes far away
```
Result: Atmospheric but not limiting

### Puzzle Game
```csharp
fogDistanceFalloff = 60f;   // Medium-short
fogMinDensity = 0.2f;       // Very clear near
fogMaxDensity = 1.0f;       // Focus on nearby puzzles
```
Result: Forces focus on local area

## Advanced Customization

### Linear Falloff (instead of Quadratic)

Edit shader, find this line:
```hlsl
float distanceDensityMultiplier = _FogMinDensity + (distanceRatio * distanceRatio * (_FogMaxDensity - _FogMinDensity));
```

Change to:
```hlsl
// Linear falloff (smoother):
float distanceDensityMultiplier = _FogMinDensity + (distanceRatio * (_FogMaxDensity - _FogMinDensity));

// Cubic falloff (more dramatic):
float distanceDensityMultiplier = _FogMinDensity + (pow(distanceRatio, 3.0) * (_FogMaxDensity - _FogMinDensity));
```

### Exponential Falloff

For even more dramatic fog:
```hlsl
// Exponential falloff:
float distanceDensityMultiplier = _FogMinDensity + (exp(distanceRatio * 2.0) - 1.0) / (exp(2.0) - 1.0) * (_FogMaxDensity - _FogMinDensity);
```

### Height-Based Density

Add vertical fog variation:
```hlsl
// In shader, after distance calculation:
float heightFactor = saturate((input.positionWS.y - _PulseCenter.y) / 10.0);
distanceDensityMultiplier *= (1.0 + heightFactor * 0.5); // More fog above player
```

## Troubleshooting

### Fog Too Dense Everywhere
**Cause:** Min Density too high or Fog Density base value too high

**Solution:**
```csharp
fogMinDensity = 0.1f;  // Lower near-player fog
fogDensity = 0.8f;     // Lower base fog
```

### No Visible Gradient
**Cause:** Min and Max Density too similar

**Solution:**
```csharp
fogMinDensity = 0.2f;
fogMaxDensity = 1.0f;
// Increase difference for stronger gradient
```

### Can See Too Far
**Cause:** Distance Falloff too large

**Solution:**
```csharp
fogDistanceFalloff = 50f;  // Reduce from 100
```

### Fog Transition Too Abrupt
**Cause:** Distance Falloff too small

**Solution:**
```csharp
fogDistanceFalloff = 150f;  // Increase from 100
```

### Performance Issues
**Note:** Distance-based fog has **minimal performance impact**
- Calculated once per pixel
- Simple arithmetic operations
- No additional texture lookups

If experiencing issues, check other systems (pulse, reveals, etc.)

## Combining with Base Fog Density

The system uses **multiplication**, so:
```
Final Fog = Base Density × Distance Multiplier

Example:
  Base Density = 0.95
  Distance Multiplier = 0.3 (near) to 1.0 (far)
  
  Near Player: 0.95 × 0.3 = 0.285 (light)
  Far Away:    0.95 × 1.0 = 0.95 (heavy)
```

**Tip:** Keep Base Density high (0.9-0.95) and control visibility with Min/Max Density

## Testing Checklist

- [ ] Walk away from start position
- [ ] Fog should get gradually darker/denser
- [ ] Near player should be lighter
- [ ] Transition should be smooth (not abrupt)
- [ ] Pulse reveals should still work
- [ ] Permanent reveal area still visible
- [ ] Revealed objects clear fog locally

## Example Scenarios

### Scenario 1: Dark Cave
```csharp
fogColor = Color.black;
fogDensity = 0.98f;
fogDistanceFalloff = 40f;
fogMinDensity = 0.3f;
fogMaxDensity = 1.0f;
```
Effect: Player has small visible area, darkness beyond

### Scenario 2: Misty Forest
```csharp
fogColor = new Color(0.7f, 0.75f, 0.8f, 0.8f); // Light blue-gray
fogDensity = 0.7f;
fogDistanceFalloff = 120f;
fogMinDensity = 0.2f;
fogMaxDensity = 0.9f;
```
Effect: Atmospheric mist, can see shapes at distance

### Scenario 3: Underwater
```csharp
fogColor = new Color(0.0f, 0.2f, 0.3f, 0.9f); // Dark blue
fogDensity = 0.95f;
fogDistanceFalloff = 60f;
fogMinDensity = 0.4f;
fogMaxDensity = 1.0f;
```
Effect: Murky water effect, visibility drops quickly

## Performance Notes

### GPU Cost
- **Addition:** 1 texture sample (distance calculation)
- **Math:** 2-3 simple operations per pixel
- **Impact:** Negligible (<0.05ms on most hardware)

### Optimization Tips
1. No additional textures needed ?
2. Calculation is per-pixel but very fast ?
3. No loops or branches ?
4. Works well on mobile ?

---

**Version:** Updated with distance-based fog density
**Feature:** Progressive fog density based on distance from player
**Performance:** Minimal impact, recommended for all projects
