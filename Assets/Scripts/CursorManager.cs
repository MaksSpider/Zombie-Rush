using UnityEngine;

public class CursorManager : MonoBehaviour
{
    public static CursorManager Instance;

    [SerializeField] private CursorsData _cursorsData;

    private CursorData _currentDefault;
    private CursorData _currentCursor;
    private CursorData _lastCursor;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        _lastCursor = _cursorsData.cursors[0];
        _currentDefault = _cursorsData.cursors[0];
    }

    public void SetLast()
    {
        Cursor.SetCursor(_lastCursor.texture, _lastCursor.hotspot, CursorMode.Auto);

        CursorData old = _currentCursor;
        _currentCursor = _lastCursor;
        _lastCursor = old;
    }

    public void SetNormal()
    {
        _lastCursor = _currentCursor;
        _currentCursor = _cursorsData.cursors[0];
        _currentDefault = _cursorsData.cursors[0];
        Cursor.SetCursor(null, new Vector2(0, 0), CursorMode.Auto);
    }

    public void ChangeCurrentDefault(string cursorName)
    {
        foreach (CursorData cursorData in _cursorsData.cursors)
        {
            if (cursorData.name == cursorName)
            {
                _currentDefault = cursorData;
                return;
            }
        }
    }

    public void SetCustom(Texture2D texture, Vector2 hotspot)
    {
        _lastCursor = _currentCursor;
        _currentCursor = new CursorData
        {
            name = "",
            texture = texture,
            hotspot = hotspot
        };

        Cursor.SetCursor(texture, hotspot, CursorMode.Auto);
    }

    public void SetDefault()
    {
        _lastCursor = _currentCursor;
        _currentCursor = _currentDefault;
        Cursor.SetCursor(_currentDefault.texture, _currentDefault.hotspot, CursorMode.Auto);
    }

    public bool Change(string cursorName)
    {
        foreach(CursorData cursorData in _cursorsData.cursors)
        {
            if(cursorData.name == cursorName)
            {
                _lastCursor = _currentCursor;
                _currentCursor = cursorData;
                Cursor.SetCursor(cursorData.texture, cursorData.hotspot, CursorMode.Auto);
                return true;
            }
        }

        return false;
    }
}
