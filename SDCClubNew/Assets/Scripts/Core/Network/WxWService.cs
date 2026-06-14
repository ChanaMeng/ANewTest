using System;
using System.Collections.Generic;
using System.Net;
using System.Net.WebSockets;

namespace SDClub.Core
{
    /// <summary>
    /// 微信小游戏 / WebGL 兼容的 WebSocket Service
    /// 客户端专用实现：连接到远程服务器，不监听本地端口
    /// 与 WService 的区别：
    /// - 不依赖 HttpListener（WebGL 不可用）
    /// - 不使用 Task.Run（WebGL 单线程不兼容）
    /// - 每帧 Update 驱动所有 Channel 的消息轮询
    /// </summary>
    public class WxWService : AService
    {
        private readonly Dictionary<long, WxWChannel> channels = new Dictionary<long, WxWChannel>();

        public WxWService()
        {
        }

        public override AChannel Create(IPEndPoint ipEndPoint)
        {
            long id = NetServices.Instance.CreateConnectChannelId();
            var uri = new Uri($"ws://{ipEndPoint.Address}:{ipEndPoint.Port}");
            var channel = new WxWChannel(id, uri, this);
            channel.Id = id;
            channel.RemoteAddress = ipEndPoint;
            this.channels[channel.Id] = channel;
            return channel;
        }

        public override AChannel Connect(IPEndPoint ipEndPoint)
        {
            var channel = (WxWChannel)this.Create(ipEndPoint);
            channel.Start();
            return channel;
        }

        public override void Send(long channelId, byte[] data)
        {
            if (this.channels.TryGetValue(channelId, out WxWChannel channel))
            {
                channel.Send(data);
            }
        }

        public override void Remove(long channelId)
        {
            if (this.channels.TryGetValue(channelId, out WxWChannel channel))
            {
                this.channels.Remove(channelId);
                channel.Dispose();
            }
        }

        /// <summary>
        /// 每帧调用 — 驱动所有 Channel 的消息轮询
        /// </summary>
        public override void Update()
        {
            // 遍历副本，避免 dispose 时修改集合
            var snapshot = new List<WxWChannel>(this.channels.Values);
            foreach (var channel in snapshot)
            {
                if (!channel.IsDisposed)
                {
                    channel.Update();
                }
            }
        }

        public override void Dispose()
        {
            base.Dispose();

            foreach (long id in new List<long>(this.channels.Keys))
            {
                if (this.channels.TryGetValue(id, out WxWChannel channel))
                {
                    channel.Dispose();
                }
            }
            this.channels.Clear();
        }
    }
}
