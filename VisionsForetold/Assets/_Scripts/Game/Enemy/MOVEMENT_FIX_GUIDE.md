# ?? Enemy Movement Issues - FIXED!

## Problems Found & Fixed

### ? Issue #1: NavMeshAgent Speed Never Set
**Problem**: All enemy scripts defined `moveSpeed` but never assigned it to `agent.speed`

**Fixed In:**
- `BaseEnemy.cs` - Now sets `agent.speed = moveSpeed` in Awake()
- `MeleeEnemy.cs` - Now sets `agent.speed = chaseSpeed` in Awake()
- `RangedEnemy.cs` - Now sets `agent.speed = moveSpeed` in Awake()

**Before:**
```csharp
[SerializeField] protected float moveSpeed = 3f; // Defined but never used!
```

**After:**
```csharp
if (agent != null)
{
    agent.speed = moveSpeed; // Now properly assigned!
}
```

### ? Issue #2: Wrong Health Property Access
**Problem**: Code was checking `health.isDead` (private field) instead of `health.IsDead` (public property)

**Fixed In:**
- `BaseEnemy.cs` - Changed `health.isDead` to `health.IsDead`

**The Health component has:**
```csharp
private bool isDead = false;     // ? Private - can't access from outside!
public bool IsDead => isDead;    // ? Public property - use this!
```

## Additional Checklist (In Unity Editor)

### 1. Check NavMesh Baking
- [ ] Window ? AI ? Navigation
- [ ] Ensure your scene has a baked NavMesh
- [ ] Floor/ground should be blue (walkable)
- [ ] Check "Agent Radius" and "Agent Height" match your enemy size

### 2. Check Enemy GameObject Setup
For EACH enemy in your scene:

**Required Components:**
- [ ] `NavMeshAgent` component attached
- [ ] `Health` component attached (NOT `EnemyHealth`!)
- [ ] Enemy script attached (Ghoul, Ghost, MeleeEnemy, etc.)

**NavMeshAgent Settings:**
- [ ] **Speed**: Should match your moveSpeed value (3-4)
- [ ] **Angular Speed**: 120 (default is fine)
- [ ] **Acceleration**: 8 (default is fine)
- [ ] **Stopping Distance**: 0.5 - 1.0
- [ ] **Auto Braking**: ? Checked
- [ ] **Obstacle Avoidance**: Quality - High or Medium

**Transform:**
- [ ] Enemy is placed ON the NavMesh (not floating, not underground)
- [ ] Enemy Y position is at ground level

### 3. Check Player Setup
- [ ] Player GameObject exists in scene
- [ ] Player has **Tag: "Player"** (very important!)
- [ ] Player is on a layer that enemies can detect

### 4. Common Unity 6 NavMesh Issues

**Issue: NavMeshAgent shows "Not on NavMesh" warning**
- Solution: Move enemy slightly up or down until on NavMesh
- Or rebake NavMesh with larger Agent Radius

**Issue: Enemies slide instead of walk**
- Check: NavMeshAgent ? Base Offset should be 0
- Check: Rigidbody if present should be Kinematic

**Issue: Enemies spin/rotate weirdly**
- Set: NavMeshAgent ? Angular Speed to 120-200
- Check: No conflicting rotation scripts

### 5. Debug in Play Mode

Add this to any enemy to see debug info:
```csharp
void OnDrawGizmos()
{
    if (Application.isPlaying && agent != null)
    {
        // Draw line to player
        if (player != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, player.position);
        }
        
        // Draw agent path
        if (agent.hasPath)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, agent.destination);
        }
        
        // Draw detection range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}
```

### 6. Console Debug Messages

When playing, you should see:
```
Ghost: Agent ready - enabled=True, isOnNavMesh=True
Ghoul (Basic) bit player for 15 damage!
```

If you see:
```
Warning: Agent not ready - enabled=False, isOnNavMesh=False
```

**Then:**
- Enemy is not on NavMesh
- NavMeshAgent is disabled
- NavMesh wasn't baked properly

## Quick Test Procedure

1. **Start Play Mode**
2. **Open Console** (Ctrl+Shift+C)
3. **Select an enemy** in Hierarchy
4. **Check Inspector:**
   - NavMeshAgent ? Speed should be 3.0 or higher (not 0!)
   - NavMeshAgent ? "On NavMesh" should say "Yes"
   - Health component ? Is Dead should be "False"
5. **Move player close to enemy**
6. **Enemy should start moving toward you**

## Still Not Working?

### Check These:

**1. NavMesh Not Baked**
- Window ? AI ? Navigation ? Bake tab
- Click "Bake" button
- Blue areas = walkable NavMesh

**2. Enemy Not On NavMesh**
- Select enemy
- Scene view should show blue NavMesh underneath
- If not, move enemy to valid NavMesh area

**3. Speed Still Zero**
- Select enemy in Hierarchy
- Play mode
- Inspector ? NavMeshAgent ? Speed = should be 3.0+
- If it's 0, the Awake() didn't run

**4. Player Tag Missing**
- Select Player GameObject
- Inspector ? Tag dropdown (top)
- Should be "Player"
- If not, set it to Player

**5. Distance Too Far**
- Enemies detect at `detectionRange` (10 units default)
- Move player VERY close to enemy
- If enemy then moves, increase detectionRange to 20

## Enemy Speed Reference

| Enemy Type | Speed | Behavior |
|------------|-------|----------|
| Ghoul (Basic) | 3.0 | Chase player |
| Ghoul (Fast) | 4.5 | Fast chase |
| Ghoul (Strong) | 2.5 | Slow but strong |
| Ghost (All) | 3.0 | Keep distance, shoot |
| MeleeEnemy | 3.5 | Chase |
| RangedEnemy | 3.0 | Keep distance |

All speeds are now properly assigned in the fixed code!

## Files Modified
- ? `BaseEnemy.cs` - Added agent.speed assignment
- ? `MeleeEnemy.cs` - Added agent.speed assignment
- ? `RangedEnemy.cs` - Added agent.speed assignment
- ? All fixed health.IsDead property access

Your enemies should now move! ??
