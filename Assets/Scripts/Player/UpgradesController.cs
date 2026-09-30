using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeSlot
{
    public Upgrade upgrade;

    public GameObject upgradeSlot;
    public Image iconImage;
    public Image background;
    public TextMeshProUGUI name;

    public UpgradeSlot(Upgrade upgrade, GameObject upgradeSlot, Image iconImage, Image background, TextMeshProUGUI name)
    {
        this.upgrade = upgrade;
        this.upgradeSlot = upgradeSlot;
        this.iconImage = iconImage;
        this.background = background;
        this.name = name;
    }
}

public class UpgradesController : MonoBehaviour
{
    public static UpgradesController Instance;

    private StatesManager _statesManager;

    public event Action OnUpgradesChange;

    [SerializeField] private List<Bonus> _upgradesChances;
    [SerializeField] private List<Bonus> _chanceIncreaseByLevels;
    [SerializeField] private GameObject _upgradesPickerPopUp;
    [SerializeField] private UpgradesData _upgradesData;

    [SerializeField] private List<Button> _upgradeButtons;

    [SerializeField] private GameObject _upgradePrefab;
    [SerializeField] private Transform _upgradesTransform;

    [SerializeField] private List<Sprite> _cardsByRarity = new List<Sprite>();

    private List<Upgrade> _upgradesToPick = new List<Upgrade>();
    private Dictionary<int, UpgradeSlot> _playerUpgrades = new Dictionary<int, UpgradeSlot>();
    
    [SerializeField] public int maxUpgradesAmount = 3;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        _statesManager = StatesManager.Instance;

        _upgradeButtons[0].onClick.AddListener(() => SelectUpgrade(0));
        _upgradeButtons[1].onClick.AddListener(() => SelectUpgrade(1));
        _upgradeButtons[2].onClick.AddListener(() => SelectUpgrade(2));
    }

    void Start()
    {
        Invoke(nameof(TestUpgrade), 0.1f);
    }

    void TestUpgrade()
    {
        AddUpgrade(_upgradesData.upgradesData[0]);
        AddUpgrade(_upgradesData.upgradesData[1]);
        AddUpgrade(_upgradesData.upgradesData[2]);

        InventoryController inventoryController = GetComponent<InventoryController>();
        inventoryController.AddItemById(3, 5);
        inventoryController.AddItemById(4, 5);
        inventoryController.AddItemById(5, 5);
        inventoryController.AddItemById(6, 5);
        inventoryController.AddItemById(7, 5);
        inventoryController.AddItemById(8, 1);
    }

    // TODO: if player dont have any aviable slot then select one to switch
    private void SelectUpgrade(int upgrade)
    {
        if (_upgradesToPick.Count <= 0) return;

        AddUpgrade(_upgradesToPick[upgrade]);
        _upgradesPickerPopUp.SetActive(false);
    }

    public void DisplayUpgradesWindow(int level)
    {
        _upgradesToPick.Clear();

        for(int i = 0; i <  _upgradesToPick.Count; i++)
        {
            _upgradesToPick.Add(RandomUpgrade(level).upgrade);
            _upgradeButtons[i].image.sprite = _cardsByRarity[GetTypeIndex(_upgradesToPick[i].type)];
            _upgradeButtons[i].transform.Find("Icon").GetComponent<Image>().sprite = _upgradesToPick[i].icon;
            _upgradeButtons[i].transform.Find("Title").GetComponent<TextMeshProUGUI>().text = _upgradesToPick[i].title;
            _upgradeButtons[i].transform.Find("Description").GetComponent<TextMeshProUGUI>().text = _upgradesToPick[i].description;
        }

        _upgradesPickerPopUp.SetActive(true);
        _statesManager.SwitchState<UpgradeState>();
    }


    // TODO: validate if player have this upgrade then remove from list
    public (bool finded, Upgrade upgrade) RandomUpgrade(int level)
    {
        List<Upgrade> upgradeList = new List<Upgrade>();
        List<float> chances = new List<float>();

        foreach (Upgrade upgrade in _upgradesData.upgradesData)
        {
            float multiplier = 1f;

            foreach (Bonus chance in _chanceIncreaseByLevels)
            {
                if (chance.name == upgrade.name)
                {
                    multiplier = chance.value;
                    break;
                }
            }

            foreach (Bonus chance in _upgradesChances)
            {
                if (chance.name == upgrade.name)
                {
                    upgradeList.Add(upgrade);
                    chances.Add(chance.value * (multiplier * level));
                    break;
                }
            }
        }

        if (upgradeList.Count == 0)
        {
            return (false, new Upgrade());
        }

        float totalChance = 0f;

        foreach (float chance in chances)
        {
            totalChance += chance;
        }

        float randomValue = UnityEngine.Random.Range(0f, totalChance);

        for (int i = 0; i < upgradeList.Count; i++)
        {
            randomValue -= chances[i];

            if (randomValue <= 0f)
            {
                return (true, upgradeList[i]);
            }
        }

        return (true, upgradeList[^1]);
    }

    public (bool exist, Upgrade upgrade) GetUpgrade(int slot)
    {
        if (!_playerUpgrades.ContainsKey(slot)) return (false, new Upgrade());

        return (true, _playerUpgrades[slot].upgrade);
    }

    public (bool exist, float value) GetUpgradeBonus(int slot, string bonusIndex)
    {
        if (!_playerUpgrades.ContainsKey(slot)) return (false, 0f);

        foreach(Bonus bonus in _playerUpgrades[slot].upgrade.bonuses)
        {
            if (bonus.name == bonusIndex)
            {
                return (true, bonus.value);
            }
        }

        return (false, 0f);
    }

    public float GetBonusValues(string bonusIndex)
    {
        float value = 0f;

        foreach(UpgradeSlot upgradeSlot in _playerUpgrades.Values)
        {
            foreach (Bonus bonus in upgradeSlot.upgrade.bonuses)
            {
                if (bonus.name == bonusIndex)
                {
                    value += bonus.value;
                }
            }
        }

        return value;
    }

    public bool AddUpgradeOnSlot(int slot, Upgrade upgrade)
    {
        if (maxUpgradesAmount <= slot) return false;

        _playerUpgrades[slot].upgrade = upgrade;
        OnUpgradesChange?.Invoke();
        return true;
    }

    public bool AddUpgrade(Upgrade upgrade)
    {
        var result = GetFirstFreeSlot();
        if (!result.exist) return false;

        GameObject upgradeSlotObject = Instantiate(_upgradePrefab, _upgradesTransform);
        upgradeSlotObject.name = $"slot_{result.slot}";

        Image iconImage = upgradeSlotObject.transform.Find("Icon").GetComponent<Image>();
        Image backgroundImage = upgradeSlotObject.transform.Find("Background").GetComponent<Image>();

        TextMeshProUGUI nameText = upgradeSlotObject.transform.Find("Name").GetComponent<TextMeshProUGUI>();

        Color color = GetTypeColor(upgrade.type);
        color.a = 150;

        iconImage.sprite = upgrade.icon;
        backgroundImage.color = color;
        nameText.text = upgrade.title;

        _playerUpgrades.Add(result.slot, new UpgradeSlot(upgrade, upgradeSlotObject,  iconImage, backgroundImage, nameText));
        OnUpgradesChange?.Invoke();
        return true;
    }

    public bool RemoveUpgrade(String upgradeName)
    {
        foreach((int slot, UpgradeSlot upgradeSlot) in _playerUpgrades)
        {
            if(upgradeName == upgradeSlot.upgrade.name)
            {
                RemoveUpgradeOnSlot(slot);
                OnUpgradesChange?.Invoke();
                return true;
            }
        }

        return false;
    }

    public bool RemoveUpgradeOnSlot(int slot)
    {
        if (!_playerUpgrades.ContainsKey(slot)) return false;

        _playerUpgrades.Remove(slot);
        OnUpgradesChange?.Invoke();
        return true;
    }

    public (bool exist, int slot) GetFirstFreeSlot()
    {
        for (int i = 0; i < maxUpgradesAmount; i++)
        {
            if(!_playerUpgrades.ContainsKey(i))
            {
                return (true, i);
            }
        }

        return (false, 0);
    }

    public Color GetTypeColor(string type)
    {
        switch (type)
        {
            case "common":
                return Color.white;
            case "rare":
                return new Color(0.25f, 0.55f, 1.0f);
            case "epic":
                return new Color(0.65f, 0.3f, 1.0f);
            case "legendary":
                return new Color(1.0f, 0.65f, 0.1f);
        }

        return Color.white;
    }

    public int GetTypeIndex(string type)
    {
        switch (type)
        {
            case "common":
                return 0;
            case "rare":
                return 1;
            case "epic":
                return 2;
            case "legendary":
                return 3;
        }

        return 0;
    }
}
