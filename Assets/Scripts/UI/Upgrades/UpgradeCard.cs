using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UpgradeCard : MonoBehaviour
{
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
}