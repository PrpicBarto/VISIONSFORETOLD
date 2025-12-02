using UnityEngine;
using System.Collections.Generic;

namespace VisionsForetold.PointNClick
{
    /// <summary>
    /// Manages connections between scenes and areas
    /// Tracks which scenes unlock which areas, and handles scene transitions
    /// </summary>
    [CreateAssetMenu(fileName = "SceneConnections", menuName = "Map System/Scene Connections", order = 2)]
    public class SceneConnectionData : ScriptableObject
    {
        [System.Serializable]
        public class SceneConnection
        {
            [Tooltip("Name of the scene")]
            public string sceneName;
            
            [Tooltip("Areas unlocked when this scene is completed/visited")]
            public List<AreaData> unlockedAreas = new List<AreaData>();
            
            [Tooltip("Areas that become locked (optional)")]
            public List<AreaData> lockedAreas = new List<AreaData>();
            
            [Tooltip("Return map scene name (where to go when exiting this scene)")]
            public string returnMapScene = "MapScene";
            
            [Tooltip("Auto-unlock areas when scene loads")]
            public bool autoUnlock = true;
        }

        [Header("Scene Connections")]
        [Tooltip("Define which scenes unlock which areas")]
        public List<SceneConnection> connections = new List<SceneConnection>();

        [Header("Default Settings")]
        [Tooltip("Default map scene to return to")]
        public string defaultMapScene = "MapScene";

        /// <summary>
        /// Get connection data for a specific scene
        /// </summary>
        public SceneConnection GetConnection(string sceneName)
        {
            return connections.Find(c => c.sceneName == sceneName);
        }

        /// <summary>
        /// Get areas unlocked by a scene
        /// </summary>
        public List<AreaData> GetUnlockedAreas(string sceneName)
        {
            var connection = GetConnection(sceneName);
            return connection?.unlockedAreas ?? new List<AreaData>();
        }

        /// <summary>
        /// Get return map scene for a specific scene
        /// </summary>
        public string GetReturnMapScene(string sceneName)
        {
            var connection = GetConnection(sceneName);
            return connection?.returnMapScene ?? defaultMapScene;
        }
    }
}
