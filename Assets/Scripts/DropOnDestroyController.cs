
using System.Collections.Generic;
using UnityEngine;

public class DropOnDestroyController : MonoBehaviour
{
    InventoryController _inventoryController => InventoryController.Instance;

    private bool _lootDroped = false;
    public List<ItemDrop> loot;

    public void DropLoot()
    {
        if (_lootDroped) return;

        (bool success, ItemDrop itemToDrop) = RandomItem(loot);
        if (!success) return;

        (bool finded, Item item) = _inventoryController.GetItemById(itemToDrop.itemId);
        if (!finded) return;

        int amount = UnityEngine.Random.Range(itemToDrop.amount.x, itemToDrop.amount.y + 1);

        Vector2 direction = UnityEngine.Random.insideUnitCircle.normalized;
        Vector2 spawnPosition = (Vector2)gameObject.transform.position + direction * 0.5f;

        _inventoryController.DropItem(item, amount, spawnPosition, direction);

        _lootDroped = true;
    }

    private (bool succes, ItemDrop itemDrop) RandomItem(List<ItemDrop> items)
    {
        if (items.Count == 0) return (false, new ItemDrop());

        float totalChance = 0f;

        foreach (ItemDrop itemToDrop in items)
        {
            totalChance += itemToDrop.chance;
        }

        if (totalChance <= 0f) return (false, new ItemDrop());

        float randomValue = UnityEngine.Random.Range(0f, totalChance);

        foreach (ItemDrop item in items)
        {
            randomValue -= Mathf.Max(0f, item.chance);

            if (randomValue <= 0f) return (true, item);
        }

        return (true, items[^1]);
    }
}
