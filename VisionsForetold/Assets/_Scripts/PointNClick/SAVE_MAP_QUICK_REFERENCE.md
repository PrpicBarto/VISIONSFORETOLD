# Save + Map Integration - Quick Reference

## ? HOW IT WORKS

### **Simple Flow:**
```
Save in Area ? Quit ? Load ? Map opens with area highlighted
```

### **What Gets Saved:**
```
? Current scene name (e.g., "ForestArea")
? Return map scene (e.g., "MapScene")
? Save station position
? Player data (health, skills, etc.)
```

### **What Happens on Load:**
```
1. Game loads map scene
2. Map finds area matching saved scene
3. Area gets highlighted automatically
4. Info panel shows (optional)
5. Player can re-enter or explore
```

---

## ?? PLAYER EXPERIENCE

**Before:**
```
Load save ? Spawn in Forest ? Where am I? ??
```

**After:**
```
Load save ? Map opens ? Forest highlighted ? "Ah, I was here!" ??
?
Re-enter Forest OR Choose different area
```

---

## ?? SETUP CHECKLIST

**Nothing extra needed!** Just ensure:

- [x] SaveManager exists
- [x] MapController in map scene
- [x] SceneConnectionManager in gameplay scenes
- [x] AreaData has sceneName filled

**That's it!** Integration is automatic.

---

## ?? CODE EXAMPLES

### **Check Where Player Saved:**
```csharp
var saveData = SaveManager.Instance.GetCurrentSaveData();
Debug.Log($"Saved in: {saveData.returnAreaId}");
```

### **Manually Select Area:**
```csharp
AreaData area = FindAreaBySceneName("ForestArea");
MapController.Instance.SelectArea(area, clickable);
```

---

## ?? USE CASES

**Hub World:**
```
Central Map ? Multiple dungeons
Each dungeon save ? Returns to central map
```

**Linear Progression:**
```
World Map ? Area 1 ? Area 2 ? Area 3
Load Area 2 save ? Highlight Area 2 on map
```

**Multiple Maps:**
```
Region A Map ? ? Region B Map
Save in Region A ? Returns to Region A Map
```

---

## ? TESTING

1. **Enter area** from map
2. **Save** at save station
3. **Quit** to main menu
4. **Load** save
5. **Check:** Map opens? ?
6. **Check:** Area highlighted? ?
7. **Check:** Can re-enter? ?

---

## ?? QUICK FIXES

**Area not highlighted?**
```
? Check AreaData.sceneName matches save
? Check area exists in current map
```

**Wrong map loads?**
```
? Check SceneConnectionData settings
? Verify "Return Map Scene" field
```

---

## ?? CUSTOMIZATION

**Change behavior:**

Edit `MapController.SelectAreaAfterFrame()`:

```csharp
// Option 1: Just highlight (default)
SelectAreaByIndex(i);

// Option 2: Also open panel
ShowInfoPanel(areaData);

// Option 3: Auto-enter
EnterArea(areaData);
```

---

## ?? SAVE DATA

**Example save file:**
```json
{
  "currentSceneName": "ForestArea",
  "lastMapScene": "MapScene",
  "returnAreaId": "ForestArea",
  "saveStationPosition": "(10,0,15)"
}
```

---

**That's it! Automatic integration between saves and map.** ?????

**Result:** Players always know where they saved and can easily navigate back!
