using System;
using UnityEngine;

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
