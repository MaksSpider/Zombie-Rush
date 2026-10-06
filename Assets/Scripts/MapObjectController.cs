using UnityEngine;

public class MapObjectController : DamageHandler
{
    private WorldGenerator _worldGenerator;
    void Awake()
    {
        _worldGenerator = WorldGenerator.Instance;
    }

    public override void OnObjectDestroy()
    {
        _worldGenerator.RemoveDestroyedMapObject(gameObject);
    }
}
