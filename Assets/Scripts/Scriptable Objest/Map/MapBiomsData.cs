using System.Collections.Generic;
using System;
using UnityEngine;

[CreateAssetMenu(fileName = "MapBiomesData", menuName = "Scriptable Objects/MapBiomesData")]
public class MapBiomesData : ScriptableObject
{
    public List<Biom> mapBiomes;
}

[Serializable]
public struct Biom
{
    [field: SerializeField]
    public string id { get; set; }
    [field: SerializeField]
    public string name { get; set; }

    [field: SerializeField]
    public float frequency { get; set; }

    [field: SerializeField]
    public float size { get; set; }

    [field: SerializeField]
    public Sprite tileSprite;
}