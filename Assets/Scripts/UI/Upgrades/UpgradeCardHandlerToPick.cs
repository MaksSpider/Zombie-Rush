using UnityEngine;
using UnityEngine.EventSystems;

public class UpgradeCardHandlerToPick : UpgradeCardHandler//, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    //private UpgradesMenuController _upgradesMenuController => UpgradesMenuController.Instance;
    //private UpgradesController _upgradesController => UpgradesController.Instance;
    //private CursorManager _cursorManager => CursorManager.Instance;
    //private InputManager _inputManager => InputManager.Instance;

    //private bool _isHolding = false;

    //private Transform _originalParent;
    //private int _originalSiblingIndex;

    //public void OnPointerDown(PointerEventData eventData)
    //{
    //    if (!_upgradesMenuController.openedUpgradesMenu) return;
    //    Select();
    //}

    //public void OnPointerUp(PointerEventData eventData)
    //{
    //    if (!_upgradesMenuController.openedUpgradesMenu) return;
    //    UnSelect();
    //}

    //public override void OnPointerEnter(PointerEventData eventData)
    //{
    //    if (!_upgradesMenuController.openedUpgradesMenu) return;
    //    if (_isHolding) return;

    //    base.OnPointerEnter(eventData);
    //    _cursorManager.Change("grabHover");
    //}

    //public override void OnPointerExit(PointerEventData eventData)
    //{
    //    if (!_upgradesMenuController.openedUpgradesMenu) return;
    //    if (_isHolding) return;

    //    base.OnPointerExit(eventData);
    //    _cursorManager.SetDefault();
    //}

    //private void LateUpdate()
    //{
    //    HandleMovement();
    //}

    //private void Select()
    //{
    //    if (_upgradesMenuController.holdingCard) return;
    //    _isHolding = true;

    //    base.OnPointerExit();

    //    _originalParent = transform.parent;
    //    _originalSiblingIndex = transform.GetSiblingIndex();

    //    transform.SetParent(_upgradesMenuController.hoverLayer, true);

    //    _cursorManager.Change("grab");

    //    _upgradesMenuController.SetActivePlayerCardsBackground(true);
    //    _upgradesMenuController.holdingCard = true;
    //}

    //private void UnSelect()
    //{
    //    _upgradesMenuController.holdingCard = false;

    //    RectTransform rectTransform = GetComponent<RectTransform>();

    //    Vector2 mousePosition = _inputManager.InputActions.Player.MousePosition.ReadValue<Vector2>();
    //    (bool insideDeck, int closestIndex) = _upgradesMenuController.GetPosition(mousePosition);

    //    if (!insideDeck)
    //    {
    //        transform.SetParent(_originalParent, true);
    //        transform.SetSiblingIndex(_originalSiblingIndex);
    //    }

    //    if (insideDeck)
    //    {
    //        _upgradesController.playerUpgrades.Add(GetComponent<UpgradeCard>());

    //        transform.SetParent(_upgradesMenuController.playerUpgrades.transform, true);
    //        transform.SetSiblingIndex(closestIndex);

    //        _upgradesMenuController.CloseUpgradesPicker();
    //    }

    //    _isHolding = false;
    //    _upgradesMenuController.SetActivePlayerCardsBackground(false);
    //    _cursorManager.SetDefault();
    //}

    //private void HandleMovement()
    //{
    //    if (!_isHolding) return;

    //    RectTransform rectTransform = GetComponent<RectTransform>();
    //    Vector2 mousePosition = _inputManager.InputActions.Player.MousePosition.ReadValue<Vector2>();

    //    Vector2 bordersOffset = new Vector2(0, 0);
    //    Vector2 screenSize = new Vector2(Screen.width, Screen.height);

    //    float minX = mousePosition.x - (rectTransform.rect.width / 2);
    //    float minY = mousePosition.y - (rectTransform.rect.height / 2);
    //    float maxX = mousePosition.x + (rectTransform.rect.width / 2);
    //    float maxY = mousePosition.y + (rectTransform.rect.height / 2);

    //    if (minX < 0) bordersOffset.x -= minX;
    //    if (minY < 0) bordersOffset.y -= minY;
    //    if (maxX > screenSize.x) bordersOffset.x -= maxX - screenSize.x;
    //    if (maxY > screenSize.y) bordersOffset.y -= maxY - screenSize.y;


    //    rectTransform.position = mousePosition + bordersOffset;
    //}
}
