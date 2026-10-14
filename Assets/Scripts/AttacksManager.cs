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

    public void CreateBullet(Vector2 spawnPosition, Vector2 baseDirection, Weapon weapon, float damageModifer = 1f, LayerMask targets = default)
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
            attackCtr.speed = weapon.bulletSpeed;
            attackCtr.maxLifeTime = weapon.attackRange / weapon.bulletSpeed;
            attackCtr.multiTarget = false;

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
        Vector2 bulletPosition = spawnPosition + baseDirection * 0.2f;

        float angle = Mathf.Atan2(baseDirection.y, baseDirection.x) * Mathf.Rad2Deg;
        Quaternion rotation = Quaternion.Euler(0f, 0f, angle + 0);

        GameObject bulletPrefab = _meleeBulletPrefab;
        GameObject bulletGameObject = Instantiate(bulletPrefab, bulletPosition, rotation, transform);

        AttackController attackCtr = bulletGameObject.GetComponent<AttackController>();

        attackCtr.damage = Mathf.RoundToInt((float)weapon.damage * damageModifer);
        attackCtr.direction = baseDirection.normalized;
        attackCtr.speed = 4f;
        attackCtr.maxLifeTime = weapon.attackRange / 4f;
        attackCtr.multiTarget = weapon.multiTargeting;

        DrawSemicircle(bulletGameObject, 0.32f, 0.25f, weapon.attackAngle);

        if (targets.value != 0)
        {
            Collider2D collider = bulletGameObject.GetComponent<Collider2D>();

            collider.layerOverridePriority = 10;
            collider.includeLayers = targets;
            collider.excludeLayers = ~targets;
        }

        Bullets.Add(attackCtr);
    }

    private void DrawSemicircle(GameObject gameObject, float radius, float innerRadius, float spreadAngle, int segments = 40)
    {
        Vector3[] vertices = new Vector3[(segments + 1) * 2];
        int[] triangles = new int[segments * 6];

        for (int i = 0; i <= segments; i++)
        {
            float angle = Mathf.Deg2Rad * (-spreadAngle / 2f + spreadAngle * i / segments);

            float x = Mathf.Cos(angle);
            float y = Mathf.Sin(angle);

            vertices[i * 2] = new Vector3(
                x * innerRadius,
                y * innerRadius,
                0f
            );

            vertices[i * 2 + 1] = new Vector3(
                x * radius,
                y * radius,
                0f
            );
        }

        for (int i = 0; i < segments; i++)
        {
            int innerCurrent = i * 2;
            int outerCurrent = innerCurrent + 1;
            int innerNext = innerCurrent + 2;
            int outerNext = innerCurrent + 3;

            triangles[i * 6] = innerCurrent;
            triangles[i * 6 + 1] = outerCurrent;
            triangles[i * 6 + 2] = outerNext;

            triangles[i * 6 + 3] = innerCurrent;
            triangles[i * 6 + 4] = outerNext;
            triangles[i * 6 + 5] = innerNext;
        }

        Mesh mesh = gameObject.GetComponent<MeshFilter>().mesh;

        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        gameObject.GetComponent<MeshRenderer>().material.color = new Color(1f, 1f, 1f, 0.75f);
    }
}
