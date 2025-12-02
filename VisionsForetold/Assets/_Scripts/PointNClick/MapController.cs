using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

namespace VisionsForetold.PointNClick
{
    /// <summary>
    /// Main controller for the point-and-click map system
    /// Manages UI panels, area selection, and scene loading
    /// </summary>
    public class MapController : MonoBehaviour
    {
        public static MapController Instance { get; private set; }

        [Header("UI References")]
        [Tooltip("Panel showing area information")]
        [SerializeField] private GameObject infoPanel;
        
        [Tooltip("Title text in info panel")]
        [SerializeField] private TMP_Text areaTitleText;
        
        [Tooltip("Description text in info panel")]
        [SerializeField] private TMP_Text areaDescriptionText;
        
        [Tooltip("Icon image in info panel (optional)")]
        [SerializeField] private Image areaIconImage;
        
        [Tooltip("Preview image showing the area's sprite")]
        [SerializeField] private Image areaPreviewImage;
        
        [Tooltip("Button to enter the selected area")]
        [SerializeField] private Button enterButton;
        
        [Tooltip("Button to close info panel")]
        [SerializeField] private Button closeButton;

        [Header("Hover Tooltip (Optional)")]
        [Tooltip("Tooltip shown when hovering over areas")]
        [SerializeField] private GameObject hoverTooltip;
        
        [Tooltip("Tooltip text")]
        [SerializeField] private TMP_Text tooltipText;
        
        [Tooltip("Offset from mouse position")]
        [SerializeField] private Vector2 tooltipOffset = new Vector2(20f, -20f);

        [Header("Selection Indicator (For Gamepad/Keyboard)")]
        [Tooltip("Visual indicator showing selected area")]
        [SerializeField] private SelectionIndicator selectionIndicator;
        
        [Tooltip("Automatically create indicator if not assigned")]
        [SerializeField] private bool autoCreateIndicator = true;

        [Header("Gamepad/Keyboard Navigation")]
        [Tooltip("Enable gamepad and keyboard navigation")]
        [SerializeField] private bool enableGamepadNavigation = true;
        
        [Tooltip("Navigation input delay (seconds)")]
        [SerializeField] private float navigationDelay = 0.2f;
        
        [Tooltip("Auto-select first available area on enable")]
        [SerializeField] private bool autoSelectFirst = true;
        
        [Tooltip("Navigate in grid layout (true) or list (false)")]
        [SerializeField] private bool gridNavigation = true;
        
        [Tooltip("Number of columns in grid (for grid navigation)")]
        [SerializeField] private int gridColumns = 3;

        [Header("Loading Screen (Optional)")]
        [Tooltip("Loading screen to show during scene transitions")]
        [SerializeField] private GameObject loadingScreen;
        
        [Tooltip("Loading progress bar")]
        [SerializeField] private Slider loadingProgressBar;
        
        [Tooltip("Loading text")]
        [SerializeField] private TMP_Text loadingText;

        [Header("Map Settings")]
        [Tooltip("Fade duration for UI transitions")]
        [SerializeField] private float uiFadeDuration = 0.3f;
        
        [Tooltip("Automatically close info panel when entering area")]
        [SerializeField] private bool autoCloseOnEnter = true;

        [Header("Audio")]
        [Tooltip("Audio source for UI sounds")]
        [SerializeField] private AudioSource uiAudioSource;
        
        [Tooltip("Sound played when opening info panel")]
        [SerializeField] private AudioClip panelOpenSound;
        
        [Tooltip("Sound played when closing info panel")]
        [SerializeField] private AudioClip panelCloseSound;

        // Runtime state
        private AreaData currentlySelectedArea;
        private object currentlySelectedClickable; // Can be ClickableArea or ClickableAreaUI
        private CanvasGroup infoPanelCanvasGroup;
        private CanvasGroup tooltipCanvasGroup;
        
        // Gamepad navigation
        private float lastNavigationTime;
        private int currentSelectionIndex = -1;
        private object[] allClickableAreas; // Array of all clickable areas for navigation

        #region Unity Lifecycle

        private void Awake()
        {
            // Singleton pattern
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Debug.LogWarning("[MapController] Multiple instances detected! Destroying duplicate.", this);
                Destroy(gameObject);
                return;
            }

            // Setup canvas groups for fading
            SetupCanvasGroups();

            // Setup audio
            if (uiAudioSource == null)
            {
                uiAudioSource = gameObject.AddComponent<AudioSource>();
                uiAudioSource.playOnAwake = false;
            }
        }

        private void Start()
        {
            // Hide panels initially
            if (infoPanel != null)
            {
                infoPanel.SetActive(false);
            }

            if (hoverTooltip != null)
            {
                hoverTooltip.SetActive(false);
            }

            if (loadingScreen != null)
            {
                loadingScreen.SetActive(false);
            }

            // Create selection indicator if needed
            if (selectionIndicator == null && autoCreateIndicator)
            {
                CreateSelectionIndicator();
            }
            else if (selectionIndicator != null)
            {
                selectionIndicator.Hide(true);
            }

            // Setup button listeners
            if (enterButton != null)
            {
                enterButton.onClick.AddListener(OnEnterButtonClicked);
            }

            if (closeButton != null)
            {
                closeButton.onClick.AddListener(CloseInfoPanel);
            }

            // Initialize gamepad navigation
            if (enableGamepadNavigation)
            {
                InitializeNavigation();
            }

            // Check for loaded save data and return to saved area
            CheckForSaveReturn();
        }

        private void Update()
        {
            // Update tooltip position
            if (hoverTooltip != null && hoverTooltip.activeSelf)
            {
                UpdateTooltipPosition();
            }

            // Handle gamepad/keyboard navigation
            if (enableGamepadNavigation)
            {
                HandleGamepadInput();
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        #endregion

        #region Setup

        private void SetupCanvasGroups()
        {
            // Info panel
            if (infoPanel != null)
            {
                infoPanelCanvasGroup = infoPanel.GetComponent<CanvasGroup>();
                if (infoPanelCanvasGroup == null)
                {
                    infoPanelCanvasGroup = infoPanel.AddComponent<CanvasGroup>();
                }
            }

            // Tooltip
            if (hoverTooltip != null)
            {
                tooltipCanvasGroup = hoverTooltip.GetComponent<CanvasGroup>();
                if (tooltipCanvasGroup == null)
                {
                    tooltipCanvasGroup = hoverTooltip.AddComponent<CanvasGroup>();
                }
            }
        }

        private void CreateSelectionIndicator()
        {
            // Create indicator GameObject
            GameObject indicatorObj = new GameObject("SelectionIndicator");
            indicatorObj.transform.SetParent(transform);

            // Add RectTransform
            RectTransform rect = indicatorObj.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);

            // Add Image component
            Image img = indicatorObj.AddComponent<Image>();
            img.color = new Color(1f, 1f, 0f, 0.8f); // Yellow
            img.raycastTarget = false;

            // Create simple border sprite (can be replaced with custom sprite)
            // For now, use a simple colored border
            img.sprite = null; // Will show as colored rectangle

            // Add SelectionIndicator component
            selectionIndicator = indicatorObj.AddComponent<SelectionIndicator>();

            Debug.Log("[MapController] Auto-created selection indicator");
        }

        #endregion

        #region Gamepad Navigation

        private void InitializeNavigation()
        {
            // Find all clickable areas in the scene
            var clickableAreas = FindObjectsOfType<ClickableArea>();
            var clickableAreasUI = FindObjectsOfType<ClickableAreaUI>();

            // Combine into single array
            allClickableAreas = new object[clickableAreas.Length + clickableAreasUI.Length];
            
            int index = 0;
            foreach (var area in clickableAreas)
            {
                allClickableAreas[index++] = area;
            }
            foreach (var area in clickableAreasUI)
            {
                allClickableAreas[index++] = area;
            }

            // Auto-select first area if configured
            if (autoSelectFirst && allClickableAreas.Length > 0)
            {
                SelectAreaByIndex(0);
            }
        }

        private void HandleGamepadInput()
        {
            // Check if enough time has passed since last navigation
            if (Time.time - lastNavigationTime < navigationDelay) return;

            // No areas to navigate
            if (allClickableAreas == null || allClickableAreas.Length == 0) return;

            // Handle info panel open/close
            if (infoPanel != null && infoPanel.activeSelf)
            {
                // ESC / B button to close
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    CloseInfoPanel();
                    lastNavigationTime = Time.time;
                    return;
                }

                try
                {
                    if (Input.GetButtonDown("Jump"))
                    {
                        CloseInfoPanel();
                        lastNavigationTime = Time.time;
                        return;
                    }
                }
                catch (System.ArgumentException) { }

                // Enter / Space / A button to enter area
                if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Space))
                {
                    OnEnterButtonClicked();
                    lastNavigationTime = Time.time;
                    return;
                }

                try
                {
                    if (Input.GetButtonDown("Fire1"))
                    {
                        OnEnterButtonClicked();
                        lastNavigationTime = Time.time;
                        return;
                    }
                }
                catch (System.ArgumentException) { }
            }

            // Navigation input - combine axis and keys for both joystick and keyboard
            float horizontal = 0f;
            float vertical = 0f;

            try
            {
                horizontal = Input.GetAxisRaw("Horizontal");
                vertical = Input.GetAxisRaw("Vertical");
            }
            catch (System.ArgumentException) { }

            // Arrow keys and WASD
            bool left = horizontal < -0.5f || Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A);
            bool right = horizontal > 0.5f || Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D);
            bool up = vertical > 0.5f || Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W);
            bool down = vertical < -0.5f || Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S);

            // Navigate
            if (gridNavigation)
            {
                // Grid-based navigation
                if (left)
                {
                    NavigateLeft();
                    lastNavigationTime = Time.time;
                }
                else if (right)
                {
                    NavigateRight();
                    lastNavigationTime = Time.time;
                }
                else if (up)
                {
                    NavigateUp();
                    lastNavigationTime = Time.time;
                }
                else if (down)
                {
                    NavigateDown();
                    lastNavigationTime = Time.time;
                }
            }
            else
            {
                // List-based navigation
                if (left || up)
                {
                    NavigatePrevious();
                    lastNavigationTime = Time.time;
                }
                else if (right || down)
                {
                    NavigateNext();
                    lastNavigationTime = Time.time;
                }
            }

            // Enter / Space / A button to select
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Space))
            {
                SelectCurrentArea();
                lastNavigationTime = Time.time;
            }

            try
            {
                if (Input.GetButtonDown("Fire1"))
                {
                    SelectCurrentArea();
                    lastNavigationTime = Time.time;
                }
            }
            catch (System.ArgumentException) { }

            // Q/E for quick navigation
            if (Input.GetKeyDown(KeyCode.Q))
            {
                NavigatePrevious();
                lastNavigationTime = Time.time;
            }
            else if (Input.GetKeyDown(KeyCode.E))
            {
                NavigateNext();
                lastNavigationTime = Time.time;
            }

            // Shoulder buttons for quick navigation (optional)
            try
            {
                if (Input.GetButtonDown("Previous")) // L1/LB
                {
                    NavigatePrevious();
                    lastNavigationTime = Time.time;
                }
                else if (Input.GetButtonDown("Next")) // R1/RB
                {
                    NavigateNext();
                    lastNavigationTime = Time.time;
                }
            }
            catch (System.ArgumentException)
            {
                // Buttons not configured - Q/E keys already handle this above
            }
        }

        private void NavigateLeft()
        {
            if (currentSelectionIndex < 0) return;
            
            int newIndex = currentSelectionIndex - 1;
            if (newIndex < 0)
            {
                newIndex = allClickableAreas.Length - 1; // Wrap around
            }
            
            SelectAreaByIndex(newIndex);
        }

        private void NavigateRight()
        {
            if (currentSelectionIndex < 0) return;
            
            int newIndex = currentSelectionIndex + 1;
            if (newIndex >= allClickableAreas.Length)
            {
                newIndex = 0; // Wrap around
            }
            
            SelectAreaByIndex(newIndex);
        }

        private void NavigateUp()
        {
            if (currentSelectionIndex < 0 || gridColumns <= 0) return;
            
            int newIndex = currentSelectionIndex - gridColumns;
            if (newIndex < 0)
            {
                // Wrap to bottom
                int remainder = currentSelectionIndex % gridColumns;
                int lastRow = (allClickableAreas.Length - 1) / gridColumns;
                newIndex = lastRow * gridColumns + remainder;
                if (newIndex >= allClickableAreas.Length)
                {
                    newIndex -= gridColumns;
                }
            }
            
            SelectAreaByIndex(newIndex);
        }

        private void NavigateDown()
        {
            if (currentSelectionIndex < 0 || gridColumns <= 0) return;
            
            int newIndex = currentSelectionIndex + gridColumns;
            if (newIndex >= allClickableAreas.Length)
            {
                // Wrap to top
                int remainder = currentSelectionIndex % gridColumns;
                newIndex = remainder;
            }
            
            SelectAreaByIndex(newIndex);
        }

        private void NavigatePrevious()
        {
            if (currentSelectionIndex < 0) return;
            
            int newIndex = currentSelectionIndex - 1;
            if (newIndex < 0)
            {
                newIndex = allClickableAreas.Length - 1;
            }
            
            SelectAreaByIndex(newIndex);
        }

        private void NavigateNext()
        {
            if (currentSelectionIndex < 0) return;
            
            int newIndex = currentSelectionIndex + 1;
            if (newIndex >= allClickableAreas.Length)
            {
                newIndex = 0;
            }
            
            SelectAreaByIndex(newIndex);
        }

        private void SelectAreaByIndex(int index)
        {
            if (index < 0 || index >= allClickableAreas.Length) return;

            currentSelectionIndex = index;
            object clickable = allClickableAreas[index];

            // Get area data
            AreaData areaData = GetAreaDataFromClickable(clickable);
            if (areaData == null) return;

            // Update selection indicator
            if (selectionIndicator != null)
            {
                RectTransform targetRect = GetRectTransform(clickable);
                if (targetRect != null)
                {
                    selectionIndicator.ShowSelected(targetRect);
                }
            }

            // Update current selection
            currentlySelectedClickable = clickable;
            currentlySelectedArea = areaData;
        }

        private void SelectCurrentArea()
        {
            if (currentlySelectedClickable == null || currentlySelectedArea == null) return;

            // Trigger click event
            OnAreaClicked(currentlySelectedArea, currentlySelectedClickable);
        }

        private AreaData GetAreaDataFromClickable(object clickable)
        {
            if (clickable is ClickableArea ca)
            {
                return ca.GetAreaData();
            }
            else if (clickable is ClickableAreaUI cui)
            {
                return cui.GetAreaData();
            }
            return null;
        }

        #endregion

        #region Area Interaction Callbacks

        /// <summary>
        /// Called when mouse enters an area
        /// </summary>
        public void OnAreaHoverEnter(AreaData areaData)
        {
            if (areaData == null || hoverTooltip == null) return;

            // Show tooltip with area name
            if (tooltipText != null)
            {
                tooltipText.text = areaData.isUnlocked ? areaData.areaName : "Locked";
            }

            hoverTooltip.SetActive(true);
            UpdateTooltipPosition();
        }

        /// <summary>
        /// Called when mouse exits an area
        /// </summary>
        public void OnAreaHoverExit(AreaData areaData)
        {
            if (hoverTooltip != null)
            {
                hoverTooltip.SetActive(false);
            }
        }

        /// <summary>
        /// Called when an area is clicked
        /// </summary>
        public void OnAreaClicked(AreaData areaData, object clickable)
        {
            if (areaData == null) return;

            currentlySelectedArea = areaData;

            // Reset previous selection
            if (currentlySelectedClickable != null && currentlySelectedClickable != clickable)
            {
                // Try both types
                if (currentlySelectedClickable is ClickableArea ca)
                {
                    ca.ResetToIdle();
                }
                else if (currentlySelectedClickable is ClickableAreaUI cui)
                {
                    cui.ResetToIdle();
                }
            }

            currentlySelectedClickable = clickable;

            // Show selection indicator
            if (selectionIndicator != null)
            {
                RectTransform targetRect = GetRectTransform(clickable);
                if (targetRect != null)
                {
                    selectionIndicator.ShowSelected(targetRect);
                }
            }

            ShowInfoPanel(areaData);
        }

        #endregion

        #region Info Panel

        /// <summary>
        /// Show info panel with area details
        /// </summary>
        private void ShowInfoPanel(AreaData areaData)
        {
            if (infoPanel == null || areaData == null) return;

            // Update UI elements
            if (areaTitleText != null)
            {
                areaTitleText.text = areaData.areaName;
            }

            if (areaDescriptionText != null)
            {
                areaDescriptionText.text = areaData.detailedDescription;
            }

            // Update preview image with area's sprite
            if (areaPreviewImage != null)
            {
                // Use the idle sprite (or selected sprite) as preview
                Sprite previewSprite = areaData.selectedSprite ?? areaData.idleSprite;
                
                if (previewSprite != null)
                {
                    // Ensure GameObject is active BEFORE setting sprite
                    areaPreviewImage.gameObject.SetActive(true);
                    areaPreviewImage.sprite = previewSprite;
                    
                    // Optional: Preserve aspect ratio
                    areaPreviewImage.preserveAspect = true;
                }
                else
                {
                    areaPreviewImage.gameObject.SetActive(false);
                }
            }

            // Update icon (separate from preview)
            if (areaIconImage != null && areaData.areaIcon != null)
            {
                areaIconImage.sprite = areaData.areaIcon;
                areaIconImage.gameObject.SetActive(true);
            }
            else if (areaIconImage != null)
            {
                areaIconImage.gameObject.SetActive(false);
            }

            // Update enter button state
            if (enterButton != null)
            {
                enterButton.interactable = areaData.isUnlocked && !string.IsNullOrEmpty(areaData.sceneName);
            }

            // Show panel with fade
            infoPanel.SetActive(true);
            StartCoroutine(FadeCanvasGroup(infoPanelCanvasGroup, 0f, 1f, uiFadeDuration));

            // Play sound
            PlayUISound(panelOpenSound);
        }

        /// <summary>
        /// Close the info panel
        /// </summary>
        public void CloseInfoPanel()
        {
            if (infoPanel == null) return;

            StartCoroutine(FadeOutAndHide(infoPanelCanvasGroup, infoPanel, uiFadeDuration));

            // Reset selected area
            if (currentlySelectedClickable != null)
            {
                if (currentlySelectedClickable is ClickableArea ca)
                {
                    ca.ResetToIdle();
                }
                else if (currentlySelectedClickable is ClickableAreaUI cui)
                {
                    cui.ResetToIdle();
                }
            }

            // Hide selection indicator
            if (selectionIndicator != null)
            {
                selectionIndicator.Hide();
            }

            currentlySelectedArea = null;
            currentlySelectedClickable = null;

            // Play sound
            PlayUISound(panelCloseSound);
        }

        #endregion

        #region Scene Loading

        /// <summary>
        /// Enter the selected area (load scene)
        /// </summary>
        public void EnterArea(AreaData areaData)
        {
            if (areaData == null || string.IsNullOrEmpty(areaData.sceneName))
            {
                Debug.LogError("[MapController] Cannot enter area - no scene name specified!");
                return;
            }

            if (!areaData.isUnlocked)
            {
                Debug.LogWarning($"[MapController] Area '{areaData.areaName}' is locked!");
                return;
            }

            // Close info panel if configured
            if (autoCloseOnEnter && infoPanel != null)
            {
                infoPanel.SetActive(false);
            }

            // Load scene
            if (areaData.loadAsync)
            {
                StartCoroutine(LoadSceneAsync(areaData.sceneName));
            }
            else
            {
                SceneManager.LoadScene(areaData.sceneName);
            }
        }

        private void OnEnterButtonClicked()
        {
            if (currentlySelectedArea != null)
            {
                EnterArea(currentlySelectedArea);
            }
        }

        /// <summary>
        /// Load scene asynchronously with loading screen
        /// </summary>
        private IEnumerator LoadSceneAsync(string sceneName)
        {
            // Show loading screen
            if (loadingScreen != null)
            {
                loadingScreen.SetActive(true);
            }

            // Start loading
            AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
            operation.allowSceneActivation = false;

            // Update progress
            while (!operation.isDone)
            {
                float progress = Mathf.Clamp01(operation.progress / 0.9f);

                // Update UI
                if (loadingProgressBar != null)
                {
                    loadingProgressBar.value = progress;
                }

                if (loadingText != null)
                {
                    loadingText.text = $"Loading... {Mathf.Round(progress * 100)}%";
                }

                // Allow scene activation when ready
                if (operation.progress >= 0.9f)
                {
                    // Optional: Wait for player input or delay
                    yield return new WaitForSeconds(0.5f);
                    operation.allowSceneActivation = true;
                }

                yield return null;
            }
        }

        #endregion

        #region Tooltip

        private void UpdateTooltipPosition()
        {
            if (hoverTooltip == null) return;

            Vector2 mousePos = Input.mousePosition;
            hoverTooltip.transform.position = mousePos + tooltipOffset;
        }

        #endregion

        #region UI Helpers

        private IEnumerator FadeCanvasGroup(CanvasGroup canvasGroup, float from, float to, float duration)
        {
            if (canvasGroup == null) yield break;

            float elapsed = 0f;
            canvasGroup.alpha = from;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                canvasGroup.alpha = Mathf.Lerp(from, to, elapsed / duration);
                yield return null;
            }

            canvasGroup.alpha = to;
        }

        private IEnumerator FadeOutAndHide(CanvasGroup canvasGroup, GameObject panel, float duration)
        {
            yield return FadeCanvasGroup(canvasGroup, 1f, 0f, duration);
            if (panel != null)
            {
                panel.SetActive(false);
            }
        }

        private void PlayUISound(AudioClip clip)
        {
            if (clip != null && uiAudioSource != null)
            {
                uiAudioSource.PlayOneShot(clip);
            }
        }

        #endregion

        #region Public API

        /// <summary>
        /// Get currently selected area
        /// </summary>
        public AreaData GetSelectedArea() => currentlySelectedArea;

        /// <summary>
        /// Programmatically select an area
        /// </summary>
        public void SelectArea(AreaData areaData, object clickable)
        {
            OnAreaClicked(areaData, clickable);
        }

        /// <summary>
        /// Close all UI panels
        /// </summary>
        public void CloseAllPanels()
        {
            CloseInfoPanel();
            
            if (hoverTooltip != null)
            {
                hoverTooltip.SetActive(false);
            }

            if (selectionIndicator != null)
            {
                selectionIndicator.Hide(true);
            }
        }

        /// <summary>
        /// Get RectTransform from clickable object
        /// </summary>
        private RectTransform GetRectTransform(object clickable)
        {
            if (clickable is ClickableArea ca)
            {
                return ca.GetComponent<RectTransform>();
            }
            else if (clickable is ClickableAreaUI cui)
            {
                return cui.GetComponent<RectTransform>();
            }
            else if (clickable is Component comp)
            {
                return comp.GetComponent<RectTransform>();
            }

            return null;
        }

        /// <summary>
        /// Check if returning from a saved game and select the appropriate area
        /// </summary>
        private void CheckForSaveReturn()
        {
            // Get save manager
            var saveManager = VisionsForetold.Game.SaveSystem.SaveManager.Instance;
            if (saveManager == null) return;

            // Get current save data
            var saveData = saveManager.GetCurrentSaveData();
            if (saveData == null) return;

            // Check if we have a return area ID
            if (string.IsNullOrEmpty(saveData.returnAreaId))
            {
                Debug.Log("[MapController] No return area in save data");
                return;
            }

            // Find the area with matching scene name
            var area = FindAreaBySceneName(saveData.returnAreaId);
            if (area != null)
            {
                Debug.Log($"[MapController] Returning to saved area: {area.areaName}");
                
                // Auto-select this area
                StartCoroutine(SelectAreaAfterFrame(area));
            }
            else
            {
                Debug.LogWarning($"[MapController] Could not find area with scene name: {saveData.returnAreaId}");
            }
        }

        /// <summary>
        /// Find an area by its scene name
        /// </summary>
        private AreaData FindAreaBySceneName(string sceneName)
        {
            if (allClickableAreas == null) return null;

            foreach (var clickable in allClickableAreas)
            {
                AreaData areaData = GetAreaDataFromClickable(clickable);
                if (areaData != null && areaData.sceneName == sceneName)
                {
                    return areaData;
                }
            }

            return null;
        }

        /// <summary>
        /// Select area after one frame (ensures initialization is complete)
        /// </summary>
        private IEnumerator SelectAreaAfterFrame(AreaData areaData)
        {
            yield return null; // Wait one frame

            // Find the clickable for this area
            for (int i = 0; i < allClickableAreas.Length; i++)
            {
                var clickable = allClickableAreas[i];
                AreaData clickableData = GetAreaDataFromClickable(clickable);
                
                if (clickableData == areaData)
                {
                    // Select this area
                    SelectAreaByIndex(i);
                    
                    // Optionally open the info panel
                    if (infoPanel != null && !infoPanel.activeSelf)
                    {
                        ShowInfoPanel(areaData);
                    }
                    
                    break;
                }
            }
        }

        #endregion
    }
}
