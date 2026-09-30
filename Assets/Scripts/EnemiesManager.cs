using System.Collections.Generic;
using UnityEngine;

public class EnemiesManager : MonoBehaviour
{
    public static EnemiesManager Instance { get; private set; }

    private PlayerController _playerController;

    [SerializeField] private bool _isWave = false;
    [SerializeField] private float _minDistanceFromPlayer = 15f;
    [SerializeField] private float _maxDistanceFromPlayer = 35f;

    [SerializeField] private GameObject _debugEnemy;

    public List<string> EnemiesPool;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        _playerController = PlayerController.Instance;
    }

    public void Tick1s()
    {
        if (!_isWave) return;

        SpawnEnemies(20);
    }

    void SpawnEnemies(int amount)
    {
        //if (EnemiesPool.Count < 1) return;

        for (int i = 0; i < amount; i++)
        {
            int randomEnemy = Random.Range(0, EnemiesPool.Count);
            //string enemyToSpawnName = EnemiesPool[randomEnemy];

            // get enemy by name from database

            Vector2 direction = Random.insideUnitCircle.normalized;
            float distance = Random.Range(_minDistanceFromPlayer, _maxDistanceFromPlayer);

            Vector2 spawnPosition = (Vector2)_playerController.transform.position + direction * distance;

            GameObject enemy = Instantiate(
                _debugEnemy,
                spawnPosition,
                Quaternion.identity,
                transform
            );
        }
    }

    public void StartWave(List<string> pool = null)
    {
        foreach(string s in pool)
        {
            EnemiesPool.Add(s);
        }

        _isWave = true;
    }

    public void StopWave()
    {
        _isWave = false;
    }
}
