using UnityEngine;

public class ObjectController : MonoBehaviour
{
    [SerializeField] private bool isDestructable = false;
    [SerializeField] private int health = 1;

    [SerializeField] private int experience = 0;
    [SerializeField] private float expSpawnRadius = 1.5f;

    [SerializeField] private Vector2 damageDisplayOffset = new Vector2(0, 0.25f);
    [SerializeField] private GameObject expOrbPrefab;
    [SerializeField] private GameObject damagePopupPrefab;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!isDestructable) return;

        if (other.CompareTag("Bullet"))
        {
            int damage = other.gameObject.GetComponent<BulletController>().damage;
            health -= damage;
            Destroy(other.gameObject);
            DisplayDamage(damage);
            if (health <= 0) DestroyObject();
        }
    }

    private void DestroyObject()
    {
        for (int i = 0; i < experience; i++)
        {
            Vector2 randomOffset = Random.insideUnitCircle * expSpawnRadius;
            Vector3 targetPosition = transform.position + new Vector3(randomOffset.x, randomOffset.y, 0f);

            GameObject orb = Instantiate(expOrbPrefab, transform.position, Quaternion.identity);
            ExpOrbController orbController = orb.GetComponent<ExpOrbController>();
            orbController.expValue = 1;
            orbController.Burst(targetPosition);
        }

        Destroy(gameObject);
    }

    private void DisplayDamage(int damage)
    {
        Vector3 spawnPosition = transform.position + new Vector3(damageDisplayOffset.x, damageDisplayOffset.y, 0f);

        GameObject popup = Instantiate(damagePopupPrefab, spawnPosition, Quaternion.identity);
        popup.GetComponent<DamagePopupController>().Setup(damage);
    }
}
