using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UpgradeCard : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    private PlayerController _playerController => PlayerController.Instance;

    public Upgrade upgrade;

    [SerializeField] private Image icon;
    [SerializeField] private Image background;
    [SerializeField] private TextMeshProUGUI title;
    [SerializeField] private TextMeshProUGUI description;

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

    }

    public void OnPointerExit(PointerEventData eventData = null)
    {

    }

    public void OnPointerClick(PointerEventData eventData = null)
    {
        _playerController.upgradesController.AddUpgrade(upgrade);
    }
}