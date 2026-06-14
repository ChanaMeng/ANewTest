using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace SDClub.Core
{
    public sealed class TService : AService
    {
        private readonly Dictionary<long, TChannel> idChannels = new Dictionary<long, TChannel>();

        private TcpListener tcpListener;
        private bool isListening;

        public TService(IPEndPoint ipEndPoint)
        {
            this.tcpListener = new TcpListener(ipEndPoint);
        }

        public TService()
        {
            this.tcpListener = null;
        }

        public override AChannel Create(IPEndPoint ipEndPoint)
        {
            long id = NetServices.Instance.CreateAcceptChannelId();
            TChannel channel = new TChannel(id, ipEndPoint, this);
            channel.Id = id;
            this.idChannels[channel.Id] = channel;
            return channel;
        }

        public override AChannel Connect(IPEndPoint ipEndPoint)
        {
            long id = NetServices.Instance.CreateConnectChannelId();
            TChannel channel = new TChannel(id, ipEndPoint, this);
            channel.Id = id;
            this.idChannels[channel.Id] = channel;
            channel.Start();
            return channel;
        }

        public void StartListen()
        {
            if (this.tcpListener == null) return;

            try
            {
                this.tcpListener.Start();
                this.isListening = true;
            }
            catch (Exception e)
            {
                Log.Error($"TService listen error: {e}");
            }
        }

        public void Accept()
        {
            if (!this.isListening || this.tcpListener == null) return;

            try
            {
                while (this.tcpListener.Pending())
                {
                    TcpClient tcpClient = this.tcpListener.AcceptTcpClient();
                    long id = NetServices.Instance.CreateAcceptChannelId();
                    TChannel channel = new TChannel(id, tcpClient, this);
                    channel.Id = id;
                    this.idChannels[channel.Id] = channel;

                    this.AcceptCallback?.Invoke(channel.Id, channel.RemoteAddress);
                }
            }
            catch (Exception e)
            {
                Log.Error($"TService accept error: {e}");
            }
        }

        public override void Send(long channelId, byte[] data)
        {
            if (this.idChannels.TryGetValue(channelId, out TChannel channel))
            {
                channel.Send(data);
            }
            else
            {
                this.ErrorCallback?.Invoke(channelId, 10003);
            }
        }

        public override void Remove(long channelId)
        {
            if (this.idChannels.TryGetValue(channelId, out TChannel channel))
            {
                this.idChannels.Remove(channelId);
                channel.Dispose();
            }
        }

        public override void Update()
        {
            if (this.isListening) this.Accept();
        }

        public override void Dispose()
        {
            base.Dispose();

            foreach (long id in new List<long>(this.idChannels.Keys))
            {
                TChannel channel = this.idChannels[id];
                channel.Dispose();
            }
            this.idChannels.Clear();

            this.tcpListener?.Stop();
            this.tcpListener = null;
            this.isListening = false;
        }
    }
}
