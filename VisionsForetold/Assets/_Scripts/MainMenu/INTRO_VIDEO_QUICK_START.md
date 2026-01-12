# ?? Quick Setup - Intro Video

## ? What You Need

1. **Video file** (.mp4, .mov, or .webm)
2. **3 Scenes:**
   - MainMenuScene (existing)
   - IntroVideoScene (NEW - create this)
   - PointNClickScene (existing)

---

## ?? 5-Minute Setup

### **1. Create IntroVideoScene**

```
File ? New Scene ? Save as "IntroVideoScene"
```

### **2. Add UI Components**

**Hierarchy:**
```
Create ? UI ? Canvas
  ?? RawImage (name: "VideoDisplay", anchor: stretch full screen)
  ?? Image (name: "FadePanel", anchor: stretch, color: black)
      ?? Add Component: Canvas Group
  ?? Text (name: "SkipText", text: "Press any key to skip")
```

### **3. Add Video Player Script**

```
Create Empty GameObject ? Name: "IntroVideoPlayer"
Add Component ? IntroVideoPlayer script
```

**Inspector Settings:**
```
Video Settings:
  - Video Clip: [YOUR VIDEO]
  - Use UI Display: ?
  - Video Display: [VideoDisplay RawImage]

Scene Settings:
  - Next Scene Name: "PointNClickScene"

Playback Settings:
  - Allow Skip: ?
  - Skip Prompt Text: [SkipText]

Fade Settings:
  - Use Fade Transitions: ?
  - Fade Panel: [FadePanel Canvas Group]
```

### **4. Update Build Settings**

```
File ? Build Settings ? Add Scenes:

0: MainMenuScene
1: IntroVideoScene ? ADD THIS!
2: PointNClickScene
```

### **5. Update MenuManager**

```
Select MenuManager in MainMenu scene

Inspector ? Scene Settings:
  - Video Scene Name: "IntroVideoScene"
  - Game Scene Name: "PointNClickScene"
  - Skip Intro Video: ? (unchecked)
```

---

## ?? Done!

Press Play in MainMenu ? Video plays ? Game loads! ???

---

## ?? Quick Toggle

**To skip video entirely:**
```
MenuManager ? Skip Intro Video: ?
```

**To re-enable video:**
```
MenuManager ? Skip Intro Video: ?
```

---

## ?? If Video Doesn't Play

**Check these 4 things:**
```
1. Video file is assigned in IntroVideoPlayer
2. IntroVideoScene is in Build Settings (index 1)
3. MenuManager Video Scene Name is "IntroVideoScene"
4. RawImage is assigned to Video Display field
```

---

## ?? Video Specs

**Recommended:**
```
Resolution: 1920x1080 (Full HD)
Format: .mp4 (H.264)
Frame Rate: 30 fps
Audio: AAC
```

---

**That's it! See INTRO_VIDEO_SETUP.md for detailed guide.**
