using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "HealingItemsData", menuName = "Scriptable Objects/HealingItems")]
public class HealingItemsData : ScriptableObject
{
    public List<HealingItem> healingItems;
}

[Serializable]
public struct HealingItem
{
    [field: SerializeField]
    public int itemId { get; set; }

    [field: SerializeField]
    public int hpToAdd { get; set; }
    [field: SerializeField]
    public int regenerationToAdd { get; set; }
    [field: SerializeField]
    public int maxHpToAdd { get; set; }

    [field: SerializeField]
    public float duration { get; set; }
}
