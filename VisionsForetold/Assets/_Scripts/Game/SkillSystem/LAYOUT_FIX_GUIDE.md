# Vertical Layout Group - Stacking Issue Fix Guide

## ?? Problem

**Skill buttons stack on top of each other instead of listing vertically in the scroll view.**

---

## ? Complete Fix Checklist

### **1. Content Size Fitter (CRITICAL)**

**Location:** Content GameObject (inside ScrollView)

**Required Component:** `Content Size Fitter`

**Settings:**
```
Content Size Fitter:
?? Horizontal Fit: Unconstrained
?? Vertical Fit: Preferred Size  ? MUST BE SET
```

**How to Add:**
1. Select Content GameObject
2. Add Component ? Layout ? Content Size Fitter
3. Set Vertical Fit to "Preferred Size"

**Why:** This makes the content height automatically adjust based on children. Without it, content stays at fixed height and buttons overlap.

---

### **2. Vertical Layout Group Settings**

**Location:** Content GameObject (inside ScrollView)

**Component:** `Vertical Layout Group`

**Correct Settings:**
```
Vertical Layout Group:
?? Padding
?  ?? Left: 10
?  ?? Right: 10
?  ?? Top: 10
?  ?? Bottom: 10
?? Spacing: 15-20 (space between buttons)
?? Child Alignment: Upper Center
?? Control Child Size
?  ?? Width: ? Checked
?  ?? Height: ? UNCHECKED ? IMPORTANT
?? Use Child Scale: ? Unchecked
?? Child Force Expand
   ?? Width: ? Checked
   ?? Height: ? UNCHECKED ? IMPORTANT
```

**Critical Settings:**
- ? **Control Child Size ? Height: OFF**
- ? **Child Force Expand ? Height: OFF**
- ? **Spacing: 15-20** (prevents overlap)

**Why:** If you control height or force expand height, buttons might collapse or stretch incorrectly.

---

### **3. Content Rect Transform**

**Location:** Content GameObject

**Settings:**
```
Rect Transform:
?? Anchor Preset: Top-Stretch
?  (Click top-middle preset with Alt)
?? Pivot: X: 0.5, Y: 1
?? Left: 0
?? Right: 0
?? Top: 0
?? Height: Auto (controlled by Content Size Fitter)
```

**Visual:**
```
???????????????????????
? ? Content anchored here
? Top edge stretched
?
? Height auto-grows
? as buttons added
?
???????????????????????
```

---

### **4. Skill Button Prefab Setup**

**Location:** SkillButton Prefab

**Required:** Fixed height for each button

#### Option A: Using Layout Element (Recommended)

1. **Select SkillButton Prefab**
2. **Add Component ? Layout Element**
3. **Settings:**
   ```
   Layout Element:
   ?? Ignore Layout: ? Unchecked
   ?? Min Width: -1
   ?? Min Height: 60
   ?? Preferred Width: -1
   ?? Preferred Height: 80  ? Set desired button height
   ?? Flexible Width: -1
   ?? Flexible Height: -1
   ```

#### Option B: Using Rect Transform

1. **Select SkillButton Prefab**
2. **Rect Transform:**
   ```
   Width: Flexible
   Height: 80 (FIXED)
   ```

**Why:** Buttons need a defined height. Without it, they can collapse to 0 height and stack.

---

### **5. Scroll View Setup**

**Location:** Scroll View GameObject

**Required Components:**
- Scroll Rect
- Mask (or Rect Mask 2D)
- Image (for background)

**Scroll Rect Settings:**
```
Scroll Rect:
?? Content: [Drag Content GameObject here]
?? Horizontal: ? Unchecked
?? Vertical: ? Checked
?? Movement Type: Elastic (or Clamped)
?? Elasticity: 0.1
?? Inertia: ? Checked
?? Scroll Sensitivity: 20-30
?? Viewport: [Drag Viewport GameObject]
```

---

### **6. Code Fix (Already Applied)**

The `SkillTreeUI.cs` now includes layout rebuild to prevent stacking:

```csharp
private void RefreshSkillList()
{
    // Clear existing buttons
    foreach (var skillButton in skillButtons)
    {
        if (skillButton.gameObject != null)
            Destroy(skillButton.gameObject);
    }
    skillButtons.Clear();

    // Create new buttons...
    
    // Force rebuild layout to prevent stacking issues
    StartCoroutine(RebuildLayoutNextFrame());
}

private System.Collections.IEnumerator RebuildLayoutNextFrame()
{
    yield return null; // Wait one frame
    
    if (skillListContainer != null)
    {
        LayoutRebuilder.ForceRebuildLayoutImmediate(skillListContainer as RectTransform);
    }
}
```

**Why:** Unity sometimes doesn't update layouts immediately when spawning objects. This forces a rebuild.

---

## ?? Hierarchy Structure

Correct hierarchy for scroll view:

```
SkillsPanel
?? SkillList (Scroll View)
   ?? Viewport
   ?  ?? Content  ? This is skillListContainer
   ?     ?? SkillButton (Clone)
   ?     ?? SkillButton (Clone)
   ?     ?? SkillButton (Clone)
   ?     ?? ... (more buttons)
   ?? Scrollbar Vertical (optional)
```

**Content GameObject Components:**
- ? Rect Transform
- ? Vertical Layout Group
- ? Content Size Fitter
- ? NO Image (unless you want background)

---

## ?? Quick Diagnostic

### Test 1: Check Content Height

1. Press Play
2. Select Content GameObject
3. Inspector ? Rect Transform ? Height
4. **Expected:** Height should be larger than viewport (e.g., 500+ if you have 5+ buttons)
5. **Problem:** Height = 0 or very small = **Content Size Fitter missing/wrong**

### Test 2: Check Button Positions

1. Press Play
2. Hierarchy ? Expand Content
3. Select first SkillButton
4. Inspector ? Rect Transform ? Pos Y
5. Select second SkillButton ? Pos Y should be different
6. **Problem:** All buttons have same Pos Y = **Layout Group not working**

### Test 3: Spacing Test

1. In Vertical Layout Group, set Spacing to 100
2. Press Play
3. **Expected:** Large gaps between buttons
4. **Problem:** No gaps = **Layout Group not active**

---

## ??? Step-by-Step Fix Procedure

### If buttons are stacking:

1. **Select Content GameObject**
   
2. **Check Components:**
   ```
   ? Rect Transform
   ? Vertical Layout Group
   ? Content Size Fitter  ? Add if missing
   ```

3. **Configure Content Size Fitter:**
   ```
   Horizontal Fit: Unconstrained
   Vertical Fit: Preferred Size
   ```

4. **Configure Vertical Layout Group:**
   ```
   Spacing: 15
   Control Child Size ? Height: OFF
   Child Force Expand ? Height: OFF
   ```

5. **Select SkillButton Prefab**

6. **Add Layout Element Component:**
   ```
   Preferred Height: 80
   ```

7. **Test in Play Mode**

8. **If still broken ? Continue to Advanced Fixes**

---

## ?? Advanced Fixes

### Fix A: Manually Rebuild Layout

Add this method to SkillTreeUI and call it after creating buttons:

```csharp
private void ForceLayoutRebuild()
{
    if (skillListContainer != null)
    {
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(skillListContainer as RectTransform);
    }
}
```

Call it in `RefreshSkillList()` after creating buttons.

### Fix B: Delay Layout Rebuild

```csharp
private IEnumerator RebuildLayoutDelayed()
{
    yield return new WaitForEndOfFrame();
    ForceLayoutRebuild();
}
```

### Fix C: Reset Content Position

Sometimes content gets offset. Add this:

```csharp
private void ResetContentPosition()
{
    if (skillListContainer != null)
    {
        RectTransform rt = skillListContainer as RectTransform;
        rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, 0);
    }
}
```

---

## ? Verification Checklist

After applying fixes, verify:

- [ ] Content GameObject has Content Size Fitter
- [ ] Vertical Fit set to "Preferred Size"
- [ ] Vertical Layout Group ? Control Child Height = OFF
- [ ] Vertical Layout Group ? Spacing > 10
- [ ] Each button has Layout Element OR fixed height
- [ ] Content height grows when buttons added
- [ ] Buttons are vertically spaced
- [ ] Can scroll through all buttons
- [ ] No overlap between buttons

---

## ?? Visual Examples

### ? WRONG - Stacking
```
[Button1]
[Button2]  ? All stacked
[Button3]  ? in same spot
[Button4]
```

### ? CORRECT - Listed
```
[Button 1]

[Button 2]

[Button 3]

[Button 4]
```

---

## ?? Common Mistakes

| Mistake | Result | Fix |
|---------|--------|-----|
| No Content Size Fitter | Content doesn't grow | Add component |
| Vertical Fit = Unconstrained | Content fixed height | Set to Preferred Size |
| Control Child Height = ON | Buttons collapse | Turn OFF |
| No spacing | Buttons touch | Set Spacing to 15-20 |
| No button height | Buttons = 0 height | Add Layout Element |
| Wrong anchor preset | Content doesn't stretch | Use Top-Stretch |

---

## ?? Quick Fix Summary

**Most Common Solution (90% of cases):**

1. Select Content GameObject
2. Add Component ? Content Size Fitter
3. Set Vertical Fit = Preferred Size
4. In Vertical Layout Group:
   - Set Spacing = 15
   - Control Child Height = OFF
5. Add Layout Element to SkillButton Prefab:
   - Preferred Height = 80

**Done!** Buttons should now list properly.

---

## ?? Still Not Working?

If buttons still stack after trying all fixes:

1. **Delete and recreate Content GameObject**
2. **Recreate Scroll View from scratch**
3. **Check for conflicting Layout Groups**
4. **Remove any extra Layout components**
5. **Ensure no custom scripts are modifying positions**

**Last Resort:**
- Export SkillButton prefab
- Delete entire Scroll View
- Recreate from Unity menu: Right-click ? UI ? Scroll View
- Re-add Vertical Layout Group
- Re-add Content Size Fitter
- Import SkillButton prefab back

---

## ?? Debug Logging

Add this to SkillTreeUI for diagnostics:

```csharp
private void DebugLayoutInfo()
{
    if (skillListContainer != null)
    {
        RectTransform rt = skillListContainer as RectTransform;
        Debug.Log($"Content Height: {rt.rect.height}");
        Debug.Log($"Content Children: {skillListContainer.childCount}");
        Debug.Log($"Content Position: {rt.anchoredPosition}");
    }
}
```

Call after creating buttons to see if content is resizing properly.

---

**The code fix has already been applied to SkillTreeUI.cs. Now just configure the UI components in Unity Editor following this guide!**
