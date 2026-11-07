using System.Collections.Generic;
using System;
using UnityEngine;

[CreateAssetMenu(fileName = "BiomData", menuName = "WorldGenerator/BiomData")]
public class WorldBiomData : ScriptableObject
{
    public WorldBiom biom;
}

[Serializable]
public struct WorldBiom
{
    [field: SerializeField]
    public string id { get; set; }

    [field: SerializeField]
    public float frequency { get; set; }

    [field: SerializeField]
    public float size { get; set; }

    [field: SerializeField]
    public Sprite tileSprite;

    [field: SerializeField]
    public float vegetationDensity { get; set; }
}