using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class DamageHandler : MonoBehaviour
{
    private UpgradesController _upgradesController => UpgradesController.Instance;

    [SerializeField] private bool _isDestructable = false;

    [SerializeField] private float _expSpawnRadius = 1.5f;

    [SerializeField] private Vector2 _damageDisplayOffset = new Vector2(0, 0.25f);
    [SerializeField] private Transform _expOrbsTransform;
    [SerializeField] private GameObject _expOrbPrefab;
    [SerializeField] private GameObject _damagePopupPrefab;

    [SerializeField] private AudioClip _deathSound;

    [SerializeField] public int experience = 0;

    private bool _isDying = false;

    [SerializeField] public int Health = 1;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!_isDestructable) return;

        if (other.CompareTag("Bullet"))
        {
            if (_isDying) return;

            AttackController attackController = other.gameObject.GetComponent<AttackController>();

            if (attackController.targetHited && !attackController.multiTarget) return;
            attackController.targetHited = true;

            if(!attackController.multiTarget) Destroy(other.gameObject);

            OnDamageDealt(attackController.damage);
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

        if (_deathSound != null)
        {
            AudioSource.PlayClipAtPoint(_deathSound, transform.position);
        }

        ParticleSystem particleSystem = GetComponent<ParticleSystem>();
        if (particleSystem != null)
        {
            particleSystem.Play();
        }

        int xpToSpawn = Mathf.RoundToInt((float)experience * (1 + _upgradesController.GetBonusValues("additionalXp")));

        for (int i = 0; i < xpToSpawn; i++)
        {
            Vector2 randomOffset = Random.insideUnitCircle * _expSpawnRadius;
            Vector3 targetPosition = transform.position + new Vector3(randomOffset.x, randomOffset.y, 0f);

            GameObject orb = Instantiate(_expOrbPrefab, transform.position, Quaternion.identity);
            ExpOrbController orbController = orb.GetComponent<ExpOrbController>();
            orbController.expValue = 1;
            orbController.Burst(targetPosition);
        }

        DropOnDestroyController dropCtr = gameObject.GetComponent<DropOnDestroyController>();

        if(dropCtr != null)
        {
            dropCtr.DropLoot();
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
