using System;
using UnityEngine;

public class BulletController : MonoBehaviour
{
    private float lifeTime = 0f;

    [NonSerialized] public int damage;
    [NonSerialized] public bool targetHited = false;
    [NonSerialized] public Vector2 direction;
    [NonSerialized] public float maxLifeTime = 0f;
    [NonSerialized] public float speed;

    public void FixedTick(float deltaTime)
    {
        lifeTime += deltaTime;

        if(lifeTime > maxLifeTime) Destroy(gameObject);

        transform.position += (Vector3)(direction * speed * deltaTime);
    }
}