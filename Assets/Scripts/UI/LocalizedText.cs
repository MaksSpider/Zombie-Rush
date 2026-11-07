
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_Text))]
public class LocalizedText : MonoBehaviour
{
    [SerializeField] private string key;

    private TMP_Text textComponent;

    private void Awake()
    {
        textComponent = GetComponent<TMP_Text>();
    }

    private void OnEnable()
    {
        if (TranslationManager.Instance != null)
        {
            TranslationManager.Instance.OnLanguageChanged += Refresh;
        }

        Refresh();
    }

    private void OnDisable()
    {
        if (TranslationManager.Instance != null)
        {
            TranslationManager.Instance.OnLanguageChanged -= Refresh;
        }
    }

    public void Refresh()
    {
        if (textComponent == null || TranslationManager.Instance == null) return;

        textComponent.text = TranslationManager.Instance.GetText(key);
    }

    public void SetKey(string key)
    {
        this.key = key;
        Refresh();
    }
}
