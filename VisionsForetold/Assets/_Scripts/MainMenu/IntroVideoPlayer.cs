using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Plays a video cutscene before loading the game scene.
/// Place this in a dedicated "VideoScene" between MainMenu and PointNClickScene.
/// </summary>
public class IntroVideoPlayer : MonoBehaviour
{
    [Header("Video Settings")]
    [Tooltip("The video clip to play")]
    [SerializeField] private VideoClip videoClip;
    
    [Tooltip("Render texture for the video (optional, uses camera if null)")]
    [SerializeField] private RenderTexture renderTexture;
    
    [Tooltip("Raw Image to display video (if using UI)")]
    [SerializeField] private RawImage videoDisplay;
    
    [Tooltip("Use UI display instead of camera")]
    [SerializeField] private bool useUIDisplay = true;

    [Header("Scene Settings")]
    [Tooltip("Scene to load after video finishes")]
    [SerializeField] private string nextSceneName = "PointNClickScene";
    
    [Tooltip("Use scene index instead of name")]
    [SerializeField] private bool useSceneIndex = false;
    
    [Tooltip("Scene build index (if useSceneIndex is true)")]
    [SerializeField] private int nextSceneIndex = 2;

    [Header("Playback Settings")]
    [Tooltip("Allow skipping video with any key/button")]
    [SerializeField] private bool allowSkip = true;
    
    [Tooltip("Show skip prompt UI")]
    [SerializeField] private bool showSkipPrompt = true;
    
    [Tooltip("Skip prompt text (e.g., 'Press any key to skip')")]
    [SerializeField] private Text skipPromptText;
    
    [Tooltip("Delay before allowing skip (seconds)")]
    [SerializeField] private float skipDelay = 2f;
    
    [Tooltip("Auto-load next scene after video")]
    [SerializeField] private bool autoLoadNextScene = true;
    
    [Tooltip("Delay after video ends before loading (seconds)")]
    [SerializeField] private float postVideoDelay = 0.5f;

    [Header("Fade Settings")]
    [Tooltip("Enable fade in/out transitions")]
    [SerializeField] private bool useFadeTransitions = true;
    
    [Tooltip("Fade panel (black overlay)")]
    [SerializeField] private CanvasGroup fadePanel;
    
    [Tooltip("Fade in duration at start")]
    [SerializeField] private float fadeInDuration = 1f;
    
    [Tooltip("Fade out duration at end")]
    [SerializeField] private float fadeOutDuration = 1f;

    [Header("Audio Settings")]
    [Tooltip("Video audio volume (0-1)")]
    [SerializeField, Range(0f, 1f)] private float videoVolume = 1f;
    
    [Tooltip("Mute video audio")]
    [SerializeField] private bool muteAudio = false;

    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;

    // Components
    private VideoPlayer videoPlayer;
    private AudioSource audioSource;
    private bool isVideoPlaying = false;
    private bool canSkip = false;
    private bool hasSkipped = false;
    private float videoStartTime;

    #region Unity Lifecycle

    private void Awake()
    {
        InitializeComponents();
    }

    private void Start()
    {
        StartCoroutine(PlayVideoSequence());
    }

    private void Update()
    {
        // Handle skip input
        if (allowSkip && canSkip && !hasSkipped && isVideoPlaying)
        {
            if (Input.anyKeyDown || Input.GetMouseButtonDown(0))
            {
                SkipVideo();
            }
        }

        // Update skip prompt visibility
        if (skipPromptText != null && showSkipPrompt)
        {
            skipPromptText.gameObject.SetActive(canSkip && isVideoPlaying && !hasSkipped);
        }
    }

    #endregion

    #region Initialization

    private void InitializeComponents()
    {
        // Create or get VideoPlayer component
        videoPlayer = GetComponent<VideoPlayer>();
        if (videoPlayer == null)
        {
            videoPlayer = gameObject.AddComponent<VideoPlayer>();
        }

        // Setup video player
        videoPlayer.playOnAwake = false;
        videoPlayer.waitForFirstFrame = true;
        videoPlayer.skipOnDrop = true;

        // Setup render mode
        if (useUIDisplay && videoDisplay != null)
        {
            // UI display mode
            if (renderTexture == null)
            {
                renderTexture = new RenderTexture(1920, 1080, 0);
            }

            videoPlayer.renderMode = VideoRenderMode.RenderTexture;
            videoPlayer.targetTexture = renderTexture;
            videoDisplay.texture = renderTexture;

            if (showDebugLogs)
            {
                Debug.Log("[IntroVideo] Using UI RawImage display");
            }
        }
        else
        {
            // Camera display mode
            videoPlayer.renderMode = VideoRenderMode.CameraNearPlane;
            videoPlayer.targetCamera = Camera.main;

            if (showDebugLogs)
            {
                Debug.Log("[IntroVideo] Using Camera display");
            }
        }

        // Setup audio
        videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
        
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        videoPlayer.SetTargetAudioSource(0, audioSource);
        audioSource.volume = muteAudio ? 0f : videoVolume;

        // Assign video clip
        if (videoClip != null)
        {
            videoPlayer.clip = videoClip;
            
            if (showDebugLogs)
            {
                Debug.Log($"[IntroVideo] Video clip assigned: {videoClip.name}");
                Debug.Log($"[IntroVideo] Duration: {videoClip.length:F2} seconds");
            }
        }
        else
        {
            Debug.LogError("[IntroVideo] No video clip assigned! Skipping to next scene...");
        }

        // Setup events
        videoPlayer.loopPointReached += OnVideoFinished;
        videoPlayer.errorReceived += OnVideoError;

        // Hide skip prompt initially
        if (skipPromptText != null)
        {
            skipPromptText.gameObject.SetActive(false);
        }

        // Setup fade panel
        if (fadePanel != null && useFadeTransitions)
        {
            fadePanel.alpha = 1f; // Start fully black
            fadePanel.blocksRaycasts = true;
        }
    }

    #endregion

    #region Video Playback

    private IEnumerator PlayVideoSequence()
    {
        // Wait a frame for everything to initialize
        yield return null;

        // Fade in
        if (useFadeTransitions && fadePanel != null)
        {
            yield return StartCoroutine(FadeIn());
        }

        // Check if video clip is assigned
        if (videoClip == null || videoPlayer == null)
        {
            Debug.LogWarning("[IntroVideo] No video clip - loading next scene immediately");
            LoadNextScene();
            yield break;
        }

        // Prepare and play video
        if (showDebugLogs)
        {
            Debug.Log("[IntroVideo] Starting video playback...");
        }

        videoPlayer.Prepare();
        
        // Wait for video to be ready
        while (!videoPlayer.isPrepared)
        {
            yield return null;
        }

        // Start playing
        videoPlayer.Play();
        isVideoPlaying = true;
        videoStartTime = Time.time;

        if (showDebugLogs)
        {
            Debug.Log("[IntroVideo] Video playing");
        }

        // Enable skipping after delay
        if (allowSkip && skipDelay > 0)
        {
            yield return new WaitForSeconds(skipDelay);
            canSkip = true;
            
            if (showDebugLogs)
            {
                Debug.Log("[IntroVideo] Skip enabled");
            }
        }
        else if (allowSkip)
        {
            canSkip = true;
        }

        // Wait for video to finish (unless skipped)
        while (videoPlayer.isPlaying && !hasSkipped)
        {
            yield return null;
        }

        // Video finished or was skipped
        if (!hasSkipped)
        {
            if (showDebugLogs)
            {
                Debug.Log("[IntroVideo] Video finished naturally");
            }

            // Wait post-video delay
            if (postVideoDelay > 0)
            {
                yield return new WaitForSeconds(postVideoDelay);
            }
        }

        // Fade out and load next scene
        if (autoLoadNextScene)
        {
            if (useFadeTransitions && fadePanel != null)
            {
                yield return StartCoroutine(FadeOut());
            }

            LoadNextScene();
        }
    }

    private void SkipVideo()
    {
        if (hasSkipped) return;

        hasSkipped = true;
        isVideoPlaying = false;

        if (showDebugLogs)
        {
            float watchedTime = Time.time - videoStartTime;
            Debug.Log($"[IntroVideo] Video skipped after {watchedTime:F2} seconds");
        }

        // Stop video
        if (videoPlayer != null && videoPlayer.isPlaying)
        {
            videoPlayer.Stop();
        }

        // Hide skip prompt
        if (skipPromptText != null)
        {
            skipPromptText.gameObject.SetActive(false);
        }
    }

    #endregion

    #region Scene Loading

    private void LoadNextScene()
    {
        if (showDebugLogs)
        {
            Debug.Log($"[IntroVideo] Loading next scene: {nextSceneName}");
        }

        try
        {
            if (useSceneIndex)
            {
                SceneManager.LoadScene(nextSceneIndex);
            }
            else
            {
                SceneManager.LoadScene(nextSceneName);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[IntroVideo] Failed to load scene: {e.Message}");
            Debug.LogError($"[IntroVideo] Make sure scene '{nextSceneName}' is in Build Settings!");
        }
    }

    #endregion

    #region Fade Transitions

    private IEnumerator FadeIn()
    {
        float elapsed = 0f;

        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            fadePanel.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeInDuration);
            yield return null;
        }

        fadePanel.alpha = 0f;
        fadePanel.blocksRaycasts = false;
    }

    private IEnumerator FadeOut()
    {
        fadePanel.blocksRaycasts = true;
        float elapsed = 0f;

        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            fadePanel.alpha = Mathf.Lerp(0f, 1f, elapsed / fadeOutDuration);
            yield return null;
        }

        fadePanel.alpha = 1f;
    }

    #endregion

    #region Video Events

    private void OnVideoFinished(VideoPlayer vp)
    {
        if (showDebugLogs)
        {
            Debug.Log("[IntroVideo] Video playback completed");
        }

        isVideoPlaying = false;
    }

    private void OnVideoError(VideoPlayer vp, string message)
    {
        Debug.LogError($"[IntroVideo] Video error: {message}");
        
        // Skip to next scene on error
        if (autoLoadNextScene)
        {
            LoadNextScene();
        }
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Manually skip the video (can be called from UI button)
    /// </summary>
    public void ManualSkip()
    {
        if (canSkip)
        {
            SkipVideo();
        }
    }

    /// <summary>
    /// Set video volume (0-1)
    /// </summary>
    public void SetVolume(float volume)
    {
        videoVolume = Mathf.Clamp01(volume);
        if (audioSource != null)
        {
            audioSource.volume = muteAudio ? 0f : videoVolume;
        }
    }

    /// <summary>
    /// Mute/unmute video
    /// </summary>
    public void SetMute(bool mute)
    {
        muteAudio = mute;
        if (audioSource != null)
        {
            audioSource.volume = muteAudio ? 0f : videoVolume;
        }
    }

    #endregion

    #region Cleanup

    private void OnDestroy()
    {
        // Clean up events
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached -= OnVideoFinished;
            videoPlayer.errorReceived -= OnVideoError;
        }

        // Clean up render texture
        if (renderTexture != null && useUIDisplay)
        {
            renderTexture.Release();
        }
    }

    #endregion
}
