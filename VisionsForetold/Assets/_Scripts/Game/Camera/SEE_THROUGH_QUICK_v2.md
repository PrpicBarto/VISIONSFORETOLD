# ??? See-Through System - Quick Reference (Unity 6 Compatible)

## ? 60-Second Setup

```
1. Create Material ? Shader: Custom/SeeThrough
2. Add "See Through System" to Camera
3. Assign Player as Target
4. Assign Material
5. Make sure enemies are tagged "Enemy"
6. Done! (Enemies automatically included)
```

## ?? What It Does

Makes objects between camera and **player/enemies** transparent so you never lose sight of important characters.

```
Before: Camera ? [WALL] ? ? Can't see player/enemy
After:  Camera ? [????] ? ? See through wall!
```

## ?? Key Settings

```csharp
Target: Player (primary target)
Include Enemies: ? (also track enemies)
Enemy Tag: "Enemy" (auto-finds all enemies)
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

### Disable Enemy Tracking
```
Include Enemies: ? (only track player)
```

## ?? Quick Fixes

**Not working?**
? Check Target is assigned
? Check Material is assigned
? Check shader is Custom/SeeThrough
? Check enemies are tagged "Enemy"

**Enemies not working?**
? Check "Include Enemies" is enabled
? Verify Enemy Tag matches your enemies
? Check enemy GameObjects are active

**Wrong objects transparent?**
? Adjust Obstruction Layers
? Add tags to Ignore Tags

**Performance issues?**
? Increase Check Interval to 0.2s
? Use specific layers only
? Disable enemy tracking if not needed

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
? Player (handled separately)
? Enemy (handled separately)
? UI (ignore)
```

## ?? Use Cases

**Third Person:** Essential!  
**Isometric:** Optional  
**Indoor Scenes:** Very useful  
**Outdoor:** Helpful for trees  
**Boss Fights:** Keep enemies visible!  
**Combat:** Track multiple enemies  

## ? Checklist

- [ ] Material created with shader
- [ ] System on camera
- [ ] Player assigned as target
- [ ] Material assigned
- [ ] Enemies tagged correctly
- [ ] "Include Enemies" enabled
- [ ] Test with objects in way
- [ ] Colors look good
- [ ] Performance is good

## ?? Best Settings

### Standard Third Person + Enemies
```
Check Interval: 0.1s
Transparency: 0.5
Transition: 0.2s
Color: Light Blue
Include Enemies: ?
```

### Fast Action (Player Focus)
```
Check Interval: 0.05s
Transparency: 0.6
Transition: 0.1s
Color: Bright Cyan
Include Enemies: ? (faster)
```

### Cinematic
```
Check Interval: 0.15s
Transparency: 0.4
Transition: 0.4s
Color: Subtle White
Include Enemies: ?
```

### Boss Fight
```
Check Interval: 0.1s
Transparency: 0.7
Transition: 0.15s
Color: Bright Red (for boss)
Include Enemies: ?
```

## ?? Visual Reference

```
Camera Position:
     ??
     |
  [Object] ? Becomes transparent
     |
     ? Player (always visible)
     |
     ? Enemy (also visible!)
```

## ?? Pro Tips

1. **Tag all enemies** as "Enemy" for auto-detection
2. **Use layers** for better control
3. **Match colors** to your game theme
4. **Test in combat** with multiple enemies
5. **Adjust per camera** perspective
6. **Disable for cutscenes** if needed
7. **Enemy tracking** updates every 0.5s (automatic)

## ?? Enemy System Features

**Automatic Detection:**
- Finds all enemies with "Enemy" tag
- Updates list every 0.5 seconds
- Tracks active enemies only
- Removes dead/inactive enemies

**Performance:**
- Efficient enemy tracking
- Separate check interval for enemy updates
- No performance hit with many enemies

**Customization:**
- Change enemy tag if needed
- Enable/disable enemy tracking
- Same transparency as player

## ?? Unity 6 Compatibility

? **Fully Compatible with Unity 6000.2.7f2**
- Works with new Cinemachine
- Updated shader for Unity 6 rendering
- Optimized for performance
- No deprecated APIs

## ?? Files

- `SeeThrough.shader` - The shader (Unity 6 compatible)
- `SeeThroughSystem.cs` - The script (enemies supported!)
- `SEE_THROUGH_SETUP.md` - Full guide

**Never lose sight of your player or enemies!** ????
