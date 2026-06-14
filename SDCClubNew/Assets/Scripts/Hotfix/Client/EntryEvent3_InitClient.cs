using Cysharp.Threading.Tasks;
using SDClub.Core;
using SDClub.Model;

namespace SDClub.Hotfix
{
    [Event(SceneType.Main)]
    public class EntryEvent3_InitClient : AEvent<Scene, EntryEvent3>
    {
        protected override async UniTask Run(Scene scene, EntryEvent3 a)
        {
            // 初始化客户端组件
            Log.Debug("EntryEvent3: Init Client Components");
        }
    }
}
