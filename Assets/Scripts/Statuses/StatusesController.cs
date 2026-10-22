using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StatusesController : MonoBehaviour
{
    [SerializeField] private GameObject StatusPrefab;
    [SerializeField] private Transform StatusesParent;

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
    public int AddStatus(Status status, float duration, Sprite icon, Color color, int priority = 0)
    {
        int currentId = nextId;
        nextId++;

        GameObject statusObject = Instantiate(StatusPrefab, StatusesParent);
        statusObject.name = $"status_{currentId}";

        Image statusIcon = statusObject.transform.Find("Icon").GetComponent<Image>();
        Image statusProgress = statusObject.transform.Find("Progress").GetComponent<Image>();

        statusIcon.sprite = icon;
        statusProgress.color = color;

        status.Initialize(currentId, this, statusObject, duration);

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
