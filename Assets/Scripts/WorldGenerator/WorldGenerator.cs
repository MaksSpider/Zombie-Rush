using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class Chunk
{
    Vector2Int position;

    TileController[] tiles = new TileController[64];
    Dictionary<Vector2Int, GameObject> worldObjects = new();

    private bool loaded = false;

    public Chunk(Vector2Int position, TileController[] tiles, Dictionary<Vector2Int, GameObject> worldObjects)
    {
        this.position = position;
        this.tiles = tiles;
        this.worldObjects = worldObjects;

        loaded = true;
    }

    public void Load()
    {
        if (loaded) return;

        tiles = WorldGenerator.Instance.GenerateTiles(position);

        foreach (GameObject worldObject in worldObjects.Values)
        {
            if (worldObject != null) worldObject.SetActive(true);
        }

        loaded = true;
    }

    public void Unload()
    {
        if (!loaded) return;

        if (tiles != null)
        {
            for (int i = 0; i < tiles.Length; i++)
            {
                if (tiles[i] != null) WorldGenerator.Instance.ReleaseTile(tiles[i]);
            }
        }

        tiles = null;

        foreach (GameObject worldObject in worldObjects.Values)
        {
            if (worldObject != null) worldObject.SetActive(false);
        }

        loaded = false;
    }

    public bool RemoveObject(GameObject toRemove)
    {
        foreach (KeyValuePair<Vector2Int, GameObject> pair in worldObjects)
        {
            if (pair.Value == toRemove)
            {
                worldObjects.Remove(pair.Key);
                return true;
            }
        }

        return false;
    }
}

public class WorldGenerator : MonoBehaviour
{
    public static WorldGenerator Instance { get; private set; }

    private PlayerController _playerController => PlayerController.Instance;
    private GameSession _gameSession => GameSession.Instance;

    [SerializeField] public int RenderDistance = 6;
    [SerializeField] public int UnloadMargin = 2;

    [SerializeField] private TileController _tilePrefab;
    [SerializeField] private int _chunkSize = 8;

    [SerializeField] private Transform _worldBackground;
    [SerializeField] private Transform _worldObjects;

    [SerializeField] private int _tilesPoolCapacity = 2000;
    [SerializeField] private int _tilesPoolMaxSize = 20000;
    [SerializeField] private float _maxTickTime = 0.003f;

    private Dictionary<string, WorldBiom> _biomesDatabase = new();
    private Dictionary<string, List<WorldObject>> _objectsDatabase = new();

    private readonly List<WorldBiom> _biomesSorted = new();

    private float _tileSize => _tilePrefab.transform.localScale.x;

    private ObjectPool<TileController> _tilesPool;

    private Dictionary<Vector2Int, Chunk> _map = new();

    private readonly List<Vector2Int> _toUnloadBuffer = new();

    [NonSerialized] public HashSet<Vector2Int> loadedChunks = new();
    [NonSerialized] public HashSet<Vector2Int> chunksToLoad = new();

    private Dictionary<string, Vector2> _biomeNoiseOffsets = new();

    private System.Random _rng;

    private Vector2Int _lastCenter;
    private int _lastDistance;
    private bool _mapInitialized;

    public Queue<Vector2Int> chunksToLoadQueue { get; private set; } = new();

    public Action OnWorldLoaded;

    public bool isWorldLoaded = false;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        Init();
    }

    public void Tick()
    {
        GenerateMapPerTick();
    }

    public void Tick1s()
    {
        UpdateMap();
    }

    private void Init()
    {
        _tilesPool = new ObjectPool<TileController>(
            createFunc: () => Instantiate(_tilePrefab, _worldBackground),
            actionOnRelease: p => p.gameObject.SetActive(false),
            actionOnDestroy: p => Destroy(p.gameObject),
            collectionCheck: false,
            defaultCapacity: _tilesPoolCapacity,
            maxSize: _tilesPoolMaxSize
        );

        _rng = new System.Random(_gameSession.playerProfile.seed);

        _biomeNoiseOffsets.Clear();

        ScriptableObject[] allBiomes = Resources.LoadAll<ScriptableObject>("World/Biomes");

        foreach (ScriptableObject asset in allBiomes)
        {
            if (asset is WorldBiomData biomData)
            {
                _biomesDatabase.Add(biomData.biom.id, biomData.biom);
            }
        }

        _biomesSorted.Clear();
        _biomesSorted.AddRange(_biomesDatabase.Values);
        _biomesSorted.Sort((a, b) => string.CompareOrdinal(a.id, b.id));

        foreach (WorldBiom biom in _biomesSorted)
        {
            _biomeNoiseOffsets[biom.id] = new Vector2(
                _rng.Next(-100000, 100000),
                _rng.Next(-100000, 100000)
            );
        }

        Debug.Log($"Loaded {_biomesDatabase.Count} biomes.");

        ScriptableObject[] allWorldObjects = Resources.LoadAll<ScriptableObject>("World/WorldObjects");

        int loadedObjects = 0;

        foreach (ScriptableObject asset in allWorldObjects)
        {
            if (asset is WorldObjectsData objectData)
            {
                foreach (WorldObject worldObject in objectData.worldObjects)
                {
                    loadedObjects++;
                    foreach (WorldObjectBioms objectBiome in worldObject.biomes)
                    {
                        WorldObject worldObjectToSave = worldObject;
                        worldObjectToSave.frequency = objectBiome.frequency;

                        if (!_objectsDatabase.TryGetValue(objectBiome.biomId, out var list))
                        {
                            list = new List<WorldObject>();
                            _objectsDatabase[objectBiome.biomId] = list;
                        }

                        list.Add(worldObjectToSave);
                    }
                }
            }
        }

        foreach (List<WorldObject> list in _objectsDatabase.Values)
        {
            list.Sort((a, b) => string.CompareOrdinal(
                a.gameObject != null ? a.gameObject.name : "",
                b.gameObject != null ? b.gameObject.name : ""));
        }

        Debug.Log($"Loaded {loadedObjects} world objects.");
    }

    public bool RemoveDestroyedMapObject(GameObject toRemove)
    {
        if (toRemove == null) return false;

        Vector2Int chunkPos = WorldToChunk(toRemove.transform.position);

        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                if (_map.TryGetValue(chunkPos + new Vector2Int(dx, dy), out Chunk chunk)
                    && chunk.RemoveObject(toRemove))
                {
                    return true;
                }
            }
        }

        return false;
    }

    public void GenerateMap(Vector2Int center, int distance)
    {
        _lastCenter = center;
        _lastDistance = distance;
        _mapInitialized = true;

        for (int ring = 0; ring <= distance; ring++)
        {
            for (int dx = -ring; dx <= ring; dx++)
            {
                for (int dy = -ring; dy <= ring; dy++)
                {
                    if (Mathf.Max(Mathf.Abs(dx), Mathf.Abs(dy)) != ring) continue;

                    Vector2Int position = new Vector2Int(center.x + dx, center.y + dy);

                    if (loadedChunks.Contains(position)) continue;
                    if (!chunksToLoad.Add(position)) continue;

                    chunksToLoadQueue.Enqueue(position);
                }
            }
        }

        int keepDistance = distance + UnloadMargin;

        _toUnloadBuffer.Clear();

        foreach (Vector2Int chunk in loadedChunks)
        {
            int dx = Mathf.Abs(chunk.x - center.x);
            int dy = Mathf.Abs(chunk.y - center.y);

            if (dx > keepDistance || dy > keepDistance)
                _toUnloadBuffer.Add(chunk);
        }

        foreach (Vector2Int chunk in _toUnloadBuffer)
        {
            loadedChunks.Remove(chunk);

            if (_map.TryGetValue(chunk, out Chunk c))
            {
                c.Unload();
            }
        }
    }

    public void UpdateMap()
    {
        if (_playerController == null) return; // POPRAWKA: null-check

        Vector2Int center = WorldToChunk(_playerController.transform.position);

        if (_mapInitialized && center == _lastCenter) return;

        GenerateMap(center, RenderDistance);
    }

    public void ReleaseTile(TileController tileController) => _tilesPool.Release(tileController);

    public Vector2Int WorldToTile(Vector2 worldPosition)
    {
        return new Vector2Int(
            Mathf.RoundToInt(worldPosition.x / _tileSize),
            Mathf.RoundToInt(worldPosition.y / _tileSize)
        );
    }

    public Vector2Int TileToChunk(Vector2Int tile)
    {
        return new Vector2Int(
            Mathf.FloorToInt((float)tile.x / _chunkSize),
            Mathf.FloorToInt((float)tile.y / _chunkSize)
        );
    }

    public Vector2Int WorldToChunk(Vector2 worldPosition) => TileToChunk(WorldToTile(worldPosition));

    public TileController[] GenerateTiles(Vector2Int chunkPosition)
    {
        return GenerateChunkData(chunkPosition, null).tiles;
    }

    public (TileController[] tiles, Dictionary<Vector2Int, GameObject> objects) GenerateChunkData(Vector2Int chunkPosition)
    {
        Dictionary<Vector2Int, GameObject> mapObjects = new();
        TileController[] tiles = GenerateChunkData(chunkPosition, mapObjects).tiles;
        return (tiles, mapObjects);
    }

    private (TileController[] tiles, Dictionary<Vector2Int, GameObject> objects) GenerateChunkData(
        Vector2Int chunkPosition, Dictionary<Vector2Int, GameObject> objectsOrNull)
    {
        TileController[] tiles = new TileController[_chunkSize * _chunkSize];

        int tileIndex = 0;

        for (int x = 0; x < _chunkSize; x++)
        {
            for (int y = 0; y < _chunkSize; y++)
            {
                Vector2Int position = new Vector2Int(chunkPosition.x * _chunkSize + x, chunkPosition.y * _chunkSize + y);
                WorldBiom biom = ComputeBiomAt(position.x, position.y);

                TileController tileCtr = _tilesPool.Get();
#if UNITY_EDITOR
                tileCtr.name = $"tile_{position.x}_{position.y}-{biom.id}";
#endif
                tileCtr.Setup(new Vector2(position.x * _tileSize, position.y * _tileSize), biom.tileSprite);

                tiles[tileIndex] = tileCtr;

                if (objectsOrNull != null)
                {
                    (bool result, GameObject worldObject) = TrySpawnObject(position, biom);

                    if (result)
                    {
                        objectsOrNull[position] = worldObject;
                    }
                }

                tileIndex++;
            }
        }

        return (tiles, objectsOrNull);
    }

    private void GenerateMapPerTick()
    {
        if (!_mapInitialized) UpdateMap();
        if (!_mapInitialized) return;

        float startTime = Time.realtimeSinceStartup;

        while (chunksToLoadQueue.Count > 0)
        {
            if (Time.realtimeSinceStartup - startTime >= _maxTickTime) break;

            Vector2Int position = chunksToLoadQueue.Dequeue();
            chunksToLoad.Remove(position);

            if (!IsInKeepRange(position)) continue;
            if (loadedChunks.Contains(position)) continue;

            if (_map.TryGetValue(position, out Chunk existing))
            {
                existing.Load();
                loadedChunks.Add(position);
                continue;
            }

            (bool result, Chunk chunk) = CreateChunk(position);
            if (!result) continue;

            _map[position] = chunk;
        }

        if (chunksToLoadQueue.Count == 0 && !isWorldLoaded)
        {
            isWorldLoaded = true;
            OnWorldLoaded?.Invoke();
        }
    }

    private bool IsInKeepRange(Vector2Int chunk)
    {
        int keep = _lastDistance + UnloadMargin;

        return Mathf.Abs(chunk.x - _lastCenter.x) <= keep
            && Mathf.Abs(chunk.y - _lastCenter.y) <= keep;
    }

    public (bool result, Chunk chunk) CreateChunk(Vector2Int chunkPosition)
    {
        if (loadedChunks.Contains(chunkPosition)) return (false, null);

        (TileController[] tiles, Dictionary<Vector2Int, GameObject> mapObjects) = GenerateChunkData(chunkPosition);

        loadedChunks.Add(chunkPosition);
        return (true, new Chunk(chunkPosition, tiles, mapObjects));
    }

    private WorldBiom ComputeBiomAt(int x, int y)
    {
        WorldBiom best = default;
        float bestValue = float.MinValue;
        bool found = false;

        for (int i = 0; i < _biomesSorted.Count; i++)
        {
            WorldBiom biom = _biomesSorted[i];

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

    private (bool result, GameObject worldObject) TrySpawnObject(Vector2Int position, WorldBiom biom)
    {
        if (!_objectsDatabase.TryGetValue(biom.id, out List<WorldObject> worldObjects)) return (false, null);

        int salt = GetStableStringHash(biom.id);

        float roll = Hash01(position.x, position.y, salt);
        float accumulated = biom.vegetationDensity;

        if (roll >= accumulated) return (false, null);

        for (int i = 0; i < worldObjects.Count; i++)
        {
            WorldObject worldObject = worldObjects[i];

            if (worldObject.gameObject == null) continue;
            if (worldObject.frequency <= 0f) continue;

            accumulated += worldObject.frequency;

            if (roll >= accumulated) continue;

            float jitterX = (Hash01(position.x, position.y, salt + 1) - 0.5f) * _tileSize;
            float jitterY = (Hash01(position.x, position.y, salt + 2) - 0.5f) * _tileSize;

            Vector3 spawnPosition = new Vector3(
                position.x * _tileSize + jitterX,
                position.y * _tileSize + jitterY,
                0f
            );

            return (true, Instantiate(worldObject.gameObject, spawnPosition, Quaternion.identity, _worldObjects));
        }

        return (false, null);
    }

    private float Hash01(int x, int y, int salt)
    {
        unchecked
        {
            uint h = (uint)_gameSession.playerProfile.seed;

            h = (h ^ (uint)x) * 0x9E3779B1u;
            h ^= h >> 15;
            h = (h ^ (uint)y) * 0x85EBCA6Bu;
            h ^= h >> 13;
            h = (h ^ (uint)salt) * 0xC2B2AE35u;
            h ^= h >> 16;

            return (h & 0xFFFFFF) / (float)0x1000000;
        }
    }

    private int GetStableStringHash(string value)
    {
        unchecked
        {
            int hash = 23;

            for (int i = 0; i < value.Length; i++)
            {
                hash = hash * 31 + value[i];
            }

            return hash;
        }
    }
}