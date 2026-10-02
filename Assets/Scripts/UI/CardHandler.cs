using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CardHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private InputManager _inputManager => InputManager.Instance;
    private PlayerController _playerController => PlayerController.Instance;

    [SerializeField] private Texture2D _pointerCursor;
    [SerializeField] private Texture2D _grabCursor;
    [SerializeField] private Vector2 _hotspot;

    private Button button => GetComponent<Button>();

    private Vector2 _direction;
    private Vector2 _originalPos = Vector2.zero;
    private bool _isHolding = false;

    private Transform _originalParent;
    private int _originalSiblingIndex;

    void Awake()
    {
        button.onClick.AddListener(() =>
        {
            SwitchState();
        });
    }

    private void SwitchState()
    {
        if(_isHolding)
        {
            UnSelect();
            return;
        }

        Select();
    }

    private void Select()
    {
        _isHolding = true;

        _originalParent = transform.parent;
        _originalSiblingIndex = transform.GetSiblingIndex();

        transform.SetParent(_playerController.overlay, true);

        Cursor.SetCursor(_grabCursor, _hotspot, CursorMode.Auto);
    }

    private void UnSelect()
    {
        // check if is near one of target positions

        _isHolding = false;

        // animation

        transform.SetParent(_originalParent, true);
        transform.SetSiblingIndex(_originalSiblingIndex);

        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }

    private void LateUpdate()
    {
        if (!_isHolding) return;

        RectTransform rectTransform = GetComponent<RectTransform>();

        Vector2 mousePosition = _inputManager.InputActions.Player.MousePosition.ReadValue<Vector2>();
        print(mousePosition);

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

    private void HandleMovement()
    {
        if (_direction == (Vector2)transform.position) return;

        Vector2 newPos = Vector2.Lerp(transform.position, _direction, Time.deltaTime * 10f);
        transform.position = _inputManager.InputActions.Player.MousePosition.ReadValue<Vector2>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_isHolding) return;
        Cursor.SetCursor(_pointerCursor, _hotspot, CursorMode.Auto);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (_isHolding) return;
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }
}
