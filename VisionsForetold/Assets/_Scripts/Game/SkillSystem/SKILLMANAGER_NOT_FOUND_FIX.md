# SkillManager Not Found - Troubleshooting Guide

## ?? Error: "SkillManager not found!"

**Error Message in Console:**
```
[SkillTreeUI] SkillManager not found!
```

---

## ? QUICK FIX CHECKLIST

### **Fix 1: Verify SkillManager is in Scene**

**Step 1: Check Hierarchy**
```
1. Open your scene (the one with Skills Panel)
2. Look in Hierarchy window
3. Search for "SkillManager"
```

**Should see:**
```
Hierarchy
?? SkillManager ? Must exist!
?? Player
?? Canvas
?? ... (other objects)
```

**If SkillManager is missing:**
```
1. Hierarchy ? Right-click
2. Create Empty
3. Name it "SkillManager"
4. Inspector ? Add Component
5. Search: "SkillManager"
6. Add the component
```

---

### **Fix 2: Ensure SkillManager is Active**

**Step 1: Select SkillManager**
```
Hierarchy ? Click SkillManager
```

**Step 2: Check if Active**
```
Inspector ? Top-left checkbox should be CHECKED ?
```

**If unchecked:**
- Click the checkbox to enable it
- GameObject must be active for Instance to work

---

### **Fix 3: Check Script Execution Order (Advanced)**

Sometimes SkillTreeUI's `OnEnable()` runs before SkillManager's `Awake()`.

**Fix applied in code:**
```csharp
// SkillTreeUI now tries to find SkillManager if Instance is null
if (skillManager == null)
{
    skillManager = FindObjectOfType<SkillManager>();
}
```

**This is automatic - no action needed!**

---

### **Fix 4: Verify SkillManager Script**

**Step 1: Select SkillManager**
```
Hierarchy ? SkillManager
```

**Step 2: Check Inspector**
```
Should see:
- Transform
- Skill Manager (Script) ? Must be here!

If script is missing or says "Missing Script":
- Remove component
- Add Component ? Search "SkillManager"
- Add it again
```

---

### **Fix 5: Don't Destroy On Load Issue**

If SkillManager has "Don't Destroy On Load" checked, it might be in a different scene.

**Step 1: Check Current Scene**
```
1. Press Play
2. Hierarchy ? Look for "DontDestroyOnLoad" special area
3. Expand it
4. Look for SkillManager there
```

**If SkillManager is in DontDestroyOnLoad:**
- This is fine!
- Updated code will find it automatically

**If you want it in current scene:**
```
1. SkillManager ? Inspector
2. Find "Don't Destroy On Load" checkbox
3. Uncheck it
4. SkillManager will stay in current scene
```

---

## ?? DIAGNOSTIC PROCEDURE

### **Test 1: Check Console Messages**

**Updated SkillTreeUI provides detailed messages:**

**If you see:**
```
[SkillTreeUI] SkillManager not found on Enable, trying to find it...
[SkillTreeUI] Found SkillManager via FindObjectOfType
```
? **Working!** SkillManager was found.

**If you see:**
```
[SkillTreeUI] SkillManager not found in scene! Please add SkillManager to the scene.
```
? **Problem!** SkillManager doesn't exist in scene.

---

### **Test 2: Manual Search**

**In Play Mode:**
```
1. Press Play
2. Top menu ? Window ? General ? Console
3. Look for SkillManager initialization message:
   "[SkillManager] Initialized with 13 skills"
```

**If you see this message:**
? SkillManager is working

**If you don't see this message:**
? SkillManager never initialized
- Check if GameObject is active
- Check if script component is attached

---

### **Test 3: Inspector Reference**

**While in Play Mode:**
```
1. Hierarchy ? Find any GameObject
2. Inspector ? Add Component (temporary)
3. Add any script
4. Add this public field to script:

public SkillManager testRef;

5. Drag SkillManager from Hierarchy to this field
6. If it works ? SkillManager exists
7. If it doesn't work ? SkillManager missing
```

---

## ??? COMMON SCENARIOS

### **Scenario A: SkillManager in Different Scene**

**Problem:**
- SkillManager is in Scene A
- Skills Panel is in Scene B
- When Scene B loads, SkillManager is missing

**Solutions:**

**Option 1: DontDestroyOnLoad (Recommended)**
```
SkillManager GameObject
Inspector ? Add Component ? Add this script:

using UnityEngine;

public class DontDestroyOnLoadHelper : MonoBehaviour
{
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }
}
```

**Option 2: SkillManager in Every Scene**
```
- Add SkillManager GameObject to every scene
- Singleton pattern will destroy duplicates automatically
```

**Option 3: Additive Scene Loading**
```
- Load SkillManager scene first
- Load other scenes additively
- SkillManager persists across all scenes
```

---

### **Scenario B: Script Missing After Unity Restart**

**Problem:**
- SkillManager was working
- Closed Unity
- Reopened Unity
- Now script is missing

**Fix:**
```
1. Check for compilation errors in Console
2. Assets ? Reimport All
3. Reattach SkillManager script to GameObject
```

---

### **Scenario C: Multiple SkillManagers**

**Problem:**
- Multiple SkillManager GameObjects in scene
- Singleton destroys duplicates
- Wrong one destroyed

**Fix:**
```
1. Search Hierarchy for "SkillManager"
2. Delete all duplicates
3. Keep only ONE SkillManager GameObject
```

---

## ?? CODE IMPROVEMENTS APPLIED

### **Better Error Handling in SkillTreeUI**

**Old Code:**
```csharp
skillManager = SkillManager.Instance;
if (skillManager == null)
{
    Debug.LogError("[SkillTreeUI] SkillManager not found!");
    return; // Fails immediately
}
```

**New Code:**
```csharp
skillManager = SkillManager.Instance;

if (skillManager == null)
{
    Debug.LogWarning("[SkillTreeUI] SkillManager not found on Enable, trying to find it...");
    // Try to find it in the scene
    skillManager = FindObjectOfType<SkillManager>();
    
    if (skillManager == null)
    {
        Debug.LogError("[SkillTreeUI] SkillManager not found in scene! Please add SkillManager to the scene.");
        return;
    }
    else
    {
        Debug.Log("[SkillTreeUI] Found SkillManager via FindObjectOfType");
    }
}
```

**Benefits:**
- More informative error messages
- Attempts fallback search with FindObjectOfType
- Tells you exactly what to do if missing

---

### **Safety Checks in All Methods**

Added null checks to prevent cascading errors:

```csharp
public void RefreshUI()
{
    if (skillManager == null)
    {
        Debug.LogWarning("[SkillTreeUI] Cannot refresh UI - SkillManager is null");
        return;
    }
    // ... rest of code
}
```

**Why:**
- Prevents NullReferenceException spam
- Clear message about the root cause
- UI doesn't break if SkillManager missing temporarily

---

## ?? SETUP VERIFICATION CHECKLIST

Before running, verify all these:

```
Scene Setup:
? SkillManager GameObject exists in Hierarchy
? SkillManager GameObject is active (checkbox checked)
? SkillManager has SkillManager component attached
? SkillManager appears in scene (not hidden)

Component Setup:
? SkillTreeUI attached to Skills Panel GameObject
? SkillTreeUI has all UI references assigned
? Skill Button Prefab is assigned

Play Mode Check:
? Console shows: "[SkillManager] Initialized with 13 skills"
? Console shows: "[SkillTreeUI] Found SkillManager..." or no error
? No red error messages in Console
```

---

## ?? EMERGENCY FIX

If nothing works, create SkillManager from scratch:

### **Step 1: Delete Old SkillManager**
```
Hierarchy ? Search "SkillManager"
Select all results ? Delete
```

### **Step 2: Create New SkillManager**
```
Hierarchy ? Right-click ? Create Empty
Name: "SkillManager"
```

### **Step 3: Add Script**
```
Inspector ? Add Component
Search: "SkillManager"
Select: VisionsForetold.Game.SkillSystem.SkillManager
Add it
```

### **Step 4: Configure**
```
Skill Manager (Script) component:
?? Base Experience Per Level: 100
?? Experience Scaling: 1.2
?? Skill Points Per Level: 1
?? Show Debug Info: ? (optional)
```

### **Step 5: Test**
```
Press Play
Check Console for:
"[SkillManager] Initialized with 13 skills"
```

---

## ? SUCCESS INDICATORS

**You've fixed it when:**

? Console shows: "[SkillManager] Initialized with 13 skills"
? Console shows: "[SkillTreeUI] Showing all skills: 13 found"
? No error about SkillManager not found
? Skills panel opens without errors
? Can see skill buttons

---

## ? FAILURE INDICATORS

**Still broken if:**

? Console shows: "[SkillTreeUI] SkillManager not found in scene!"
? NullReferenceException errors
? Skills panel is empty
? No initialization messages in Console

---

## ?? LAST RESORT

**If you've tried everything:**

1. **Take a screenshot of:**
   - Hierarchy with SkillManager visible
   - SkillManager Inspector showing components
   - Console showing all error messages

2. **Check that:**
   - SkillManager.cs file exists in Assets/_Scripts/Game/SkillSystem/
   - No compilation errors
   - Unity version is correct

3. **Try:**
   - Restart Unity Editor
   - Assets ? Reimport All
   - Clean solution and rebuild

4. **Verify Scene:**
   - File ? Build Settings
   - Is your scene added to build?
   - Is SkillManager in the right scene?

---

## ?? SUMMARY

**Most Common Causes:**

1. **SkillManager GameObject doesn't exist** (90%)
   - Fix: Add it to scene

2. **SkillManager GameObject is disabled** (5%)
   - Fix: Enable it

3. **Script execution order issue** (4%)
   - Fix: Code now handles this automatically

4. **SkillManager in different scene** (1%)
   - Fix: Use DontDestroyOnLoad

**Quick Fix:**
```
1. Add SkillManager GameObject to scene
2. Attach SkillManager script component
3. Make sure GameObject is active
4. Press Play
```

That's it! The improved code will handle the rest. ??
