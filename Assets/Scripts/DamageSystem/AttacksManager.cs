using System.Collections.Generic;
using UnityEngine;

public class AttackOptions
{
    public float accuracy { get; set; } = 0.0f;
    public float damageModifer { get; set; } = 1f;
    public LayerMask targets { get; set; } = default;
}

public class AttacksManager : MonoBehaviour
{
    public static AttacksManager Instance;

    [SerializeField] private GameObject _bulletPrefab;
    [SerializeField] private GameObject _meleeBulletPrefab;

    public List<AttackController> Bullets = new List<AttackController>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void FixedTick(float deltaTime)
    {
        for (int i = Bullets.Count - 1; i >= 0; i--)
        {
            AttackController bullet = Bullets[i];

            if (bullet == null)
            {
                Bullets.RemoveAt(i);
                continue;
            }

            bullet.FixedTick(deltaTime);
        }
    }

    public void CreateBullet(Vector2 spawnPosition, Vector2 baseDirection, RangeWeapon weapon, AttackOptions options = default)
    {
        for (int i = 0; i < weapon.bulletsPerShot; i++)
        {
            float spread = (weapon.attackAngle + options.accuracy) * (1f - Mathf.Clamp01(weapon.accuracy));
            float randomAngleOffset = Random.Range(-spread * 0.5f, spread * 0.5f);
            Vector2 direction = Quaternion.Euler(0f, 0f, randomAngleOffset) * baseDirection;

            Vector2 bulletPosition = spawnPosition + direction * weapon.bulletSpawnOffset;

            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            Quaternion rotation = Quaternion.Euler(0f, 0f, angle);

            GameObject bulletPrefab = weapon.bulletGameObject == null ? _bulletPrefab : weapon.bulletGameObject;
            GameObject bulletGameObject = Instantiate(bulletPrefab, bulletPosition, rotation, transform);

            AttackController attackCtr = bulletGameObject.GetComponent<AttackController>();

            attackCtr.damage = Mathf.RoundToInt((float)weapon.damage * options.damageModifer);
            attackCtr.direction = direction.normalized;
            attackCtr.weapon = weapon;

            if (options.targets.value != 0)
            {
                Collider2D collider = bulletGameObject.GetComponent<Collider2D>();

                collider.layerOverridePriority = 10;
                collider.includeLayers = options.targets;
                collider.excludeLayers = ~options.targets;
            }

            Bullets.Add(attackCtr);
        }
    }

    public void CreateMeleeAttack(Vector2 spawnPosition, Vector2 baseDirection, MeleeWeapon weapon, AttackOptions options = default)
    {
        Vector2 bulletPosition = spawnPosition + baseDirection * 0.01f;

        float angle = Mathf.Atan2(baseDirection.y, baseDirection.x) * Mathf.Rad2Deg;
        Quaternion rotation = Quaternion.Euler(0f, 0f, angle + 0);

        GameObject bulletPrefab = _meleeBulletPrefab;
        GameObject bulletGameObject = Instantiate(bulletPrefab, bulletPosition, rotation, transform);

        AttackController attackCtr = bulletGameObject.GetComponent<AttackController>();

        attackCtr.damage = Mathf.RoundToInt((float)weapon.damage * options.damageModifer);
        attackCtr.direction = baseDirection.normalized;
        attackCtr.weapon = weapon;

        if (options.targets.value != 0)
        {
            Collider2D collider = bulletGameObject.GetComponent<Collider2D>();

            collider.layerOverridePriority = 10;
            collider.includeLayers = options.targets;
            collider.excludeLayers = ~options.targets;
        }

        Bullets.Add(attackCtr);
    }
}
