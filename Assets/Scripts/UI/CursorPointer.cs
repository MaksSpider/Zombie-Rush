using UnityEngine;
using UnityEngine.EventSystems;

public class CursorPointer : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Texture2D pointerCursor;
    public Vector2 hotspot;

    public void OnPointerEnter(PointerEventData eventData)
    {
        Cursor.SetCursor(pointerCursor, hotspot, CursorMode.Auto);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }
}