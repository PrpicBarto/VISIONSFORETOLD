# Echolocation System - Setup Guide

## Problem: Fog Not Covering Scene
If your fog isn't hiding enemies and ground, it's likely a **sorting/layering issue** or **shader configuration problem**.

---

## REQUIRED SETUP

### 1. Create the EchoManager GameObject

1. **In Unity Hierarchy**: Right-click ? Create Empty ? Name it "EcholocationManager"
2. **Add Components** (in this order):
   ```
   - EchoManager.cs
   - EcholocationController.cs
   - EchoIntersectionDetector.cs (optional but recommended)
   ```

### 2. Create the Echolocation Material

1. **Project window**: Right-click ? Create ? Material
2. **Name it**: "EcholocationFog"
3. **Set Shader**: `Custom/URP/Echolocation`
4. **Configure Material**:
   - Fog Color: Pure Black (RGB: 0, 0, 0, Alpha: 1)
   - Fog Density: 1.0 (fully opaque)
   - Fog Min Density: 0.95
   - Fog Max Density: 1.0

### 3. Configure EchoManager Component

**Inspector Settings:**
```
?? EchoManager ?????????????????????
? Required Components:              ?
?   Echo Controller: [Auto-assigned]?
?   Intersection Detector: [Auto]   ?
?                                   ?
? Player Reference:                 ?
?   Player: [Auto-detected]         ?
?                                   ?
? Camera Settings:                  ?
?   Main Camera: [Auto-detected]    ?
?   Camera Far Clip Plane: 1000     ?
?                                   ?
? Scene Configuration:              ?
?   Ground Layer: Default           ?
?   Manual Ground Level: 0          ?
?   Use Manual Ground Level: ?      ?
?                                   ?
? Material Setup:                   ?
?   Echolocation Material: ????????>?
?   [Drag EcholocationFog here]    ?
?????????????????????????????????????
```

### 4. Configure EcholocationController

**Critical Settings for Fog Coverage:**
```
?? EcholocationController ??????????
? Setup:                            ?
?   Enable Echolocation: ?          ?
?   Fog Material: EcholocationFog   ?
?   Player: [Auto-detected]         ?
?                                   ?
? Fog Plane Configuration:          ?
?   Plane Size: (200, 1, 200)      ?
?   Plane Distance From Camera: 50  ?
?   ? Use Camera Billboard: ?       ?  <-- DISABLE FOR TOP-DOWN
?   Ground Level: 0                 ?
?   Auto Detect Ground Level: ?     ?
?                                   ?
? Fog & Reveal Settings:            ?
?   Fog Color: Black (0,0,0,1)     ?
?   Fog Density: 1.0               ?  <-- MAXIMUM
?   Fog Min Density: 0.95          ?
?   Fog Max Density: 1.0           ?
?????????????????????????????????????
```

---

## FIXING "FOG NOT COVERING SCENE"

### Issue #1: Fog Plane Not Rendering Over Scene

**Cause**: Render order / sorting issue

**Fix**:
1. In shader (Echolocation.shader), ensure:
   ```hlsl
   Tags 
   { 
       "RenderType" = "Transparent" 
       "Queue" = "Transparent+200"    // High priority
       "IgnoreProjector" = "True"
   }
   
   Pass
   {
       Blend SrcAlpha OneMinusSrcAlpha
       ZWrite Off
       ZTest LEqual                    // Always render
       Cull Off
   }
   ```

2. Check fog plane layer:
   - Should be on "Ignore Raycast" layer
   - Should NOT be on same layer as UI

### Issue #2: Fog Appears Transparent

**Cause**: Fog density too low or wrong blend mode

**Fix**:
1. **EcholocationController settings**:
   - Fog Density: 1.0 (fully opaque)
   - Fog Min Density: 0.95
   - Fog Max Density: 1.0
   - Fog Color Alpha: 1.0

2. **Material settings**:
   - Rendering Mode: Transparent
   - Check "Enable GPU Instancing": OFF

### Issue #3: Enemies/Ground Visible Through Fog

**Cause**: Fog plane too small or positioned wrong

**Fix**:
1. **Increase Plane Size**:
   ```
   Plane Size: (500, 1, 500)  // Much larger
   ```

2. **Use Ground-Aligned Mode**:
   ```
   Use Camera Billboard: ? (disabled)
   Auto Detect Ground Level: ? (enabled)
   ```

3. **Check Plane Position** (Debug UI shows):
   ```
   Plane Y: Should match Ground Level
   ```

### Issue #4: Stuttering/Glitching Fog

**Cause**: Camera reference lost or update issues

**Fix**: Already fixed in latest optimizations
- Camera is re-cached if null
- Shader updates every frame while pulsing

---

## RENDER SETTINGS (URP)

### URP Renderer Asset
**DO NOT** use EchoRenderFeature.cs (it's deprecated and caused black screens)

Instead, rely on **fog plane overlay** method (current system).

### Camera Settings
```
Camera Component:
  Clear Flags: Skybox (or Solid Color)
  Culling Mask: Everything
  Depth: -1
  Rendering Path: Use Pipeline Settings
  Far Clip Plane: 1000+
```

---

## LAYER SETUP

1. **Create Layers** (if needed):
   ```
   Layer 8: Ground
   Layer 9: Enemy
   Layer 10: Items
   ```

2. **Tag Objects**:
   ```
   Ground objects: Tag = "Ground"
   Enemy objects: Tag = "Enemy"
   Item objects: Tag = "Item"
   Player: Tag = "Player"
   ```

3. **EchoIntersectionDetector**:
   ```
   Detection Layers: Everything (or specific layers)
   Wall Tag: "Wall"
   Enemy Tag: "Enemy"
   Item Tag: "Item"
   ```

---

## TESTING CHECKLIST

- [ ] Fog plane exists in hierarchy (during Play mode)
- [ ] Fog plane name is "EcholocationFogPlane"
- [ ] Fog plane is active (enabled in hierarchy)
- [ ] Material is assigned to fog plane
- [ ] Material uses "Custom/URP/Echolocation" shader
- [ ] Fog Color is black (0, 0, 0, 1)
- [ ] Fog Density is 1.0
- [ ] Ground Level matches player Y position
- [ ] Plane Y position matches Ground Level
- [ ] FPS is stable (60+)
- [ ] Pulse expands smoothly
- [ ] Pulse reveals ground and enemies

---

## DEBUG COMMANDS

Enable debug UI in EcholocationController:
```
Show Debug: ?
```

**Watch for**:
- FPS drops (performance issue)
- Camera: NULL (camera reference lost)
- Plane Y mismatch (positioning issue)
- Pulsing stuck (animation issue)

---

## COMMON MISTAKES

? **Using Billboard Mode for top-down games**
? **Disable billboard, use ground-aligned mode**

? **Fog Material not assigned**
? **Drag material to both EchoManager AND EcholocationController**

? **Plane too small (objects outside coverage)**
? **Increase Plane Size to (500, 1, 500) or larger**

? **Fog density too low**
? **Set Fog Density to 1.0 and Min/Max to 0.95/1.0**

? **Wrong render queue**
? **Shader Queue must be "Transparent+200"**

---

## OPTIMAL SETTINGS FOR TOP-DOWN GAME

```csharp
// EcholocationController
useCameraBillboard = false;
autoDetectGroundLevel = true;
planeSize = new Vector3(500, 1, 500);

// Fog Transparency (NEW - more transparent fog)
fogDensity = 0.85f;           // Slightly transparent (was 1.0)
fogMinDensity = 0.7f;         // Less dense near player (was 0.95)
fogMaxDensity = 0.95f;        // Still opaque far away (was 1.0)
fogColor = Color.black;       // (0, 0, 0, 0.85) - with alpha

// Player Visibility (NEW - always visible)
permanentRevealRadius = 3f;   // 3 meter clear area around player

verticalInfluence = 0.3f;     // 30% vertical influence for height variation

// Vertical Fog Coverage (for tall enemies/walls)
useMultiplePlanes = true;
verticalPlaneCount = 5;
verticalCoverageHeight = 20f;

// EchoIntersectionDetector
detect3D = true;
raycastResolution = 64;
detectionThreshold = 2f;
enableHighlighting = true;
verticalRayCount = 5;
verticalRayHeight = 10f;
```

---

## FOG TRANSPARENCY TUNING

### Visual Comparison

**Before (Too Opaque):**
```
??????????  <- Pure black fog
????P?????  <- Can't see player!
??????????     Can't see enemies!
```

**After (Balanced):**
```
??????????  <- Semi-transparent fog
???  P  ??  <- Player visible!
??????????     Enemy shapes visible!
```

### Making Fog More/Less Transparent

**Location:** EcholocationController ? Fog & Reveal Settings

#### **More Transparent** (can see shapes through fog):
```
Fog Density: 0.7 - 0.85
Fog Min Density: 0.5 - 0.7
Fog Max Density: 0.8 - 0.95
Fog Color Alpha: 0.7 - 0.85
Permanent Reveal Radius: 3 - 5
```

#### **Less Transparent** (pure black fog):
```
Fog Density: 0.95 - 1.0
Fog Min Density: 0.9 - 0.95
Fog Max Density: 1.0
Fog Color Alpha: 1.0
Permanent Reveal Radius: 0 - 2
```

#### **Player Always Visible:**
```
Permanent Reveal Radius: 2 - 5 meters
```
- 2m = tight circle (can only see player)
- 3m = comfortable (player + immediate area) **? Recommended**
- 5m = generous (player + surroundings)

### Quick Presets

**Balanced Mode (Recommended):**
```csharp
fogDensity = 0.85f;
fogMinDensity = 0.7f;
fogMaxDensity = 0.95f;
permanentRevealRadius = 3f;
```

**Horror Mode (Very Dark):**
```csharp
fogDensity = 0.95f;
fogMinDensity = 0.9f;
fogMaxDensity = 1.0f;
permanentRevealRadius = 2f;
```

**Easy Mode (Light Fog):**
```csharp
fogDensity = 0.6f;
fogMinDensity = 0.4f;
fogMaxDensity = 0.8f;
permanentRevealRadius = 5f;
```

**?? Full Transparency Guide:** `Tools ? Echolocation ? Open Transparency Guide`

---

## VERTICAL COVERAGE FIX (Tall Enemies/Walls)

If enemies or walls at different heights are still visible through fog:

### Multiple Fog Planes (Recommended)
**Location:** EcholocationController ? Vertical Fog Coverage

**How it works:**
Instead of one thin horizontal plane, this creates **multiple stacked planes** that cover vertical space:

```
     Top Plane    ??????????  Y = 20m
                      ?
     Plane 3      ??????????  Y = 15m
                      ?
     Plane 2      ??????????  Y = 10m
                      ?
     Plane 1      ??????????  Y = 5m
                      ?
     Ground Plane ??????????  Y = 0m
     
     = Vertical fog volume covering 0-20m
```

**Settings:**
- `Use Multiple Planes: ?` (enable vertical coverage)
- `Vertical Plane Count: 5-10` (more planes = better coverage)
- `Vertical Coverage Height: 20` (match your tallest enemy/wall)

**Performance:**
- 5 planes: Good for most games
- 10 planes: Better coverage, slight performance cost
- 15+ planes: Only if you have very tall structures

### When to Use:
? Enemies taller than 2-3 meters
? Walls/buildings with vertical surfaces
? Multi-level environments
? Flying enemies

### When NOT to Use:
? Purely ground-level gameplay (keep useMultiplePlanes = false)
? Very low-end hardware (use single plane with higher vertical influence)

---

## HEIGHT VARIATION FIX

If objects at different heights aren't being covered/revealed properly:

### Vertical Influence Setting
**Location:** EcholocationController ? Vertical Distance Settings ? Vertical Influence

**Values:**
- `0.0` = Pure 2D (XZ only, ignores all height differences)
- `0.3` = Hybrid (recommended - height matters but less than horizontal distance)
- `0.5` = Balanced (height and horizontal distance equally important)
- `1.0` = Pure 3D (full spherical echolocation)

**Recommended:** `0.3` for most top-down games with height variation

### 3D Raycasting
**Location:** EchoIntersectionDetector ? Detection Settings ? Detect 3D

**Enable this if:**
- You have enemies/objects at different elevations
- You have multi-story buildings or platforms
- Objects are flying/jumping

**Settings:**
- Detect 3D: ? (enabled)
- Vertical Ray Count: 5-7
- Vertical Ray Height: Match your tallest object

**How it works:**
```
Without 3D Detection:          With 3D Detection:
     Enemy                          Enemy
       |                              |||
       | <-- Missed!                  ||| <-- Hit!
       |                              |||
  ----Pulse----                  ----Pulse----
  (only checks                   (checks multiple
   ground level)                  vertical levels)
```

---

## NEXT STEPS

1. Create EcholocationManager GameObject
2. Assign EchoManager script
3. Create and assign Echolocation material
4. Configure settings as above
5. Press Play
6. Check Debug UI (top-left)
7. Verify fog covers scene
8. Test pulse reveals enemies

If issues persist, check Console for errors and verify all components are present on the manager GameObject.
