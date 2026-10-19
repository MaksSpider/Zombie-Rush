using System;
using UnityEngine;

[CreateAssetMenu(fileName = "Item", menuName = "Items/Item")]
public class ItemsData : ScriptableObject
{
    public Item item;
}

public interface IItem
{
    public string id { get; set; }
    public string name { get; set; }
    public string description { get; set; }
    public Sprite icon { get; set; }
    public int maxStack { get; set; }
    public Rarity rarity { get; set; }
}

[Serializable]
public struct Item : IItem
{
    [field: SerializeField] public string id { get; set; }

    [field: SerializeField] public string name { get; set; }
    [field: SerializeField] public string description { get; set; }
    [field: SerializeField] public Sprite icon { get; set; }

    [field: SerializeField] public int maxStack { get; set; }

    [field: SerializeField] public Rarity rarity { get; set; }
}