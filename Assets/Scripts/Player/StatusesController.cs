using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Status
{
    public int id;

    public GameObject status;

    private Image icon;
    private Image progress;

    private float animationDuration = 0f;
    private float animationStartTime = 0f;
    private Action onFinish;

    public Status(int id, GameObject status, Image icon, Image progress, Color color, float duration, Action onFinish)
    {
        this.id = id;
        this.status = status;
        this.icon = icon;
        this.progress = progress;
        this.animationDuration = duration;
        this.animationStartTime = Time.time;
        this.onFinish = onFinish;

        if(duration == 0f)
        {
            progress.fillAmount = 1f;
            return;
        }

        progress.fillAmount = 0f;
    }

    public bool UpdateAnimation()
    {
        if (animationDuration == 0f) return false;

        if (animationDuration + animationStartTime <= Time.time)
        {
            animationDuration = 0f;
            animationStartTime = 0f;

            if (onFinish == null) return true;

            onFinish?.Invoke();
            onFinish = null;

            return true;
        }

        float elapsed = Time.time - animationStartTime;
        progress.fillAmount = elapsed / animationDuration;
        return false;
    }
}

public class StatusesController : MonoBehaviour
{
    public static StatusesController Instance { get; private set; }

    [SerializeField] private GameObject StatusPrefab;
    [SerializeField] private Transform StatusesParent;

    private List<Status> statuses = new List<Status>();

    private int nextId = 0;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void Tick()
    {
        UpdateAniamtions();
    }

    private void UpdateAniamtions()
    {
        for (int i = statuses.Count - 1; i >= 0; i--)
        {
            Status status = statuses[i];

            if (status.UpdateAnimation())
            {
                Destroy(status.status);
                statuses.RemoveAt(i);
            }
        }
    }

    public int RegisterStatus(Sprite icon, float duration, Color color, Action onFinish = null)
    {
        int currentId = nextId;
        nextId++;

        GameObject statusObject = Instantiate(StatusPrefab, StatusesParent);
        statusObject.name = $"status_{currentId}";

        Image statusIcon = statusObject.transform.Find("Icon").GetComponent<Image>();
        Image statusProgress = statusObject.transform.Find("Progress").GetComponent<Image>();

        statusIcon.sprite = icon;
        statusProgress.color = color;

        statuses.Add(new Status(currentId, statusObject, statusIcon, statusProgress, color, duration, onFinish));

        return currentId;
    }
}
