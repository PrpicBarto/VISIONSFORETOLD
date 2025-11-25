# Echo Intersection Detection - Quick Reference

## ? What's Been Added

### New Component: `EchoIntersectionDetector`
Detects objects hit by echolocation pulses and provides visual/event feedback.

### Key Features
- ?? **Automatic Detection** - Raycasts at pulse edge detect nearby objects
- ?? **Smart Highlighting** - Objects glow with type-specific colors
- ?? **Event System** - Subscribe to detections for game logic
- ??? **Object Classification** - Auto-categorizes by tag (Wall, Enemy, Item, etc.)
- ? **Performance Optimized** - Configurable raycast density and update frequency

## ?? Quick Setup (3 Steps)

### 1. Add Component
```
GameObject with EcholocationController
  ?? Add Component ? Echo Intersection Detector
```

### 2. Tag Scene Objects
```
Walls:       Tag = "Wall"
Enemies:     Tag = "Enemy"
Items:       Tag = "Item"
Doors/NPCs:  Tag = "Interactive"
Traps:       Tag = "Hazard"
```

### 3. Press Play!
Objects hit by pulse rings will automatically highlight with colored emission!

## ?? Highlight Colors

| Object Type | Color | Use Case |
|-------------|-------|----------|
| Wall | Gray | Buildings, obstacles, terrain |
| Enemy | Red | Hostile NPCs, monsters |
| Item | Yellow | Collectibles, loot, pickups |
| Interactive | Green | Doors, switches, NPCs, quest objects |
| Hazard | Orange | Traps, spikes, fire, poison |
| Unknown | Cyan | Everything else |

## ?? Code Examples

### Listen for Detections
```csharp
using VisionsForetold.Game.Player.Echo;

void Start()
{
    var detector = GetComponent<EchoIntersectionDetector>();
    detector.OnObjectDetected += hit =>
    {
        Debug.Log($"Detected: {hit.objectType} at {hit.distanceFromPlayer}m");
    };
}
```

### React to Enemies
```csharp
detector.OnObjectDetected += hit =>
{
    if (hit.objectType == EchoObjectType.Enemy)
    {
        ShowWarningUI();
        PlayAlertSound();
    }
};
```

### Count All Detections
```csharp
detector.OnPulseComplete += allHits =>
{
    int enemyCount = allHits.FindAll(h => h.objectType == EchoObjectType.Enemy).Count;
    Debug.Log($"Found {enemyCount} enemies");
};
```

## ?? Inspector Settings

### Essential Settings
- **Detection Layers** - Which layers to detect (default: Everything)
- **Raycast Resolution** - Number of rays (32 = balanced, 64 = precise)
- **Detection Threshold** - How close to pulse edge (2.0 units default)
- **Enable Highlighting** - Turn visual feedback on/off

### Performance Settings
- **Detection Frame Interval** - Update every N frames (1 = every frame)
- **Max Tracked Objects** - Limit simultaneous highlights (100 default)

### Advanced
- **Detect 3D** - Enable vertical raycasting for 3D environments
- **Vertical Ray Count** - Number of vertical rays (if 3D enabled)
- **Log Detections** - Debug logging (disable in production)

## ?? Documentation Files

| File | Purpose |
|------|---------|
| **README.md** | Main echolocation system docs |
| **QUICKSTART.md** | 5-minute setup guide |
| **INTERSECTION_GUIDE.md** | Complete intersection detection guide |
| **EchoIntersectionDetector.cs** | Main detection component |
| **EchoInteractionExample.cs** | Example implementation |

## ?? Usage Scenarios

### Stealth Game
```csharp
// Alert enemies when detected
detector.OnObjectDetected += hit =>
{
    if (hit.objectType == EchoObjectType.Enemy)
    {
        hit.hitObject.GetComponent<EnemyAI>().InvestigateNoise(player.position);
    }
};
```

### Collectathon Game
```csharp
// Auto-collect nearby items
detector.OnObjectDetected += hit =>
{
    if (hit.objectType == EchoObjectType.Item && hit.distanceFromPlayer < 2f)
    {
        Inventory.Add(hit.hitObject);
        Destroy(hit.hitObject);
    }
};
```

### Puzzle Game
```csharp
// Reveal hidden switches
detector.OnObjectDetected += hit =>
{
    if (hit.objectType == EchoObjectType.Interactive)
    {
        hit.hitObject.GetComponent<HiddenSwitch>().Reveal();
    }
};
```

## ?? Customization

### Change Highlight Colors
Edit `GetHighlightColorForType()` in `EchoIntersectionDetector.cs`:
```csharp
case EchoObjectType.Enemy:
    return new Color(1f, 0f, 0f); // Pure red for enemies
```

### Adjust Highlight Duration
```csharp
// In Inspector:
Highlight Duration = 5.0  // Highlights last 5 seconds
```

### Custom Detection Logic
```csharp
// Disable built-in highlights
detector.SetHighlightingEnabled(false);

// Apply your own effects
detector.OnObjectDetected += hit =>
{
    SpawnCustomParticleEffect(hit.hitPosition);
};
```

## ?? Common Issues

### Objects Not Detected
- ? Check objects have Colliders
- ? Verify objects are on correct layers
- ? Increase Detection Threshold (try 5.0)
- ? Increase Raycast Resolution (try 64)

### No Highlights
- ? Check Enable Highlighting is checked
- ? Objects need Renderer components
- ? Materials must support emission (URP Lit shader)

### Performance Lag
- ? Reduce Raycast Resolution to 16-32
- ? Set Detection Frame Interval to 2-3
- ? Use layer masks to filter objects

## ?? Performance Tips

### Optimal Settings for Most Games
```
Raycast Resolution: 32
Detection Frame Interval: 1
Detect 3D: false (unless 3D game)
Max Tracked Objects: 100
```

### High Performance (Mobile)
```
Raycast Resolution: 16
Detection Frame Interval: 2
Detection Threshold: 3.0
```

### High Precision (PC)
```
Raycast Resolution: 64
Detection Frame Interval: 1
Detection Threshold: 1.0
Detect 3D: true (if needed)
```

## ?? Integration Examples

### With Quest System
```csharp
detector.OnObjectDetected += hit =>
{
    if (hit.hitObject.CompareTag("QuestItem"))
    {
        QuestManager.Instance.RevealObjective(hit.hitObject);
    }
};
```

### With Minimap
```csharp
detector.OnObjectDetected += hit =>
{
    if (hit.objectType == EchoObjectType.Enemy)
    {
        Minimap.Instance.AddMarker(hit.hitPosition, MarkerType.Enemy);
    }
};
```

### With Tutorial System
```csharp
detector.OnObjectDetected += hit =>
{
    if (hit.objectType == EchoObjectType.Interactive && !tutorialComplete)
    {
        Tutorial.Instance.Show("Press E to interact");
    }
};
```

## ?? Learning Path

1. **Start Simple** - Just add component and tag objects
2. **Test Detection** - Watch console for detections
3. **Add Events** - Subscribe to `OnObjectDetected`
4. **Custom Logic** - Implement game-specific behavior
5. **Optimize** - Tune performance settings
6. **Polish** - Custom highlights, sounds, UI

## ?? Support

- **Full Guide:** See `INTERSECTION_GUIDE.md`
- **Code Reference:** Check `EchoIntersectionDetector.cs` comments
- **Example:** Look at `EchoInteractionExample.cs`
- **Main System:** Refer to `README.md`

## ? Features Summary

? Automatic object detection at pulse edge
? Color-coded highlighting by object type
? Event system for game integration
? Performance optimized with configurable settings
? Works in 2D and 3D games
? Fully documented with examples
? Production-ready with debug tools

---

**Created for VISIONSFORETOLD** - Echolocation system with intersection detection.
