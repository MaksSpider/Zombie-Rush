using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class UpgradesMenuController : MonoBehaviour
{
    public static UpgradesMenuController Instance { get; private set; }

    private StatesManager _statesManager => StatesManager.Instance;

    [SerializeField] private GameObject _upgradeCardPrefab;

    private GameObject _upgradesPicking;
    private Transform _cardsToPickGrid;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public UpgradeCard AddPlayerUpgrade(Upgrade upgrade, int siblingIndex)
    {
        GameObject upgradeCard = Instantiate(_upgradeCardPrefab);
        upgradeCard.name = $"upgrade_{upgradeCard.name}";

        upgradeCard.AddComponent<UpgradeCardHandler>();

        UpgradeCard cardCtr = upgradeCard.GetComponent<UpgradeCard>();

        cardCtr.UpdateUpgrade(upgrade);

        if (siblingIndex >= 0)
        {
            upgradeCard.transform.SetSiblingIndex(siblingIndex);
        }

        return cardCtr;
    }

    public void OpenUpgradesPicker(List<Upgrade> upgrades)
    {
        _statesManager.SwitchState<UpgradeState>();

        _upgradesPicking.SetActive(true);

        _upgradesPicking.GetComponent<UIEnterAnimations>().EnterAnimation();

        for (int i = 0; i < upgrades.Count; i++)
        {
            Upgrade upgrade = upgrades[i];

            GameObject upgradeCard = Instantiate(_upgradeCardPrefab, _cardsToPickGrid.transform);
            upgradeCard.name = $"upgrade_{upgradeCard.name}";

            UpgradeCard cardCtr = upgradeCard.GetComponent<UpgradeCard>();
            cardCtr.UpdateUpgrade(upgrade);
        }
    }

    public void CloseUpgradesPicker()
    {
        _statesManager.SwitchState<PlayingState>();

        for (int i = _cardsToPickGrid.childCount - 1; i >= 0; i--)
        {
            Transform child = _cardsToPickGrid.GetChild(i);
            Destroy(child.gameObject);
        }

        _upgradesPicking.SetActive(false);
    }
}