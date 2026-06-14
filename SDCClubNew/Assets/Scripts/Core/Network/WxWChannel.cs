using System;
using System.Collections.Generic;
using System.Net;
using System.Net.WebSockets;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace SDClub.Core
{
    /// <summary>
    /// 微信小游戏 / WebGL 兼容的 WebSocket Channel
    /// 与 WChannel 的主要区别：
    /// - 不使用 Task.Run（WebGL 单线程不兼容）
    /// - 使用 UniTask 驱动所有异步操作
    /// - 通过 Update() 轮询收发状态（不阻塞）
    /// - 客户端专用（不依赖 HttpListener）
    /// </summary>
    public class WxWChannel : AChannel
    {
        private readonly WxWService service;
        private readonly ClientWebSocket webSocket;
        private readonly Uri serverUri;

        private bool isConnected;
        private readonly Queue<byte[]> sendQueue = new Queue<byte[]>();
        private CancellationTokenSource cancellationTokenSource;

        /// <summary>
        /// 构造函数 — 客户端发起连接
        /// </summary>
        public WxWChannel(long id, Uri uri, WxWService service)
        {
            this.service = service;
            this.Id = id;
            this.ChannelType = ChannelType.Connect;
            this.webSocket = new ClientWebSocket();
            this.serverUri = uri;
            this.Stream = new System.IO.MemoryStream();
            this.isConnected = false;
            this.cancellationTokenSource = new CancellationTokenSource();
        }

        /// <summary>
        /// 构造函数 — 服务端 Accept 的连接（WebGL 环境不可用，保留为架构兼容）
        /// </summary>
        internal WxWChannel(long id, ClientWebSocket webSocket, IPEndPoint remoteEndPoint, WxWService service)
        {
            this.service = service;
            this.Id = id;
            this.ChannelType = ChannelType.Accept;
            this.RemoteAddress = remoteEndPoint;
            this.webSocket = webSocket;
            this.serverUri = null;
            this.Stream = new System.IO.MemoryStream();
            this.cancellationTokenSource = new CancellationTokenSource();
            this.isConnected = true;
        }

        public override void Start()
        {
            if (this.ChannelType == ChannelType.Connect)
            {
                StartConnectAsync().Forget();
            }
        }

        private async UniTaskVoid StartConnectAsync()
        {
            try
            {
                await this.webSocket.ConnectAsync(this.serverUri, this.cancellationTokenSource.Token);
                this.isConnected = true;
                Log.Debug($"WxWChannel connected: {this.Id} {this.serverUri}");

                // 发送排队中的消息
                lock (this.sendQueue)
                {
                    while (this.sendQueue.Count > 0)
                    {
                        byte[] data = this.sendQueue.Dequeue();
                        SendDataAsync(data).Forget();
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error($"WxWChannel connect failed: {e}");
                this.OnError(10005);
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

            SendDataAsync(data).Forget();
        }

        private async UniTaskVoid SendDataAsync(byte[] data)
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
                Log.Error($"WxWChannel send failed: {e}");
                this.OnError(10006);
            }
        }

        /// <summary>
        /// 每帧调用 — 轮询接收 WebSocket 消息（单线程兼容）
        /// </summary>
        public void Update()
        {
            if (!this.isConnected || this.webSocket.State != WebSocketState.Open)
                return;

            TryReceiveAsync().Forget();
        }

        private async UniTaskVoid TryReceiveAsync()
        {
            try
            {
                var buffer = new byte[8192];
                var result = await this.webSocket.ReceiveAsync(
                    new ArraySegment<byte>(buffer),
                    CancellationToken.None);

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    Log.Debug($"WxWChannel close received: {this.Id}");
                    this.OnError(10004);
                    return;
                }

                if (result.Count > 0)
                {
                    byte[] data = new byte[result.Count];
                    Array.Copy(buffer, 0, data, 0, result.Count);
                    this.OnRead(data);
                }
            }
            catch (OperationCanceledException)
            {
                // 正常取消，忽略
            }
            catch (WebSocketException)
            {
                this.OnError(10007);
            }
            catch (Exception e)
            {
                Log.Error($"WxWChannel recv error: {e}");
                this.OnError(10007);
            }
        }

        public override void Dispose()
        {
            if (this.IsDisposed) return;

            Log.Debug($"WxWChannel dispose: {this.Id}");

            long id = this.Id;
            this.Id = 0;
            this.service.Remove(id);

            this.isConnected = false;
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
                this.service.ReadCallback?.Invoke(this.Id, data);
            }
            catch (Exception e)
            {
                Log.Error(e);
                this.OnError(10002);
            }
        }

        private void OnError(int error)
        {
            Log.Debug($"WxWChannel OnError: {error} {this.RemoteAddress}");

            long channelId = this.Id;
            this.service.Remove(channelId);
            this.service.ErrorCallback?.Invoke(channelId, error);
        }
    }
}
