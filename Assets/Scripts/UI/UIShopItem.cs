using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIShopItem : MonoBehaviour
{
    public TextMeshProUGUI title;
    public TextMeshProUGUI price;
    public TextMeshProUGUI amount;
    public Image icon;
    public Button buyButton;

    public void Destroy()
    {
        Destroy(gameObject);
    }
}
