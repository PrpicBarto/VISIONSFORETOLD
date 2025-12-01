using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.EventSystems;

namespace VisionsForetold.Game.SaveSystem
{
    /// <summary>
    /// UI Menu for save stations - handles saving, skills, and exiting
    /// Supports both Keyboard/Mouse and Gamepad navigation
    /// </summary>
    public class SaveStationMenu : MonoBehaviour
    {
        [Header("Main Menu")]
        [SerializeField] private GameObject menuPanel;
        [SerializeField] private Button saveButton;
        [SerializeField] private Button skillsButton;
        [SerializeField] private Button exitButton;

        [Header("Save Panel")]
        [SerializeField] private GameObject savePanel;
        [SerializeField] private TMP_InputField saveNameInput;
        [SerializeField] private TMP_Dropdown slotDropdown;
        [SerializeField] private Button confirmSaveButton;
        [SerializeField] private Button cancelSaveButton;
        [SerializeField] private TMP_Text saveStatusText;

        [Header("Skills Panel")]
        [SerializeField] private GameObject skillsPanel;
        [SerializeField] private Button closeSkillsButton;
        
        [Header("Confirmation Dialog")]
        [SerializeField] private GameObject confirmationDialog;
        [SerializeField] private TMP_Text confirmationText;
        [SerializeField] private Button confirmYesButton;
        [SerializeField] private Button confirmNoButton;

        [Header("Settings")]
        [SerializeField] private int defaultSaveSlot = 0;

        [Header("Gamepad Navigation")]
        [SerializeField] private bool enableGamepadNavigation = true;

        // References
        private SaveStation currentSaveStation;
        private SaveManager saveManager;
        private EventSystem eventSystem;
        private PlayerInput playerInput;

        #region Unity Lifecycle

        private void Awake()
        {
            // Initialize EventSystem first
            InitializeEventSystem();
            
            // Setup button listeners
            InitializeButtons();
            
            // Initialize panels
            InitializePanels();
        }

        private void Start()
        {
            // Get SaveManager instance (might not be ready in Awake)
            saveManager = SaveManager.Instance;
            
            if (saveManager == null)
            {
                Debug.LogError("[SaveStationMenu] SaveManager instance not found! Make sure SaveManager exists in the scene.");
            }
        }

        private void InitializeEventSystem()
        {
            eventSystem = EventSystem.current;

            // Find or create EventSystem if it doesn't exist
            if (eventSystem == null)
            {
                GameObject eventSystemObj = new GameObject("EventSystem");
                eventSystem = eventSystemObj.AddComponent<EventSystem>();
                eventSystemObj.AddComponent<StandaloneInputModule>();
                Debug.Log("[SaveStationMenu] Created EventSystem");
            }
        }

        private void InitializeButtons()
        {
            // Main menu buttons
            if (saveButton != null) 
                saveButton.onClick.AddListener(OnSaveButtonClicked);
            else
                Debug.LogWarning("[SaveStationMenu] Save button not assigned!");
                
            if (skillsButton != null) 
                skillsButton.onClick.AddListener(OnSkillsButtonClicked);
            if (exitButton != null) 
                exitButton.onClick.AddListener(OnExitButtonClicked);

            // Save panel buttons
            if (confirmSaveButton != null) 
                confirmSaveButton.onClick.AddListener(OnConfirmSaveClicked);
            if (cancelSaveButton != null) 
                cancelSaveButton.onClick.AddListener(OnCancelSaveClicked);

            // Skills panel buttons
            if (closeSkillsButton != null) 
                closeSkillsButton.onClick.AddListener(OnCloseSkillsClicked);

            // Confirmation dialog buttons
            if (confirmYesButton != null) 
                confirmYesButton.onClick.AddListener(OnConfirmYes);
            if (confirmNoButton != null) 
                confirmNoButton.onClick.AddListener(OnConfirmNo);
        }

        private void InitializePanels()
        {
            // Initialize all panels to hidden state
            if (menuPanel != null) 
                menuPanel.SetActive(false);
            else
                Debug.LogError("[SaveStationMenu] Menu panel not assigned!");
                
            if (savePanel != null) 
                savePanel.SetActive(false);
            if (skillsPanel != null) 
                skillsPanel.SetActive(false);
            if (confirmationDialog != null) 
                confirmationDialog.SetActive(false);
        }

        private void OnDestroy()
        {
            // Remove button listeners
            if (saveButton != null) saveButton.onClick.RemoveListener(OnSaveButtonClicked);
            if (skillsButton != null) skillsButton.onClick.RemoveListener(OnSkillsButtonClicked);
            if (exitButton != null) exitButton.onClick.RemoveListener(OnExitButtonClicked);
            if (confirmSaveButton != null) confirmSaveButton.onClick.RemoveListener(OnConfirmSaveClicked);
            if (cancelSaveButton != null) cancelSaveButton.onClick.RemoveListener(OnCancelSaveClicked);
            if (closeSkillsButton != null) closeSkillsButton.onClick.RemoveListener(OnCloseSkillsClicked);
            if (confirmYesButton != null) confirmYesButton.onClick.RemoveListener(OnConfirmYes);
            if (confirmNoButton != null) confirmNoButton.onClick.RemoveListener(OnConfirmNo);
        }

        #endregion

        #region Menu Control

        /// <summary>
        /// Opens the save station menu
        /// </summary>
        public void OpenMenu()
        {
            // Validate before opening
            if (!ValidateMenuOpen())
                return;

            if (menuPanel != null)
            {
                menuPanel.SetActive(true);
                Time.timeScale = 0f; // Pause game

                // Set first selected button for gamepad navigation
                if (enableGamepadNavigation && eventSystem != null && saveButton != null)
                {
                    eventSystem.SetSelectedGameObject(saveButton.gameObject);
                    Debug.Log("[SaveStationMenu] Set Save button as first selected for gamepad");
                }
                
                Debug.Log("[SaveStationMenu] Menu opened successfully");
            }
        }

        /// <summary>
        /// Validates that the menu can be opened
        /// </summary>
        private bool ValidateMenuOpen()
        {
            if (menuPanel == null)
            {
                Debug.LogError("[SaveStationMenu] Cannot open menu - Menu panel not assigned!");
                return false;
            }

            if (saveManager == null)
            {
                Debug.LogError("[SaveStationMenu] Cannot open menu - SaveManager not found! Retrying...");
                saveManager = SaveManager.Instance;
                
                if (saveManager == null)
                {
                    Debug.LogError("[SaveStationMenu] SaveManager still not found. Please ensure SaveManager exists in scene.");
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Closes the save station menu
        /// </summary>
        public void CloseMenu()
        {
            if (menuPanel != null)
            {
                menuPanel.SetActive(false);
                Time.timeScale = 1f; // Unpause game
            }

            // Close all sub-panels
            if (savePanel != null) savePanel.SetActive(false);
            if (skillsPanel != null) skillsPanel.SetActive(false);
            if (confirmationDialog != null) confirmationDialog.SetActive(false);

            // Clear selected object
            if (eventSystem != null)
            {
                eventSystem.SetSelectedGameObject(null);
            }

            // Notify save station
            if (currentSaveStation != null)
            {
                currentSaveStation.OnMenuClosed();
            }
        }

        #endregion

        #region Button Handlers

        private void OnSaveButtonClicked()
        {
            ShowSavePanel();
        }

        private void OnSkillsButtonClicked()
        {
            ShowSkillsPanel();
        }

        private void OnExitButtonClicked()
        {
            CloseMenu();
        }

        private void OnConfirmSaveClicked()
        {
            PerformSave();
        }

        private void OnCancelSaveClicked()
        {
            HideSavePanel();
        }

        private void OnCloseSkillsClicked()
        {
            HideSkillsPanel();
        }

        private void OnConfirmYes()
        {
            // Handle confirmation (can be used for overwrite warnings etc.)
            if (confirmationDialog != null)
            {
                confirmationDialog.SetActive(false);
            }

            // Return to main menu
            SetGamepadSelection(saveButton);
        }

        private void OnConfirmNo()
        {
            if (confirmationDialog != null)
            {
                confirmationDialog.SetActive(false);
            }

            // Return to save panel
            SetGamepadSelection(confirmSaveButton);
        }

        #endregion

        #region Save Panel

        private void ShowSavePanel()
        {
            if (savePanel != null)
            {
                savePanel.SetActive(true);

                // Populate save name with default or current
                if (saveNameInput != null)
                {
                    SaveData currentSave = saveManager?.GetCurrentSaveData();
                    saveNameInput.text = currentSave != null ? currentSave.saveName : "New Save";
                }

                // Clear status text
                if (saveStatusText != null)
                {
                    saveStatusText.text = "";
                }

                // Set first selected for gamepad
                SetGamepadSelection(saveNameInput);
            }
        }

        private void HideSavePanel()
        {
            if (savePanel != null)
            {
                savePanel.SetActive(false);
            }

            // Return to main menu
            SetGamepadSelection(saveButton);
        }

        private void PerformSave()
        {
            // Validate SaveManager
            if (saveManager == null)
            {
                Debug.LogError("[SaveStationMenu] SaveManager is null, attempting to re-acquire...");
                saveManager = SaveManager.Instance;
                
                if (saveManager == null)
                {
                    ShowSaveStatus("Save Manager not found!", false);
                    return;
                }
            }

            try
            {
                // Get save parameters
                int slotIndex = slotDropdown != null ? slotDropdown.value : defaultSaveSlot;
                string saveName = saveNameInput != null && !string.IsNullOrWhiteSpace(saveNameInput.text) 
                    ? saveNameInput.text 
                    : $"Save {slotIndex + 1}";

                // Validate slot index
                if (slotIndex < 0)
                {
                    ShowSaveStatus("Invalid save slot!", false);
                    return;
                }

                Debug.Log($"[SaveStationMenu] Attempting to save to slot {slotIndex} with name '{saveName}'");

                // Check if save already exists
                if (saveManager.DoesSaveExist(slotIndex))
                {

                    ShowConfirmation($"Overwrite save in slot {slotIndex + 1}?", () =>
                    {
                        ExecuteSave(slotIndex, saveName);
                    });
                }
                else
                {
                    ExecuteSave(slotIndex, saveName);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[SaveStationMenu] Save failed with exception: {e}");
                ShowSaveStatus($"Save Failed: {e.Message}", false);
            }
        }

        /// <summary>
        /// Executes the actual save operation
        /// </summary>
        private void ExecuteSave(int slotIndex, string saveName)
        {
            try
            {
                saveManager.SaveGame(slotIndex, saveName);
                ShowSaveStatus("Game Saved Successfully!", true);
                Debug.Log($"[SaveStationMenu] Save successful to slot {slotIndex}");
                
                // Auto-hide save panel after 2 seconds
                Invoke(nameof(HideSavePanel), 2f);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[SaveStationMenu] ExecuteSave failed: {e}");
                ShowSaveStatus($"Save Failed: {e.Message}", false);
            }
        }

        private void ShowSaveStatus(string message, bool success)
        {
            if (saveStatusText != null)
            {
                saveStatusText.text = message;
                saveStatusText.color = success ? Color.green : Color.red;
            }

            Debug.Log($"[SaveStationMenu] {message}");
        }

        #endregion

        #region Skills Panel

        private void ShowSkillsPanel()
        {
            if (skillsPanel != null)
            {
                skillsPanel.SetActive(true);
                LoadSkillsData();

                // Set first selected for gamepad
                SetGamepadSelection(closeSkillsButton);
            }
        }

        private void HideSkillsPanel()
        {
            if (skillsPanel != null)
            {
                skillsPanel.SetActive(false);
            }

            // Return to main menu
            SetGamepadSelection(saveButton);
        }

        private void LoadSkillsData()
        {
            if (saveManager == null)
                return;

            SaveData currentSave = saveManager.GetCurrentSaveData();
            if (currentSave != null && currentSave.skills != null)
            {
                // TODO: Populate skills UI with current skills data
                // This will be expanded when you implement your skill tree system
                Debug.Log($"[SaveStationMenu] Loaded skills - Level: {currentSave.skills.level}, Points: {currentSave.skills.skillPoints}");
            }
        }

        #endregion

        #region Confirmation Dialog

        private void ShowConfirmation(string message, System.Action onConfirm)
        {
            if (confirmationDialog != null)
            {
                confirmationDialog.SetActive(true);

                if (confirmationText != null)
                {
                    confirmationText.text = message;
                }

                // Setup confirmation callback
                if (confirmYesButton != null)
                {
                    confirmYesButton.onClick.RemoveAllListeners();
                    confirmYesButton.onClick.AddListener(() =>
                    {
                        onConfirm?.Invoke();
                        confirmationDialog.SetActive(false);
                        SetGamepadSelection(saveButton);
                    });
                }

                // Set first selected for gamepad
                SetGamepadSelection(confirmYesButton);
            }
        }

        #endregion

        #region Gamepad Navigation

        /// <summary>
        /// Sets the currently selected UI element for gamepad navigation
        /// </summary>
        private void SetGamepadSelection(Selectable selectable)
        {
            if (!enableGamepadNavigation || eventSystem == null || selectable == null)
                return;

            eventSystem.SetSelectedGameObject(selectable.gameObject);
        }

        /// <summary>
        /// Sets the currently selected UI element for gamepad navigation (InputField override)
        /// </summary>
        private void SetGamepadSelection(TMP_InputField inputField)
        {
            if (!enableGamepadNavigation || eventSystem == null || inputField == null)
                return;

            eventSystem.SetSelectedGameObject(inputField.gameObject);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Sets the current save station reference
        /// </summary>
        public void SetSaveStation(SaveStation station)
        {
            currentSaveStation = station;
        }

        /// <summary>
        /// Sets the player input reference for better integration
        /// </summary>
        public void SetPlayerInput(PlayerInput input)
        {
            playerInput = input;
        }

        #endregion
    }
}
