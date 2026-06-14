using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;

namespace SDClub.Core
{
    public class NetServices : Singleton<NetServices>, ISingletonAwake
    {
        private readonly Dictionary<long, AService> services = new Dictionary<long, AService>();
        private long idGenerator;
        private int acceptIdGenerator = int.MinValue;

        public void Awake()
        {
            this.idGenerator = 0;
        }

        public uint CreateConnectChannelId()
        {
            return (uint)Interlocked.Increment(ref this.acceptIdGenerator);
        }

        public uint CreateAcceptChannelId()
        {
            return (uint)Interlocked.Increment(ref this.acceptIdGenerator);
        }

        /// <summary>
        /// 创建监听服务（服务端专用，桌面平台使用）
        /// WebGL 平台下仅支持 WebSocket 协议
        /// </summary>
        public long Create(NetworkProtocol protocol, IPEndPoint ipEndPoint)
        {
            long id = Interlocked.Increment(ref this.idGenerator);
            AService service = null;

#if UNITY_WEBGL && !UNITY_EDITOR
            // WebGL 平台：仅支持 WebSocket 客户端模式
            if (protocol == NetworkProtocol.Websocket)
            {
                var wxService = new WxWService();
                service = wxService;
            }
            else
            {
                Log.Error($"NetServices: {protocol} is not supported on WebGL/WeChat platform");
            }
#else
            switch (protocol)
            {
                case NetworkProtocol.TCP:
                    var tService = new TService(ipEndPoint);
                    tService.StartListen();
                    service = tService;
                    break;
                case NetworkProtocol.KCP:
                    service = new KService(ipEndPoint);
                    break;
                case NetworkProtocol.Websocket:
                    var wService = new WService(new[] { $"http://{ipEndPoint.Address}:{ipEndPoint.Port}/" });
                    wService.StartListen();
                    service = wService;
                    break;
                case NetworkProtocol.UDP:
                    service = new KService(ipEndPoint);
                    break;
            }
#endif

            if (service != null)
            {
                service.Id = id;
                service.AcceptCallback = this.OnAccept;
                service.ReadCallback = this.OnRead;
                service.ErrorCallback = this.OnError;
                this.services[id] = service;
            }

            return id;
        }

        /// <summary>
        /// 创建客户端连接服务并连接到远程服务器
        /// 跨平台安全：桌面/WebGL 均可用
        /// </summary>
        public AChannel CreateAndConnect(NetworkProtocol protocol, IPEndPoint remoteEndPoint)
        {
            AService service = null;
            long id = Interlocked.Increment(ref this.idGenerator);

#if UNITY_WEBGL && !UNITY_EDITOR
            // WebGL/微信小游戏：仅 WebSocket
            if (protocol == NetworkProtocol.Websocket)
            {
                service = new WxWService();
            }
            else
            {
                Log.Error($"NetServices: {protocol} is not supported on WebGL/WeChat platform");
                return null;
            }
#else
            switch (protocol)
            {
                case NetworkProtocol.TCP:
                    service = new TService();
                    break;
                case NetworkProtocol.KCP:
                    service = new KService(new IPEndPoint(IPAddress.Any, 0));
                    break;
                case NetworkProtocol.Websocket:
                    service = new WService();
                    break;
                default:
                    return null;
            }
#endif

            service.Id = id;
            service.AcceptCallback = this.OnAccept;
            service.ReadCallback = this.OnRead;
            service.ErrorCallback = this.OnError;
            this.services[id] = service;

            return service.Connect(remoteEndPoint);
        }

        public AService Get(long id)
        {
            this.services.TryGetValue(id, out AService service);
            return service;
        }

        public void Send(long serviceId, long channelId, byte[] data)
        {
            if (this.services.TryGetValue(serviceId, out AService service))
            {
                service.Send(channelId, data);
            }
        }

        public void Remove(long serviceId, long channelId)
        {
            if (this.services.TryGetValue(serviceId, out AService service))
            {
                service.Remove(channelId);
            }
        }

        public void Update()
        {
            foreach (var kv in this.services)
            {
                kv.Value.Update();
            }
        }

        public void RemoveService(long id)
        {
            if (this.services.TryGetValue(id, out AService service))
            {
                this.services.Remove(id);
                service.Dispose();
            }
        }

        private void OnAccept(long channelId, IPEndPoint ipEndPoint)
        {
            Log.Debug($"NetService OnAccept: {channelId} {ipEndPoint}");
        }

        private void OnRead(long channelId, byte[] data)
        {
            // Data will be dispatched to message handlers
        }

        private void OnError(long channelId, int error)
        {
            Log.Debug($"NetService OnError: {channelId} {error}");
        }

        public override void Dispose()
        {
            if (this.IsDisposed()) return;

            foreach (var kv in new Dictionary<long, AService>(this.services))
            {
                kv.Value.Dispose();
            }
            this.services.Clear();

            base.Dispose();
        }
    }
}
