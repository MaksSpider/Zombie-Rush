using System;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class ShopItem
{
    [SerializeReference]
    public string itemId;
    public int price;
    public int amount;

    public ShopItem(string itemId, int price, int amount)
    {
        this.itemId = itemId;
        this.price = price;
        this.amount = amount;
    }
}

[Serializable]
public class ShopCategory
{
    public string index;
    public string name;
    public Sprite icon;

    public List<ShopItem> items;

    public ShopCategory(string name, Sprite icon, List<ShopItem> items)
    {
        this.name = name;
        this.icon = icon;
        this.items = items;
    }

    public bool AddItem(string itemId, int amount, int price = -1)
    {
        if (amount <= 0) return false;

        foreach (ShopItem shopItem in items)
        {
            if (shopItem.itemId == itemId)
            {
                shopItem.amount += amount;
                shopItem.price += price;

                if (shopItem.price < 0) shopItem.price = 0;

                return true;
            }
        }

        if (price < 0)
        {
            Debug.Log($"Item price must be higher then 0!");
            return false;
        }

        items.Add(new ShopItem(itemId, price, amount));

        return true;
    }

    public bool RemoveItem(string itemId, int amount)
    {
        if (amount <= 0) return false;

        for (int i = 0; i < items.Count; i++)
        {
            if (items[i].itemId == itemId)
            {
                items.RemoveAt(i);
                return true;
            }
        }

        return false;
    }
}

[Serializable]
public class Shop
{
    private ShopsManager _shopsManager => ShopsManager.Instance;

    public string index;

    public string name;
    public string currentPage;

    [SerializeField] public List<ShopCategory> shopData;

    public Shop(string index, string name, List<ShopCategory> shopData, string defaultPage = null)
    {
        this.shopData = shopData;

        if(defaultPage != null)
        { 
            currentPage = defaultPage;
        }
        else
        {
            currentPage = shopData[0].index;
        }
    }

    public void Open()
    {
        _shopsManager.Open(this);
    }

    public void Close()
    {
        _shopsManager.Close();
    }

    public bool AddItem(string categoryIndex, string itemId, int amount, int price = -1)
    {
        (bool result, ShopCategory shopCategory) = GetCategory(categoryIndex);
        if (!result)
        {
            Debug.Log($"Shop Category with index {categoryIndex} doesnt exist!");
            return false;
        }

        return shopCategory.AddItem(itemId, amount, price);
    }

    public bool RemoveItem(string categoryIndex, string itemId, int amount)
    {
        (bool result, ShopCategory shopCategory) = GetCategory(categoryIndex);
        if (!result)
        {
            Debug.Log($"Shop Category with index {categoryIndex} doesnt exist!");
            return false;
        }

        return shopCategory.RemoveItem(itemId, amount);
    }

    // TODO: implement
    public bool AddCategory()
    {
        return true;
    }

    // TODO: implement
    public bool RemoveCategory()
    {
        return true;
    }

    public (bool result, ShopCategory shopCategory) GetCategory(string index)
    {
        for (int i = 0; i < shopData.Count; i++)
        {
            if(shopData[i].index == index)
            {
                return (true, shopData[i]);
            }
        }

        return (false, null);
    }
}

public class ShopsManager : MonoBehaviour
{
    public static ShopsManager Instance { get; private set; }

    private InventoryController _inventoryController => InventoryController.Instance;
    private PlayerController _playerController => PlayerController.Instance;
    private StatesManager _statesManager => StatesManager.Instance;
    private InputManager _inputManager => InputManager.Instance;

    [SerializeField] private GameObject _shopMenu;
    [SerializeField] private Transform _shopItems;
    [SerializeField] private Transform _shopHeader;

    [SerializeField] private TextMeshProUGUI _titleText;

    [SerializeField] private GameObject _categoryPrefab;
    [SerializeField] private GameObject _itemPrefab;

    private List<Shop> _shops = new List<Shop>();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        _shopMenu.SetActive(false);
    }

    public (bool result, Shop shop) RegisterShop(string index, string title, List<ShopCategory> shopData)
    {
        if (ContainsShop(index))
        {
            Debug.Log($"Shop with index {index} already exist!");
            return (false, null);
        }

        Shop shop = new Shop(index, title, shopData);

        _shops.Add(shop);

        return (true, shop);
    }

    public void Open(Shop shop)
    {
        _statesManager.SwitchState<ShopState>();

        _titleText.text = shop.name;
        _shopMenu.SetActive(true);

        GenerateCategory(shop.shopData);

        LoadCategoryItems(shop.GetCategory(shop.currentPage).shopCategory);
        Canvas.ForceUpdateCanvases();
    }

    public void Close(UnityEngine.InputSystem.InputAction.CallbackContext ctx = default)
    {
        _shopMenu.SetActive(false);
        _statesManager.SwitchState<PlayingState>();
    }

    public bool ContainsShop(string index)
    {
        foreach (Shop shop in _shops)
        {
            if (shop.index == index) return true;
        }

        return false;
    }

    private void GenerateCategory(List<ShopCategory> shop)
    {
        for (int i = _shopHeader.childCount - 1; i >= 0; i--)
        {
            Transform child = _shopHeader.GetChild(i);
            if (child.name == "Line") continue;

            Destroy(child.gameObject);
        }

        foreach (ShopCategory category in shop)
        {
            GameObject categoryObject = Instantiate(_categoryPrefab, _shopHeader);
            categoryObject.name = $"category_{category.index}";

            categoryObject.GetComponent<Button>().onClick.AddListener(() => {
                LoadCategoryItems(category);
            });

            categoryObject.transform.Find("Icon").GetComponent<Image>().sprite = category.icon;
            categoryObject.transform.Find("Title").GetComponent<TextMeshProUGUI>().text = category.name;
        }
    }

    private void LoadCategoryItems(ShopCategory shopCategory)
    {
        for (int i = _shopItems.childCount - 1; i >= 0; i--)
        {
            Transform child = _shopItems.GetChild(i);

            Destroy(child.gameObject);
        }

        foreach(ShopItem shopItem in shopCategory.items)
        {
            (bool result, IItem item) = _inventoryController.GetItem(shopItem.itemId);
            if(!result)
            {
                Debug.Log($"Item with id {shopItem.itemId} doesnt exist!");
                continue;
            }

            GameObject itemObject = Instantiate(_itemPrefab, _shopItems);
            itemObject.name = $"item_{item.name}";

            UIShopItem uiShopItem = itemObject.GetComponent<UIShopItem>();

            uiShopItem.title.text = item.name;
            uiShopItem.amount.text = shopItem.amount.ToString();
            uiShopItem.price.text = shopItem.price.ToString("#,0", CultureInfo.InvariantCulture).Replace(",", ".");
            uiShopItem.icon.sprite = item.icon;

            uiShopItem.buyButton.onClick.AddListener(() => {
                TryToBuy(shopItem, uiShopItem);
            });
        }
    }

    private void TryToBuy(ShopItem shopItem, UIShopItem uIShopItem)
    {
        if (shopItem.amount < 1) 
        { 
            uIShopItem.Destroy(); 
            return; 
        }

        int amountToBuy = 1;
        if (_inputManager.InputActions.Player.SelectX5.IsPressed()) amountToBuy = 5;
        if (_inputManager.InputActions.Player.SelectAll.IsPressed()) amountToBuy = shopItem.amount;

        amountToBuy = Mathf.Clamp(amountToBuy, 1, shopItem.amount);

        (bool result, IItem item) = _inventoryController.GetItem(shopItem.itemId);
        if (!result)
        {
            Debug.Log($"Item with id {shopItem.itemId} doesnt exist!");
            return;
        }

        if (_playerController.Money < shopItem.price * amountToBuy) return;
        if (!_inventoryController.GetFirstEmptySlot(item, amountToBuy).result) return;

        _playerController.ChangeMoney(-shopItem.price * amountToBuy);
        _inventoryController.AddItem(item, amountToBuy);
        shopItem.amount -= amountToBuy;

        uIShopItem.amount.text = shopItem.amount.ToString();

        if (shopItem.amount < 1)
        {
            uIShopItem.Destroy();
        }
    }
}
