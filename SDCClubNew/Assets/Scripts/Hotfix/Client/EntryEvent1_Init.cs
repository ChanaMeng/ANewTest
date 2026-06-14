using Cysharp.Threading.Tasks;
using SDClub.Core;
using SDClub.Model;

namespace SDClub.Hotfix
{
    [Event(SceneType.Main)]
    public class EntryEvent1_InitSerialization : AEvent<Scene, EntryEvent1>
    {
        protected override async UniTask Run(Scene scene, EntryEvent1 a)
        {
            // 初始化序列化/元数据
            Log.Debug("EntryEvent1: Init Serialization");
        }
    }
}
