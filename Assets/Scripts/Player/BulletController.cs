using UnityEngine;

public class BulletController : MonoBehaviour
{
    private float _lifeTime = 10f;
    
    public int damage;
    public bool targetHited = false;

    private void Start()
    {
        Destroy(gameObject, _lifeTime);
    }
}