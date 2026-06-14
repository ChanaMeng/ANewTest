using System;
using System.Collections.Generic;
using System.IO;
using System.Net;

namespace SDClub.Core
{
    public static class KcpProtocalType
    {
        public const byte SYN = 1;
        public const byte ACK = 2;
        public const byte FIN = 3;
        public const byte MSG = 4;
    }

    public class KChannel : AChannel
    {
        private const int MaxKcpMessageSize = 10000;

        private readonly KService Service;
        private Kcp kcp;

        private readonly Queue<byte[]> waitSendMessages = new Queue<byte[]>();

        public uint LocalConn { get; set; }
        public uint RemoteConn { get; set; }

        public bool IsConnected { get; set; }

        private readonly byte[] sendCache = new byte[2 * 1024];

        private MemoryStream readMemory;

        public KChannel(uint localConn, IPEndPoint remoteEndPoint, KService kService)
        {
            this.Service = kService;
            this.LocalConn = localConn;
            this.ChannelType = ChannelType.Connect;
            this.RemoteAddress = remoteEndPoint;
            this.Stream = new MemoryStream();

            Log.Debug($"KChannel create: {this.LocalConn} {remoteEndPoint} Connect");
        }

        public KChannel(uint localConn, uint remoteConn, IPEndPoint remoteEndPoint, KService kService)
        {
            this.Service = kService;
            this.ChannelType = ChannelType.Accept;
            this.LocalConn = localConn;
            this.RemoteConn = remoteConn;
            this.RemoteAddress = remoteEndPoint;
            this.Stream = new MemoryStream();
            this.kcp = new Kcp(this.RemoteConn, this.Output);
            InitKcp();

            Log.Debug($"KChannel create: {localConn} {remoteConn} {remoteEndPoint} Accept");
        }

        private void InitKcp()
        {
            this.kcp.SetNoDelay(1, 10, 2, 1);
            this.kcp.SetWindowSize(128, 128);
            this.kcp.SetMtu(1400);
            this.kcp.SetMinrto(30);
        }

        public override void Start()
        {
        }

        public override void Dispose()
        {
            if (this.IsDisposed) return;

            Log.Debug($"KChannel dispose: {this.LocalConn} {this.RemoteConn} {this.Error}");

            long id = this.Id;
            this.Id = 0;
            this.Service.Remove(id);

            this.kcp = null;
        }

        public void HandleConnect(uint timeNow)
        {
            if (this.IsConnected) return;

            this.kcp = new Kcp(this.RemoteConn, this.Output);
            InitKcp();
            this.IsConnected = true;

            while (this.waitSendMessages.Count > 0)
            {
                byte[] data = this.waitSendMessages.Dequeue();
                this.KcpSend(data);
            }
        }

        public void UpdateKcp(uint timeNow, byte[] kcpBuffer)
        {
            if (this.IsDisposed) return;
            if (this.kcp == null) return;

            try
            {
                this.kcp.Update(timeNow, kcpBuffer);
            }
            catch (Exception e)
            {
                Log.Error(e);
                this.OnError(10001);
            }
        }

        public void HandleRecv(byte[] data, int offset, int length)
        {
            if (this.IsDisposed) return;

            this.kcp.Input(new Span<byte>(data, offset, length));

            while (true)
            {
                if (this.IsDisposed) break;

                int peekSize = this.kcp.PeekSize();
                if (peekSize < 0) break;

                byte[] buffer = new byte[peekSize];
                int count = this.kcp.Receive(new Span<byte>(buffer));

                if (count > 0)
                {
                    this.OnRead(buffer);
                }
            }
        }

        public override void Send(byte[] data)
        {
            if (!this.IsConnected)
            {
                this.waitSendMessages.Enqueue(data);
                return;
            }

            if (this.kcp == null)
            {
                throw new Exception("KChannel connected but kcp is null!");
            }

            if (this.IsDisposed) return;

            this.KcpSend(data);
        }

        private void KcpSend(byte[] data)
        {
            if (this.IsDisposed) return;

            int count = data.Length;
            if (count <= MaxKcpMessageSize)
            {
                this.kcp.Send(new Span<byte>(data, 0, count));
            }
            else
            {
                // Shard info header
                this.sendCache.WriteTo(0, 0);
                this.sendCache.WriteTo(4, count);
                this.kcp.Send(new Span<byte>(this.sendCache, 0, 8));

                int alreadySendCount = 0;
                while (alreadySendCount < count)
                {
                    int leftCount = count - alreadySendCount;
                    int sendCount = leftCount < MaxKcpMessageSize ? leftCount : MaxKcpMessageSize;
                    this.kcp.Send(new Span<byte>(data, alreadySendCount, sendCount));
                    alreadySendCount += sendCount;
                }
            }
        }

        private void Output(byte[] bytes, int count)
        {
            if (this.IsDisposed) return;
            if (!this.IsConnected) return;

            try
            {
                bytes.WriteTo(0, KcpProtocalType.MSG);
                bytes.WriteTo(1, this.LocalConn);
                this.Service.SendRaw(bytes, 0, count + 5, this.RemoteAddress);
            }
            catch (Exception e)
            {
                Log.Error(e);
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

        public void OnError(int error)
        {
            Log.Debug($"KChannel OnError: {error} {this.RemoteAddress}");

            long channelId = this.Id;
            this.Service.Remove(channelId);
            this.Service.ErrorCallback?.Invoke(channelId, error);
        }
    }

    public static class ByteArrayExtension
    {
        public static void WriteTo(this byte[] bytes, int offset, int num)
        {
            bytes[offset] = (byte)num;
            bytes[offset + 1] = (byte)(num >> 8);
            bytes[offset + 2] = (byte)(num >> 16);
            bytes[offset + 3] = (byte)(num >> 24);
        }

        public static void WriteTo(this byte[] bytes, int offset, uint num)
        {
            bytes[offset] = (byte)num;
            bytes[offset + 1] = (byte)(num >> 8);
            bytes[offset + 2] = (byte)(num >> 16);
            bytes[offset + 3] = (byte)(num >> 24);
        }
    }
}
