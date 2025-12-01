# Vertical Fog Coverage - Visual Guide

## The Problem

### Single Horizontal Plane (Old System)
```
Side View:

   Enemy (Y=10m)  ??  <-- VISIBLE! (above fog)
        |
        |
   ?????????????????? Fog Plane (Y=0.1m) - thin slice
   ????????????????  Ground (Y=0m)
```

**Issue:** The fog is only a thin horizontal slice at ground level. Anything above it (enemies, walls) remains visible!

---

## The Solution

### Multiple Stacked Planes (New System)
```
Side View:

   Enemy (Y=10m)  ??
        |               Plane 5  ??????????  Y=20m
        |???????????    Plane 4  ??????????  Y=15m
        |???????????    Plane 3  ??????????  Y=10m  <-- Enemy covered!
        |???????????    Plane 2  ??????????  Y=5m
        |???????????    Plane 1  ??????????  Y=0.1m
   ????????????????    Ground   ??????????  Y=0m

   ??? = Fog Volume (covers 0-20m vertically)
```

**Solution:** Multiple horizontal planes stacked at different heights create a **vertical fog volume** that covers tall objects!

---

## Configuration Examples

### Example 1: Low Enemies (2-3m tall)
```csharp
useMultiplePlanes = true;
verticalPlaneCount = 3;
verticalCoverageHeight = 5f;
```

**Result:** 3 planes covering 0-5m
```
  Plane 3  ??????  Y=5m
  Plane 2  ??????  Y=2.5m
  Plane 1  ??????  Y=0m
```

---

### Example 2: Medium Enemies/Walls (5-10m tall)
```csharp
useMultiplePlanes = true;
verticalPlaneCount = 5;
verticalCoverageHeight = 10f;
```

**Result:** 5 planes covering 0-10m
```
  Plane 5  ??????  Y=10m
  Plane 4  ??????  Y=7.5m
  Plane 3  ??????  Y=5m
  Plane 2  ??????  Y=2.5m
  Plane 1  ??????  Y=0m
```

---

### Example 3: Tall Buildings (10-20m)
```csharp
useMultiplePlanes = true;
verticalPlaneCount = 10;
verticalCoverageHeight = 20f;
```

**Result:** 10 planes covering 0-20m
```
  Plane 10 ??????  Y=20m
  Plane 9  ??????  Y=18m
  ...
  Plane 1  ??????  Y=0m
```

---

### Example 4: Very Tall Structures (20-50m)
```csharp
useMultiplePlanes = true;
verticalPlaneCount = 15;
verticalCoverageHeight = 50f;
```

**Result:** 15 planes covering 0-50m
```
  Plane 15 ??????  Y=50m
  Plane 14 ??????  Y=46.7m
  ...
  Plane 1  ??????  Y=0m
```

---

## Top-Down View (How It Looks From Above)

### Single Plane Mode
```
     North
       ?
       
  ??????????????
  ?    ???     ?  Enemy visible
  ?   ? P ?    ?  P = Player
  ?    ???     ?  Fog only covers ground
  ??????????????
  
       ?
     South
```

### Multiple Plane Mode (Stacked)
```
     North
       ?
       
  ??????????????  Each plane covers horizontally
  ??????????????  Stacked vertically = volume
  ????? P ??????  P = Player (revealed)
  ??????????????  Enemy hidden by fog volume
  ??????????????
  
  ? = Fog (multiple planes stacked)
  ? = Revealed area (pulse)
  
       ?
     South
```

---

## Performance Impact

| Plane Count | Performance | Best For |
|-------------|-------------|----------|
| 1 (single) | Fastest | Flat ground only |
| 3 planes | Very Fast | Low enemies (2-5m) |
| 5 planes | Fast | Medium enemies (5-10m) |
| 10 planes | Good | Tall buildings (10-20m) |
| 15+ planes | Moderate | Very tall (20m+) |

**Note:** Each plane is a simple quad with a shared material, so performance impact is minimal on modern hardware.

---

## Quick Setup Steps

1. **Select EcholocationManager** in Hierarchy

2. **Find "Vertical Fog Coverage" section** in Inspector

3. **Enable Multiple Planes:**
   ```
   ? Use Multiple Planes
   ```

4. **Set Plane Count** (based on your tallest object):
   ```
   Vertical Plane Count: 5  (for 10m tall enemies)
   ```

5. **Set Coverage Height** (slightly taller than your tallest object):
   ```
   Vertical Coverage Height: 12  (for 10m enemies + 2m buffer)
   ```

6. **Press Play** - fog now covers vertical space!

---

## Visual Debugging

### In Play Mode:
1. Select **EcholocationFogPlane_0** to **EcholocationFogPlane_4** in Hierarchy
2. You'll see 5 planes stacked at different heights
3. Each plane follows the player horizontally
4. Together they form a vertical fog volume

### Scene View:
- Enable Gizmos
- Select EcholocationManager
- You'll see wireframes showing each plane's position

---

## Troubleshooting

### "Enemies still visible at certain heights"
**Solution:** Increase `Vertical Plane Count`:
```
verticalPlaneCount = 10;  // More planes = better coverage
```

### "Performance drops with multiple planes"
**Solution:** Reduce plane count:
```
verticalPlaneCount = 3;  // Fewer planes = faster
```

Or disable and use single plane with higher vertical influence:
```
useMultiplePlanes = false;
verticalInfluence = 0.8f;  // Rely on shader instead
```

### "Gaps between planes visible"
**Solution:** Increase plane count or coverage height:
```
verticalPlaneCount = 7;         // More overlap
verticalCoverageHeight = 25f;   // Taller coverage
```

---

## Summary

? **Single Plane:** Fast, but only covers ground level
? **Multiple Planes:** Covers vertical space, works with tall enemies/walls
? **Configurable:** Adjust plane count and height to match your game
? **Efficient:** Minimal performance impact with shared material

**Recommended Default:**
```
useMultiplePlanes = true;
verticalPlaneCount = 5;
verticalCoverageHeight = 15f;
```
