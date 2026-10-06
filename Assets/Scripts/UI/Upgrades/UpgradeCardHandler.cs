using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UpgradeCardHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    private UpgradesController _upgradesController => UpgradesController.Instance;
    private PlayerController _playerController => PlayerController.Instance;
    private CursorManager _cursorManager => CursorManager.Instance;
    private InputManager _inputManager => InputManager.Instance;

    private Vector2 _originalPos = Vector2.zero;
    private bool _isHolding = false;

    private Transform _originalParent;
    private int _originalSiblingIndex;

    public void OnPointerDown(PointerEventData eventData)
    {
        Select();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        UnSelect();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_isHolding) return;
        _cursorManager.Change("grabHover");
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (_isHolding) return;
        _cursorManager.SetDefault();
    }

    private void Select()
    {
        _isHolding = true;

        _originalParent = transform.parent;
        _originalSiblingIndex = transform.GetSiblingIndex();

        transform.SetParent(_playerController.overlay, true);

        _cursorManager.Change("grab");

        _upgradesController.playerUpgradeCards.transform.Find("Background").gameObject.SetActive(true);
    }

    private void UnSelect()
    {
        RectTransform rectTransform = GetComponent<RectTransform>();

        Vector2 mousePosition = _inputManager.InputActions.Player.MousePosition.ReadValue<Vector2>();

        bool isInside = RectTransformUtility.RectangleContainsScreenPoint(
            _upgradesController.playerUpgradeCards.GetComponent<RectTransform>(),
            mousePosition,
            null
        );

        if (isInside)
        {
            int siblingIndex = GetClosestSiblingIndex(mousePosition);

            transform.SetParent(_upgradesController.playerUpgradeCards.transform, false);
            transform.SetSiblingIndex(siblingIndex);

            _isHolding = false;
            _cursorManager.SetDefault();
            _upgradesController.playerUpgradeCards.transform.Find("Background").gameObject.SetActive(false);

            UpgradeCard upgradeCard = gameObject.GetComponent<UpgradeCard>();
            _upgradesController.AddUpgrade(_upgradesController.upgradesToPick[upgradeCard.upgradeIndex]);

            _upgradesController.CloseUpgradesPopUp();
            return;
        }

        rectTransform.position = _originalPos;
        transform.SetParent(_originalParent, true);
        transform.SetSiblingIndex(_originalSiblingIndex);

        _isHolding = false;

        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        _upgradesController.playerUpgradeCards.transform.Find("Background").gameObject.SetActive(false);
    }

    private void LateUpdate()
    {
        HandleMovement();
    }

    private int GetClosestSiblingIndex(Vector2 mousePosition)
    {
        RectTransform deck = _upgradesController.playerUpgradeCards.GetComponent<RectTransform>();

        int closestIndex = 0;
        float closestDistance = float.MaxValue;

        for (int i = 0; i < deck.childCount; i++)
        {
            RectTransform slot = deck.GetChild(i).GetComponent<RectTransform>();

            Vector2 slotScreenPosition = RectTransformUtility.WorldToScreenPoint(null, slot.position);

            float distance = Vector2.Distance(mousePosition, slotScreenPosition);

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestIndex = i;
            }
        }

        return closestIndex;
    }

    private void HandleMovement()
    {
        if (!_isHolding) return;

        RectTransform rectTransform = GetComponent<RectTransform>();
        Vector2 mousePosition = _inputManager.InputActions.Player.MousePosition.ReadValue<Vector2>();

        Vector2 bordersOffset = new Vector2(0, 0);
        Vector2 screenSize = new Vector2(Screen.width, Screen.height);

        float minX = mousePosition.x - (rectTransform.rect.width / 2);
        float minY = mousePosition.y - (rectTransform.rect.height / 2);
        float maxX = mousePosition.x + (rectTransform.rect.width / 2);
        float maxY = mousePosition.y + (rectTransform.rect.height / 2);

        if (minX < 0) bordersOffset.x -= minX;
        if (minY < 0) bordersOffset.y -= minY;
        if (maxX > screenSize.x) bordersOffset.x -= maxX - screenSize.x;
        if (maxY > screenSize.y) bordersOffset.y -= maxY - screenSize.y;


        rectTransform.position = mousePosition + bordersOffset;
    }
}
