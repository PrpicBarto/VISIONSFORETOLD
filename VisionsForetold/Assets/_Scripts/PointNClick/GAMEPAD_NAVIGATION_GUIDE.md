# Gamepad Navigation & Scene Connections - Setup Guide

## ? FEATURES ADDED

### 1. **Full Gamepad/Controller Support**
- Navigate map areas with D-Pad or analog stick
- Select areas with A button (Xbox) / X button (PlayStation)
- Close panels with B button (Xbox) / Circle button (PlayStation)  
- Quick navigation with shoulder buttons (L1/R1, LB/RB)

### 2. **Keyboard Navigation**
- Arrow keys for navigation
- Enter/Return to select
- ESC to close panels
- Q/E for quick previous/next

### 3. **Scene Connection System**
- Define which scenes unlock which areas
- Auto-return to map from gameplay scenes
- Bidirectional scene flow (map ? gameplay)

---

## ?? GAMEPAD CONTROLS

### **Map Navigation:**

| Button | Xbox | PlayStation | Keyboard | Action |
|--------|------|-------------|----------|--------|
| Navigate | D-Pad / Left Stick | D-Pad / Left Stick | Arrow Keys | Move between areas |
| Select | A | X (Cross) | Enter | Select highlighted area |
| Cancel | B | Circle | ESC | Close info panel |
| Previous | LB | L1 | Q | Previous area |
| Next | RB | R1 | E | Next area |
| Enter Area | A (in panel) | X (in panel) | Enter | Enter selected area |

### **In-Game (Gameplay Scene):**

| Button | Xbox | PlayStation | Keyboard | Action |
|--------|------|-------------|----------|--------|
| Return to Map | Start | Start | M | Return to map scene |

---

## ?? SETUP GUIDE

### **Step 1: Enable Gamepad Navigation**

On your **MapController**:

```
Inspector ? Map Controller
?? Gamepad/Keyboard Navigation
?  ?? Enable Gamepad Navigation: ?
?  ?? Navigation Delay: 0.2
?  ?? Auto Select First: ?
?  ?? Grid Navigation: ?
?  ?? Grid Columns: 3
```

**Settings Explained:**
- **Enable Gamepad Navigation**: Turn on/off controller support
- **Navigation Delay**: Delay between inputs (prevents too-fast scrolling)
- **Auto Select First**: Automatically highlight first area on map load
- **Grid Navigation**: Navigate in grid (true) vs list (false)
- **Grid Columns**: Number of columns for grid navigation

---

### **Step 2: Create Scene Connection Data**

**Create the asset:**

```
1. Right-click in Project window
2. Create ? Map System ? Scene Connections
3. Name: "SceneConnections"
```

**Configure connections:**

```
Inspector ? Scene Connections
?? Default Map Scene: "MapScene"
?? Connections (size 3)
   ?? Element 0
   ?  ?? Scene Name: "ForestArea"
   ?  ?? Unlocked Areas: [Village Area, Lake Area]
   ?  ?? Return Map Scene: "MapScene"
   ?  ?? Auto Unlock: ?
   ?? Element 1
   ?  ?? Scene Name: "VillageArea"
   ?  ?? Unlocked Areas: [Castle Area]
   ?  ?? Return Map Scene: "MapScene"
   ?  ?? Auto Unlock: ?
   ?? Element 2
      ?? Scene Name: "CastleArea"
      ?? Unlocked Areas: [Final Boss Area]
      ?? Return Map Scene: "MapScene"
      ?? Auto Unlock: ?
```

---

### **Step 3: Add Scene Connection Manager to Gameplay Scenes**

In **each gameplay scene** (ForestArea, VillageArea, etc.):

```
1. Create Empty GameObject: "SceneConnectionManager"
2. Add Component ? Scene Connection Manager
3. Configure:
   ?? Connection Data: SceneConnections (asset from Step 2)
   ?? Current Scene Name: (auto-detected)
   ?? Unlock On Start: ? (unchecked)
   ?? Unlock On Completion: ? (checked)
   ?? Show Return UI: ?
   ?? Return Key: M
   ?? Return Button: "Start"
```

---

### **Step 4: Connect Scenes**

**In your map scene (MapScene):**

Each ClickableArea should have:
```
Inspector ? Clickable Area
?? Area Data
   ?? Scene Loading
      ?? Scene Name: "ForestArea"
      ?? Load Async: ?
```

**In gameplay scenes:**
- SceneConnectionManager handles return to map automatically
- Press M or Start button to return

---

## ?? USAGE EXAMPLES

### **Example 1: Linear Progression**

```
Map Scene
  ?
Forest (unlocks Village)
  ?
Village (unlocks Castle)
  ?
Castle (unlocks Boss)
  ?
Boss Area (game complete)
```

**Scene Connections Setup:**
```
Forest ? Unlocks: [Village]
Village ? Unlocks: [Castle]
Castle ? Unlocks: [Boss]
```

---

### **Example 2: Hub World**

```
        Map Scene (Hub)
       /    |    \
      /     |     \
Forest  Village  Lake
      \     |     /
       \    |    /
        Final Area
```

**Scene Connections Setup:**
```
Forest ? Unlocks: [Final Area]
Village ? Unlocks: [Final Area]
Lake ? Unlocks: [Final Area]
```

All three must be completed to unlock final area.

---

### **Example 3: Branching Paths**

```
         Map
          |
      Forest Area
        /   \
       /     \
   Path A   Path B
      |       |
   End A    End B
```

**Scene Connections Setup:**
```
Forest ? Unlocks: [Path A, Path B]
Path A ? Unlocks: [End A], Locks: [Path B]
Path B ? Unlocks: [End B], Locks: [Path A]
```

---

## ?? CODE EXAMPLES

### **Unlock Area from Code**

```csharp
// In gameplay scene, when objective complete:
SceneConnectionManager.Instance.CompleteScene();
```

### **Manual Area Unlock**

```csharp
// Unlock specific area
public AreaData villageArea;

void UnlockVillage()
{
    villageArea.isUnlocked = true;
    villageArea.isDiscovered = true;
}
```

### **Return to Map Programmatically**

```csharp
// From any scene
SceneConnectionManager.Instance.ReturnToMap();
```

### **Custom Scene Loading**

```csharp
// Load specific scene
SceneConnectionManager.Instance.LoadScene("BossArena");
```

---

## ?? NAVIGATION MODES

### **Grid Navigation (Default)**

Best for map with areas arranged in grid:

```
[Area1] [Area2] [Area3]
[Area4] [Area5] [Area6]
[Area7] [Area8] [Area9]
```

**Navigation:**
- Left/Right: Move horizontally
- Up/Down: Move vertically
- Wraps around edges

**Setup:**
```
Grid Navigation: ?
Grid Columns: 3
```

---

### **List Navigation**

Best for vertical or horizontal list:

```
[Area1]
[Area2]
[Area3]
[Area4]
```

**Navigation:**
- Any direction: Next/Previous
- Wraps around at end

**Setup:**
```
Grid Navigation: ?
```

---

## ?? ADVANCED CONFIGURATION

### **Custom Navigation Delay**

Adjust responsiveness:

```csharp
// Faster navigation (twitchy)
Navigation Delay: 0.1

// Default (balanced)
Navigation Delay: 0.2

// Slower navigation (deliberate)
Navigation Delay: 0.3
```

---

### **Auto-Select First Area**

```csharp
// Automatically highlight first area when map loads
Auto Select First: ?

// Start with nothing selected
Auto Select First: ?
```

---

### **Custom Grid Layout**

Match your area arrangement:

```csharp
// 2-column grid
Grid Columns: 2

// 3-column grid (default)
Grid Columns: 3

// 4-column grid (wide map)
Grid Columns: 4
```

---

## ?? INPUT SYSTEM SETUP

Make sure these inputs are defined in **Edit ? Project Settings ? Input Manager**:

### **Required Axes:**

```
Horizontal:
?? Positive Button: "right"
?? Negative Button: "left"
?? Alt Positive Button: "d"
?? Alt Negative Button: "a"
?? Type: Key or Mouse Button

Vertical:
?? Positive Button: "up"
?? Negative Button: "down"
?? Alt Positive Button: "w"
?? Alt Negative Button: "s"
?? Type: Key or Mouse Button

Fire1: (A button / Space)
Jump: (B button / Escape)
Previous: (LB / Q)
Next: (RB / E)
Start: (Start button / M)
```

---

## ?? SELECTION INDICATOR

The selection indicator shows which area is highlighted.

### **Auto-Created Indicator:**

```
Auto Create Indicator: ?
```

Creates a yellow border around selected area.

### **Custom Indicator:**

```
1. Create UI GameObject with Image
2. Assign to Selection Indicator field
3. Configure colors and animation
```

---

## ? TESTING CHECKLIST

### **Gamepad Navigation:**
- [ ] D-Pad moves between areas
- [ ] Analog stick moves between areas
- [ ] A button selects area
- [ ] B button closes panel
- [ ] LB/RB navigate quickly
- [ ] Selection indicator follows highlight

### **Keyboard Navigation:**
- [ ] Arrow keys move between areas
- [ ] Enter selects area
- [ ] ESC closes panel
- [ ] Q/E navigate quickly

### **Scene Connections:**
- [ ] Entering area loads correct scene
- [ ] Completing scene unlocks areas
- [ ] Return to map works
- [ ] M key returns to map
- [ ] Start button returns to map

### **Mouse Still Works:**
- [ ] Clicking areas still works
- [ ] Hovering still works
- [ ] Tooltip still shows
- [ ] Mouse and gamepad can be mixed

---

## ?? TROUBLESHOOTING

### **Gamepad not working:**

Check:
- [ ] Enable Gamepad Navigation is checked
- [ ] Input Manager has required axes
- [ ] Controller is connected
- [ ] Unity recognizes controller (test in Input window)

---

### **Navigation skips areas:**

Fix:
```
Grid Columns doesn't match actual layout

If you have 6 areas in 2 rows of 3:
Grid Columns: 3 ?

If you have 6 areas in 3 rows of 2:
Grid Columns: 2 ?
```

---

### **Can't return to map:**

Check:
- [ ] SceneConnectionManager in gameplay scene
- [ ] Connection Data assigned
- [ ] Return Map Scene field is filled
- [ ] Map scene is in Build Settings

---

### **Areas don't unlock:**

Check:
- [ ] SceneConnectionManager.CompleteScene() is called
- [ ] Unlock On Completion is checked
- [ ] Area is in Unlocked Areas list
- [ ] AreaData isUnlocked is true

---

## ?? EXAMPLE FULL SETUP

### **Map Scene:**

```
MapCanvas
?? MapController
?  ?? Enable Gamepad Navigation: ?
?  ?? Grid Navigation: ?
?  ?? Grid Columns: 3
?? Areas
   ?? ForestArea (ClickableAreaUI)
   ?? VillageArea (ClickableAreaUI)
   ?? CastleArea (ClickableAreaUI)
   ?? ... (more areas)
```

### **Gameplay Scene (ForestArea):**

```
Scene Root
?? SceneConnectionManager
?  ?? Connection Data: SceneConnections
?  ?? Unlock On Completion: ?
?  ?? Return Key: M
?? Level Content
   ?? (your gameplay objects)
```

### **Connection Data Asset:**

```
SceneConnections
?? Connections
   ?? ForestArea
      ?? Unlocked Areas: [VillageArea, LakeArea]
      ?? Return Map Scene: "MapScene"
```

---

## ?? SUMMARY

**Gamepad Navigation:**
- ? Full controller support
- ? Grid or list navigation
- ? Keyboard alternative
- ? Auto-select first area
- ? Visual selection indicator

**Scene Connections:**
- ? Define unlock progression
- ? Auto-unlock on completion
- ? Return to map button
- ? Bidirectional flow
- ? Lock/unlock areas

**Easy Setup:**
1. Enable gamepad navigation on MapController
2. Create SceneConnections asset
3. Add SceneConnectionManager to gameplay scenes
4. Define which scenes unlock which areas
5. Done!

---

**Now your map system supports full gamepad navigation and connected scenes!** ?????
