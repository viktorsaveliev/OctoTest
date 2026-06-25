using System;
using System.IO;
using UnityEngine;

namespace OctoTest.SaveSystem
{
    public class SaveLoadService
    {
        private string GetPath(string key) => Path.Combine(Application.persistentDataPath, $"{key}.json");

        public void Save<T>(string key, T data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            string json = JsonUtility.ToJson(data, prettyPrint: true);
            string path = GetPath(key);
            string tmp = path + ".tmp";

            try
            {
                File.WriteAllText(tmp, json);
                File.Move(tmp, path);
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveLoadService] Save failed for '{key}': { e.Message}");
                throw;
            }
        }

        public T Load<T>(string key, T defaultValue = default)
        {
            string path = GetPath(key);

            if (!File.Exists(path))
            {
                Debug.Log($"[SaveLoadService] No save for '{key}', returning default.");
                return defaultValue;
            }

            try
            {
                string json = File.ReadAllText(path);
                return JsonUtility.FromJson<T>(json);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[SaveLoadService] Corrupt save for '{key}': {e.Message}. Returning default.");
                return defaultValue;
            }
        }

        public bool HasSave(string key) => File.Exists(GetPath(key));

        public void Delete(string key)
        {
            string path = GetPath(key);

            if (File.Exists(path)) 
            {
                File.Delete(path);
            }
        }
    }
}