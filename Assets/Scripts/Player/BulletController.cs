using UnityEngine;

public class BulletController : MonoBehaviour
{
    public int damage;
    private float lifeTime = 10f;

    private void Start()
    {
        Destroy(gameObject, lifeTime);
    }
}