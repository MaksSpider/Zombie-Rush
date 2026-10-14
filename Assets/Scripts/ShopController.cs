
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct ShopItem
{
    public string itemId;
    public string rarity;
    public int price;
    public int amount;
}

public class ShopController : MonoBehaviour
{
    private InventoryController _inventoryController => InventoryController.Instance;
    private InteractionsManager _interactionsManager => InteractionsManager.Instance;

    public List<ShopItem> shopItems;

    private int _interactionId;

    private void Start()
    {
        _interactionId = _interactionsManager.RegisterInteractionOnObject("Shop", 1.5f, gameObject, new Vector2(0, 0.5f), 1.5f, false, (int interactionId) => OpenShop());
    }

    private void OnDisable()
    {
        _interactionsManager.RemoveInteraction(_interactionId);
    }

    private void OnEnable()
    {
        _interactionId = _interactionsManager.RegisterInteractionOnObject("Shop", 1.5f, gameObject, new Vector2(0, 0.5f), 1.5f, false, (int interactionId) => OpenShop());
    }

    private void OnDestroy()
    {
        _interactionsManager.RemoveInteraction(_interactionId);
    }

    private void OpenShop()
    {

    }
}
