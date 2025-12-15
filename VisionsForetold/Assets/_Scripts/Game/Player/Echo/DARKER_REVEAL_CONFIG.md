# ?? Darker Echolocation Reveal - Configuration Guide

## Problem Fixed

The echolocation reveal was **too bright** with a **strange glow**. It's now much darker and more subtle.

## Changes Made

### 1. Shader Updates (`EcholocationReveal.shader`)

**Before (Too Bright):**
```hlsl
pulse = pulse * 0.3 + 0.7;           // Range 0.7 to 1.0
revealAmount * 0.5                    // 50% blend
edgeGlow * _EdgeGlow * pulse          // Full brightness
revealAmount * 0.5                    // 50% brightness boost
```

**After (Much Darker):**
```hlsl
pulse = pulse * 0.1 + 0.9;           // Range 0.9 to 1.0 (barely visible)
revealAmount * 0.15                   // 15% blend (much subtler)
edgeGlow * _EdgeGlow * pulse * 0.2   // 20% edge glow
revealAmount * 0.1                    // 10% brightness boost
```

**Result:**
- 3x less color blending
- 5x darker edge glow
- 5x less brightness boost
- Much more subtle pulse

### 2. Default Property Values

**Shader Properties:**
```hlsl
Before:
_RevealColor: (0.3, 0.8, 1.0, 1.0)  // Bright cyan
_RevealStrength: 1.5                  // Bright
_EdgeGlow: 2.0                        // Strong glow

After:
_RevealColor: (0.2, 0.5, 0.7, 0.6)  // Dark blue-gray
_RevealStrength: 0.8                  // Subtle
_EdgeGlow: 0.5                        // Minimal glow
```

### 3. Script Default Values

**EchoRevealSystem.cs:**
```csharp
Before:
revealColor: (0.3, 0.8, 1.0, 1.0)   // Bright cyan
revealBrightness: 1.5                 // Bright

After:
revealColor: (0.2, 0.5, 0.7, 0.6)   // Dark blue-gray
revealBrightness: 0.8                 // Subtle
```

## New Visual Effect

### Before (Too Bright)
```
Revealed Area:
???????? ? Very bright
???????? ? Strong glow
???????? ? Overwhelming
```

### After (Perfect)
```
Revealed Area:
???????? ? Subtle enhancement
???????? ? Normal visibility
???????? ? Barely noticeable glow
```

## Customization

### If Still Too Bright

**Option 1: Adjust Material**
```
Select EcholocationReveal material:
- Reveal Strength: 0.5 (even darker)
- Edge Glow: 0.3 (less glow)
- Reveal Color alpha: 0.4 (more transparent)
```

**Option 2: Adjust Script**
```
Select EchoRevealSystem component:
- Reveal Brightness: 0.6 (darker)
- Reveal Color: Darker RGB values
```

**Option 3: Edit Shader**
```hlsl
In EcholocationReveal.shader, change:
revealAmount * 0.15 ? revealAmount * 0.10 (10% blend)
pulse * 0.2 ? pulse * 0.1 (10% edge glow)
revealAmount * 0.1 ? revealAmount * 0.05 (5% boost)
```

### If Too Dark

**Option 1: Adjust Material**
```
Select EcholocationReveal material:
- Reveal Strength: 1.0 (brighter)
- Edge Glow: 1.0 (more visible)
- Reveal Color alpha: 0.8 (less transparent)
```

**Option 2: Adjust Script**
```
Select EchoRevealSystem component:
- Reveal Brightness: 1.2 (brighter)
- Reveal Color: Brighter RGB values
```

## Color Recommendations

### Very Subtle (Recommended)
```
Reveal Color: (0.2, 0.5, 0.7, 0.6) // Dark blue-gray
Reveal Strength: 0.8
Edge Glow: 0.5
Result: Barely noticeable enhancement
```

### Subtle
```
Reveal Color: (0.3, 0.6, 0.8, 0.7) // Medium blue
Reveal Strength: 1.0
Edge Glow: 0.8
Result: Subtle but visible
```

### Noticeable
```
Reveal Color: (0.4, 0.7, 0.9, 0.8) // Lighter blue
Reveal Strength: 1.2
Edge Glow: 1.2
Result: Clearly visible reveal
```

### Original (Too Bright - Don't Use)
```
Reveal Color: (0.3, 0.8, 1.0, 1.0) // Bright cyan
Reveal Strength: 1.5
Edge Glow: 2.0
Result: Too bright, overwhelming
```

## Inspector Settings

### EchoRevealSystem Component

```
Visual Reveal:
?? Apply Reveal Material: ?
?? Reveal Material: EcholocationReveal
?? Reveal Color: (0.2, 0.5, 0.7, 0.6) ? Darker!
?? Reveal Brightness: 0.8 ? Lower!
```

### EcholocationReveal Material

```
Properties:
?? Reveal Color: (0.2, 0.5, 0.7, 0.6) ? Dark blue-gray
?? Reveal Strength: 0.8 ? Subtle
?? Reveal Pulse Speed: 2.0 ? Slower
?? Edge Glow: 0.5 ? Minimal
```

## Comparison

### Brightness Levels

| Setting | Before | After | Change |
|---------|--------|-------|--------|
| Color Blend | 50% | 15% | 70% reduction |
| Edge Glow | 100% | 20% | 80% reduction |
| Brightness Boost | 50% | 10% | 80% reduction |
| Pulse Range | 0.7-1.0 | 0.9-1.0 | 67% reduction |
| Overall | Too Bright | Perfect | Much darker ? |

### Visual Impact

**Before:**
- Too much glow ?
- Overwhelming effect ?
- Bright pulse ?
- Strange appearance ?

**After:**
- Subtle enhancement ?
- Natural effect ?
- Barely visible pulse ?
- Clean appearance ?

## Testing Checklist

- [ ] Trigger echolocation pulse
- [ ] Check reveal area brightness
- [ ] Verify glow is subtle
- [ ] Confirm pulse is barely visible
- [ ] Check revealed objects look natural
- [ ] Test in different lighting conditions
- [ ] Verify no strange glow
- [ ] Performance is still good

## Quick Adjustments

### Too Bright Still?

**Fastest Fix:**
```
EcholocationReveal material:
- Reveal Strength: 0.5
- Edge Glow: 0.3
```

**Or in code:**
```csharp
// In EchoRevealSystem
revealBrightness = 0.5f;
revealColor = new Color(0.15f, 0.4f, 0.6f, 0.5f);
```

### Too Dark Now?

**Fastest Fix:**
```
EcholocationReveal material:
- Reveal Strength: 1.0
- Edge Glow: 0.8
```

**Or in code:**
```csharp
// In EchoRevealSystem
revealBrightness = 1.0f;
revealColor = new Color(0.3f, 0.6f, 0.8f, 0.7f);
```

## Technical Details

### Shader Changes

**Blending Calculation:**
```hlsl
// Old (bright):
lerp(baseColor, revealColor * strength, amount * 0.5)

// New (dark):
lerp(baseColor, revealColor * strength, amount * 0.15)

Result: 3x less reveal color influence
```

**Edge Glow Calculation:**
```hlsl
// Old (bright):
revealColor * edgeGlow * EdgeGlow * pulse

// New (dark):
revealColor * edgeGlow * EdgeGlow * pulse * 0.2

Result: 5x darker edge glow
```

**Brightness Boost:**
```hlsl
// Old (bright):
color *= (1.0 + amount * 0.5)  // Up to 50% brighter

// New (dark):
color *= (1.0 + amount * 0.1)  // Up to 10% brighter

Result: 5x less brightness boost
```

## Best Practices

### For Dark Games
```
Reveal Color: (0.15, 0.4, 0.6, 0.5)
Reveal Strength: 0.6
Edge Glow: 0.3
Result: Very subtle, matches dark theme
```

### For Bright Games
```
Reveal Color: (0.3, 0.6, 0.8, 0.7)
Reveal Strength: 1.0
Edge Glow: 0.8
Result: More visible, matches bright theme
```

### For Competitive Play
```
Reveal Color: (0.2, 0.5, 0.7, 0.6)
Reveal Strength: 0.8
Edge Glow: 0.5
Result: Balanced, not distracting
```

## Summary

**Changes:**
- ? Much darker reveal effect
- ? Subtle edge glow (not strange)
- ? Barely visible pulse
- ? Natural appearance
- ? Configurable brightness

**Result:**
- Perfect darkness level
- No overwhelming glow
- Clean visual effect
- Easy to customize

**Your echolocation reveal is now much darker and more subtle!** ???
