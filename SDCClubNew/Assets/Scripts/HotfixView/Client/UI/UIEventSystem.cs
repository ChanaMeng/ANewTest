using SDClub.Core;
using SDClub.Model;
using SDClub.UIFrameWork;
using Cysharp.Threading.Tasks;

namespace SDClub.HotfixView
{
    // 初始化 UI 框架 - 响应 EntryEvent3 创建 GameUIComponent
    [Event(SceneType.Main)]
    public class InitUIEvent : AEvent<Scene, EntryEvent3>
    {
        protected override async UniTask Run(Scene scene, EntryEvent3 a)
        {
            Log.Debug("HotfixView: Init UIFramework");

            // 创建 UI 管理器
            scene.AddComponent<GameUIComponent>();

            // 创建 GameObject 对象池
            scene.AddComponent<GameObjectPoolComponent>();

            // 创建音频组件
            var audio = scene.AddComponent<AudioComponent>();
            await UniTask.CompletedTask;
        }
    }
}
