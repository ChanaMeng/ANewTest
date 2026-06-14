using Cysharp.Threading.Tasks;
using SDClub.Core;
using SDClub.Model;

namespace SDClub.Hotfix
{
    [Event(SceneType.Main)]
    public class NetClientOnReadEvent : AEvent<Scene, NetClientOnRead>
    {
        protected override async UniTask Run(Scene scene, NetClientOnRead a)
        {
            // 消息到达处理
            Log.Debug($"NetClientOnRead: channelId={a.ChannelId}");
        }
    }

    public struct NetClientOnRead
    {
        public long ChannelId;
        public ushort Opcode;
        public object Message;
    }
}
