using System.Collections.Generic;
using UnityEngine;

public class AttacksManager : MonoBehaviour
{
    public static AttacksManager Instance;

    [SerializeField] private float _bulletsLifeTime = 10f;
    [SerializeField] private GameObject _bulletPrefab;

    public List<BulletController> Bullets = new List<BulletController>();

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
            BulletController bullet = Bullets[i];

            if (bullet == null)
            {
                Bullets.RemoveAt(i);
                continue;
            }

            bullet.FixedTick(deltaTime);
        }
    }

    public void CreateBullet(Vector2 spawnPosition, Vector2 baseDirection, Weapon weapon, float maxSpreadAngle, Vector2 offset, float damageModifer = 1f, LayerMask targets = default)
    {
        for (int i = 0; i < weapon.bulletsPerShot; i++)
        {
            float spread = maxSpreadAngle * (1f - Mathf.Clamp01(weapon.accuracy));
            float randomAngleOffset = Random.Range(-spread * 0.5f, spread * 0.5f);

            Vector2 direction = Quaternion.Euler(0f, 0f, randomAngleOffset) * baseDirection;

            spawnPosition += direction * weapon.bulletSpawnOffset + offset;

            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            Quaternion rotation = Quaternion.Euler(0f, 0f, angle);

            GameObject bulletPrefab = weapon.bulletGameObject == null ? _bulletPrefab : weapon.bulletGameObject;
            GameObject bulletGameObject = Instantiate(bulletPrefab, spawnPosition, rotation, transform);

            BulletController bulletCtr = bulletGameObject.GetComponent<BulletController>();

            print(damageModifer);
            print((int)((float)weapon.damage * damageModifer));

            bulletCtr.damage = Mathf.RoundToInt((float)weapon.damage * damageModifer);
            bulletCtr.direction = direction.normalized;
            bulletCtr.speed = weapon.bulletSpeed;
            bulletCtr.maxLifeTime = weapon.attackRange == 0f ? _bulletsLifeTime : weapon.attackRange;

            if (targets.value != 0)
            {
                Collider2D collider = bulletGameObject.GetComponent<Collider2D>();

                collider.layerOverridePriority = 10;
                collider.includeLayers = targets;
                collider.excludeLayers = ~targets;
            }

            Bullets.Add(bulletCtr);
        }
    }

    public void CreateMeleeAttack(Vector2 spawnPosition, Vector2 baseDirection, MeleeWeapon weapon, LayerMask ignoreTargets = default)
    {

    }
}
