
using UnityEngine;
using UnityEditor;
using UnityEditor.U2D.Sprites;
using System.Linq;

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
        GUILayout.Label(
            "Set Pivot to Bottom Center",
            EditorStyles.boldLabel
        );

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

                var factory =
                    new SpriteDataProviderFactories();

                factory.Init();

                var importer =
                    AssetImporter.GetAtPath(path) as TextureImporter;

                if (importer == null ||
                    importer.textureType != TextureImporterType.Sprite)
                    continue;

                var dataProvider =
                    factory.GetSpriteEditorDataProviderFromObject(importer);

                if (dataProvider == null)
                    continue;

                dataProvider.InitSpriteEditorDataProvider();

                var spriteRects = dataProvider.GetSpriteRects();

                foreach (var rect in spriteRects)
                {
                    rect.alignment = SpriteAlignment.Custom;
                    rect.pivot = new Vector2(0.5f, 0f);
                }

                dataProvider.SetSpriteRects(spriteRects);
                dataProvider.Apply();

                importer.SaveAndReimport();

                changed++;
            }

            Debug.Log($"Updated {changed} sprite textures.");
        }
    }
}