using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UpgradesMenuController : MonoBehaviour
{
    public static UpgradesMenuController Instance { get; private set; }

    private StatesManager _statesManager => StatesManager.Instance;
    private UpgradesController _upgradesController => UpgradesController.Instance;

    [SerializeField] private GameObject _upgradeCardPrefab;
    [SerializeField] private Button _finishButton;

    public GameObject playerUpgrades;
    public GameObject upgradesPicking;
    public GameObject cardsTrash;

    public Transform _cardsToPickGrid;


    [NonSerialized] public bool openedUpgradesMenu = false;
    [NonSerialized] public bool holdingCard = false;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        _finishButton.onClick.AddListener(() =>
        {
            if(_upgradesController.maxUpgradesAmount < _upgradesController.playerUpgrades.Count) return;

            _statesManager.SwitchState<PlayingState>();
            CloseUpgradesPicker();
        });
    }

    public UpgradeCard AddPlayerUpgrade(Upgrade upgrade, int siblingIndex)
    {
        GameObject upgradeCard = Instantiate(_upgradeCardPrefab, playerUpgrades.transform);
        upgradeCard.name = $"upgrade_{upgradeCard.name}";

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
        openedUpgradesMenu = true;
        upgradesPicking.SetActive(true);

        playerUpgrades.GetComponent<Animator>().SetBool("Picking", true);

        for (int i = 0; i < upgrades.Count; i++)
        {
            Upgrade upgrade = upgrades[i];

            GameObject upgradeCard = Instantiate(_upgradeCardPrefab, _cardsToPickGrid.transform);
            upgradeCard.name = $"upgrade_{upgradeCard.name}";

            UpgradeCard cardCtr = upgradeCard.GetComponent<UpgradeCard>();
            cardCtr.UpdateUpgrade(upgrade);
        }

        _statesManager.SwitchState<UpgradeState>();
    }

    public void CloseUpgradesPicker()
    {
        playerUpgrades.GetComponent<Animator>().SetBool("Picking", false);

        for (int i = _cardsToPickGrid.childCount - 1; i >= 0; i--)
        {
            Transform child = _cardsToPickGrid.GetChild(i);
            if (child.name == "Background") continue;

            Destroy(child.gameObject);
        }

        holdingCard = false;
        openedUpgradesMenu = false;

        upgradesPicking.SetActive(false);
    }

    public (string where, int index) GetPosition(Vector2 position)
    {
        RectTransform deck = playerUpgrades.GetComponent<RectTransform>();

        int closestIndex = 0;
        float closestDistance = float.MaxValue;

        for (int i = 0; i < deck.childCount; i++)
        {
            RectTransform slot = deck.GetChild(i).GetComponent<RectTransform>();

            Vector2 slotScreenPosition = RectTransformUtility.WorldToScreenPoint(null, slot.position);

            float distance = Vector2.Distance(position, slotScreenPosition);

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestIndex = i;
            }
        }

        if(RectTransformUtility.RectangleContainsScreenPoint(playerUpgrades.GetComponent<RectTransform>(), position, null))
        {
            return ("playerDeck", closestIndex);
        } else if(RectTransformUtility.RectangleContainsScreenPoint(cardsTrash.GetComponent<RectTransform>(), position, null))
        {
            return ("trash", closestIndex);
        }

        return ("wrong", closestIndex);
    }

    public void SetActivePlayerCardsBackground(bool active)
    {
        playerUpgrades.transform.Find("Background").gameObject.SetActive(active);
    }
}
