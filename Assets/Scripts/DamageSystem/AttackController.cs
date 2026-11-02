using System;
using System.Collections.Generic;
using UnityEngine;

public class AttackController : MonoBehaviour
{
    [SerializeField] private Color color;

    private float lifeTime;

    private Rigidbody2D _rb;
    private MeshFilter _meshFilter;
    private MeshRenderer _meshRenderer;
    private PolygonCollider2D _polygonCollider;

    private List<int> hittedIds = new();

    [NonSerialized] public Vector2 direction;
    [NonSerialized] public IWeapon weapon;
    [NonSerialized] public int damage;

    void Awake()
    {
        _meshFilter = gameObject.GetComponent<MeshFilter>();
        _meshRenderer = gameObject.GetComponent<MeshRenderer>();
        _polygonCollider = GetComponent<PolygonCollider2D>();

        _rb = GetComponent<Rigidbody2D>();
        _rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    }

    void Start()
    {
        if (!_meshFilter || !_meshRenderer) return;
        DrawSemicircle(0.32f, 0.25f, weapon.attackAngle);
    }

    public bool Hit(int id)
    {
        if (!weapon.multiTargeting)
        {
            Destroy(gameObject);
            return true;
        }

        for(int i = 0; i < hittedIds.Count; i++)
        {
            if(hittedIds[i] == id) return false;
        }

        hittedIds.Add(id);
        return true;
    }

    public void FixedTick(float deltaTime)
    {
        lifeTime += deltaTime * weapon.attackSpeed;

        if (lifeTime >= weapon.attackRange)
        {
            Destroy(gameObject);
            return;
        }

        Vector2 nextPosition = _rb.position + direction.normalized * weapon.attackSpeed * deltaTime;
        _rb.MovePosition(nextPosition);
    }

    private void DrawSemicircle(float radius, float innerRadius, float spreadAngle, int segments = 40)
    {
        Vector3[] vertices = new Vector3[(segments + 1) * 2];
        int[] triangles = new int[segments * 6];

        Vector2[] colliderPoints = new Vector2[(segments + 1) * 2];

        for (int i = 0; i <= segments; i++)
        {
            float angle = Mathf.Deg2Rad * (-spreadAngle / 2f + spreadAngle * i / segments);

            float x = Mathf.Cos(angle);
            float y = Mathf.Sin(angle);

            Vector2 inner = new Vector2(
                x * innerRadius,
                y * innerRadius
            );

            Vector2 outer = new Vector2(
                x * radius,
                y * radius
            );

            vertices[i * 2] = inner;
            vertices[i * 2 + 1] = outer;

            colliderPoints[i] = outer;

            colliderPoints[segments + 1 + i] =
                new Vector2(
                    Mathf.Cos(Mathf.Deg2Rad *
                        (spreadAngle / 2f -
                         spreadAngle * i / segments)) * innerRadius,
                    Mathf.Sin(Mathf.Deg2Rad *
                        (spreadAngle / 2f -
                         spreadAngle * i / segments)) * innerRadius
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

        Mesh mesh = _meshFilter.mesh;
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        _meshRenderer.material.color = color;

        if (_polygonCollider != null)
        {
            _polygonCollider.SetPath(0, colliderPoints);
        }
    }
}