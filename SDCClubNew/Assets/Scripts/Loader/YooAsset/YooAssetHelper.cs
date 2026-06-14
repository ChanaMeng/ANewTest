// TODO: 需要安装 YooAsset 3.0 package (通过 Unity Package Manager)
// 安装后取消下方注释以启用 YooAsset 集成
// #define YOOASSET

using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SDClub.Loader
{
    /// <summary>
    /// YooAsset 资源管理辅助类
    /// 注意: 需要先通过 Unity Package Manager 安装 YooAsset 3.0
    /// 安装后取消 #define YOOASSET 启用功能
    /// </summary>
    public static class YooAssetHelper
    {
#if YOOASSET
        private static ResourcePackage defaultPackage;

        public static async UniTask InitializeAsync()
        {
            // 初始化 YooAssets
            YooAssets.Initialize();

            // 创建默认资源包
            defaultPackage = YooAssets.CreatePackage("DefaultPackage");

            // 根据平台选择初始化参数
#if UNITY_EDITOR
            // 编辑器模式：使用模拟资源
            var parameters = new EditorSimulateInitializeParameters();
            parameters.SimulateManifestFilePath = EditorSimulateModeHelper.SimulateBuild("DefaultPackage");
#else
            // 真机模式：离线模式或热更模式
            // 离线模式
            var parameters = new OfflinePlayInitializeParameters();
            // 热更模式 (需要时可切换)
            // var parameters = new HostPlayModeInitializeParameters();
#endif

            var initOperation = defaultPackage.InitializeAsync(parameters);
            await initOperation;

            if (initOperation.Status == EOperationStatus.Succeed)
            {
                SDClub.Core.Log.Debug("YooAsset 初始化成功");
            }
            else
            {
                SDClub.Core.Log.Error($"YooAsset 初始化失败: {initOperation.Error}");
            }
        }

        public static async UniTask<T> LoadAssetAsync<T>(string path) where T : Object
        {
            if (defaultPackage == null)
            {
                SDClub.Core.Log.Error("YooAsset 未初始化");
                return null;
            }

            var handle = defaultPackage.LoadAssetAsync<T>(path);
            await handle;
            return handle.AssetObject as T;
        }

        public static async UniTask<Scene> LoadSceneAsync(string path, LoadSceneMode mode = LoadSceneMode.Single)
        {
            if (defaultPackage == null)
            {
                SDClub.Core.Log.Error("YooAsset 未初始化");
                return default;
            }

            var handle = defaultPackage.LoadSceneAsync(path, mode);
            await handle;
            return handle.SceneObject;
        }

        public static T LoadAssetSync<T>(string path) where T : Object
        {
            if (defaultPackage == null)
            {
                SDClub.Core.Log.Error("YooAsset 未初始化");
                return null;
            }

            var handle = defaultPackage.LoadAssetSync<T>(path);
            return handle.AssetObject as T;
        }
#else
        /// <summary>
        /// YooAsset 未安装时的空实现
        /// </summary>
        public static async UniTask InitializeAsync()
        {
            SDClub.Core.Log.Debug("YooAsset 未安装，跳过初始化。请通过 Unity Package Manager 安装 YooAsset 3.0");
            await UniTask.CompletedTask;
        }

        public static async UniTask<T> LoadAssetAsync<T>(string path) where T : Object
        {
            SDClub.Core.Log.Error("YooAsset 未安装，无法加载资源: " + path);
            await UniTask.CompletedTask;
            return null;
        }

        public static async UniTask<Scene> LoadSceneAsync(string path)
        {
            SDClub.Core.Log.Error("YooAsset 未安装，无法加载场景: " + path);
            await UniTask.CompletedTask;
            return default;
        }

        public static T LoadAssetSync<T>(string path) where T : Object
        {
            SDClub.Core.Log.Error("YooAsset 未安装，无法加载资源: " + path);
            return null;
        }
#endif
    }
}
