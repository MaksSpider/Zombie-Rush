
using UnityEngine;
using UnityEditor;

public class BatchPivotSetter : EditorWindow
{
    private DefaultAsset targetFolder;

    [MenuItem("Tools/Batch Pivot Setter")]
    static void Open()
    {
        GetWindow<BatchPivotSetter>("Batch Pivot Setter");
    }

    void OnGUI()
    {
        GUILayout.Label("Set Pivot to Bottom Center",
            EditorStyles.boldLabel);

        targetFolder = (DefaultAsset)EditorGUILayout.ObjectField(
            "Target Folder",
            targetFolder,
            typeof(DefaultAsset),
            false
        );

        if (GUILayout.Button("Set Bottom Center"))
        {
            if (targetFolder == null)
            {
                Debug.LogWarning("Select a folder first!");
                return;
            }

            string folderPath =
                AssetDatabase.GetAssetPath(targetFolder);

            string[] guids = AssetDatabase.FindAssets(
                "t:Texture2D", new[] { folderPath });

            int changed = 0;

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);

                TextureImporter importer =
                    AssetImporter.GetAtPath(path) as TextureImporter;

                if (importer == null ||
                    importer.textureType != TextureImporterType.Sprite)
                    continue;

                importer.spritePivot = new Vector2(0.5f, 0f);

                EditorUtility.SetDirty(importer);
                importer.SaveAndReimport();

                changed++;
            }

            Debug.Log($"Updated {changed} sprite textures.");
        }
    }
}