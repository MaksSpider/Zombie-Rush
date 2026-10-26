using UnityEngine;
using UnityEngine.EventSystems;

public class CursorOnPointer : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private CursorManager _cursorManager => CursorManager.Instance;

    public string cursorType = "pointer";

    public void OnPointerEnter(PointerEventData eventData)
    {
        _cursorManager.Change(cursorType);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _cursorManager.SetDefault();
    }
}