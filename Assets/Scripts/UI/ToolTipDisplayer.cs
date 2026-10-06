using UnityEngine;
using UnityEngine.EventSystems;

public class ToolTipDisplayer : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private TooltipController _tooltipController => TooltipController.Instance;

    [SerializeField] public string title;
    [SerializeField] public string description;

    public void OnPointerEnter(PointerEventData eventData)
    {
        _tooltipController.Show(title, description);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _tooltipController.Close();
    }
}
