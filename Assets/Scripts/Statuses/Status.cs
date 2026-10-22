using UnityEngine;
using UnityEngine.UI;

public class Status
{
    public StatusesController _statusesController { get; private set; }

    public int id;

    public GameObject gameObject { get; private set; }
    private Image progress;

    private float animationDuration = 0f;
    private float animationStartTime = 0f;

    public void Initialize(int id, StatusesController statusesController, GameObject gameObject, float duration)
    {
        this.id = id;
        this.gameObject = gameObject;
        _statusesController = statusesController;
        progress = gameObject.transform.Find("Progress").GetComponent<Image>();

        animationDuration = duration;
        animationStartTime = Time.time;

        if (duration == 0f)
        {
            progress.fillAmount = 1f;
            return;
        }

        progress.fillAmount = 0f;

        OnAdd();
    }

    public virtual void OnAdd()
    {

    }

    public virtual void OnRemove()
    {

    }

    public virtual void Tick()
    {

    }

    public virtual void FixedTick()
    {

    }

    public virtual void Tick1s()
    {

    }

    public virtual void Tick5s()
    {

    }

    public virtual void Tick10s()
    {

    }

    public virtual void Tick30s()
    {

    }

    public virtual void Tick1m()
    {

    }

    public bool UpdateAnimation()
    {
        if (animationDuration == 0f) return false;

        if (animationDuration + animationStartTime <= Time.time)
        {
            animationDuration = 0f;
            animationStartTime = 0f;

            return true;
        }

        float elapsed = Time.time - animationStartTime;
        progress.fillAmount = elapsed / animationDuration;
        return false;
    }

    public void Remove()
    {
        _statusesController.RemoveStatus(id);
    }
}