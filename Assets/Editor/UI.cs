
using UnityEditor;
using UnityEngine;

public static class UIMenu
{

    [MenuItem("GameObject/Zombie Rush/ProgressBar", false, 4)]
    static void CreateProgressBar(MenuCommand menuCommand)
    {
        const string prefabPath =
            "Assets/Prefabs/UI/ProgressBar.prefab";

        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

        if (prefab == null)
        {
            Debug.LogError(
                $"Prefab not found at the path: {prefabPath}"
            );
            return;
        }

        GameObject parent = menuCommand.context as GameObject;

        GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);

        if (instance == null) return;

        GameObjectUtility.SetParentAndAlign(instance, parent);

        Undo.RegisterCreatedObjectUndo(instance, "Create ProgressBar");

        Selection.activeGameObject = instance;
    }

    [MenuItem("GameObject/Zombie Rush/ProgressBar", true)]
    static bool ValidateCreateProgressBar()
    {
        return !EditorApplication.isPlaying;
    }
}
