using System;
using System.Collections.Generic;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SDClub.Core
{
    public class WChannel : AChannel
    {
        private readonly WService Service;
        private readonly WebSocket webSocket;
        private readonly Uri serverUri;

        private bool isConnected;
        private bool isSending;
        private readonly Queue<byte[]> sendQueue = new Queue<byte[]>();
        private CancellationTokenSource cancellationTokenSource;

        public WChannel(long id, ClientWebSocket webSocket, Uri uri, WService service)
        {
            this.Service = service;
            this.Id = id;
            this.ChannelType = ChannelType.Connect;
            this.webSocket = webSocket;
            this.serverUri = uri;
            this.Stream = new System.IO.MemoryStream();
            this.isConnected = false;
            this.isSending = false;
            this.cancellationTokenSource = new CancellationTokenSource();
        }

        public WChannel(long id, WebSocket webSocket, IPEndPoint remoteEndPoint, WService service)
        {
            this.Service = service;
            this.Id = id;
            this.ChannelType = ChannelType.Accept;
            this.RemoteAddress = remoteEndPoint;
            this.webSocket = webSocket;
            this.serverUri = null;
            this.Stream = new System.IO.MemoryStream();
            this.cancellationTokenSource = new CancellationTokenSource();
            this.isConnected = true;
            this.isSending = false;
        }

        public override void Start()
        {
            if (this.ChannelType == ChannelType.Connect)
            {
                Task.Run(async () =>
                {
                    try
                    {
                        await ((ClientWebSocket)this.webSocket).ConnectAsync(this.serverUri, this.cancellationTokenSource.Token);
                        this.isConnected = true;
                        Log.Debug($"WChannel connected: {this.Id} {this.serverUri}");
                        _ = this.StartRecvAsync();
                    }
                    catch (Exception e)
                    {
                        Log.Error(e);
                        this.OnError(10005);
                    }
                });
            }
            else
            {
                Task.Run(async () => await this.StartRecvAsync());
            }
        }

        public override void Send(byte[] data)
        {
            if (!this.isConnected)
            {
                lock (this.sendQueue)
                {
                    this.sendQueue.Enqueue(data);
                }
                return;
            }

            Task.Run(async () =>
            {
                try
                {
                    await this.webSocket.SendAsync(
                        new ArraySegment<byte>(data),
                        WebSocketMessageType.Binary,
                        true,
                        this.cancellationTokenSource.Token);
                }
                catch (Exception e)
                {
                    Log.Error(e);
                    this.OnError(10006);
                }
            });
        }

        private async Task StartRecvAsync()
        {
            var buffer = new byte[8192];

            while (this.webSocket.State == WebSocketState.Open ||
                   this.webSocket.State == WebSocketState.Connecting)
            {
                try
                {
                    var result = await this.webSocket.ReceiveAsync(
                        new ArraySegment<byte>(buffer),
                        this.cancellationTokenSource.Token);

                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        Log.Debug($"WChannel close received: {this.Id}");
                        this.OnError(10004);
                        return;
                    }

                    if (result.Count > 0)
                    {
                        byte[] data = new byte[result.Count];
                        Array.Copy(buffer, 0, data, 0, result.Count);
                        this.OnRead(data);
                    }

                    if (result.EndOfMessage == false)
                    {
                        // Multi-frame message: simplified, just continue reading
                        continue;
                    }
                }
                catch (OperationCanceledException)
                {
                    return;
                }
                catch (Exception e)
                {
                    Log.Error($"WChannel recv error: {e}");
                    this.OnError(10007);
                    return;
                }
            }
        }

        public override void Dispose()
        {
            if (this.IsDisposed) return;

            Log.Debug($"WChannel dispose: {this.Id}");

            long id = this.Id;
            this.Id = 0;
            this.Service.Remove(id);

            this.cancellationTokenSource?.Cancel();
            this.cancellationTokenSource?.Dispose();
            this.cancellationTokenSource = null;

            if (this.webSocket != null)
            {
                try
                {
                    this.webSocket.CloseAsync(
                        WebSocketCloseStatus.NormalClosure,
                        "Dispose",
                        CancellationToken.None).Wait(1000);
                }
                catch { }
                this.webSocket.Dispose();
            }
        }

        private void OnRead(byte[] data)
        {
            try
            {
                this.Service.ReadCallback?.Invoke(this.Id, data);
            }
            catch (Exception e)
            {
                Log.Error(e);
                this.OnError(10002);
            }
        }

        private void OnError(int error)
        {
            Log.Debug($"WChannel OnError: {error} {this.RemoteAddress}");

            long channelId = this.Id;
            this.Service.Remove(channelId);
            this.Service.ErrorCallback?.Invoke(channelId, error);
        }
    }
}
