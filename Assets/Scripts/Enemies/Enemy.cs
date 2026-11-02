using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public abstract class EnemyAttribute : MonoBehaviour
{
    public Enemy self;
    public abstract void UpdateAttribute(float deltaTime);
}

public class Enemy : DamageHandler
{
    private EnemiesManager _enemiesManager => EnemiesManager.Instance;
    private OrbsManager _orbsManager => OrbsManager.Instance;

    public PlayerController playerController => PlayerController.Instance;
    public AttacksManager attacksManager => AttacksManager.Instance;

    [SerializeField] private GameObject _killOrbPrefab;

    private readonly List<EnemyAttribute> _attributes = new();

    [NonSerialized] public Rigidbody2D rb;
    [NonSerialized] public Transform target;
    [NonSerialized] public Animator animator;

    public Vector2 movementDirection;

    [NonSerialized] public string enemyName;
    [NonSerialized] public float speed;
    [NonSerialized] public float damageModifier;
    [NonSerialized] public IWeapon weapon;

    protected override void Awake()
    {
        base.Awake();

        animator = GetComponent<Animator>();

        rb = GetComponent<Rigidbody2D>();
        target = playerController.transform;
    }

    public void FixedTick(float deltaTime)
    {
        if (_isDying) return;
        foreach (EnemyAttribute a in _attributes)
        {
            a.UpdateAttribute(deltaTime);
        }

        if (animator == null || rb == null)
            return;

        if (movementDirection.sqrMagnitude > 0.001f)
        {
            animator.SetFloat("DirectionHorizontal", movementDirection.x);
            animator.SetFloat("DirectionVertical", movementDirection.y);
        }
    }

    private IEnumerator DestroyAfterAnimation()
    {
        yield return new WaitForSeconds(1.2f);

        _orbsManager.CreateOrb(_killOrbPrefab, transform.position, transform.position, false, true);
        _enemiesManager.AddToEnemyPool(enemyName);

        base.OnObjectDestroy();
    }

    public override void OnObjectDestroy()
    {
        _isDying = true;

        rb.simulated = false;

        foreach (Collider2D col in GetComponents<Collider2D>())
        {
            col.enabled = false;
        }

        animator.SetTrigger("Death");

        StartCoroutine(DestroyAfterAnimation());
    }

    public T AddAttribute<T>() where T : EnemyAttribute
    {
        T attribute = gameObject.AddComponent<T>();
        attribute.self = this;
        _attributes.Add(attribute);

        return attribute;
    }
}
