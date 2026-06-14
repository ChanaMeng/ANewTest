using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using YooAsset;

namespace SDClub.Loader
{
    /// <summary>
    /// YooAsset 资源管理组件，负责资源包的初始化和生命周期管理
    /// </summary>
    public class YooAssetComponent : SDClub.Core.Singleton<YooAssetComponent>, SDClub.Core.ISingletonAwake
    {
        private readonly Dictionary<string, ResourcePackage> packages = new();

        public void Awake()
        {
        }

        /// <summary>
        /// 初始化 YooAsset 引擎并创建所有资源包
        /// </summary>
        public async UniTask InitializeAsync()
        {
            // 初始化 YooAssets 引擎
            YooAssets.Initialize();

            // 创建默认资源包
            await CreatePackageAsync(YooAssetConfig.DefaultPackage);

            // 创建 UI 资源包
            await CreatePackageAsync(YooAssetConfig.UIPackage);

            // 创建配置资源包
            await CreatePackageAsync(YooAssetConfig.ConfigPackage);

            SDClub.Core.Log.Debug("YooAssetComponent 初始化完成");
        }

        private async UniTask CreatePackageAsync(string packageName)
        {
            var package = YooAssets.CreatePackage(packageName);
            packages[packageName] = package;

            // 根据平台选择初始化选项
            var options = CreateInitializeOptions(packageName);

            var initOperation = package.InitializePackageAsync(options);
            await initOperation;

            if (initOperation.Status == EOperationStatus.Succeeded)
            {
                SDClub.Core.Log.Debug($"资源包 [{packageName}] 初始化成功");
            }
            else
            {
                SDClub.Core.Log.Error($"资源包 [{packageName}] 初始化失败: {initOperation.Error}");
            }
        }

        private static InitializePackageOptions CreateInitializeOptions(string packageName)
        {
#if UNITY_EDITOR
            // 编辑器模式：使用模拟资源
            var buildResult = EditorSimulateBuildInvoker.Build(packageName, (int)EBundleType.VirtualAssetBundle);
            var fileSystemParams = FileSystemParameters.CreateDefaultEditorFileSystemParameters(buildResult.PackageRootDirectory);

            var options = new EditorSimulateModeOptions();
            options.EditorFileSystemParameters = fileSystemParams;
            return options;
#elif WEIXINMINIGAME
            // 微信小游戏：使用 WebPlayMode
            var remoteService = new RemoteService(YooAssetConfig.GetPackageUrl(packageName));
            var webServerParams = FileSystemParameters.CreateDefaultWebServerFileSystemParameters();
            var webNetworkParams = FileSystemParameters.CreateDefaultWebNetworkFileSystemParameters(remoteService);

            var options = new WebPlayModeOptions();
            options.WebServerFileSystemParameters = webServerParams;
            options.WebNetworkFileSystemParameters = webNetworkParams;
            return options;
#else
            // 其他真机平台：离线模式
            var fileSystemParams = FileSystemParameters.CreateDefaultBuiltinFileSystemParameters();

            var options = new OfflinePlayModeOptions();
            options.BuiltinFileSystemParameters = fileSystemParams;
            return options;
#endif
        }

        /// <summary>
        /// 获取指定名称的资源包
        /// </summary>
        public ResourcePackage GetPackage(string packageName)
        {
            packages.TryGetValue(packageName, out var package);
            return package;
        }

        /// <summary>
        /// 获取默认资源包
        /// </summary>
        public ResourcePackage GetDefaultPackage()
        {
            return GetPackage(YooAssetConfig.DefaultPackage);
        }
    }

    /// <summary>
    /// YooAsset 远端资源服务实现
    /// </summary>
    internal class RemoteService : IRemoteService
    {
        private readonly string _baseUrl;

        public RemoteService(string baseUrl)
        {
            _baseUrl = baseUrl;
        }

        public IReadOnlyList<string> GetRemoteUrls(string fileName)
        {
            return new[] { $"{_baseUrl}/{fileName}" };
        }
    }
}
