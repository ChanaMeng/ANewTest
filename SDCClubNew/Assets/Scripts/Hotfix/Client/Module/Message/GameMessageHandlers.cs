using SDClub.Core;
using SDClub.Model;

namespace SDClub.Hotfix
{
    // ===== Event Structs =====

    public struct S2C_EnterRoomEvent
    {
        public long PlayerId;
        public long RoomId;
    }

    public struct S2C_MoveEvent
    {
        public long PlayerId;
        public float X;
        public float Y;
        public float Z;
        public float RotationY;
    }

    public struct S2C_PlayerJoinEvent
    {
        public long PlayerId;
        public string Nickname;
        public int Level;
    }

    public struct S2C_PlayerLeaveEvent
    {
        public long PlayerId;
    }

    // ===== Handlers =====

    [MessageSessionHandler(SceneType.Main)]
    public class S2C_EnterRoomHandler : MessageSessionHandler<S2C_EnterRoom>
    {
        protected override void Run(Session session, S2C_EnterRoom message)
        {
            Log.Info($"Entered room: RoomId={message.RoomId}, PlayerId={message.PlayerId}");

            EventSystem.Instance.Publish(session.IScene, new S2C_EnterRoomEvent
            {
                PlayerId = message.PlayerId,
                RoomId = message.RoomId,
            });
        }
    }

    [MessageSessionHandler(SceneType.Main)]
    public class S2C_MoveHandler : MessageSessionHandler<S2C_Move>
    {
        protected override void Run(Session session, S2C_Move message)
        {
            EventSystem.Instance.Publish(session.IScene, new S2C_MoveEvent
            {
                PlayerId = message.PlayerId,
                X = message.X,
                Y = message.Y,
                Z = message.Z,
                RotationY = message.RotationY,
            });
        }
    }

    [MessageSessionHandler(SceneType.Main)]
    public class S2C_PlayerJoinHandler : MessageSessionHandler<S2C_PlayerJoin>
    {
        protected override void Run(Session session, S2C_PlayerJoin message)
        {
            Log.Info($"Player joined: PlayerId={message.PlayerId}, Nickname={message.Nickname}");

            EventSystem.Instance.Publish(session.IScene, new S2C_PlayerJoinEvent
            {
                PlayerId = message.PlayerId,
                Nickname = message.Nickname,
                Level = message.Level,
            });
        }
    }

    [MessageSessionHandler(SceneType.Main)]
    public class S2C_PlayerLeaveHandler : MessageSessionHandler<S2C_PlayerLeave>
    {
        protected override void Run(Session session, S2C_PlayerLeave message)
        {
            Log.Info($"Player left: PlayerId={message.PlayerId}");

            EventSystem.Instance.Publish(session.IScene, new S2C_PlayerLeaveEvent
            {
                PlayerId = message.PlayerId,
            });
        }
    }
}
