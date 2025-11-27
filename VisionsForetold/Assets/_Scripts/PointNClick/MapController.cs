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
        }

        private void Update()
        {
            // Update tooltip position
            if (hoverTooltip != null && hoverTooltip.activeSelf)
            {
                UpdateTooltipPosition();
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

        #endregion
    }
}
