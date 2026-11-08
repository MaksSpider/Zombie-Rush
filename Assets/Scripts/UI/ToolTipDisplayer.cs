using UnityEngine;
using UnityEngine.EventSystems;

public class ToolTipDisplayer : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private TooltipController _tooltipController => TooltipController.Instance;

    [SerializeField] public string title;
    [SerializeField] public string description;

    public ToolTipDisplayer SetTitle(string title)
    {
        this.title = title;
        return this;
    }

    public ToolTipDisplayer SetDesc(string description)
    {
        this.description = description;
        return this;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _tooltipController.Show(title, description);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _tooltipController.Close();
    }
}
