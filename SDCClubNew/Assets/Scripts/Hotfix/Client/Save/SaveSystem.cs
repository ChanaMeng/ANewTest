using SDClub.Core;
using UnityEngine;

namespace SDClub.Hotfix
{
    [EntitySystem]
    public class SaveAwakeSystem : AwakeSystem<SaveComponent>
    {
        protected override void Awake(SaveComponent self)
        {
        }
    }

    public static class SaveHelper
    {
        public static void Save<T>(this SaveComponent self, string key, T value)
        {
            self.SaveData[key] = value;
            string json = JsonUtility.ToJson(new SaveWrapper<T> { Data = value });
            PlayerPrefs.SetString(key, json);
            PlayerPrefs.Save();
        }

        public static T Load<T>(this SaveComponent self, string key, T defaultValue = default)
        {
            if (self.SaveData.TryGetValue(key, out var obj) && obj is T cached)
            {
                return cached;
            }

            if (!PlayerPrefs.HasKey(key))
            {
                return defaultValue;
            }

            string json = PlayerPrefs.GetString(key);
            if (!string.IsNullOrEmpty(json))
            {
                var wrapper = JsonUtility.FromJson<SaveWrapper<T>>(json);
                self.SaveData[key] = wrapper.Data;
                return wrapper.Data;
            }

            return defaultValue;
        }

        public static void Delete(this SaveComponent self, string key)
        {
            self.SaveData.Remove(key);
            PlayerPrefs.DeleteKey(key);
            PlayerPrefs.Save();
        }

        public static bool HasKey(this SaveComponent self, string key)
        {
            return self.SaveData.ContainsKey(key) || PlayerPrefs.HasKey(key);
        }

        public static void ClearAll(this SaveComponent self)
        {
            self.SaveData.Clear();
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
        }
    }

    [System.Serializable]
    internal struct SaveWrapper<T>
    {
        public T Data;
    }
}
