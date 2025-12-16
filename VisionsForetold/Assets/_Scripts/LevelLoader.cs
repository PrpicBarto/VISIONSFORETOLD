// Unity Level Loader Script
// Place this file in your Unity project's Scripts folder
// To use: Attach to a GameObject or call LevelLoader.LoadLevel("levelName") from code

using UnityEngine;
using System.Collections.Generic;
using System.IO;

namespace UnityLevelEditor.Unity
{
    [System.Serializable]
    public class UnityLevelData
    {
        public string levelName;
        public int gridWidth;
        public int gridHeight;
        public float tileSize;
        public GroundPlaneData groundPlane;
        public List<AssetData> assets;
    }

    [System.Serializable]
    public class GroundPlaneData
    {
        public Vector3Data position;
        public Vector3Data scale;
        public Vector3Data rotation;
    }

    [System.Serializable]
    public class AssetData
    {
        public string assetId;
        public string prefabName;
        public Vector3Data position;
        public Vector3Data rotation;
        public Vector3Data scale;
        public string layer;
        public Dictionary<string, object> customProperties;
    }

    [System.Serializable]
    public class Vector3Data
    {
        public float x;
        public float y;
        public float z;

        public Vector3 ToVector3()
        {
            return new Vector3(x, y, z);
        }

        public static Vector3Data FromVector3(Vector3 v)
        {
            return new Vector3Data { x = v.x, y = v.y, z = v.z };
        }
    }

    public class LevelLoader : MonoBehaviour
    {
        [Header("Level Settings")]
        [Tooltip("Name of the level JSON file (without .json extension)")]
        public string levelFileName = "MyLevel_unity";

        [Tooltip("Parent transform for all spawned objects")]
        public Transform levelParent;

        [Header("Ground Plane")]
        [Tooltip("Create ground plane automatically")]
        public bool createGroundPlane = true;
        
        [Tooltip("Material for ground plane")]
        public Material groundMaterial;

        [Header("Prefab Mapping")]
        [Tooltip("Map prefab paths to actual Unity prefabs")]
        public List<PrefabMapping> prefabMappings = new List<PrefabMapping>();

        private Dictionary<string, GameObject> _prefabDictionary;

        [System.Serializable]
        public class PrefabMapping
        {
            public string prefabPath;
            public GameObject prefab;
        }

        void Start()
        {
            BuildPrefabDictionary();
            
            if (!string.IsNullOrEmpty(levelFileName))
            {
                LoadLevel(levelFileName);
            }
        }

        private void BuildPrefabDictionary()
        {
            _prefabDictionary = new Dictionary<string, GameObject>();
            
            foreach (var mapping in prefabMappings)
            {
                if (!string.IsNullOrEmpty(mapping.prefabPath) && mapping.prefab != null)
                {
                    _prefabDictionary[mapping.prefabPath] = mapping.prefab;
                }
            }
        }

        public void LoadLevel(string fileName)
        {
            // Try to load from Resources folder
            TextAsset jsonFile = Resources.Load<TextAsset>(fileName);
            
            if (jsonFile == null)
            {
                Debug.LogError($"Level file not found in Resources folder: {fileName}.json");
                return;
            }

            try
            {
                UnityLevelData levelData = JsonUtility.FromJson<UnityLevelData>(jsonFile.text);
                SpawnLevel(levelData);
                Debug.Log($"Successfully loaded level: {levelData.levelName}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to load level: {e.Message}");
            }
        }

        public void LoadLevelFromPath(string fullPath)
        {
            if (!File.Exists(fullPath))
            {
                Debug.LogError($"Level file not found: {fullPath}");
                return;
            }

            try
            {
                string json = File.ReadAllText(fullPath);
                UnityLevelData levelData = JsonUtility.FromJson<UnityLevelData>(json);
                SpawnLevel(levelData);
                Debug.Log($"Successfully loaded level: {levelData.levelName}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to load level: {e.Message}");
            }
        }

        private void SpawnLevel(UnityLevelData levelData)
        {
            // Clear existing level
            ClearLevel();

            // Create parent if not assigned
            if (levelParent == null)
            {
                GameObject parentObj = new GameObject(levelData.levelName);
                levelParent = parentObj.transform;
            }

            // Create ground plane
            if (createGroundPlane && levelData.groundPlane != null)
            {
                CreateGroundPlane(levelData.groundPlane);
            }

            // Spawn all assets
            foreach (var assetData in levelData.assets)
            {
                SpawnAsset(assetData);
            }
        }

        private void CreateGroundPlane(GroundPlaneData groundData)
        {
            GameObject plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
            plane.name = "Ground";
            plane.transform.parent = levelParent;
            
            if (groundData.position != null)
                plane.transform.position = groundData.position.ToVector3();
            
            if (groundData.scale != null)
                plane.transform.localScale = groundData.scale.ToVector3();
            
            if (groundData.rotation != null)
                plane.transform.rotation = Quaternion.Euler(groundData.rotation.ToVector3());

            if (groundMaterial != null)
            {
                plane.GetComponent<Renderer>().material = groundMaterial;
            }
        }

        private void SpawnAsset(AssetData assetData)
        {
            // Try to find prefab
            GameObject prefab = null;
            
            if (_prefabDictionary.ContainsKey(assetData.prefabName))
            {
                prefab = _prefabDictionary[assetData.prefabName];
            }
            else
            {
                // Try to load from Resources
                prefab = Resources.Load<GameObject>(assetData.prefabName);
            }

            if (prefab == null)
            {
                Debug.LogWarning($"Prefab not found: {assetData.prefabName}. Creating placeholder.");
                prefab = CreatePlaceholder(assetData);
            }

            // Instantiate
            GameObject instance = Instantiate(prefab, levelParent);
            instance.name = $"{prefab.name}_{assetData.assetId}";

            // Set transform
            if (assetData.position != null)
                instance.transform.position = assetData.position.ToVector3();
            
            if (assetData.rotation != null)
                instance.transform.rotation = Quaternion.Euler(assetData.rotation.ToVector3());
            
            if (assetData.scale != null)
                instance.transform.localScale = assetData.scale.ToVector3();

            // Set layer
            if (!string.IsNullOrEmpty(assetData.layer))
            {
                int layerIndex = LayerMask.NameToLayer(assetData.layer);
                if (layerIndex >= 0)
                {
                    instance.layer = layerIndex;
                }
            }

            // Apply custom properties if needed
            if (assetData.customProperties != null && assetData.customProperties.Count > 0)
            {
                var customData = instance.AddComponent<CustomAssetData>();
                customData.properties = assetData.customProperties;
            }
        }

        private GameObject CreatePlaceholder(AssetData assetData)
        {
            GameObject placeholder = GameObject.CreatePrimitive(PrimitiveType.Cube);
            placeholder.name = $"Placeholder_{assetData.assetId}";
            
            // Color based on layer
            var renderer = placeholder.GetComponent<Renderer>();
            switch (assetData.layer)
            {
                case "Environment":
                    renderer.material.color = new Color(0.6f, 0.4f, 0.2f);
                    break;
                case "Gameplay":
                    renderer.material.color = Color.yellow;
                    break;
                case "Enemies":
                    renderer.material.color = Color.red;
                    break;
                case "Items":
                    renderer.material.color = Color.cyan;
                    break;
                default:
                    renderer.material.color = Color.gray;
                    break;
            }
            
            return placeholder;
        }

        public void ClearLevel()
        {
            if (levelParent == null) return;

            // Destroy all children
            while (levelParent.childCount > 0)
            {
                DestroyImmediate(levelParent.GetChild(0).gameObject);
            }
        }

        // Optional: Component to store custom properties
        public class CustomAssetData : MonoBehaviour
        {
            public Dictionary<string, object> properties = new Dictionary<string, object>();

            public T GetProperty<T>(string key, T defaultValue = default)
            {
                if (properties.TryGetValue(key, out var value))
                {
                    try
                    {
                        return (T)value;
                    }
                    catch
                    {
                        return defaultValue;
                    }
                }
                return defaultValue;
            }
        }
    }

    // Editor utility to load levels at edit time
    #if UNITY_EDITOR
    [UnityEditor.CustomEditor(typeof(LevelLoader))]
    public class LevelLoaderEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            LevelLoader loader = (LevelLoader)target;

            UnityEditor.EditorGUILayout.Space();
            
            if (GUILayout.Button("Load Level"))
            {
                if (!string.IsNullOrEmpty(loader.levelFileName))
                {
                    loader.LoadLevel(loader.levelFileName);
                }
                else
                {
                    Debug.LogWarning("Please specify a level file name.");
                }
            }

            if (GUILayout.Button("Clear Level"))
            {
                loader.ClearLevel();
            }

            UnityEditor.EditorGUILayout.HelpBox(
                "1. Export your level from the Unity Level Editor\n" +
                "2. Place the JSON file in Assets/Resources/\n" +
                "3. Enter the filename (without .json)\n" +
                "4. Map your prefabs in the Prefab Mapping list\n" +
                "5. Click 'Load Level' or run the scene",
                UnityEditor.MessageType.Info);
        }
    }
    #endif
}
