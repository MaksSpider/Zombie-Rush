using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

public class UpgradeCardHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private UpgradesMenuController _upgradesMenuController => UpgradesMenuController.Instance;

    private GameObject _CardButton;
    private Transform _originalParent;

    private bool _hover = false;
    private bool _blockHover = false;

    public virtual void OnPointerEnter(PointerEventData eventData = null)
    {
        if (_hover || _blockHover) return;

        _hover = true;

        _CardButton = transform.Find("CardButton").gameObject;
        _originalParent = _CardButton.transform.parent;

        Vector3 position = _CardButton.GetComponent<RectTransform>().position;

        _CardButton.transform.SetParent(_upgradesMenuController.hoverLayer, true);
        _CardButton.transform.SetAsLastSibling();

        _CardButton.transform.GetComponent<RectTransform>().position = position + new Vector3(0, 15, 0);
    }

    public virtual void OnPointerExit(PointerEventData eventData = null)
    {
        if(!_hover) return;

        Vector3 position = _CardButton.GetComponent<RectTransform>().position;
        _CardButton.transform.GetComponent<RectTransform>().position = position - new Vector3(0, 15, 0);
        _CardButton.transform.SetParent(_originalParent, true);

        _hover = false;
    }
}