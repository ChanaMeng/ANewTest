using Cysharp.Threading.Tasks;
using UnityEngine;
using YooAsset;
using UObject = UnityEngine.Object;
using UScene = UnityEngine.SceneManagement.Scene;
using USceneMode = UnityEngine.SceneManagement.LoadSceneMode;

namespace SDClub.Loader
{
    /// <summary>
    /// YooAsset 资源加载辅助类，提供简洁的异步/同步资源加载 API
    /// </summary>
    public static class YooAssetHelper
    {
        private static YooAssetComponent component;

        private static YooAssetComponent GetComponent()
        {
            if (component == null)
            {
                component = SDClub.Core.World.Instance.AddSingleton<YooAssetComponent>();
            }

            return component;
        }

        public static async UniTask InitializeAsync()
        {
            var comp = GetComponent();
            await comp.InitializeAsync();
        }

        public static async UniTask<T> LoadAssetAsync<T>(string path) where T : UObject
        {
            var package = GetComponent().GetDefaultPackage();
            if (package == null)
            {
                SDClub.Core.Log.Error("YooAsset DefaultPackage 未初始化");
                return null;
            }

            var handle = package.LoadAssetAsync<T>(path);
            await handle;
            return handle.AssetObject as T;
        }

        public static async UniTask<UScene> LoadSceneAsync(string path, USceneMode mode = USceneMode.Single)
        {
            var package = GetComponent().GetDefaultPackage();
            if (package == null)
            {
                SDClub.Core.Log.Error("YooAsset DefaultPackage 未初始化");
                return default;
            }

            var handle = package.LoadSceneAsync(path, mode);
            await handle;
            return handle.SceneObject;
        }

        public static T LoadAssetSync<T>(string path) where T : UObject
        {
            var package = GetComponent().GetDefaultPackage();
            if (package == null)
            {
                SDClub.Core.Log.Error("YooAsset DefaultPackage 未初始化");
                return null;
            }

            var handle = package.LoadAssetSync<T>(path);
            return handle.AssetObject as T;
        }
    }
}
