using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using ZombieRush.Framework;

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

    [SerializeField] private float _minDistanceFromPlayer = 15f;
    [SerializeField] private float _maxDistanceFromPlayer = 35f;

    [SerializeField] private GameObject _clock;
    [SerializeField] private GameObject _clockHand;
    [SerializeField] private GameObject _nightPie;
    [SerializeField] private GameObject _dayPie;
    [SerializeField] private TextMeshProUGUI _daysCounter;
    [SerializeField] private float _dayTimeDuration = 300f;
    [SerializeField] private float _nightTimeDuration = 0.3f;
    [SerializeField] private int _enemiesPoolMaxSize = 100;

    [SerializeField] private int _spawningEnemiesAmount = 5;
    [SerializeField] private int _startingEnemiesAmount = 10;
    [SerializeField] private int _additionalNewEnemiesPerDay = 2;

    private Dictionary<string, EnemyData> _enemiesDatabase = new();

    private ObjectPool<string, Enemy> _enemiesPool;
    private List<string> _enemiesToSpawnPool = new();

    private float _currentTime = 0;

    public int dayNumber { get; private set; } = 1;
    public Action OnDayStart;
    public Action OnNightStart;

    public bool _isWave { get; private set; } = false;


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
        ScriptableObject[] allEnemies = Resources.LoadAll<ScriptableObject>("Enemies");

        foreach (ScriptableObject asset in allEnemies)
        {
            if (asset is EnemyData enemyData)
            {
                _enemiesDatabase.Add(enemyData.id, enemyData);
            }
        }

        Debug.Log($"Loaded {_enemiesDatabase.Count} enemies.");

        _enemiesPool = new ObjectPool<string, Enemy>(
            createFunc: (id) => CreateEnemy(id),
            actionOnRelease: p => ReleaseEnemyFromPool(p),
            actionOnGet: p => SpawnEnemy(p),
            actionOnDestroy: p => Destroy(p.gameObject),
            defaultCapacity: 50,
            maxSize: _enemiesPoolMaxSize,
            listOfIds : new List<string>
            { 
                "simple_zombie",
                "axe_zombie",
                "speed_zombie"
            }
        );

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
        if (_currentTime >= _dayTimeDuration * 0.95) return;
        SpawnEnemiesFromPool();
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

    public void AddToEnemyPool(int amount)
    {
        List<EnemyData> availableEnemies = new List<EnemyData>();

        foreach (EnemyData enemy in _enemiesDatabase.Values)
        {
            if (enemy.spawnDay <= dayNumber && enemy.spawnChance > 0)
                availableEnemies.Add(enemy);
        }

        if (availableEnemies.Count == 0)
            return;

        float totalChance = 0;

        foreach (EnemyData enemy in availableEnemies)
            totalChance += enemy.spawnChance;

        for (int i = 0; i < amount; i++)
        {
            float randomValue = UnityEngine.Random.Range(0f, totalChance);

            foreach (EnemyData enemy in availableEnemies)
            {
                randomValue -= enemy.spawnChance;

                if (randomValue <= 0)
                {
                    _enemiesToSpawnPool.Add(enemy.id);
                    break;
                }
            }
        }
    }

    public void ReleaseEnemy(Enemy enemy) => _enemiesPool.Release(enemy.enemyId, enemy);

    private void ReleaseEnemyFromPool(Enemy enemy)
    {
        enemy.gameObject.SetActive(false);
        Enemies.Remove(enemy);
    }

    private Enemy CreateEnemy(string id)
    {
        (bool result, EnemyData enemyToSpawn) = GetEnemyData(id);
        if (!result) return null;

        EnemyBuilder builder = new EnemyBuilder().SetBase(enemyToSpawn.prefab);

        foreach (EnemyAttributeType attribute in enemyToSpawn.attributes)
        {
            AddAttributeByType(builder, attribute);
        }

        Enemy enemy = builder.Build(new Vector2(0, 0), transform);
        enemy.enemyId = enemyToSpawn.id;

        if(enemyToSpawn.weapon != null)
        {
            if(enemyToSpawn.weapon is RangeWeaponData rangeWeaponData)
            {
                enemy.weapon = rangeWeaponData.weapon;
            }
            else if (enemyToSpawn.weapon is MeleeWeaponData meleeWeaponData)
            {
                enemy.weapon = meleeWeaponData.weapon;
            }
        }

        //if (enemyToSpawn.loot.Count > 0)
        //{
        //    DropOnDestroyController dropCtr = enemy.gameObject.AddComponent<DropOnDestroyController>();
        //    dropCtr.loot = enemyToSpawn.loot;
        //}

        enemy.gameObject.SetActive(false);

        return enemy;
    }

    private (bool result, EnemyData data) GetEnemyData(string id)
    {
        if (!_enemiesDatabase.ContainsKey(id)) return (false, null);

        return (true, _enemiesDatabase[id]);
    }

    private void SpawnEnemiesFromPool()
    {
        int toSpawn = Mathf.Min(_spawningEnemiesAmount, _enemiesToSpawnPool.Count);

        for (int i = 0; i < toSpawn; i++)
        {
            float totalChance = 0f;

            foreach (string id in _enemiesToSpawnPool)
            {
                var (ok, data) = GetEnemyData(id);
                if (ok) totalChance += Mathf.Max(0f, data.spawnChance);
            }

            if (totalChance <= 0f) break;

            float randomValue = UnityEngine.Random.Range(0f, totalChance);

            for (int j = 0; j < _enemiesToSpawnPool.Count; j++)
            {
                var (ok, data) = GetEnemyData(_enemiesToSpawnPool[j]);
                if (!ok) continue;

                randomValue -= Mathf.Max(0f, data.spawnChance);

                if (randomValue <= 0f)
                {
                    string id = _enemiesToSpawnPool[j];
                    _enemiesToSpawnPool.RemoveAt(j);
                    _enemiesPool.Get(id);
                    break;
                }
            }
        }
    }

    private void SpawnEnemy(Enemy enemy)
    {
        EnemyData data = GetEnemyData(enemy.enemyId).data;

        enemy.Initialize(data.startingModifiers);

        int difficultyLevel = dayNumber / 5;

        for (int i = 0; i < difficultyLevel; i++)
        {
            enemy.modifiersController.AddModifier(data.modifierPerDifficultyIncrease);
        }

        enemy.Health = enemy.MaxHealth;

        Vector2 direction = UnityEngine.Random.insideUnitCircle;
        if (direction.sqrMagnitude < 0.0001f) direction = Vector2.right;
        direction.Normalize();

        float distance = UnityEngine.Random.Range(_minDistanceFromPlayer, _maxDistanceFromPlayer);
        Vector2 spawnPosition = (Vector2)_playerController.transform.position + direction * distance;

        enemy.transform.position = spawnPosition;
        enemy.gameObject.SetActive(true);

        Enemies.Add(enemy);
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
}
