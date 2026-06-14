using SDClub.Core;
using SDClub.Model;

namespace SDClub.Hotfix
{
    [EntitySystem]
    public class NetComponentAwakeSystem : AwakeSystem<NetComponent>
    {
        protected override void Awake(NetComponent self)
        {
            Log.Debug("NetComponent Awake");
        }
    }

    [EntitySystem]
    public class NetComponentUpdateSystem : UpdateSystem<NetComponent>
    {
        protected override void Update(NetComponent self)
        {
            // 处理网络消息
            self.Service?.Update();
        }
    }
}
