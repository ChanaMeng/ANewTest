using SDClub.Core;
using SDClub.Model;

namespace SDClub.Hotfix
{
    [EntitySystem]
    public class SessionAwakeSystem : AwakeSystem<Session, AService, long>
    {
        protected override void Awake(Session self, AService service, long channelId)
        {
            self.AService = service;
            self.ChannelId = channelId;
        }
    }
}
