# Fog of War Fade-Back Effect - Update Summary

## ? IMPLEMENTED

The echolocation fog now **gradually fades back** after the pulse has passed, creating a more realistic sonar effect where revealed areas don't instantly disappear.

---

## ?? What Changed

### **Before:**
```
Pulse passes ? Fog instantly returns
[Revealed] ? [INSTANT FOG]
```

### **After:**
```
Pulse passes ? Fog slowly fades back in
[Revealed] ? [Fading...] ? [Fog restored]
```

---

## ?? Changes Made

### **1. Shader Updates (Echolocation.shader)**

**Added Properties:**
```hlsl
_PulseAge ("Pulse Age (Time)", Float) = 0
_PulseFadeDelay ("Fade Delay After Pulse", Float) = 0.5
_PulseFadeSpeed ("Fade Back Speed", Float) = 2.0
```

**Updated Fragment Shader:**
- Now tracks areas **behind** the pulse ring
- Calculates time since pulse passed each pixel
- Applies gradual fade based on time and distance
- Uses configurable delay and speed parameters

**Fade Logic:**
```hlsl
// Calculate time since pulse passed this pixel
timeSincePulsePassed = distanceBehindPulse / pulseRadius * pulseAge

// Apply delay before starting fade
fadeAmount = (timeSincePulsePassed - delay) * speed

// Gradually reduce reveal amount
pulseReveal = (1.0 - fadeAmount) * intensity
```

---

### **2. C# Script Updates (EchoController.cs)**

**Added Tracking:**
```csharp
private float pulseAge; // Tracks time since pulse started
```

**Updated Property ID:**
```csharp
private static readonly int PulseAgeID = Shader.PropertyToID("_PulseAge");
```

**Modified Methods:**
- `UpdatePulseAnimation()` - Increments `pulseAge` each frame
- `UpdateMaterialProperties()` - Sends `pulseAge` to shader
- `TriggerPulse()` - Resets `pulseAge` to 0

---

## ?? Configuration

### **Shader Material Properties:**

| Property | Description | Default | Recommended Range |
|----------|-------------|---------|-------------------|
| **Pulse Fade Delay** | Delay before fog starts fading back | 0.5 | 0.0 - 2.0 |
| **Pulse Fade Speed** | How fast fog fades back | 2.0 | 0.5 - 5.0 |

### **Usage Examples:**

#### Quick Fade (Fast-Paced Action)
```
Pulse Fade Delay: 0.2
Pulse Fade Speed: 4.0
Result: Fog returns quickly after pulse
```

#### Slow Fade (Exploration)
```
Pulse Fade Delay: 1.0
Pulse Fade Speed: 1.0
Result: Long visibility window for exploring
```

#### No Fade (Instant Return)
```
Pulse Fade Speed: 10.0
Result: Fog returns almost instantly
```

#### Long Memory (Generous)
```
Pulse Fade Delay: 2.0
Pulse Fade Speed: 0.5
Result: Areas stay visible for ~4+ seconds
```

---

## ?? How It Works

### **Visual Timeline:**

```
T=0s:  Pulse starts at player
       [?]         Radius = 0

T=1s:  Pulse expanding
       [???????]   Radius = 20m
       Behind pulse: Revealed, starting to fade

T=2s:  Pulse at max radius
       [???????????] Radius = 40m
       Close to player: Fog fading back in
       Near pulse: Still revealed

T=3s:  Pulse complete
       [           ] Radius = 0
       All areas: Fog fading back gradually
       Areas closer to start fade first
```

### **Fade Gradient:**

```
Distance from player ? Time since pulse passed ? Fade amount

Player Position:
[????????] Fully fogged (passed 2s ago, faded)
    ?
[????????] Mostly fogged (passed 1s ago, fading)
    ?
[????????] Lightly fogged (passed 0.5s ago, just starting)
    ?
[        ] Clear (pulse just passed, no fade yet)
    ?
[========] Pulse Ring (actively revealing)
```

---

## ?? Technical Details

### **Shader Calculations:**

```hlsl
// 1. Calculate distance to pulse ring
distanceToPulse = pixelDistance - pulseRadius

// 2. If behind pulse (negative distance)
if (distanceToPulse < 0)
{
    // 3. Normalize how far behind
    timeSincePulsePassed = abs(distanceToPulse) / pulseRadius
    
    // 4. Scale by actual time
    timeSincePulsePassed *= pulseAge
    
    // 5. Apply delay and speed
    fadeAmount = saturate((timeSincePulsePassed - fadeDelay) * fadeSpeed)
    
    // 6. Calculate reveal (1.0 = fully revealed, 0.0 = fogged)
    pulseReveal = (1.0 - fadeAmount) * pulseIntensity
}
```

### **Key Insights:**

1. **Distance-Based Timing:**
   - Pixels closer to player fade first
   - Creates natural "wave" effect of fog returning

2. **Delay Parameter:**
   - Prevents instant fade-back
   - Gives brief moment to see revealed area

3. **Speed Parameter:**
   - Controls how quickly transition happens
   - Higher = faster fade, lower = slower

4. **Pulse Age:**
   - Global timer for entire pulse
   - Ensures consistent timing across all pixels

---

## ?? In-Game Effect

### **What Players Experience:**

1. **Pulse Expands:**
   - Fog clears as pulse moves outward
   - Edge glow marks the pulse ring

2. **Behind Pulse:**
   - Areas remain visible briefly
   - Fog starts creeping back gradually
   - Creates "memory" of what was just revealed

3. **After Pulse Complete:**
   - All areas slowly return to fog
   - Areas further from player fade faster
   - Natural, organic feel

### **Gameplay Impact:**

? **Improved:**
- More realistic sonar effect
- Better sense of pulse "passing through"
- Players have time to process revealed information
- Less jarring transitions

? **Balanced:**
- Not overpowered (fog still returns)
- Configurable timing for difficulty tuning
- Maintains fog-of-war challenge

---

## ?? Performance

### **Impact:**
- **Minimal** - One additional float calculation per pixel
- **Shader-side only** - No extra CPU work
- **No frame rate impact** on modern hardware

### **Optimization:**
- Calculations only happen behind pulse
- Uses `saturate()` for GPU-friendly clamping
- No texture lookups or complex operations

---

## ?? Customization Examples

### **Instant Return (Classic)**
```
Material Inspector:
?? Pulse Fade Delay: 0.0
?? Pulse Fade Speed: 10.0
```

### **Slow Cinematic Fade**
```
Material Inspector:
?? Pulse Fade Delay: 1.5
?? Pulse Fade Speed: 0.5
```

### **Balanced Default**
```
Material Inspector:
?? Pulse Fade Delay: 0.5
?? Pulse Fade Speed: 2.0
```

### **Long Memory (Easy Mode)**
```
Material Inspector:
?? Pulse Fade Delay: 2.0
?? Pulse Fade Speed: 0.3
```

---

## ?? Troubleshooting

### **Fog Not Fading Back:**

Check:
- [ ] `_PulseFadeSpeed` is > 0
- [ ] `_PulseAge` is being set by script
- [ ] Material is using updated shader

### **Fade Too Fast:**

Solution:
```
Reduce Pulse Fade Speed: 2.0 ? 1.0
Increase Pulse Fade Delay: 0.5 ? 1.0
```

### **Fade Too Slow:**

Solution:
```
Increase Pulse Fade Speed: 2.0 ? 4.0
Reduce Pulse Fade Delay: 0.5 ? 0.2
```

### **Uneven Fading:**

This is **intentional**:
- Areas closer to player fade first
- Creates natural wave effect
- If you want uniform fade, increase speed dramatically

---

## ?? Summary

**What You Get:**
- ? Gradual fog fade-back after pulse
- ? Configurable delay before fading starts
- ? Adjustable fade speed
- ? Distance-based fade timing (closer fades first)
- ? No performance impact
- ? Natural, realistic sonar effect

**How to Use:**
1. Material has new properties automatically
2. Adjust `Pulse Fade Delay` and `Pulse Fade Speed`
3. Test in-game to find preferred feel
4. Done!

---

**Files Modified:**
- ? `Echolocation.shader` - Added fade-back logic
- ? `EchoController.cs` - Added pulse age tracking

**Backwards Compatible:**
- ? Existing materials work (use defaults)
- ? No breaking changes
- ? Can disable by setting speed = 10+

---

Enjoy your realistic fading fog-of-war effect! ????
