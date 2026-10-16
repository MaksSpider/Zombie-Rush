using System.Collections.Generic;
using UnityEngine;

public class ShopController : MonoBehaviour
{
    private InteractionsManager _interactionsManager => InteractionsManager.Instance;
    private ShopMenuController _shopMenuController => ShopMenuController.Instance;

    public List<ShopItem> shopItems;

    private int _interactionId;

    private void Start()
    {
        _interactionId = _interactionsManager.RegisterInteractionOnObject("Shop", 0.75f, gameObject, new Vector2(0, 0.5f), 1.5f, false, (int interactionId) => OpenShop());
    }

    private void OnDisable()
    {
        _interactionsManager.RemoveInteraction(_interactionId);
    }

    private void OnEnable()
    {
        _interactionId = _interactionsManager.RegisterInteractionOnObject("Shop", 0.75f, gameObject, new Vector2(0, 0.5f), 1.5f, false, (int interactionId) => OpenShop());
    }

    private void OnDestroy()
    {
        _interactionsManager.RemoveInteraction(_interactionId);
    }

    private void OpenShop()
    {

    }
}
