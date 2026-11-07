
using UnityEngine;

public class TileController : MonoBehaviour
{
    private SpriteRenderer _spriteRenderer;

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void Setup(Vector2 position, Sprite sprite, Color color = default)
    {
        transform.position = position;
        _spriteRenderer.sprite = sprite;
        gameObject.SetActive(true);
    }
}
