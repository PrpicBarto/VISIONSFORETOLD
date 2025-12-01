# Only Seeing One Skill in List - Troubleshooting Guide

## ?? Problem

Only one skill button is visible in the skill list area, even though multiple skills should be displayed.

---

## ?? Most Likely Causes

### **Cause 1: Skills Are Stacking (All in Same Position)**
All skill buttons are spawning at the same position, so you only see the top one.

### **Cause 2: Content Size Not Growing**
Content GameObject isn't expanding in height, so only the first button fits.

### **Cause 3: Viewport Mask Is Too Small**
The visible area (viewport) is limited to one button's height.

### **Cause 4: Skills Are Actually Not Spawning**
Only one skill is actually being created by the code.

---

## ?? Quick Diagnosis

### **Step 1: Check if Skills Are Actually Spawning**

**While in Play Mode:**

```
1. Press Play
2. Open Skills Panel
3. Look at Hierarchy window
4. Expand "Content" GameObject
```

**What you should see:**
```
Content
?? SkillButton(Clone)  ? First skill
?? SkillButton(Clone)  ? Second skill
?? SkillButton(Clone)  ? Third skill
?? SkillButton(Clone)  ? Fourth skill
?? ... (more skills)
```

**Problem Indicators:**

? **Only ONE SkillButton(Clone)** = Skills not spawning (code issue)
? **Multiple SkillButton(Clone) but all at Y=0** = Stacking issue (layout issue)
? **Multiple SkillButton(Clone) at different Y positions** = Working correctly

---

## ? Solution 1: Fix Content Size Fitter (Most Common)

### **The Issue:**
Content GameObject doesn't have Content Size Fitter, so it stays at a fixed small height (only one button visible).

### **The Fix:**

**Step 1: Select Content GameObject**
```
Hierarchy ? SkillList ? Viewport ? Content
```

**Step 2: Check for Content Size Fitter**
```
Inspector ? Look for "Content Size Fitter" component
```

**Step 3: Add if Missing**
```
Inspector ? Add Component
Search: "Content Size Fitter"
Click to add
```

**Step 4: Configure**
```
Content Size Fitter:
?? Horizontal Fit: Unconstrained
?? Vertical Fit: Preferred Size  ? MUST BE THIS!
```

**Why This Works:**
- Preferred Size makes Content height grow automatically based on children
- Without it, Content stays at initial small height
- Only first button fits in view

---

## ? Solution 2: Fix Vertical Layout Group Settings

### **The Issue:**
Vertical Layout Group is forcing all buttons to same height (0 or very small).

### **The Fix:**

**Step 1: Select Content GameObject**
```
Hierarchy ? Content
```

**Step 2: Find Vertical Layout Group**
```
Inspector ? Vertical Layout Group component
```

**Step 3: Check These Settings:**
```
Vertical Layout Group:

MUST BE OFF:
?? Control Child Size
?  ?? Width: Can be ON
?  ?? Height: ? MUST BE OFF  ? CRITICAL!
?? Child Force Expand
   ?? Width: Can be ON
   ?? Height: ? MUST BE OFF  ? CRITICAL!

RECOMMENDED:
?? Spacing: 15-20  ? Space between buttons
?? Child Alignment: Upper Center
```

**Why This Works:**
- If "Control Child Height" is ON, buttons collapse to 0 height
- If "Force Expand Height" is ON, buttons stretch incorrectly
- Spacing creates visible gaps between buttons

---

## ? Solution 3: Fix Button Height

### **The Issue:**
Skill buttons don't have a defined height, so they collapse.

### **The Fix:**

**Step 1: Find SkillButton Prefab**
```
Project window ? Search "SkillButton"
Double-click to open in Prefab Mode
```

**Step 2: Select Root GameObject**
```
Select the root "SkillButton" GameObject
```

**Step 3: Add Layout Element**
```
Inspector ? Add Component
Search: "Layout Element"

Settings:
?? Min Height: 60
?? Preferred Height: 80  ? SET THIS
```

**Alternative - Set Fixed Height:**
```
SkillButton ? Rect Transform
Set Height: 80 (fixed value)
```

**Step 4: Save Prefab**
```
File ? Save (or Ctrl+S)
Exit Prefab Mode
```

---

## ? Solution 4: Fix Content Rect Transform

### **The Issue:**
Content is anchored incorrectly and doesn't expand.

### **The Fix:**

**Step 1: Select Content**
```
Hierarchy ? Content
```

**Step 2: Set Anchor Preset**
```
Inspector ? Rect Transform
Click anchor preset button (square icon top-left)
Hold Alt+Shift
Click: Top-Stretch preset (top middle with horizontal arrows)
```

**Step 3: Set Position Values**
```
Rect Transform:
?? Left: 0
?? Right: 0
?? Top: 0
?? Height: Will auto-adjust with Content Size Fitter
```

---

## ?? Detailed Diagnosis Procedure

### **Test 1: Check Button Count in Hierarchy**

**In Play Mode:**
1. Open Skills Panel
2. Hierarchy ? Expand Content
3. Count SkillButton(Clone) children

**Results:**
- **1 child** = Only one skill spawning ? Check SkillTreeUI code/filter
- **Multiple children** = Skills spawning but layout broken ? Check layout settings

---

### **Test 2: Check Button Positions**

**In Play Mode with skills panel open:**
1. Select first SkillButton(Clone) in Hierarchy
2. Inspector ? Rect Transform ? Look at "Pos Y"
3. Note the Y value (e.g., Y = 0)
4. Select second SkillButton(Clone)
5. Look at Pos Y

**Results:**
- **Same Y position** = Stacking issue ? Fix Vertical Layout Group
- **Different Y positions** (e.g., 0, -95, -190) = Working correctly ? Check viewport size

---

### **Test 3: Check Content Height**

**In Play Mode with skills panel open:**
1. Select Content in Hierarchy
2. Inspector ? Rect Transform
3. Look at Height value

**Results:**
- **Height = 0 or very small** = Content Size Fitter missing/wrong
- **Height = 400+** = Content growing correctly ? Check viewport size
- **Height = 80** = Only room for one button ? Content Size Fitter issue

---

### **Test 4: Check Scroll Functionality**

**In Play Mode:**
1. Open Skills Panel
2. Mouse over skill list area
3. Try scrolling with mouse wheel

**Results:**
- **Can scroll, see more skills** = Viewport too small, but skills exist
- **Nothing happens** = Content not expanding OR only one skill exists
- **Error in console** = Script issue

---

## ?? Common Scenarios & Fixes

### **Scenario A: Only Combat Skills Showing (1 skill)**

**Cause:** Filter is set to Combat, but only 1 Combat skill unlocked/exists at player level

**Check:**
```csharp
// In SkillTreeUI.cs
private SkillCategory currentFilter = SkillCategory.Combat;
```

**Fix:**
- Click "All Skills" button to see all skills
- Or change default filter to show all:
```csharp
private SkillCategory? currentFilter = null; // null = show all
```

---

### **Scenario B: Buttons Stack on Top of Each Other**

**Cause:** Vertical Layout Group not working or Content Size Fitter missing

**Fix:**
1. Add Content Size Fitter to Content
2. Set Vertical Fit = Preferred Size
3. In Vertical Layout Group: Control Child Height = OFF

See: `LAYOUT_QUICK_FIX.md` for complete guide

---

### **Scenario C: Scroll View Shows Empty Space**

**Cause:** Content expanding but buttons not visible

**Fix:**
1. Check Viewport has Rect Mask 2D component
2. Check buttons have Image component (to be visible)
3. Check button colors aren't transparent

---

### **Scenario D: Only First Skill Visible, Rest Cut Off**

**Cause:** Viewport too small or Content not scrolling

**Fix:**
1. Select Scroll View parent
2. Check Scroll Rect ? Content field points to Content
3. Check Scroll Rect ? Vertical = Checked
4. Increase Viewport height in Rect Transform

---

## ??? Complete Fix Procedure (Do All Steps)

### **Step 1: Fix Content GameObject**

```
Select: Hierarchy ? Content

Components needed:
? Rect Transform
? Vertical Layout Group
? Content Size Fitter

Settings:
Content Size Fitter:
?? Vertical Fit: Preferred Size

Vertical Layout Group:
?? Spacing: 15
?? Control Child Height: OFF
?? Force Expand Height: OFF

Rect Transform:
?? Anchor: Top-Stretch
?? Left: 0
?? Right: 0
?? Top: 0
```

---

### **Step 2: Fix SkillButton Prefab**

```
Open SkillButton prefab
Select root GameObject

Add Layout Element:
?? Preferred Height: 80

OR set Rect Transform:
?? Height: 80 (fixed)

Save prefab
```

---

### **Step 3: Verify Scroll View Setup**

```
Select: Scroll View parent GameObject

Check Scroll Rect component:
?? Content: Points to Content GameObject ?
?? Viewport: Points to Viewport GameObject ?
?? Horizontal: Unchecked ?
?? Vertical: Checked ?
```

---

### **Step 4: Verify SkillTreeUI Assignment**

```
Select: GameObject with SkillTreeUI component

Check Inspector:
?? Skill List Container: Points to Content GameObject ?
?? Skill Button Prefab: Assigned ?
```

---

### **Step 5: Test Filter Buttons**

```
Press Play
Open Skills Panel
Click "All Skills" button
Should see all 13 skills in different categories
```

---

## ?? Testing Checklist

After applying fixes:

- [ ] Press Play
- [ ] Open Skills Panel
- [ ] Click "All Skills" filter button
- [ ] Check Hierarchy ? Content has multiple SkillButton(Clone) children
- [ ] Each SkillButton at different Y position
- [ ] Can scroll through skill list
- [ ] Can see multiple skills in viewport
- [ ] Content height is large (400+)
- [ ] Clicking different category filters shows different skills

---

## ?? Unity 6 Specific Debug

### **Add Debug Script to Content:**

Create temporary debugging script:

```csharp
using UnityEngine;

public class DebugSkillList : MonoBehaviour
{
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            var rect = GetComponent<RectTransform>();
            int childCount = transform.childCount;
            
            Debug.Log($"=== SKILL LIST DEBUG ===");
            Debug.Log($"Content Height: {rect.rect.height}");
            Debug.Log($"Children Count: {childCount}");
            
            if (childCount == 0)
            {
                Debug.LogWarning("NO SKILLS SPAWNED!");
            }
            else if (childCount == 1)
            {
                Debug.LogWarning("Only 1 skill spawned - check filter or skill manager");
            }
            else
            {
                Debug.Log($"? {childCount} skills spawned");
                
                // Check positions
                for (int i = 0; i < Mathf.Min(childCount, 5); i++)
                {
                    var child = transform.GetChild(i).GetComponent<RectTransform>();
                    Debug.Log($"  Skill {i}: Y = {child.anchoredPosition.y}");
                }
            }
        }
    }
}
```

**Usage:**
1. Attach to Content GameObject
2. Press Play
3. Open Skills Panel
4. Press F1
5. Check Console for debug info

---

## ?? Summary

**Most Common Fix:**

1. **Add Content Size Fitter to Content**
   - Set Vertical Fit = Preferred Size

2. **Fix Vertical Layout Group on Content**
   - Control Child Height = OFF
   - Force Expand Height = OFF
   - Spacing = 15+

3. **Add Layout Element to SkillButton Prefab**
   - Preferred Height = 80

4. **Click "All Skills" filter button**
   - Default filter might be showing category with few skills

**Quick Test:**
```
Hierarchy (Play Mode) ? Content ? Should have 13 children
If not ? Skills not spawning (check SkillManager)
If yes ? Check Y positions of each child
  ? All same Y = Layout issue (apply fixes above)
  ? Different Y = Viewport size issue
```

---

## ?? Still Only Seeing One Skill?

**Last Resort Checklist:**

1. **Check SkillManager exists in scene**
2. **Check SkillManager has 13 skills registered**
3. **Right-click SkillManager ? Print Skill Stats**
4. **Check Console for skill count**
5. **Try clicking each category filter button**
6. **Check if "All Skills" button exists and works**
7. **Verify `currentFilter` in SkillTreeUI.cs**

**If still broken:**
- See `LAYOUT_QUICK_FIX.md`
- See `LAYOUT_FIX_GUIDE.md`
- Post error messages from Console

The issue is almost certainly one of the layout settings above! ??
