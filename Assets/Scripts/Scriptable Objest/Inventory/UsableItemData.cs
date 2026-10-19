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

[CreateAssetMenu(fileName = "UsableItem", menuName = "Items/UsableItem")]
public class UsableItemData : ScriptableObject
{
    public UsableItem usableItem;
}

[Serializable]
public struct UsableItem : IItem
{
    [field: SerializeField] public string id { get; set; }

    [field: SerializeField] public string name { get; set; }
    [field: SerializeField] public string description { get; set; }
    [field: SerializeField] public Sprite icon { get; set; }

    [field: SerializeField] public int maxStack { get; set; }

    [field: SerializeField] public string type { get; set; }

    [field: SerializeField] public ItemUsage usageFunction { get; set; }
}
