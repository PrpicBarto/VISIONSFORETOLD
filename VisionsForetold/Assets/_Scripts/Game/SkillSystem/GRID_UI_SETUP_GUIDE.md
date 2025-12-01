# Grid-Based Skill Tree UI - Gamepad Friendly Setup Guide

## ?? Overview

This is an **alternative skill tree UI** designed specifically for gamepad/joystick support while still working great with keyboard and mouse.

**Key Features:**
- ? Grid layout with automatic navigation
- ? Full gamepad/joystick support
- ? Tab-based category filtering
- ? Side preview panel for skill details
- ? Visual selection indicators
- ? Keyboard shortcuts (Q/E for tabs)
- ? Responsive card-based design

---

## ?? Comparison: Grid vs List

| Feature | List UI (Original) | Grid UI (New) |
|---------|-------------------|---------------|
| Layout | Vertical scroll | Grid with tabs |
| Navigation | Mouse/Scroll | D-Pad/Analog |
| Preview | Separate panel | Side panel |
| Gamepad | Basic | Optimized |
| Categories | Filter buttons | Tab system |
| Selection | Click only | Button highlight |

---

## ?? Quick Setup (5 Minutes)

### **Step 1: Create Skill Card Prefab**

```
1. Create UI GameObject: "SkillCard"
2. Add Image component (background)
3. Add Button component
4. Add these children:

SkillCard (200x150)
?? SkillName (TMP_Text) - Top
?? SkillLevel (TMP_Text) - Top Right
?? Icon (Image) - Center
?? LockIcon (Image) - Optional overlay
?? SelectionBorder (Image) - Border highlight
```

**Prefab Structure:**
```
SkillCard
?? Rect Transform (200 x 150)
?? Image (Background)
?? Button
?? SkillName (TMP_Text)
?  ?? Position: Top center
?? SkillLevel (TMP_Text)
?  ?? Position: Top right
?? Icon (Image)
?  ?? Position: Center
?? LockIcon (Image - optional)
?  ?? Position: Bottom right corner
?? SelectionBorder (Image)
   ?? Position: Stretch, Border outline
```

### **Step 2: Setup Main Panel**

```
SkillsPanel
?? Header
?  ?? LevelText
?  ?? ExperienceText
?  ?? SkillPointsText
?  ?? ExperienceBar
?? CategoryTabs (Horizontal Layout)
?  ?? AllSkillsTab
?  ?? CombatTab
?  ?? MagicTab
?  ?? DefenseTab
?  ?? UtilityTab
?? SkillGridContainer (Grid Layout Group)
?  ?? [Cards spawn here]
?? PreviewPanel (Side panel)
   ?? PreviewName
   ?? PreviewIcon
   ?? PreviewDescription
   ?? PreviewLevel
   ?? PreviewCost
   ?? ActionButton (Unlock/Upgrade)
```

### **Step 3: Add Component**

```
Select SkillsPanel
Inspector ? Add Component
Search: "SkillTreeUI_GridBased"
Add it
```

### **Step 4: Assign References**

**In SkillTreeUI_GridBased Inspector:**

**UI References:**
- Skill Panel: SkillsPanel
- Skill Grid Container: SkillGridContainer
- Skill Card Prefab: SkillCard prefab

**Player Info:**
- Level Text: LevelText
- Experience Text: ExperienceText
- Skill Points Text: SkillPointsText
- Experience Bar: ExperienceBar slider

**Skill Preview Panel:**
- Preview Panel: PreviewPanel
- Preview Name Text: PreviewName
- Preview Description Text: PreviewDescription
- Preview Level Text: PreviewLevel
- Preview Cost Text: PreviewCost
- Preview Icon: PreviewIcon image
- Action Button: ActionButton
- Action Button Text: ActionButton text child

**Category Tabs:**
- All Skills Tab: AllSkillsTab button
- Combat Skills Tab: CombatTab button
- Magic Skills Tab: MagicTab button
- Defense Skills Tab: DefenseTab button
- Utility Skills Tab: UtilityTab button

**Grid Settings:**
- Grid Columns: 3 (or 2, 4 depending on screen)
- Card Spacing: 10
- Card Size: X: 200, Y: 150

**Colors:**
- Unlocked Color: Green (0.2, 0.8, 0.2)
- Locked Color: Gray (0.5, 0.5, 0.5)
- Can Unlock Color: Yellow (0.8, 0.8, 0.2)
- Selected Color: Blue (0.3, 0.6, 1.0)

---

## ?? Gamepad Controls

### **PlayStation Controller:**

| Button | Action |
|--------|--------|
| Left Stick / D-Pad | Navigate cards |
| X (Cross) | Select card / Perform action |
| Circle | Close preview |
| L1 | Previous category tab |
| R1 | Next category tab |

### **Xbox Controller:**

| Button | Action |
|--------|--------|
| Left Stick / D-Pad | Navigate cards |
| A | Select card / Perform action |
| B | Close preview |
| LB | Previous category tab |
| RB | Next category tab |

### **Keyboard:**

| Key | Action |
|-----|--------|
| Arrow Keys / WASD | Navigate cards |
| Enter / Space | Select card / Perform action |
| ESC | Close preview |
| Q | Previous category tab |
| E | Next category tab |

### **Mouse:**

| Action | Result |
|--------|--------|
| Click Card | Select and preview |
| Click Action Button | Unlock/Upgrade |
| Click Tab | Switch category |

---

## ?? Skill Card Design

### **Recommended Layout:**

```
????????????????????????
? Power Strike    Lv 3/5? ? Name + Level
?                      ?
?      [ICON]          ? ? Large centered icon
?                      ?
?                [??]  ? ? Lock icon (if locked)
????????????????????????
 ?                    ?
 Selection Border (blue when selected)
```

### **Visual States:**

**Unlocked Skill:**
```
Green background
No lock icon
Shows current/max level
Bright icon
```

**Can Unlock:**
```
Yellow background
Lock icon visible
Shows "Locked" text
Highlighted icon
```

**Locked:**
```
Gray background
Lock icon visible
Shows "Locked" text
Dimmed icon
```

**Selected:**
```
Blue border overlay
All other states remain
Preview panel shows
```

---

## ?? Grid Container Setup

### **Grid Layout Group Settings:**

```
Skill Grid Container
?? Grid Layout Group component

Settings:
?? Cell Size: 200 x 150
?? Spacing: 10 x 10
?? Start Corner: Upper Left
?? Start Axis: Horizontal
?? Child Alignment: Upper Center
?? Constraint: Fixed Column Count
?? Constraint Count: 3
```

### **Content Size Fitter:**

```
Add Component ? Content Size Fitter

Settings:
?? Horizontal Fit: Preferred Size
?? Vertical Fit: Preferred Size
```

### **Scroll View (Optional):**

If you want scrolling for many skills:

```
Put Grid Container inside Scroll View:
ScrollView
?? Viewport
   ?? Content (Grid Container)
```

---

## ?? Preview Panel Design

### **Recommended Layout:**

```
???? PREVIEW PANEL ????????
?                          ?
?    [LARGE ICON]          ?
?                          ?
?  Power Strike            ?
?  Level 3 / 5             ?
?                          ?
?  Increases attack damage ?
?                          ?
?  Effects:                ?
?  • DamageBoost: +15      ?
?  • CritChance: +5%       ?
?                          ?
?  Upgrade Cost: 2 SP      ?
?                          ?
?  [  UPGRADE  ]           ? ? Action button
?                          ?
????????????????????????????
```

### **Preview Panel Settings:**

```
Position: Right side of screen
Width: 300-400 pixels
Height: Full height or auto
Background: Semi-transparent dark

Components:
- Large icon (128x128 or bigger)
- Scrollable description area
- Action button at bottom
- Close button (optional)
```

---

## ?? Tab System

### **Tab Button Design:**

```
[  All  ] [Combat] [Magic] [Defense] [Utility]
   ? Selected (different color/underline)
```

### **Tab Settings:**

**Selected Tab:**
```
Background: Bright color
Text: White
Border: Bottom line
```

**Unselected Tab:**
```
Background: Darker
Text: Gray
Border: None
```

---

## ?? Advanced Customization

### **Change Grid Columns:**

For different screen sizes:

```csharp
// In Inspector:
Grid Columns: 2  // For small screens
Grid Columns: 3  // Default
Grid Columns: 4  // For wide screens
Grid Columns: 5  // For ultrawide
```

### **Card Size:**

```csharp
// Square cards
Card Size: 150 x 150

// Wide cards
Card Size: 250 x 150

// Tall cards
Card Size: 150 x 200
```

### **Navigation Delay:**

Adjust responsiveness:

```csharp
// In Inspector:
Navigation Delay: 0.1f  // Very responsive
Navigation Delay: 0.15f // Default
Navigation Delay: 0.2f  // Slower, more controlled
```

---

## ?? Input Mapping

### **Current Mapping:**

```csharp
Input.GetButtonDown("Previous") // L1/LB - Previous Tab
Input.GetButtonDown("Next")     // R1/RB - Next Tab
Input.GetButtonDown("Jump")     // B/Circle - Cancel
```

### **To Change Mapping:**

Edit in script or use Unity Input Manager:

```
Edit ? Project Settings ? Input Manager

Add buttons:
- Name: "SkillTabLeft"
  Joystick Button: 4 (L1/LB)
  
- Name: "SkillTabRight"
  Joystick Button: 5 (R1/RB)
```

---

## ? Performance Tips

### **Optimize for Many Skills:**

**1. Object Pooling:**
```csharp
// Instead of Destroy/Instantiate
// Reuse card objects
```

**2. Lazy Loading:**
```csharp
// Load icons on demand
// Don't load all at once
```

**3. Limit Grid Size:**
```csharp
// Show max 12-15 cards per page
// Use pagination for more
```

---

## ?? Troubleshooting

### **Cards Don't Navigate Properly:**

**Fix:**
```
1. Check Grid Layout Group is on container
2. Verify Button has Navigation = Automatic
3. Check EventSystem exists in scene
4. Ensure buttons are active and interactable
```

### **Preview Doesn't Show:**

**Fix:**
```
1. Check PreviewPanel is assigned
2. Verify all text references assigned
3. Check ActionButton is assigned
4. Look for null reference errors in Console
```

### **Tabs Don't Switch:**

**Fix:**
```
1. Check tab buttons assigned in Inspector
2. Verify onClick listeners setup
3. Check Input Manager has Previous/Next
4. Test with keyboard Q/E keys
```

### **Selection Border Not Visible:**

**Fix:**
```
1. Add SelectionBorder Image to prefab
2. Set border to disabled by default
3. Set color to visible blue/white
4. Use outline or border image
```

---

## ?? Complete Checklist

**Before Testing:**

- [ ] SkillCard prefab created with all children
- [ ] Grid Container has GridLayoutGroup
- [ ] Preview Panel setup with all text fields
- [ ] Tab buttons assigned and working
- [ ] SkillTreeUI_GridBased component added
- [ ] All references assigned in Inspector
- [ ] SkillManager exists in scene
- [ ] EventSystem exists in scene
- [ ] Input mapping configured

**Test Procedure:**

1. [ ] Press Play
2. [ ] Open skills panel
3. [ ] See grid of skill cards
4. [ ] Navigate with gamepad D-pad
5. [ ] Select card with A/X button
6. [ ] Preview shows on right
7. [ ] Switch tabs with L1/R1
8. [ ] Unlock/upgrade works
9. [ ] Cards update colors
10. [ ] No errors in Console

---

## ?? Advantages Over List UI

**Gamepad Navigation:**
? Natural grid navigation (up/down/left/right)
? Clear visual selection
? Shoulder button tab switching
? Single button actions

**Visual Design:**
? More information at a glance
? Better use of screen space
? Iconic representation
? Professional game UI feel

**User Experience:**
? Faster skill browsing
? Less scrolling
? Category at-a-glance
? Console-style interface

---

## ?? Switching Between UIs

### **To Use Grid UI:**

```
1. Disable SkillTreeUI component
2. Enable SkillTreeUI_GridBased component
3. Assign all references
4. Test!
```

### **To Use List UI:**

```
1. Disable SkillTreeUI_GridBased component
2. Enable SkillTreeUI component
3. Use list layout
```

### **To Use Both:**

```
1. Create two different panels
2. Toggle between them with button
3. Same SkillManager backend
4. Different visual presentation
```

---

## ?? Summary

**Grid-Based UI is Perfect For:**
- ? Gamepad/Controller play
- ? Console-style games
- ? Couch gaming
- ? Large screen displays
- ? Visual skill browsing

**List UI is Better For:**
- ? Mouse-heavy games
- ? Lots of text details
- ? Mobile touch screens
- ? Traditional RPG feel

**Both UIs:**
- ? Use same SkillManager
- ? Same skill data
- ? Same functionality
- ? Just different presentation

---

**Choose the UI that best fits your game's style and target platform!** ??

For a hybrid approach, you could even include both and let players choose their preferred style in settings!
