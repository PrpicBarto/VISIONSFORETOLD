# ? Complete Vision Systems - Setup Checklist

## Quick Summary

**Two systems working together:**
1. **X-Ray:** Shows characters through walls (permanent)
2. **Echolocation:** Reveals areas when pulsed (temporary)

## ?? Setup Steps

### Part 1: Character X-Ray (5 min)

```
[ ] Create material "CharacterXRay"
    - Shader: Custom ? CharacterXRay
    - X-Ray Color: (0.2, 0.8, 1.0, 0.8)
    - X-Ray Strength: 0.8
    - Rim Power: 3.0

[ ] Add to camera: Character X Ray System
    - Player: Drag player GameObject
    - X Ray Material: CharacterXRay
    - Include Enemies: ?
    - Enemy Tag: "Enemy"
    
[ ] Configure layers
    - Obstruction Layers:
      ? Environment
      ? Obstacles
      ? Player (uncheck)
      ? Enemy (uncheck)
      ? Ground (uncheck)

[ ] Test
    - Put wall between camera and player
    - Player should glow through wall
    - Blue silhouette visible
```

### Part 2: Echolocation Reveal (5 min)

```
[ ] Create material "EcholocationReveal"
    - Shader: Custom ? EcholocationReveal
    - Reveal Color: (0.3, 0.8, 1.0, 1.0)
    - Reveal Strength: 1.5
    
[ ] Configure EchoRevealSystem
    - Find on player GameObject
    - Apply Reveal Material: ?
    - Reveal Material: EcholocationReveal
    - Reveal Color: (0.3, 0.8, 1.0)
    - Reveal Brightness: 1.5
    - Reveal Duration: 5
    
[ ] Test
    - Trigger echolocation pulse
    - Objects should glow when hit
    - Glow lasts 5 seconds
    - Objects return to normal
```

### Part 3: Integration Test

```
[ ] X-Ray active
    - Player glows through walls ?
    
[ ] Echolocation active
    - Pulse reveals objects ?
    
[ ] Both working together
    - Player visible (x-ray) ?
    - Objects revealed (pulse) ?
    - No conflicts ?
```

## ?? Expected Results

### X-Ray System
```
? Player glows blue when behind walls
? Enemies glow blue when behind objects
? Pulsing rim light effect
? Always active (not temporary)
? Walls stay solid (not transparent)
```

### Echolocation System
```
? Objects glow when pulse hits them
? Glow lasts 5 seconds
? Objects fade back to normal
? Temporary effect (not permanent)
? Works on environment and enemies
```

### Together
```
? Player always visible (x-ray)
? Areas revealed when pulsed (echo)
? No visual conflicts
? Smooth transitions
? Performance is good
```

## ?? Color Settings

### Unified Theme (Recommended)
```
X-Ray Color: (0.2, 0.8, 1.0) Cyan
Reveal Color: (0.3, 0.8, 1.0) Cyan
Result: Consistent blue glow theme
```

### Distinct Theme
```
X-Ray Color: (0.2, 0.8, 1.0) Cyan (characters)
Reveal Color: (0.3, 1.0, 0.3) Green (environment)
Result: Clear visual distinction
```

## ?? Quick Fixes

### X-Ray Not Working
```
? Check CharacterXRay material assigned
? Reimport CharacterXRay.shader
? Exclude Player from Obstruction Layers
? Check player has renderers
```

### Echolocation Not Revealing
```
? Check EcholocationReveal material assigned
? Reimport EcholocationReveal.shader
? Enable "Apply Reveal Material"
? Check Detection Layers
? Verify pulse is active
```

### Both Systems Not Working
```
? Check Console for shader errors
? Reimport both shaders
? Verify both materials created
? Check both systems enabled
```

## ?? Layer Configuration

### X-Ray Obstruction Layers
```
What blocks line of sight:
? Environment (walls/buildings)
? Obstacles (rocks/props)
? Player (exclude)
? Enemy (exclude)
? Ground (exclude)
```

### Echolocation Detection Layers
```
What pulse can reveal:
? Environment
? Obstacles
? Enemy
? Player (exclude)
? Ground (optional)
```

## ? Performance Check

### Good Performance
```
FPS: Stable
X-Ray check: 0.1s interval
Echo raycasts: 64 per frame
Both systems: < 0.2ms overhead
```

### Optimize If Needed
```
X-Ray:
- Increase Check Interval: 0.15s
- Disable Include Enemies if not needed

Echolocation:
- Reduce Raycasts Per Frame: 32
- Reduce Max Revealed Objects: 30
- Decrease Reveal Duration: 3s
```

## ?? Verification Checklist

### X-Ray System
- [ ] Material created with correct shader
- [ ] System added to camera
- [ ] Player assigned
- [ ] Material assigned
- [ ] Layers configured
- [ ] Player glows when behind wall
- [ ] Enemies glow when enabled
- [ ] No pink materials
- [ ] Performance is good

### Echolocation System
- [ ] Material created with correct shader
- [ ] System has material assigned
- [ ] Apply Reveal Material enabled
- [ ] Colors configured
- [ ] Pulse triggers reveals
- [ ] Objects glow when hit
- [ ] Glow fades after 5s
- [ ] No errors in console

### Integration
- [ ] Both systems work independently
- [ ] Both work simultaneously
- [ ] No visual conflicts
- [ ] Colors match theme
- [ ] Performance is acceptable
- [ ] No material leaks

## ?? Files You Need

### Shaders
```
? CharacterXRay.shader (character x-ray)
? EcholocationReveal.shader (object reveal)
? SeeThrough.shader (old, can disable)
```

### Scripts
```
? CharacterXRaySystem.cs (x-ray system)
? EchoRevealSystem.cs (reveal system - updated)
? SeeThroughSystem.cs (old, can disable)
```

### Materials (You Create)
```
? CharacterXRay material
? EcholocationReveal material
```

### Documentation
```
? XRAY_SETUP_GUIDE.md (x-ray details)
? XRAY_ECHO_INTEGRATION.md (integration guide)
? XRAY_COMPLETE_CHECKLIST.md (this file)
```

## ?? Success Criteria

**You're done when:**

? Player glows through walls (x-ray)  
? Objects glow when pulse hits them (echo)  
? Both systems work together  
? No pink materials  
? No console errors  
? Performance is good  
? Colors match your game theme  

## ?? Next Steps

After setup:

1. **Test thoroughly**
   - Various walls/objects
   - Different distances
   - Multiple enemies
   
2. **Adjust to taste**
   - Colors
   - Glow intensities
   - Durations
   
3. **Optimize**
   - Check performance
   - Adjust intervals
   - Fine-tune settings
   
4. **Integrate gameplay**
   - Connect to abilities
   - Add UI feedback
   - Balance mechanics

**Your vision systems are complete!** ??????
