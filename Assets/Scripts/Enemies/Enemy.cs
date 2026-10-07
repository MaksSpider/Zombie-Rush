using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EnemyAttribute : MonoBehaviour
{
    public Enemy self;
    public abstract void UpdateAttribute(float deltaTime);
}

public class Enemy : DamageHandler
{
    private EnemiesManager _emiesManager => EnemiesManager.Instance; 
    
    public PlayerController playerController => PlayerController.Instance;
    public AttacksManager attacksManager => AttacksManager.Instance;

    [SerializeField] private GameObject _killOrbPrefab;
    [SerializeField] private AudioClip _deathSound;

    private List<EnemyAttribute> _attributes = new();

    [NonSerialized] public Rigidbody2D rb;
    [NonSerialized] public Transform target;

    public string enemyName;
    public float speed;
    public IWeapon weapon;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        target = playerController.transform;
    }

    public void FixedTick(float deltaTime)
    {
        foreach (EnemyAttribute a in _attributes)
        {
            a.UpdateAttribute(deltaTime);
        }
    }

    public override void OnObjectDestroy()
    {
        playerController.ChangeKills(1);
        Instantiate(_killOrbPrefab, transform.position, Quaternion.identity);

        _emiesManager.AddToEnemyPool(enemyName);

        gameObject.GetComponent<AudioSource>().PlayOneShot(_deathSound);

        base.OnObjectDestroy();
    }

    public T AddAttribute<T>() where T : EnemyAttribute
    {
        T attribute = gameObject.AddComponent<T>();

        attribute.self = this;

        _attributes.Add(attribute);

        return attribute;
    }
}
