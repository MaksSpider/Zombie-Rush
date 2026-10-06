using UnityEngine;
using UnityEngine.EventSystems;

public class CursorPointer : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private CursorManager _cursorManager => CursorManager.Instance;

    public void OnPointerEnter(PointerEventData eventData)
    {
        _cursorManager.Change("pointer");
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _cursorManager.SetDefault();
    }
}