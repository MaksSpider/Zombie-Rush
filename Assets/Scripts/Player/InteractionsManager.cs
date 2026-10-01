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

    private InputManager _inputManager;
    private PlayerController _playerController;

    private List<Interaction> _interactions = new List<Interaction>();

    [SerializeField] private GameObject _interactionPrefab;

    private GameObject _interactionPopUp;
    private RectTransform _interactionPopUpRect;
    private int _currentInteractionId = -1;
    private bool _interacting = false;
    private float _interactionProgress = 0f;
    private int _nextInteractionId = 0;
    private float _maxWidth = 1f;
    private RectTransform _fillMask;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        _inputManager = InputManager.Instance;
        _playerController = PlayerController.Instance;
    }

    void OnDestroy()
    {
        if (_inputManager != null)
        {
            _inputManager.InputActions.Player.Interact.performed -= ctx => TryToInteract();
        }
    }

    public void OnEnterState()
    {
        _inputManager.InputActions.Player.Interact.performed += ctx => TryToInteract();
    }

    public void OnExitState()
    {
        _inputManager.InputActions.Player.Interact.performed -= ctx => TryToInteract();
    }

    public void Tick()
    {
        HandleCurrentInteractionPostion();
        HandleInteracting();
    }

    public void FixedTick()
    {
        UpdateInteractions();
    }

    private void HandleInteracting()
    {
        if (!_interacting || _currentInteractionId < 0) return;

        if (!_inputManager.InputActions.Player.Interact.IsPressed())
        {
            _interacting = false;
            return;
        }

        _interactionProgress += Time.fixedDeltaTime;

        Interaction current = _interactions.FirstOrDefault(i => i.id == _currentInteractionId);
        if (current == null)
        {
            _interacting = false;
            _currentInteractionId = -1;
            return;
        }

        if (_interactionProgress >= current.duration)
        {
            current.onInteraction?.Invoke(current.id);

            if (current.removeOnUse)
            {
                RemoveInteraction(current.id);
            }

            _interacting = false;
        }
    }

    private void HandleCurrentInteractionPostion()
    {
        if (_interactionPopUp)
        {
            Interaction current = _interactions.FirstOrDefault(i => i.id == _currentInteractionId);

            if (current == null)
            {
                HideInteraction();
                return;
            }

            Vector3 worldPos = current.getPosition();
            Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);

            RectTransform overlayRect = _playerController.overlay.GetComponent<RectTransform>();
            Canvas canvas = overlayRect.GetComponentInParent<Canvas>();

            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                overlayRect,
                screenPos,
                canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera,
                out localPoint
            );

            _interactionPopUpRect.anchoredPosition = localPoint;

            float progress = current.duration > 0f ? _interactionProgress / current.duration : 1f;
            SetFill(progress);
        }
    }

    public void TryToInteract()
    {
        if (_currentInteractionId < 0) return;
        _interacting = true;
        _interactionProgress = 0f;
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
            _interacting = false;
            HideInteraction();
        }

        _interactions.Remove(interaction);

        return true;
    }

    public void UpdateInteractions()
    {
        if (_interactions == null || _interactions.Count == 0) return;
        if (_interacting) return;

        if (_playerController == null)
        {
            _playerController = PlayerController.Instance;
        }

        Interaction interaction = GetClosestInteraction(_playerController.transform.position);
        if (interaction == null)
        {
            _currentInteractionId = -1;
            HideInteraction();
            return;
        }

        if (interaction.id == _currentInteractionId) return;

        _currentInteractionId = interaction.id;
        _interactionProgress = 0f;
        DisplayInteraction(interaction.id);
    }

    public Interaction GetClosestInteraction(Vector2 position)
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

        return closestInteraction;
    }

    public void DisplayInteraction(int id)
    {
        if (id < 0 || _interactions.Count == 0) return;

        Interaction interaction = _interactions.FirstOrDefault(i => i.id == id);
        if (interaction == null) return;

        HideInteraction();

        _interactionPopUp = Instantiate(_interactionPrefab, _playerController.overlay);
        _interactionPopUpRect = _interactionPopUp.GetComponent<RectTransform>();
        _interactionPopUp.transform.Find("TitleText").GetComponent<TextMeshProUGUI>().text = interaction.title;
        _fillMask = _interactionPopUp.transform.Find("Progressbar").GetComponent<RectTransform>();

        _maxWidth = _fillMask.rect.width;
    }

    public void HideInteraction()
    {
        if (_interactionPopUp == null) return;

        Destroy(_interactionPopUp);
        _interactionPopUp = null;
        _interactionPopUpRect = null;
    }

    public void SetFill(float value)
    {
        if (!_interactionPopUp || _fillMask == null) return;

        float percentage = Mathf.Clamp01(value);

        Vector2 size = _fillMask.sizeDelta;
        size.x = _maxWidth * percentage;

        _fillMask.sizeDelta = size;
    }
}