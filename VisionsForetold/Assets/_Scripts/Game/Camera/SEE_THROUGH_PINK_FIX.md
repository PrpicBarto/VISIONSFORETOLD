# ?? See-Through System - Pink Shader Fix

## Problem

Objects and ground appear **pink** when see-through system is active.

**Cause:** Shader was using old Unity syntax. Pink = shader compilation error in Unity 6.

## ? Solution Applied

Updated shader to be **fully compatible with Unity 6** and both URP and Built-in pipelines.

## What Was Fixed

### 1. Shader Updated for Unity 6

**Before:** Used old CG/HLSL syntax
**After:** Uses modern HLSL with URP support

**Changes:**
- Added URP (Universal Render Pipeline) SubShader
- Updated to use `HLSL` instead of `CG`
- Added fallback for Built-in Render Pipeline
- Fixed includes for Unity 6 packages

### 2. Ground Exclusion

**Problem:** Ground layer was being made transparent

**Solution:** 
- Updated obstruction layers default
- Added clear tooltip to exclude Ground/Terrain
- Inspector shows proper layer selection

## ?? How to Fix (If Still Pink)

### Step 1: Check Your Render Pipeline

**Are you using URP?**
```
Edit ? Project Settings ? Graphics
Check "Scriptable Render Pipeline Settings"
```

**If URP:** Shader should work automatically
**If Built-in:** Uses fallback shader (also works)

### Step 2: Exclude Ground Layer

```
1. Select Camera ? SeeThroughSystem component
2. Find "Obstruction Layers"
3. UNCHECK these layers:
   ? Ground
   ? Terrain  
   ? Floor
4. KEEP these checked:
   ? Default
   ? Environment
   ? Obstacles
```

### Step 3: Reimport Shader

If still pink:
```
1. Find SeeThrough.shader in Project
2. Right-click ? Reimport
3. Check Console for errors
```

### Step 4: Check Material

```
1. Select your SeeThroughMaterial
2. Shader should show: Custom/SeeThrough
3. No error icon (!)
4. Properties should be visible
```

## ?? Layer Setup (Recommended)

### Create Layers

```
Edit ? Project Settings ? Tags and Layers

Add these layers:
?? Ground (for ground/floors)
?? Environment (for walls)
?? Obstacles (for objects)
?? Terrain (for terrain)
```

### Assign Objects

**Ground/Floors:**
```
Select ground objects
Inspector ? Layer ? Ground
```

**Walls/Buildings:**
```
Select wall objects
Inspector ? Layer ? Environment
```

### Configure See-Through System

```
Obstruction Layers:
? Environment (walls should become transparent)
? Obstacles (objects should become transparent)
? Ground (ground should NOT become transparent)
? Terrain (terrain should NOT become transparent)
? Player (never transparent)
? Enemy (handled separately)
```

## ?? Verification

### Test 1: Shader Compiles

```
1. Select SeeThroughMaterial
2. No pink/magenta color in preview
3. No errors in Console
4. Shader properties visible
```

### Test 2: Ground Not Affected

```
1. Play game
2. Look at ground
3. Ground should be NORMAL color
4. NOT transparent or pink
```

### Test 3: Walls Become Transparent

```
1. Put wall between camera and player
2. Wall should become transparent
3. Player visible through wall
4. Ground stays normal
```

## ?? Still Pink? Advanced Fixes

### Fix 1: Check URP Packages

If using URP:
```
Window ? Package Manager
?? Universal RP (should be installed)
?? Shader Graph (should be installed)
?? Core RP Library (should be installed)
```

### Fix 2: Shader Include Paths

If Console shows include errors:
```
Edit shader line:
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

Should match your Unity version
```

### Fix 3: Fallback Shader

If nothing works, use simpler shader:
```csharp
In SeeThroughSystem.cs CreateSeeThroughMaterial():
// Use Standard Transparent shader as fallback
seeThroughMaterial = new Material(Shader.Find("Standard"));
seeThroughMaterial.SetFloat("_Mode", 3); // Transparent
// ... rest of transparency setup
```

### Fix 4: Force Built-in Pipeline Shader

If URP causing issues:
```
Edit SeeThrough.shader:
1. Remove first SubShader (URP)
2. Keep only second SubShader (Built-in)
3. Save and reimport
```

## ?? Render Pipeline Compatibility

### Universal Render Pipeline (URP)
? **Primary Support** - First SubShader
- Uses modern HLSL
- Full lighting support
- Optimized for Unity 6

### Built-in Render Pipeline
? **Fallback Support** - Second SubShader
- Uses CG/HLSL
- Compatible with older projects
- Automatic fallback

### HDRP (High Definition)
?? **Not tested** - May need custom shader
- Contact if needed
- Can create HDRP version

## ?? Common Issues

### Issue 1: Everything Pink

**Cause:** Shader compilation error

**Fix:**
1. Check Console for errors
2. Reimport shader
3. Verify URP packages installed (if using URP)

### Issue 2: Ground Transparent

**Cause:** Ground layer in obstruction layers

**Fix:**
1. SeeThroughSystem ? Obstruction Layers
2. Uncheck Ground/Terrain layers
3. Only check walls/obstacles

### Issue 3: Works in Editor, Pink in Build

**Cause:** Shader not included in build

**Fix:**
```
Edit ? Project Settings ? Graphics
"Always Included Shaders" ? Add SeeThrough shader
```

### Issue 4: Pink Only on Some Objects

**Cause:** Different render pipeline materials

**Fix:**
- Check if objects use URP materials
- Ensure consistent pipeline across project

## ? Quick Fix Checklist

- [ ] Shader reimported (right-click ? Reimport)
- [ ] No errors in Console
- [ ] Material shows Custom/SeeThrough shader
- [ ] Ground layer excluded from Obstruction Layers
- [ ] Test in Play mode
- [ ] Ground is normal color
- [ ] Walls become transparent when blocking view
- [ ] Player/enemies always visible

## ?? Expected Behavior

### Normal Objects
```
Ground ? Normal color (gray/brown/texture)
Walls ? Normal color when not obstructing
Player ? Normal color
```

### When Obstructing
```
Wall between camera and player:
Wall ? Transparent (light blue/your color)
Ground ? Still normal (NOT transparent)
Player ? Visible through wall
```

## ?? Files Updated

1. **`SeeThrough.shader`** - Complete rewrite
   - URP support added
   - Built-in pipeline fallback
   - Unity 6 compatible
   - Fixed pink shader issue

2. **`SeeThroughSystem.cs`** - Minor update
   - Obstruction layers tooltip updated
   - Better default value

3. **`SEE_THROUGH_PINK_FIX.md`** - This guide
   - Fix instructions
   - Troubleshooting
   - Layer setup

## ?? Result

After applying fixes:

- ? No more pink objects
- ? Shader compiles correctly
- ? Ground stays normal
- ? Walls become transparent as intended
- ? Works in Unity 6
- ? Compatible with URP and Built-in

**Your see-through system should now work perfectly!** ????

## Need More Help?

If still having issues:

1. Check Console for specific shader errors
2. Verify Unity version (6000.2.7f2 tested)
3. Confirm render pipeline (URP or Built-in)
4. Check all objects are on correct layers

The shader is now fully Unity 6 compatible and should work without pink colors!
