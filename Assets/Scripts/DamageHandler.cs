using UnityEngine;

public class DamageHandler : MonoBehaviour
{
    [SerializeField] private bool _isDestructable = false;

    [SerializeField] private int _experience = 0;
    [SerializeField] private float _expSpawnRadius = 1.5f;

    [SerializeField] private Vector2 _damageDisplayOffset = new Vector2(0, 0.25f);
    [SerializeField] private Transform _expOrbsTransform;
    [SerializeField] private GameObject _expOrbPrefab;
    [SerializeField] private GameObject _damagePopupPrefab;

    private bool _isDying = false;

    [SerializeField] public int Health = 1;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!_isDestructable) return;

        if (other.CompareTag("Bullet"))
        {
            if (_isDying) return;

            BulletController bulletController = other.gameObject.GetComponent<BulletController>();
            if (bulletController.targetHited) return;

            bulletController.targetHited = true;

            int damage = bulletController.damage;

            Destroy(other.gameObject);
            OnDamageDealt(damage);
        }
    }

    public virtual void OnDamageDealt(int damage)
    {
        Health -= damage;
        DisplayDamage(damage);
        if (Health <= 0) OnObjectDestroy();
    }

    public virtual void OnObjectDestroy()
    {
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

        GameObject popup = Instantiate(_damagePopupPrefab, spawnPosition, _expOrbsTransform != null ? _expOrbsTransform.rotation : Quaternion.identity);
        popup.GetComponent<DamagePopupController>().Setup(damage);
    }
}
