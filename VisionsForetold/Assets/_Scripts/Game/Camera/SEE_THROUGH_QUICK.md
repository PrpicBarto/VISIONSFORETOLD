# ??? See-Through System - Quick Reference

## ? 60-Second Setup

```
1. Create Material ? Shader: Custom/SeeThrough
2. Add "See Through System" to Camera
3. Assign Player as Target
4. Assign Material
5. Done!
```

## ?? What It Does

Makes objects between camera and player **transparent** so you never lose sight of your character.

```
Before: Camera ? [WALL] ? ? Can't see player
After:  Camera ? [????] ? ? See through wall!
```

## ?? Key Settings

```csharp
Target: Player (what to keep visible)
See Through Material: Your material
See Through Color: (0.5, 0.8, 1.0, 0.5) // Light blue
Transparency Amount: 0.5 (50% transparent)
Check Interval: 0.1s (how often to check)
Transition Time: 0.2s (fade speed)
```

## ?? Color Presets

```
Light Blue:  (0.5, 0.8, 1.0, 0.5) // Tech/Modern
Light Green: (0.5, 1.0, 0.5, 0.5) // Nature/Fantasy
Light Pink:  (1.0, 0.5, 0.8, 0.5) // Magical
White:       (0.8, 0.8, 0.8, 0.3) // Clean/Realistic
```

## ?? Transparency Guide

```
0.3 = Subtle (realistic)
0.5 = Balanced (default)
0.7 = Strong (game-like)
0.9 = Nearly invisible
```

## ?? Common Adjustments

### More Responsive
```
Check Interval: 0.05s
Transition Time: 0.1s
```

### Smoother
```
Check Interval: 0.2s
Transition Time: 0.4s
```

### Stronger Effect
```
Transparency Amount: 0.7
See Through Color: Brighter
```

### Subtle Effect
```
Transparency Amount: 0.3
See Through Color: (0.8, 0.8, 0.8, 0.3)
```

## ?? Quick Fixes

**Not working?**
? Check Target is assigned
? Check Material is assigned
? Check shader is Custom/SeeThrough

**Wrong objects transparent?**
? Adjust Obstruction Layers
? Add tags to Ignore Tags

**Performance issues?**
? Increase Check Interval to 0.2s
? Use specific layers only

## ?? Scripting

```csharp
var seeThrough = GetComponent<SeeThroughSystem>();

// Change target
seeThrough.SetTarget(boss.transform);

// Adjust transparency
seeThrough.SetTransparency(0.8f);

// Enable/disable
seeThrough.SetEnabled(true);
```

## ?? Layer Setup

```
Recommended Layers:
? Environment
? Obstacles
? Player (ignore)
? Enemy (ignore)
? UI (ignore)
```

## ?? Use Cases

**Third Person:** Essential!  
**Isometric:** Optional  
**Indoor Scenes:** Very useful  
**Outdoor:** Helpful for trees  
**Boss Fights:** Keep target visible  

## ? Checklist

- [ ] Material created with shader
- [ ] System on camera
- [ ] Player assigned as target
- [ ] Material assigned
- [ ] Test with objects in way
- [ ] Colors look good
- [ ] Performance is good

## ?? Best Settings

### Standard Third Person
```
Check Interval: 0.1s
Transparency: 0.5
Transition: 0.2s
Color: Light Blue
```

### Fast Action
```
Check Interval: 0.05s
Transparency: 0.6
Transition: 0.1s
Color: Bright Cyan
```

### Cinematic
```
Check Interval: 0.15s
Transparency: 0.4
Transition: 0.4s
Color: Subtle White
```

## ?? Visual Reference

```
Camera Position:
     ??
     |
  [Object] ? Becomes transparent
     |
     ?  ? Player (always visible)
```

## ?? Pro Tips

1. **Use layers** for better control
2. **Match colors** to your game theme
3. **Test in different areas** (indoors/outdoors)
4. **Adjust per camera** perspective
5. **Disable for cutscenes** if needed

## ?? Files

- `SeeThrough.shader` - The shader
- `SeeThroughSystem.cs` - The script
- `SEE_THROUGH_SETUP.md` - Full guide

**Never lose sight of your player!** ????
