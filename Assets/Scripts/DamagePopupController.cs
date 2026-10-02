using TMPro;
using UnityEngine;

public class DamagePopupController : MonoBehaviour
{
    [SerializeField] private float _floatSpeed = 1f;
    [SerializeField] private float _lifeTime = 1f;

    private TextMeshPro _text;
    private float _timer;
    private Color _startColor;

    void Awake()
    {
        _text = GetComponent<TextMeshPro>();
        _startColor = _text.color;
    }

    public void Setup(int damage)
    {
        _text.text = "-" + damage.ToString();
        _timer = _lifeTime;
    }

    void Update()
    {
        transform.position += Vector3.up * _floatSpeed * Time.deltaTime;

        _timer -= Time.deltaTime;
        float alpha = Mathf.Clamp01(_timer / _lifeTime);
        _text.color = new Color(_startColor.r, _startColor.g, _startColor.b, alpha);

        if (_timer <= 0f) Destroy(gameObject);
    }
}