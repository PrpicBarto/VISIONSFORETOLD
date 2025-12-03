using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using VisionsForetold.Game.SkillSystem;

namespace VisionsForetold.Game.SaveSystem
{
    /// <summary>
    /// UI Controller for the skill tree panel in save station
    /// </summary>
    public class SkillTreeUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject skillPanel;
        [SerializeField] private Transform skillListContainer;
        [SerializeField] private GameObject skillButtonPrefab;

        [Header("Player Info")]
        [SerializeField] private TMP_Text levelText;
        [SerializeField] private TMP_Text experienceText;
        [SerializeField] private TMP_Text skillPointsText;
        [SerializeField] private Slider experienceBar;

        [Header("Skill Details")]
        [SerializeField] private GameObject skillDetailPanel;
        [SerializeField] private TMP_Text skillNameText;
        [SerializeField] private TMP_Text skillDescriptionText;
        [SerializeField] private TMP_Text skillLevelText;
        [SerializeField] private TMP_Text skillCostText;
        [SerializeField] private Button unlockButton;
        [SerializeField] private Button levelUpButton;
        [SerializeField] private Button backButton; // Back from skill detail to skill list
        [SerializeField] private Button closeSkillTreeButton; // Close entire skill tree and return to save station menu
        [SerializeField] private Image skillIcon;

        [Header("Category Filters")]
        [SerializeField] private Button allSkillsButton;
        [SerializeField] private Button combatSkillsButton;
        [SerializeField] private Button magicSkillsButton;
        [SerializeField] private Button defenseSkillsButton;
        [SerializeField] private Button utilitySkillsButton;

        [Header("Colors")]
        [SerializeField] private Color unlockedColor = Color.green;
        [SerializeField] private Color lockedColor = Color.gray;
        [SerializeField] private Color canUnlockColor = Color.yellow;

        private SkillManager skillManager;
        private List<SkillButtonUI> skillButtons = new List<SkillButtonUI>();
        private Skill selectedSkill;
        private SkillCategory? currentFilter = null; // null = show all skills by default

        // Event to notify when skill tree is closed
        public event System.Action OnSkillTreeClosed;

        private class SkillButtonUI
        {
            public GameObject gameObject;
            public Button button;
            public TMP_Text nameText;
            public TMP_Text levelText;
            public Image icon;
            public Image background;
            public Skill skill;
        }

        private void Awake()
        {
            // Setup button listeners
            if (allSkillsButton != null) 
                allSkillsButton.onClick.AddListener(() => FilterSkills(null));
            if (combatSkillsButton != null) 
                combatSkillsButton.onClick.AddListener(() => FilterSkills(SkillCategory.Combat));
            if (magicSkillsButton != null) 
                magicSkillsButton.onClick.AddListener(() => FilterSkills(SkillCategory.Magic));
            if (defenseSkillsButton != null) 
                defenseSkillsButton.onClick.AddListener(() => FilterSkills(SkillCategory.Defense));
            if (utilitySkillsButton != null) 
                utilitySkillsButton.onClick.AddListener(() => FilterSkills(SkillCategory.Utility));

            if (unlockButton != null) 
                unlockButton.onClick.AddListener(UnlockSelectedSkill);
            if (levelUpButton != null) 
                levelUpButton.onClick.AddListener(LevelUpSelectedSkill);
            if (backButton != null)
                backButton.onClick.AddListener(HideSkillDetails);
            if (closeSkillTreeButton != null)
                closeSkillTreeButton.onClick.AddListener(CloseSkillTree);

            // Hide detail panel initially
            if (skillDetailPanel != null)
                skillDetailPanel.SetActive(false);
        }

        private void OnEnable()
        {
            // Try to find SkillManager - might need a frame if it's initializing
            skillManager = SkillManager.Instance;

            if (skillManager == null)
            {
                skillManager = FindObjectOfType<SkillManager>();
                
                if (skillManager == null)
                    return;
            }

            // Subscribe to events
            skillManager.OnSkillUnlocked += OnSkillChanged;
            skillManager.OnSkillLeveledUp += OnSkillChanged;
            skillManager.OnLevelUp += OnPlayerLevelUp;
            skillManager.OnSkillPointsChanged += OnSkillPointsChanged;
            skillManager.OnExperienceGained += OnExperienceGained;

            RefreshUI();
        }

        private void Update()
        {
            // Allow Escape key to go back
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                // If skill detail panel is open, close it first
                if (skillDetailPanel != null && skillDetailPanel.activeSelf)
                {
                    HideSkillDetails();
                }
                // Otherwise, close the entire skill tree
                else
                {
                    CloseSkillTree();
                }
            }
        }

        private void OnDisable()
        {
            if (skillManager != null)
            {
                skillManager.OnSkillUnlocked -= OnSkillChanged;
                skillManager.OnSkillLeveledUp -= OnSkillChanged;
                skillManager.OnLevelUp -= OnPlayerLevelUp;
                skillManager.OnSkillPointsChanged -= OnSkillPointsChanged;
                skillManager.OnExperienceGained -= OnExperienceGained;
            }
        }

        /// <summary>
        /// Refresh the entire UI
        /// </summary>
        public void RefreshUI()
        {
            if (skillManager == null)
                return;

            UpdatePlayerInfo();
            RefreshSkillList();
        }

        /// <summary>
        /// Update player level, XP, and skill points display
        /// </summary>
        private void UpdatePlayerInfo()
        {
            if (skillManager == null)
                return;

            var skillData = skillManager.GetSkillSaveData();
            
            if (skillData == null)
                return;

            if (levelText != null)
                levelText.text = $"Level: {skillData.level}";

            if (experienceText != null)
                experienceText.text = $"XP: {skillData.experience} / {skillData.experienceToNextLevel}";

            if (skillPointsText != null)
                skillPointsText.text = $"Skill Points: {skillData.skillPoints}";

            if (experienceBar != null)
            {
                experienceBar.maxValue = skillData.experienceToNextLevel;
                experienceBar.value = skillData.experience;
            }
        }

        /// <summary>
        /// Refresh the skill list based on current filter
        /// </summary>
        private void RefreshSkillList()
        {
            if (skillManager == null)
                return;

            // Clear existing buttons
            foreach (var skillButton in skillButtons)
            {
                if (skillButton.gameObject != null)
                    Destroy(skillButton.gameObject);
            }
            skillButtons.Clear();

            // Get skills based on filter
            List<Skill> skills;
            if (currentFilter.HasValue)
            {
                skills = skillManager.GetSkillsByCategory(currentFilter.Value);
            }
            else
            {
                skills = skillManager.GetAllSkills();
            }

            // Create skill buttons
            foreach (Skill skill in skills.OrderBy(s => s.tier).ThenBy(s => s.skillName))
            {
                CreateSkillButton(skill);
            }

            // Force rebuild layout to prevent stacking issues
            StartCoroutine(RebuildLayoutNextFrame());
        }

        /// <summary>
        /// Rebuild the layout in the next frame to ensure proper positioning
        /// </summary>
        private System.Collections.IEnumerator RebuildLayoutNextFrame()
        {
            yield return null; // Wait one frame
            
            if (skillListContainer != null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(skillListContainer as RectTransform);
            }
        }

        /// <summary>
        /// Create a button for a skill
        /// </summary>
        private void CreateSkillButton(Skill skill)
        {
            if (skillButtonPrefab == null || skillListContainer == null)
                return;

            GameObject buttonObj = Instantiate(skillButtonPrefab, skillListContainer);
            SkillButtonUI skillButtonUI = new SkillButtonUI
            {
                gameObject = buttonObj,
                skill = skill
            };

            // Get components
            skillButtonUI.button = buttonObj.GetComponent<Button>();
            skillButtonUI.nameText = buttonObj.transform.Find("SkillName")?.GetComponent<TMP_Text>();
            skillButtonUI.levelText = buttonObj.transform.Find("SkillLevel")?.GetComponent<TMP_Text>();
            skillButtonUI.icon = buttonObj.transform.Find("Icon")?.GetComponent<Image>();
            skillButtonUI.background = buttonObj.GetComponent<Image>();

            // Set text
            if (skillButtonUI.nameText != null)
                skillButtonUI.nameText.text = skill.skillName;

            if (skillButtonUI.levelText != null)
                skillButtonUI.levelText.text = skill.IsUnlocked ? $"Lv. {skill.currentLevel}/{skill.maxLevel}" : "Locked";

            // Set icon
            if (skillButtonUI.icon != null && skill.icon != null)
                skillButtonUI.icon.sprite = skill.icon;

            // Set color based on status
            UpdateSkillButtonColor(skillButtonUI);

            // Add click listener
            if (skillButtonUI.button != null)
                skillButtonUI.button.onClick.AddListener(() => SelectSkill(skill));

            skillButtons.Add(skillButtonUI);
        }

        /// <summary>
        /// Update skill button color based on unlock status
        /// </summary>
        private void UpdateSkillButtonColor(SkillButtonUI skillButton)
        {
            if (skillButton.background == null) return;

            var skillData = skillManager.GetSkillSaveData();
            Skill skill = skillButton.skill;

            if (skill.IsUnlocked)
            {
                skillButton.background.color = unlockedColor;
            }
            else if (skill.CanUnlock(skillData))
            {
                skillButton.background.color = canUnlockColor;
            }
            else
            {
                skillButton.background.color = lockedColor;
            }
        }

        /// <summary>
        /// Filter skills by category
        /// </summary>
        private void FilterSkills(SkillCategory? category)
        {
            currentFilter = category; // null = all skills, otherwise specific category
            RefreshSkillList();
        }

        /// <summary>
        /// Select a skill to show details
        /// </summary>
        private void SelectSkill(Skill skill)
        {
            selectedSkill = skill;
            ShowSkillDetails(skill);
        }

        /// <summary>
        /// Show detailed information about a skill
        /// </summary>
        private void ShowSkillDetails(Skill skill)
        {
            if (skillDetailPanel != null)
                skillDetailPanel.SetActive(true);

            var skillData = skillManager.GetSkillSaveData();

            // Basic info
            if (skillNameText != null)
                skillNameText.text = skill.skillName;

            if (skillDescriptionText != null)
            {
                string desc = skill.description;
                
                // Add effect details
                desc += "\n\nEffects:";
                foreach (var effect in skill.effects)
                {
                    float value = effect.GetValue(skill.currentLevel);
                    string valueStr = effect.isPercentage ? $"{value}%" : $"{value}";
                    desc += $"\n  • {effect.effectType}: {valueStr}";
                }

                // Add requirements if locked
                if (!skill.IsUnlocked)
                {
                    desc += "\n\nRequirements:";
                    desc += $"\n  • Level: {skill.requirements.minimumLevel}";
                    desc += $"\n  • Skill Points: {skill.requirements.requiredSkillPoints}";
                    
                    if (skill.requirements.prerequisiteSkillIds.Count > 0)
                    {
                        desc += "\n  • Prerequisites:";
                        foreach (string prereqId in skill.requirements.prerequisiteSkillIds)
                        {
                            Skill prereq = skillManager.GetSkill(prereqId);
                            if (prereq != null)
                            {
                                desc += $"\n    - {prereq.skillName}";
                            }
                        }
                    }
                }

                skillDescriptionText.text = desc;
            }

            if (skillLevelText != null)
                skillLevelText.text = skill.IsUnlocked ? 
                    $"Level: {skill.currentLevel}/{skill.maxLevel}" : 
                    "Locked";

            // Show appropriate button
            if (!skill.IsUnlocked)
            {
                if (unlockButton != null)
                {
                    unlockButton.gameObject.SetActive(true);
                    unlockButton.interactable = skill.CanUnlock(skillData);
                }
                if (levelUpButton != null)
                    levelUpButton.gameObject.SetActive(false);

                if (skillCostText != null)
                    skillCostText.text = $"Cost: {skill.requirements.requiredSkillPoints} SP";
            }
            else if (skill.CanLevelUp(skillData))
            {
                if (unlockButton != null)
                    unlockButton.gameObject.SetActive(false);
                if (levelUpButton != null)
                {
                    levelUpButton.gameObject.SetActive(true);
                    levelUpButton.interactable = true;
                }

                if (skillCostText != null)
                    skillCostText.text = $"Cost: {skill.GetLevelUpCost()} SP";
            }
            else
            {
                if (unlockButton != null)
                    unlockButton.gameObject.SetActive(false);
                if (levelUpButton != null)
                {
                    levelUpButton.gameObject.SetActive(true);
                    levelUpButton.interactable = false;
                }

                if (skillCostText != null)
                    skillCostText.text = skill.IsMaxLevel ? "Max Level" : "Insufficient Points";
            }

            // Update icon
            if (skillIcon != null && skill.icon != null)
                skillIcon.sprite = skill.icon;
        }

        /// <summary>
        /// Hide skill details panel and return to skill list
        /// </summary>
        private void HideSkillDetails()
        {
            if (skillDetailPanel != null)
            {
                skillDetailPanel.SetActive(false);
            }

            selectedSkill = null;
        }

        /// <summary>
        /// Close the entire skill tree and notify listeners (e.g., SaveStationMenu)
        /// </summary>
        public void CloseSkillTree()
        {
            // Close detail panel if open
            if (skillDetailPanel != null && skillDetailPanel.activeSelf)
            {
                HideSkillDetails();
            }

            // Notify listeners that skill tree is being closed
            OnSkillTreeClosed?.Invoke();
        }

        /// <summary>
        /// Unlock the selected skill
        /// </summary>
        private void UnlockSelectedSkill()
        {
            if (selectedSkill == null) return;

            if (skillManager.UnlockSkill(selectedSkill.skillId))
            {
                RefreshUI();
                ShowSkillDetails(selectedSkill); // Refresh details
            }
        }

        /// <summary>
        /// Level up the selected skill
        /// </summary>
        private void LevelUpSelectedSkill()
        {
            if (selectedSkill == null) return;

            if (skillManager.LevelUpSkill(selectedSkill.skillId))
            {
                RefreshUI();
                ShowSkillDetails(selectedSkill); // Refresh details
            }
        }

        #region Event Handlers

        private void OnSkillChanged(Skill skill)
        {
            RefreshUI();
            
            // Refresh details if it's the selected skill
            if (selectedSkill != null && selectedSkill.skillId == skill.skillId)
            {
                ShowSkillDetails(skill);
            }
        }

        private void OnPlayerLevelUp(int newLevel)
        {
            UpdatePlayerInfo();
        }

        private void OnSkillPointsChanged(int points)
        {
            UpdatePlayerInfo();
            
            // Update all button colors
            foreach (var skillButton in skillButtons)
            {
                UpdateSkillButtonColor(skillButton);
            }
        }

        private void OnExperienceGained(int amount)
        {
            UpdatePlayerInfo();
        }

        #endregion
    }
}
