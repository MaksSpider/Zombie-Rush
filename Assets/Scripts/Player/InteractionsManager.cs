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

    private InputManager inputManager;
    private PlayerController playerController;
    private UIController uIController;

    private List<Interaction> interactions = new List<Interaction>();

    [SerializeField] private GameObject interactionPrefab;

    private GameObject interactionPopUp;
    private RectTransform interactionPopUpRect;
    private int currentInteractionId = -1;
    private bool interacting = false;
    private float interactionProgress = 0f;
    private int nextInteractionId = 0;
    private float maxWidth = 1f;
    private RectTransform fillMask;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        inputManager = InputManager.Instance;
        playerController = PlayerController.Instance;
        uIController = UIController.Instance;

        inputManager.InputActions.Player.Interact.performed += OnInteractPerformed;
    }

    void OnDestroy()
    {
        if (inputManager != null)
        {
            inputManager.InputActions.Player.Interact.performed -= OnInteractPerformed;
        }
    }

    private void OnInteractPerformed(UnityEngine.InputSystem.InputAction.CallbackContext ctx)
    {
        TryToInteract();
    }

    void Start()
    {
        InvokeRepeating(nameof(UpdateInteractions), 0f, 0.1f);
    }

    void Update()
    {
        if (interactionPopUp)
        {
            Interaction current = interactions.FirstOrDefault(i => i.id == currentInteractionId);

            if (current == null)
            {
                HideInteraction();
                return;
            }

            Vector3 worldPos = current.getPosition();
            Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);

            RectTransform overlayRect = uIController.overlay.GetComponent<RectTransform>();
            Canvas canvas = overlayRect.GetComponentInParent<Canvas>();

            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                overlayRect,
                screenPos,
                canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera,
                out localPoint
            );

            interactionPopUpRect.anchoredPosition = localPoint;

            float progress = current.duration > 0f ? interactionProgress / current.duration : 1f;
            SetFill(progress);
        }
    }

    void FixedUpdate()
    {
        if (!interacting || currentInteractionId < 0) return;

        if (!inputManager.InputActions.Player.Interact.IsPressed())
        {
            interacting = false;
            return;
        }

        interactionProgress += Time.fixedDeltaTime;

        Interaction current = interactions.FirstOrDefault(i => i.id == currentInteractionId);
        if (current == null)
        {
            interacting = false;
            currentInteractionId = -1;
            return;
        }

        if (interactionProgress >= current.duration)
        {
            current.onInteraction?.Invoke(current.id);

            if (current.removeOnUse)
            {
                RemoveInteraction(current.id);
            }

            interacting = false;
        }
    }

    public void TryToInteract()
    {
        if (currentInteractionId < 0) return;
        interacting = true;
        interactionProgress = 0f;
    }

    public int RegisterInteraction(string title, float duration, Vector2 position, float range, bool removeOnUse, Action<int> onInteraction)
    {
        int id = nextInteractionId++;
        interactions.Add(new Interaction(id, title, duration, () => {
            return position;
        }, range, removeOnUse, onInteraction));

        return id;
    }

    public int RegisterInteractionOnObject(string title, float duration, GameObject gameObject, Vector2 offset, float range, bool removeOnUse, Action<int> onInteraction)
    {
        int id = nextInteractionId++;
        interactions.Add(new Interaction(id, title, duration, () => {
            return (Vector2)gameObject.transform.position + offset;
        }, range, removeOnUse, onInteraction));

        return id;
    }

    public bool RemoveInteraction(int id)
    {
        Interaction interaction = interactions.FirstOrDefault(i => i.id == id);
        if (interaction == null) return false;

        if (interaction.id == currentInteractionId)
        {
            currentInteractionId = -1;
            interacting = false;
            HideInteraction();
        }

        interactions.Remove(interaction);

        return true;
    }

    public void UpdateInteractions()
    {
        if (interactions == null || interactions.Count == 0) return;
        if (interacting) return;

        if (playerController == null)
        {
            playerController = PlayerController.Instance;
        }

        Interaction interaction = GetClosestInteraction(playerController.transform.position);
        if (interaction == null)
        {
            currentInteractionId = -1;
            HideInteraction();
            return;
        }

        if (interaction.id == currentInteractionId) return;

        currentInteractionId = interaction.id;
        interactionProgress = 0f;
        DisplayInteraction(interaction.id);
    }

    public Interaction GetClosestInteraction(Vector2 position)
    {
        Interaction closestInteraction = null;
        float closestDistance = float.MaxValue;

        foreach (Interaction interaction in interactions)
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
        if (id < 0 || interactions.Count == 0) return;

        Interaction interaction = interactions.FirstOrDefault(i => i.id == id);
        if (interaction == null) return;

        HideInteraction();

        if (uIController == null)
        {
            uIController = UIController.Instance;
        }

        interactionPopUp = Instantiate(interactionPrefab, uIController.overlay);
        interactionPopUpRect = interactionPopUp.GetComponent<RectTransform>();
        interactionPopUp.transform.Find("TitleText").GetComponent<TextMeshProUGUI>().text = interaction.title;
        fillMask = interactionPopUp.transform.Find("Progressbar").GetComponent<RectTransform>();

        maxWidth = fillMask.rect.width;
    }

    public void HideInteraction()
    {
        if (interactionPopUp == null) return;

        Destroy(interactionPopUp);
        interactionPopUp = null;
        interactionPopUpRect = null;
    }

    public void SetFill(float value)
    {
        if (!interactionPopUp || fillMask == null) return;

        float percentage = Mathf.Clamp01(value);

        Vector2 size = fillMask.sizeDelta;
        size.x = maxWidth * percentage;

        fillMask.sizeDelta = size;
    }
}