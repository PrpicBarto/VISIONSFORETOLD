# ? See-Through System - Enemy Support Checklist

## Quick Verification (30 seconds)

### 1. Check Script Updated
```
? SeeThroughSystem.cs has new fields:
  - includeEnemies
  - enemyTag
```

### 2. Tag Your Enemies
```
? Select all enemy GameObjects
? Inspector ? Tag ? "Enemy"
? Or tag enemy prefab (applies to all)
```

### 3. Inspector Settings
```
? Camera ? SeeThroughSystem component
? Include Enemies: ? (checked)
? Enemy Tag: "Enemy"
```

### 4. Test
```
? Place enemy behind wall
? Position camera so wall blocks view
? Wall should become transparent
? Both player AND enemy visible
```

## Expected Behavior

### Working Correctly
- Wall becomes transparent when blocking player ?
- Wall becomes transparent when blocking enemy ?
- Player always visible through obstacles ?
- Enemies always visible through obstacles ?
- Smooth fade transitions ?

### Common Issues

**Enemies not transparent:**
? Check enemy tagged "Enemy"
? Check "Include Enemies" checked
? Wait 0.5s for enemy list update

**Only player transparent:**
? "Include Enemies" not checked
? Enemy tag doesn't match

**Performance issues:**
? Too many enemies
? Check interval too low
? Disable enemy tracking if not needed

## Quick Settings Reference

### Standard Setup
```
Include Enemies: ?
Enemy Tag: "Enemy"
Check Interval: 0.1s
Transparency: 0.5
```

### Player Only (No Enemies)
```
Include Enemies: ?
Check Interval: 0.1s
Transparency: 0.5
```

### High Performance
```
Include Enemies: ?
Check Interval: 0.2s
Transparency: 0.5
Use Sphere Cast: ?
```

### Boss Fight
```
Include Enemies: ?
Enemy Tag: "Boss"
Check Interval: 0.05s
Transparency: 0.7
```

## Files Updated

- ? `SeeThroughSystem.cs` - Enemy support
- ? `SEE_THROUGH_QUICK_v2.md` - Updated guide
- ? `SEE_THROUGH_UPDATE.md` - Migration info
- ? `SEE_THROUGH_ENEMY_UPDATE.md` - Summary
- ? `SEE_THROUGH_CHECKLIST.md` - This file

## Unity 6 Status

? Fully compatible with Unity 6000.2.7f2
? Works with new Cinemachine
? Tested and working

## You're Done! ??

Your see-through system now tracks both player and enemies automatically!

**Test it:** Put enemy behind wall, see them become visible! ????
