# Echolocation Brightness Adjustment Guide

## Quick Fixes for "Too Bright" Shader

### Problem: Shader Appears Too Bright
The echolocation fog might appear washed out, too bright, or glowing too much.

## ? Quick Fixes (In Inspector)

### 1. Reduce Edge Glow Intensity
```
EcholocationController ? Visual Settings
  Edge Intensity: 1.5 ? 0.5 (lower = dimmer)
```

### 2. Darken Fog Color
```
EcholocationController ? Fog & Reveal Settings
  Fog Color: (0.05, 0.05, 0.08) ? (0.02, 0.02, 0.05)
  Make RGB values closer to 0 for darker fog
```

### 3. Increase Fog Density
```
EcholocationController ? Fog & Reveal Settings
  Fog Density: 0.95 ? 0.98
  Higher = more opaque/darker
```

### 4. Reduce Edge Color Brightness
```
EcholocationController ? Visual Settings
  Edge Color: (0.3, 0.6, 1.0) ? (0.1, 0.3, 0.5)
  Lower values = dimmer glow
```

## ?? Preset Configurations

### Very Dark (Horror Game)
```csharp
fogColor = new Color(0.01f, 0.01f, 0.02f, 0.98f);
fogDensity = 0.98f;
edgeColor = new Color(0.1f, 0.2f, 0.3f, 1f);
edgeIntensity = 0.5f;
```

### Moderate Dark (Default)
```csharp
fogColor = new Color(0.05f, 0.05f, 0.08f, 0.95f);
fogDensity = 0.95f;
edgeColor = new Color(0.3f, 0.6f, 1f, 1f);
edgeIntensity = 1.5f;
```

### Light Atmospheric (Exploration)
```csharp
fogColor = new Color(0.1f, 0.1f, 0.15f, 0.8f);
fogDensity = 0.8f;
edgeColor = new Color(0.4f, 0.7f, 1f, 1f);
edgeIntensity = 2.0f;
```

## ?? Advanced Shader Adjustments

If Inspector changes aren't enough, you can edit the shader directly.

### Location
`Assets\_Scripts\Game\Player\Echo\Echolocation.shader`

### 1. Reduce Glow Multiplier
Find this line in the shader:
```hlsl
half glowStrength = saturate(edgeGlow * 0.5); // Current
```

Change to:
```hlsl
half glowStrength = saturate(edgeGlow * 0.25); // Even dimmer
```

### 2. Limit Glow to Fog Areas
The shader already does this with:
```hlsl
finalColor.rgb = _FogColor.rgb + (_EdgeColor.rgb * glowStrength * fogAlpha);
```

The `* fogAlpha` ensures glow only appears where fog exists.

### 3. Adjust Permanent Reveal Intensity
Find this line:
```hlsl
fogAlpha *= (1.0 - permanentReveal * 0.7); // Current
```

Change to reduce reveal brightness:
```hlsl
fogAlpha *= (1.0 - permanentReveal * 0.5); // Less revealing
```

## ?? Debugging Steps

### 1. Check Material Settings
```
1. Find your EcholocationFog material
2. Inspector ? Fog Color ? Should be DARK (near black)
3. Inspector ? Fog Density ? Should be HIGH (0.9+)
4. Inspector ? Edge Intensity ? Should be LOW (0.5-2.0)
```

### 2. Check Scene Lighting
```
Sometimes the shader looks bright because:
- Scene has high ambient light
- Directional light is very bright
- Skybox is too bright

Solution: Test in a darker scene
```

### 3. Check Camera Settings
```
Camera ? Post Processing
  - Bloom might be making glow too bright
  - Exposure might be too high
  
Try disabling post-processing temporarily to test
```

## ?? Brightness Comparison Table

| Setting | Very Dark | Dark | Medium | Light |
|---------|-----------|------|--------|-------|
| Fog Color (R,G,B) | 0.01, 0.01, 0.02 | 0.05, 0.05, 0.08 | 0.1, 0.1, 0.15 | 0.2, 0.2, 0.25 |
| Fog Density | 0.98 | 0.95 | 0.85 | 0.7 |
| Edge Intensity | 0.5 | 1.5 | 2.5 | 4.0 |
| Edge Color Brightness | 0.1-0.3 | 0.3-0.6 | 0.6-0.9 | 0.9-1.0 |

## ?? Runtime Adjustment Script

Add this to dynamically adjust brightness in-game:

```csharp
using VisionsForetold.Game.Player.Echo;

public class EchoBrightnessControl : MonoBehaviour
{
    private EcholocationController echo;

    void Start()
    {
        echo = GetComponent<EcholocationController>();
    }

    void Update()
    {
        // Press - to darken
        if (Input.GetKeyDown(KeyCode.Minus))
        {
            DarkenFog();
        }

        // Press + to brighten
        if (Input.GetKeyDown(KeyCode.Plus))
        {
            BrightenFog();
        }
    }

    void DarkenFog()
    {
        // Access via reflection or make fields public
        // This is pseudocode - adjust based on your setup
        echo.fogDensity = Mathf.Min(1f, echo.fogDensity + 0.05f);
        echo.edgeIntensity = Mathf.Max(0.1f, echo.edgeIntensity - 0.2f);
        
        Debug.Log($"Darkened: Density={echo.fogDensity}, Edge={echo.edgeIntensity}");
    }

    void BrightenFog()
    {
        echo.fogDensity = Mathf.Max(0.5f, echo.fogDensity - 0.05f);
        echo.edgeIntensity = Mathf.Min(5f, echo.edgeIntensity + 0.2f);
        
        Debug.Log($"Brightened: Density={echo.fogDensity}, Edge={echo.edgeIntensity}");
    }
}
```

## ?? Common Causes & Solutions

### Issue: Everything is Washed Out
**Cause:** Edge glow too intense
**Solution:** Set Edge Intensity to 0.5-1.0

### Issue: Can See Everything Too Clearly
**Cause:** Fog density too low
**Solution:** Set Fog Density to 0.95 or higher

### Issue: Pulse Ring Too Bright
**Cause:** Edge Color too bright + high intensity
**Solution:** 
- Lower Edge Color RGB values (e.g., 0.2, 0.4, 0.6)
- Lower Edge Intensity (e.g., 1.0)

### Issue: Revealed Areas Too Bright
**Cause:** Permanent reveal reducing fog too much
**Solution:** Edit shader line:
```hlsl
fogAlpha *= (1.0 - permanentReveal * 0.5); // Was 0.7
```

### Issue: Entire Screen Bright
**Cause:** Fog plane not rendering or material wrong
**Solution:**
- Check fog plane exists in Hierarchy
- Check material is assigned
- Check shader is "Custom/URP/Echolocation"

## ?? Recommended Settings by Game Type

### Survival Horror
```
Fog Color: RGB(0.01, 0.01, 0.02)
Fog Density: 0.98
Edge Intensity: 0.5
Permanent Reveal Radius: 5
```

### Stealth Action
```
Fog Color: RGB(0.03, 0.03, 0.05)
Fog Density: 0.95
Edge Intensity: 1.0
Permanent Reveal Radius: 10
```

### Exploration Adventure
```
Fog Color: RGB(0.08, 0.08, 0.12)
Fog Density: 0.85
Edge Intensity: 2.0
Permanent Reveal Radius: 20
```

### Puzzle Platformer
```
Fog Color: RGB(0.1, 0.1, 0.15)
Fog Density: 0.8
Edge Intensity: 2.5
Permanent Reveal Radius: 15
```

## ? Quick Test Checklist

- [ ] Fog Color is dark (R,G,B all under 0.1)
- [ ] Fog Density is high (0.9+)
- [ ] Edge Intensity is moderate (0.5-2.0)
- [ ] Edge Color is not pure white
- [ ] Post-processing (Bloom) is not over-brightening
- [ ] Camera exposure is reasonable
- [ ] Scene lighting is not too bright

## ?? Understanding the Shader

The brightness comes from three sources:

1. **Fog Base Color** - The dark overlay color
2. **Edge Glow** - The bright ring around pulse edge
3. **Reveal Areas** - Where fog is reduced/removed

The shader now uses **additive glow** instead of **lerp**, which means:
- Glow is ADDED to fog color
- Glow strength is multiplied by fog alpha
- Areas with no fog = no glow

This prevents washed-out brightness!

## ?? Final Tips

1. **Start Dark** - It's easier to brighten than darken
2. **Test in Play Mode** - Lighting looks different in editor
3. **Check on Target Hardware** - Brightness varies by display
4. **Use Fog Density First** - Easiest way to control overall brightness
5. **Edge Intensity Last** - Fine-tune the pulse ring glow

---

**Version:** Updated for dimmer, more atmospheric echolocation
**Last Updated:** After brightness fix (additive glow)
