# ?? Intro Video Setup Guide

## Overview
Play a video cutscene after pressing Play in the main menu, before loading the Point-N-Click scene.

---

## ?? Setup Steps

### **Step 1: Create Video Scene**

1. **Create New Scene:**
   ```
   File ? New Scene
   Save as "IntroVideoScene"
   ```

2. **Add Canvas:**
   ```
   Hierarchy ? Right-click ? UI ? Canvas
   
   Canvas settings:
   - Render Mode: Screen Space - Overlay
   - Pixel Perfect: ? (optional)
   ```

3. **Add Black Fade Panel:**
   ```
   Canvas ? Right-click ? UI ? Image
   Name: "FadePanel"
   
   Settings:
   - Anchor: Stretch (full screen)
   - Color: Black (0, 0, 0, 255)
   - Add Component: Canvas Group
   ```

4. **Add Video Display:**
   ```
   Canvas ? Right-click ? UI ? Raw Image
   Name: "VideoDisplay"
   
   Settings:
   - Anchor: Stretch (full screen)
   - UV Rect: X:0, Y:0, W:1, H:1
   ```

5. **Add Skip Prompt (Optional):**
   ```
   Canvas ? Right-click ? UI ? Text
   Name: "SkipPromptText"
   
   Settings:
   - Text: "Press any key to skip"
   - Anchor: Bottom-Center
   - Font Size: 24
   - Color: White with slight transparency
   - Alignment: Center
   ```

---

### **Step 2: Setup Video Player**

1. **Create Video Player Object:**
   ```
   Hierarchy ? Right-click ? Create Empty
   Name: "IntroVideoPlayer"
   
   Add Component ? IntroVideoPlayer (the script we created)
   ```

2. **Configure IntroVideoPlayer:**

   **Video Settings:**
   ```
   - Video Clip: [Assign your video file]
   - Use UI Display: ?
   - Video Display: [Assign VideoDisplay RawImage]
   ```

   **Scene Settings:**
   ```
   - Next Scene Name: "PointNClickScene"
   - Use Scene Index: ? (or ? with index 2)
   - Next Scene Index: 2
   ```

   **Playback Settings:**
   ```
   - Allow Skip: ?
   - Show Skip Prompt: ?
   - Skip Prompt Text: [Assign SkipPromptText]
   - Skip Delay: 2.0 seconds
   - Auto Load Next Scene: ?
   - Post Video Delay: 0.5 seconds
   ```

   **Fade Settings:**
   ```
   - Use Fade Transitions: ?
   - Fade Panel: [Assign FadePanel Canvas Group]
   - Fade In Duration: 1.0 seconds
   - Fade Out Duration: 1.0 seconds
   ```

   **Audio Settings:**
   ```
   - Video Volume: 1.0
   - Mute Audio: ?
   ```

---

### **Step 3: Prepare Your Video**

1. **Import Video:**
   ```
   Project ? Create folder: "Assets/Videos"
   Drag your video file into this folder
   ```

2. **Video Import Settings:**
   ```
   Select video in Project window
   Inspector:
   
   - Transcode: ? (or ? for better compatibility)
   - Codec: Auto or H.264
   - Quality: High
   - Audio: ? Keep Audio
   ```

3. **Supported Formats:**
   ```
   ? .mp4 (H.264) - Recommended
   ? .mov
   ? .webm
   ? .avi
   ```

4. **Recommended Settings:**
   ```
   Resolution: 1920x1080 (Full HD)
   Frame Rate: 30 or 60 fps
   Codec: H.264
   Audio: AAC
   ```

---

### **Step 4: Update Main Menu**

1. **Open Main Menu Scene**

2. **Select MenuManager Object**

3. **Update Scene Settings:**

   **OLD settings:**
   ```
   Scene Settings:
   - Game Scene Name: "PointNClickScene"
   - Game Scene Index: 1
   ```

   **NEW settings:**
   ```
   Scene Settings:
   - Video Scene Name: "IntroVideoScene"
   - Game Scene Name: "PointNClickScene"
   - Skip Intro Video: ? (unchecked)
   - Use Scene Index: ? (or ?)
   - Video Scene Index: 1
   - Game Scene Index: 2
   ```

---

### **Step 5: Build Settings**

1. **Open Build Settings:**
   ```
   File ? Build Settings
   ```

2. **Add Scenes in Order:**
   ```
   Build Index 0: MainMenuScene
   Build Index 1: IntroVideoScene ? NEW!
   Build Index 2: PointNClickScene
   Build Index 3+: Other game scenes...
   ```

3. **Verify Order:**
   ```
   Make sure IntroVideoScene is BETWEEN MainMenu and PointNClickScene!
   ```

---

## ?? Usage

### **Normal Flow:**
```
MainMenu ? Play Button
  ?
IntroVideoScene ? Video plays
  ?
PointNClickScene ? Game starts
```

### **With Skip:**
```
MainMenu ? Play Button
  ?
IntroVideoScene ? Video starts
  ?
Player presses any key (after 2s delay)
  ?
PointNClickScene ? Game starts immediately
```

### **Skip Video Entirely:**
```
MenuManager ? Skip Intro Video: ?

MainMenu ? Play Button
  ?
PointNClickScene ? Game starts directly (no video)
```

---

## ?? Configuration Options

### **Video Display Methods:**

**Method 1: UI Display (Recommended)**
```
IntroVideoPlayer:
- Use UI Display: ?
- Video Display: [RawImage]

Pros:
? Easy to setup
? Works with UI overlays
? Better for UI-based skip buttons
```

**Method 2: Camera Display**
```
IntroVideoPlayer:
- Use UI Display: ?
- Camera: Main Camera

Pros:
? Better performance
? Full-screen rendering
```

---

### **Skip Options:**

**Allow Skip After Delay:**
```
- Allow Skip: ?
- Skip Delay: 2.0 seconds
- Show Skip Prompt: ?
```

**Immediate Skip:**
```
- Allow Skip: ?
- Skip Delay: 0.0 seconds
- Show Skip Prompt: ?
```

**No Skip:**
```
- Allow Skip: ?
- Show Skip Prompt: ?
```

---

## ?? Customization

### **Add Skip Button:**

1. **Create Button:**
   ```
   Canvas ? Right-click ? UI ? Button
   Name: "SkipButton"
   Text: "Skip"
   ```

2. **Position:**
   ```
   Anchor: Bottom-Right
   Position: Offset from corner
   ```

3. **Connect to Script:**
   ```
   Button ? OnClick()
   - IntroVideoPlayer.ManualSkip()
   ```

---

### **Multiple Videos:**

To play multiple videos in sequence:

1. **Create Multiple Video Scenes:**
   ```
   IntroVideoScene1 ? Video1
   IntroVideoScene2 ? Video2
   IntroVideoScene3 ? Video3
   ```

2. **Chain Them:**
   ```
   MainMenu ? IntroVideoScene1
   IntroVideoScene1 ? IntroVideoScene2 (Next Scene Name)
   IntroVideoScene2 ? IntroVideoScene3
   IntroVideoScene3 ? PointNClickScene
   ```

---

### **Subtitles/Captions:**

Add subtitle system:

1. **Create Subtitle Text:**
   ```
   Canvas ? UI ? Text
   Name: "SubtitleText"
   Settings:
   - Anchor: Bottom-Center
   - Background: Semi-transparent black panel
   ```

2. **Create Subtitle Manager Script:**
   - Load subtitles from file or array
   - Sync with video playback time
   - Show/hide based on timestamps

---

## ?? Troubleshooting

### **Video doesn't play:**

**Check:**
```
? Video clip is assigned in IntroVideoPlayer
? Video is imported correctly (check Inspector)
? Render Texture is created (if using UI display)
? VideoDisplay RawImage is assigned
? Console shows any errors
```

**Fix:**
```
- Reimport video with Transcode enabled
- Try H.264 codec
- Check video resolution (not too high)
```

---

### **Video plays but no audio:**

**Check:**
```
? Video has audio track (check original file)
? Mute Audio is unchecked
? Video Volume > 0
? AudioSource component exists
```

**Fix:**
```
- Reimport video with "Keep Audio" enabled
- Check AudioSource volume in Inspector
- Verify audio mixer settings
```

---

### **Video appears stretched/distorted:**

**Fix:**
```
1. RawImage ? UV Rect:
   - X: 0, Y: 0, W: 1, H: 1

2. Match aspect ratio:
   - Video: 16:9 (1920x1080)
   - RawImage: Set to maintain aspect ratio

3. Use AspectRatioFitter component:
   - Add to VideoDisplay
   - Aspect Mode: Fit In Parent
```

---

### **Scene doesn't load after video:**

**Check:**
```
? Next Scene Name is correct
? Scene is added to Build Settings
? Auto Load Next Scene is checked
? Console shows any errors
```

**Fix:**
```
1. File ? Build Settings
2. Add "PointNClickScene"
3. Note the build index
4. Update IntroVideoPlayer settings
```

---

### **Can't skip video:**

**Check:**
```
? Allow Skip is checked
? Skip Delay has passed (default 2 seconds)
? Video is actually playing
```

**Debug:**
```
- Press keys/mouse during video
- Check Console for "[IntroVideo] Skip enabled" message
- Verify canSkip flag is true
```

---

## ?? Performance Tips

### **Optimize Video:**

```
Resolution:
- 1080p for high quality
- 720p for better performance
- Match game's target resolution

Frame Rate:
- 30 fps for cutscenes
- 24 fps for cinematic feel
- Lower = smaller file size

Compression:
- H.264 codec (best compatibility)
- Medium to High quality
- Avoid lossless (huge files)
```

### **Reduce File Size:**

```
1. Lower resolution (1080p ? 720p)
2. Lower bitrate (balance quality vs size)
3. Use video compression tools
4. Consider using WebM format
```

---

## ? Testing Checklist

**Before releasing:**
```
? Video plays correctly
? Audio is synchronized
? Skip works after delay
? Scene transitions smoothly
? Fade in/out works
? No black screen freezes
? Works in Build (not just Editor)
? Tested on target platforms
? Video file is in correct folder
? All scenes in Build Settings
```

---

## ?? Quick Start Summary

**Minimum Setup:**

```
1. Create "IntroVideoScene"
2. Add Canvas with RawImage
3. Add IntroVideoPlayer script
4. Assign video clip
5. Assign RawImage to VideoDisplay
6. Set Next Scene Name: "PointNClickScene"
7. Add scene to Build Settings (index 1)
8. Update MenuManager video scene name
9. Test!
```

**That's it! Your intro video is ready!** ???

---

## ?? Example Setup Values

**IntroVideoPlayer Inspector:**
```
Video Settings:
  Video Clip: IntroVideo.mp4
  Use UI Display: ?
  Video Display: VideoDisplay

Scene Settings:
  Next Scene Name: PointNClickScene
  Use Scene Index: ?
  Next Scene Index: 2

Playback Settings:
  Allow Skip: ?
  Show Skip Prompt: ?
  Skip Delay: 2
  Auto Load Next Scene: ?
  Post Video Delay: 0.5

Fade Settings:
  Use Fade Transitions: ?
  Fade In Duration: 1
  Fade Out Duration: 1

Audio Settings:
  Video Volume: 1
  Mute Audio: ?
```

---

## ?? Advanced Features

### **Add Loading Screen:**

Create a loading screen between video and game:

```
IntroVideoScene ? LoadingScene ? PointNClickScene
```

### **Save Skip Preference:**

Remember if player skipped the video:

```csharp
// In IntroVideoPlayer
private void SkipVideo()
{
    PlayerPrefs.SetInt("SkippedIntro", 1);
    PlayerPrefs.Save();
    // ... rest of skip code
}

// In MenuManager
private void LoadGameScene()
{
    bool hasSkipped = PlayerPrefs.GetInt("SkippedIntro", 0) == 1;
    if (hasSkipped || skipIntroVideo)
    {
        // Load game directly
    }
}
```

---

**Your intro video system is now complete!** ??

Press Play in the main menu and your video will play before the game starts!

