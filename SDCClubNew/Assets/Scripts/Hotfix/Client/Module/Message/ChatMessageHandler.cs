using SDClub.Core;
using SDClub.Model;

namespace SDClub.Hotfix
{
    public struct S2C_ChatEvent
    {
        public long SenderId;
        public string SenderName;
        public string Content;
        public int Channel;
    }

    [MessageSessionHandler(SceneType.Main)]
    public class S2C_ChatHandler : MessageSessionHandler<S2C_Chat>
    {
        protected override void Run(Session session, S2C_Chat message)
        {
            Log.Info($"Chat: [{message.SenderName}({message.SenderId})] {message.Content}");

            EventSystem.Instance.Publish(session.IScene, new S2C_ChatEvent
            {
                SenderId = message.SenderId,
                SenderName = message.SenderName,
                Content = message.Content,
                Channel = message.Channel,
            });
        }
    }
}
