using UnityEngine;

public class EnemyController : ObjectController
{
    private PlayerController playerController;
    private WorldGenerator worldGenerator;

    public float moveSpeed = 2f;
    public int attack = 2;

    [SerializeField] private GameObject killOrbPrefab;

    private Rigidbody2D rb;
    private Transform target;

    private float attackCooldown = 1f;
    private float nextAttackTime;

    private bool isNearPlayer = true;
    private float lastMoveTime;

    void Awake()
    {
        playerController = PlayerController.Instance;
        worldGenerator = WorldGenerator.Instance;

        rb = GetComponent<Rigidbody2D>();
        target = playerController.transform;
    }

    void Start()
    {
        lastMoveTime = Time.time;

        float randomOffset = Random.Range(0f, 2f);
        InvokeRepeating(nameof(CheckProximity), randomOffset, 2f);
    }

    void FixedUpdate()
    {
        if (!isNearPlayer) return;

        HandleMovement(Time.fixedDeltaTime);
        lastMoveTime = Time.time;
    }

    private void OnDestroy()
    {
        CancelInvoke();

        if (!gameObject.scene.isLoaded) return;

        playerController.Kills++;
        Instantiate(killOrbPrefab, transform.position, Quaternion.identity);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            AttackPlayer();
        }
    }

    private void HandleMovement(float time)
    {
        Vector2 dir = (target.position - transform.position).normalized;
        rb.MovePosition(rb.position + dir * moveSpeed * time);
    }

    private void CheckProximity()
    {
        float distance = Vector2.Distance(playerController.transform.position, gameObject.transform.position);

        isNearPlayer = distance < worldGenerator.RenderDistance * 2f;

        if (!isNearPlayer)
        {
            float delta = Time.time - lastMoveTime;
            HandleMovement(delta);
            lastMoveTime = Time.time;
        }
    }

    private void AttackPlayer()
    {
        if (Time.time < nextAttackTime) return;
        nextAttackTime = Time.time + attackCooldown;

        playerController.ChangeHealth(-attack);
    }
}