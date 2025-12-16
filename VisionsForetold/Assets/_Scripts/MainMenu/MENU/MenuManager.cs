using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using System.Collections;

/// <summary>
/// Main Menu Manager - Handles main menu navigation and scene loading
/// Includes Play, Options, and Exit functionality
/// Full joystick/gamepad and keyboard/mouse support
/// </summary>
public class MenuManager : MonoBehaviour
{
    [Header("Menu Panels")]
    [Tooltip("Main menu panel with Play/Options/Exit buttons")]
    [SerializeField] private GameObject mainMenuPanel;
    
    [Tooltip("Options menu panel")]
    [SerializeField] private GameObject optionsMenuPanel;
    
    [Tooltip("Credits panel (optional)")]
    [SerializeField] private GameObject creditsPanel;

    [Header("First Selected Buttons")]
    [Tooltip("First button to select in main menu (for gamepad)")]
    [SerializeField] private GameObject mainMenuFirstButton;
    
    [Tooltip("First button to select in options menu (for gamepad)")]
    [SerializeField] private GameObject optionsMenuFirstButton;
    
    [Tooltip("First button to select in credits (for gamepad)")]
    [SerializeField] private GameObject creditsFirstButton;

    [Header("Scene Settings")]
    [Tooltip("Name of the scene to load when Play is pressed")]
    [SerializeField] private string gameSceneName = "PointNClickScene";
    
    [Tooltip("Use scene index instead of name")]
    [SerializeField] private bool useSceneIndex = false;
    
    [Tooltip("Scene build index (if useSceneIndex is true)")]
    [SerializeField] private int gameSceneIndex = 1;

    [Header("Transition Settings")]
    [Tooltip("Enable fade transition when loading scene")]
    [SerializeField] private bool useFadeTransition = true;
    
    [Tooltip("Fade duration in seconds")]
    [SerializeField] private float fadeDuration = 1f;
    
    [Tooltip("Fade panel (assign a black Image UI element)")]
    [SerializeField] private CanvasGroup fadePanel;

    [Header("Audio Settings")]
    [Tooltip("Play sound on button click")]
    [SerializeField] private bool playButtonSounds = true;
    
    [Tooltip("Button click sound (will be integrated with AudioManager later)")]
    [SerializeField] private AudioClip buttonClickSound;
    
    [Tooltip("AudioSource for menu sounds")]
    [SerializeField] private AudioSource menuAudioSource;

    [Header("Input Settings")]
    [Tooltip("Allow ESC key to go back in menus")]
    [SerializeField] private bool allowEscapeKey = true;
    
    [Tooltip("Gamepad back button (usually B/Circle)")]
    [SerializeField] private bool allowGamepadBack = true;
    
    [Tooltip("Auto-detect input method and switch cursor visibility")]
    [SerializeField] private bool autoDetectInputMethod = true;
    
    [Tooltip("Time before switching input method")]
    [SerializeField] private float inputSwitchDelay = 0.2f;

    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;

    // State tracking
    private bool isTransitioning = false;
    private GameObject currentActivePanel;
    private EventSystem eventSystem;
    
    // Input detection
    private bool isUsingGamepad = false;
    private float lastGamepadInputTime;
    private float lastMouseInputTime;
    private Vector2 lastMousePosition;

    #region Unity Lifecycle

    private void Awake()
    {
        InitializeMenu();
    }

    private void Start()
    {
        ShowMainMenu();
    }

    private void Update()
    {
        HandleInput();
        
        if (autoDetectInputMethod)
        {
            DetectInputMethod();
        }
    }

    #endregion

    #region Initialization

    private void InitializeMenu()
    {
        // Get or create EventSystem
        eventSystem = EventSystem.current;
        if (eventSystem == null)
        {
            GameObject eventSystemObj = new GameObject("EventSystem");
            eventSystem = eventSystemObj.AddComponent<EventSystem>();
            eventSystemObj.AddComponent<StandaloneInputModule>();
            
            if (showDebugLogs)
            {
                Debug.Log("[MenuManager] Created EventSystem");
            }
        }

        // Ensure cursor is visible and unlocked in menu
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        // Create fade panel if it doesn't exist
        if (useFadeTransition && fadePanel == null)
        {
            CreateFadePanel();
        }

        // Set fade panel to transparent initially
        if (fadePanel != null)
        {
            fadePanel.alpha = 0f;
            fadePanel.blocksRaycasts = false;
        }

        // Create AudioSource if needed
        if (menuAudioSource == null && playButtonSounds)
        {
            menuAudioSource = gameObject.AddComponent<AudioSource>();
            menuAudioSource.playOnAwake = false;
        }

        // Auto-find first selected buttons if not assigned
        AutoFindFirstSelectedButtons();

        if (showDebugLogs)
        {
            Debug.Log("[MenuManager] Menu initialized successfully");
            Debug.Log("[MenuManager] Gamepad support: ENABLED");
        }
    }

    private void AutoFindFirstSelectedButtons()
    {
        // Main menu first button
        if (mainMenuFirstButton == null && mainMenuPanel != null)
        {
            Button[] buttons = mainMenuPanel.GetComponentsInChildren<Button>(true);
            if (buttons.Length > 0)
            {
                mainMenuFirstButton = buttons[0].gameObject;
                if (showDebugLogs)
                {
                    Debug.Log($"[MenuManager] Auto-found main menu first button: {mainMenuFirstButton.name}");
                }
            }
        }

        // Options menu first button
        if (optionsMenuFirstButton == null && optionsMenuPanel != null)
        {
            Button[] buttons = optionsMenuPanel.GetComponentsInChildren<Button>(true);
            if (buttons.Length > 0)
            {
                optionsMenuFirstButton = buttons[0].gameObject;
                if (showDebugLogs)
                {
                    Debug.Log($"[MenuManager] Auto-found options menu first button: {optionsMenuFirstButton.name}");
                }
            }
        }

        // Credits first button
        if (creditsFirstButton == null && creditsPanel != null)
        {
            Button[] buttons = creditsPanel.GetComponentsInChildren<Button>(true);
            if (buttons.Length > 0)
            {
                creditsFirstButton = buttons[0].gameObject;
            }
        }
    }

    private void CreateFadePanel()
    {
        // Find or create fade panel
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas != null)
        {
            GameObject fadePanelObj = new GameObject("FadePanel");
            fadePanelObj.transform.SetParent(canvas.transform, false);

            RectTransform rectTransform = fadePanelObj.AddComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.sizeDelta = Vector2.zero;

            Image image = fadePanelObj.AddComponent<Image>();
            image.color = Color.black;

            fadePanel = fadePanelObj.AddComponent<CanvasGroup>();
            fadePanel.alpha = 0f;
            fadePanel.blocksRaycasts = false;

            // Move to front
            fadePanelObj.transform.SetAsLastSibling();

            if (showDebugLogs)
            {
                Debug.Log("[MenuManager] Created fade panel automatically");
            }
        }
    }

    #endregion

    #region Menu Navigation

    /// <summary>
    /// Show the main menu panel
    /// </summary>
    public void ShowMainMenu()
    {
        SetActivePanel(mainMenuPanel);
        SelectFirstButton(mainMenuFirstButton);
        
        if (showDebugLogs)
        {
            Debug.Log("[MenuManager] Showing main menu");
        }
    }

    /// <summary>
    /// Show the options menu panel
    /// </summary>
    public void ShowOptionsMenu()
    {
        PlayButtonSound();
        SetActivePanel(optionsMenuPanel);
        SelectFirstButton(optionsMenuFirstButton);
        
        if (showDebugLogs)
        {
            Debug.Log("[MenuManager] Showing options menu");
        }
    }

    /// <summary>
    /// Show the credits panel
    /// </summary>
    public void ShowCredits()
    {
        PlayButtonSound();
        SetActivePanel(creditsPanel);
        SelectFirstButton(creditsFirstButton);
        
        if (showDebugLogs)
        {
            Debug.Log("[MenuManager] Showing credits");
        }
    }

    /// <summary>
    /// Go back to previous menu
    /// </summary>
    public void GoBack()
    {
        PlayButtonSound();
        ShowMainMenu();
    }

    private void SetActivePanel(GameObject panelToShow)
    {
        // Hide all panels
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (optionsMenuPanel != null) optionsMenuPanel.SetActive(false);
        if (creditsPanel != null) creditsPanel.SetActive(false);

        // Show requested panel
        if (panelToShow != null)
        {
            panelToShow.SetActive(true);
            currentActivePanel = panelToShow;
        }
    }

    /// <summary>
    /// Select a button for gamepad navigation
    /// </summary>
    private void SelectFirstButton(GameObject buttonToSelect)
    {
        if (buttonToSelect != null && eventSystem != null)
        {
            eventSystem.SetSelectedGameObject(null);
            eventSystem.SetSelectedGameObject(buttonToSelect);
        }
    }

    #endregion

    #region Button Actions

    /// <summary>
    /// Play button - Load the game scene
    /// Call this from your Play button's OnClick event
    /// </summary>
    public void OnPlayButton()
    {
        if (isTransitioning) return;

        PlayButtonSound();
        
        if (showDebugLogs)
        {
            Debug.Log($"[MenuManager] Play button pressed - Loading scene: {gameSceneName}");
        }

        LoadGameScene();
    }

    /// <summary>
    /// Options button - Show options menu
    /// Call this from your Options button's OnClick event
    /// </summary>
    public void OnOptionsButton()
    {
        if (isTransitioning) return;
        ShowOptionsMenu();
    }

    /// <summary>
    /// Credits button - Show credits
    /// Call this from your Credits button's OnClick event
    /// </summary>
    public void OnCreditsButton()
    {
        if (isTransitioning) return;
        ShowCredits();
    }

    /// <summary>
    /// Exit button - Quit the game
    /// Call this from your Exit button's OnClick event
    /// </summary>
    public void OnExitButton()
    {
        if (isTransitioning) return;

        PlayButtonSound();
        
        if (showDebugLogs)
        {
            Debug.Log("[MenuManager] Exit button pressed - Quitting game");
        }

        QuitGame();
    }

    /// <summary>
    /// Back button - Return to main menu
    /// Call this from your Back button's OnClick event
    /// </summary>
    public void OnBackButton()
    {
        if (isTransitioning) return;
        GoBack();
    }

    #endregion

    #region Scene Loading

    private void LoadGameScene()
    {
        if (useFadeTransition && fadePanel != null)
        {
            StartCoroutine(LoadSceneWithFade());
        }
        else
        {
            LoadSceneImmediate();
        }
    }

    private void LoadSceneImmediate()
    {
        try
        {
            if (useSceneIndex)
            {
                SceneManager.LoadScene(gameSceneIndex);
            }
            else
            {
                SceneManager.LoadScene(gameSceneName);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[MenuManager] Failed to load scene: {e.Message}");
            Debug.LogError($"[MenuManager] Make sure scene '{gameSceneName}' is added to Build Settings!");
        }
    }

    private IEnumerator LoadSceneWithFade()
    {
        isTransitioning = true;

        // Fade out
        yield return StartCoroutine(Fade(0f, 1f, fadeDuration));

        // Load scene
        AsyncOperation asyncLoad = null;
        bool loadFailed = false;
        
        // Try to start loading
        if (useSceneIndex)
        {
            if (gameSceneIndex < SceneManager.sceneCountInBuildSettings)
            {
                asyncLoad = SceneManager.LoadSceneAsync(gameSceneIndex);
            }
            else
            {
                Debug.LogError($"[MenuManager] Scene index {gameSceneIndex} is out of range!");
                loadFailed = true;
            }
        }
        else
        {
            if (IsSceneInBuild(gameSceneName))
            {
                asyncLoad = SceneManager.LoadSceneAsync(gameSceneName);
            }
            else
            {
                Debug.LogError($"[MenuManager] Scene '{gameSceneName}' not found in Build Settings!");
                loadFailed = true;
            }
        }

        if (loadFailed)
        {
            // Fade back in if loading failed
            yield return StartCoroutine(Fade(1f, 0f, fadeDuration));
            isTransitioning = false;
            yield break;
        }

        // Wait until scene is loaded
        while (asyncLoad != null && !asyncLoad.isDone)
        {
            yield return null;
        }

        isTransitioning = false;
    }

    private IEnumerator Fade(float startAlpha, float endAlpha, float duration)
    {
        if (fadePanel == null) yield break;

        fadePanel.blocksRaycasts = true;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            fadePanel.alpha = Mathf.Lerp(startAlpha, endAlpha, elapsed / duration);
            yield return null;
        }

        fadePanel.alpha = endAlpha;
        fadePanel.blocksRaycasts = endAlpha > 0.5f;
    }

    #endregion

    #region Game Exit

    private void QuitGame()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        if (showDebugLogs)
        {
            Debug.Log("[MenuManager] Quit game (Editor mode)");
        }
        #else
        Application.Quit();
        if (showDebugLogs)
        {
            Debug.Log("[MenuManager] Quit game");
        }
        #endif
    }

    #endregion

    #region Input Handling

    private void HandleInput()
    {
        if (isTransitioning) return;

        // ESC key or gamepad B/Circle to go back
        bool backPressed = false;
        
        if (allowEscapeKey && Input.GetKeyDown(KeyCode.Escape))
        {
            backPressed = true;
        }

        if (allowGamepadBack)
        {
            // Gamepad back button (B/Circle)
            if (Gamepad.current != null)
            {
                if (Gamepad.current.buttonEast.wasPressedThisFrame)
                {
                    backPressed = true;
                    lastGamepadInputTime = Time.time;
                }
            }
        }

        if (backPressed)
        {
            if (currentActivePanel == optionsMenuPanel || currentActivePanel == creditsPanel)
            {
                GoBack();
            }
        }

        // Gamepad submit/confirm (A/Cross)
        if (Gamepad.current != null && eventSystem != null)
        {
            if (Gamepad.current.buttonSouth.wasPressedThisFrame)
            {
                lastGamepadInputTime = Time.time;
                
                // Simulate clicking the currently selected button
                GameObject selected = eventSystem.currentSelectedGameObject;
                if (selected != null)
                {
                    Button button = selected.GetComponent<Button>();
                    if (button != null && button.interactable)
                    {
                        button.onClick.Invoke();
                    }
                }
            }
        }
    }

    private void DetectInputMethod()
    {
        // Check for mouse movement
        Vector2 currentMousePosition = Mouse.current != null ? Mouse.current.position.ReadValue() : Vector2.zero;
        if (Vector2.Distance(currentMousePosition, lastMousePosition) > 1f)
        {
            lastMouseInputTime = Time.time;
            lastMousePosition = currentMousePosition;
        }

        // Check for gamepad input
        if (Gamepad.current != null)
        {
            // Check any gamepad stick movement
            Vector2 leftStick = Gamepad.current.leftStick.ReadValue();
            Vector2 rightStick = Gamepad.current.rightStick.ReadValue();
            Vector2 dpad = Gamepad.current.dpad.ReadValue();
            
            if (leftStick.magnitude > 0.2f || rightStick.magnitude > 0.2f || dpad.magnitude > 0.1f)
            {
                lastGamepadInputTime = Time.time;
            }

            // Check any button press
            if (Gamepad.current.buttonSouth.wasPressedThisFrame ||
                Gamepad.current.buttonNorth.wasPressedThisFrame ||
                Gamepad.current.buttonEast.wasPressedThisFrame ||
                Gamepad.current.buttonWest.wasPressedThisFrame)
            {
                lastGamepadInputTime = Time.time;
            }
        }

        // Determine which input method is active
        bool wasUsingGamepad = isUsingGamepad;
        
        float timeSinceGamepad = Time.time - lastGamepadInputTime;
        float timeSinceMouse = Time.time - lastMouseInputTime;
        
        if (timeSinceGamepad < inputSwitchDelay)
        {
            isUsingGamepad = true;
        }
        else if (timeSinceMouse < inputSwitchDelay)
        {
            isUsingGamepad = false;
        }

        // Update cursor visibility when input method changes
        if (wasUsingGamepad != isUsingGamepad)
        {
            UpdateCursorVisibility();
            
            if (showDebugLogs)
            {
                Debug.Log($"[MenuManager] Input method: {(isUsingGamepad ? "Gamepad" : "Mouse/Keyboard")}");
            }

            // Re-select button when switching to gamepad
            if (isUsingGamepad && eventSystem != null)
            {
                if (eventSystem.currentSelectedGameObject == null)
                {
                    if (currentActivePanel == mainMenuPanel)
                    {
                        SelectFirstButton(mainMenuFirstButton);
                    }
                    else if (currentActivePanel == optionsMenuPanel)
                    {
                        SelectFirstButton(optionsMenuFirstButton);
                    }
                    else if (currentActivePanel == creditsPanel)
                    {
                        SelectFirstButton(creditsFirstButton);
                    }
                }
            }
        }
    }

    private void UpdateCursorVisibility()
    {
        if (isUsingGamepad)
        {
            // Hide cursor for gamepad
            Cursor.visible = false;
        }
        else
        {
            // Show cursor for mouse
            Cursor.visible = true;
        }
    }

    #endregion

    #region Audio

    private void PlayButtonSound()
    {
        if (!playButtonSounds || menuAudioSource == null || buttonClickSound == null)
            return;

        menuAudioSource.PlayOneShot(buttonClickSound);
    }

    /// <summary>
    /// Set button click sound (for use with AudioManager later)
    /// </summary>
    public void SetButtonClickSound(AudioClip clip)
    {
        buttonClickSound = clip;
    }

    #endregion

    #region Public Utility Methods

    /// <summary>
    /// Load a specific scene by name
    /// </summary>
    public void LoadScene(string sceneName)
    {
        if (isTransitioning) return;

        PlayButtonSound();
        gameSceneName = sceneName;
        useSceneIndex = false;
        LoadGameScene();
    }

    /// <summary>
    /// Load a specific scene by index
    /// </summary>
    public void LoadSceneByIndex(int sceneIndex)
    {
        if (isTransitioning) return;

        PlayButtonSound();
        gameSceneIndex = sceneIndex;
        useSceneIndex = true;
        LoadGameScene();
    }

    /// <summary>
    /// Check if a scene exists in build settings
    /// </summary>
    public bool IsSceneInBuild(string sceneName)
    {
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
            string sceneNameInBuild = System.IO.Path.GetFileNameWithoutExtension(scenePath);
            
            if (sceneNameInBuild == sceneName)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Force cursor visibility (useful for debugging)
    /// </summary>
    public void SetCursorVisible(bool visible)
    {
        Cursor.visible = visible;
    }

    /// <summary>
    /// Get current input method
    /// </summary>
    public bool IsUsingGamepad()
    {
        return isUsingGamepad;
    }

    #endregion

    #region Debug

    private void OnValidate()
    {
        fadeDuration = Mathf.Max(0.1f, fadeDuration);
        gameSceneIndex = Mathf.Max(0, gameSceneIndex);
        inputSwitchDelay = Mathf.Max(0.05f, inputSwitchDelay);
    }

    #endregion
}
