using UnityEngine;

public class ShopController : MonoBehaviour
{
    private InteractionsManager _interactionsManager => InteractionsManager.Instance;

    public Shop shop;

    private int _interactionId;

    private void Start()
    {
        _interactionId = _interactionsManager.RegisterInteractionOnObject(shop.name, 0.75f, gameObject, new Vector2(0, 0.5f), 1.5f, false, (int interactionId) => shop.Open());
    }

    private void OnDisable()
    {
        _interactionsManager.RemoveInteraction(_interactionId);
    }

    private void OnEnable()
    {
        _interactionId = _interactionsManager.RegisterInteractionOnObject(shop.name, 0.75f, gameObject, new Vector2(0, 0.5f), 1.5f, false, (int interactionId) => shop.Open());
    }

    private void OnDestroy()
    {
        _interactionsManager.RemoveInteraction(_interactionId);
    }
}
