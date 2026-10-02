using TMPro;
using UnityEngine;

public class TooltipController : MonoBehaviour
{
    public static TooltipController Instance;

    private InputManager _inputManager => InputManager.Instance;

    [SerializeField] private GameObject _tooltipWindow;
    [SerializeField] private Texture2D _infoCursor;
    [SerializeField] private Vector2 _hotspot;
    [SerializeField] private Vector2 _offset = new Vector2(24, -24);

    private bool _isTooltipVisible = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        DontDestroyOnLoad(gameObject);

        _tooltipWindow.SetActive(false);
    }

    private void LateUpdate()
    {
        if (!_isTooltipVisible) return;

        RectTransform rectTransform = _tooltipWindow.GetComponent<RectTransform>();

        Vector2 mousePosition = _inputManager.InputActions.Player.MousePosition.ReadValue<Vector2>();
        Vector2 mouseOffset = new Vector2(rectTransform.rect.width / 2, -(rectTransform.rect.height / 2));

        Vector2 position = mousePosition + mouseOffset + _offset;

        Vector2 bordersOffset = new Vector2(0, 0);
        Vector2 screenSize = new Vector2(Screen.width, Screen.height);

        float minX = position.x - (rectTransform.rect.width / 2);
        float minY = position.y - (rectTransform.rect.height / 2);
        float maxX = position.x + (rectTransform.rect.width / 2);
        float maxY = position.y + (rectTransform.rect.height / 2);

        if (minX < 0) bordersOffset.x -= minX;
        if (minY < 0) bordersOffset.y -= minY;
        if (maxX > screenSize.x) bordersOffset.x -= maxX - screenSize.x;
        if (maxY > screenSize.y) bordersOffset.y -= maxY - screenSize.y;


        rectTransform.position = position + bordersOffset;
    }

    public void Show(string title, string description = "")
    {
        Cursor.SetCursor(_infoCursor, _hotspot, CursorMode.Auto);

        _tooltipWindow.transform.Find("Title").GetComponent<TextMeshProUGUI>().text = title;
        _tooltipWindow.transform.Find("Description").GetComponent<TextMeshProUGUI>().text = description;

        _isTooltipVisible = true;
        _tooltipWindow.SetActive(true);
    }

    public void Close()
    {
        _isTooltipVisible = false;
        _tooltipWindow.SetActive(false);

        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }
}
