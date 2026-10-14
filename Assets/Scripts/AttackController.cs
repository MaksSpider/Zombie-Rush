using System;
using UnityEngine;

public class AttackController : MonoBehaviour
{
    private float lifeTime;

    private Rigidbody2D _rb;

    [NonSerialized] public int damage;
    [NonSerialized] public bool multiTarget;
    [NonSerialized] public bool targetHited;
    [NonSerialized] public Vector2 direction;
    [NonSerialized] public float maxLifeTime;
    [NonSerialized] public float speed;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    }

    public void FixedTick(float deltaTime)
    {
        lifeTime += deltaTime;

        if (lifeTime >= maxLifeTime)
        {
            Destroy(gameObject);
            return;
        }

        Vector2 nextPosition = _rb.position + direction.normalized * speed * deltaTime;
        _rb.MovePosition(nextPosition);
    }
}