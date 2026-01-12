using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// Displays a "To Be Continued" screen and returns to main menu.
/// Call ToBeContinuedManager.Instance.ShowToBeContinued() when boss is defeated.
/// </summary>
public class ToBeContinuedManager : MonoBehaviour
{
    public static ToBeContinuedManager Instance { get; private set; }

    [Header("UI References")]
    [Tooltip("Canvas with the To Be Continued UI")]
    [SerializeField] private Canvas toBeContinuedCanvas;
    
    [Tooltip("Main text showing 'To Be Continued'")]
    [SerializeField] private Text toBeContinuedText;
    
    [Tooltip("Optional subtitle text")]
    [SerializeField] private Text subtitleText;
    
    [Tooltip("Fade panel (black background)")]
    [SerializeField] private CanvasGroup fadePanel;

    [Header("Scene Settings")]
    [Tooltip("Name of the main menu scene")]
    [SerializeField] private string mainMenuSceneName = "MainMenuScene";
    
    [Tooltip("Use scene index instead of name")]
    [SerializeField] private bool useSceneIndex = false;
    
    [Tooltip("Main menu scene build index")]
    [SerializeField] private int mainMenuSceneIndex = 0;

    [Header("Display Settings")]
    [Tooltip("Text to display")]
    [SerializeField] private string displayText = "TO BE CONTINUED...";
    
    [Tooltip("Optional subtitle text")]
    [SerializeField] private string subtitle = "";
    
    [Tooltip("How long to display the screen")]
    [SerializeField] private float displayDuration = 5f;
    
    [Tooltip("Fade in duration")]
    [SerializeField] private float fadeInDuration = 2f;
    
    [Tooltip("Fade out duration")]
    [SerializeField] private float fadeOutDuration = 1f;

    [Header("Animation Settings")]
    [Tooltip("Animate text fade in")]
    [SerializeField] private bool animateText = true;
    
    [Tooltip("Text fade in duration")]
    [SerializeField] private float textFadeInDuration = 1.5f;
    
    [Tooltip("Delay before showing text")]
    [SerializeField] private float textDelay = 0.5f;

    [Header("Audio Settings")]
    [Tooltip("Sound to play when screen appears")]
    [SerializeField] private AudioClip toBeContinuedSound;
    
    [Tooltip("Music to play during screen")]
    [SerializeField] private AudioClip toBeContinuedMusic;
    
    [Tooltip("Volume for sound effect")]
    [SerializeField, Range(0f, 1f)] private float soundVolume = 1f;
    
    [Tooltip("Volume for music")]
    [SerializeField, Range(0f, 1f)] private float musicVolume = 0.5f;

    [Header("Input Settings")]
    [Tooltip("Allow skipping with any key")]
    [SerializeField] private bool allowSkip = true;
    
    [Tooltip("Skip prompt text")]
    [SerializeField] private Text skipPromptText;
    
    [Tooltip("Delay before allowing skip")]
    [SerializeField] private float skipDelay = 2f;

    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;

    private bool isShowing = false;
    private bool canSkip = false;
    private float showStartTime;
    private AudioSource audioSource;

    #region Initialization

    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Persist across scenes
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        InitializeComponents();
    }

    private void InitializeComponents()
    {
        // Get or create audio source
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Hide UI initially
        if (toBeContinuedCanvas != null)
        {
            toBeContinuedCanvas.gameObject.SetActive(false);
        }

        if (fadePanel != null)
        {
            fadePanel.alpha = 0f;
            fadePanel.blocksRaycasts = false;
        }

        if (skipPromptText != null)
        {
            skipPromptText.gameObject.SetActive(false);
        }

        if (showDebugLogs)
        {
            Debug.Log("[ToBeContinued] Manager initialized");
        }
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Show the "To Be Continued" screen and return to main menu
    /// </summary>
    public void ShowToBeContinued()
    {
        if (isShowing)
        {
            Debug.LogWarning("[ToBeContinued] Already showing!");
            return;
        }

        if (showDebugLogs)
        {
            Debug.Log("[ToBeContinued] Showing 'To Be Continued' screen...");
        }

        StartCoroutine(ToBeContinuedSequence());
    }

    /// <summary>
    /// Show with custom text
    /// </summary>
    public void ShowToBeContinued(string customText, string customSubtitle = "")
    {
        displayText = customText;
        subtitle = customSubtitle;
        ShowToBeContinued();
    }

    #endregion

    #region Coroutines

    private IEnumerator ToBeContinuedSequence()
    {
        isShowing = true;
        canSkip = false;
        showStartTime = Time.time;

        // Stop game
        Time.timeScale = 0f;

        // Show canvas
        if (toBeContinuedCanvas != null)
        {
            toBeContinuedCanvas.gameObject.SetActive(true);
        }

        // Pause game music (if AudioManager supports it)
        // Note: Uncomment if your AudioManager has these methods
        // if (AudioManager.Instance != null)
        // {
        //     AudioManager.Instance.PauseMusic();
        // }

        // Play sound effect
        if (toBeContinuedSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(toBeContinuedSound, soundVolume);
        }

        // Play music
        if (toBeContinuedMusic != null && audioSource != null)
        {
            audioSource.clip = toBeContinuedMusic;
            audioSource.volume = musicVolume;
            audioSource.loop = true;
            audioSource.Play();
        }

        // Fade in background
        if (fadePanel != null)
        {
            fadePanel.blocksRaycasts = true;
            yield return StartCoroutine(FadeIn());
        }

        // Show text
        if (animateText)
        {
            yield return StartCoroutine(AnimateTextIn());
        }
        else
        {
            ShowTextImmediate();
        }

        // Enable skip after delay
        if (allowSkip)
        {
            yield return new WaitForSecondsRealtime(skipDelay);
            canSkip = true;
            
            if (skipPromptText != null)
            {
                skipPromptText.gameObject.SetActive(true);
            }
            
            if (showDebugLogs)
            {
                Debug.Log("[ToBeContinued] Skip enabled");
            }
        }

        // Wait for duration or skip
        float elapsed = 0f;
        while (elapsed < displayDuration)
        {
            // Check for skip input
            if (canSkip && (Input.anyKeyDown || Input.GetMouseButtonDown(0)))
            {
                if (showDebugLogs)
                {
                    Debug.Log("[ToBeContinued] Skipped by player");
                }
                break;
            }

            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        // Hide skip prompt
        if (skipPromptText != null)
        {
            skipPromptText.gameObject.SetActive(false);
        }

        // Fade out
        if (fadePanel != null)
        {
            yield return StartCoroutine(FadeOut());
        }

        // Stop music
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }

        // Resume time
        Time.timeScale = 1f;

        // Load main menu
        LoadMainMenu();
    }

    private IEnumerator FadeIn()
    {
        float elapsed = 0f;

        while (elapsed < fadeInDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            fadePanel.alpha = Mathf.Lerp(0f, 1f, elapsed / fadeInDuration);
            yield return null;
        }

        fadePanel.alpha = 1f;
    }

    private IEnumerator FadeOut()
    {
        float elapsed = 0f;
        float startAlpha = fadePanel.alpha;

        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            fadePanel.alpha = Mathf.Lerp(startAlpha, 0f, elapsed / fadeOutDuration);
            yield return null;
        }

        fadePanel.alpha = 0f;
        fadePanel.blocksRaycasts = false;
    }

    private IEnumerator AnimateTextIn()
    {
        // Wait for delay
        if (textDelay > 0)
        {
            yield return new WaitForSecondsRealtime(textDelay);
        }

        // Set text content
        if (toBeContinuedText != null)
        {
            toBeContinuedText.text = displayText;
        }

        if (subtitleText != null && !string.IsNullOrEmpty(subtitle))
        {
            subtitleText.text = subtitle;
        }

        // Animate text fade in
        float elapsed = 0f;
        Color textColor = toBeContinuedText != null ? toBeContinuedText.color : Color.white;
        Color subtitleColor = subtitleText != null ? subtitleText.color : Color.white;

        while (elapsed < textFadeInDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float alpha = Mathf.Lerp(0f, 1f, elapsed / textFadeInDuration);

            if (toBeContinuedText != null)
            {
                textColor.a = alpha;
                toBeContinuedText.color = textColor;
            }

            if (subtitleText != null && !string.IsNullOrEmpty(subtitle))
            {
                subtitleColor.a = alpha;
                subtitleText.color = subtitleColor;
            }

            yield return null;
        }

        // Ensure full opacity
        if (toBeContinuedText != null)
        {
            textColor.a = 1f;
            toBeContinuedText.color = textColor;
        }

        if (subtitleText != null && !string.IsNullOrEmpty(subtitle))
        {
            subtitleColor.a = 1f;
            subtitleText.color = subtitleColor;
        }
    }

    private void ShowTextImmediate()
    {
        if (toBeContinuedText != null)
        {
            toBeContinuedText.text = displayText;
            Color color = toBeContinuedText.color;
            color.a = 1f;
            toBeContinuedText.color = color;
        }

        if (subtitleText != null && !string.IsNullOrEmpty(subtitle))
        {
            subtitleText.text = subtitle;
            Color color = subtitleText.color;
            color.a = 1f;
            subtitleText.color = color;
        }
    }

    #endregion

    #region Scene Loading

    private void LoadMainMenu()
    {
        if (showDebugLogs)
        {
            Debug.Log($"[ToBeContinued] Loading main menu: {mainMenuSceneName}");
        }

        try
        {
            // Hide UI
            if (toBeContinuedCanvas != null)
            {
                toBeContinuedCanvas.gameObject.SetActive(false);
            }

            // Reset state
            isShowing = false;
            canSkip = false;

            // Resume music (if AudioManager supports it)
            // Note: Uncomment if your AudioManager has these methods
            // if (AudioManager.Instance != null)
            // {
            //     AudioManager.Instance.ResumeMusic();
            // }

            // Load scene
            if (useSceneIndex)
            {
                SceneManager.LoadScene(mainMenuSceneIndex);
            }
            else
            {
                SceneManager.LoadScene(mainMenuSceneName);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[ToBeContinued] Failed to load main menu: {e.Message}");
            Debug.LogError($"[ToBeContinued] Make sure '{mainMenuSceneName}' is in Build Settings!");
        }
    }

    #endregion

    #region Cleanup

    private void OnDestroy()
    {
        // Resume time scale on destroy
        Time.timeScale = 1f;

        if (Instance == this)
        {
            Instance = null;
        }
    }

    #endregion
}
