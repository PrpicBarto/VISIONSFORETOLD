# ?? Aim Camera System - Quick Reference

## ?? What You Get

**Two Components:**
1. **AimOffsetCamera** - Shifts camera based on aim direction
2. **AimIndicator** - Visual crosshair showing aim point

**Works with:**
- ? Mouse (automatic)
- ? Gamepad (automatic)
- ? Your existing PlayerMovement
- ? Your existing PlayerAttack
- ? Cinemachine

## ? 60-Second Setup

### On Cinemachine Virtual Camera:
```
1. Add Component ? AimOffsetCamera
2. Done! (auto-finds player & aim target)
```

### On Player:
```
1. Add Component ? AimIndicator
2. Done! (auto-finds references)
```

**That's it!** Play and test.

## ?? How It Works

```
You aim ? Camera shifts to show aim direction
        ? Indicator appears at aim point
        ? Color changes per attack mode
```

### Mouse Input:
- Move mouse ? Aim target moves ? Camera follows
- Smooth, responsive, natural

### Gamepad Input:
- Right stick ? Aim target moves ? Camera follows
- Same smooth behavior

## ?? Key Settings (Start Here)

### AimOffsetCamera
```
Max Camera Offset: 3.0    (how far camera shifts)
Camera Smooth Speed: 5.0  (smoothness)
Enable Look Ahead: ?      (show more ahead)
Enable Aim Zoom: ?        (optional zoom)
```

### AimIndicator
```
Show Indicator: ?         (visual crosshair)
Indicator Size: 0.5       (size in meters)
Rotate Indicator: ?       (spin animation)
Use Attack Mode Colors: ? (red/green/cyan)
```

## ?? Visual Feedback

**Indicator Colors (Attack Modes):**
- **Red** = Melee mode
- **Green** = Ranged mode
- **Cyan** = Spell mode

**Indicator States:**
- Idle = Hidden
- Near aim = Faded
- Active = Full opacity + rotating

## ?? Recommended Presets

### Hack & Slash (Melee Focus)
```
Camera:
- Max Offset: 2.0
- Smooth Speed: 8.0
- Zoom: OFF
- Look Ahead: ON

Indicator:
- Size: 0.5
- Rotate: ON
- Line: OFF
```

### Twin-Stick Shooter (Ranged)
```
Camera:
- Max Offset: 4.0
- Smooth Speed: 4.0
- Zoom: ON (45° FOV)
- Look Ahead: ON

Indicator:
- Size: 0.7
- Rotate: OFF
- Line: ON
```

### Tactical/Souls-like
```
Camera:
- Max Offset: 2.5
- Smooth Speed: 3.0
- Dead Zone: 0.2
- Zoom: OFF

Indicator:
- Show: OFF (no indicator)
```

## ?? Quick Fixes

**Camera not moving?**
? Increase Max Camera Offset to 5.0

**Too jerky?**
? Increase Smooth Speed to 10.0

**Indicator not showing?**
? Uncheck "Hide When Idle"

**Wrong colors?**
? Check "Use Attack Mode Colors"

## ?? Inspector Reference

### Must Assign (Auto-Found)
- Player Transform ?
- Aim Target ?
- Virtual Camera ?

### Everything Else
- Automatic!

## ?? Demo Scene Setup

1. Create new scene
2. Add Player with PlayerMovement
3. Add Cinemachine Virtual Camera
4. Add AimOffsetCamera to camera
5. Add AimIndicator to player
6. Play!

## ?? Pro Tips

1. **Start with defaults** - They work for most games
2. **Adjust Max Offset first** - This has the biggest impact
3. **Enable Debug UI** to see what's happening
4. **Test with gamepad** - Make sure it feels good
5. **Try different colors** - Match your game's style

## ?? Integration

**No code needed!** Just add components and play.

**Optional scripting:**
```csharp
// Adjust camera at runtime
aimCamera.SetMaxOffset(5.0f);

// Change indicator color
indicator.SetColor(Color.yellow);
```

## ?? Scene View Debugging

Select camera in Hierarchy to see:
- **Cyan line** = Aim direction
- **Yellow sphere** = Aim target
- **Red sphere** = Dead zone
- **Green sphere** = Max offset range
- **Blue line** = Look ahead direction

## ?? Goal Achievement

? **Camera shows where player is aiming**  
? **Works with mouse**  
? **Works with gamepad**  
? **Visual indicator at aim point**  
? **Smooth, professional feel**  
? **Zero code integration**  

**You're all set!** ???

For detailed info, see `AIM_CAMERA_SETUP.md`
