using Cysharp.Threading.Tasks;
using SDClub.Core;
using UnityEngine;

namespace SDClub.Hotfix
{
    [EntitySystem]
    public class ConfigAwakeSystem : AwakeSystem<ConfigComponent>
    {
        protected override void Awake(ConfigComponent self)
        {
        }
    }

    public static class ConfigHelper
    {
        public static async UniTask<T> LoadConfigAsync<T>(this ConfigComponent self, string configName) where T : class
        {
            var request = Resources.LoadAsync<TextAsset>($"Configs/{configName}");
            if (request == null)
            {
                Log.Error($"ConfigSystem: config '{configName}' not found.");
                return null;
            }

            await request;

            var asset = request.asset as TextAsset;
            if (asset == null)
            {
                Log.Error($"ConfigSystem: failed to load config '{configName}'.");
                return null;
            }

            T result = JsonUtility.FromJson<T>(asset.text);
            self.Configs[configName] = result;
            return result;
        }

        public static T GetConfig<T>(this ConfigComponent self, string configName) where T : class
        {
            if (self.Configs.TryGetValue(configName, out var obj) && obj is T result)
            {
                return result;
            }

            return null;
        }

        public static async UniTask LoadAndStoreAsync(this ConfigComponent self, string configName)
        {
            var request = Resources.LoadAsync<TextAsset>($"Configs/{configName}");
            if (request == null)
            {
                Log.Error($"ConfigSystem: config '{configName}' not found.");
                return;
            }

            await request;

            var asset = request.asset as TextAsset;
            if (asset != null)
            {
                self.Configs[configName] = asset.text;
            }
        }
    }
}
