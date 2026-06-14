using System;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace SDClub.Core
{
    public sealed class TChannel : AChannel
    {
        private readonly TService Service;
        private TcpClient tcpClient;
        private NetworkStream networkStream;

        private bool isConnected;
        private bool isSending;

        private readonly MemoryStream sendBuffer = new MemoryStream();

        public TChannel(long id, IPEndPoint ipEndPoint, TService service)
        {
            this.Service = service;
            this.ChannelType = ChannelType.Connect;
            this.Id = id;
            this.RemoteAddress = ipEndPoint;
            this.Stream = new MemoryStream();
            this.isConnected = false;
            this.isSending = false;

            this.tcpClient = new TcpClient();
        }

        public TChannel(long id, TcpClient tcpClient, TService service)
        {
            this.Service = service;
            this.ChannelType = ChannelType.Accept;
            this.Id = id;
            this.tcpClient = tcpClient;
            this.networkStream = tcpClient.GetStream();
            this.RemoteAddress = (IPEndPoint)tcpClient.Client.RemoteEndPoint;
            this.Stream = new MemoryStream();
            this.isConnected = true;
            this.isSending = false;

            Log.Debug($"TChannel accept: {this.Id} {this.RemoteAddress}");
        }

        public override void Dispose()
        {
            if (this.IsDisposed) return;

            Log.Debug($"TChannel dispose: {this.Id} {this.RemoteAddress}");

            long id = this.Id;
            this.Id = 0;
            this.Service.Remove(id);
            this.tcpClient?.Close();
            this.tcpClient = null;
            this.networkStream = null;
        }

        public void Connect()
        {
            try
            {
                this.tcpClient.Connect(this.RemoteAddress);
                this.networkStream = this.tcpClient.GetStream();
                this.isConnected = true;

                Log.Debug($"TChannel connected: {this.Id} {this.RemoteAddress}");
            }
            catch (Exception e)
            {
                Log.Error(e);
                this.OnError((int)SocketError.ConnectionRefused);
            }
        }

        public void Receive()
        {
            if (!this.isConnected) return;
            if (this.IsDisposed) return;

            byte[] buffer = new byte[8192];
            try
            {
                int bytesRead = this.networkStream.Read(buffer, 0, buffer.Length);
                if (bytesRead > 0)
                {
                    byte[] data = new byte[bytesRead];
                    Array.Copy(buffer, 0, data, 0, bytesRead);
                    this.OnRead(data);
                }
            }
            catch (Exception e)
            {
                Log.Error($"TChannel recv error: {this.Id}\n{e}");
                this.OnError((int)SocketError.ConnectionReset);
            }
        }

        public override void Send(byte[] data)
        {
            if (this.IsDisposed)
            {
                throw new Exception("TChannel has been disposed, cannot send message");
            }

            if (!this.isConnected) return;

            try
            {
                // Write length prefix: [4 bytes length][opcode(2 bytes)][payload]
                int messageSize = data.Length;
                byte[] lengthBytes = BitConverter.GetBytes(messageSize);
                this.networkStream.Write(lengthBytes, 0, lengthBytes.Length);
                this.networkStream.Write(data, 0, data.Length);
                this.networkStream.Flush();
            }
            catch (Exception e)
            {
                Log.Error($"TChannel send error: {e}");
                this.OnError((int)SocketError.ConnectionReset);
            }
        }

        public override void Start()
        {
            // Connect channel will connect, Accept channel is already connected
            if (this.ChannelType == ChannelType.Connect)
            {
                this.Connect();
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
            Log.Debug($"TChannel OnError: {error} {this.RemoteAddress}");

            long channelId = this.Id;
            this.Service.Remove(channelId);
            this.Service.ErrorCallback?.Invoke(channelId, error);
        }
    }
}
