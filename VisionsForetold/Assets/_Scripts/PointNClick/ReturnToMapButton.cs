using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

namespace VisionsForetold.PointNClick
{
    /// <summary>
    /// Close/Return to Map button component
    /// Supports both mouse clicks and gamepad input
    /// Works with New Input System
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class ReturnToMapButton : MonoBehaviour
    {
        [Header("Input Settings")]
        [Tooltip("Enable keyboard shortcut (ESC key)")]
        [SerializeField] private bool enableEscapeKey = true;
        
        [Tooltip("Enable gamepad input (B/Circle button)")]
        [SerializeField] private bool enableGamepadInput = true;
        
        [Tooltip("Input action reference for gamepad 'Back' button (optional)")]
        [SerializeField] private InputActionReference cancelAction;

        [Header("Audio (Optional)")]
        [Tooltip("Sound to play when returning to map")]
        [SerializeField] private AudioClip returnSound;

        [Header("Debug")]
        [SerializeField] private bool showDebug = false;

        private Button button;
        private SceneConnectionManager sceneManager;

        #region Unity Lifecycle

        private void Awake()
        {
            button = GetComponent<Button>();
            
            // Subscribe to button click
            if (button != null)
            {
                button.onClick.AddListener(OnButtonClick);
            }
        }

        private void Start()
        {
            // Find scene connection manager
            sceneManager = SceneConnectionManager.Instance;
            
            if (sceneManager == null)
            {
                sceneManager = FindObjectOfType<SceneConnectionManager>();
            }

            if (sceneManager == null && showDebug)
            {
                Debug.LogWarning("[ReturnToMapButton] No SceneConnectionManager found in scene!");
            }
        }

        private void OnEnable()
        {
            // Subscribe to input action if assigned
            if (cancelAction != null && cancelAction.action != null)
            {
                cancelAction.action.Enable();
                cancelAction.action.performed += OnCancelAction;
            }
        }

        private void OnDisable()
        {
            // Unsubscribe from input action
            if (cancelAction != null && cancelAction.action != null)
            {
                cancelAction.action.performed -= OnCancelAction;
            }
        }

        private void Update()
        {
            HandleKeyboardInput();
        }

        private void OnDestroy()
        {
            // Cleanup
            if (button != null)
            {
                button.onClick.RemoveListener(OnButtonClick);
            }
        }

        #endregion

        #region Input Handling

        private void HandleKeyboardInput()
        {
            // ESC key to return to map
            if (enableEscapeKey && Input.GetKeyDown(KeyCode.Escape))
            {
                ReturnToMap();
            }
        }

        private void OnCancelAction(InputAction.CallbackContext context)
        {
            // Gamepad "Back" button pressed (B/Circle)
            if (enableGamepadInput)
            {
                ReturnToMap();
            }
        }

        private void OnButtonClick()
        {
            // Mouse click on button
            ReturnToMap();
        }

        #endregion

        #region Map Return Logic

        private void ReturnToMap()
        {
            if (showDebug)
            {
                Debug.Log("[ReturnToMapButton] Returning to map...");
            }

            // Play sound if assigned
            if (returnSound != null)
            {
                AudioSource.PlayClipAtPoint(returnSound, UnityEngine.Camera.main.transform.position);
            }

            // Use SceneConnectionManager if available
            if (sceneManager != null)
            {
                sceneManager.ReturnToMap();
            }
            else
            {
                // Fallback: Try to find and return to a default map scene
                Debug.LogWarning("[ReturnToMapButton] No SceneConnectionManager - attempting fallback to 'MapScene'");
                UnityEngine.SceneManagement.SceneManager.LoadScene("MapScene");
            }
        }

        #endregion

        #region Public API

        /// <summary>
        /// Manually trigger return to map (can be called from other scripts)
        /// </summary>
        public void TriggerReturn()
        {
            ReturnToMap();
        }

        /// <summary>
        /// Enable or disable the button
        /// </summary>
        public void SetInteractable(bool interactable)
        {
            if (button != null)
            {
                button.interactable = interactable;
            }
        }

        #endregion
    }
}
