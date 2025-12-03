using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using VisionsForetold.Game.SkillSystem;
using UnityEngine.EventSystems;

namespace VisionsForetold.Game.SaveSystem
{
    /// <summary>
    /// Grid-based UI Controller for skill tree - gamepad friendly
    /// </summary>
    public class SkillTreeUI_GridBased : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject skillPanel;
        [SerializeField] private Transform skillGridContainer;
        [SerializeField] private GameObject skillCardPrefab;

        [Header("Player Info")]
        [SerializeField] private TMP_Text levelText;
        [SerializeField] private TMP_Text experienceText;
        [SerializeField] private TMP_Text skillPointsText;
        [SerializeField] private Slider experienceBar;

        [Header("Skill Preview Panel")]
        [SerializeField] private GameObject previewPanel;
        [SerializeField] private TMP_Text previewNameText;
        [SerializeField] private TMP_Text previewDescriptionText;
        [SerializeField] private TMP_Text previewLevelText;
        [SerializeField] private TMP_Text previewCostText;
        [SerializeField] private Image previewIcon;
        [SerializeField] private Button actionButton; // Unlock or Level Up
        [SerializeField] private TMP_Text actionButtonText;
        [SerializeField] private Button backButton; // Back from preview to grid
        [SerializeField] private Button closeSkillTreeButton; // Close entire skill tree and return to save station

        [Header("Category Tabs")]
        [SerializeField] private Button allSkillsTab;
        [SerializeField] private Button combatSkillsTab;
        [SerializeField] private Button magicSkillsTab;
        [SerializeField] private Button defenseSkillsTab;
        [SerializeField] private Button utilitySkillsTab;

        [Header("Grid Settings")]
        [SerializeField] private int gridColumns = 3;
        [SerializeField] private float cardSpacing = 10f;
        [SerializeField] private Vector2 cardSize = new Vector2(200, 150);

        [Header("Colors")]
        [SerializeField] private Color unlockedColor = new Color(0.2f, 0.8f, 0.2f);
        [SerializeField] private Color lockedColor = new Color(0.5f, 0.5f, 0.5f);
        [SerializeField] private Color canUnlockColor = new Color(0.8f, 0.8f, 0.2f);
        [SerializeField] private Color selectedColor = new Color(0.3f, 0.6f, 1f);

        [Header("Gamepad Navigation")]
        [SerializeField] private bool enableAutoNavigation = true;
        [SerializeField] private float navigationDelay = 0.15f;

        private SkillManager skillManager;
        private List<SkillCard> skillCards = new List<SkillCard>();
        private Skill selectedSkill;
        private SkillCard selectedCard;
        private SkillCategory? currentFilter = null;
        private EventSystem eventSystem;
        private float lastNavigationTime;

        // Event to notify when skill tree is closed
        public event System.Action OnSkillTreeClosed;

        private class SkillCard
        {
            public GameObject gameObject;
            public Button button;
            public TMP_Text nameText;
            public TMP_Text levelText;
            public Image icon;
            public Image background;
            public Skill skill;
            public GameObject lockIcon;
            public Image selectionBorder;
        }

        #region Unity Lifecycle

        private void Awake()
        {
            eventSystem = EventSystem.current;
            if (eventSystem == null)
            {
                GameObject eventSystemObj = new GameObject("EventSystem");
                eventSystem = eventSystemObj.AddComponent<EventSystem>();
                eventSystemObj.AddComponent<StandaloneInputModule>();
            }

            SetupTabButtons();
            SetupActionButton();

            if (previewPanel != null)
                previewPanel.SetActive(false);
        }

        private void OnEnable()
        {
            skillManager = SkillManager.Instance;

            if (skillManager == null)
            {
                skillManager = FindObjectOfType<SkillManager>();

                if (skillManager == null)
                    return;
            }

            SubscribeToEvents();
            RefreshUI();

            // Set first selectable for gamepad
            Invoke(nameof(SetInitialSelection), 0.1f);
        }

        private void OnDisable()
        {
            UnsubscribeFromEvents();
        }

        private void Update()
        {
            HandleGamepadInput();
            HandleKeyboardInput();
        }

        #endregion

        #region Setup

        private void SetupTabButtons()
        {
            if (allSkillsTab != null)
                allSkillsTab.onClick.AddListener(() => FilterSkills(null));
            if (combatSkillsTab != null)
                combatSkillsTab.onClick.AddListener(() => FilterSkills(SkillCategory.Combat));
            if (magicSkillsTab != null)
                magicSkillsTab.onClick.AddListener(() => FilterSkills(SkillCategory.Magic));
            if (defenseSkillsTab != null)
                defenseSkillsTab.onClick.AddListener(() => FilterSkills(SkillCategory.Defense));
            if (utilitySkillsTab != null)
                utilitySkillsTab.onClick.AddListener(() => FilterSkills(SkillCategory.Utility));
        }

        private void SetupActionButton()
        {
            if (actionButton != null)
                actionButton.onClick.AddListener(PerformSkillAction);
            
            if (backButton != null)
                backButton.onClick.AddListener(HidePreview);
            
            if (closeSkillTreeButton != null)
                closeSkillTreeButton.onClick.AddListener(CloseSkillTree);
        }

        private void SubscribeToEvents()
        {
            if (skillManager == null) return;

            skillManager.OnSkillUnlocked += OnSkillChanged;
            skillManager.OnSkillLeveledUp += OnSkillChanged;
            skillManager.OnLevelUp += OnPlayerLevelUp;
            skillManager.OnSkillPointsChanged += OnSkillPointsChanged;
            skillManager.OnExperienceGained += OnExperienceGained;
        }

        private void UnsubscribeFromEvents()
        {
            if (skillManager == null) return;

            skillManager.OnSkillUnlocked -= OnSkillChanged;
            skillManager.OnSkillLeveledUp -= OnSkillChanged;
            skillManager.OnLevelUp -= OnPlayerLevelUp;
            skillManager.OnSkillPointsChanged -= OnSkillPointsChanged;
            skillManager.OnExperienceGained -= OnExperienceGained;
        }

        #endregion

        #region Input Handling

        private void HandleGamepadInput()
        {
            if (!enableAutoNavigation) return;
            if (Time.time - lastNavigationTime < navigationDelay) return;

            // Quick tab switching with shoulder buttons
            if (Input.GetButtonDown("Previous")) // L1/LB
            {
                SwitchToPreviousTab();
                lastNavigationTime = Time.time;
            }
            else if (Input.GetButtonDown("Next")) // R1/RB
            {
                SwitchToNextTab();
                lastNavigationTime = Time.time;
            }

            // Action button (A/Cross) is handled by UI Button
            // Cancel button (B/Circle) for back navigation
            if (Input.GetButtonDown("Jump")) // B/Circle button
            {
                // If preview panel is open, close it
                if (previewPanel != null && previewPanel.activeSelf)
                {
                    HidePreview();
                }
                // Otherwise, close the entire skill tree
                else
                {
                    CloseSkillTree();
                }
                lastNavigationTime = Time.time;
            }
        }

        private void HandleKeyboardInput()
        {
            // ESC to go back
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                // If preview panel is open, close it first
                if (previewPanel != null && previewPanel.activeSelf)
                {
                    HidePreview();
                }
                // Otherwise, close the entire skill tree
                else
                {
                    CloseSkillTree();
                }
            }

            // Tab navigation
            if (Input.GetKeyDown(KeyCode.Q))
            {
                SwitchToPreviousTab();
            }
            else if (Input.GetKeyDown(KeyCode.E))
            {
                SwitchToNextTab();
            }
        }

        private void SwitchToPreviousTab()
        {
            if (currentFilter == null)
            {
                FilterSkills(SkillCategory.Utility);
            }
            else
            {
                switch (currentFilter.Value)
                {
                    case SkillCategory.Combat:
                        FilterSkills(null);
                        break;
                    case SkillCategory.Magic:
                        FilterSkills(SkillCategory.Combat);
                        break;
                    case SkillCategory.Defense:
                        FilterSkills(SkillCategory.Magic);
                        break;
                    case SkillCategory.Utility:
                        FilterSkills(SkillCategory.Defense);
                        break;
                }
            }
        }

        private void SwitchToNextTab()
        {
            if (currentFilter == null)
            {
                FilterSkills(SkillCategory.Combat);
            }
            else
            {
                switch (currentFilter.Value)
                {
                    case SkillCategory.Combat:
                        FilterSkills(SkillCategory.Magic);
                        break;
                    case SkillCategory.Magic:
                        FilterSkills(SkillCategory.Defense);
                        break;
                    case SkillCategory.Defense:
                        FilterSkills(SkillCategory.Utility);
                        break;
                    case SkillCategory.Utility:
                        FilterSkills(null);
                        break;
                }
            }
        }

        #endregion

        #region UI Refresh

        public void RefreshUI()
        {
            if (skillManager == null) return;

            UpdatePlayerInfo();
            RefreshSkillGrid();
        }

        private void UpdatePlayerInfo()
        {
            if (skillManager == null) return;

            var skillData = skillManager.GetSkillSaveData();
            if (skillData == null) return;

            if (levelText != null)
                levelText.text = $"Lv. {skillData.level}";

            if (experienceText != null)
                experienceText.text = $"{skillData.experience} / {skillData.experienceToNextLevel} XP";

            if (skillPointsText != null)
                skillPointsText.text = $"Points: {skillData.skillPoints}";

            if (experienceBar != null)
            {
                experienceBar.maxValue = skillData.experienceToNextLevel;
                experienceBar.value = skillData.experience;
            }
        }

        private void RefreshSkillGrid()
        {
            ClearGrid();

            List<Skill> skills;
            if (currentFilter.HasValue)
            {
                skills = skillManager.GetSkillsByCategory(currentFilter.Value);
            }
            else
            {
                skills = skillManager.GetAllSkills();
            }

            // Sort by tier then name
            skills = skills.OrderBy(s => s.tier).ThenBy(s => s.skillName).ToList();

            // Create grid layout
            CreateGridLayout(skills);
        }

        private void ClearGrid()
        {
            foreach (var card in skillCards)
            {
                if (card.gameObject != null)
                    Destroy(card.gameObject);
            }
            skillCards.Clear();
        }

        private void CreateGridLayout(List<Skill> skills)
        {
            if (skillCardPrefab == null || skillGridContainer == null)
                return;

            // Setup grid layout group
            GridLayoutGroup gridLayout = skillGridContainer.GetComponent<GridLayoutGroup>();
            if (gridLayout == null)
            {
                gridLayout = skillGridContainer.gameObject.AddComponent<GridLayoutGroup>();
            }

            gridLayout.cellSize = cardSize;
            gridLayout.spacing = new Vector2(cardSpacing, cardSpacing);
            gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            gridLayout.constraintCount = gridColumns;
            gridLayout.childAlignment = TextAnchor.UpperCenter;

            // Create cards
            for (int i = 0; i < skills.Count; i++)
            {
                CreateSkillCard(skills[i], i);
            }
        }

        private void CreateSkillCard(Skill skill, int index)
        {
            GameObject cardObj = Instantiate(skillCardPrefab, skillGridContainer);
            SkillCard card = new SkillCard
            {
                gameObject = cardObj,
                skill = skill
            };

            // Get components
            card.button = cardObj.GetComponent<Button>();
            card.nameText = cardObj.transform.Find("SkillName")?.GetComponent<TMP_Text>();
            card.levelText = cardObj.transform.Find("SkillLevel")?.GetComponent<TMP_Text>();
            card.icon = cardObj.transform.Find("Icon")?.GetComponent<Image>();
            card.background = cardObj.GetComponent<Image>();
            card.lockIcon = cardObj.transform.Find("LockIcon")?.gameObject;
            card.selectionBorder = cardObj.transform.Find("SelectionBorder")?.GetComponent<Image>();

            // Set content
            if (card.nameText != null)
                card.nameText.text = skill.skillName;

            if (card.levelText != null)
            {
                if (skill.IsUnlocked)
                    card.levelText.text = $"{skill.currentLevel}/{skill.maxLevel}";
                else
                    card.levelText.text = "Locked";
            }

            if (card.icon != null && skill.icon != null)
                card.icon.sprite = skill.icon;

            if (card.lockIcon != null)
                card.lockIcon.SetActive(!skill.IsUnlocked);

            if (card.selectionBorder != null)
                card.selectionBorder.enabled = false;

            // Set color
            UpdateCardColor(card);

            // Setup button
            if (card.button != null)
            {
                card.button.onClick.AddListener(() => SelectSkillCard(card));

                // Setup navigation
                Navigation nav = card.button.navigation;
                nav.mode = Navigation.Mode.Automatic;
                card.button.navigation = nav;
            }

            skillCards.Add(card);
        }

        private void UpdateCardColor(SkillCard card)
        {
            if (card.background == null) return;

            var skillData = skillManager.GetSkillSaveData();

            if (card.skill.IsUnlocked)
            {
                card.background.color = unlockedColor;
            }
            else if (card.skill.CanUnlock(skillData))
            {
                card.background.color = canUnlockColor;
            }
            else
            {
                card.background.color = lockedColor;
            }
        }

        #endregion

        #region Skill Selection

        private void SelectSkillCard(SkillCard card)
        {
            // Deselect previous
            if (selectedCard != null && selectedCard.selectionBorder != null)
            {
                selectedCard.selectionBorder.enabled = false;
            }

            // Select new
            selectedCard = card;
            selectedSkill = card.skill;

            if (card.selectionBorder != null)
            {
                card.selectionBorder.enabled = true;
                card.selectionBorder.color = selectedColor;
            }

            ShowPreview(card.skill);
        }

        private void ShowPreview(Skill skill)
        {
            if (previewPanel == null) return;

            previewPanel.SetActive(true);

            var skillData = skillManager.GetSkillSaveData();

            // Name
            if (previewNameText != null)
                previewNameText.text = skill.skillName;

            // Description with effects
            if (previewDescriptionText != null)
            {
                string desc = skill.description;
                desc += "\n\n<b>Effects:</b>";
                
                foreach (var effect in skill.effects)
                {
                    float value = effect.GetValue(skill.IsUnlocked ? skill.currentLevel : 1);
                    string valueStr = effect.isPercentage ? $"{value}%" : $"+{value}";
                    desc += $"\n  • {effect.effectType}: {valueStr}";
                }

                if (!skill.IsUnlocked)
                {
                    desc += "\n\n<b>Requirements:</b>";
                    desc += $"\n  • Player Level: {skill.requirements.minimumLevel}";
                    desc += $"\n  • Skill Points: {skill.requirements.requiredSkillPoints}";

                    if (skill.requirements.prerequisiteSkillIds.Count > 0)
                    {
                        desc += "\n  • Prerequisites:";
                        foreach (string prereqId in skill.requirements.prerequisiteSkillIds)
                        {
                            Skill prereq = skillManager.GetSkill(prereqId);
                            if (prereq != null)
                            {
                                string status = prereq.IsUnlocked ? "?" : "?";
                                desc += $"\n    {status} {prereq.skillName}";
                            }
                        }
                    }
                }

                previewDescriptionText.text = desc;
            }

            // Level
            if (previewLevelText != null)
            {
                if (skill.IsUnlocked)
                    previewLevelText.text = $"Level {skill.currentLevel} / {skill.maxLevel}";
                else
                    previewLevelText.text = "Locked";
            }

            // Cost
            if (previewCostText != null)
            {
                if (!skill.IsUnlocked)
                    previewCostText.text = $"Unlock Cost: {skill.requirements.requiredSkillPoints} SP";
                else if (!skill.IsMaxLevel)
                    previewCostText.text = $"Upgrade Cost: {skill.GetLevelUpCost()} SP";
                else
                    previewCostText.text = "Max Level";
            }

            // Icon
            if (previewIcon != null && skill.icon != null)
                previewIcon.sprite = skill.icon;

            // Action button
            UpdateActionButton(skill, skillData);
        }

        private void HidePreview()
        {
            if (previewPanel != null)
                previewPanel.SetActive(false);

            if (selectedCard != null && selectedCard.selectionBorder != null)
                selectedCard.selectionBorder.enabled = false;

            selectedCard = null;
            selectedSkill = null;
        }

        /// <summary>
        /// Close the entire skill tree and notify listeners (e.g., SaveStationMenu)
        /// </summary>
        public void CloseSkillTree()
        {
            // Close preview panel if open
            if (previewPanel != null && previewPanel.activeSelf)
            {
                HidePreview();
            }

            // Notify listeners that skill tree is being closed
            OnSkillTreeClosed?.Invoke();
        }

        private void UpdateActionButton(Skill skill, SkillSaveData skillData)
        {
            if (actionButton == null) return;

            if (!skill.IsUnlocked)
            {
                // Show unlock button
                actionButton.gameObject.SetActive(true);
                actionButton.interactable = skill.CanUnlock(skillData);

                if (actionButtonText != null)
                    actionButtonText.text = "Unlock";
            }
            else if (skill.CanLevelUp(skillData))
            {
                // Show upgrade button
                actionButton.gameObject.SetActive(true);
                actionButton.interactable = true;

                if (actionButtonText != null)
                    actionButtonText.text = "Upgrade";
            }
            else
            {
                // Show disabled button
                actionButton.gameObject.SetActive(true);
                actionButton.interactable = false;

                if (actionButtonText != null)
                {
                    if (skill.IsMaxLevel)
                        actionButtonText.text = "Max Level";
                    else
                        actionButtonText.text = "Insufficient Points";
                }
            }
        }

        #endregion

        #region Skill Actions

        private void PerformSkillAction()
        {
            if (selectedSkill == null) return;

            bool success = false;

            if (!selectedSkill.IsUnlocked)
            {
                success = skillManager.UnlockSkill(selectedSkill.skillId);
            }
            else if (!selectedSkill.IsMaxLevel)
            {
                success = skillManager.LevelUpSkill(selectedSkill.skillId);
            }

            if (success)
            {
                RefreshUI();
                if (selectedSkill != null)
                    ShowPreview(selectedSkill);
            }
        }

        private void FilterSkills(SkillCategory? category)
        {
            currentFilter = category;
            HidePreview();
            RefreshSkillGrid();

            // Set initial selection after filter
            SetInitialSelection();
        }

        private void SetInitialSelection()
        {
            if (skillCards.Count > 0 && eventSystem != null)
            {
                var firstCard = skillCards[0];
                if (firstCard.button != null)
                {
                    eventSystem.SetSelectedGameObject(firstCard.button.gameObject);
                }
            }
        }

        #endregion

        #region Event Handlers

        private void OnSkillChanged(Skill skill)
        {
            RefreshUI();

            // Refresh preview if it's the selected skill
            if (selectedSkill != null && selectedSkill.skillId == skill.skillId)
            {
                ShowPreview(skill);
            }
        }

        private void OnPlayerLevelUp(int newLevel)
        {
            UpdatePlayerInfo();
        }

        private void OnSkillPointsChanged(int points)
        {
            UpdatePlayerInfo();

            // Update all card colors
            foreach (var card in skillCards)
            {
                UpdateCardColor(card);
            }

            // Update action button
            if (selectedSkill != null)
            {
                UpdateActionButton(selectedSkill, skillManager.GetSkillSaveData());
            }
        }

        private void OnExperienceGained(int amount)
        {
            UpdatePlayerInfo();
        }

        #endregion
    }
}
