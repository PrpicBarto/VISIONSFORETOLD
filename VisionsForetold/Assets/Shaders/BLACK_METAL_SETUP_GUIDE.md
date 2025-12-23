# ?? Black Metal Visual Style - Setup Guide

## ? **System Added Successfully!**

Your game will now render in a black metal album aesthetic (Darkthrone - Transylvanian Hunger style)!

---

## ?? **What Was Created**

### 1. **BlackMetalOutline.shader**
- Location: `Assets/Shaders/BlackMetalOutline.shader`
- Creates black outlines around objects
- High contrast lighting
- Makes objects recognizable

### 2. **BlackMetalPostProcess.shader & Script**
- Shader: `Assets/Shaders/BlackMetalPostProcess.shader`
- Script: `Assets/_Scripts/Game/Screen/BlackMetalPostProcess.cs`
- Full screen effect
- Film grain, desaturation, harsh contrast
- Vignette (dark edges)
- Optional scan lines

---

## ?? **Setup Instructions**

### Method 1: Automatic Application (Recommended) ?

**Easiest way - applies to all objects automatically!**

```
1. Create an empty GameObject in your scene:
   - Hierarchy ? Right-click ? Create Empty
   - Name it "BlackMetalManager"

2. Add the auto-apply script:
   - Select BlackMetalManager
   - Add Component ? ApplyBlackMetalToAll

3. Configure (optional):
   - Auto Create Material: ? (checked)
   - Apply On Start: ? (checked)
   - Override Existing: ? (checked)
   - Main Color: (0.8, 0.8, 0.8)
   - Outline Color: (0, 0, 0)
   - Outline Width: 0.005
   - Brightness: 0.8

4. Apply:
   Option A - Automatic:
   - Enter Play Mode
   - Shader applies to all objects automatically!

   Option B - Manual:
   - Right-click on the script in Inspector
   - Click "Apply Black Metal Shader to All Objects"
   - Shader applies immediately!
```

**Exclude Specific Objects:**
```
Exclude Tags: Add tags like "UI", "MainCamera"
Exclude Names: Add name patterns like "Camera", "Light"

Example:
- Exclude Tags: ["UI", "MainCamera", "Ignore"]
- Exclude Names: ["Camera", "Light", "Skybox"]
```

---

### Method 2: Manual Application (Traditional)

**For more control over individual objects:**

### Step 1: Apply Outline Shader to Objects

**Create Material for Objects:**
```
1. Right-click in Project ? Create ? Material
2. Name it "BlackMetal_ObjectMaterial"
3. Select the material
4. Inspector ? Shader dropdown ? Custom ? BlackMetalOutline
5. Configure:
   - Main Color: Light gray/white (0.8, 0.8, 0.8)
   - Outline Color: Black (0, 0, 0)
   - Outline Width: 0.005 - 0.01
   - Brightness: 0.6 - 0.9
```

**Apply to Objects:**
```
Method 1 - Individual Objects:
1. Select object in Hierarchy
2. Inspector ? MeshRenderer ? Materials
3. Drag "BlackMetal_ObjectMaterial" to material slot

Method 2 - Multiple Objects:
1. Select multiple objects
2. Drag material to any selected object
3. All selected objects get the material
```

**Which Objects to Apply:**
- Player model
- Enemy models
- Environment objects (trees, rocks, buildings)
- Props and interactive objects
- Anything that needs to be visible

---

### Step 2: Add Post-Processing to Camera

**On Your Main Camera:**
```
1. Select Main Camera in Hierarchy
2. Add Component ? BlackMetalPostProcess
3. Inspector ? Configure settings:

Recommended Settings (Transylvanian Hunger style):
?? Effect Intensity: 0.8
?? Contrast: 1.8 - 2.2
?? Brightness: -0.2 to -0.3
?? Desaturation: 0.85 - 0.95
?? Enable Grain: ?
?? Grain Intensity: 0.15 - 0.2
?? Grain Size: 1.5
?? Enable Vignette: ?
?? Vignette Intensity: 0.4 - 0.5
?? Enable Scan Lines: ? (optional, try 0.1 if enabled)
?? Scan Line Density: 400
```

---

## ?? **Style Presets**

### Preset 1: Classic Transylvanian Hunger
```
Contrast: 2.0
Brightness: -0.25
Desaturation: 0.9
Grain Intensity: 0.18
Vignette: 0.45
Scan Lines: Disabled
```

### Preset 2: Ultra Grim (Extreme)
```
Contrast: 2.5
Brightness: -0.4
Desaturation: 0.98
Grain Intensity: 0.25
Vignette: 0.6
Scan Lines: Enabled (density 500)
```

### Preset 3: Readable (Softer)
```
Contrast: 1.5
Brightness: -0.1
Desaturation: 0.75
Grain Intensity: 0.12
Vignette: 0.3
Scan Lines: Disabled
```

---

## ?? **Configuration Options**

### Outline Settings (Material)
```
Outline Color: 
- Black (0,0,0) - Classic
- Dark gray (0.1,0.1,0.1) - Softer
- White (1,1,1) - Inverted style

Outline Width:
- 0.003 - Thin (subtle)
- 0.005 - Medium (recommended)
- 0.01 - Thick (very visible)

Brightness:
- 0.5 - Dark, grim
- 0.8 - Balanced
- 1.2 - Brighter, more visible
```

### Post-Process Settings (Camera)
```
Effect Intensity:
- 0.0 - Disabled (normal rendering)
- 0.5 - Subtle black metal look
- 0.8 - Strong effect (recommended)
- 1.0 - Maximum grimness

Contrast:
- 1.0 - Normal
- 1.8 - Strong (recommended)
- 2.5 - Extreme harsh

Desaturation:
- 0.0 - Full color
- 0.85 - Mostly black/white (recommended)
- 1.0 - Pure grayscale

Grain:
- Creates film/tape texture
- 0.15 recommended
- 0.25 for extreme grain

Vignette:
- Darkens edges
- 0.4 recommended
- Creates focus on center
```

---

## ?? **Testing**

### Test Checklist:
```
? Objects have black outlines
? Scene is mostly black/white/gray
? High contrast (harsh shadows)
? Film grain visible
? Dark edges (vignette)
? Objects are recognizable
? Performance is acceptable
? Effect looks grim and atmospheric
```

### Performance:
```
Post-processing: ~2-3ms per frame (negligible)
Outline shader: Similar to standard shader
Overall: Should run smoothly on most hardware
```

---

## ?? **Troubleshooting**

### Issue 0: Using Automatic Application (New!)
```
If using ApplyBlackMetalToAll script:

Check Console:
? "Applied to X objects, skipped Y objects"
? If 0 objects applied, check filters

Common Issues:
- Objects have excluded tags
- Objects have excluded names (Camera, Light)
- Override Existing is unchecked
- Shader not found

Fix:
1. Check Exclude Tags list
2. Check Exclude Names list
3. Enable Override Existing
4. Verify shader exists: Custom/BlackMetalOutline
5. Right-click script ? "Apply Black Metal Shader to All Objects"
```

### Issue 1: Outlines Not Appearing
```
Check:
? Material uses BlackMetalOutline shader
? Outline Width > 0.003
? Objects have MeshRenderer
? Camera can see the objects

Fix:
- Increase Outline Width to 0.01
- Check material is assigned
- Ensure objects aren't too far from camera
```

### Issue 2: Post-Process Not Working
```
Check:
? BlackMetalPostProcess script on camera
? Effect Intensity > 0
? Camera is Main Camera tag
? Shaders are in correct folders

Fix:
- Verify shader name matches in script
- Check Console for errors
- Increase Effect Intensity
```

### Issue 3: Too Dark / Can't See Anything
```
Adjust:
- Brightness: Increase to -0.1 or 0.0
- Contrast: Decrease to 1.5
- Add more lights to scene
- Increase object material Brightness
```

### Issue 4: Not Dark Enough / Too Much Color
```
Adjust:
- Desaturation: Increase to 0.95
- Contrast: Increase to 2.2
- Brightness: Decrease to -0.3
- Vignette Intensity: Increase to 0.6
```

### Issue 5: Grain Too Strong
```
Adjust:
- Grain Intensity: Decrease to 0.1
- Grain Size: Decrease to 1.0
- Or disable grain completely
```

---

## ?? **Advanced Customization**

### Per-Object Outline Colors
```
Create multiple materials:
- BlackMetal_Player (white outline)
- BlackMetal_Enemy (red outline)
- BlackMetal_Environment (black outline)

Different outline colors help identify object types!
```

### Dynamic Effect Intensity
```csharp
// Add to your game manager
BlackMetalPostProcess postProcess;

void Start()
{
    postProcess = Camera.main.GetComponent<BlackMetalPostProcess>();
}

void AdjustIntensity(float newIntensity)
{
    if (postProcess != null)
    {
        postProcess.effectIntensity = newIntensity;
    }
}

// Use for:
// - Cutscenes (reduce intensity)
// - Boss fights (increase intensity)
// - UI menus (disable effect)
```

### Lighting Tips
```
For best black metal look:
- Use directional light only
- Low ambient light
- Strong shadows
- No point lights (or very few)
- Cold color temperature (blue-ish)
```

---

## ?? **The Black Metal Aesthetic Explained**

### What Makes Transylvanian Hunger Look:
```
? High contrast black/white
? Minimal gray tones
? Harsh shadows
? Film grain/noise
? Dark vignette
? Low production quality feel
? Atmospheric and grim
? Objects still recognizable via outlines
```

### How We Achieve It:
```
1. Outline Shader:
   - Black outlines = object recognition
   - Harsh lighting = high contrast
   - Low brightness = dark atmosphere

2. Post-Processing:
   - Desaturation = black/white look
   - Contrast boost = harsh tones
   - Grain = film/tape texture
   - Vignette = focus and darkness
```

---

## ?? **Quick Setup (2 Minutes)** ?

**Fastest way to get the full black metal look:**

```
Method A - Fully Automatic (Recommended):

1. Create GameObject:
   - Hierarchy ? Create Empty
   - Name: "BlackMetalManager"

2. Add Scripts:
   - Add Component ? ApplyBlackMetalToAll
   - Add Component ? BlackMetalPostProcess (to Main Camera instead)

3. Configure ApplyBlackMetalToAll:
   - Auto Create Material: ?
   - Apply On Start: ?
   - All other settings: Use defaults

4. Configure BlackMetalPostProcess (on camera):
   - Effect Intensity: 0.8
   - Use "Classic Transylvanian Hunger" preset

5. Enter Play Mode:
   - Everything applies automatically!
   - All objects get outlines
   - Full black metal effect active

Done! ?

---

Method B - Manual (Traditional):

1. Create Material:
   - Right-click ? Material
   - Shader: Custom/BlackMetalOutline
   - Settings: Defaults are good

2. Apply to All Objects:
   - Select all visible objects
   - Drag material to them

3. Add to Camera:
   - Main Camera ? Add Component
   - BlackMetalPostProcess
   - Use "Classic Transylvanian Hunger" preset

4. Test:
   - Play Mode
   - Adjust settings to taste
```

---

## ? **Summary**

**Created:**
- BlackMetalOutline.shader (object outlines)
- BlackMetalPostProcess.shader (screen effect)
- BlackMetalPostProcess.cs (effect script)
- This setup guide

**Features:**
- Black outlines on all objects
- High contrast black/white rendering
- Film grain texture
- Dark vignette
- Customizable intensity
- Performance friendly

**Result:**
- Game looks like a black metal album cover
- Darkthrone - Transylvanian Hunger aesthetic
- All objects clearly recognizable
- Atmospheric and grim

---

**Your game will now look like a black metal album!** ????

**Apply the shader and test it!** ??

**Adjust settings to get your perfect grim atmosphere!** ??

---

## ?? **Next Steps**

1. **Create material** with BlackMetalOutline shader
2. **Apply to objects** in your scene
3. **Add BlackMetalPostProcess** to camera
4. **Adjust settings** using presets above
5. **Test and refine** until it looks grim enough
6. **Enjoy your black metal game!** ??

---

**Remember:** The goal is to be dark and atmospheric while keeping objects recognizable through outlines!
