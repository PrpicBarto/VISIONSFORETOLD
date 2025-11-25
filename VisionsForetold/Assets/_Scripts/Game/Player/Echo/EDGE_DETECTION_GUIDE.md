# Echo Edge Detection System - True Sonar Effect

## Overview

The **EchoEdgeDetection** system creates a true sonar effect by **detecting and rendering the edges/outlines** of GameObjects when the echolocation pulse intersects with them. Objects appear as glowing silhouettes/contours when hit by the pulse.

## How It Works

1. **Pulse Expands** - Echolocation pulse grows from player
2. **Raycasts Detect Edges** - System casts rays at pulse edge
3. **Object Hit** - When ray hits an object
4. **Edge Outline Created** - Glowing outline/silhouette appears
5. **Edges Persist** - Outlines stay visible for duration
6. **Fade Out** - Edges gradually fade away

## Visual Effect

```
Before Pulse:
[????????????????] (Complete darkness)

Pulse Hits Object:
[????????????????] (Edge outline appears)
     ??? Glowing silhouette

After Pulse Passes:
[????????????????] (Outline stays visible)
     ??? Fades out over time
```

## Quick Setup

### Step 1: Create Edge Material
```
1. Right-click in Project ? Create ? Material
2. Name: "EchoEdgeMaterial"
3. Shader: Custom/URP/EchoEdgeOutline
4. Set Edge Color (cyan recommended)
```

### Step 2: Add Component
```
1. Select GameObject with EcholocationController
2. Add Component ? Echo Edge Detection
3. Assign Edge Material to "Edge Material" slot
```

### Step 3: Configure (Optional)
```
Inspector ? Echo Edge Detection:
  Raycast Resolution: 64 (detection quality)
  Edge Color: Cyan/Blue (outline color)
  Edge Visibility Duration: 2.0 (seconds visible)
  Edge Thickness: 0.01 (outline thickness)
```

### Step 4: Press Play!
Objects hit by pulse will show glowing edge outlines!

## Configuration

### Detection Settings

| Setting | Description | Default |
|---------|-------------|---------|
| **Detection Layers** | Which layers to detect | Everything |
| **Raycast Resolution** | Number of rays (higher = better) | 64 |
| **Detection Threshold** | Distance from pulse edge | 2.0 |
| **Detect 3D** | Enable vertical raycasting | false |
| **Vertical Ray Count** | Vertical rays (if 3D) | 5 |
| **Vertical Range** | Height range for 3D | 10 |

### Edge Rendering

| Setting | Description | Default |
|---------|-------------|---------|
| **Edge Material** | Material for outlines | (assign manually) |
| **Edge Color** | Outline color | Cyan (0.3, 0.8, 1) |
| **Edge Thickness** | Outline width | 0.01 |
| **Edge Visibility Duration** | How long visible | 2.0s |
| **Edge Fade Out Duration** | Fade out time | 0.5s |

### Performance

| Setting | Description | Default |
|---------|-------------|---------|
| **Max Tracked Objects** | Maximum simultaneous edges | 50 |
| **Detection Frame Interval** | Update every N frames | 1 |

## How Edge Rendering Works

### Outline Technique
The system uses **inverted hull** method:
1. **Duplicate object mesh**
2. **Slightly expand it** (scale * 1.01)
3. **Render with front-face culling** (shows only back faces)
4. **Apply edge material** (glowing color)

Result: Glowing outline around object silhouette

### Shader Details (`EchoEdgeOutline.shader`)
```hlsl
// Expand vertices along normals
expandedPos = position + normal * 0.01;

// Cull front faces (show only back/expanded parts)
Cull Front

// Render with transparency
color = EdgeColor with Alpha
```

## Usage Examples

### Basic Usage
```csharp
// Component works automatically when attached!
// No code needed for basic functionality
```

### Query Detected Objects
```csharp
using VisionsForetold.Game.Player.Echo;

EchoEdgeDetection edgeDetection = GetComponent<EchoEdgeDetection>();

// Get count
int count = edgeDetection.GetDetectedObjectCount();

// Get all detected objects
List<GameObject> detected = edgeDetection.GetDetectedObjects();
```

### Clear All Edges
```csharp
// Immediately remove all edge outlines
edgeDetection.ClearAllEdges();
```

### Adjust Settings at Runtime
```csharp
// Change edge color
edgeMaterial.SetColor("_EdgeColor", Color.red);

// Change alpha
edgeMaterial.SetFloat("_Alpha", 0.5f);
```

## Visual Customization

### Edge Colors by Game Type

#### Sci-Fi/Tech
```csharp
edgeColor = new Color(0.0f, 1.0f, 1.0f, 1.0f); // Cyan
```

#### Horror
```csharp
edgeColor = new Color(0.5f, 0.1f, 0.1f, 1.0f); // Dark red
```

#### Fantasy
```csharp
edgeColor = new Color(0.7f, 0.3f, 1.0f, 1.0f); // Purple
```

#### Retro/Tron
```csharp
edgeColor = new Color(0.0f, 1.0f, 0.0f, 1.0f); // Green
```

### Edge Thickness

```csharp
// Thin edges (subtle):
edgeThickness = 0.005f;

// Standard edges (balanced):
edgeThickness = 0.01f;

// Thick edges (dramatic):
edgeThickness = 0.03f;
```

### Visibility Duration

```csharp
// Brief flash (intense):
edgeVisibilityDuration = 0.5f;

// Standard (balanced):
edgeVisibilityDuration = 2.0f;

// Long persistence (exploration):
edgeVisibilityDuration = 5.0f;
```

## Combining Systems

Use **all systems** together for best effect:

```
GameObject
?? EcholocationController (pulse + fog)
?? EchoEdgeDetection (edge outlines) ? NEW!
?? EchoPulseMemory (area memory - optional)
?? EchoRevealSystem (object reveals - optional)
?? EchoIntersectionDetector (highlights + events)
```

**Result:**
- Global fog covers world
- Pulse expands revealing sphere
- **Object edges/outlines appear** (edge detection)
- Objects highlighted (intersection detector)
- Events triggered (intersection detector)

## Performance Optimization

### Raycasts Per Frame
```csharp
// Low Performance (Mobile):
raycastResolution = 32;

// Balanced (PC):
raycastResolution = 64;

// High Quality (High-end PC):
raycastResolution = 128;
```

### Max Tracked Objects
```csharp
// Few objects (faster):
maxTrackedObjects = 25;

// Standard:
maxTrackedObjects = 50;

// Many objects (slower):
maxTrackedObjects = 100;
```

### Detection Frame Interval
```csharp
// Every frame (accurate):
detectionFrameInterval = 1;

// Every 2 frames (faster):
detectionFrameInterval = 2;

// Every 3 frames (much faster):
detectionFrameInterval = 3;
```

## Troubleshooting

### Edges Not Appearing

**Check:**
- [ ] Edge Material is assigned
- [ ] Edge Material uses "Custom/URP/EchoEdgeOutline" shader
- [ ] Objects have MeshFilter + MeshRenderer
- [ ] Objects have Colliders
- [ ] Objects are on Detection Layers
- [ ] Pulse is expanding

### Edges Are Invisible

**Check:**
- [ ] Edge Color alpha > 0
- [ ] Edge Material is transparent (Rendering Mode)
- [ ] Camera can see objects
- [ ] Edge Thickness > 0

### Edges Look Wrong/Broken

**Causes:**
1. **Mesh has no normals** - Edges won't expand correctly
2. **Complex/non-manifold mesh** - Outline artifacts
3. **Too thick** - Edges overlap with object

**Solutions:**
```csharp
// Reduce thickness:
edgeThickness = 0.03f ? 0.005f;

// Use simpler collision mesh
// Ensure mesh has proper normals
```

### Performance Issues

**Solutions:**
1. Reduce raycast resolution:
   ```csharp
   raycastResolution = 64 ? 32;
   ```

2. Increase frame interval:
   ```csharp
   detectionFrameInterval = 1 ? 2;
   ```

3. Reduce max tracked objects:
   ```csharp
   maxTrackedObjects = 50 ? 25;
   ```

4. Use layer masks:
   ```csharp
   detectionLayers = LayerMask.GetMask("Default", "Enemy");
   ```

### Edges Disappear Too Fast

```csharp
edgeVisibilityDuration = 2.0f ? 4.0f;
edgeFadeOutDuration = 0.5f ? 1.0f;
```

### Edges Stay Too Long

```csharp
edgeVisibilityDuration = 2.0f ? 0.8f;
```

## Technical Details

### Edge Creation Process

1. **Detection:**
   - Raycast hits object at pulse edge
   - System gets object's Renderer + MeshFilter

2. **Duplicate Mesh:**
   - Create new GameObject
   - Copy mesh from original
   - Parent to original object

3. **Expand & Style:**
   - Scale slightly larger (1 + edgeThickness)
   - Apply edge material
   - Set to cull front faces

4. **Fade & Cleanup:**
   - Update alpha over time
   - Destroy after visibility duration

### Shader Technique: Inverted Hull

```
Original Object:    [???]
Expanded Duplicate: [?????]
Cull Front Faces:   [?  ?]  ? Only back faces visible
Result:              ??????  ? Outline effect!
```

## Comparison with Other Systems

| Feature | EchoEdgeDetection | EchoRevealSystem | EchoIntersectionDetector |
|---------|-------------------|------------------|--------------------------|
| **Effect** | Edge outlines | Clears fog | Emission glow |
| **Visual** | Silhouette/contour | Full object visible | Highlight overlay |
| **Performance** | Medium (mesh duplication) | Light (shader only) | Light (material swap) |
| **Best For** | Sonar aesthetic | Object persistence | Event triggering |
| **Drawbacks** | Mesh-dependent | Needs colliders | Less dramatic |

## Example Configurations

### Subtle Sonar
```csharp
edgeColor = new Color(0.2f, 0.5f, 0.7f, 0.6f); // Faint blue
edgeThickness = 0.005f;
edgeVisibilityDuration = 1.5f;
```

### Dramatic Sci-Fi
```csharp
edgeColor = new Color(0.0f, 1.0f, 1.0f, 1.0f); // Bright cyan
edgeThickness = 0.02f;
edgeVisibilityDuration = 3.0f;
```

### Horror/Dark
```csharp
edgeColor = new Color(0.6f, 0.1f, 0.1f, 0.8f); // Dark red
edgeThickness = 0.008f;
edgeVisibilityDuration = 2.0f;
```

### Retro/Wireframe
```csharp
edgeColor = new Color(0.0f, 1.0f, 0.0f, 1.0f); // Green
edgeThickness = 0.005f;
edgeVisibilityDuration = 1.0f;
```

## Best Practices

### ? DO:
- Use simple collision meshes for better performance
- Adjust edge thickness based on object size
- Use layer masks to filter detection
- Test with various object shapes
- Combine with fog system for full effect

### ? DON'T:
- Use extremely high raycast resolution (>128)
- Set edge thickness too high (>0.05)
- Track too many objects simultaneously (>100)
- Apply to objects without proper meshes
- Forget to assign edge material

## Advanced Customization

### Custom Edge Shader

You can modify `EchoEdgeOutline.shader` for:
- **Animated edges** (pulsing, scrolling)
- **Gradient colors**
- **Texture-based effects**
- **Wireframe rendering**

### Example: Pulsing Edges
```hlsl
// In fragment shader:
float pulse = sin(_Time.y * 3.0) * 0.5 + 0.5;
color.a *= _Alpha * pulse;
```

### Example: Distance-Based Color
```hlsl
// In fragment shader:
float dist = length(_WorldSpaceCameraPos - input.positionWS);
color.rgb = lerp(_EdgeColor.rgb, float3(1,0,0), saturate(dist / 50.0));
```

## Integration Examples

### With Quest System
```csharp
edgeDetection.OnObjectDetected += obj =>
{
    if (obj.CompareTag("QuestTarget"))
    {
        QuestManager.Instance.MarkObjectFound(obj);
    }
};
```

### With Enemy AI
```csharp
// Enemies react when their edges are revealed
edgeDetection.OnObjectDetected += obj =>
{
    EnemyAI enemy = obj.GetComponent<EnemyAI>();
    if (enemy != null)
    {
        enemy.AlertToNoise(player.position);
    }
};
```

---

**Created for VISIONSFORETOLD** - True sonar edge detection system
**Effect:** Objects appear as glowing silhouettes/contours when pulse hits
**Performance:** Medium impact (mesh duplication)
**Compatibility:** Works with all echo systems
