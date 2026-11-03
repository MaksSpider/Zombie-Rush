using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum StatusType
{
    normal,
    upgrade
}

public class StatusesController : MonoBehaviour
{
    [SerializeField] private GameObject _statusPrefab;
    [SerializeField] private Transform _statusesParent;
    [SerializeField] private Transform _upgradeStatusesParent;

    private List<Status> statuses = new();

    private int nextId = 0;

    public void Tick()
    {
        for (int i = statuses.Count - 1; i >= 0; i--)
        {
            Status status = statuses[i];

            status.Tick();

            if (status.UpdateAnimation())
            {
                status.OnRemove();
                Destroy(status.gameObject);
                statuses.RemoveAt(i);
            }
        }
    }

    public void FixedTick()
    {
        for (int i = statuses.Count - 1; i >= 0; i--)
        {
            Status status = statuses[i];

            if(status == null)
            {
                statuses.RemoveAt(i);
            }

            status.FixedTick();
        }
    }

    public void Tick1s()
    {
        for (int i = statuses.Count - 1; i >= 0; i--)
        {
            Status status = statuses[i];

            if (status == null)
            {
                statuses.RemoveAt(i);
            }

            status.Tick1s();
        }
    }

    public void Tick5s()
    {
        for (int i = statuses.Count - 1; i >= 0; i--)
        {
            Status status = statuses[i];

            if (status == null)
            {
                statuses.RemoveAt(i);
            }

            status.Tick5s();
        }
    }

    public void Tick10s()
    {
        for (int i = statuses.Count - 1; i >= 0; i--)
        {
            Status status = statuses[i];

            if (status == null)
            {
                statuses.RemoveAt(i);
            }

            status.Tick10s();
        }
    }

    public void Tick30s()
    {
        for (int i = statuses.Count - 1; i >= 0; i--)
        {
            Status status = statuses[i];

            if (status == null)
            {
                statuses.RemoveAt(i);
            }

            status.Tick30s();
        }
    }

    public void Tick1m()
    {
        for (int i = statuses.Count - 1; i >= 0; i--)
        {
            Status status = statuses[i];

            if (status == null)
            {
                statuses.RemoveAt(i);
            }

            status.Tick1m();
        }
    }

    // TODO: add priority
    public int AddStatus(Status status, float duration, Sprite icon, Color color, StatusType statusType = StatusType.normal, bool removeable = false, int priority = 0)
    {
        int currentId = nextId;
        nextId++;

        Transform parent = null;

        switch (statusType)
        {
            case StatusType.normal:
                parent = _statusesParent;
                break;
            case StatusType.upgrade:
                parent = _upgradeStatusesParent;
                break;
            default:
                parent = _statusesParent;
                break;
        }

        GameObject statusObject = Instantiate(_statusPrefab, parent);
        statusObject.name = $"status_{currentId}";

        int slotIndex = statuses.Count;

        foreach (Status statusToCheck in statuses)
        {
            if (statusToCheck.priority < priority)
            {
                slotIndex = statusToCheck.gameObject.GetComponent<RectTransform>().GetSiblingIndex();
            }
        }

        statusObject.GetComponent<RectTransform>().SetSiblingIndex(slotIndex);

        Image statusIcon = statusObject.transform.Find("Icon").GetComponent<Image>();
        Image statusProgress = statusObject.transform.Find("Progress").GetComponent<Image>();
        GameObject removeButton = statusObject.transform.Find("RemoveButton").gameObject;

        if (removeable) 
        {
            removeButton.GetComponent<Button>().onClick.AddListener(() =>
            {
                status.Remove();
            });
        }
        else
        {
            removeButton.SetActive(false);
        }

        statusIcon.sprite = icon;
        statusProgress.color = color;

        status.Initialize(currentId, this, statusObject, duration, priority);

        statuses.Add(status);

        return currentId;
    }

    public bool RemoveStatus(int id)
    {
        for (int i = statuses.Count - 1; i >= 0; i--)
        {
            Status status = statuses[i];

            if (status.id == id)
            {
                status.OnRemove();
                Destroy(status.gameObject);
                statuses.RemoveAt(i);
                return true;
            }
        }

        return false;
    }
}
