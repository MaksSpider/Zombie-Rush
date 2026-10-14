using System;
using System.Collections;
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

    private StatesManager _statesManager => StatesManager.Instance;
    private InventoryController _inventoryController => InventoryController.Instance;

    public event Action OnUpgradesChange;

    [SerializeField] private List<Bonus> _upgradesChances;
    [SerializeField] private List<Bonus> _chanceIncreaseByLevels;
    [SerializeField] private GameObject _upgradesPickerPopUp;

    [SerializeField] private GameObject _upgradeCardPrefab;

    [SerializeField] private GameObject _upgradePrefab;
    [SerializeField] private Transform _upgradesTransform;

    [SerializeField] private List<Sprite> _cardsByRarity = new List<Sprite>();

    [SerializeField] public UpgradesData _upgradesData;
    [SerializeField] public Transform playerUpgradeCards;
    [SerializeField] public Transform upgradeCards;

    private Dictionary<int, UpgradeSlot> _playerUpgrades = new Dictionary<int, UpgradeSlot>();
    
    [SerializeField] public int maxUpgradesAmount = 3;
    [SerializeField] public int maxUpgradesToPickAmount = 3;
    
    [NonSerialized] public List<Upgrade> upgradesToPick = new List<Upgrade>();

    public bool isSelectingUpgradeCard = false;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    void Start()
    {
        _upgradesPickerPopUp.SetActive(false);
        Invoke(nameof(Debug), 1f);
    }

    // for debuging
    void Debug()
    {

        //AddUpgrade("additional_xp_2");

        // debug
        AddUpgrade(_upgradesData.upgradesData[0]);
        AddUpgrade(_upgradesData.upgradesData[1]);
        AddUpgrade(_upgradesData.upgradesData[2]);

        _inventoryController.AddItem("weapon_rifle", 1);
        _inventoryController.AddItem("weapon_shotgun", 1);
        _inventoryController.AddItem("ammo_rifle", 5);
        _inventoryController.AddItem("ammo_shotgun", 5);
        _inventoryController.AddItem("weapon_bat", 1);

        //DisplayUpgradesWindow(4);
    }

    private IEnumerator AfterCloseAnimation()
    {
        yield return new WaitForSeconds(0.3f);

        upgradesToPick.Clear();
        isSelectingUpgradeCard = false;

        _upgradesPickerPopUp.SetActive(false);
        _statesManager.SwitchState<PlayingState>();
    }

    public void DisplayUpgradesWindow(int level)
    {
        upgradesToPick.Clear();

        for (int i = upgradeCards.childCount - 1; i >= 0; i--)
        {
            Transform child = upgradeCards.GetChild(i);
            if (child.name == "Background") continue;

            Destroy(child.gameObject);
        }

        for (int i = 0; i < maxUpgradesToPickAmount; i++)
        {
            (bool finded, Upgrade upgrade) = RandomUpgrade(level, upgradesToPick);
            if(!finded) continue;

            upgradesToPick.Add(upgrade);

            GameObject upgradeCard = Instantiate(_upgradeCardPrefab, upgradeCards);
            upgradeCard.name = $"upgrade_{i}";

            GameObject cardButton = upgradeCard.transform.Find("CardButton").gameObject;
            cardButton.GetComponent<Image>().sprite = _cardsByRarity[GetTypeIndex(upgrade.type)];
            cardButton.AddComponent<UpgradeCardHandler>();

            UpgradeCard cardCtr = cardButton.GetComponent<UpgradeCard>();
            cardCtr.upgradeIndex = i;
            cardCtr.icon.sprite = upgrade.icon;
            cardCtr.title.text = upgrade.title;
            cardCtr.description.text = upgrade.description;
        }

        for (int i = playerUpgradeCards.childCount - 1; i >= 0; i--)
        {
            Transform child = playerUpgradeCards.GetChild(i);
            if (child.name == "Background") continue;

            Destroy(child.gameObject);
        }

        foreach((int slot, UpgradeSlot upgradeSlot) in _playerUpgrades)
        {
            Upgrade upgrade = upgradeSlot.upgrade;

            GameObject upgradeCard = Instantiate(_upgradeCardPrefab, playerUpgradeCards);
            upgradeCard.name = $"upgrade_{slot}";

            GameObject cardButton = upgradeCard.transform.Find("CardButton").gameObject;
            cardButton.GetComponent<Image>().sprite = _cardsByRarity[GetTypeIndex(upgrade.type)];

            UpgradeCard cardCtr = cardButton.GetComponent<UpgradeCard>();

            cardCtr.upgradeIndex = slot;

            cardCtr.icon.sprite = upgrade.icon;
            cardCtr.title.text = upgrade.title;
            cardCtr.description.text = upgrade.description;

            upgradeCard.transform.SetSiblingIndex(slot);
        }

        _upgradesPickerPopUp.SetActive(true);
        _statesManager.SwitchState<UpgradeState>();
        _upgradesPickerPopUp.transform.Find("Menu").GetComponent<UIEnterAnimations>().EnterAnimation();
    }

    public void CloseUpgradesPopUp()
    {
        _upgradesPickerPopUp.transform.Find("Menu").GetComponent<UIEnterAnimations>().ExitAnimation();
        StartCoroutine(AfterCloseAnimation());
    }


    public (bool finded, Upgrade upgrade) RandomUpgrade(int level, List<Upgrade> upgradesToExclude =  null, bool excludeOwned = false)
    {
        List<Upgrade> upgradeList = new List<Upgrade>();
        List<float> chances = new List<float>();

        float totalChance = 0f;

        foreach (Upgrade upgrade in _upgradesData.upgradesData)
        {
            if (upgradesToExclude != null && upgradesToExclude.Count > 0 && upgradesToExclude.Contains(upgrade)) continue;

            if (excludeOwned && _playerUpgrades.Count > 0)
            {
                bool alreadyHave = false;
                foreach (UpgradeSlot upgradeSlot in _playerUpgrades.Values)
                {
                    if (upgradeSlot.upgrade.name == upgrade.name)
                    {
                        alreadyHave = true;
                        break;
                    }
                }
                if (alreadyHave) continue;
            }

            float multiplier = 1f;

            foreach (Bonus chance in _chanceIncreaseByLevels)
            {
                if (chance.name == upgrade.type)
                {
                    multiplier *= chance.value * level;
                    break;
                }
            }

            foreach (Bonus chance in _upgradesChances)
            {
                if (chance.name == upgrade.type)
                {
                    upgradeList.Add(upgrade);

                    float chanceValue = chance.value * multiplier;
                    chances.Add(chanceValue);
                    totalChance += chanceValue;

                    break;
                }
            }
        }

        if (upgradeList.Count == 0)
        {
            return (false, new Upgrade());
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

        return (true, upgradeList[0]);
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
        if (slot < 0 || slot >= maxUpgradesAmount) return false;

        if (_playerUpgrades.TryGetValue(slot, out UpgradeSlot existing))
        {
            existing.upgrade = upgrade;
            existing.iconImage.sprite = upgrade.icon;
            existing.name.text = upgrade.title;
            existing.background.color = GetTypeColor(upgrade.type);
            OnUpgradesChange?.Invoke();
            return true;
        }

        return AddUpgrade(upgrade);
    }

    public bool AddUpgrade(string upgradeName)
    {
        foreach(Upgrade upgrade in _upgradesData.upgradesData)
        {
            if(upgrade.name == upgradeName)
            {
                return AddUpgrade(upgrade);
            }
        }

        return false;
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
        color.a = 0.6f;

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
        if (!_playerUpgrades.TryGetValue(slot, out UpgradeSlot upgradeSlot))
            return false;

        Destroy(upgradeSlot.upgradeSlot);

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
