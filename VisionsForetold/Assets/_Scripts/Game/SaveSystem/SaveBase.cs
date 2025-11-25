using System;
using UnityEngine;

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

        // Skills (to be expanded based on your skill system)
        public SkillData skills;

        // Constructor
        public SaveData()
        {
            saveName = "New Save";
            saveDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            playtime = 0f;
            skills = new SkillData();
        }
    }

    /// <summary>
    /// Skills data structure
    /// </summary>
    [Serializable]
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
