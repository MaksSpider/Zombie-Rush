using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace ZombieRush.Framework
{
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
}