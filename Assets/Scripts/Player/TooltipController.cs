using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TooltipController : MonoBehaviour
{
    public static TooltipController Instance;

    private InputManager _inputManager => InputManager.Instance;
    private CursorManager _cursorManager => CursorManager.Instance;

    [SerializeField] private GameObject _tooltipWindow;
    [SerializeField] private Texture2D _infoCursor;
    [SerializeField] private Vector2 _hotspot;
    [SerializeField] private Vector2 _offset = new Vector2(24, -24);

    private RectTransform _tooltipRect;
    private TextMeshProUGUI _titleText;
    private TextMeshProUGUI _descriptionText;

    public bool _isTooltipVisible;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        _tooltipRect = _tooltipWindow.GetComponent<RectTransform>();
        _titleText = _tooltipWindow.transform.Find("Title").GetComponent<TextMeshProUGUI>();
        _descriptionText = _tooltipWindow.transform.Find("Description").GetComponent<TextMeshProUGUI>();

        _tooltipWindow.SetActive(false);
    }

    private void LateUpdate()
    {
        if (!_isTooltipVisible) return;

        Vector2 mousePosition = _inputManager.InputActions.Player.MousePosition.ReadValue<Vector2>();

        float width = _tooltipRect.rect.width;
        float height = _tooltipRect.rect.height;

        Vector2 position = mousePosition + new Vector2(width / 2f, -height / 2f) + _offset;

        Vector2 screenSize = new Vector2(Screen.width, Screen.height);
        Vector2 bordersOffset = Vector2.zero;

        float minX = position.x - width / 2f;
        float minY = position.y - height / 2f;
        float maxX = position.x + width / 2f;
        float maxY = position.y + height / 2f;

        if (minX < 0) bordersOffset.x -= minX;

        if (minY < 0) bordersOffset.y -= minY;

        if (maxX > screenSize.x) bordersOffset.x -= maxX - screenSize.x;

        if (maxY > screenSize.y) bordersOffset.y -= maxY - screenSize.y;

        _tooltipRect.position = position + bordersOffset;
    }

    public void Show(string title, string description = "")
    {
        _cursorManager.Change("help");

        _tooltipWindow.SetActive(true);

        _titleText.text = title;
        _descriptionText.text = description;

        _titleText.ForceMeshUpdate();
        _descriptionText.ForceMeshUpdate();

        LayoutRebuilder.ForceRebuildLayoutImmediate(_tooltipRect);

        _isTooltipVisible = true;
    }

    public void Close()
    {
        _isTooltipVisible = false;
        _tooltipWindow.SetActive(false);

        _cursorManager.SetDefault();
    }
}