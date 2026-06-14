using SDClub.Core;
using SDClub.Model;

namespace SDClub.Hotfix
{
    [MessageSessionHandler(SceneType.Main)]
    public class S2C_LoginHandler : MessageSessionHandler<S2C_Login>
    {
        protected override void Run(Session session, S2C_Login message)
        {
            if (message.Error != (int)ErrorCode.Success)
            {
                Log.Error($"Login failed: Error={message.Error}, Message={message.Message}");
                return;
            }

            Log.Info($"Login success: PlayerId={message.PlayerId}, Nickname={message.Nickname}, Level={message.Level}");

            // Store player info for later use
            EventSystem.Instance.Publish(session.IScene, new PlayerLoginEvent
            {
                PlayerId = message.PlayerId,
                Nickname = message.Nickname,
                Level = message.Level,
            });
        }
    }

    public struct PlayerLoginEvent
    {
        public long PlayerId;
        public string Nickname;
        public int Level;
    }
}
