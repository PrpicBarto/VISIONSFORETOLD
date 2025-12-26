using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using VisionsForetold.Game.SaveSystem;

/// <summary>
/// Pause Menu - For in-game pause functionality
/// Press ESC or Start/Options button to pause
/// Full joystick/gamepad and keyboard/mouse support
/// </summary>
public class PauseMenu : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject pauseMenuPanel;
    
    [Tooltip("First button to select when paused (for gamepad)")]
    [SerializeField] private GameObject firstSelectedButton;

    [Header("Settings")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";
    [SerializeField] private bool pauseOnStart = false;
    [SerializeField] private KeyCode pauseKey = KeyCode.Escape;

    [Header("Gamepad Settings")]
    [Tooltip("Allow Start/Options button to pause")]
    [SerializeField] private bool allowGamepadPause = true;
    
    [Tooltip("Auto-detect input method and switch cursor")]
    [SerializeField] private bool autoDetectInputMethod = true;

    [Header("Time Settings")]
    [SerializeField] private bool freezeTimeWhenPaused = true;

    [Header("Cursor Settings")]
    [Tooltip("Show cursor when paused")]
    [SerializeField] private bool showCursorWhenPaused = true;
    
    [Tooltip("Lock cursor when unpaused (for FPS games)")]
    [SerializeField] private bool lockCursorWhenUnpaused = false;

    private bool isPaused = false;
    private float previousTimeScale = 1f;
    private EventSystem eventSystem;
    
    // Input detection
    private bool isUsingGamepad = false;
    private float lastGamepadInputTime;
    private float lastMouseInputTime;
    private Vector2 lastMousePosition;

    private void Awake()
    {
        eventSystem = EventSystem.current;
        if (eventSystem == null)
        {
            GameObject eventSystemObj = new GameObject("EventSystem");
            eventSystem = eventSystemObj.AddComponent<EventSystem>();
            eventSystemObj.AddComponent<StandaloneInputModule>();
        }
    }

    private void Start()
    {
        // Hide pause menu initially
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(false);
            Debug.Log("[PauseMenu] Pause menu panel found and hidden");
        }
        else
        {
            Debug.LogError("[PauseMenu] Pause menu panel is NOT assigned! Please assign it in the Inspector.");
        }

        // Auto-find first selected button if not assigned
        if (firstSelectedButton == null && pauseMenuPanel != null)
        {
            Button[] buttons = pauseMenuPanel.GetComponentsInChildren<Button>(true);
            if (buttons.Length > 0)
            {
                firstSelectedButton = buttons[0].gameObject;
                Debug.Log($"[PauseMenu] Auto-found first button: {firstSelectedButton.name}");
            }
        }

        if (pauseOnStart)
        {
            Pause();
        }
        else
        {
            Resume();
        }
        
        Debug.Log("[PauseMenu] Initialized - Press ESC or Start to pause");
    }

    private void Update()
    {
        HandlePauseInput();
        
        if (isPaused)
        {
            HandleMenuInput();
            
            if (autoDetectInputMethod)
            {
                DetectInputMethod();
            }
        }
    }

    #region Input Handling

    private void HandlePauseInput()
    {
        bool pausePressed = false;

        // Check if save station menu is open - don't allow pausing
        SaveStationMenu saveStation = FindObjectOfType<SaveStationMenu>();
        if (saveStation != null)
        {
            // Check if save station menu panel is active
            if (saveStation.gameObject.activeInHierarchy)
            {
                Debug.Log("[PauseMenu] Save station is open - pause blocked");
                return; // Don't allow pause while in save station
            }
        }

        // Keyboard ESC - New Input System
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            pausePressed = true;
            Debug.Log("[PauseMenu] ESC key pressed");
        }

        // Gamepad Start/Options button
        if (allowGamepadPause && Gamepad.current != null)
        {
            if (Gamepad.current.startButton.wasPressedThisFrame)
            {
                pausePressed = true;
                lastGamepadInputTime = Time.unscaledTime;
                Debug.Log("[PauseMenu] Start button pressed");
            }
        }

        if (pausePressed)
        {
            if (isPaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }
    }

    private void HandleMenuInput()
    {
        if (!isPaused) return;

        // Gamepad B/Circle to resume
        if (Gamepad.current != null)
        {
            if (Gamepad.current.buttonEast.wasPressedThisFrame)
            {
                Resume();
                lastGamepadInputTime = Time.unscaledTime;
            }

            // Gamepad A/Cross to confirm selection
            if (Gamepad.current.buttonSouth.wasPressedThisFrame)
            {
                lastGamepadInputTime = Time.unscaledTime;
                
                if (eventSystem != null && eventSystem.currentSelectedGameObject != null)
                {
                    Button button = eventSystem.currentSelectedGameObject.GetComponent<Button>();
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
        // Check for mouse movement (use unscaled time since game is paused)
        Vector2 currentMousePosition = Mouse.current != null ? Mouse.current.position.ReadValue() : Vector2.zero;
        if (Vector2.Distance(currentMousePosition, lastMousePosition) > 1f)
        {
            lastMouseInputTime = Time.unscaledTime;
            lastMousePosition = currentMousePosition;
        }

        // Check for gamepad input
        if (Gamepad.current != null)
        {
            Vector2 leftStick = Gamepad.current.leftStick.ReadValue();
            Vector2 dpad = Gamepad.current.dpad.ReadValue();
            
            if (leftStick.magnitude > 0.2f || dpad.magnitude > 0.1f)
            {
                lastGamepadInputTime = Time.unscaledTime;
            }
        }

        // Determine input method
        bool wasUsingGamepad = isUsingGamepad;
        float timeSinceGamepad = Time.unscaledTime - lastGamepadInputTime;
        float timeSinceMouse = Time.unscaledTime - lastMouseInputTime;
        
        if (timeSinceGamepad < 0.2f)
        {
            isUsingGamepad = true;
        }
        else if (timeSinceMouse < 0.2f)
        {
            isUsingGamepad = false;
        }

        // Update cursor visibility
        if (wasUsingGamepad != isUsingGamepad && isPaused)
        {
            UpdateCursorForPauseMenu();
            
            // Reselect button for gamepad
            if (isUsingGamepad && eventSystem != null)
            {
                if (eventSystem.currentSelectedGameObject == null)
                {
                    SelectFirstButton();
                }
            }
        }
    }

    #endregion

    #region Pause/Resume

    public void Pause()
    {
        isPaused = true;

        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(true);
        }

        if (freezeTimeWhenPaused)
        {
            previousTimeScale = Time.timeScale;
            Time.timeScale = 0f;
        }

        UpdateCursorForPauseMenu();
        SelectFirstButton();

        Debug.Log("[PauseMenu] Game paused");
    }

    public void Resume()
    {
        isPaused = false;

        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(false);
        }

        if (freezeTimeWhenPaused)
        {
            Time.timeScale = previousTimeScale;
        }

        UpdateCursorForGameplay();

        Debug.Log("[PauseMenu] Game resumed");
    }

    private void SelectFirstButton()
    {
        if (firstSelectedButton != null && eventSystem != null)
        {
            eventSystem.SetSelectedGameObject(null);
            eventSystem.SetSelectedGameObject(firstSelectedButton);
        }
    }

    private void UpdateCursorForPauseMenu()
    {
        if (showCursorWhenPaused || !isUsingGamepad)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            Cursor.visible = false;
        }
    }

    private void UpdateCursorForGameplay()
    {
        if (lockCursorWhenUnpaused)
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
        else
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
    }

    #endregion

    #region Button Actions

    /// <summary>
    /// Resume button - Continue playing
    /// Call from Resume button OnClick
    /// </summary>
    public void OnResumeButton()
    {
        Resume();
    }

    /// <summary>
    /// Restart button - Reload current scene
    /// Call from Restart button OnClick
    /// </summary>
    public void OnRestartButton()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    /// <summary>
    /// Main Menu button - Return to main menu
    /// Call from Main Menu button OnClick
    /// </summary>
    public void OnMainMenuButton()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }

    /// <summary>
    /// Quit button - Exit game
    /// Call from Quit button OnClick
    /// </summary>
    public void OnQuitButton()
    {
        Time.timeScale = 1f;

        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }

    #endregion

    #region Public API

    public bool IsPaused => isPaused;

    public void SetPaused(bool paused)
    {
        if (paused)
        {
            Pause();
        }
        else
        {
            Resume();
        }
    }

    public bool IsUsingGamepad()
    {
        return isUsingGamepad;
    }

    #endregion
}
