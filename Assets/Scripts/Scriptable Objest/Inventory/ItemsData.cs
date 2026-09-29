using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemsData", menuName = "Scriptable Objects/Items")]
public class ItemsData : ScriptableObject
{
    public List<Item> itemsData;
}

[Serializable]
public struct Item
{
    [field: SerializeField]
    public int id { get; set; }

    [field: SerializeField]
    public string name { get; set; }
    [field: SerializeField]
    public string description { get; set; }
    [field: SerializeField]
    public Sprite icon { get; set; }

    [field: SerializeField]
    public int maxStack { get; set; }

    [field: SerializeField]
    public string type { get; set; }

    [field: SerializeField]
    public bool isUsable { get; set; }
    [field: SerializeField]
    public float usageDuration { get; set; }
}
