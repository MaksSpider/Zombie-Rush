using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEditor.ShaderGraph.Internal.KeywordDependentCollection;

public class WorldGenerator : MonoBehaviour
{
    public static WorldGenerator Instance { get; private set; }

    private PlayerController _playerController;

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

    private Dictionary<Vector2Int, string> _mapBiomes = new Dictionary<Vector2Int, string>();
    private Dictionary<Vector2Int, GameObject> _mapTiles = new Dictionary<Vector2Int, GameObject>();
    private Dictionary<Vector2, GameObject> _mapObjectInstances = new Dictionary<Vector2, GameObject>();

    private Dictionary<string, Vector2> _biomeNoiseOffsets = new Dictionary<string, Vector2>();

    private System.Random _rng;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        _playerController = PlayerController.Instance;

        _rng = new System.Random(seed);

        Init();
    }

    void Start()
    {
        UpdateMap();
    }

    public void Tick1s()
    {
        UpdateMap();
    }

    private void Init()
    {
        _rng = new System.Random(seed);

        _biomeNoiseOffsets.Clear();

        if (_biomesData == null || _biomesData.mapBiomes == null) return;

        foreach (Biom biom in _biomesData.mapBiomes)
        {
            _biomeNoiseOffsets[biom.id] = new Vector2(
                _rng.Next(-100000, 100000),
                _rng.Next(-100000, 100000)
            );
        }
    }

    private float RandomFloat(float min, float max)
    {
        return (float)(_rng.NextDouble() * (max - min) + min);
    }

    private (bool exit, Biom biom) GetBiom(string id)
    {
        foreach(Biom biom in _biomesData.mapBiomes)
        {
            if(biom.id == id) return (true, biom);
        }

        Debug.LogError("Biom doesn't exist: " + id);
        return (false, _biomesData.mapBiomes[0]);
    }

    private Biom GetBiomAt(Vector2Int position)
    {
        if (_mapBiomes.ContainsKey(position)) return GetBiom(_mapBiomes[position]).biom;

        Biom result = ComputeBiomAt(position.x, position.y);
        _mapBiomes[position] = result.id;
        return result;
    }

    private Biom ComputeBiomAt(int x, int y)
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
        Biom biom = GetBiomAt(position);

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

            if (mapObject.frequency <= 0f)
                continue;

            if (!ShouldSpawn(position.x, position.y, mapObject.biom, mapObject.frequency))
                continue;

            Vector2 jitter = new Vector2(
                RandomFloat(-_tileSize / 2f, _tileSize / 2f),
                RandomFloat(-_tileSize / 2f, _tileSize / 2f)
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
        List<Vector2> objectsToRemove = new List<Vector2>();

        foreach ((Vector2Int index, GameObject tile) in _mapTiles)
        {
            float distance = Vector2.Distance(playerPosition, (Vector2)tile.transform.position);

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

        foreach ((Vector2 position, GameObject mapObject) in _mapObjectInstances)
        {
            float distance = Vector2.Distance(playerPosition, (Vector2)mapObject.transform.position);

            if (RenderDistance < distance)
            {
                mapObject.SetActive(false);

                if (UnloadDistance < distance)
                {
                    Destroy(mapObject);
                    objectsToRemove.Add(position);
                }
            }
            else
            {
                mapObject.SetActive(true);
            }
        }

        foreach (Vector2 index in objectsToRemove)
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
                if (distance > RenderDistance) continue;

                if (_mapTiles.ContainsKey(position)) continue;
                CreateTile(position);
            }
        }
    }

    public bool RemoveDestroyedMapObject(GameObject toRemove)
    {
        if (toRemove == null) return false;

        foreach((Vector2 index, GameObject mapObject) in _mapObjectInstances)
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