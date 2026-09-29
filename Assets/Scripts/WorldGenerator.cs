using System.Collections.Generic;
using UnityEngine;

public class WorldGenerator : MonoBehaviour
{
    public static WorldGenerator Instance { get; private set; }

    private PlayerController playerController;

    [SerializeField] public float RenderDistance = 10f;
    [SerializeField] public float UnloadDistance = 100f;

    [SerializeField] private GameObject debugTile;
    [SerializeField] private float tileSize = 0.2f;

    [SerializeField] private Transform worldBackground;
    [SerializeField] private Transform worldObjects;

    [SerializeField] private MapBiomesData biomesData;
    [SerializeField] private MapObjectsData objectsData;

    [Header("Generation")]
    [SerializeField] private int seed = 0;

    Dictionary<Vector2Int, string> map = new Dictionary<Vector2Int, string>();
    Dictionary<Vector2Int, GameObject> mapTiles = new Dictionary<Vector2Int, GameObject>();
    Dictionary<Vector2Int, GameObject> mapObjectInstances = new Dictionary<Vector2Int, GameObject>();

    Dictionary<string, Vector2> biomeNoiseOffsets = new Dictionary<string, Vector2>();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        playerController = PlayerController.Instance;

        InitBiomeOffsets();
    }

    void Start()
    {
        InvokeRepeating(nameof(UpdateMap), 0f, 1f);
    }
    private void InitBiomeOffsets()
    {
        int actualSeed = seed == 0 ? Random.Range(int.MinValue, int.MaxValue) : seed;
        System.Random rng = new System.Random(actualSeed);

        biomeNoiseOffsets.Clear();

        if (biomesData == null || biomesData.mapBiomes == null)
            return;

        foreach (Biom biom in biomesData.mapBiomes)
        {
            biomeNoiseOffsets[biom.id] = new Vector2(
                rng.Next(-100000, 100000),
                rng.Next(-100000, 100000)
            );
        }
    }
    private (bool finded, Biom biom) GetBiomeAt(Vector2Int position)
    {
        if (map.ContainsKey(position)) return (false, new Biom());

        Biom result = ComputeBiomeAt(position.x, position.y);
        map[position] = result.id;
        return (true, result);
    }

    private Biom ComputeBiomeAt(int x, int y)
    {
        Biom best = default;
        float bestValue = float.MinValue;
        bool found = false;

        foreach (Biom biom in biomesData.mapBiomes)
        {
            float size = Mathf.Max(biom.size, 0.0001f);

            Vector2 offset = biomeNoiseOffsets.TryGetValue(biom.id, out Vector2 off)
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
        if (mapTiles.ContainsKey(position))
            return;

        var result = GetBiomeAt(position);
        if (!result.finded) return;

        Biom biom = result.biom;

        GameObject tile = Instantiate(
            debugTile,
            new Vector2(position.x * tileSize, position.y * tileSize),
            Quaternion.identity,
            worldBackground
        );
        tile.name = $"tile_{position.x}_{position.y}_{biom.id}";

        SpriteRenderer sr = tile.GetComponent<SpriteRenderer>();
        sr.size = new Vector2(tileSize, tileSize);
        if (biom.tileSprite != null)
        {
            sr.sprite = biom.tileSprite;
        }

        mapTiles[position] = tile;

        TrySpawnObjects(position, biom);
    }

    private void TrySpawnObjects(Vector2Int position, Biom biom)
    {
        if (objectsData == null || objectsData.mapObjects == null)
            return;

        if (mapObjectInstances.ContainsKey(position))
            return;

        Vector2 tileWorldPos = new Vector2(position.x * tileSize, position.y * tileSize);

        foreach (MapObject mapObject in objectsData.mapObjects)
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
                Random.Range(-tileSize / 2f, tileSize / 2f),
                Random.Range(-tileSize / 2f, tileSize / 2f)
            );

            GameObject instance = Instantiate(
                mapObject.gameObject,
                tileWorldPos + jitter,
                Quaternion.identity,
                worldObjects
            );

            mapObjectInstances[position] = instance;
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

    private void UpdateMap()
    {
        Vector2 playerPosition = playerController.transform.position;

        List<Vector2Int> tilesToRemove = new List<Vector2Int>();
        List<Vector2Int> objectsToRemove = new List<Vector2Int>();

        foreach ((Vector2Int index, GameObject tile) in mapTiles)
        {
            float distance = Vector2.Distance(playerPosition, (Vector2)tile.transform.position);

            if (mapObjectInstances.ContainsKey(index))
            {
                GameObject mapObject = mapObjectInstances[index];

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
            mapTiles.Remove(index);
        }

        foreach (Vector2Int index in objectsToRemove)
        {
            mapObjectInstances.Remove(index);
        }

        int minY = Mathf.FloorToInt((playerPosition.y - RenderDistance) / tileSize);
        int maxY = Mathf.FloorToInt((playerPosition.y + RenderDistance) / tileSize);
        int minX = Mathf.FloorToInt((playerPosition.x - RenderDistance) / tileSize);
        int maxX = Mathf.FloorToInt((playerPosition.x + RenderDistance) / tileSize);

        for (int y = minY; y < maxY; y++)
        {
            for (int x = minX; x < maxX; x++)
            {
                Vector2Int position = new Vector2Int(x, y);

                float distance = Vector2.Distance(playerPosition, new Vector2(x * tileSize, y * tileSize));
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

        foreach((Vector2Int index, GameObject mapObject) in mapObjectInstances)
        {
            if(mapObject.transform.position == toRemove.transform.position)
            {
                mapObjectInstances.Remove(index);
                return true;
            }
        }

        return false;
    }
}