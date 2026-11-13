using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace ZombieRush.Framework
{

    public class ObjectPool<TId, TValue> where TId : notnull
    {
        public Dictionary<TId, List<TValue>> pool { get; private set; }

        private readonly Func<TId, TValue> CreateFunc;
        private readonly Action<TValue> OnGet;
        private readonly Action<TValue> OnRelease;
        private readonly Action<TValue> OnDestroy;

        private readonly int defaultCapacity;
        private readonly int maxSize;

        public ObjectPool(Func<TId, TValue> createFunc, List<TId> listOfIds = null, Action<TValue> actionOnRelease = null, 
            Action<TValue> actionOnGet = null, Action<TValue> actionOnDestroy = null, int defaultCapacity = 1, int maxSize = 100)
        {
            if (createFunc == null) throw new ArgumentNullException(nameof(createFunc));
            if (defaultCapacity < 0) throw new ArgumentOutOfRangeException(nameof(defaultCapacity));
            if (maxSize <= 0) throw new ArgumentOutOfRangeException(nameof(maxSize));

            CreateFunc = createFunc;
            OnGet = actionOnGet;
            OnRelease = actionOnRelease;
            OnDestroy = actionOnDestroy;

            this.defaultCapacity = Math.Min(defaultCapacity, maxSize);
            this.maxSize = maxSize;

            pool = new Dictionary<TId, List<TValue>>();

            if (listOfIds == null) return;

            foreach (TId id in listOfIds)
            {
                if (pool.ContainsKey(id)) continue;

                List<TValue> list = new List<TValue>(this.defaultCapacity);

                for (int i = 0; i < this.defaultCapacity; i++)
                    list.Add(CreateFunc(id));

                pool.Add(id, list);
            }
        }

        public TValue Get(TId id)
        {
            if (!pool.TryGetValue(id, out List<TValue> list))
            {
                list = new List<TValue>(defaultCapacity);
                pool.Add(id, list);
            }

            TValue value;

            if (list.Count > 0)
            {
                int index = list.Count - 1;
                value = list[index];
                list.RemoveAt(index);
            }
            else
            {
                value = CreateFunc(id);
            }

            OnGet?.Invoke(value);

            return value;
        }

        public void Release(TId id, TValue value)
        {
            if (!pool.TryGetValue(id, out List<TValue> list))
            {
                list = new List<TValue>();
                pool.Add(id, list);
            }

            if (list.Count >= maxSize)
            {
                OnDestroy?.Invoke(value);
                return;
            }

            OnRelease?.Invoke(value);
            list.Add(value);
        }
    }

    public static class JsonFile
    {
        private static string SavePath => Application.persistentDataPath;

        public static void Save<T>(string fileName, string path, T data)
        {
            string directoryPath = Path.Combine(SavePath, path);

            Directory.CreateDirectory(directoryPath);

            string json = JsonConvert.SerializeObject(data, Formatting.Indented);

            string fullPath = Path.Combine(directoryPath, fileName + ".json");

            File.WriteAllText(fullPath, json);
        }

        public static T Load<T>(string fileName, string path = "")
        {
            string fullPath = Path.Combine(SavePath, path, fileName + ".json");

            if (!File.Exists(fullPath))
            {
                Debug.LogWarning($"File doesn't exist: {fullPath}");
                return default;
            }

            string json = File.ReadAllText(fullPath);

            return JsonConvert.DeserializeObject<T>(json);
        }

        public static List<T> LoadAll<T>(string path)
        {
            string fullPath = Path.Combine(SavePath, path);

            if (!Directory.Exists(fullPath))
            {
                Debug.LogWarning($"Directory doesn't exist: {fullPath}");
                return new List<T>();
            }

            string[] files = Directory.GetFiles(fullPath, "*.json");

            List<T> dataList = new();

            foreach (string file in files)
            {
                string json = File.ReadAllText(file);

                T data = JsonConvert.DeserializeObject<T>(json);

                if (data != null)
                {
                    dataList.Add(data);
                }
            }

            return dataList;
        }
    }

    public static class RarityData
    {
        public static Color GetRarityColor(Rarity rarity)
        {
            switch (rarity)
            {
                case Rarity.Common:
                    return Color.white;
                case Rarity.Rare:
                    return new Color(0.25f, 0.55f, 1.0f);
                case Rarity.Epic:
                    return new Color(0.65f, 0.3f, 1.0f);
                case Rarity.Legendary:
                    return new Color(1.0f, 0.65f, 0.1f);
            }

            return GetDefaultRarityColor();
        }

        public static Color GetDefaultRarityColor()
        {
            return Color.white;
        }
    }
}