using TMPro;
using UnityEngine;

public class DamagePopupController : MonoBehaviour
{
    [SerializeField] private float floatSpeed = 1f;
    [SerializeField] private float lifeTime = 1f;

    private TextMeshPro text;
    private float timer;
    private Color startColor;

    void Awake()
    {
        text = GetComponent<TextMeshPro>();
        startColor = text.color;
    }

    public void Setup(int damage)
    {
        text.text = "-" + damage.ToString();
        timer = lifeTime;
    }

    void Update()
    {
        transform.position += Vector3.up * floatSpeed * Time.deltaTime;

        timer -= Time.deltaTime;
        float alpha = Mathf.Clamp01(timer / lifeTime);
        text.color = new Color(startColor.r, startColor.g, startColor.b, alpha);

        if (timer <= 0f)
            Destroy(gameObject);
    }
}