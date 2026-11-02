using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

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

    public void CreateBullet(Vector2 spawnPosition, Vector2 baseDirection, RangeWeapon weapon, float damageModifer = 1f, LayerMask targets = default)
    {
        for (int i = 0; i < weapon.bulletsPerShot; i++)
        {
            float random = Random.Range(-1f, 1f);
            float spread = Mathf.Sign(random) * Mathf.Pow(Mathf.Abs(random), weapon.accuracy / 1f) * weapon.attackAngle;
            float randomAngleOffset = Random.Range(-weapon.attackAngle * 0.5f, weapon.attackAngle * 0.5f);

            Vector2 direction = Quaternion.Euler(0f, 0f, randomAngleOffset) * baseDirection;

            Vector2 bulletPosition = spawnPosition + direction * weapon.bulletSpawnOffset;

            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            Quaternion rotation = Quaternion.Euler(0f, 0f, angle);

            GameObject bulletPrefab = weapon.bulletGameObject == null ? _bulletPrefab : weapon.bulletGameObject;
            GameObject bulletGameObject = Instantiate(bulletPrefab, bulletPosition, rotation, transform);

            AttackController attackCtr = bulletGameObject.GetComponent<AttackController>();

            attackCtr.damage = Mathf.RoundToInt((float)weapon.damage * damageModifer);
            attackCtr.direction = direction.normalized;
            attackCtr.weapon = weapon;

            if (targets.value != 0)
            {
                Collider2D collider = bulletGameObject.GetComponent<Collider2D>();

                collider.layerOverridePriority = 10;
                collider.includeLayers = targets;
                collider.excludeLayers = ~targets;
            }

            Bullets.Add(attackCtr);
        }
    }

    public void CreateMeleeAttack(Vector2 spawnPosition, Vector2 baseDirection, MeleeWeapon weapon, float damageModifer = 1f, LayerMask targets = default)
    {
        Vector2 bulletPosition = spawnPosition + baseDirection * 0.01f;

        float angle = Mathf.Atan2(baseDirection.y, baseDirection.x) * Mathf.Rad2Deg;
        Quaternion rotation = Quaternion.Euler(0f, 0f, angle + 0);

        GameObject bulletPrefab = _meleeBulletPrefab;
        GameObject bulletGameObject = Instantiate(bulletPrefab, bulletPosition, rotation, transform);

        AttackController attackCtr = bulletGameObject.GetComponent<AttackController>();

        attackCtr.damage = Mathf.RoundToInt((float)weapon.damage * damageModifer);
        attackCtr.direction = baseDirection.normalized;
        attackCtr.weapon = weapon;

        if (targets.value != 0)
        {
            Collider2D collider = bulletGameObject.GetComponent<Collider2D>();

            collider.layerOverridePriority = 10;
            collider.includeLayers = targets;
            collider.excludeLayers = ~targets;
        }

        Bullets.Add(attackCtr);
    }
}
