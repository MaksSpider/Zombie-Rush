using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EnemyBuilder
{
    private GameObject _prefab;
    private readonly List<Action<Enemy>> _buildSteps = new();

    public EnemyBuilder SetBase(GameObject prefab)
    {
        _prefab = prefab;
        return this;
    }

    public EnemyBuilder WithAttribute<T>() where T : EnemyAttribute
    {
        _buildSteps.Add(enemy =>
        {
            enemy.AddAttribute<T>();
        });

        return this;
    }

    public EnemyBuilder WithAttribute<T>(Action<T> configure) where T : EnemyAttribute
    {
        _buildSteps.Add(enemy =>
        {
            T attribute = enemy.AddAttribute<T>();
            configure(attribute);
        });

        return this;
    }

    public Enemy Build(Vector2 position, Transform transform)
    {
        Enemy enemy = UnityEngine.Object.Instantiate(_prefab, position, Quaternion.identity, transform).GetComponent<Enemy>();

        foreach (Action<Enemy> step in _buildSteps)
        {
            step(enemy);
        }

        _buildSteps.Clear();

        return enemy;
    }
}

public class EnemiesManager : MonoBehaviour
{
    public static EnemiesManager Instance { get; private set; }

    private PlayerController _playerController => PlayerController.Instance;

    [SerializeField] private bool _isWave = false;
    [SerializeField] private float _minDistanceFromPlayer = 15f;
    [SerializeField] private float _maxDistanceFromPlayer = 35f;

    [SerializeField] private GameObject _debugEnemy;
    [SerializeField] private GameObject axeBullet;


    [SerializeField] private GameObject _simpleZombie;

    [SerializeField] private GameObject _clock;
    [SerializeField] private GameObject _clockHand;
    [SerializeField] private GameObject _nightPie;
    [SerializeField] private GameObject _dayPie;
    [SerializeField] private TextMeshProUGUI _daysCounter;
    [SerializeField] private float _dayTimeDuration = 300f;
    [SerializeField] private float _nightTimeDuration = 0.7f;
    [SerializeField] private int _dayNumber = 1;

    private EnemyBuilder _builder;
    private float _currentTime = 0;

    public List<Enemy> Enemies = new();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        _builder = new EnemyBuilder();

        SetupUI();

        _clock.GetComponent<Button>().onClick.AddListener(() => {
            SkipToNight();
        });
    }

    public void FixedTick(float fixedDeltaTime)
    {
        for (int i = Enemies.Count - 1; i >= 0; i--)
        {
            Enemy enemy = Enemies[i];

            if (enemy == null)
            {
                Enemies.RemoveAt(i);
                continue;
            }

            enemy.FixedTick(fixedDeltaTime);
        }
    }

    public void Tick1s()
    {
        _currentTime += 1f;
        UpdateUI();

        if(_currentTime >= _dayTimeDuration) EndWave();
        if(_currentTime > _dayTimeDuration - _nightTimeDuration) StartWave();
    }

    public void Tick5s()
    {
        if (!_isWave) return;
        SpawnEnemiesFromPool();
    }

    private void SpawnEnemiesFromPool()
    {
        for (int i = 0; i < 10; i++)
        {
            Vector2 direction = UnityEngine.Random.insideUnitCircle.normalized;
            float distance = UnityEngine.Random.Range(_minDistanceFromPlayer, _maxDistanceFromPlayer);

            Vector2 spawnPosition = (Vector2)_playerController.transform.position + direction * distance;

            SpawnEnemy(spawnPosition);
        }
    }

    private void SetupUI()
    {
        _nightPie.GetComponent<Image>().fillAmount = _nightTimeDuration;
        _dayPie.GetComponent<Image>().fillAmount = (1 - _nightTimeDuration);
        _daysCounter.text = "Day " + _dayNumber.ToString();
    }

    private void UpdateUI()
    {
        float rotation = -(_currentTime / _dayTimeDuration) * 360;

        _clockHand.transform.rotation = Quaternion.Euler(0, 0, rotation);
        _daysCounter.text = "Day " + _dayNumber.ToString();
    }

    public void SpawnEnemy(Vector2 spawnPosition)
    {
        _builder.SetBase(_simpleZombie);
        Enemy Zombie = _builder
            .WithAttribute<RangedAttack>(attack =>
            {
                attack.weapon.bulletGameObject = axeBullet;
                attack.weapon.damage = 5;
                attack.weapon.attackRate = 2.5f;
            })
            .WithAttribute<CircleMovement>()
            .Build(spawnPosition, transform);

        Enemies.Add(Zombie);
    }

    public void SkipToNight()
    {
        float nightStartTime = _dayTimeDuration - (_nightTimeDuration * _dayTimeDuration);

        if (_currentTime > nightStartTime) return;

        _currentTime = nightStartTime;
        StartWave();
        UpdateUI();
    }

    public void StartWave(List<string> pool = null)
    {
        //foreach (string s in pool)
        //{
        //    //EnemiesPool.Add(s);
        //}

        _isWave = true;
        SpawnEnemiesFromPool();
    }

    public void EndWave()
    {
        _isWave = false;
        _currentTime = 0f;
        _playerController.Score += 5 * _dayNumber;
        _dayNumber++;
        SetupUI();
    }
}
