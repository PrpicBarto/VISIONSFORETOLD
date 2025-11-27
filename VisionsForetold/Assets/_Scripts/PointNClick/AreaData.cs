using UnityEngine;

namespace VisionsForetold.PointNClick
{
    /// <summary>
    /// ScriptableObject containing all data for a map area
    /// Create instances via: Assets > Create > Map System > Area Data
    /// </summary>
    [CreateAssetMenu(fileName = "NewArea", menuName = "Map System/Area Data", order = 1)]
    public class AreaData : ScriptableObject
    {
        [Header("Area Information")]
        [Tooltip("Display name of the area")]
        public string areaName = "Unnamed Area";
        
        [Tooltip("Short description shown when hovering")]
        [TextArea(2, 4)]
        public string shortDescription = "A mysterious location...";
        
        [Tooltip("Detailed description shown in info panel")]
        [TextArea(4, 8)]
        public string detailedDescription = "Full description of this area with lore and details.";

        [Header("Visual Assets")]
        [Tooltip("Default/idle state sprite")]
        public Sprite idleSprite;
        
        [Tooltip("Sprite shown when mouse hovers over area")]
        public Sprite hoverSprite;
        
        [Tooltip("Sprite shown when area is clicked/selected")]
        public Sprite selectedSprite;
        
        [Tooltip("Sprite shown when area is locked/unavailable")]
        public Sprite lockedSprite;

        [Header("Area State")]
        [Tooltip("Is this area currently accessible?")]
        public bool isUnlocked = true;
        
        [Tooltip("Has the player discovered this area?")]
        public bool isDiscovered = false;
        
        [Tooltip("Is this area marked as important/quest-related?")]
        public bool isQuestArea = false;

        [Header("Scene Loading")]
        [Tooltip("Name of the scene to load when entering this area")]
        public string sceneName;
        
        [Tooltip("Load scene asynchronously (recommended for large scenes)")]
        public bool loadAsync = true;

        [Header("Optional Features")]
        [Tooltip("Icon shown on the area (optional)")]
        public Sprite areaIcon;
        
        [Tooltip("Audio clip to play when hovering (optional)")]
        public AudioClip hoverSound;
        
        [Tooltip("Audio clip to play when clicking (optional)")]
        public AudioClip clickSound;

        /// <summary>
        /// Get the appropriate sprite based on current state
        /// </summary>
        public Sprite GetCurrentStateSprite(AreaState state)
        {
            return state switch
            {
                AreaState.Idle => idleSprite,
                AreaState.Hover => hoverSprite ?? idleSprite,
                AreaState.Selected => selectedSprite ?? hoverSprite ?? idleSprite,
                AreaState.Locked => lockedSprite ?? idleSprite,
                _ => idleSprite
            };
        }

        /// <summary>
        /// Validate that all required data is assigned
        /// </summary>
        public bool IsValid()
        {
            bool valid = true;

            if (string.IsNullOrEmpty(areaName))
            {
                Debug.LogWarning($"[{name}] Area name is empty!", this);
                valid = false;
            }

            if (idleSprite == null)
            {
                Debug.LogWarning($"[{name}] Idle sprite is missing!", this);
                valid = false;
            }

            if (string.IsNullOrEmpty(sceneName))
            {
                Debug.LogWarning($"[{name}] Scene name is empty! Area cannot be entered.", this);
                valid = false;
            }

            return valid;
        }
    }

    /// <summary>
    /// Visual states for map areas
    /// </summary>
    public enum AreaState
    {
        Idle,       // Default state
        Hover,      // Mouse is over area
        Selected,   // Area is clicked/active
        Locked      // Area is not accessible
    }
}
