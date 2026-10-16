using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Interaction
{
    public int id;
    public string title;
    public Func<Vector2> getPosition;
    public float range;
    public float duration;

    public bool removeOnUse;

    public Action<int> onInteraction;

    public Interaction(int id, string title, float duration, Func<Vector2> getPosition, float range, bool removeOnUse, Action<int> onInteraction)
    {
        this.id = id;
        this.title = title;
        this.duration = duration;
        this.getPosition = getPosition;
        this.range = range;
        this.removeOnUse = removeOnUse;
        this.onInteraction = onInteraction;
    }
}

public class InteractionsManager : MonoBehaviour
{
    public static InteractionsManager Instance { get; private set; }

    private InputManager _inputManager => InputManager.Instance;

    private List<Interaction> _interactions = new List<Interaction>();

    [SerializeField] private GameObject _interactionPrefab;

    private GameObject _interactionPopUp;
    private ProgressBar _progressBar;

    private int _currentInteractionId = -1;
    private float _interactionProgress = 0f;

    private int _nextInteractionId = 0;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void OnEnterState()
    {
        if (_currentInteractionId < 0 || _interactionPopUp == null) return;

        _interactionPopUp.SetActive(true);
    }

    public void OnExitState()
    {
        if (_currentInteractionId < 0 || _interactionPopUp == null) return;

        _interactionPopUp.SetActive(false);
    }

    public void Tick()
    {
        HandleInteractionPostion();
        HandleInteracting();
    }

    public void FixedTick()
    {
        UpdateInteractions();
    }

    private void HandleInteracting()
    {
        if (_currentInteractionId < 0) return;

        if (_inputManager.InputActions.Player.Interact.IsPressed())
        {
            _interactionProgress += Time.deltaTime;

            (bool result, Interaction current) = GetInteraction(_currentInteractionId);
            if (!result)
            {
                _currentInteractionId = -1;
                return;
            }

            if (_interactionProgress >= current.duration)
            {
                current.onInteraction?.Invoke(current.id);
                _interactionProgress = 0f;

                if (current.removeOnUse)
                {
                    RemoveInteraction(current.id);
                }
            }

            return;
        }

        if(_interactionProgress > 0f)
        {
            _interactionProgress -= Time.deltaTime;
        }
    }

    private void HandleInteractionPostion()
    {
        if (_interactionPopUp)
        {
            (bool result, Interaction interaction) = GetInteraction(_currentInteractionId);

            if (!result)
            {
                HideInteraction();
                return;
            }

            Vector3 worldPos = interaction.getPosition();
            Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);

            RectTransform overlayRect = PlayerController.Instance.overlay.GetComponent<RectTransform>();
            Canvas canvas = overlayRect.GetComponentInParent<Canvas>();

            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                overlayRect,
                screenPos,
                canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera,
                out localPoint
            );

            _interactionPopUp.GetComponent<RectTransform>().anchoredPosition = localPoint;

            _progressBar.value = Mathf.Clamp01(_interactionProgress / interaction.duration);
        }
    }

    private void GeneratePopUp()
    {
        _interactionPopUp = Instantiate(_interactionPrefab, PlayerController.Instance.overlay);
        _progressBar = _interactionPopUp.transform.Find("ProgressBar").GetComponent<ProgressBar>();
    }

    public int RegisterInteraction(string title, float duration, Vector2 position, float range, bool removeOnUse, Action<int> onInteraction)
    {
        int id = _nextInteractionId++;
        _interactions.Add(new Interaction(id, title, duration, () => {
            return position;
        }, range, removeOnUse, onInteraction));

        return id;
    }

    public int RegisterInteractionOnObject(string title, float duration, GameObject gameObject, Vector2 offset, float range, bool removeOnUse, Action<int> onInteraction)
    {
        int id = _nextInteractionId++;
        _interactions.Add(new Interaction(id, title, duration, () => {
            return (Vector2)gameObject.transform.position + offset;
        }, range, removeOnUse, onInteraction));

        return id;
    }

    public bool RemoveInteraction(int id)
    {
        Interaction interaction = _interactions.FirstOrDefault(i => i.id == id);
        if (interaction == null) return false;

        if (interaction.id == _currentInteractionId)
        {
            _currentInteractionId = -1;
            HideInteraction();
        }

        _interactions.Remove(interaction);

        return true;
    }

    public void UpdateInteractions()
    {
        if (_interactions == null || _interactions.Count == 0) return;

        (bool result, Interaction interaction) = GetClosestInteraction(PlayerController.Instance.transform.position);
        if (!result)
        {
            _currentInteractionId = -1;
            HideInteraction();
            return;
        }

        if (interaction.id == _currentInteractionId) return;

        HideInteraction();

        _currentInteractionId = interaction.id;

        DisplayInteraction(interaction.id);
    }

    public (bool result, Interaction interaction) GetClosestInteraction(Vector2 position)
    {
        Interaction closestInteraction = null;
        float closestDistance = float.MaxValue;

        foreach (Interaction interaction in _interactions)
        {
            Vector2 interactionPosition = interaction.getPosition();
            float distance = Vector2.Distance(position, interactionPosition);

            if (distance < interaction.range && distance < closestDistance)
            {
                closestInteraction = interaction;
                closestDistance = distance;
            }
        }

        return (closestInteraction == null ? false : true, closestInteraction);
    }

    public (bool result, Interaction interaction) GetInteraction(int id)
    {
        foreach (Interaction interaction in _interactions)
        {
            if(interaction.id == id) return (true, interaction);
        }

        return (false, null);
    }

    private void DisplayInteraction(int id)
    {
        if (id < 0 || _interactions.Count == 0) return;

        (bool result, Interaction interaction) = GetInteraction(id);
        if (!result) return;

        if (_interactionPopUp == null) GeneratePopUp();

        _interactionPopUp.SetActive(true);

        _interactionPopUp.transform.Find("Title").GetComponent<TextMeshProUGUI>().text = interaction.title;
        _progressBar.value = 0f;
        _interactionProgress = 0f;

        LayoutRebuilder.ForceRebuildLayoutImmediate(_interactionPopUp.GetComponent<RectTransform>());

    }

    private void HideInteraction()
    {
        if (_interactionPopUp == null) return;

        _progressBar.value = 0f;
        _interactionProgress = 0f;

        _interactionPopUp.SetActive(false);
    }
}