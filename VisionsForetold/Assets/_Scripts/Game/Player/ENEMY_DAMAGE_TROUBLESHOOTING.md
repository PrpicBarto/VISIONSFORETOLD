# ?? Enemy Damage Troubleshooting Guide

## Common Reasons Why Enemies Can't Be Damaged

Based on your PlayerAttack and Health system, here are the most likely causes:

---

## ?? **Issue 1: Layer Mask Not Set (MOST COMMON)**

### Problem:
The `aimingLayerMask` in PlayerAttack might not include the Enemy layer.

### Check:
```
1. Select Player GameObject in Hierarchy
2. Find PlayerAttack component in Inspector
3. Look at "Aiming Settings" section
4. Check "Aiming Layer Mask" field
```

### Solution:
```
Aiming Layer Mask should include:
? Default
? Enemy (or whatever layer your enemies are on)
? Environment (for projectiles to hit walls)

Should NOT include:
? Player (would hit yourself)
? UI
```

**Quick Fix:**
```
PlayerAttack ? Aiming Layer Mask ? Select "Everything"
(Then uncheck Player layer)
```

---

## ?? **Issue 2: Enemies on Wrong Layer**

### Problem:
Enemy GameObjects might not be on the "Enemy" layer.

### Check:
```
1. Select any enemy in Hierarchy
2. Look at top of Inspector
3. Check "Layer" dropdown
```

### Solution:
```
1. Select enemy prefab or instance
2. Inspector ? Layer ? Enemy
3. When prompted: "Change layer of child objects?"
4. Click "Yes, change children"
```

**For all enemies:**
```
1. Select all enemy prefabs in Project
2. Inspector ? Layer ? Enemy
3. Apply to all children
```

---

## ?? **Issue 3: No Collider on Enemies**

### Problem:
Enemies need a collider for Physics.OverlapSphere to detect them.

### Check:
```
1. Select enemy in Hierarchy
2. Look for Collider component (Capsule, Box, Sphere, etc.)
3. Make sure it's not disabled
```

### Solution:
```
If no collider:
1. Add Component ? Capsule Collider
2. Size it to fit enemy
3. Is Trigger: ? (unchecked for melee damage)

If collider exists but disabled:
1. Check the ? box next to component name
```

---

## ?? **Issue 4: Health Component Missing**

### Problem:
Enemies need a Health component to take damage.

### Check:
```
1. Select enemy in Hierarchy
2. Look for Health component in Inspector
```

### Solution:
```
If missing:
1. Add Component ? Health
2. Set Max Health: 100 (or desired value)
3. Make sure "Initialize Health On Start" is ?
```

---

## ?? **Issue 5: Attack Range Too Small**

### Problem:
Player's attack range might be too short to reach enemies.

### Check:
```
1. Select Player in Hierarchy
2. PlayerAttack component ? Attack Settings
3. Look at "Attack Range" value
```

### Solution:
```
Attack Range should be:
- Melee: 2.0 - 3.0 (current is probably too small)
- Ranged: 15.0 - 30.0

Increase if enemies are out of range
```

---

## ?? **Issue 6: Attack Cooldown Active**

### Problem:
Attacks might be on cooldown.

### Check:
```
Console log when clicking attack:
- If nothing appears, attack isn't registering
- If "Combo Hit X" appears, attack is working
```

### Solution:
```
PlayerAttack ? Attack Settings:
Attack Cooldown: 1.0 (wait 1 second between attacks)

Try attacking with 1-2 second delay between clicks
```

---

## ?? **Issue 7: Enemies Already Dead**

### Problem:
Enemies might spawn with 0 health.

### Check:
```
1. Select enemy in Hierarchy (in Play Mode)
2. Health component ? Current Health value
```

### Solution:
```
Health component:
? Initialize Health On Start
Max Health: 100
Current Health: Should auto-fill on play

If not auto-filling, set in Inspector:
Current Health: 100
```

---

## ?? **Diagnostic Steps**

### Step 1: Enable Debug Logs

**Add to PlayerAttack.cs ? PerformConeAttack method:**

Find this line (around line 401):
```csharp
return enemiesHit;
```

**Add before it:**
```csharp
Debug.Log($"[PlayerAttack] Found {hitColliders.Length} colliders in range");
Debug.Log($"[PlayerAttack] Hit {enemiesHit} enemies with {damage} damage");
```

**This will show:**
- How many colliders detected
- How many enemies actually damaged

---

### Step 2: Visualize Attack Range

**Add to PlayerAttack.cs:**

```csharp
private void OnDrawGizmosSelected()
{
    // Draw attack range sphere
    Gizmos.color = Color.red;
    Vector3 attackOrigin = transform.position + Vector3.up * 0.5f;
    Gizmos.DrawWireSphere(attackOrigin, attackRange);
    
    // Draw attack cone
    Gizmos.color = Color.yellow;
    Vector3 forward = transform.forward * attackRange;
    Gizmos.DrawLine(attackOrigin, attackOrigin + forward);
}
```

**This shows:**
- Red sphere = attack range
- Yellow line = attack direction
- Visible when Player selected in Hierarchy

---

### Step 3: Check LayerMask Value

**Add to PlayerAttack.cs ? Start() method:**

```csharp
Debug.Log($"[PlayerAttack] Aiming Layer Mask value: {aimingLayerMask.value}");
Debug.Log($"[PlayerAttack] Layer Mask includes: {LayerMask.LayerToName(aimingLayerMask)}");
```

**This shows:**
- What layers are included in attack detection
- If -1, means "Everything" (good)
- If 0, means "Nothing" (bad!)

---

### Step 4: Check Enemy Setup

**Select enemy, verify:**
```
? Layer is set to "Enemy"
? Has Collider component (Capsule, Box, or Sphere)
? Collider is enabled (checked)
? Has Health component
? Health ? Max Health > 0
? Health ? Initialize Health On Start: ?
? Health ? Current Health > 0 (in Play Mode)
```

---

## ?? **Quick Fix Checklist**

**Most common fixes that solve 90% of issues:**

```
Player Setup:
? PlayerAttack ? Aiming Layer Mask includes "Enemy" layer
? PlayerAttack ? Attack Range: 2.5 (melee) or 15+ (ranged)
? PlayerAttack ? Attack Damage > 0

Enemy Setup:
? Enemy GameObject ? Layer: Enemy
? Has Capsule Collider (or Box/Sphere)
? Collider is enabled (checked)
? Has Health component
? Health ? Max Health: 100
? Health ? Initialize Health On Start: ?

Scene Setup:
? Floor/ground has collider
? NavMesh baked (for enemy movement)
? Player and Enemy in same scene
```

---

## ?? **Testing Procedure**

### Test 1: Layer Mask
```
1. Select Player
2. PlayerAttack ? Aiming Layer Mask
3. Set to "Everything"
4. Uncheck "Player" layer
5. Test attack
```

### Test 2: Visual Debug
```
1. Select Player in Hierarchy
2. Look for red sphere (attack range) in Scene view
3. Move close to enemy (inside red sphere)
4. Attack
5. Check Console for debug messages
```

### Test 3: Manual Damage
```
1. Enter Play Mode
2. Select enemy in Hierarchy
3. Health component ? Right-click
4. Find "TakeDamage" method
5. Call with value: 10
6. Check if enemy health decreases
```

If manual damage works but attacks don't:
- Problem is in PlayerAttack detection
- Focus on LayerMask and attack range

If manual damage doesn't work:
- Problem is in Health component
- Check OnDeath event is wired correctly

---

## ?? **Most Likely Issue**

Based on common Unity issues, **99% chance it's the Layer Mask**:

**Fix:**
```
1. Select Player in Hierarchy
2. PlayerAttack component ? Aiming Settings
3. Aiming Layer Mask ? Click dropdown
4. Select "Everything"
5. Then uncheck "Player" layer
6. Test attack immediately
```

**Should work instantly!**

---

## ?? **Detailed Layer Setup**

### Create Enemy Layer (if doesn't exist):

```
1. Select any GameObject
2. Inspector ? Layer dropdown (top right)
3. Click "Add Layer..."
4. User Layer 8: "Enemy" (type this)
5. User Layer 9: "Player" (type this)
```

### Assign Layers:

```
All Enemy Prefabs:
1. Project ? Select all enemy prefabs
2. Inspector ? Layer ? Enemy
3. "Change children?" ? Yes

Player GameObject:
1. Hierarchy ? Select Player
2. Inspector ? Layer ? Player
```

### Configure Layer Collision (Optional):

```
Edit ? Project Settings ? Physics
Layer Collision Matrix:
- Enemy can hit: Default, Environment, Player
- Player can hit: Default, Environment, Enemy
- Enemy should NOT hit: Enemy (enemies don't damage each other)
```

---

## ?? **Expected Behavior After Fix**

**When attacking enemy:**
```
1. Click attack button
2. Console shows: "Combo Hit 1/3 - Damaged 1 enemies for 10 damage"
3. Enemy health bar decreases (if visible)
4. Enemy plays hurt animation/sound
5. After X hits, enemy dies
6. Enemy ragdolls or plays death animation
```

**If this happens, everything works!**

---

## ?? **Still Not Working?**

**If none of the above works, provide:**

```
1. Screenshot of Player ? PlayerAttack component
2. Screenshot of Enemy ? Health component  
3. Screenshot of Enemy ? Inspector (top, showing Layer)
4. Console log messages when attacking
5. Video/GIF of attack attempt in Scene view
```

**Common edge cases:**

- Using old EnemyHealth instead of Health
- Health script disabled
- Enemy has multiple Health components
- PlayerAttack script disabled
- Input not wired correctly
- Animator blocking attacks

---

## Summary

**Most Common Issue:** Layer Mask not including Enemy layer

**Quick Fix:**
```
PlayerAttack ? Aiming Layer Mask ? Everything (except Player)
```

**If that doesn't work:**
```
1. Check enemies have Health component
2. Check enemies have Collider
3. Check enemies on Enemy layer
4. Check attack range is big enough
```

**99% of the time, it's the Layer Mask!** ??

---

**Your enemies should now take damage!** ???
