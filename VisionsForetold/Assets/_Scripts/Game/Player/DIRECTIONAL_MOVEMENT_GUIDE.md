# ?? Body Rotation vs Aiming - Directional Movement Guide

## The Issue

Your character's **body rotation** and **aim direction** need to be separated for proper strafe/directional animations.

**Current Problem:**
```
Character rotates entire body to movement direction
Cannot strafe while aiming in different direction
Animation always faces movement direction
```

**Desired Behavior:**
```
Character body faces movement direction
Upper body/weapon aims at mouse cursor
Can run forward while aiming backward
Proper strafe left/right animations
```

---

## ?? **Solutions (3 Options)**

### Option 1: Simple Body Rotation (Current - Basic)

**What it does:**
```
Body always faces movement direction
No independent aiming
Simple, works for basic movement
```

**Good for:**
- Single animation per direction
- No complex aiming mechanics
- Melee-focused combat

**Already Implemented:** ? (Current system)

---

### Option 2: Animation Blend Tree (Recommended for You!)

**What it does:**
```
4-8 directional run animations
Forward, Backward, Left, Right, Diagonals
Body faces movement, animations show direction
Character can aim while moving in any direction
```

**Setup Required:**
1. Import directional run animations
2. Create 2D Blend Tree
3. Add DirectionX and DirectionY parameters
4. Blend based on movement input relative to camera

**Best for:**
- Action games
- Third-person shooters
- Games with strafing
- **Your current needs!**

---

### Option 3: IK Upper Body Aiming (Advanced)

**What it does:**
```
Lower body faces movement
Upper body rotates to aim target
Procedural spine/torso rotation
Most realistic aiming
```

**Requires:**
- Unity's Animation Rigging package
- IK constraints on spine/chest
- More complex setup
- Performance overhead

**Best for:**
- AAA-quality games
- Realistic shooters
- Games requiring precise aim

---

## ? **Recommended: Blend Tree Setup**

Since you want the player to move in all directions with proper animations while aiming independently, use a **2D Blend Tree**.

### Step 1: Create Animation Parameters

**Add to Animator:**
```
DirectionX ? Float (Left -1.0 to Right 1.0)
DirectionY ? Float (Backward -1.0 to Forward 1.0)
Speed ? Float (0.0 to 1.8) - Already have this
```

### Step 2: Get/Create Directional Animations

**You need:**
```
Run Forward
Run Backward
Run Left (strafe)
Run Right (strafe)
Run Forward-Left (diagonal)
Run Forward-Right (diagonal)
Run Backward-Left (diagonal)
Run Backward-Right (diagonal)
```

**If you only have one run animation:**
```
Use it for all directions (temporary)
Mirror/rotate for different angles
Get proper directional set from Mixamo later
```

### Step 3: Create 2D Blend Tree

**In Animator:**
```
1. Delete current "Run" state
2. Right-click ? Create State ? From New Blend Tree
3. Rename to "Movement"
4. Double-click to enter blend tree
5. Set Blend Type: 2D Freeform Directional
6. Add Parameter: DirectionX (horizontal)
7. Add Parameter: DirectionY (vertical)
```

### Step 4: Add Motions to Blend Tree

**Position animations in 2D space:**
```
Position: (X, Y) = (DirectionX, DirectionY)

Forward: (0, 1)
Backward: (0, -1)
Left: (-1, 0)
Right: (1, 0)
Forward-Left: (-0.7, 0.7)
Forward-Right: (0.7, 0.7)
Backward-Left: (-0.7, -0.7)
Backward-Right: (0.7, -0.7)
Center/Idle: (0, 0) - optional
```

### Step 5: Update Code

**Add to PlayerMovement.cs UpdateAnimations():**

```csharp
private void UpdateAnimations()
{
    if (animator == null || animator.runtimeAnimatorController == null) return;

    bool isMoving = movementInput.magnitude > 0.1f && !isDodging && !isDashing;
    
    if (isMoving)
    {
        // Calculate movement direction relative to camera/character
        Vector2 moveInput = movementInput.normalized;
        
        // Set directional parameters for blend tree
        animator.SetFloat("DirectionX", moveInput.x); // Left/Right
        animator.SetFloat("DirectionY", moveInput.y); // Forward/Back
        
        // Calculate speed with sprint
        float targetSpeed = movementInput.magnitude;
        if (isSprinting)
        {
            targetSpeed *= sprintSpeedMultiplier;
        }
        
        currentAnimationSpeed = Mathf.Lerp(currentAnimationSpeed, targetSpeed, Time.deltaTime / animationSmoothTime);
        animator.SetFloat(SpeedHash, currentAnimationSpeed);
        animator.SetBool(IsMovingHash, true);
        animator.SetBool(IsRunningHash, true);
    }
    else
    {
        // Not moving - idle
        animator.SetFloat("DirectionX", 0);
        animator.SetFloat("DirectionY", 0);
        currentAnimationSpeed = Mathf.Lerp(currentAnimationSpeed, 0f, Time.deltaTime / animationSmoothTime);
        animator.SetFloat(SpeedHash, 0);
        animator.SetBool(IsMovingHash, false);
        animator.SetBool(IsRunningHash, false);
    }

    // Update other animations...
    UpdateHealthAnimations();
}
```

---

## ?? **How It Works**

### With Blend Tree:

**Moving Forward (W):**
```
DirectionX: 0
DirectionY: 1
Animation: Run Forward
Body faces forward
```

**Moving Backward (S):**
```
DirectionX: 0
DirectionY: -1
Animation: Run Backward
Body faces forward, runs backward
```

**Strafing Left (A):**
```
DirectionX: -1
DirectionY: 0
Animation: Run Left (strafe)
Body faces forward, moves left
```

**Strafing Right (D):**
```
DirectionX: 1
DirectionY: 0
Animation: Run Right (strafe)
Body faces forward, moves right
```

**Diagonal (W+D):**
```
DirectionX: 0.7
DirectionY: 0.7
Animation: Blends Forward + Right
Smooth diagonal movement
```

---

## ?? **Rotation Behavior with Blend Tree**

### Body Rotation:

**Option A: Face Movement (Current)**
```
Character rotates to face movement direction
Blend tree shows appropriate direction
Good for melee combat
```

**Option B: Face Aim Target**
```
Character always faces where aiming
Blend tree handles all 8 directions
Perfect for shooter mechanics
Can strafe while facing enemy
```

**Recommended:** Option B (Face Aim Target)

---

## ?? **Implementation: Face Aim While Moving**

### Updated HandleRotation():

```csharp
private void HandleRotation()
{
    // When using blend tree: Always face aim target
    // Blend tree handles movement direction animations
    if (aimTarget != null)
    {
        RotateTowardsAim();
    }
    // Fallback: Face movement if no aim target
    else if (movementInput.magnitude > 0.1f)
    {
        Vector3 worldMovement = GetWorldSpaceMovement(movementInput.normalized);
        RotateTowardsMovement(worldMovement);
    }
}
```

### Calculate Relative Movement for Blend Tree:

```csharp
private Vector2 GetRelativeMovementInput()
{
    // Get movement input relative to character forward
    Vector3 moveWorld = GetWorldSpaceMovement(movementInput);
    
    // Convert to local space
    Vector3 moveLocal = transform.InverseTransformDirection(moveWorld);
    
    return new Vector2(moveLocal.x, moveLocal.z);
}
```

---

## ?? **Quick Setup Checklist**

### Animator Setup:
```
? Add DirectionX (Float) parameter
? Add DirectionY (Float) parameter
? Create Movement blend tree (2D Freeform Directional)
? Add 8 directional run animations
? Position in 2D space (see above)
? Test in Animator preview
```

### Code Update:
```
? Update UpdateAnimations() to set DirectionX/Y
? Update HandleRotation() to face aim target
? Calculate relative movement direction
? Test in Play Mode
```

### Animation Import:
```
? Get 8 directional run animations (or use mirroring)
? Import with Humanoid rig
? Set Loop Time: ?
? Bake all Root Transforms
? Apply
```

---

## ?? **Temporary Solution (Without Blend Tree)**

### If you only have ONE run animation:

**Use rotation + single animation:**
```csharp
private void HandleRotation()
{
    // Face aim target
    if (aimTarget != null)
    {
        RotateTowardsAim();
    }
    
    // Body faces aim, animation always plays forward run
    // Not ideal but works until you get directional animations
}
```

**Limitation:**
- Character always uses forward run animation
- Looks like moonwalking when moving backward
- No proper strafe animations

---

## ?? **Mixamo Directional Animations**

### Free Animations to Download:

**Search on Mixamo:**
```
"Run Forward" or "Running"
"Run Backward" or "Running Backwards"
"Strafe Left" or "Running Strafe Left"
"Strafe Right" or "Running Strafe Right"
```

**Download Settings:**
```
Format: FBX for Unity
Skin: Without Skin
Frame Rate: 30
```

**Import to Unity:**
```
All animations:
- Animation Type: Humanoid
- Avatar: Copy from your character
- Loop Time: ?
- Bake Root Transforms: ?
```

---

## ?? **Complete Implementation**

### 1. Add Animation Hashes:

```csharp
// In PlayerMovement.cs class variables
private static readonly int DirectionXHash = Animator.StringToHash("DirectionX");
private static readonly int DirectionYHash = Animator.StringToHash("DirectionY");
```

### 2. Update UpdateAnimations():

```csharp
private void UpdateAnimations()
{
    if (animator == null || animator.runtimeAnimatorController == null) return;

    bool isMoving = movementInput.magnitude > 0.1f && !isDodging && !isDashing;
    
    if (isMoving)
    {
        // Get movement relative to character forward
        Vector3 worldMove = GetWorldSpaceMovement(movementInput.normalized);
        Vector3 localMove = transform.InverseTransformDirection(worldMove);
        
        // Set blend tree parameters
        animator.SetFloat(DirectionXHash, localMove.x);
        animator.SetFloat(DirectionYHash, localMove.z);
        
        // Speed
        float targetSpeed = movementInput.magnitude;
        if (isSprinting) targetSpeed *= sprintSpeedMultiplier;
        
        currentAnimationSpeed = Mathf.Lerp(currentAnimationSpeed, targetSpeed, Time.deltaTime / animationSmoothTime);
        animator.SetFloat(SpeedHash, currentAnimationSpeed);
        animator.SetBool(IsMovingHash, true);
    }
    else
    {
        animator.SetFloat(DirectionXHash, 0);
        animator.SetFloat(DirectionYHash, 0);
        currentAnimationSpeed = Mathf.Lerp(currentAnimationSpeed, 0, Time.deltaTime / animationSmoothTime);
        animator.SetFloat(SpeedHash, 0);
        animator.SetBool(IsMovingHash, false);
    }

    UpdateHealthAnimations();
}
```

### 3. Update HandleRotation():

```csharp
private void HandleRotation()
{
    // Always face aim target when aiming
    // Blend tree handles directional animations
    if (aimTarget != null)
    {
        RotateTowardsAim();
    }
}
```

---

## Summary

**Your Problem:**
```
Body rotation not showing proper directional movement
Can't strafe while aiming
Need separation between body movement and aim direction
```

**Solution: 2D Blend Tree**
```
8 directional run animations
DirectionX and DirectionY parameters
Character faces aim target
Blend tree shows correct movement animation
Perfect for action combat!
```

**Steps:**
1. Add DirectionX and DirectionY parameters
2. Create 2D Blend Tree with 8 directions
3. Update code to set parameters based on local movement
4. Character faces aim, animations show movement direction

**Result:**
- ? Run forward while aiming backward
- ? Strafe left/right with proper animations
- ? Smooth diagonal movement
- ? Independent body and aim control
- ? Professional third-person shooter feel

**Your character will move and aim independently!** ???
