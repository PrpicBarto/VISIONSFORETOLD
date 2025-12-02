# Quick Setup - Gamepad Navigation

## ? 3-MINUTE SETUP

### **1. Enable Gamepad on Map (30 seconds)**

```
MapController Inspector:
?? Gamepad/Keyboard Navigation
?  ?? Enable Gamepad Navigation: ?
?  ?? Auto Select First: ?
?  ?? Grid Navigation: ?
?  ?? Grid Columns: 3
```

### **2. Create Connections Asset (1 minute)**

```
Right-click Project ? Create ? Map System ? Scene Connections

Configure:
?? Default Map Scene: "MapScene"
?? Add connections for each area scene
```

### **3. Add Manager to Scenes (1 minute)**

```
In each gameplay scene:
1. Create GameObject: "SceneConnectionManager"
2. Add Component: Scene Connection Manager
3. Assign: Connection Data asset
```

### **4. Test!** (30 seconds)

```
Press Play
Use D-Pad or Arrow Keys
Select with A button or Enter
```

---

## ?? CONTROLS CHEAT SHEET

### **Xbox Controller:**
```
D-Pad / Left Stick ? Navigate
A ? Select / Confirm
B ? Cancel / Back
LB / RB ? Quick Nav
Start ? Return to Map
```

### **PlayStation Controller:**
```
D-Pad / Left Stick ? Navigate
X (Cross) ? Select / Confirm
Circle ? Cancel / Back
L1 / R1 ? Quick Nav
Start ? Return to Map
```

### **Keyboard:**
```
Arrow Keys ? Navigate
Enter ? Select / Confirm
ESC ? Cancel / Back
Q / E ? Quick Nav
M ? Return to Map
```

---

## ?? SCENE CONNECTIONS PATTERN

```csharp
// Simple unlock chain:
Forest ? Unlocks Village
Village ? Unlocks Castle
Castle ? Unlocks Boss

// In SceneConnections asset:
Forest Connection:
  Unlocked Areas: [Village]
  
Village Connection:
  Unlocked Areas: [Castle]
  
Castle Connection:
  Unlocked Areas: [Boss]
```

---

## ?? CODE SNIPPETS

### **Complete Scene (Unlock Areas)**

```csharp
// When player finishes objective:
SceneConnectionManager.Instance.CompleteScene();
```

### **Return to Map**

```csharp
// Programmatically return to map:
SceneConnectionManager.Instance.ReturnToMap();
```

### **Manual Unlock**

```csharp
// Unlock specific area:
areaData.isUnlocked = true;
```

---

## ? QUICK TEST

1. **Map Scene:**
   - Press Play
   - Press Arrow Keys
   - See selection indicator move
   - Press Enter
   - See info panel open

2. **Enter Area:**
   - Press Enter again
   - Scene loads

3. **Return to Map:**
   - Press M or Start
   - Back to map

4. **Check Unlocks:**
   - New areas should be unlocked

---

**That's it! Gamepad navigation and scene connections working!** ???
