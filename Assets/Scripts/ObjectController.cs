using UnityEngine;

public class ObjectController : MonoBehaviour
{
    [SerializeField] private bool _isDestructable = false;
    [SerializeField] private int _health = 1;

    [SerializeField] private int _experience = 0;
    [SerializeField] private float _expSpawnRadius = 1.5f;

    [SerializeField] private Vector2 _damageDisplayOffset = new Vector2(0, 0.25f);
    [SerializeField] private GameObject _expOrbPrefab;
    [SerializeField] private GameObject _damagePopupPrefab;

    private bool _isDying = false;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!_isDestructable) return;

        if (other.CompareTag("Bullet"))
        {
            BulletController bulletController = other.gameObject.GetComponent<BulletController>();
            if (bulletController.targetHited) return;

            bulletController.targetHited = true;

            int damage = bulletController.damage;
            _health -= damage;
            Destroy(other.gameObject);
            DisplayDamage(damage);
            if (_health <= 0) DestroyObject();
        }
    }

    private void DestroyObject()
    {
        if(_isDying) return;
        _isDying = true;

        for (int i = 0; i < _experience; i++)
        {
            Vector2 randomOffset = Random.insideUnitCircle * _expSpawnRadius;
            Vector3 targetPosition = transform.position + new Vector3(randomOffset.x, randomOffset.y, 0f);

            GameObject orb = Instantiate(_expOrbPrefab, transform.position, Quaternion.identity);
            ExpOrbController orbController = orb.GetComponent<ExpOrbController>();
            orbController.expValue = 1;
            orbController.Burst(targetPosition);
        }

        Destroy(gameObject);
    }

    private void DisplayDamage(int damage)
    {
        Vector3 spawnPosition = transform.position + new Vector3(_damageDisplayOffset.x, _damageDisplayOffset.y, 0f);

        GameObject popup = Instantiate(_damagePopupPrefab, spawnPosition, Quaternion.identity);
        popup.GetComponent<DamagePopupController>().Setup(damage);
    }
}
