using System;
using UnityEngine;

public abstract class ItemUsage : ScriptableObject
{
    public float usageDuration = 1.0f;
    public bool remove = true;

    public virtual bool CanUseItem(UsableItem itemData)
    {
        return true;
    }

    public abstract void UseItem(UsableItem itemData);
}


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
    public string type { get; set; }
}

[Serializable]
public struct Item : IItem
{
    [field: SerializeField] public string id { get; set; }

    [field: SerializeField] public string name { get; set; }
    [field: SerializeField] public string description { get; set; }
    [field: SerializeField] public Sprite icon { get; set; }

    [field: SerializeField] public int maxStack { get; set; }

    [field: SerializeField] public string type { get; set; }
}