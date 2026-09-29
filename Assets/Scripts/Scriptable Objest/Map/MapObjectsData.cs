using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MapObjectsData", menuName = "Scriptable Objects/MapObjectsData")]
public class MapObjectsData : ScriptableObject
{
    public List<MapObject> mapObjects;
}

[Serializable]
public struct MapObject
{
    [field: SerializeField]
    public string biom { get; set; }
    [field: SerializeField]
    public string frequency { get; set; }

    [field: SerializeField]
    public GameObject gameObject { get; set; }
}