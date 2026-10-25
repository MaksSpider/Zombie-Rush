using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class UpgradesMenuController : MonoBehaviour
{
    public static UpgradesMenuController Instance { get; private set; }

    private UpgradesController _upgradesController => UpgradesController.Instance;
    private StatesManager _statesManager => StatesManager.Instance;

    [SerializeField] private GameObject _upgradeCardPrefab;

    private Vector2 _deckOriginalPosition;

    public GameObject playerUpgrades;
    public GameObject upgradesPicking;

    public Transform _cardsToPickGrid;
    public Transform hoverLayer;


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

    public UpgradeCard AddPlayerUpgrade(Upgrade upgrade, int siblingIndex)
    {
        GameObject upgradeCard = Instantiate(_upgradeCardPrefab, playerUpgrades.transform);
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

        openedUpgradesMenu = true;
        upgradesPicking.SetActive(true);

        _deckOriginalPosition = playerUpgrades.GetComponent<RectTransform>().anchoredPosition;
        StartCoroutine(AnimatePlayerDeck(playerUpgrades.GetComponent<RectTransform>(), new Vector2(0, 450), 1f));

        upgradesPicking.GetComponent<UIEnterAnimations>().EnterAnimation();

        for (int i = 0; i < upgrades.Count; i++)
        {
            Upgrade upgrade = upgrades[i];

            GameObject upgradeCard = Instantiate(_upgradeCardPrefab, _cardsToPickGrid.transform);
            upgradeCard.name = $"upgrade_{upgradeCard.name}";

            upgradeCard.AddComponent<UpgradeCardHandlerToPick>();

            UpgradeCard cardCtr = upgradeCard.GetComponent<UpgradeCard>();
            cardCtr.UpdateUpgrade(upgrade);
        }
    }

    public void CloseUpgradesPicker()
    {
        StartCoroutine(AnimatePlayerDeck(playerUpgrades.GetComponent<RectTransform>(), _deckOriginalPosition, 1f));

        _statesManager.SwitchState<PlayingState>();

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

    public (bool insideDeck, int index) GetPosition(Vector2 position)
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
            return (true, closestIndex);
        }

        return (false, closestIndex);
    }

    public void SetActivePlayerCardsBackground(bool active)
    {
        playerUpgrades.transform.Find("Background").gameObject.SetActive(active);
    }

    private IEnumerator AnimatePlayerDeck(RectTransform element, Vector2 targetPos, float duration)
    {
        float elapsed = 0f;

        Vector2 startPos = element.anchoredPosition;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(elapsed / duration);

            element.anchoredPosition = Vector2.Lerp(startPos, targetPos, t);

            yield return null;
        }

        element.anchoredPosition = targetPos;
    }
}