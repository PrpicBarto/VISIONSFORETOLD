# ?? Camera Perspective Switcher - Quick Reference

## ? 60-Second Setup

```
1. Select Player
2. Add Component ? Camera Perspective Switcher
3. Play game
4. Press V to switch perspectives
```

Done! Auto-creates both cameras.

## ?? Controls

### Keyboard
- **V** = Switch perspective
- **Right Mouse + Drag** = Rotate third-person camera

### Gamepad
- **Y/Triangle** = Switch perspective (if Input Action set up)
- **Right Stick** = Rotate third-person camera

## ?? Two Perspectives

### Isometric (Top-Down)
```
     Camera
       ?
   [Fixed 45°]
       ?
    Player ?
```
**Fixed angle, see more area**

### Third Person (Behind Player)
```
    Camera
      ? (rotates)
   [? Player]
```
**Follows player, immersive**

## ?? Key Settings

### Quick Tweaks

```csharp
Default Perspective: Isometric/ThirdPerson
Switch Key: V (or any key)
Blend Time: 0.8s (transition speed)

Third Person:
?? Distance: 5.0 (how far behind)
?? Height: 2.0 (how high up)
?? Manual Rotation: ? (allow mouse/stick)

Isometric:
?? Angle: 45° (30-60° typical)
?? Distance: 10.0
?? Height: 10.0
```

## ?? Presets

### Diablo-Style
```
Default: Isometric
Angle: 60°
Distance: 12
```

### Souls-Like
```
Default: ThirdPerson
Distance: 4.0
Height: 1.5
Manual Rotation: ?
```

### Hybrid Tactical
```
Default: Isometric
Blend Time: 0.5 (fast switch)
```

## ?? Scripting

```csharp
// Get component
var switcher = GetComponent<CameraPerspectiveSwitcher>();

// Switch modes
switcher.SwitchToIsometric();
switcher.SwitchToThirdPerson();
switcher.TogglePerspective();

// Check current mode
if (switcher.IsIsometric()) { }
if (switcher.IsThirdPerson()) { }

// Adjust at runtime
switcher.SetThirdPersonDistance(8.0f);
```

## ?? Common Issues

**Not switching?**
? Check component is enabled
? Press V or assigned button

**Camera jumpy?**
? Increase Blend Time to 1.0+
? Enable Smooth Transition

**Can't rotate third person?**
? Check "Enable Manual Camera Rotation"
? Hold right mouse button
? Use right stick

## ?? Pro Tips

1. **Isometric** = Better for seeing many enemies
2. **Third Person** = Better for precision combat
3. **Adjust distances** to match your game's scale
4. **Enable Debug Info** to see current mode
5. **Combine with AimOffsetCamera** for dynamic view

## ?? Visual Comparison

```
Isometric:
? See more enemies
? Strategic positioning
? Twin-stick feel
? Less immersive

Third Person:
? Immersive
? Precision aiming
? Exploration
? Limited visibility
```

## ?? When to Use Each

### Use Isometric For:
- Large enemy groups
- Arena combat
- Area effects
- Strategic gameplay

### Use Third Person For:
- Boss fights
- Exploration
- Precision platforming
- Cinematic moments

## ?? Integration

Works with:
- ? PlayerMovement
- ? PlayerAttack
- ? AimIndicator
- ? AimOffsetCamera
- ? Cinemachine
- ? New Input System

## ?? Camera Angles

```
Isometric Angle Guide:
30° = Shallow (more horizontal)
45° = Classic isometric ? RECOMMENDED
60° = Steep (more top-down)
90° = Pure top-down
```

## ? Input Action Setup

For gamepad button:

```
1. Input Actions asset
2. Add Action: "SwitchCamera"
3. Binding: Button North (Y/Triangle)
4. Save
5. Assign name in Inspector: "SwitchCamera"
```

## ?? Best Settings by Genre

### ARPG (Diablo-like)
```
Default: Isometric
Angle: 55°
Allow switching: ?
```

### Action (Souls-like)
```
Default: ThirdPerson
Distance: 4.0
Allow switching: ? (for boss fights)
```

### Twin-Stick Shooter
```
Default: Isometric
Angle: 90° (pure top-down)
Allow switching: ? (keep isometric only)
```

### Exploration Game
```
Default: ThirdPerson
Blend Time: 1.2 (cinematic)
Allow switching: ?
```

## ? Quick Checklist

Setup:
- [ ] Component on Player
- [ ] Both cameras created (auto)
- [ ] Switch key assigned (V default)
- [ ] Input Action for gamepad (optional)

Test:
- [ ] Press V ? Switches view
- [ ] Smooth transition
- [ ] Third person camera rotates
- [ ] No errors in console

Customize:
- [ ] Adjust distances
- [ ] Set default perspective
- [ ] Configure blend time
- [ ] Test with your game

## ?? Full Documentation

See `CAMERA_SWITCHER_SETUP.md` for:
- Detailed settings
- Advanced configuration
- Troubleshooting
- Scripting examples
- Best practices

**You're all set! Press V to switch perspectives!** ???
