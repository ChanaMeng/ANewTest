using Cysharp.Threading.Tasks;
using SDClub.Core;
using SDClub.Model;

namespace SDClub.Hotfix
{
    [Event(SceneType.Main)]
    public class EntryEvent2_InitManagers : AEvent<Scene, EntryEvent2>
    {
        protected override async UniTask Run(Scene scene, EntryEvent2 a)
        {
            // 初始化管理器单例
            Log.Debug("EntryEvent2: Init Managers");
        }
    }
}
