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

    [SerializeField] private GameObject _upgradesPicking;
    [SerializeField] private Transform _cardsToPickGrid;

    public bool addingAnimation = false;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        _upgradesPicking.SetActive(false);
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

        StartCoroutine(AddUpgradeCards(upgrades));
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


    private IEnumerator AddUpgradeCards(List<Upgrade> upgrades)
    {
        addingAnimation = true;

        HorizontalLayoutGroup layout = _cardsToPickGrid.GetComponent<HorizontalLayoutGroup>();

        RectTransform gridRect = _cardsToPickGrid.GetComponent<RectTransform>();

        for (int i = 0; i < upgrades.Count; i++)
        {
            List<RectTransform> existingCards = new();
            List<Vector2> oldPositions = new();

            foreach (Transform child in _cardsToPickGrid)
            {
                RectTransform rect = child.GetComponent<RectTransform>();

                existingCards.Add(rect);
                oldPositions.Add(rect.anchoredPosition);
            }

            GameObject upgradeCard = Instantiate(_upgradeCardPrefab, _cardsToPickGrid);

            upgradeCard.name = $"upgrade_{upgradeCard.name}";

            UpgradeCard cardCtr = upgradeCard.GetComponent<UpgradeCard>();
            cardCtr.UpdateUpgrade(upgrades[i]);

            RectTransform newCardRect = upgradeCard.GetComponent<RectTransform>();

            LayoutRebuilder.ForceRebuildLayoutImmediate(gridRect);

            List<Vector2> targetPositions = new();

            foreach (Transform child in _cardsToPickGrid)
            {
                targetPositions.Add(
                    child.GetComponent<RectTransform>().anchoredPosition
                );
            }

            layout.enabled = false;

            for (int j = 0; j < existingCards.Count; j++)
            {
                existingCards[j].anchoredPosition = oldPositions[j];
            }

            newCardRect.anchoredPosition = Vector2.zero;

            List<RectTransform> allCards = new();

            foreach (Transform child in _cardsToPickGrid)
            {
                allCards.Add(child.GetComponent<RectTransform>());
            }

            yield return StartCoroutine(
                AnimateCards(allCards, targetPositions, 0.15f)
            );

            layout.enabled = true;

            yield return new WaitForSeconds(0.15f);
        }

        layout.enabled = true;

        addingAnimation = false;
    }

    private IEnumerator AnimateCards(List<RectTransform> cards, List<Vector2> targetPositions, float duration)
    {
        List<Vector2> startPositions = new();

        foreach (RectTransform card in cards)
        {
            startPositions.Add(card.anchoredPosition);
        }

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(elapsed / duration);

            t = 1f - Mathf.Pow(1f - t, 3f);

            for (int i = 0; i < cards.Count; i++)
            {
                cards[i].anchoredPosition = Vector2.Lerp(
                    startPositions[i],
                    targetPositions[i],
                    t
                );
            }

            yield return null;
        }

        for (int i = 0; i < cards.Count; i++)
        {
            cards[i].anchoredPosition = targetPositions[i];
        }
    }
}