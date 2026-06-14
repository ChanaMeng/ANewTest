using System;
using SDClub.Core;
using SDClub.Model;

namespace SDClub.Hotfix
{
    /// <summary>
    /// 心跳系统：每30秒发送心跳，60秒无响应则触发断线重连
    /// </summary>
    [EntitySystem]
    public class HeartbeatSystem : UpdateSystem<NetComponent>
    {
        private const long HeartbeatInterval = 30000;  // 30秒
        private const long HeartbeatTimeout = 60000;   // 60秒超时

        protected override void Update(NetComponent self)
        {
            var session = self.Session;
            if (session == null || session.IsDisposed)
            {
                return;
            }

            long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            long elapsed = now - session.LastRecvTime;

            // 超时检测：60秒无响应则断开并重连
            if (elapsed >= HeartbeatTimeout)
            {
                Log.Warning($"Heartbeat timeout after {elapsed}ms, triggering reconnect...");
                EventSystem.Instance.Publish(session.IScene, new ConnectionTimeoutEvent
                {
                    LastRecvTime = session.LastRecvTime,
                });

                // 断开连接
                session.Dispose();
                self.Session = null;
                return;
            }

            // 发送心跳：每30秒发送一次
            long sinceLastSend = now - session.LastSendTime;
            if (sinceLastSend >= HeartbeatInterval)
            {
                session.Send(new C2S_Heartbeat { Timestamp = now });
                session.LastSendTime = now;
            }
        }
    }

    public struct ConnectionTimeoutEvent
    {
        public long LastRecvTime;
    }
}
