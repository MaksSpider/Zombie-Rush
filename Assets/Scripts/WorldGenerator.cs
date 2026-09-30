using System.Collections.Generic;
using UnityEngine;

public class WorldGenerator : MonoBehaviour
{
    public static WorldGenerator Instance { get; private set; }

    private PlayerController _playerController;
    private StatesManager _statesManager;

    [SerializeField] public float RenderDistance = 10f;
    [SerializeField] public float UnloadDistance = 100f;

    [SerializeField] private GameObject _debugTile;
    [SerializeField] private float _tileSize = 0.2f;

    [SerializeField] private Transform _worldBackground;
    [SerializeField] private Transform _worldObjects;

    [SerializeField] private MapBiomesData _biomesData;
    [SerializeField] private MapObjectsData _objectsData;

    [Header("Generation")]
    [SerializeField] public int seed = 0;

    private Dictionary<Vector2Int, string> _map = new Dictionary<Vector2Int, string>();
    private Dictionary<Vector2Int, GameObject> _mapTiles = new Dictionary<Vector2Int, GameObject>();
    private Dictionary<Vector2Int, GameObject> _mapObjectInstances = new Dictionary<Vector2Int, GameObject>();

    private Dictionary<string, Vector2> _biomeNoiseOffsets = new Dictionary<string, Vector2>();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        _playerController = PlayerController.Instance;
        _statesManager = StatesManager.Instance;

        InitBiomeOffsets();
    }

    void Start()
    {
        UpdateMap();
    }

    public void Tick1s()
    {
        UpdateMap();
    }

    private void InitBiomeOffsets()
    {
        int actualSeed = seed == 0 ? Random.Range(int.MinValue, int.MaxValue) : seed;
        System.Random rng = new System.Random(actualSeed);

        _biomeNoiseOffsets.Clear();

        if (_biomesData == null || _biomesData.mapBiomes == null) return;

        foreach (Biom biom in _biomesData.mapBiomes)
        {
            _biomeNoiseOffsets[biom.id] = new Vector2(
                rng.Next(-100000, 100000),
                rng.Next(-100000, 100000)
            );
        }
    }
    private (bool finded, Biom biom) GetBiomeAt(Vector2Int position)
    {
        if (_map.ContainsKey(position)) return (false, new Biom());

        Biom result = ComputeBiomeAt(position.x, position.y);
        _map[position] = result.id;
        return (true, result);
    }

    private Biom ComputeBiomeAt(int x, int y)
    {
        Biom best = default;
        float bestValue = float.MinValue;
        bool found = false;

        foreach (Biom biom in _biomesData.mapBiomes)
        {
            float size = Mathf.Max(biom.size, 0.0001f);

            Vector2 offset = _biomeNoiseOffsets.TryGetValue(biom.id, out Vector2 off)
                ? off
                : Vector2.zero;

            float noise = Mathf.PerlinNoise(
                (x + offset.x) / size,
                (y + offset.y) / size
            );

            float value = noise * Mathf.Max(biom.frequency, 0f);

            if (!found || value > bestValue)
            {
                bestValue = value;
                best = biom;
                found = true;
            }
        }

        return best;
    }

    private void CreateTile(Vector2Int position)
    {
        if (_mapTiles.ContainsKey(position)) return;

        var result = GetBiomeAt(position);
        if (!result.finded) return;

        Biom biom = result.biom;

        GameObject tile = Instantiate(
            _debugTile,
            new Vector2(position.x * _tileSize, position.y * _tileSize),
            Quaternion.identity,
            _worldBackground
        );
        tile.name = $"tile_{position.x}_{position.y}_{biom.id}";

        SpriteRenderer sr = tile.GetComponent<SpriteRenderer>();
        sr.size = new Vector2(_tileSize, _tileSize);
        if (biom.tileSprite != null)
        {
            sr.sprite = biom.tileSprite;
        }

        _mapTiles[position] = tile;

        TrySpawnObjects(position, biom);
    }

    private void TrySpawnObjects(Vector2Int position, Biom biom)
    {
        if (_objectsData == null || _objectsData.mapObjects == null) return;
        if (_mapObjectInstances.ContainsKey(position)) return;

        Vector2 tileWorldPos = new Vector2(position.x * _tileSize, position.y * _tileSize);

        foreach (MapObject mapObject in _objectsData.mapObjects)
        {
            if (mapObject.biom != biom.id)
                continue;

            if (mapObject.gameObject == null)
            {
                Debug.LogWarning($"MapObject for biom '{biom.id}' doesnt have assigned gameobject - skipped.");
                continue;
            }

            float chance = ParseFrequency(mapObject.frequency);
            if (chance <= 0f)
                continue;

            if (!ShouldSpawn(position.x, position.y, mapObject.biom, chance))
                continue;

            Vector2 jitter = new Vector2(
                Random.Range(-_tileSize / 2f, _tileSize / 2f),
                Random.Range(-_tileSize / 2f, _tileSize / 2f)
            );

            GameObject instance = Instantiate(
                mapObject.gameObject,
                tileWorldPos + jitter,
                Quaternion.identity,
                _worldObjects
            );

            _mapObjectInstances[position] = instance;
            break;
        }
    }

    private float ParseFrequency(string frequency)
    {
        if (string.IsNullOrWhiteSpace(frequency))
            return 0f;

        frequency = frequency.Trim();
        bool isPercent = frequency.EndsWith("%");
        if (isPercent)
            frequency = frequency.Substring(0, frequency.Length - 1);

        if (!float.TryParse(
                frequency,
                System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture,
                out float value))
        {
            return 0f;
        }

        return isPercent ? value / 100f : value;
    }
    private bool ShouldSpawn(int x, int y, string salt, float chance)
    {
        unchecked
        {
            int hash = x * 73856093 ^ y * 19349663 ^ salt.GetHashCode() ^ seed;
            System.Random localRng = new System.Random(hash);
            return localRng.NextDouble() < chance;
        }
    }

    public void UpdateMap()
    {
        Vector2 playerPosition = _playerController.transform.position;

        List<Vector2Int> tilesToRemove = new List<Vector2Int>();
        List<Vector2Int> objectsToRemove = new List<Vector2Int>();

        foreach ((Vector2Int index, GameObject tile) in _mapTiles)
        {
            float distance = Vector2.Distance(playerPosition, (Vector2)tile.transform.position);

            if (_mapObjectInstances.ContainsKey(index))
            {
                GameObject mapObject = _mapObjectInstances[index];

                if (RenderDistance < distance)
                {
                    mapObject.SetActive(false);

                    if (UnloadDistance < distance)
                    {
                        Destroy(mapObject);
                        objectsToRemove.Add(index);
                    }
                }
                else
                {
                    mapObject.SetActive(true);
                }
            }

            if (RenderDistance < distance)
            {
                tile.SetActive(false);

                if (UnloadDistance < distance)
                {
                    Destroy(tile);
                    tilesToRemove.Add(index);
                }
                continue;
            }

            tile.SetActive(true);
        }

        foreach (Vector2Int index in tilesToRemove)
        {
            _mapTiles.Remove(index);
        }

        foreach (Vector2Int index in objectsToRemove)
        {
            _mapObjectInstances.Remove(index);
        }

        int minY = Mathf.FloorToInt((playerPosition.y - RenderDistance) / _tileSize);
        int maxY = Mathf.FloorToInt((playerPosition.y + RenderDistance) / _tileSize);
        int minX = Mathf.FloorToInt((playerPosition.x - RenderDistance) / _tileSize);
        int maxX = Mathf.FloorToInt((playerPosition.x + RenderDistance) / _tileSize);

        for (int y = minY; y < maxY; y++)
        {
            for (int x = minX; x < maxX; x++)
            {
                Vector2Int position = new Vector2Int(x, y);

                float distance = Vector2.Distance(playerPosition, new Vector2(x * _tileSize, y * _tileSize));
                if (distance > RenderDistance)
                    continue;

                CreateTile(position);
            }
        }
    }

    public bool RemoveObject(GameObject toRemove)
    {
        if (toRemove == null)
            return false;

        foreach((Vector2Int index, GameObject mapObject) in _mapObjectInstances)
        {
            if(mapObject.transform.position == toRemove.transform.position)
            {
                _mapObjectInstances.Remove(index);
                return true;
            }
        }

        return false;
    }
}