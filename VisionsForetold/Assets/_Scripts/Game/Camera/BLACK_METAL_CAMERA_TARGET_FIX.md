# ?? BlackMetalRenderFeature - Camera Color Target Fix

## Problem Fixed

**Error:** `You can only call cameraColorTargetHandle inside the scope of a ScriptableRenderPass`

**Location:** `BlackMetalRenderFeature.cs` - `Setup()` method called from `AddRenderPasses()`

**Cause:** Trying to access `renderer.cameraColorTargetHandle` outside of the render pass execution scope

## Root Cause

In Unity's URP, the `cameraColorTargetHandle` can only be accessed **inside** the render pass methods (like `Execute()`, `OnCameraSetup()`, `Configure()`), not in `AddRenderPasses()` or custom setup methods called from it.

```csharp
// ? WRONG - Outside render pass scope
public override void AddRenderPasses(...)
{
    renderPass.Setup(renderer); // Calls cameraColorTargetHandle - ERROR!
    renderer.EnqueuePass(renderPass);
}

public void Setup(ScriptableRenderer renderer)
{
    source = renderer.cameraColorTargetHandle; // ? Error!
}
```

## Solution Applied

**Moved the `cameraColorTargetHandle` access into the `Execute()` method**, which is called within the proper render pass scope.

### Changes Made

**1. Removed Setup() Call**

```csharp
// Before:
public override void AddRenderPasses(...)
{
    renderPass.Setup(renderer); // ? Removed
    renderer.EnqueuePass(renderPass);
}

// After:
public override void AddRenderPasses(...)
{
    renderer.EnqueuePass(renderPass); // ? No setup call
}
```

**2. Removed Setup() Method**

```csharp
// Removed entirely:
public void Setup(ScriptableRenderer renderer)
{
    source = renderer.cameraColorTargetHandle;
}
```

**3. Get Source in Execute()**

```csharp
// Added to Execute():
public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
{
    // Get camera color target within render pass scope ?
    var renderer = renderingData.cameraData.renderer;
    source = renderer.cameraColorTargetHandle;
    
    // Rest of the code...
}
```

## Why This Works

### URP Render Pass Lifecycle

```
1. AddRenderPasses()        ? Enqueue passes (NO render target access)
2. OnCameraSetup()           ? Setup render targets (CAN access)
3. Configure()               ? Configure render targets (CAN access)
4. Execute()                 ? Execute rendering (CAN access) ?
5. OnCameraCleanup()         ? Cleanup
```

**Camera color target is only valid in steps 2-4!**

### Correct Pattern

```csharp
public override void Execute(...)
{
    // ? Safe - inside render pass scope
    var source = renderingData.cameraData.renderer.cameraColorTargetHandle;
    
    // Use source for blitting
    Blitter.BlitCameraTexture(cmd, source, destination, material, 0);
}
```

## Fixed Code Structure

### BlackMetalRenderPass Class

```csharp
class BlackMetalRenderPass : ScriptableRenderPass
{
    private RTHandle source;        // Store temporarily
    private RTHandle tempRTHandle;  // Our temp buffer
    
    public override void OnCameraSetup(...)
    {
        // Setup temp render texture
        RenderingUtils.ReAllocateIfNeeded(ref tempRTHandle, ...);
    }
    
    public override void Execute(...)
    {
        // Get source HERE - inside render pass scope ?
        source = renderingData.cameraData.renderer.cameraColorTargetHandle;
        
        // Apply effect
        Blitter.BlitCameraTexture(cmd, source, tempRTHandle, material, 0);
        Blitter.BlitCameraTexture(cmd, tempRTHandle, source);
    }
}
```

### BlackMetalRenderFeature Class

```csharp
public override void AddRenderPasses(...)
{
    if (settings.material == null)
        return;
    
    // Just enqueue - no setup needed ?
    renderer.EnqueuePass(renderPass);
}
```

## Testing

**Verify Fix:**
1. No error in Console ?
2. Effect applies correctly ?
3. No pink screen ?
4. Performance is good ?

**Expected Behavior:**
- Black metal effect applies to entire screen
- No console errors
- Smooth rendering
- All settings work

## Related Information

### When to Access Camera Color Target

**? Safe Locations:**
- `OnCameraSetup()`
- `Configure()`
- `Execute()`
- `OnCameraCleanup()`

**? Unsafe Locations:**
- `AddRenderPasses()`
- Custom setup methods called from `AddRenderPasses()`
- Constructor
- `Create()`

### Proper Resource Management

```csharp
// Allocate temp textures in OnCameraSetup
public override void OnCameraSetup(...)
{
    RenderingUtils.ReAllocateIfNeeded(ref tempRT, descriptor, ...);
}

// Access camera targets in Execute
public override void Execute(...)
{
    var source = renderingData.cameraData.renderer.cameraColorTargetHandle;
    // Use source...
}

// Clean up in Dispose
public void Dispose()
{
    tempRT?.Release();
}
```

## Best Practices

### For Custom Render Features

**1. Don't Cache Camera Target Early**
```csharp
// ? Bad:
private RTHandle cachedSource;

public override void AddRenderPasses(...)
{
    cachedSource = renderer.cameraColorTargetHandle; // Error!
}

// ? Good:
public override void Execute(...)
{
    var source = renderingData.cameraData.renderer.cameraColorTargetHandle;
}
```

**2. Allocate Temp Textures in OnCameraSetup**
```csharp
public override void OnCameraSetup(...)
{
    // Allocate/reallocate temp textures here
    RenderingUtils.ReAllocateIfNeeded(ref tempRT, ...);
}
```

**3. Access Renderer Resources in Execute**
```csharp
public override void Execute(...)
{
    // Get all renderer resources here
    var source = renderingData.cameraData.renderer.cameraColorTargetHandle;
    var depthTarget = renderingData.cameraData.renderer.cameraDepthTargetHandle;
}
```

## Summary

**Problem:** Accessing `cameraColorTargetHandle` outside render pass scope  
**Solution:** Move access to `Execute()` method  
**Result:** No errors, effect works perfectly  

**Key Changes:**
1. ? Removed `Setup()` call from `AddRenderPasses()`
2. ? Removed `Setup()` method entirely
3. ? Get `cameraColorTargetHandle` in `Execute()`
4. ? All within proper render pass scope

**The black metal effect now works correctly in Unity 6!** ???

## Prevention

**For Future Render Features:**

```csharp
// Template for proper URP Render Feature

public override void AddRenderPasses(...)
{
    // Just enqueue - don't access camera targets!
    renderer.EnqueuePass(renderPass);
}

public override void Execute(...)
{
    // Access camera targets HERE
    var source = renderingData.cameraData.renderer.cameraColorTargetHandle;
    
    // Do your rendering
}
```

**Remember:** Camera render targets are only available **inside** render pass execution methods!
