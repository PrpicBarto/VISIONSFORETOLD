using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Helper script to quickly setup the "To Be Continued" UI in the editor.
/// </summary>
#if UNITY_EDITOR
public class ToBeContinuedSetupHelper : EditorWindow
{
    [MenuItem("Tools/Game/Setup To Be Continued Screen")]
    public static void ShowWindow()
    {
        var window = GetWindow<ToBeContinuedSetupHelper>("TBC Setup");
        window.minSize = new Vector2(400, 500);
        window.Show();
    }

    private string displayText = "TO BE CONTINUED...";
    private string subtitleText = "The journey continues...";
    private Font customFont;
    private int fontSize = 72;
    private int subtitleFontSize = 32;

    private void OnGUI()
    {
        GUILayout.Space(10);
        EditorGUILayout.LabelField("To Be Continued Screen Setup", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("This will create a complete 'To Be Continued' screen setup in your current scene.", MessageType.Info);
        
        GUILayout.Space(10);
        
        // Settings
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("Text Settings", EditorStyles.boldLabel);
        displayText = EditorGUILayout.TextField("Display Text", displayText);
        subtitleText = EditorGUILayout.TextField("Subtitle", subtitleText);
        customFont = (Font)EditorGUILayout.ObjectField("Custom Font", customFont, typeof(Font), false);
        fontSize = EditorGUILayout.IntSlider("Font Size", fontSize, 24, 120);
        subtitleFontSize = EditorGUILayout.IntSlider("Subtitle Size", subtitleFontSize, 16, 72);
        EditorGUILayout.EndVertical();
        
        GUILayout.Space(10);
        
        // Create button
        GUI.backgroundColor = Color.green;
        if (GUILayout.Button("Create To Be Continued Screen", GUILayout.Height(40)))
        {
            CreateToBeContinuedScreen();
        }
        GUI.backgroundColor = Color.white;
        
        GUILayout.Space(10);
        
        // Instructions
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("Instructions:", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("1. Set your desired text and font", EditorStyles.wordWrappedLabel);
        EditorGUILayout.LabelField("2. Click 'Create To Be Continued Screen'", EditorStyles.wordWrappedLabel);
        EditorGUILayout.LabelField("3. The system will be added to your scene", EditorStyles.wordWrappedLabel);
        EditorGUILayout.LabelField("4. Adjust settings in Inspector if needed", EditorStyles.wordWrappedLabel);
        EditorGUILayout.EndVertical();
        
        GUILayout.Space(10);
        
        // Existing check
        ToBeContinuedManager existing = FindObjectOfType<ToBeContinuedManager>();
        if (existing != null)
        {
            EditorGUILayout.HelpBox("To Be Continued Manager already exists in scene!", MessageType.Warning);
            
            if (GUILayout.Button("Select Existing Manager"))
            {
                Selection.activeGameObject = existing.gameObject;
                EditorGUIUtility.PingObject(existing.gameObject);
            }
        }
    }

    private void CreateToBeContinuedScreen()
    {
        // Check if already exists
        ToBeContinuedManager existing = FindObjectOfType<ToBeContinuedManager>();
        if (existing != null)
        {
            if (!EditorUtility.DisplayDialog(
                "Already Exists",
                "A To Be Continued Manager already exists. Create another?",
                "Yes",
                "No"))
            {
                return;
            }
        }

        // Create Canvas
        GameObject canvasObj = new GameObject("ToBeContinuedCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        
        canvasObj.AddComponent<GraphicRaycaster>();

        // Create Fade Panel
        GameObject fadePanelObj = new GameObject("FadePanel");
        fadePanelObj.transform.SetParent(canvasObj.transform, false);
        
        RectTransform fadeRect = fadePanelObj.AddComponent<RectTransform>();
        fadeRect.anchorMin = Vector2.zero;
        fadeRect.anchorMax = Vector2.one;
        fadeRect.sizeDelta = Vector2.zero;
        
        Image fadeImage = fadePanelObj.AddComponent<Image>();
        fadeImage.color = Color.black;
        
        CanvasGroup fadeGroup = fadePanelObj.AddComponent<CanvasGroup>();
        fadeGroup.alpha = 0f;
        fadeGroup.blocksRaycasts = false;

        // Create Main Text
        GameObject textObj = new GameObject("ToBeContinuedText");
        textObj.transform.SetParent(canvasObj.transform, false);
        
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0.5f, 0.5f);
        textRect.anchorMax = new Vector2(0.5f, 0.5f);
        textRect.sizeDelta = new Vector2(1200, 200);
        textRect.anchoredPosition = new Vector2(0, 50);
        
        Text text = textObj.AddComponent<Text>();
        text.text = displayText;
        text.font = customFont != null ? customFont : Resources.GetBuiltinResource<Font>("Arial.ttf");
        text.fontSize = fontSize;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.white;
        text.supportRichText = true;
        
        // Add outline
        Outline outline = textObj.AddComponent<Outline>();
        outline.effectColor = Color.black;
        outline.effectDistance = new Vector2(3, -3);

        // Create Subtitle Text
        GameObject subtitleObj = new GameObject("SubtitleText");
        subtitleObj.transform.SetParent(canvasObj.transform, false);
        
        RectTransform subtitleRect = subtitleObj.AddComponent<RectTransform>();
        subtitleRect.anchorMin = new Vector2(0.5f, 0.5f);
        subtitleRect.anchorMax = new Vector2(0.5f, 0.5f);
        subtitleRect.sizeDelta = new Vector2(1000, 100);
        subtitleRect.anchoredPosition = new Vector2(0, -50);
        
        Text subtitle = subtitleObj.AddComponent<Text>();
        subtitle.text = subtitleText;
        subtitle.font = customFont != null ? customFont : Resources.GetBuiltinResource<Font>("Arial.ttf");
        subtitle.fontSize = subtitleFontSize;
        subtitle.alignment = TextAnchor.MiddleCenter;
        subtitle.color = new Color(0.8f, 0.8f, 0.8f, 1f);
        
        Outline subtitleOutline = subtitleObj.AddComponent<Outline>();
        subtitleOutline.effectColor = Color.black;
        subtitleOutline.effectDistance = new Vector2(2, -2);

        // Create Skip Prompt
        GameObject skipObj = new GameObject("SkipPromptText");
        skipObj.transform.SetParent(canvasObj.transform, false);
        
        RectTransform skipRect = skipObj.AddComponent<RectTransform>();
        skipRect.anchorMin = new Vector2(0.5f, 0f);
        skipRect.anchorMax = new Vector2(0.5f, 0f);
        skipRect.sizeDelta = new Vector2(600, 50);
        skipRect.anchoredPosition = new Vector2(0, 50);
        
        Text skipText = skipObj.AddComponent<Text>();
        skipText.text = "Press any key to continue";
        skipText.font = customFont != null ? customFont : Resources.GetBuiltinResource<Font>("Arial.ttf");
        skipText.fontSize = 24;
        skipText.alignment = TextAnchor.MiddleCenter;
        skipText.color = new Color(1f, 1f, 1f, 0.8f);
        
        skipObj.SetActive(false);

        // Create Manager
        GameObject managerObj = new GameObject("ToBeContinuedManager");
        ToBeContinuedManager manager = managerObj.AddComponent<ToBeContinuedManager>();
        
        // Use reflection to set private fields (since they're SerializeField)
        System.Type type = typeof(ToBeContinuedManager);
        
        type.GetField("toBeContinuedCanvas", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.SetValue(manager, canvas);
        type.GetField("toBeContinuedText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.SetValue(manager, text);
        type.GetField("subtitleText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.SetValue(manager, subtitle);
        type.GetField("fadePanel", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.SetValue(manager, fadeGroup);
        type.GetField("skipPromptText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.SetValue(manager, skipText);
        type.GetField("displayText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.SetValue(manager, displayText);
        type.GetField("subtitle", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.SetValue(manager, subtitleText);

        // Hide canvas initially
        canvasObj.SetActive(false);

        // Mark dirty
        EditorUtility.SetDirty(managerObj);
        EditorUtility.SetDirty(canvasObj);

        // Select manager
        Selection.activeGameObject = managerObj;
        EditorGUIUtility.PingObject(managerObj);

        Debug.Log("[ToBeContinuedSetup] Successfully created To Be Continued screen!");
        EditorUtility.DisplayDialog(
            "Success!",
            "To Be Continued screen created!\n\n" +
            "Manager: ToBeContinuedManager\n" +
            "Canvas: ToBeContinuedCanvas\n\n" +
            "Adjust settings in the Inspector as needed.",
            "OK"
        );
    }
}
#endif
