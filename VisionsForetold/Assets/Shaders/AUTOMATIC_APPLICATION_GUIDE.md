# ? Black Metal Shader - AUTOMATIC APPLICATION

## ? **New Feature: Automatic Application!**

The Black Metal outline shader can now be applied to **ALL objects in your scene automatically**!

---

## ?? **What Was Added**

**New Script: `ApplyBlackMetalToAll.cs`**
- Location: `Assets/_Scripts/Game/Screen/ApplyBlackMetalToAll.cs`
- Automatically applies Black Metal shader to all renderers
- Configurable filters and settings
- One-click application

---

## ? **Super Quick Setup (1 Minute)**

```
1. Create empty GameObject in scene:
   Hierarchy ? Right-click ? Create Empty
   Name: "BlackMetalManager"

2. Add script:
   Inspector ? Add Component ? ApplyBlackMetalToAll

3. Use defaults (everything auto-configured):
   - Auto Create Material: ?
   - Apply On Start: ?
   - Override Existing: ?

4. Enter Play Mode:
   ALL objects get Black Metal shader automatically!
```

**That's it!** ??

---

## ?? **How It Works**

**On Start:**
1. Finds all MeshRenderer and SkinnedMeshRenderer in scene
2. Creates Black Metal material (if not assigned)
3. Applies material to all objects
4. Respects filters (excluded tags/names)
5. Shows summary in Console

**Manual Application:**
- Right-click script ? "Apply Black Metal Shader to All Objects"
- Works in Edit Mode or Play Mode

---

## ?? **Configuration**

### Inspector Settings:

```
???????????????????????????????????????????
? Shader Settings                         ?
???????????????????????????????????????????
? Black Metal Material: [Optional]        ?
? Auto Create Material: ?                 ?
? Main Color: (0.8, 0.8, 0.8)            ?
? Outline Color: (0, 0, 0)               ?
? Outline Width: 0.005                    ?
? Brightness: 0.8                          ?
???????????????????????????????????????????
? Application Settings                     ?
???????????????????????????????????????????
? Apply On Start: ?                       ?
? Include Inactive: ?                     ?
? Override Existing: ?                    ?
???????????????????????????????????????????
? Filter Settings                          ?
???????????????????????????????????????????
? Include Tags: []                         ?
? Exclude Tags: ["UI", "MainCamera"]     ?
? Exclude Names: ["Camera", "Light"]     ?
???????????????????????????????????????????
? Debug                                    ?
???????????????????????????????????????????
? Show Debug Logs: ?                      ?
???????????????????????????????????????????
```

---

## ?? **Usage Examples**

### Example 1: Apply to Everything
```
Settings:
- Include Tags: [] (empty)
- Exclude Tags: ["UI"]
- Exclude Names: ["Camera"]

Result: All objects except UI and cameras get shader
```

### Example 2: Only Enemies
```
Settings:
- Include Tags: ["Enemy"]
- Exclude Tags: []
- Exclude Names: []

Result: Only objects tagged "Enemy" get shader
```

### Example 3: Everything Except Player
```
Settings:
- Include Tags: [] (empty)
- Exclude Tags: ["Player", "UI"]
- Exclude Names: []

Result: All objects except Player and UI get shader
```

---

## ?? **Advanced Usage**

### Dynamic Application (Runtime)

```csharp
public class GameManager : MonoBehaviour
{
    private ApplyBlackMetalToAll blackMetalApplier;

    void Start()
    {
        blackMetalApplier = FindFirstObjectByType<ApplyBlackMetalToAll>();
    }

    // Apply shader at runtime
    void EnableBlackMetalMode()
    {
        if (blackMetalApplier != null)
        {
            blackMetalApplier.ApplyToAllObjects();
        }
    }

    // Remove shader
    void DisableBlackMetalMode()
    {
        if (blackMetalApplier != null)
        {
            blackMetalApplier.RemoveFromAllObjects();
        }
    }
}
```

### Per-Scene Configuration

```
Scene 1 (Normal):
- Apply On Start: ?

Scene 2 (Black Metal):
- Apply On Start: ?

Scene 3 (Mixed):
- Apply On Start: ?
- Include Tags: ["Enemy", "Environment"]
```

---

## ?? **Console Output**

**On Application:**
```
[ApplyBlackMetalToAll] Found 45 renderers in scene
[ApplyBlackMetalToAll] Found 12 skinned renderers in scene
[ApplyBlackMetalToAll] Applied to: Player_Mesh
[ApplyBlackMetalToAll] Applied to: Enemy_Mesh
[ApplyBlackMetalToAll] Skipping Main Camera - excluded name contains: Camera
[ApplyBlackMetalToAll] Complete! Applied to 52 objects, skipped 5 objects
```

**Material auto-created:**
```
[ApplyBlackMetalToAll] Auto-created Black Metal material
```

---

## ?? **Customization**

### Custom Material Settings

**Option 1: Assign Pre-Made Material**
```
1. Create material manually
2. Configure it perfectly
3. Drag to "Black Metal Material" field
4. Disable "Auto Create Material"
```

**Option 2: Adjust Auto-Created Material**
```
Adjust these settings:
- Main Color: Lighter/darker
- Outline Color: Black/white/red
- Outline Width: Thicker/thinner
- Brightness: Brighter/darker

Material auto-creates with these settings!
```

---

## ?? **Tips**

### Best Practices:
```
? Use Auto Create Material for quick setup
? Enable Apply On Start for automatic application
? Exclude UI, Camera, Light objects
? Check Console for confirmation
? Use manual application in Edit Mode for testing
```

### Performance:
```
? Application is one-time (on Start)
? No runtime overhead after application
? Can be disabled after initial application
? Material shared across all objects (efficient)
```

### Workflow:
```
1. Set up once in main scene
2. Apply automatically on Start
3. Adjust post-processing on camera
4. Test and refine
5. Ship it! ??
```

---

## ?? **Troubleshooting**

### No Objects Getting Shader

**Check:**
```
? Script attached to GameObject
? Apply On Start is checked
? Exclude Tags/Names don't match everything
? Console shows "Applied to X objects"
? Shader exists: Custom/BlackMetalOutline
```

**Fix:**
```
1. Check Exclude lists (too restrictive?)
2. Enable Show Debug Logs
3. Check Console for details
4. Try manual: Right-click ? Apply
5. Verify shader is in correct folder
```

### Some Objects Not Getting Shader

**Likely Causes:**
```
- Object has excluded tag
- Object name contains excluded string
- Object doesn't have MeshRenderer
- Override Existing is disabled and object has material
```

**Fix:**
```
1. Check object tags
2. Check object name
3. Add MeshRenderer if missing
4. Enable Override Existing
```

### Material Looks Wrong

**Adjust Settings:**
```
Main Color: (0.8, 0.8, 0.8) - lighter/darker
Outline Width: 0.005 - thicker/thinner
Brightness: 0.8 - brighter/darker
Outline Color: (0, 0, 0) - black/white
```

---

## ? **Complete Setup**

**For full black metal effect:**

```
1. BlackMetalManager GameObject:
   - ApplyBlackMetalToAll script
   - Default settings

2. Main Camera:
   - BlackMetalPostProcess script
   - Classic Transylvanian Hunger preset

3. Enter Play Mode:
   - All objects get outlines
   - Full screen effect active
   - Game looks like black metal album!
```

---

## ?? **Summary**

**What It Does:**
- Automatically applies Black Metal shader to all objects
- Configurable filters (tags, names)
- One-time setup, automatic application
- Manual trigger available
- Can remove shader if needed

**Benefits:**
- No manual material application needed
- Consistent look across all objects
- Easy to enable/disable
- Runtime control possible
- Performance friendly

**Result:**
- Every object in scene has Black Metal outline
- Combined with post-processing = full black metal aesthetic
- Darkthrone - Transylvanian Hunger look achieved!

---

**Your entire scene will now have the black metal look automatically!** ????

**Just add the script and enter Play Mode!** ?

**No more manual material assignment needed!** ?

---

## ?? **Next Steps**

1. ? Add ApplyBlackMetalToAll to scene
2. ? Configure filters if needed
3. ? Add BlackMetalPostProcess to camera
4. ? Enter Play Mode
5. ? Enjoy automatic black metal aesthetic!

**Everything is automated now!** ??
