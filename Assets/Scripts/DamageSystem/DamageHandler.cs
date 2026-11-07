using TMPro;
using UnityEngine;

public class DamageHandler : MonoBehaviour
{
    private PlayerController _playerController => PlayerController.Instance;
    private OrbsManager _orbsManager => OrbsManager.Instance;

    [SerializeField] private bool _isDestructable = false;

    [SerializeField] private float _expSpawnRadius = 1.5f;

    [SerializeField] private Vector2 _damageDisplayOffset = new Vector2(0, 0.25f);
    [SerializeField] private GameObject _expOrbPrefab;
    [SerializeField] private GameObject _cashOrbPrefab;
    [SerializeField] private GameObject _damagePopupPrefab;

    [SerializeField] private AudioClip _deathSound;

    public bool destroy = true;

    protected bool _isDying = false;

    private static int _nextId = 0;
    public int ID { get; private set; }

    [SerializeField] public int Health = 1;
    [SerializeField] public int MaxHealth = 1;

    protected virtual void Awake()
    {
        ID = _nextId++;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!_isDestructable) return;

        if (other.CompareTag("Bullet"))
        {
            if (_isDying) return;

            AttackController attackController = other.gameObject.GetComponent<AttackController>();
            if (attackController == null) return;

            if (!attackController.Hit(ID)) return;
            ChangeHealth(-attackController.damage);
            DisplayDamage(attackController.damage);
        }
    }

    public void ChangeHealth(int value)
    {
        Health += value;

        if (Health > MaxHealth)
        {
            Health = MaxHealth;
        }

        OnChangeHealth(value);

        if (Health <= 0)
        {
            OnObjectDestroy();
        }
    }

    protected virtual void OnChangeHealth(int value) { }

    public void SetMaxHealth(int value)
    {
        int delta = value - MaxHealth;
        MaxHealth = value;

        if (delta > 0) Health += delta;
        if (Health > MaxHealth) Health = MaxHealth;

        OnChangeMaxHealth(value);
    }

    protected virtual void OnChangeMaxHealth(int value) { }

    public virtual void OnObjectDestroy()
    {
        _isDying = true;

        if (_deathSound != null)
        {
            AudioSource.PlayClipAtPoint(_deathSound, transform.position);
        }

        ModifiersController modifiersController = GetComponent<ModifiersController>();

        if(modifiersController != null)
        {
            int xpToSpawn = Mathf.RoundToInt(modifiersController.GetModifierValue("base_experience_drop") * (1 + modifiersController.GetModifierValue("experience_multipler_modifier")) * (1 + _playerController.modifiersController.GetModifierValue("experience_gain_multipler_modifier")));

            for (int i = 0; i < xpToSpawn; i++)
            {
                Vector2 randomOffset = Random.insideUnitCircle * _expSpawnRadius;
                Vector3 targetPosition = transform.position + new Vector3(randomOffset.x, randomOffset.y, 0f);

                _orbsManager.CreateOrb(_expOrbPrefab, transform.position, targetPosition);
            }

            int money = (int)(modifiersController.GetModifierValue("base_money_drop") * (1f + modifiersController.GetModifierValue("money_drop_multipler_modifier")) * (1f + _playerController.modifiersController.GetModifierValue("money_multipler_modifier")));

            int cashPerOrb = 1;
            int cashToDrop = money;

            if (money >= 10)
            {
                cashPerOrb = 5;
            }

            int orbsToDrop = money / cashPerOrb;
            cashToDrop = money % cashPerOrb;

            for (int i = 0; i < orbsToDrop; i++)
            {
                Vector2 randomOffset = Random.insideUnitCircle * _expSpawnRadius;
                Vector3 targetPosition = transform.position + new Vector3(randomOffset.x, randomOffset.y, 0f);

                OrbController orbCtr = _orbsManager.CreateOrb(
                    _cashOrbPrefab, transform.position, targetPosition
                );

                orbCtr.GetComponent<CashOrbController>().money = cashPerOrb;
            }

            while (cashToDrop > 0)
            {
                Vector2 randomOffset = Random.insideUnitCircle * _expSpawnRadius;
                Vector3 targetPosition = transform.position + new Vector3(randomOffset.x, randomOffset.y, 0f);

                OrbController orbCtr = _orbsManager.CreateOrb(
                    _cashOrbPrefab, transform.position, targetPosition
                );

                orbCtr.GetComponent<CashOrbController>().money = 1;
                cashToDrop--;
            }
        }

        DropOnDestroyController dropCtr = gameObject.GetComponent<DropOnDestroyController>();

        if (dropCtr != null)
        {
            dropCtr.DropLoot();
        }

        if (destroy)
        {
            Destroy(gameObject);
        }
    }

    private void DisplayDamage(int damage)
    {
        Vector3 spawnPosition = transform.position + new Vector3(_damageDisplayOffset.x, _damageDisplayOffset.y, 0f);

        GameObject popup = Instantiate(_damagePopupPrefab, spawnPosition, Quaternion.identity);
        popup.GetComponent<DamagePopupController>().Setup(damage);
    }
}
