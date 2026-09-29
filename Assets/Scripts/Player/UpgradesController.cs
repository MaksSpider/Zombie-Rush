using System;
using System.Collections.Generic;
using UnityEngine;

public class UpgradesController : MonoBehaviour
{
    public static UpgradesController Instance;

    [SerializeField] private Sprite noUpgradeIcon;

    public event System.Action OnUpgradesChange;

    [SerializeField] private GameObject UpgradesPickerPopUp;
    [SerializeField] private UpgradesData upgradesData;

    private Dictionary<int, Upgrade> playerUpgrades = new Dictionary<int, Upgrade>();
    
    [SerializeField] public int maxUpgradesAmount = 3;

    void Start()
    {
        Invoke(nameof(TestUpgrade), 1f);
    }

    void TestUpgrade()
    {
        AddUpgrade(upgradesData.upgradesData[0]);
        AddUpgrade(upgradesData.upgradesData[1]);
        AddUpgrade(upgradesData.upgradesData[2]);

        InventoryController inventoryController = GetComponent<InventoryController>();
        inventoryController.AddItemById(3, 5);
        inventoryController.AddItemById(4, 5);
        inventoryController.AddItemById(5, 5);
        inventoryController.AddItemById(6, 5);
        inventoryController.AddItemById(7, 5);
        inventoryController.AddItemById(8, 1);
    }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public (bool exist, Upgrade upgrade) GetUpgrade(int slot)
    {
        if (!playerUpgrades.ContainsKey(slot)) return (false, new Upgrade());

        return (true, playerUpgrades[slot]);
    }

    public (bool exist, float value) GetUpgradeBonus(int slot, string bonusIndex)
    {
        if (!playerUpgrades.ContainsKey(slot)) return (false, 0f);

        foreach(Bonus bonus in playerUpgrades[slot].bonuses)
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

        foreach(Upgrade upgrade in playerUpgrades.Values)
        {
            foreach (Bonus bonus in upgrade.bonuses)
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

        playerUpgrades[slot] = upgrade;
        OnUpgradesChange?.Invoke();
        return true;
    }

    public bool AddUpgrade(Upgrade upgrade)
    {
        var result = GetFirstFreeSlot();
        if (!result.exist) return false;

        playerUpgrades.Add(result.slot, upgrade);
        OnUpgradesChange?.Invoke();
        return true;
    }

    public bool RemoveUpgrade(String upgradeName)
    {
        foreach((int slot, Upgrade upgrade) in playerUpgrades)
        {
            if(upgradeName == upgrade.name)
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
        if (!playerUpgrades.ContainsKey(slot)) return false;

        playerUpgrades.Remove(slot);
        OnUpgradesChange?.Invoke();
        return true;
    }

    public (bool exist, int slot) GetFirstFreeSlot()
    {
        for (int i = 0; i < maxUpgradesAmount; i++)
        {
            if(!playerUpgrades.ContainsKey(i))
            {
                return (true, i);
            }
        }

        return (false, 0);
    }
}
