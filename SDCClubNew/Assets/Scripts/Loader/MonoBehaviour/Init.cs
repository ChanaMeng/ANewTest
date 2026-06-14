using SDClub.Core;
using UnityEngine;

namespace SDClub.Loader
{
    /// <summary>
    /// Unity 启动入口，挂在场景 GameObject 上
    /// </summary>
    public class Init : MonoBehaviour
    {
        private void Start()
        {
            StartAsync();
        }

        private async void StartAsync()
        {
            // 1. 创建核心基础设施单例
            World.Instance.AddSingleton<Options>();
            World.Instance.AddSingleton<UnityLogger>();
            World.Instance.AddSingleton<TimeInfo>();

            // FiberManager
            World.Instance.AddSingleton<FiberManager>();

            // 2. 初始化 YooAsset 资源管理系统
            await YooAssetHelper.InitializeAsync();

            // 3. 启动代码加载（内部创建 CodeTypes → [Code] → Entry）
            CodeLoader.Instance.Start();
        }

        private void Update()
        {
            TimeInfo.Instance?.Update();
            FiberManager.Instance?.Update();
        }

        private void LateUpdate()
        {
            FiberManager.Instance?.LateUpdate();
        }
    }
}
