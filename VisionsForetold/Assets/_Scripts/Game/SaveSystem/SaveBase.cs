using System;
using UnityEngine;
using VisionsForetold.Game.SkillSystem;

namespace VisionsForetold.Game.SaveSystem
{
    /// <summary>
    /// Base class for save data containing player information, progress, and skills
    /// </summary>
    [Serializable]
    public class SaveData
    {
        // Save meta info
        public string saveName;
        public string saveDate;
        public float playtime;
        public int saveSlotIndex;

        // Player stats
        public int playerHealth;
        public int playerMaxHealth;
        public Vector3 playerPosition;
        public Quaternion playerRotation;
        public string currentSceneName;

        // Map system integration
        public string lastMapScene = "MapScene"; // Last map scene player was on
        public string returnAreaId; // AreaData name/ID to return to on map
        public Vector3 saveStationPosition; // Position of save station used

        // Skills (integrated with SkillSystem)
        public SkillSaveData skills;

        // Constructor
        public SaveData()
        {
            saveName = "New Save";
            saveDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            playtime = 0f;
            skills = new SkillSaveData();
            lastMapScene = "MapScene";
            returnAreaId = "";
        }
    }

    /// <summary>
    /// DEPRECATED: Old SkillData structure - use SkillSaveData instead
    /// Kept for backward compatibility
    /// </summary>
    [Serializable]
    [System.Obsolete("Use SkillSaveData from SkillSystem namespace instead")]
    public class SkillData
    {
        public int skillPoints;
        public int level;
        public int experience;

        // Skill tree unlocks (example structure - customize as needed)
        public bool[] unlockedSkills;

        public SkillData()
        {
            skillPoints = 0;
            level = 1;
            experience = 0;
            unlockedSkills = new bool[20]; // Adjust size based on your skill tree
        }
    }
}
