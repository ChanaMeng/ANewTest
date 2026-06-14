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

        public long Create(NetworkProtocol protocol, IPEndPoint ipEndPoint)
        {
            long id = Interlocked.Increment(ref this.idGenerator);
            AService service = null;

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
