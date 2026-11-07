using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ObjectsData", menuName = "WorldGenerator/ObjectsData")]
public class WorldObjectsData : ScriptableObject
{
    public List<WorldObject> worldObjects;
}

[Serializable]
public struct WorldObject
{

    [field: SerializeField]
    public GameObject gameObject { get; set; }

    [field: SerializeField]
    public Vector2Int size { get; set; }

    [field: NonSerialized]
    public float frequency { get; set; }

    [field: SerializeField]
    public List<WorldObjectBioms> biomes { get; set; }
}

[Serializable]
public struct WorldObjectBioms
{
    [field: SerializeField]
    public string biomId { get; set; }

    [field: SerializeField]
    public float frequency { get; set; }
}