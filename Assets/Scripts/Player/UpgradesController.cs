using System;
using System.Collections.Generic;
using UnityEngine;
using ZombieRush.Framework;

public class UpgradesController : MonoBehaviour
{
    private UpgradesMenuController _upgradesMenuController => UpgradesMenuController.Instance;
    private InventoryController _inventoryController => InventoryController.Instance;
    private PlayerController _playerController;

    private Dictionary<string, Upgrade> _upgradesDataBase = new();
    public List<Upgrade> playerUpgrades = new();
    
    [SerializeField] public int maxUpgradesAmount = 3;
    [SerializeField] public int maxUpgradesToPickAmount = 3;
    
    [NonSerialized] public List<Upgrade> upgradesToPick = new List<Upgrade>();

    private void Awake()
    {
        _playerController = GetComponent<PlayerController>();
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

        Invoke(nameof(DebugFunc), 3f);
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

        //OpenUpgrades(1);
    }

    public void OpenUpgrades(int level)
    {
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
                foreach (Upgrade playerUpgrade in playerUpgrades)
                {
                    if (playerUpgrade.id == upgrade.id)
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
            Debug.Log("Upgrade: " + upgrade.GetName());
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
                Debug.Log("Upgrade: " + upgradeList[i].GetName());
                Debug.Log("Chance: " + chances[i]);
                Debug.Log("Rarity: " + upgradeList[i].rarity);
                return (true, upgradeList[i]);
            }
        }

        return (true, upgradeList[0]);
    }

    public bool AddUpgrade(string upgradeId)
    {
        if(_upgradesDataBase.ContainsKey(upgradeId))
        {
            return AddUpgrade(_upgradesDataBase[upgradeId]);
        }

        return false;
    }

    public bool AddUpgrade(Upgrade upgrade)
    {
        if (playerUpgrades.Count >= maxUpgradesAmount) return false;

        upgrade.status = new UpgradeStatus(upgrade.id, upgrade.modifiers);
        _playerController.statusesController.AddStatus(upgrade.status, 0f, upgrade.icon, RarityData.GetRarityColor(upgrade.rarity), StatusType.upgrade, true, 1000 - playerUpgrades.Count);

        playerUpgrades.Add(upgrade);

        return true;
    }

    public bool RemoveUpgrade(string upgradeId, bool removeStatus = true)
    {
        for (int i = 0; i < playerUpgrades.Count; i++)
        {
            Upgrade upgrade = playerUpgrades[i];
            if (upgradeId == upgrade.id)
            {
                if(removeStatus)
                {
                    upgrade.status.Remove();
                }

                playerUpgrades.RemoveAt(i);
                return true;
            }
        }

        return false;
    }
}
