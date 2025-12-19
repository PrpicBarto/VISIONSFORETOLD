# ??? Weapon Visual System - Setup Guide

## ? **System Added Successfully!**

Your player will now change weapons in their hand when switching attack modes!

---

## ?? **How It Works**

**When you switch modes (scroll wheel or gamepad):**
- **Melee Mode** ? Sword appears in hand
- **Ranged Mode** ? Bow appears in hand
- **Spell Mode** ? Staff appears in hand

**The system:**
1. Instantiates all weapon prefabs on game start
2. Attaches them to the player's hand bone
3. Shows/hides them based on current mode
4. Updates automatically when you scroll

---

## ?? **Setup Instructions**

### Step 1: Find Your Player's Hand Bone

**In Unity Hierarchy:**
```
1. Select your Player GameObject
2. Expand the Armature/Skeleton
3. Find the Right Hand bone (usually named):
   - RightHand
   - Right_Hand
   - Hand_R
   - mixamorig:RightHand (if from Mixamo)
```

---

### Step 2: Assign Components in Inspector

**Select your Player GameObject:**

**PlayerAttack Component ? Weapon Visual Settings:**
```
???????????????????????????????????????????
? Weapon Visual Settings                  ?
???????????????????????????????????????????
? Weapon Hand: [Drag RightHand here]     ?
? Sword Prefab: [Drag sword model]       ?
? Bow Prefab: [Drag bow model]           ?
? Staff Prefab: [Drag staff model]       ?
? Hide Weapon When Not In Use: ?         ?
???????????????????????????????????????????
```

**Weapon Hand:**
- Drag the RightHand bone from Hierarchy

**Sword Prefab:**
- Your sword 3D model (from Assets or Project)

**Bow Prefab:**
- Your bow 3D model

**Staff Prefab:**
- Your staff/wand 3D model

**Hide Weapon When Not In Use:**
- ? Unchecked: All weapons visible but overlapping
- ? Checked: Only active weapon visible (recommended)

---

### Step 3: Create/Import Weapon Models

**Option A: Use Placeholder Models (Quick Test)**

Create simple cube placeholders:
```
1. GameObject ? 3D Object ? Cube
2. Scale it to look like a sword (e.g., 0.1, 1, 0.1)
3. Name it "Sword_Placeholder"
4. Drag to Project to make prefab
5. Delete from scene
6. Repeat for Bow and Staff
```

**Option B: Import Real Models**

```
1. Import your weapon FBX files to Assets/Models/Weapons
2. Select each model in Project
3. Inspector ? Materials ? Extract Materials
4. Drag weapon prefabs to Inspector slots
```

---

### Step 4: Position Weapons Correctly

**After assigning prefabs, in Play Mode:**

```
1. Enter Play Mode
2. Select Player ? RightHand in Hierarchy
3. You'll see weapon instances created:
   - Sword_Instance
   - Bow_Instance
   - Staff_Instance
4. Note their positions/rotations
5. Exit Play Mode
```

**To fix weapon positions:**

**Method 1: Add Weapon Position Script (Recommended)**

Create a simple script on each weapon prefab:
```csharp
public class WeaponPosition : MonoBehaviour
{
    public Vector3 localPosition = Vector3.zero;
    public Vector3 localRotation = Vector3.zero;
    
    void Start()
    {
        transform.localPosition = localPosition;
        transform.localEulerAngles = localRotation;
    }
}
```

Then set position/rotation in Inspector on each prefab.

**Method 2: Modify PlayerAttack Initialization**

Or ask me to add weapon position/rotation settings to PlayerAttack!

---

## ?? **Testing**

**1. Assign all components**
```
- Weapon Hand: RightHand bone
- Prefabs: Sword, Bow, Staff
- Hide Weapon: Checked
```

**2. Enter Play Mode**
```
- Check Console: "Weapons initialized on RightHand"
```

**3. Scroll to change modes**
```
- Watch the weapon in player's hand change
- Console: "Weapon visual updated to [Mode] mode"
```

**4. Check each mode:**
```
- Melee: Sword visible
- Ranged: Bow visible
- Spell: Staff visible
```

---

## ?? **Advanced Options**

### Option 1: Keep All Weapons Visible

**If you want all weapons visible (like on a belt):**
```
Inspector ? Hide Weapon When Not In Use: ? (unchecked)

Result: All weapons show, but only active one is "highlighted"
```

### Option 2: Multiple Weapon Positions

**For different attachment points:**
```
Create:
- SwordSheath (on back)
- BowHolder (on back)
- StaffHolder (on back)

Modify InitializeWeapons() to parent each weapon to its holder
Switch visibility or positions when active
```

### Option 3: Weapon Swap Animation

**Add smooth transitions:**
```csharp
// In SetActiveWeapon, add:
StartCoroutine(SmoothWeaponSwap(activeWeapon));

IEnumerator SmoothWeaponSwap(GameObject newWeapon)
{
    // Fade out current
    // Fade in new
    // Or quick equip animation
}
```

---

## ?? **Weapon Placement Tips**

### Common Hand Bone Names:
```
Mixamo: mixamorig:RightHand
Generic: RightHand, Right_Hand, Hand_R
Custom: Check your model's skeleton
```

### Typical Weapon Rotations:
```
Sword:
- Position: (0, 0, 0)
- Rotation: (0, 90, 0)

Bow:
- Position: (0, 0, 0)
- Rotation: (-90, 0, 0)

Staff:
- Position: (0, 0, 0)
- Rotation: (0, 0, 90)
```

**Adjust these in Play Mode and note the values!**

---

## ?? **Public Methods Available**

**You can now call these from other scripts:**

```csharp
// Get current weapon instance
GameObject currentWeapon = playerAttack.GetCurrentWeapon();

// Get specific weapons
GameObject sword = playerAttack.GetSword();
GameObject bow = playerAttack.GetBow();
GameObject staff = playerAttack.GetStaff();

// Use cases:
// - Attach particle effects to weapon
// - Play weapon-specific sounds
// - Enable weapon trails during attacks
// - Weapon-specific animations
```

---

## ?? **Example Usage**

### Adding Weapon Trail Effect:

```csharp
public class WeaponTrailEffect : MonoBehaviour
{
    private PlayerAttack playerAttack;
    private TrailRenderer swordTrail;
    
    void Start()
    {
        playerAttack = GetComponent<PlayerAttack>();
        
        // Get sword and add trail
        GameObject sword = playerAttack.GetSword();
        if (sword != null)
        {
            swordTrail = sword.AddComponent<TrailRenderer>();
            swordTrail.time = 0.5f;
            // Configure trail...
        }
    }
    
    void OnAttack()
    {
        // Enable trail during attack
        if (swordTrail != null)
            swordTrail.emitting = true;
    }
}
```

---

## ?? **Troubleshooting**

### Weapons Not Appearing?
```
? Check Console for: "Weapon hand transform not assigned!"
? Make sure you assigned the hand bone
? Make sure prefabs are assigned
? Check prefabs are not null in Inspector
```

### Weapons in Wrong Position?
```
? Check weapon prefab pivot point
? Adjust localPosition in InitializeWeapons()
? Adjust localRotation in InitializeWeapons()
? Or add WeaponPosition script (see above)
```

### Multiple Weapons Visible?
```
? Set "Hide Weapon When Not In Use" to checked
? This ensures only active weapon shows
```

### Weapons Not Switching?
```
? Check Console for: "Weapon visual updated to X mode"
? If not appearing, UpdateWeaponVisuals() isn't being called
? Make sure you're scrolling to change modes
```

---

## ? **Summary**

**What Was Added:**

**New Fields:**
```csharp
[SerializeField] private Transform weaponHand;
[SerializeField] private GameObject swordPrefab;
[SerializeField] private GameObject bowPrefab;
[SerializeField] private GameObject staffPrefab;
[SerializeField] private bool hideWeaponWhenNotInUse;
```

**New Methods:**
```csharp
InitializeWeapons()        // Creates weapon instances
UpdateWeaponVisuals()      // Switches visible weapon
SetActiveWeapon()          // Shows/hides weapons
GetCurrentWeapon()         // Returns active weapon
GetSword/Bow/Staff()       // Returns specific weapons
```

**Integration:**
```
- Awake() calls InitializeWeapons()
- CycleAttackMode() calls UpdateWeaponVisuals()
- Automatic switching when changing modes
```

---

## ?? **Next Steps**

**1. Assign Components (Required)**
```
- Weapon Hand bone
- 3 weapon prefabs
```

**2. Test (2 minutes)**
```
Play Mode ? Scroll ? Watch weapons change
```

**3. Adjust Positions (Optional)**
```
Note positions in Play Mode
Add WeaponPosition script
Or ask me to add position settings
```

**4. Polish (Optional)**
```
- Add weapon trails
- Add equip sounds
- Add swap animations
- Add glow effects on active weapon
```

---

**Your weapons will now change when switching modes!** ??????

**Assign the components and test it!** ???

**The system is fully functional and ready to use!** ??
