using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UpgradeCard : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    private UpgradesMenuController _upgradesMenuController => UpgradesMenuController.Instance;
    private PlayerController _playerController => PlayerController.Instance;
    private CursorManager _cursorManager => CursorManager.Instance;

    [NonSerialized] public Upgrade upgrade;

    [SerializeField] private GameObject _cardButton;

    [SerializeField] private Image icon;
    [SerializeField] private Image background;
    [SerializeField] private TextMeshProUGUI title;
    [SerializeField] private TextMeshProUGUI description;

    private Vector2 originalPos = Vector2.zero;
    private bool _hover = false;

    void Start()
    {
        UIEnterAnimations enterAnimations = _cardButton.GetComponent<UIEnterAnimations>();
        if (enterAnimations == null) return;

        _cardButton.GetComponent<UIEnterAnimations>().EnterAnimation();
    }

    public void UpdateUpgrade(Upgrade upgrade)
    {
        this.upgrade = upgrade;
        icon.sprite = upgrade.icon;
        background.color = upgrade.GetRarityColor();
        title.text = upgrade.name;
        description.text = upgrade.description;
    }

    public void OnPointerEnter(PointerEventData eventData = null)
    {
        if (_hover) return;
        _hover = true;

        _cursorManager.Change("pointer");

        if (_upgradesMenuController.addingAnimation) return;
        RectTransform rect = _cardButton.GetComponent<RectTransform>();
        originalPos = rect.anchoredPosition;
        rect.anchoredPosition = rect.anchoredPosition + new Vector2(0, 10);
    }

    public void OnPointerExit(PointerEventData eventData = null)
    {
        if (!_hover) return;
        _hover = false;

        _cursorManager.SetDefault();

        if (_upgradesMenuController.addingAnimation) return;
        RectTransform rect = _cardButton.GetComponent<RectTransform>();
        rect.anchoredPosition = originalPos;
    }

    public void OnPointerClick(PointerEventData eventData = null)
    {
        if(!_playerController.upgradesController.AddUpgrade(upgrade)) return;

        _upgradesMenuController.CloseUpgradesPicker();
    }
}