# ?? Cone-Based Melee Attack System

## Overview

The melee attack has been reworked from a single-target raycast to a **cone-based area attack** that can damage multiple enemies in front of the player, similar to ARPG games like Diablo or Path of Exile.

## How It Works

### Cone Attack Mechanics

```
Player Position
     ?
     ?
    /|\     ? Attack Cone (90° by default)
   / | \
  /  |  \   ? Detects enemies within this cone
 /   |   \
?----?----? ? Attack Range (2m by default)
     ?
  Enemies Hit!
```

### Detection Process

1. **Sphere Check**: Finds all colliders within `attackRange` radius
2. **Cone Filter**: Checks if each enemy is within the cone angle
3. **Damage Application**: Damages up to `maxMeleeTargets` enemies
4. **Visual Feedback**: Shows damage numbers for each enemy hit

## Inspector Settings

### Melee Attack Settings

| Setting | Default | Description |
|---------|---------|-------------|
| **Attack Range** | 2.0 | Radius of the attack sphere |
| **Melee Attack Angle** | 90° | Cone angle in degrees (0-360°) |
| **Use Simple Sphere Detection** | False | If true, hits all enemies in range (no cone) |
| **Max Melee Targets** | 5 | Maximum enemies damaged per attack |

### Attack Range

Controls how far the attack reaches:
- **Small (1-1.5m)**: Close-range, precise attacks
- **Medium (2-3m)**: Standard melee range (recommended)
- **Large (3-5m)**: Wide sweeping attacks

### Melee Attack Angle

Controls the width of the attack cone:
- **Narrow (45-60°)**: Focused, single-direction attacks
- **Medium (90-120°)**: Standard cone (recommended)
- **Wide (150-180°)**: Sweeping attacks, almost full frontal arc
- **Full (360°)**: Hits enemies all around (with sphere detection)

### Max Melee Targets

Limits how many enemies can be damaged per swing:
- **1-2**: Single/dual target
- **3-5**: Small group (recommended)
- **6-10**: Large group/crowd control
- **Unlimited**: Set to 999 for no limit

## Combo System Integration

The cone attack works seamlessly with the existing combo system:

### Combo Progression
```
Hit 1: Normal damage ? Hits enemies in cone
Hit 2: Normal damage ? Hits enemies in cone  
Hit 3: CRITICAL! 2.5x damage ? Hits ALL enemies in cone with bonus damage
```

### Visual Feedback
```
Combo: ? ? ? (2/3)        ? Combo progress
Melee attack hit 3 enemies! ? Hit count
```

## Code Example

### Basic Cone Attack
```csharp
[Header("Melee Attack Settings")]
[SerializeField] private float attackRange = 2.5f;
[SerializeField] private float meleeAttackAngle = 90f;
[SerializeField] private int maxMeleeTargets = 5;
```

### Wide Sweeping Attack
```csharp
[SerializeField] private float attackRange = 3.5f;
[SerializeField] private float meleeAttackAngle = 150f;
[SerializeField] private int maxMeleeTargets = 8;
```

### Precise Strike
```csharp
[SerializeField] private float attackRange = 1.5f;
[SerializeField] private float meleeAttackAngle = 45f;
[SerializeField] private int maxMeleeTargets = 2;
```

### 360° Spin Attack
```csharp
[SerializeField] private float attackRange = 3f;
[SerializeField] private bool useSimpleSphereDetection = true; // ? Hits all around!
[SerializeField] private int maxMeleeTargets = 10;
```

## Gizmo Visualization

In the Scene view with the player selected, you'll see:

**Cone Mode:**
- Red wireframe sphere = Attack range
- Red lines = Cone boundaries
- Red arc = Cone angle visualization

**Sphere Mode:**
- Red wireframe sphere = Attack range (hits everything inside)

## Gameplay Tuning

### For Fast Combat (Hack & Slash)
```
Attack Range: 2.0-2.5m
Attack Angle: 120-150°
Max Targets: 6-8
Attack Cooldown: 0.5s
```

### For Tactical Combat (Souls-like)
```
Attack Range: 1.5-2.0m
Attack Angle: 60-90°
Max Targets: 2-3
Attack Cooldown: 1.0s
```

### For Crowd Control (Dynasty Warriors)
```
Attack Range: 3.0-4.0m
Attack Angle: 150-180°
Max Targets: 10-15
Attack Cooldown: 0.8s
```

## Performance Notes

- **Efficient**: Uses `Physics.OverlapSphere` (Unity's optimized method)
- **Smart Targeting**: Prevents hitting same enemy multiple times
- **Scalable**: Limit max targets to control performance
- **Layer Masked**: Only checks specified layers

## Advanced Features

### Unique Enemy Tracking
The system uses a `HashSet` to ensure each enemy is only damaged once per attack, even if they have multiple colliders (ragdoll, hitboxes, etc.).

### Root Object Detection
If a collider doesn't have a `Health` component, the system checks the root GameObject (useful for complex enemy hierarchies).

### Combo Damage Scaling
Final combo hit automatically multiplies damage for ALL enemies in the cone:
```
Normal hit: 10 damage × 3 enemies = 30 total
Final hit:  25 damage × 3 enemies = 75 total (2.5x multiplier!)
```

## Troubleshooting

### No Enemies Getting Hit

**Check:**
1. Enemies are within `attackRange` (2m default)
2. Enemies have `Health` component
3. `aimingLayerMask` includes enemy layer
4. Enemies are in front of player (within cone angle)

**Test:**
- Set `useSimpleSphereDetection = true`
- Increase `meleeAttackAngle` to 180°
- Increase `attackRange` to 5m

### Too Many/Few Enemies Hit

**Adjust:**
- `maxMeleeTargets` - Limit number of hits
- `meleeAttackAngle` - Narrow/widen cone
- `attackRange` - Shorter/longer reach

### Enemies Behind Player Getting Hit

**Fix:**
- Reduce `meleeAttackAngle` (try 90° or less)
- Make sure `useSimpleSphereDetection = false`
- Check player is facing correct direction

### Visual Cone Not Matching Hits

**Verify:**
- Gizmos are enabled in Scene view
- Player is selected in Hierarchy
- Attack angle matches inspector value

## Comparison: Old vs New

### Old System (Single Raycast)
```
? Only hits ONE enemy
? Must aim precisely
? Misses if enemy slightly off-center
? Very precise
```

### New System (Cone Attack)
```
? Hits MULTIPLE enemies
? Forgiving aiming
? Great for crowd control
? Feels more impactful
? Better for ARPG gameplay
```

## Future Enhancements

Possible additions to the system:

1. **Knockback**: Push enemies back when hit
2. **Different Combos**: Each hit has different cone size
3. **Weapon Types**: Sword = wide cone, Dagger = narrow cone
4. **Charge Attacks**: Hold button for larger cone
5. **Elemental Effects**: Fire spreads between enemies in cone

## Files Modified
- ? `PlayerAttack.cs` - Complete melee attack rework
  - Added cone detection system
  - Added multi-target damage
  - Added visual gizmos
  - Integrated with combo system

Enjoy your new area melee attacks! ???
