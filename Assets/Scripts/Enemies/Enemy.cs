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

[RequireComponent(typeof(ModifiersController))]
public class Enemy : DamageHandler
{
    [NonSerialized] public string enemyId;

    private EnemiesManager _enemiesManager => EnemiesManager.Instance;
    private OrbsManager _orbsManager => OrbsManager.Instance;

    public PlayerController playerController => PlayerController.Instance;
    public AttacksManager attacksManager => AttacksManager.Instance;
    public ModifiersController modifiersController;

    [SerializeField] private GameObject _killOrbPrefab;

    private readonly List<EnemyAttribute> _attributes = new();

    [NonSerialized] public Rigidbody2D rb;
    [NonSerialized] public Animator animator;

    [NonSerialized] public Vector2 movementDirection;

    [NonSerialized] public IWeapon weapon;

    protected override void Awake()
    {
        base.Awake();

        destroy = false;

        animator = GetComponent<Animator>();

        rb = GetComponent<Rigidbody2D>();
        modifiersController = GetComponent<ModifiersController>();

        modifiersController.AddOnChangeListener(new List<string> { "base_max_health", "max_health_multipler_modifier" }, () =>
        {
            if (_isDying) return;
            SetMaxHealth((int)(modifiersController.GetModifierValue("base_max_health") * (1f + modifiersController.GetModifierValue("max_health_multipler_modifier"))));
        });
    }

    public void Initialize(List<ModifierProperty> startingModifiers)
    {
        modifiersController.ClearModifiers();
        _isDying = false;

        rb.simulated = true;
        foreach (Collider2D col in GetComponents<Collider2D>()) col.enabled = true;

        animator.Rebind();

        modifiersController.AddModifier(startingModifiers);
    }

    public void FixedTick(float deltaTime)
    {
        if (!gameObject.activeSelf) return;
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
        _enemiesManager.ReleaseEnemy(this);

        base.OnObjectDestroy();
    }

    public override void OnObjectDestroy()
    {
        if (_isDying) return;

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
