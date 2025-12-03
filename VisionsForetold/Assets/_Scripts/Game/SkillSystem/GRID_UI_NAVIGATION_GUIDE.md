# Grid-Based Skill Tree - Navigation Guide

## ? What's New

Your grid-based skill tree UI now has complete back navigation support! Players can seamlessly navigate:

```
Save Station Menu
    ? (Click "Skills" / Press A)
Skill Grid View
    ? (Click skill card / Press A)
Skill Preview Panel
    ? (Click "Back" / Press B or Escape)
Skill Grid View
    ? (Click "Close" / Press B or Escape)
Save Station Menu
```

---

## ?? Controls Reference

### **Keyboard & Mouse**

| Action | Input | Description |
|--------|-------|-------------|
| Navigate Grid | Arrow Keys / WASD | Move between skill cards |
| Select Skill | Click / Enter | View skill preview |
| Close Preview | Escape / Click "Back" | Return to grid |
| Close Skill Tree | Escape (when grid visible) | Return to save menu |
| Switch Tab | Q / E | Previous / Next category |
| Unlock/Upgrade | Click Button | Perform skill action |

### **Gamepad (Xbox)**

| Action | Button | Description |
|--------|--------|-------------|
| Navigate Grid | D-Pad / Left Stick | Move between skill cards |
| Select Skill | A | View skill preview |
| Back | B | Close preview or skill tree |
| Switch Tab | LB / RB | Previous / Next category |
| Unlock/Upgrade | A (on action button) | Perform skill action |

### **Gamepad (PlayStation)**

| Action | Button | Description |
|--------|--------|-------------|
| Navigate Grid | D-Pad / Left Stick | Move between skill cards |
| Select Skill | X (Cross) | View skill preview |
| Back | Circle | Close preview or skill tree |
| Switch Tab | L1 / R1 | Previous / Next category |
| Unlock/Upgrade | X (on action button) | Perform skill action |

---

## ??? Unity Setup

### **Step 1: Assign Components**

1. **Open your Save Station Canvas** in the Unity Editor
2. **Select the SkillsPanel GameObject**
3. You should see the **SkillTreeUI_GridBased** component in the Inspector

### **Step 2: Configure Buttons**

In the **SkillTreeUI_GridBased** Inspector, you'll see new button fields:

```
Skill Preview Panel:
??? Preview Panel (GameObject)
??? Preview Name Text
??? Preview Description Text
??? Preview Level Text
??? Preview Cost Text
??? Preview Icon
??? Action Button (Unlock/Upgrade)
??? Action Button Text
??? Back Button ? NEW! (Goes back to grid)
??? Close Skill Tree Button ? NEW! (Returns to save menu)
```

### **Step 3: Create Buttons (if needed)**

#### **Back Button** (in Preview Panel):
1. Right-click Preview Panel ? UI ? Button
2. Name: "BackButton"
3. Position: Top-left or bottom of preview panel
4. Text: "Back", "?", or "Close Preview"
5. **Assign** to "Back Button" field in SkillTreeUI_GridBased

#### **Close Skill Tree Button** (in main Skill Panel):
1. Right-click SkillsPanel ? UI ? Button
2. Name: "CloseButton" or "BackToMenuButton"
3. Position: Top-right corner or bottom
4. Text: "Close", "×", or "Back to Menu"
5. **Assign** to "Close Skill Tree Button" field in SkillTreeUI_GridBased

### **Step 4: Configure SaveStationMenu**

1. **Select** your SaveStationMenu GameObject
2. In the Inspector, find the **Skills Panel** section:

```
Skills Panel:
??? Skills Panel (GameObject reference)
??? Close Skills Button
??? Skill Tree UI (for list-based, leave empty if using grid)
??? Skill Tree UI Grid ? Set this! (Drag your SkillsPanel here)
```

3. **Drag** your SkillsPanel (with SkillTreeUI_GridBased) to the "Skill Tree UI Grid" field
4. The system will auto-detect if not assigned

---

## ?? Layout Recommendations

### **Grid Layout**

```
???????????????????????????????????????????
? [All] [Combat] [Magic] [Defense] [Util] ? ? Tabs
???????????????????????????????????????????
? ??????? ??????? ???????                ?
? ?Skill? ?Skill? ?Skill?                ? ? Grid
? ?  1  ? ?  2  ? ?  3  ?    ??????????? ?
? ??????? ??????? ???????    ? Preview ? ?
?                             ?  Panel  ? ?
? ??????? ??????? ???????    ?         ? ?
? ?Skill? ?Skill? ?Skill?    ? [Back]  ? ?
? ?  4  ? ?  5  ? ?  6  ?    ??????????? ?
? ??????? ??????? ???????                ?
???????????????????????????????????????????
? [Close] Lv:5 | XP: 250/500 | Points: 3 ?
???????????????????????????????????????????
```

### **Button Placement**

**Back Button** (in Preview Panel):
- **Top-Left**: Easy to find, consistent with web conventions
- **Bottom**: Grouped with action buttons
- **Top-Right**: As an "X" close button

**Close Skill Tree Button** (main panel):
- **Top-Right**: Most common for "exit" buttons
- **Bottom-Center**: Grouped with player stats
- **Top-Left**: With a "<" icon for "back"

---

## ?? Navigation Behavior

### **Smart Back Button Logic**

The system uses intelligent back navigation:

```csharp
Escape Key / B Button:
??? If preview panel is open ? Close preview (show grid)
??? If grid is visible ? Close skill tree (show save menu)
```

### **Two-Stage Navigation**

1. **First Stage**: Viewing skills in grid
   - **Escape/B** ? Returns to Save Station Menu
   - No preview open = direct exit

2. **Second Stage**: Viewing skill preview
   - **Escape/B** ? Closes preview, shows grid
   - **Escape/B again** ? Closes skill tree

---

## ?? Visual Feedback

### **Card States**

Cards change color based on status:

| State | Color | Meaning |
|-------|-------|---------|
| **Unlocked** | Green | Skill is active |
| **Can Unlock** | Yellow | Requirements met |
| **Locked** | Gray | Requirements not met |
| **Selected** | Blue Border | Currently viewing |

### **Selection Indicator**

When you navigate with gamepad/keyboard, a **selection border** appears around the current card.

---

## ?? Advanced Configuration

### **Grid Settings** (Inspector)

```
Grid Settings:
??? Grid Columns: 3 (Default)
??? Card Spacing: 10
??? Card Size: (200, 150)
```

**Adjust for your UI:**
- **2 columns**: Wide cards, fewer skills per row
- **3 columns**: Balanced (default)
- **4+ columns**: Compact, more skills visible

### **Navigation Settings**

```
Gamepad Navigation:
??? Enable Auto Navigation: ?
??? Navigation Delay: 0.15 (seconds between inputs)
```

**Navigation Delay:**
- **0.1s**: Very responsive, might feel twitchy
- **0.15s**: Default, smooth navigation
- **0.2s**: Deliberate, prevents accidental double-taps

---

## ?? Testing Checklist

### **Basic Navigation**
- [ ] Click skill card ? Preview opens
- [ ] Click "Back" in preview ? Grid shows
- [ ] Click "Close" in grid ? Save menu shows
- [ ] Escape key works at each stage

### **Gamepad Navigation**
- [ ] D-Pad moves between cards
- [ ] A button selects card
- [ ] B button goes back (two stages)
- [ ] LB/RB switch tabs
- [ ] Selection border follows input

### **Keyboard Navigation**
- [ ] Arrow keys move between cards
- [ ] Enter selects card
- [ ] Escape goes back (two stages)
- [ ] Q/E switch tabs

### **Mouse Still Works**
- [ ] Can click cards directly
- [ ] Can click preview buttons
- [ ] Can mix mouse and keyboard
- [ ] Can mix mouse and gamepad

---

## ?? Troubleshooting

### **Preview doesn't close with Escape**

**Check:**
- Back button is assigned
- Preview Panel has the button as a child
- Button's onClick is connected

**Fix:**
```
SkillTreeUI_GridBased Inspector:
? Back Button field: Assign your back button
```

### **Can't close skill tree**

**Check:**
- Close Skill Tree Button is assigned
- SaveStationMenu has reference to SkillTreeUI_GridBased
- Event subscription is working

**Fix:**
```
SaveStationMenu Inspector:
? Skill Tree UI Grid: Assign SkillsPanel (with grid component)
```

### **Gamepad doesn't navigate grid**

**Check:**
- Enable Auto Navigation is checked
- Input Manager has "Horizontal" and "Vertical" axes
- EventSystem exists in scene

**Fix:**
```
SkillTreeUI_GridBased Inspector:
? Enable Auto Navigation: ?
```

### **Buttons don't switch tabs**

**Check:**
- Tab buttons (All/Combat/etc.) are assigned
- Buttons have onClick listeners

**Fix:**
```
SkillTreeUI_GridBased Inspector:
? Category Tabs: Assign all 5 tab buttons
```

---

## ?? Code Examples

### **Close Skill Tree Programmatically**

```csharp
// Get reference
SkillTreeUI_GridBased gridUI = GetComponent<SkillTreeUI_GridBased>();

// Close the skill tree
gridUI.CloseSkillTree();
```

### **Subscribe to Close Event**

```csharp
void Start()
{
    SkillTreeUI_GridBased gridUI = FindObjectOfType<SkillTreeUI_GridBased>();
    
    if (gridUI != null)
    {
        gridUI.OnSkillTreeClosed += OnSkillTreeClosed;
    }
}

void OnSkillTreeClosed()
{
    Debug.Log("Skill tree was closed!");
    // Do something when skill tree closes
}
```

---

## ?? Comparison: List vs Grid UI

Both UIs now have **identical navigation features**!

| Feature | List UI | Grid UI |
|---------|---------|---------|
| Back from detail | ? | ? |
| Close skill tree | ? | ? |
| Escape key (2-stage) | ? | ? |
| Gamepad B button | ? | ? |
| Event callbacks | ? | ? |
| SaveStation integration | ? | ? |

**Choose based on your preference:**
- **List UI**: Traditional, text-focused, PC-friendly
- **Grid UI**: Modern, visual, gamepad-optimized

---

## ?? Related Files

- `SkillTreeUI_GridBased.cs` - Grid UI controller (updated)
- `SkillTreeUI.cs` - List UI controller (also updated)
- `SaveStationMenu.cs` - Menu controller (supports both)
- `SaveStation.cs` - Interaction controller
- `GRID_UI_SETUP_GUIDE.md` - Full grid UI setup
- `SKILL_SYSTEM_GUIDE.md` - Skill system overview

---

## ? Summary

? **Two-stage back navigation** (preview ? grid ? menu)
? **Smart Escape key handling** (contextual back)
? **Gamepad B button support** (same as Escape)
? **Event-driven architecture** (SaveStation integration)
? **Works with both List and Grid UIs**
? **No dead ends** (always a way back)

**Your grid-based skill tree now has professional-grade navigation!** ????

---

**Need help?** Check the troubleshooting section or refer to the setup guide!
