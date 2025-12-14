# ? Character X-Ray System - Quick Setup Checklist

## Problem Fixed

? **Old:** Walls were becoming transparent (wrong!)  
? **New:** Player glows through walls (correct!)

## 30-Second Setup

### Step 1: Disable Old System
```
[ ] Select Main Camera
[ ] Find "See Through System" component
[ ] Uncheck it (or remove it)
```

### Step 2: Create Material
```
[ ] Right-click in Project
[ ] Create ? Material
[ ] Name: "CharacterXRay"
[ ] Shader: Custom ? CharacterXRay
[ ] Set X-Ray Color: Light blue (0.2, 0.8, 1.0, 0.8)
```

### Step 3: Add X-Ray System
```
[ ] Select Main Camera
[ ] Add Component ? Character X Ray System
[ ] Player: Auto-found (or drag player)
[ ] X Ray Material: Drag "CharacterXRay" material
[ ] Include Enemies: ? (check this)
```

### Step 4: Configure Layers
```
[ ] Obstruction Layers:
    ? Environment (walls)
    ? Obstacles (props)
    ? Player (uncheck!)
    ? Enemy (uncheck!)
    ? Ground (uncheck!)
```

### Step 5: Test
```
[ ] Play game
[ ] Put wall between camera and player
[ ] Player should GLOW blue through wall
[ ] Walls stay SOLID (not transparent)
[ ] Ground stays SOLID (not transparent)
```

## Expected Result

### ? Correct Behavior
```
Player Visible:     Normal colors
Player Behind Wall: GLOWING BLUE SILHOUETTE
Walls:              Always solid
Ground:             Always solid
```

### ? Wrong (If Using Old System)
```
Player Visible:     Normal colors
Player Behind Wall: Walls become transparent
Walls:              Transparent/faded
Ground:             Transparent/faded  ? Wrong!
```

## Quick Fixes

### Not seeing glowing player?
```
? Check CharacterXRay material is assigned
? Reimport CharacterXRay.shader
? Check Console for shader errors
```

### Still seeing transparent walls?
```
? Old SeeThroughSystem still active
? Disable it completely
? Remove component if needed
```

### Player always glowing?
```
? Obstruction Layers include Player layer
? Uncheck Player from Obstruction Layers
? Uncheck Ground from Obstruction Layers
```

## Visual Test

**Correct Setup:**
```
    ?? Camera
     |
  [WALL] ? Solid wall
     |
    ? ? Glowing blue player visible!
```

**Wrong Setup (Old System):**
```
    ?? Camera
     |
  [????] ? Transparent wall
     |
    ? ? Player
    
Ground also transparent ?
```

## Settings Summary

```
Character X Ray System:
?? Player: Player GameObject
?? Main Camera: Main Camera
?? Include Enemies: ?
?? Enemy Tag: "Enemy"
?? X Ray Material: CharacterXRay
?? X Ray Color: (0.2, 0.8, 1.0, 0.8)
?? X Ray Strength: 0.8
?? Rim Power: 3.0
?? Obstruction Layers: Environment + Obstacles
?? Check Interval: 0.1s
?? Use Sphere Cast: ?
?? Sphere Radius: 0.3
```

## Echolocation Note

**X-Ray and Echolocation are SEPARATE:**
- X-Ray: Shows characters through walls (permanent)
- Echolocation: Reveals areas (temporary)
- Both work together perfectly!

## Files Needed

? CharacterXRay.shader (created)  
? CharacterXRaySystem.cs (created)  
? CharacterXRay material (you create this)  

## Verification Steps

1. [ ] Old SeeThroughSystem disabled
2. [ ] CharacterXRay material created with shader
3. [ ] CharacterXRaySystem added to camera
4. [ ] Material assigned
5. [ ] Layers configured correctly
6. [ ] Tested with wall blocking player
7. [ ] Player glows through wall ?
8. [ ] Walls stay solid ?
9. [ ] Ground stays solid ?

## Success!

If you see a **glowing blue silhouette** of your player through walls, **you're done!** ??

The system is working correctly and you have proper X-ray vision.

## Documentation

- `XRAY_SETUP_GUIDE.md` - Complete setup instructions
- `XRAY_VS_SEETHROUGH.md` - Comparison of systems
- `XRAY_QUICK_SETUP.md` - This checklist

**Your player is now visible through walls!** ???
