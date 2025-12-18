using UnityEngine;
using UnityEditor;

public class SpriteTrimmer : EditorWindow
{
    [MenuItem("Tools/Trim All Sprites")]
    static void TrimAllSprites()
    {
        string[] guids = AssetDatabase.FindAssets("t:Sprite");
        
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            
            if (importer != null && importer.spriteImportMode == SpriteImportMode.Single)
            {
                // Enable Read/Write
                importer.isReadable = true;
                
                // Set to Tight mesh
                //importer.spriteMeshType = SpriteMeshType.Tight;
                
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            }
        }
        
        AssetDatabase.Refresh();
        Debug.Log($"Trimmed {guids.Length} sprites");
    }
}