using System;
using UnityEngine;
using UnityEngine.UI;

public class ProgressBar : MonoBehaviour
{
    [SerializeField, Range(0f, 1f)]
    private float _value = 1f;
    private float _lastWidth = -1f;

    public float value
    {
        get => _value;
        set
        {
            float newValue = Mathf.Clamp01(value);

            if (Mathf.Approximately(_value, newValue)) return;

            _value = newValue;
            UpdateFill();
        }
    }

    public Color backgroundColor = Color.white;
    public Color fillColor = Color.yellow;

    private void OnValidate()
    {
        UpdateFill();
    }

    private void LateUpdate()
    {
        RectTransform rectTransform = transform.Find("FillBar").GetComponent<RectTransform>();

        float currentWidth = rectTransform.rect.width;

        if (!Mathf.Approximately(_lastWidth, currentWidth))
        {
            _lastWidth = currentWidth;
            UpdateFill();
        }
    }

    public void UpdateFill()
    {
        Transform fillBar = transform.Find("FillBar");
        if (fillBar == null) return;

        RectTransform rectTransform = fillBar.GetComponent<RectTransform>();
        RectMask2D rectMask = fillBar.GetComponent<RectMask2D>();

        Transform background = transform.Find("ProgressbarBackground");
        Transform bar = fillBar.Find("bar");

        if (rectTransform == null || rectMask == null || background == null || bar == null) return;

        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);

        background.GetComponent<Image>().color = backgroundColor;
        bar.GetComponent<Image>().color = fillColor;

        float width = rectTransform.rect.width * rectTransform.lossyScale.x;
        float progress = width * (1f - _value);

        rectMask.padding = new Vector4(0, 0, progress, 0);
    }

}
