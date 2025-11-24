# Echolocation Quick Start Guide

## 5-Minute Setup

### Step 1: Create Material (30 seconds)
```
1. Right-click in Project ? Create > Material
2. Name: "EcholocationFog"
3. Shader: Custom/URP/Echolocation
```

### Step 2: Add to Scene (1 minute)
```
1. Select Player GameObject (or create empty GameObject)
2. Add Component ? Echolocation Controller
3. Drag material to "Fog Material" slot
4. Press Play!
```

### Step 3: Tweak Settings (Optional)
```
Adjust in Inspector:
- Fog Color: How dark the fog is
- Pulse Speed: How fast pulses expand
- Permanent Reveal Radius: Visible area around player
```

## Common Settings Presets

### Default (Balanced)
```
Pulse Speed: 20
Max Radius: 40
Pulse Interval: 2.5s
Fog Density: 0.95
Reveal Radius: 15
```

### Sonar (Hardcore)
```
Pulse Speed: 30
Max Radius: 50
Pulse Interval: 3s
Fog Density: 1.0
Reveal Radius: 0 (player also in darkness!)
```

### Atmospheric (Casual)
```
Pulse Speed: 15
Max Radius: 60
Pulse Interval: 2s
Fog Density: 0.7
Reveal Radius: 25
```

## Code Snippets

### Trigger Pulse on Button Press
```csharp
using VisionsForetold.Game.Player.Echo;

public class PlayerController : MonoBehaviour
{
    private EcholocationController echo;
    
    void Start()
    {
        echo = GetComponent<EcholocationController>();
    }
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            echo.TriggerPulse();
        }
    }
}
```

### Enable on Enter Dark Area
```csharp
void OnTriggerEnter(Collider other)
{
    if (other.CompareTag("DarkZone"))
    {
        echo.SetEnabled(true);
    }
}

void OnTriggerExit(Collider other)
{
    if (other.CompareTag("DarkZone"))
    {
        echo.SetEnabled(false);
    }
}
```

### Pulse on Enemy Detection
```csharp
void OnEnemyDetected()
{
    echo.TriggerPulse();
    echo.StopAutoPulse(); // Stop auto, manual control only
}
```

## Troubleshooting Checklist

- [ ] Material assigned in Inspector?
- [ ] Shader is "Custom/URP/Echolocation"?
- [ ] Player transform assigned or has "Player" tag?
- [ ] Fog Density > 0.5?
- [ ] Enable Echolocation is checked?
- [ ] Camera can see "Ignore Raycast" layer?

## Performance Tips

? Good for performance:
- Single fog plane
- Auto-pulse every 2-3 seconds
- Plane size matching level size

? Bad for performance:
- Very large plane size (>500 units)
- Very frequent pulses (<0.5s interval)
- Multiple echolocation controllers

## Support

See full documentation in README.md
Check EcholocationController.cs for all public methods
