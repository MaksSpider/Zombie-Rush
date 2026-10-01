using UnityEngine;

public class EnemyController : ObjectController
{
    private PlayerController _playerController;
    private WorldGenerator _worldGenerator;

    public float moveSpeed = 2f;
    public int attack = 2;

    [SerializeField] private GameObject _killOrbPrefab;

    private Rigidbody2D _rb;
    private Transform _target;

    private float _attackCooldown = 1f;
    private float _nextAttackTime;

    void Awake()
    {
        _playerController = PlayerController.Instance;
        _worldGenerator = WorldGenerator.Instance;

        _rb = GetComponent<Rigidbody2D>();
        _target = _playerController.transform;
    }

    void Start()
    {
        //float randomOffset = Random.Range(0f, 2f);
        //InvokeRepeating(nameof(CheckProximity), randomOffset, 2f);
    }

    void FixedUpdate()
    {
        HandleMovement();
    }

    private void OnDestroy()
    {
        CancelInvoke();

        if (!gameObject.scene.isLoaded) return;

        _playerController.ChangeKills(1);
        Instantiate(_killOrbPrefab, transform.position, Quaternion.identity);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            AttackPlayer();
        }
    }

    private void HandleMovement()
    {
        Vector2 dir = (_target.position - transform.position).normalized;
        _rb.MovePosition(_rb.position + dir * moveSpeed * Time.fixedDeltaTime);
    }

    private void AttackPlayer()
    {
        if (Time.time < _nextAttackTime) return;
        _nextAttackTime = Time.time + _attackCooldown;

        _playerController.ChangeHealth(-attack);
    }
}