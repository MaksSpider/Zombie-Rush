using UnityEngine;

public class MapObjectController : ObjectController
{
    private WorldGenerator worldGenerator;
    void Awake()
    {
        worldGenerator = WorldGenerator.Instance;
    }

    private void OnDestroy()
    {
        CancelInvoke();

        if (!gameObject.scene.isLoaded) return;

        worldGenerator.RemoveObject(gameObject);
    }
}
