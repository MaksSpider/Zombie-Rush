using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GuideKeyController : MonoBehaviour
{
    private UIEnterAnimations _enterAnimations;

    [SerializeField] private Image _keyImage;
    [SerializeField] private TextMeshProUGUI _text;

    public int priority = 0;

    private void Awake()
    {
        _enterAnimations = transform.GetComponentInChildren<UIEnterAnimations>();
        gameObject.SetActive(false);
    }

    public void Init(Sprite keySprite, string text, int priority)
    {
        _keyImage.sprite = keySprite;
        _text.text = text;
        this.priority = priority;
    }

    public void Show()
    {
        gameObject.SetActive(true);
        _enterAnimations.EnterAnimation();
    }

    public void Hide()
    {
        _enterAnimations.ExitAnimation(() =>
        {
            gameObject.SetActive(false);
        });
    }

    public void Remove()
    {
        _enterAnimations.ExitAnimation(() =>
        {
            Destroy(gameObject);
        });
    }
}