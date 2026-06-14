using System;
using System.Collections.Generic;
using System.Net;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace SDClub.Core
{
    public class WService : AService
    {
        private readonly Dictionary<long, WChannel> channels = new Dictionary<long, WChannel>();

        private HttpListener httpListener;
        private CancellationTokenSource cancellationTokenSource;
        private bool isListening;

        public WService(IEnumerable<string> prefixes)
        {
            this.httpListener = new HttpListener();
            foreach (string prefix in prefixes)
            {
                this.httpListener.Prefixes.Add(prefix);
            }
            this.cancellationTokenSource = new CancellationTokenSource();
        }

        public WService()
        {
            this.httpListener = null;
        }

        public void StartListen()
        {
            if (this.httpListener == null) return;

            try
            {
                this.httpListener.Start();
                this.isListening = true;
                Task.Run(async () => await this.AcceptLoop());
            }
            catch (Exception e)
            {
                Log.Error($"WService listen error: {e}");
            }
        }

        private async Task AcceptLoop()
        {
            while (this.isListening && this.httpListener != null)
            {
                try
                {
                    var context = await this.httpListener.GetContextAsync();
                    if (context.Request.IsWebSocketRequest)
                    {
                        var wsContext = await context.AcceptWebSocketAsync(null);
                        long id = NetServices.Instance.CreateAcceptChannelId();
                        var channel = new WChannel(id, wsContext.WebSocket, context.Request.RemoteEndPoint, this);
                        channel.Id = id;
                        this.channels[channel.Id] = channel;
                        channel.Start();

                        this.AcceptCallback?.Invoke(channel.Id, channel.RemoteAddress);
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception e)
                {
                    Log.Error($"WService accept error: {e}");
                }
            }
        }

        public override AChannel Create(IPEndPoint ipEndPoint)
        {
            long id = NetServices.Instance.CreateConnectChannelId();
            var webSocket = new ClientWebSocket();
            var uri = new Uri($"ws://{ipEndPoint.Address}:{ipEndPoint.Port}");
            var channel = new WChannel(id, webSocket, uri, this);
            channel.Id = id;
            channel.RemoteAddress = ipEndPoint;
            this.channels[channel.Id] = channel;
            return channel;
        }

        public override AChannel Connect(IPEndPoint ipEndPoint)
        {
            var channel = (WChannel)this.Create(ipEndPoint);
            channel.Start();
            return channel;
        }

        public override void Send(long channelId, byte[] data)
        {
            if (this.channels.TryGetValue(channelId, out WChannel channel))
            {
                channel.Send(data);
            }
        }

        public override void Remove(long channelId)
        {
            if (this.channels.TryGetValue(channelId, out WChannel channel))
            {
                this.channels.Remove(channelId);
                channel.Dispose();
            }
        }

        public override void Update()
        {
            // WebSocket operations are async, no per-frame update needed
        }

        public override void Dispose()
        {
            base.Dispose();

            this.isListening = false;
            this.cancellationTokenSource?.Cancel();
            this.cancellationTokenSource?.Dispose();
            this.cancellationTokenSource = null;

            foreach (long id in new List<long>(this.channels.Keys))
            {
                WChannel channel = this.channels[id];
                channel.Dispose();
            }
            this.channels.Clear();

            this.httpListener?.Stop();
            this.httpListener?.Close();
            this.httpListener = null;
        }
    }
}
