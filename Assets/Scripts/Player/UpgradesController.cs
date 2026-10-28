using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpgradesController : MonoBehaviour
{
    public static UpgradesController Instance;

    private UpgradesMenuController _upgradesMenuController => UpgradesMenuController.Instance;
    private InventoryController _inventoryController => InventoryController.Instance;
    private StatesManager _statesManager => StatesManager.Instance;

    public event Action OnUpgradesChange;

    private Dictionary<string, Upgrade> _upgradesDataBase = new();
    public List<UpgradeCard> playerUpgrades = new();
    
    [SerializeField] public int maxUpgradesAmount = 3;
    [SerializeField] public int maxUpgradesToPickAmount = 3;
    
    [NonSerialized] public List<Upgrade> upgradesToPick = new List<Upgrade>();

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
        ScriptableObject[] allUpgrades = Resources.LoadAll<ScriptableObject>("Upgrades");
        
        foreach (ScriptableObject asset in allUpgrades)
        {
            if(asset is UpgradeData upgradeData)
            {
                foreach (Upgrade upgrade in upgradeData.upgrade)
                {
                    _upgradesDataBase[upgrade.id] = upgrade;
                }
            }
        }

        Debug.Log($"Loaded {_upgradesDataBase.Count} upgrades.");

        Invoke(nameof(DebugFunc), 1f);
    }

    // for debuging
    void DebugFunc()
    {

        //AddUpgrade("additional_xp_2");

        // debug
        AddUpgrade("more_slots_0");
        AddUpgrade("more_slots_1");
        AddUpgrade("more_slots_2");

        _inventoryController.AddItem("weapon_rifle", 1);
        _inventoryController.AddItem("weapon_shotgun", 1);
        _inventoryController.AddItem("ammo_rifle", 5);
        _inventoryController.AddItem("ammo_shotgun", 5);
        _inventoryController.AddItem("weapon_bat", 1);
        _inventoryController.AddItem("medkit", 3);

        //DisplayUpgradesWindow(1);
    }

    public void DisplayUpgradesWindow(int level)
    {
        return;

        upgradesToPick.Clear();

        for (int i = 0; i < maxUpgradesToPickAmount; i++)
        {
            (bool finded, Upgrade upgrade) = RandomUpgrade(level, upgradesToPick);
            if (!finded) continue;

            upgradesToPick.Add(upgrade);
        }

        _upgradesMenuController.OpenUpgradesPicker(upgradesToPick);
    }


    public (bool finded, Upgrade upgrade) RandomUpgrade(int level, List<Upgrade> upgradesToExclude =  null, bool excludeOwned = false)
    {
        List<Upgrade> upgradeList = new List<Upgrade>();
        List<float> chances = new List<float>();

        float totalChance = 0f;

        foreach (Upgrade upgrade in _upgradesDataBase.Values)
        {
            if (upgradesToExclude != null && upgradesToExclude.Count > 0 && upgradesToExclude.Contains(upgrade)) continue;

            if (excludeOwned && playerUpgrades.Count > 0)
            {
                bool alreadyHave = false;
                foreach (UpgradeCard upgradeCard in playerUpgrades)
                {
                    if (upgradeCard.upgrade.name == upgrade.name)
                    {
                        alreadyHave = true;
                        break;
                    }
                }
                if (alreadyHave) continue;
            }

            (float chance, float chanceMultiplierPerLevel) = upgrade.GetUpgradeChance();
            float finalChance = chance * Mathf.Pow(chanceMultiplierPerLevel, level - 1);

            chances.Add(finalChance);
            upgradeList.Add(upgrade);

            Debug.Log("==============================");
            Debug.Log("Upgrade: " + upgrade.name);
            Debug.Log("Chance: " + finalChance);
            Debug.Log("Rarity: " + upgrade.rarity);

            totalChance += finalChance;
        }

        if (upgradeList.Count == 0)
        {
            return (false, new Upgrade());
        }

        float randomValue = UnityEngine.Random.Range(0f, totalChance);

        Debug.Log("==============================");
        Debug.Log("totalChance: " + totalChance);
        Debug.Log("randomValue: " + randomValue);

        for (int i = 0; i < upgradeList.Count; i++)
        {
            randomValue -= chances[i];

            if (randomValue < 0f)
            {
                Debug.Log("Upgrade: " + upgradeList[i].name);
                Debug.Log("Chance: " + chances[i]);
                Debug.Log("Rarity: " + upgradeList[i].rarity);
                return (true, upgradeList[i]);
            }
        }

        return (true, upgradeList[0]);
    }

    public float GetBonusValues(string bonusIndex)
    {
        float value = 0f;

        foreach(UpgradeCard upgradeCard in playerUpgrades)
        {
            foreach (ModifierProperty bonus in upgradeCard.upgrade.modifiers)
            {
                if (bonus.name == bonusIndex)
                {
                    value += bonus.value;
                }
            }
        }

        return value;
    }

    public bool AddUpgrade(string upgradeName, int siblingIndex = 0)
    {
        if(_upgradesDataBase.ContainsKey(upgradeName))
        {
            return AddUpgrade(_upgradesDataBase[upgradeName], siblingIndex);
        }

        return false;
    }

    public bool AddUpgrade(Upgrade upgrade, int siblingIndex = 0)
    {
        if (playerUpgrades.Count >= maxUpgradesAmount) return false;

        playerUpgrades.Add(_upgradesMenuController.AddPlayerUpgrade(upgrade, siblingIndex));
        OnUpgradesChange?.Invoke();

        return true;
    }

    public bool RemoveUpgrade(string upgradeName)
    {
        for(int i = 0; i < playerUpgrades.Count; i++)
        {
            UpgradeCard upgradeCard = playerUpgrades[i];
            if (upgradeName == upgradeCard.upgrade.name)
            {
                Destroy(upgradeCard.gameObject);
                playerUpgrades.RemoveAt(i);
                OnUpgradesChange?.Invoke();
                return true;
            }
        }

        return false;
    }
}
