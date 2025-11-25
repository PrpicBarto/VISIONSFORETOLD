using UnityEngine;
using UnityEngine.InputSystem;

namespace VisionsForetold.Game.SaveSystem
{
    /// <summary>
    /// Interactive save station that player can use to save game and access menu
    /// Place this on objects in your scene where player should be able to save
    /// Supports New Input System for both Keyboard and Gamepad
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class SaveStation : MonoBehaviour
    {
        [Header("Interaction Settings")]
        [SerializeField] private float interactionRange = 3f;
        [SerializeField] private string playerTag = "Player";

        [Header("UI References")]
        [SerializeField] private GameObject promptUI;
        [SerializeField] private SaveStationMenu saveStationMenu;

        [Header("Visual Settings")]
        [SerializeField] private GameObject activeVisual;
        [SerializeField] private Color highlightColor = Color.cyan;
        [SerializeField] private float pulseSpeed = 2f;

        // State
        private bool playerInRange;
        private Transform player;
        private Renderer visualRenderer;
        private Color originalColor;
        private PlayerInput playerInput;
        private InputAction interactAction;

        #region Unity Lifecycle

        private void Awake()
        {
            // Ensure collider is trigger
            Collider col = GetComponent<Collider>();
            if (col != null)
            {
                col.isTrigger = true;
            }

            // Get visual renderer if available
            if (activeVisual != null)
            {
                visualRenderer = activeVisual.GetComponent<Renderer>();
                if (visualRenderer != null)
                {
                    originalColor = visualRenderer.material.color;
                }
            }

            // Hide prompt initially
            if (promptUI != null)
            {
                promptUI.SetActive(false);
            }
        }

        private void Update()
        {
            if (playerInRange)
            {
                // Update visual effects
                UpdateVisuals();
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag(playerTag))
            {
                player = other.transform;
                playerInRange = true;

                // Get PlayerInput component
                playerInput = other.GetComponent<PlayerInput>();
                if (playerInput != null)
                {
                    // Find the Interact action (works with both keyboard and gamepad)
                    interactAction = playerInput.actions.FindAction("Interact");
                    if (interactAction != null)
                    {
                        interactAction.performed += OnInteractPerformed;
                        Debug.Log("[SaveStation] Subscribed to Interact action (Keyboard: E, Gamepad: Y/Triangle)");
                    }
                    else
                    {
                        Debug.LogWarning("[SaveStation] 'Interact' action not found in Input Actions!");
                    }
                }
                else
                {
                    Debug.LogWarning("[SaveStation] PlayerInput component not found on player!");
                }

                ShowPrompt();
                Debug.Log("[SaveStation] Player entered save station range");
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag(playerTag))
            {
                playerInRange = false;

                // Unsubscribe from input
                if (interactAction != null)
                {
                    interactAction.performed -= OnInteractPerformed;
                    interactAction = null;
                    Debug.Log("[SaveStation] Unsubscribed from Interact action");
                }

                playerInput = null;
                player = null;

                HidePrompt();
                ResetVisuals();
                Debug.Log("[SaveStation] Player left save station range");
            }
        }

        private void OnDisable()
        {
            // Clean up input subscription
            if (interactAction != null)
            {
                interactAction.performed -= OnInteractPerformed;
            }
        }

        #endregion

        #region Interaction

        /// <summary>
        /// Called when player performs interact action (New Input System)
        /// Works with both Keyboard (E) and Gamepad (Button North/Y/Triangle)
        /// </summary>
        private void OnInteractPerformed(InputAction.CallbackContext context)
        {
            if (playerInRange && context.performed)
            {
                OpenSaveStation();
            }
        }

        /// <summary>
        /// Opens the save station menu
        /// </summary>
        private void OpenSaveStation()
        {
            if (saveStationMenu != null)
            {
                saveStationMenu.OpenMenu();
                saveStationMenu.SetSaveStation(this); // Pass reference
                HidePrompt();

                // Disable player movement and input
                if (player != null)
                {
                    // Disable player movement
                    PlayerMovement movement = player.GetComponent<PlayerMovement>();
                    if (movement != null)
                    {
                        movement.enabled = false;
                    }

                    // Switch to UI input action map
                    if (playerInput != null)
                    {
                        // Disable Player action map
                        var playerActionMap = playerInput.actions.FindActionMap("Player");
                        if (playerActionMap != null)
                        {
                            playerActionMap.Disable();
                            Debug.Log("[SaveStation] Disabled Player action map");
                        }

                        // Enable UI action map
                        var uiActionMap = playerInput.actions.FindActionMap("UI");
                        if (uiActionMap != null)
                        {
                            uiActionMap.Enable();
                            Debug.Log("[SaveStation] Enabled UI action map");
                        }
                    }

                    // Disable player attack
                    PlayerAttack attack = player.GetComponent<PlayerAttack>();
                    if (attack != null)
                    {
                        attack.enabled = false;
                    }
                }

                Debug.Log("[SaveStation] Opened save station menu");
            }
            else
            {
                Debug.LogWarning("[SaveStation] No SaveStationMenu assigned!");
            }
        }

        /// <summary>
        /// Called when save station menu closes
        /// </summary>
        public void OnMenuClosed()
        {
            // Re-enable player components
            if (player != null)
            {
                // Re-enable player movement
                PlayerMovement movement = player.GetComponent<PlayerMovement>();
                if (movement != null)
                {
                    movement.enabled = true;
                }

                // Re-enable player attack
                PlayerAttack attack = player.GetComponent<PlayerAttack>();
                if (attack != null)
                {
                    attack.enabled = true;
                }

                // Switch back to Player input action map
                if (playerInput != null)
                {
                    // Disable UI action map
                    var uiActionMap = playerInput.actions.FindActionMap("UI");
                    if (uiActionMap != null)
                    {
                        uiActionMap.Disable();
                        Debug.Log("[SaveStation] Disabled UI action map");
                    }

                    // Re-enable Player action map
                    var playerActionMap = playerInput.actions.FindActionMap("Player");
                    if (playerActionMap != null)
                    {
                        playerActionMap.Enable();
                        Debug.Log("[SaveStation] Re-enabled Player action map");
                    }
                }
            }

            if (playerInRange)
            {
                ShowPrompt();
            }

            Debug.Log("[SaveStation] Save station menu closed");
        }

        #endregion

        #region UI

        private void ShowPrompt()
        {
            if (promptUI != null)
            {
                promptUI.SetActive(true);
            }
        }

        private void HidePrompt()
        {
            if (promptUI != null)
            {
                promptUI.SetActive(false);
            }
        }

        #endregion

        #region Visuals

        private void UpdateVisuals()
        {
            if (visualRenderer != null)
            {
                // Pulse effect
                float pulse = Mathf.PingPong(Time.time * pulseSpeed, 1f);
                Color currentColor = Color.Lerp(originalColor, highlightColor, pulse);
                visualRenderer.material.color = currentColor;
            }
        }

        private void ResetVisuals()
        {
            if (visualRenderer != null)
            {
                visualRenderer.material.color = originalColor;
            }
        }

        #endregion

        #region Gizmos

        private void OnDrawGizmosSelected()
        {
            // Draw interaction range
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, interactionRange);
        }

        #endregion
    }
}
