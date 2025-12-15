# 🎸 Black Metal Shader - Quick Reference

## Instant Setup (3 Steps)

### 1. Create Material
```
Right-click → Create → Material
Name: BlackMetalMaterial
Shader: Custom/PostProcess/BlackMetal
```

### 2. Add Renderer Feature
```
Project → Settings → URP Renderer
Add Renderer Feature → Black Metal Render Feature
Assign: BlackMetalMaterial
```

### 3. Adjust Settings
```
Pick a preset or customize:
- Contrast: 2.0 (high = harsh)
- Saturation: 0.1 (low = B&W)
- Grain: 0.3 (moderate)
- Vignette: 0.6 (strong edges)
```

## Presets Cheat Sheet

### Classic (Default)
```
For: General gameplay
Look: Traditional album cover
Settings: Balanced contrast & grain
```

### Atmospheric
```
For: Exploration, ambient
Look: Cold, distant, ethereal
Settings: More fog, softer
```

### Raw
```
For: Combat, intense moments
Look: Brutal, lo-fi, harsh
Settings: Max grain & contrast
```

### Depressive
```
For: Boss fights, dark areas
Look: Very dark, heavy, hopeless
Settings: Minimum saturation
```

## Quick Tweaks

### Too Dark?
```
Brightness: 0.0 to 0.2
Vignette Intensity: 0.4
```

### Too Grainy?
```
Grain Amount: 0.1 to 0.2
Grain Size: 1.5
```

### Too Harsh?
```
Contrast: 1.5 to 1.7
Edge Intensity: 0.1 to 0.2
```

### More Atmospheric?
```
Fog Amount: 0.4 to 0.6
Saturation: 0.15 to 0.2
```

## Settings At-a-Glance

| Category | Key Settings | Quick Adjust |
|----------|--------------|--------------|
| **Style** | Contrast, Saturation, Brightness | ±0.2 steps |
| **Grain** | Amount, Size, Speed | 0.0-1.0 range |
| **Vignette** | Intensity, Smoothness | 0.0-1.0 range |
| **Color** | Tint colors | RGB presets |
| **Fog** | Amount, Color | 0.0-1.0 range |
| **Sharp** | Sharpness | 0.5 = balanced |
| **Edges** | Intensity, Thickness | 0.3 = classic |

## Runtime Control

```csharp
// Get controller
var blackMetal = Camera.main.GetComponent<BlackMetalController>();

// Change intensity (0-1)
blackMetal.SetIntensity(0.8f);

// Switch presets
blackMetal.SwitchPreset(BlackMetalController.BlackMetalPreset.Raw);

// Enable/disable
blackMetal.SetEnabled(true);
```

## Troubleshooting

| Problem | Solution |
|---------|----------|
| Not visible | Check Renderer Feature enabled |
| Pink material | Reimport shader |
| Too heavy | Reduce Edge Detection |
| Too subtle | Increase Contrast + Grain |

## Performance Tips

**Fast:**
- Disable Edge Detection
- Lower Grain Size
- Reduce Edge Thickness

**Quality:**
- Keep all features
- Higher resolution
- More samples

## Perfect Combinations

### Dark Fantasy
```
Contrast: 2.0
Saturation: 0.1
Grain: 0.3
Fog: 0.3
Edge: 0.3
```

### Horror
```
Contrast: 2.2
Saturation: 0.05
Grain: 0.4
Vignette: 0.8
Edge: 0.4
```

### Exploration
```
Contrast: 1.7
Saturation: 0.15
Grain: 0.2
Fog: 0.4
Edge: 0.2
```

### Combat
```
Contrast: 2.5
Saturation: 0.05
Grain: 0.5
Vignette: 0.7
Edge: 0.5
```

## Color Tint Presets

```
Winter/Cold:  (0.8, 0.9, 1.0) - Blue
Hell/Fire:    (1.0, 0.8, 0.7) - Red-orange  
Forest:       (0.7, 1.0, 0.8) - Green
Night:        (0.8, 0.7, 1.0) - Purple
Monochrome:   (0.9, 0.9, 0.9) - Gray
```

## Files Created

```
✓ BlackMetalPostProcess.shader (shader)
✓ BlackMetalRenderFeature.cs (URP integration)
✓ BlackMetalController.cs (runtime control)
✓ BLACK_METAL_SETUP_GUIDE.md (full guide)
✓ BLACK_METAL_QUICK_REF.md (this file)
```

## Next Steps

1. Create material with shader
2. Add Renderer Feature
3. Test in Play mode
4. Adjust to taste
5. Save as preset
6. Enjoy the darkness! 🎸🖤

**Your game now has iconic black metal aesthetics!** ✨
