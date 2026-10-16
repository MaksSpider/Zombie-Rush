using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using TMPro;
using Unity.Android.Gradle.Manifest;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public struct ShopItem
{
    public Item item;
    public int price;
    public int amount;
}

[Serializable]
public struct ShopCategory
{
    public string name;
    public Sprite icon;

    public List<ShopItem> shopItems;
}

public class ShopMenuController : MonoBehaviour
{
    public static ShopMenuController Instance { get; private set; }

    private StatesManager _statesManager => StatesManager.Instance;

    [SerializeField] private GameObject _shopMenu;
    [SerializeField] private Transform _shopItems;
    [SerializeField] private Transform _shopHeader;

    [SerializeField] private TextMeshProUGUI _titleText;

    [SerializeField] private GameObject _categoryPrefab;
    [SerializeField] private GameObject _itemPrefab;

    private List<ShopCategory> _currentShop;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void Open(string title, List<ShopCategory> shopData, int defaultCategoryIndex = 0)
    {
        _titleText.text = title;
        _shopMenu.SetActive(true);

        GenerateShopItems(shopData);

        LoadCategory(defaultCategoryIndex);
    }

    public void Close()
    {
        _shopMenu.SetActive(false);
    }

    private void GenerateShopItems(List<ShopCategory> shopData)
    {
        _currentShop = shopData;

        for (int i = _shopHeader.childCount - 1; i >= 0; i--)
        {
            Transform child = _shopHeader.GetChild(i);
            if (child.name == "Line") continue;

            Destroy(child.gameObject);
        }

        for(int i = 0; i < _currentShop.Count; i++ )
        {
            GameObject categoryObject = Instantiate(_categoryPrefab, _shopHeader);
            categoryObject.name = $"category_{_currentShop[i].name}";

            categoryObject.GetComponent<Button>().onClick.AddListener(() => {
                LoadCategory(i);
            });

            categoryObject.transform.Find("Icon").GetComponent<Image>().sprite = _currentShop[i].icon;
            categoryObject.transform.Find("Title").GetComponent<TextMeshProUGUI>().text = _currentShop[i].name;
        }
    }

    private void LoadCategory(int categoryIndex)
    {
        if (_currentShop.Count < categoryIndex) return;
        ShopCategory shopCategory = _currentShop[categoryIndex];

        for (int i = _shopItems.childCount - 1; i >= 0; i--)
        {
            Transform child = _shopItems.GetChild(i);

            Destroy(child.gameObject);
        }

        for (int i = 0; i < shopCategory.shopItems.Count; i++)
        {
            ShopItem currentItem = shopCategory.shopItems[i];

            GameObject itemObject = Instantiate(_itemPrefab, _shopItems);
            itemObject.name = $"item_{currentItem.item.name}";

            UIShopItem shopItem = itemObject.GetComponent<UIShopItem>();

            shopItem.title.text = currentItem.item.name;
            shopItem.amount.text = currentItem.amount.ToString();
            shopItem.price.text = currentItem.price.ToString("#,0", CultureInfo.InvariantCulture).Replace(",", ".");
            shopItem.icon.sprite = currentItem.item.icon;

            shopItem.buyButton.onClick.AddListener(() => {
                TryToBuy(i);
            });
        }
    }

    private void TryToBuy(int itemIndex)
    {

    }
}
