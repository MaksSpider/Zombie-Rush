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
    private InventoryController _inventoryController => InventoryController.Instance;

    [SerializeField] private float _minDistanceFromPlayer = 15f;
    [SerializeField] private float _maxDistanceFromPlayer = 35f;

    [SerializeField] private EnemiesData _enemiesData;

    [SerializeField] private GameObject _clock;
    [SerializeField] private GameObject _clockHand;
    [SerializeField] private GameObject _nightPie;
    [SerializeField] private GameObject _dayPie;
    [SerializeField] private TextMeshProUGUI _daysCounter;
    [SerializeField] private float _dayTimeDuration = 300f;
    [SerializeField] private float _nightTimeDuration = 0.3f;

    [SerializeField] private int _spawningEnemiesAmount = 5;
    [SerializeField] private int _startingEnemiesAmount = 10;
    [SerializeField] private int _additionalNewEnemiesPerDay = 2;

    public int dayNumber { get; private set; } = 1;
    public Action OnDayStart;
    public Action OnNightStart;

    public bool _isWave { get; private set; } = false;

    private float _currentTime = 0;
    public List<EnemyData> _enemiesPool = new();

    public List<Enemy> Enemies = new();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        SetupUI();

        _clock.GetComponent<Button>().onClick.AddListener(() => {
            SkipToNight();
        });
    }

    void Start()
    {
        AddToEnemyPool(_startingEnemiesAmount);
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

        float nightStartTime = _dayTimeDuration * (1f - _nightTimeDuration);

        if (!_isWave && _currentTime >= nightStartTime) StartWave();
        if (_isWave && _currentTime >= _dayTimeDuration) EndWave();
    }

    public void Tick5s()
    {
        if (!_isWave) return;
        if (_currentTime >= _dayTimeDuration * 0.8) return;
        SpawnEnemiesFromPool();
    }

    private void AddAttributeByType(EnemyBuilder builder, EnemyAttributeType type)
    {
        switch (type)
        {
            case EnemyAttributeType.SimpleMovement:
                builder.WithAttribute<SimpleMovement>();
                break;

            case EnemyAttributeType.CircleMovement:
                builder.WithAttribute<CircleMovement>();
                break;

            case EnemyAttributeType.MeleeAttack:
                builder.WithAttribute<MeleeAttack>();
                break;

            case EnemyAttributeType.RangedAttack:
                builder.WithAttribute<RangedAttack>();
                break;
        }
    }

    private void SpawnEnemiesFromPool()
    {
        for (int i = 0; i < _spawningEnemiesAmount; i++)
        {
            (bool finded, EnemyData enemyToSpawn) = GetRandomEnemyFromPool();
            if (!finded) continue;

            Vector2 direction = UnityEngine.Random.insideUnitCircle.normalized;
            float distance = UnityEngine.Random.Range(_minDistanceFromPlayer, _maxDistanceFromPlayer);
            Vector2 spawnPosition = (Vector2)_playerController.transform.position + direction * distance;

            print("Spawned enemy: " + enemyToSpawn.name);

            SpawnEnemy(enemyToSpawn, spawnPosition);
            _enemiesPool.Remove(enemyToSpawn);
        }
    }

    private (bool found, EnemyData enemyData) GetRandomEnemyFromPool()
    {
        if (_enemiesPool.Count == 0) return (false, new EnemyData());

        float totalChance = 0f;

        foreach (EnemyData enemy in _enemiesPool)
        {
            totalChance += Mathf.Max(0f, enemy.spawnChance);
        }

        if (totalChance <= 0f) return (false, new EnemyData());

        float randomValue = UnityEngine.Random.Range(0f, totalChance);

        foreach (EnemyData enemy in _enemiesPool)
        {
            randomValue -= Mathf.Max(0f, enemy.spawnChance);

            if (randomValue <= 0f) return (true, enemy);
        }

        return (true, _enemiesPool[^1]);
    }

    public void AddToEnemyPool(string enemyName)
    {
        foreach (EnemyData enemy in _enemiesData.enemies)
        {
            if (enemy.name == enemyName)
            {
                _enemiesPool.Add(enemy);
                return;
            }
        }
    }

    public void AddToEnemyPool(EnemyData enemyData)
    {
        _enemiesPool.Add(enemyData);
    }

    public void AddToEnemyPool(int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            List<EnemyData> enemies = new();

            foreach (EnemyData enemy in _enemiesData.enemies)
            {
                if (enemy.spawnDay <= dayNumber)
                {
                    enemies.Add(enemy);
                }
            }

            EnemyData newRandomEnemy = enemies[UnityEngine.Random.Range(0, enemies.Count)];
            _enemiesPool.Add(newRandomEnemy);
        }
    }

    private void SetupUI()
    {
        _nightPie.GetComponent<Image>().fillAmount = _nightTimeDuration;
        _dayPie.GetComponent<Image>().fillAmount = (1 - _nightTimeDuration);
        _daysCounter.text = "Day " + dayNumber.ToString();
    }

    private void UpdateUI()
    {
        float rotation = -(_currentTime / _dayTimeDuration) * 360;

        _clockHand.transform.rotation = Quaternion.Euler(0, 0, rotation);
        _daysCounter.text = "Day " + dayNumber.ToString();
    }

    public void SpawnEnemy(EnemyData enemyToSpawn, Vector2 spawnPosition)
    {
        float difficultyModifier = 1f + Mathf.Round(dayNumber / 5) * 0.05f;

        EnemyBuilder builder = new EnemyBuilder().SetBase(enemyToSpawn.prefab);

        foreach(EnemyAttributeType attribute in enemyToSpawn.attributes)
        {
            AddAttributeByType(builder, attribute);
        }

        Enemy enemy = builder.Build(spawnPosition, transform);
        enemy.enemyName = enemyToSpawn.name;

        (bool isWeapon, IWeapon iWeapon, _) = _inventoryController.GetWeapon(enemyToSpawn.weaponItemId);
        if (isWeapon)
        {
            iWeapon.damage *= (int)difficultyModifier;
            enemy.weapon = iWeapon;
        }

        enemy.Health = (int)(enemyToSpawn.health * difficultyModifier);
        enemy.speed = (int)(enemyToSpawn.speed * difficultyModifier);
        enemy.experience = (int)(enemyToSpawn.exp * difficultyModifier);
        enemy.damageModifier = enemyToSpawn.damageModifier * difficultyModifier;
        enemy.money = (int)(UnityEngine.Random.Range(enemyToSpawn.moneyDrop.x, enemyToSpawn.moneyDrop.y) * difficultyModifier);

        if (enemyToSpawn.loot.Count > 0)
        {
            DropOnDestroyController dropCtr = enemy.gameObject.AddComponent<DropOnDestroyController>();
            dropCtr.loot = enemyToSpawn.loot;
        }

        Enemies.Add(enemy);
    }

    public void SkipToNight()
    {
        float nightStartTime = _dayTimeDuration - (_nightTimeDuration * _dayTimeDuration);
        if (_currentTime > nightStartTime) return;

        _currentTime = nightStartTime;
        StartWave();
        UpdateUI();
    }

    public void StartWave()
    {
        AddToEnemyPool(_additionalNewEnemiesPerDay);

        _isWave = true;
        SpawnEnemiesFromPool();

        OnNightStart?.Invoke();
    }

    public void EndWave()
    {
        _isWave = false;
        _currentTime = 0f;
        _playerController.Score += 5 * dayNumber;
        dayNumber++;
        _dayTimeDuration *= 1.05f;
        _nightTimeDuration *= 1.1f;
        _nightTimeDuration = Mathf.Min(_nightTimeDuration, 1f);
        _spawningEnemiesAmount += _additionalNewEnemiesPerDay;
        SetupUI();

        OnDayStart?.Invoke();
    }
}
