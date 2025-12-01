using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace VisionsForetold.Game.Player.Echo
{
    /// <summary>
    /// Quick setup helper for echolocation system
    /// Provides menu items to quickly create and configure the system
    /// </summary>
    public static class EchoSetupHelper
    {
#if UNITY_EDITOR
        [MenuItem("GameObject/Echolocation/Create Echo Manager", false, 10)]
        public static void CreateEchoManager()
        {
            // Create GameObject
            GameObject managerObj = new GameObject("EcholocationManager");
            
            // Add components in order
            var manager = managerObj.AddComponent<EchoManager>();
            var controller = managerObj.AddComponent<EcholocationController>();
            var detector = managerObj.AddComponent<EchoIntersectionDetector>();
            
            // Configure default settings for top-down game
            // Note: These will only apply if the fields are serialized as public or SerializeField
            Debug.Log("[EchoSetup] Created EcholocationManager with components");
            Debug.Log("[EchoSetup] Please assign the Echolocation material in the inspector!");
            
            // Select the created object
            Selection.activeGameObject = managerObj;
            
            // Focus on it in hierarchy
            EditorGUIUtility.PingObject(managerObj);
        }

        [MenuItem("GameObject/Echolocation/Create Echolocation Material", false, 11)]
        public static void CreateEcholocationMaterial()
        {
            // Find the echolocation shader
            Shader echoShader = Shader.Find("Custom/URP/Echolocation");
            
            if (echoShader == null)
            {
                Debug.LogError("[EchoSetup] Echolocation shader not found! Make sure 'Custom/URP/Echolocation' shader exists.");
                return;
            }
            
            // Create material
            Material material = new Material(echoShader);
            material.name = "EcholocationFog";
            
            // Configure material for fog coverage
            material.SetColor("_FogColor", Color.black);
            material.SetFloat("_FogDensity", 1.0f);
            material.SetFloat("_FogMinDensity", 0.95f);
            material.SetFloat("_FogMaxDensity", 1.0f);
            material.SetFloat("_FogDistanceFalloff", 100f);
            material.SetFloat("_PulseWidth", 5f);
            material.SetColor("_EdgeColor", new Color(0.3f, 0.6f, 1f, 1f));
            material.SetFloat("_EdgeIntensity", 1.5f);
            
            // Save material to Assets/Materials folder
            string folderPath = "Assets/Materials";
            if (!AssetDatabase.IsValidFolder(folderPath))
            {
                AssetDatabase.CreateFolder("Assets", "Materials");
            }
            
            string assetPath = AssetDatabase.GenerateUniqueAssetPath($"{folderPath}/EcholocationFog.mat");
            AssetDatabase.CreateAsset(material, assetPath);
            AssetDatabase.SaveAssets();
            
            // Select and ping the created material
            Selection.activeObject = material;
            EditorGUIUtility.PingObject(material);
            
            Debug.Log($"[EchoSetup] Created Echolocation material at: {assetPath}");
            Debug.Log("[EchoSetup] Drag this material to your EchoManager!");
        }

        [MenuItem("GameObject/Echolocation/Validate Setup", false, 30)]
        public static void ValidateSetup()
        {
            EchoManager manager = Object.FindFirstObjectByType<EchoManager>();
            
            if (manager == null)
            {
                EditorUtility.DisplayDialog(
                    "Echolocation Setup",
                    "No EchoManager found in scene!\n\nCreate one using:\nGameObject ? Echolocation ? Create Echo Manager",
                    "OK"
                );
                return;
            }
            
            // Validate setup
            var controller = manager.GetComponent<EcholocationController>();
            var detector = manager.GetComponent<EchoIntersectionDetector>();
            
            string report = "=== ECHOLOCATION SETUP VALIDATION ===\n\n";
            
            // Check manager
            report += $"? EchoManager found: {manager.name}\n";
            
            // Check components
            if (controller != null)
                report += "? EcholocationController found\n";
            else
                report += "? EcholocationController MISSING\n";
                
            if (detector != null)
                report += "? EchoIntersectionDetector found\n";
            else
                report += "? EchoIntersectionDetector MISSING\n";
            
            // Check player
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                report += $"? Player found: {player.name}\n";
            else
                report += "? Player NOT FOUND (tag 'Player' missing)\n";
            
            // Check camera
            Camera mainCam = Camera.main;
            if (mainCam != null)
                report += $"? Main Camera found: {mainCam.name}\n";
            else
                report += "? Main Camera NOT FOUND\n";
            
            // Check shader
            Shader shader = Shader.Find("Custom/URP/Echolocation");
            if (shader != null)
                report += "? Echolocation shader found\n";
            else
                report += "? Echolocation shader NOT FOUND\n";
            
            report += "\n";
            
            // Recommendations
            report += "NEXT STEPS:\n";
            if (controller != null && controller.GetFogMaterial() == null)
            {
                report += "1. Create Echolocation material (GameObject ? Echolocation ? Create Material)\n";
                report += "2. Assign material to EchoManager and EcholocationController\n";
            }
            if (player == null)
            {
                report += "• Tag your player GameObject as 'Player'\n";
            }
            
            Debug.Log(report);
            
            EditorUtility.DisplayDialog("Echolocation Setup", report, "OK");
        }

        [MenuItem("Tools/Echolocation/Open Setup Guide", false, 50)]
        public static void OpenSetupGuide()
        {
            string guidePath = "Assets/_Scripts/Game/Player/Echo/SETUP_GUIDE.md";
            var guideAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(guidePath);
            
            if (guideAsset != null)
            {
                Selection.activeObject = guideAsset;
                EditorGUIUtility.PingObject(guideAsset);
                Debug.Log("[EchoSetup] Setup guide selected. Open it in your text editor.");
            }
            else
            {
                Debug.LogWarning($"[EchoSetup] Setup guide not found at: {guidePath}");
            }
        }

        [MenuItem("Tools/Echolocation/Open Vertical Coverage Guide", false, 51)]
        public static void OpenVerticalCoverageGuide()
        {
            string guidePath = "Assets/_Scripts/Game/Player/Echo/VERTICAL_COVERAGE_GUIDE.md";
            var guideAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(guidePath);
            
            if (guideAsset != null)
            {
                Selection.activeObject = guideAsset;
                EditorGUIUtility.PingObject(guideAsset);
                Debug.Log("[EchoSetup] Vertical coverage guide selected. Open it in your text editor.");
            }
            else
            {
                Debug.LogWarning($"[EchoSetup] Vertical coverage guide not found at: {guidePath}");
            }
        }

        [MenuItem("Tools/Echolocation/Open Transparency Guide", false, 52)]
        public static void OpenTransparencyGuide()
        {
            string guidePath = "Assets/_Scripts/Game/Player/Echo/FOG_TRANSPARENCY_GUIDE.md";
            var guideAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(guidePath);
            
            if (guideAsset != null)
            {
                Selection.activeObject = guideAsset;
                EditorGUIUtility.PingObject(guideAsset);
                Debug.Log("[EchoSetup] Transparency guide selected. Open it in your text editor.");
            }
            else
            {
                Debug.LogWarning($"[EchoSetup] Transparency guide not found at: {guidePath}");
            }
        }
#endif
    }
}
