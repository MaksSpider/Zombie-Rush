using UnityEngine;

public class MapObjectController : ObjectController
{
    private WorldGenerator _worldGenerator;
    void Awake()
    {
        _worldGenerator = WorldGenerator.Instance;
    }

    private void OnDestroy()
    {
        CancelInvoke();

        if (!gameObject.scene.isLoaded) return;
        _worldGenerator.RemoveDestroyedMapObject(gameObject);
    }
}
