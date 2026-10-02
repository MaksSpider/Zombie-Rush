using UnityEngine;
using UnityEngine.EventSystems;

public class CursorPointer : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Texture2D _pointerCursor;
    [SerializeField] private Vector2 _hotspot;

    public void OnPointerEnter(PointerEventData eventData)
    {
        Cursor.SetCursor(_pointerCursor, _hotspot, CursorMode.Auto);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }
}