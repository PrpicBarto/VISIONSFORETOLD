# Echo Intersection Detection Guide

## Overview

The **EchoIntersectionDetector** system detects when echolocation pulses hit objects in the world, allowing you to:
- Highlight detected objects with emission glow
- Classify objects by type (Wall, Enemy, Item, etc.)
- Trigger events when specific objects are detected
- Visualize the environment through echo feedback

## Quick Setup

### Step 1: Add Component
```
1. Select the GameObject with EcholocationController
2. Add Component ? Echo Intersection Detector
3. Configure detection settings in Inspector
```

### Step 2: Tag Your Objects
```
Assign appropriate tags to objects in your scene:
- "Wall" ? Walls, buildings, obstacles
- "Enemy" ? Hostile creatures
- "Item" ? Collectibles, pickups
- "Interactive" ? Doors, levers, NPCs
- "Hazard" ? Traps, spikes, fire
```

### Step 3: Test
```
1. Press Play
2. Wait for pulse to expand
3. Objects hit by pulse ring will highlight!
```

## How It Works

### Detection Method
1. **Pulse Expands** - As the echolocation pulse grows
2. **Raycasts Fire** - System shoots rays in a circle at the pulse radius
3. **Hits Detected** - When rays hit objects near the pulse edge
4. **Highlights Applied** - Objects emit colored glow based on type
5. **Fade Out** - Highlights fade after `highlightDuration` seconds

### Detection Modes

#### 2D Detection (Default)
- Raycasts in horizontal plane only (XZ)
- Best for top-down and isometric games
- Lower performance cost

#### 3D Detection
- Raycasts at multiple heights
- Detects tall objects and vertical surfaces
- Higher performance cost but more accurate

## Configuration

### Detection Settings

| Setting | Description | Default |
|---------|-------------|---------|
| **Detection Layers** | Which layers to detect | Everything |
| **Raycast Resolution** | Number of rays (higher = better detection) | 32 |
| **Detection Threshold** | How close to pulse edge to detect (units) | 2.0 |
| **Detect 3D** | Enable vertical raycasting | false |
| **Vertical Ray Count** | Rays per angle (if 3D) | 5 |
| **Vertical Ray Height** | Height range for 3D detection | 10 |

### Object Classification Tags

| Tag | Color | Purpose |
|-----|-------|---------|
| **Wall** | Gray | Walls, buildings, terrain |
| **Enemy** | Red | Hostile NPCs, monsters |
| **Item** | Yellow | Collectibles, loot |
| **Interactive** | Green | Doors, switches, NPCs |
| **Hazard** | Orange | Traps, dangerous areas |
| **Unknown** | Cyan | Anything else |

### Highlight Settings

| Setting | Description | Default |
|---------|-------------|---------|
| **Enable Highlighting** | Turn on/off visual feedback | true |
| **Highlight Duration** | How long highlights last (seconds) | 3.0 |
| **Highlight Color** | Base emission color (if Unknown) | Cyan |
| **Highlight Intensity** | Brightness multiplier | 2.0 |

### Performance Settings

| Setting | Description | Default |
|---------|-------------|---------|
| **Max Tracked Objects** | Maximum simultaneous highlights | 100 |
| **Detection Frame Interval** | Update every N frames (1 = every frame) | 1 |

## Usage Examples

### Basic Event Subscription

```csharp
using VisionsForetold.Game.Player.Echo;

public class EchoEventHandler : MonoBehaviour
{
    private EchoIntersectionDetector detector;

    void Start()
    {
        detector = GetComponent<EchoIntersectionDetector>();
        
        // Subscribe to object detection
        detector.OnObjectDetected += HandleObjectDetected;
        
        // Subscribe to pulse completion
        detector.OnPulseComplete += HandlePulseComplete;
    }

    void OnDestroy()
    {
        // Unsubscribe
        detector.OnObjectDetected -= HandleObjectDetected;
        detector.OnPulseComplete -= HandlePulseComplete;
    }

    void HandleObjectDetected(EchoHit hit)
    {
        Debug.Log($"Detected {hit.objectType}: {hit.hitObject.name}");
        
        // React based on type
        switch (hit.objectType)
        {
            case EchoObjectType.Enemy:
                AlertPlayer("Enemy nearby!");
                break;
            case EchoObjectType.Item:
                ShowItemMarker(hit.hitPosition);
                break;
            case EchoObjectType.Hazard:
                WarnPlayer(hit.hitPosition);
                break;
        }
    }

    void HandlePulseComplete(List<EchoHit> allHits)
    {
        Debug.Log($"Pulse complete! Detected {allHits.Count} objects");
        
        // Count by type
        int enemyCount = allHits.FindAll(h => h.objectType == EchoObjectType.Enemy).Count;
        int itemCount = allHits.FindAll(h => h.objectType == EchoObjectType.Item).Count;
        
        Debug.Log($"Enemies: {enemyCount}, Items: {itemCount}");
    }
}
```

### Enemy Detection System

```csharp
public class EnemyDetector : MonoBehaviour
{
    private EchoIntersectionDetector detector;
    private List<GameObject> detectedEnemies = new List<GameObject>();

    void Start()
    {
        detector = GetComponent<EchoIntersectionDetector>();
        detector.OnObjectDetected += OnEchoDetection;
    }

    void OnEchoDetection(EchoHit hit)
    {
        if (hit.objectType == EchoObjectType.Enemy)
        {
            if (!detectedEnemies.Contains(hit.hitObject))
            {
                detectedEnemies.Add(hit.hitObject);
                NotifyUIOfEnemy(hit);
                
                // Optional: Trigger stealth warning
                if (IsPlayerInStealth())
                {
                    ShowStealthWarning();
                }
            }
        }
    }

    void NotifyUIOfEnemy(EchoHit hit)
    {
        // Update minimap, UI markers, etc.
        MinimapManager.Instance.AddEnemyMarker(hit.hitPosition);
    }
}
```

### Item Collector

```csharp
public class ItemCollector : MonoBehaviour
{
    private EchoIntersectionDetector detector;
    [SerializeField] private float autoCollectRange = 2f;

    void Start()
    {
        detector = GetComponent<EchoIntersectionDetector>();
        detector.OnObjectDetected += OnEchoDetection;
    }

    void OnEchoDetection(EchoHit hit)
    {
        if (hit.objectType == EchoObjectType.Item)
        {
            // If item is close enough, auto-collect
            if (hit.distanceFromPlayer <= autoCollectRange)
            {
                CollectItem(hit.hitObject);
            }
            else
            {
                // Otherwise, show on UI
                ItemIndicatorUI.Instance.ShowItemLocation(hit.hitPosition);
            }
        }
    }

    void CollectItem(GameObject item)
    {
        // Add to inventory, etc.
        Inventory.Instance.Add(item);
        Destroy(item);
    }
}
```

### Dynamic Layer Filtering

```csharp
public class AdaptiveDetection : MonoBehaviour
{
    private EchoIntersectionDetector detector;

    void Start()
    {
        detector = GetComponent<EchoIntersectionDetector>();
    }

    // Only detect enemies in combat
    public void EnterCombatMode()
    {
        LayerMask enemyLayer = LayerMask.GetMask("Enemy", "Hazard");
        detector.SetDetectionLayers(enemyLayer);
    }

    // Detect everything in exploration
    public void EnterExplorationMode()
    {
        LayerMask allLayers = ~0; // Everything
        detector.SetDetectionLayers(allLayers);
    }

    // Only detect interactive objects in puzzle areas
    public void EnterPuzzleMode()
    {
        LayerMask puzzleLayers = LayerMask.GetMask("Interactive", "Item");
        detector.SetDetectionLayers(puzzleLayers);
    }
}
```

### Custom Highlight Effects

```csharp
public class CustomHighlighter : MonoBehaviour
{
    private EchoIntersectionDetector detector;

    void Start()
    {
        detector = GetComponent<EchoIntersectionDetector>();
        
        // Disable built-in highlighting
        detector.SetHighlightingEnabled(false);
        
        // Use custom highlighting
        detector.OnObjectDetected += ApplyCustomHighlight;
    }

    void ApplyCustomHighlight(EchoHit hit)
    {
        // Apply your own visual effect
        switch (hit.objectType)
        {
            case EchoObjectType.Enemy:
                // Spawn particle effect
                GameObject vfx = Instantiate(enemyDetectedVFXPrefab, hit.hitPosition, Quaternion.identity);
                Destroy(vfx, 2f);
                
                // Play sound
                AudioManager.Instance.Play("EnemyDetected", hit.hitPosition);
                break;

            case EchoObjectType.Item:
                // Different effect for items
                StartCoroutine(PulseObject(hit.hitObject));
                break;
        }
    }

    IEnumerator PulseObject(GameObject obj)
    {
        Vector3 originalScale = obj.transform.localScale;
        float duration = 0.5f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float scale = 1f + Mathf.Sin(elapsed / duration * Mathf.PI) * 0.2f;
            obj.transform.localScale = originalScale * scale;
            yield return null;
        }

        obj.transform.localScale = originalScale;
    }
}
```

## Advanced Features

### Query Current Detections

```csharp
// Get all currently detected objects
List<EchoHit> hits = detector.GetCurrentHits();

// Filter by type
List<EchoHit> enemies = hits.FindAll(h => h.objectType == EchoObjectType.Enemy);

// Find closest enemy
EchoHit closestEnemy = enemies.OrderBy(h => h.distanceFromPlayer).FirstOrDefault();
```

### Manual Highlight Control

```csharp
// Clear all highlights immediately
detector.ClearAllHighlights();

// Get list of highlighted objects
List<GameObject> highlighted = detector.GetHighlightedObjects();
```

### Performance Optimization

```csharp
// Reduce raycast frequency (every 3 frames)
detector.detectionFrameInterval = 3;

// Reduce raycast count
detector.raycastResolution = 16; // Lower is faster

// Limit tracked objects
detector.maxTrackedObjects = 50;
```

## Troubleshooting

### Objects Not Being Detected

**Check:**
- [ ] Objects have colliders
- [ ] Objects are on correct layers (check Detection Layers mask)
- [ ] Pulse is expanding (check EcholocationController)
- [ ] Detection Threshold is large enough (try 5.0)
- [ ] Raycast Resolution is high enough (try 64)

### No Highlights Appearing

**Check:**
- [ ] Enable Highlighting is checked
- [ ] Objects have Renderer components
- [ ] Materials support emission (URP Lit shader recommended)
- [ ] Highlight Intensity > 0

### Performance Issues

**Solutions:**
- Reduce Raycast Resolution (16-32 is usually enough)
- Increase Detection Frame Interval (2-3 frames)
- Disable 3D detection if using 2D game
- Reduce Max Tracked Objects
- Use layer masks to filter unnecessary objects

### Wrong Object Colors

**Check:**
- Tags are assigned correctly in Inspector
- Tag names match exactly (case-sensitive!)
- Custom tag configuration in detector settings

## Performance Benchmarks

### 2D Detection (Recommended for Top-Down)
- 32 rays, every frame: ~0.1-0.2ms
- 64 rays, every frame: ~0.2-0.4ms
- 128 rays, every frame: ~0.4-0.8ms

### 3D Detection (For Full 3D Games)
- 32 rays × 5 vertical, every frame: ~0.5-1.0ms
- 32 rays × 5 vertical, every 2 frames: ~0.25-0.5ms

**Recommendation:** 32-64 rays in 2D mode is optimal for most games.

## Integration Tips

### With Enemy AI
```csharp
// Make enemies react to being detected
public class EnemyAI : MonoBehaviour
{
    void OnEchoDetected() // Called by detection system
    {
        alertLevel++;
        LookTowardsPlayer();
    }
}
```

### With Quest System
```csharp
// Use echo to reveal quest objectives
detector.OnObjectDetected += hit =>
{
    if (hit.hitObject.CompareTag("QuestItem"))
    {
        QuestManager.Instance.RevealObjective(hit.hitObject);
    }
};
```

### With Stealth Gameplay
```csharp
// Detected objects can see you
detector.OnObjectDetected += hit =>
{
    if (hit.objectType == EchoObjectType.Enemy)
    {
        hit.hitObject.GetComponent<EnemyVision>().AlertToNoise(player.position);
    }
};
```

## Example Scene Setup

```
Player
??? EcholocationController (pulse system)
??? EchoIntersectionDetector (this component)
??? Your custom scripts (event handlers)

Scene Objects:
??? Walls (Tag: "Wall", Layer: Environment)
??? Enemies (Tag: "Enemy", Layer: Enemy)
??? Items (Tag: "Item", Layer: Item)
??? Interactables (Tag: "Interactive", Layer: Interactive)
```

## Best Practices

? **DO:**
- Use appropriate tags for object classification
- Subscribe to events for game logic
- Adjust raycast resolution based on scene complexity
- Use layer masks to filter irrelevant objects
- Test performance on target hardware

? **DON'T:**
- Set raycast resolution too high (>128) unless necessary
- Detect on layers that don't need detection
- Forget to unsubscribe from events in OnDestroy
- Leave log detections enabled in builds

## Credits

Created for VISIONSFORETOLD echolocation system.
Works with Unity's Universal Render Pipeline (URP).
