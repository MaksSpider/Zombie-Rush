using System.Collections.Generic;
using UnityEngine;

public class HordesManager : MonoBehaviour
{
    public static HordesManager Instance { get; private set; }

    private PlayerController playerController;

    [SerializeField] private bool isWave = false;
    [SerializeField] private float minDistanceFromPlayer = 5f;
    [SerializeField] private float maxDistanceFromPlayer = 5f;

    [SerializeField] private GameObject DebugEnemy;

    [SerializeField] private Transform enemiesTransform;

    public List<string> EnemiesPool;

    void Start()
    {
        InvokeRepeating(nameof(SpawnEnemy), 0f, 1f);
    }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        playerController = PlayerController.Instance;
    }

    void Update()
    {
        
    }

    void SpawnEnemy()
    {
        if (!isWave) return;
        //if (EnemiesPool.Count < 1) return;

        for (int i = 0; i < 20; i++)
        {
            int randomEnemy = Random.Range(0, EnemiesPool.Count);
            //string enemyToSpawnName = EnemiesPool[randomEnemy];

            // get enemy by name from database

            Vector2 direction = Random.insideUnitCircle.normalized;
            float distance = Random.Range(minDistanceFromPlayer, maxDistanceFromPlayer);

            Vector2 spawnPosition = (Vector2)playerController.transform.position + direction * distance;

            GameObject enemy = Instantiate(
                DebugEnemy,
                spawnPosition,
                Quaternion.identity,
                enemiesTransform
            );
        }
    }

    public void StartWave(List<string> pool)
    {
        foreach(string s in pool)
        {
            EnemiesPool.Add(s);
        }

        isWave = true;
    }

    public void StopWave()
    {
        isWave = false;
    }
}
