# ?? Animation System - Quick Reference Card

## ? What's Integrated

### Automatic Triggers
- ? **Melee Attack** ? Triggers on attack in PlayerAttack.cs
- ? **Bow Attack** ? Triggers on ranged attack in PlayerAttack.cs
- ? **Fireball** ? Triggers on fireball cast in PlayerAttack.cs
- ? **Ice Spell** ? Triggers on ice cast in PlayerAttack.cs
- ? **Hurt** ? Triggers when taking damage in Health.cs
- ? **Walk/Idle** ? Auto-managed based on movement
- ? **Run** ? Auto-enabled when Speed > 1.2 (max speed)
- ? **Sprint** ? Auto-managed based on sprint state
- ? **Walk_Hurt** ? Auto-enabled when health ? 30%
- ? **Dodge** ? Auto-triggered on dodge input

## Animator Parameters

### Add These to Your Animator Controller:

**Bools:**
```
IsMoving
IsSprinting
IsRunning
IsLowHealth
```

**Floats:**
```
Speed
HealthPercentage
```

**Triggers:**
```
Attack
AttackBow
SpellFireball
SpellIce
Hurt
Dodge
Dash
Walk (optional)
WalkHurt (optional)
Idle (optional)
Run (optional trigger, or use IsRunning bool)
```

## File Changes

### PlayerMovement.cs
```csharp
// New public methods:
TriggerAttack()           // Melee
TriggerAttackBow()        // Bow
TriggerSpellFireball()    // Fireball
TriggerSpellIce()         // Ice
TriggerHurt()             // Damage
TriggerDash()             // Dash

// New properties:
IsLowHealth              // Health < 30%
HealthPercentage         // 0-1 health ratio
IsRunning                // Speed > 1.2 (max speed)
```

### PlayerAttack.cs
```csharp
// Auto-triggers animations:
PerformMeleeAttack()     ? TriggerAttack()
PerformRangedAttack()    ? TriggerAttackBow()
CastFireball()           ? TriggerSpellFireball()
CastIceBlast()           ? TriggerSpellIce()
```

### Health.cs
```csharp
// Auto-triggers animation:
TakeDamage()             ? TriggerHurt()
```

## Testing

### Quick Test Checklist:
```
? Idle plays when standing still
? Walk plays when moving slowly
? Run plays when moving at max speed (Speed > 1.2)
? Walk_Hurt plays when health < 30%
? Sprint increases speed
? Attack plays on melee (Mouse1 in melee mode)
? AttackBow plays on ranged (Mouse1 in ranged mode)
? SpellFireball plays on fireball cast
? SpellIce plays on ice cast
? Hurt plays when taking damage
? Dodge plays on dodge input
```

## Quick Setup

1. **Create Animator Parameters** (all listed above)
2. **Create Animation States** (idle, walk, attack, etc.)
3. **Setup Transitions:**
   - Idle ? Walk (IsMoving)
   - Walk ? Run (IsRunning or Speed > 1.2)
   - Walk ? Walk_Hurt (IsLowHealth)
   - Any State ? Combat animations (Triggers)
   - Any State ? Hurt (Trigger, high priority)
4. **Test in Play Mode**

## Debug

Enable Console to see:
```
[PlayerMovement] Triggered Attack animation
[PlayerMovement] Triggered Hurt animation
[PlayerMovement] Entered low health state
```

## Common Issues

**Animations not playing?**
- Check Animator Controller assigned
- Verify parameter names (case-sensitive!)
- Check transition conditions

**Walk_Hurt not working?**
- Reduce health below 30%
- Check IsLowHealth transition exists

**Combat animations interrupted?**
- Set HasExitTime = false
- Check transition priorities

## Integration Example

```csharp
// In any script:
PlayerMovement movement = GetComponent<PlayerMovement>();

// Check states
if (movement.IsLowHealth)
{
    // Health is low
}

if (movement.IsRunning)
{
    // Player is running at max speed
}

if (!movement.IsDodging)
{
    // Can attack
    movement.TriggerAttack();
}
```

## Animation Clips Needed

Import or create these animations:
- ? idle
- ? walk
- ? run (at max speed)
- ? walk_hurt (injured walk)
- ? attack (melee)
- ? attack_bow (ranged)
- ? spell_fireball (cast animation)
- ? spell_ice (cast animation)
- ? hurt (damage reaction)
- ? dash (quick movement)

**System is fully integrated and ready to use!** ???
