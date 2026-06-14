using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace SDClub.Core
{
    public sealed class KService : AService
    {
        public const int ConnectTimeoutTime = 20 * 1000;

        private readonly UdpClient udpClient;
        private readonly IPEndPoint bindEndPoint;

        private readonly Dictionary<long, KChannel> localConnChannels = new Dictionary<long, KChannel>();
        private readonly Dictionary<long, KChannel> waitAcceptChannels = new Dictionary<long, KChannel>();

        private readonly byte[] cache = new byte[2048];
        private readonly byte[] kcpBuffer = new byte[24 + (1400 + 24) * 3];

        private readonly HashSet<long> updateIds = new HashSet<long>();
        private readonly List<long> cacheIds = new List<long>();

        private uint timeNow;
        private long startTime;

        public KService(IPEndPoint ipEndPoint)
        {
            this.startTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            this.bindEndPoint = ipEndPoint;
            this.udpClient = new UdpClient(ipEndPoint);
            this.udpClient.Client.Blocking = false;
        }

        public KService(int port)
        {
            this.startTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            this.bindEndPoint = new IPEndPoint(IPAddress.Any, port);
            this.udpClient = new UdpClient(port);
            this.udpClient.Client.Blocking = false;
        }

        private uint TimeNow
        {
            get
            {
                return (uint)(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - this.startTime);
            }
        }

        public override AChannel Create(IPEndPoint ipEndPoint)
        {
            uint localConn = NetServices.Instance.CreateAcceptChannelId();
            var channel = new KChannel(localConn, ipEndPoint, this);
            channel.Id = localConn;
            this.localConnChannels[localConn] = channel;
            return channel;
        }

        public override AChannel Connect(IPEndPoint ipEndPoint)
        {
            uint localConn = NetServices.Instance.CreateConnectChannelId();
            var channel = new KChannel(localConn, ipEndPoint, this);
            channel.Id = localConn;
            this.localConnChannels[localConn] = channel;

            // Send SYN
            byte[] buffer = new byte[9];
            buffer.WriteTo(0, KcpProtocalType.SYN);
            buffer.WriteTo(1, channel.LocalConn);
            buffer.WriteTo(5, 0u);
            try
            {
                this.udpClient.Send(buffer, buffer.Length, ipEndPoint);
            }
            catch (Exception e)
            {
                Log.Error(e);
            }

            return channel;
        }

        public override void Send(long channelId, byte[] data)
        {
            if (this.localConnChannels.TryGetValue(channelId, out KChannel channel))
            {
                channel.Send(data);
            }
        }

        public void SendRaw(byte[] data, int offset, int count, IPEndPoint endPoint)
        {
            try
            {
                byte[] sendData = new byte[count];
                Array.Copy(data, offset, sendData, 0, count);
                this.udpClient.Send(sendData, count, endPoint);
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }

        public override void Remove(long channelId)
        {
            if (this.localConnChannels.TryGetValue(channelId, out KChannel channel))
            {
                this.localConnChannels.Remove(channelId);
                if (this.waitAcceptChannels.TryGetValue(channel.RemoteConn, out KChannel waitChannel))
                {
                    if (waitChannel.Id == channel.Id)
                    {
                        this.waitAcceptChannels.Remove(channel.RemoteConn);
                    }
                }
                channel.Dispose();
            }
        }

        public override void Update()
        {
            this.timeNow = this.TimeNow;
            this.Recv();
            this.UpdateChannel();
        }

        private void Recv()
        {
            while (this.udpClient.Available > 0)
            {
                IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);
                byte[] data;
                try
                {
                    data = this.udpClient.Receive(ref remoteEP);
                }
                catch
                {
                    continue;
                }

                if (data.Length < 1) continue;

                byte flag = data[0];
                try
                {
                    switch (flag)
                    {
                        case KcpProtocalType.SYN:
                        {
                            if (data.Length < 9) break;

                            uint remoteConn = BitConverter.ToUInt32(data, 1);
                            uint localConn = BitConverter.ToUInt32(data, 5);

                            // Check if we already have this accept channel
                            if (!this.waitAcceptChannels.ContainsKey(remoteConn))
                            {
                                // Create new accept channel
                                localConn = NetServices.Instance.CreateAcceptChannelId();
                                if (this.localConnChannels.ContainsKey(localConn)) break;

                                var channel = new KChannel(localConn, remoteConn, remoteEP, this);
                                channel.Id = localConn;
                                this.waitAcceptChannels[remoteConn] = channel;
                                this.localConnChannels[localConn] = channel;

                                this.AcceptCallback?.Invoke(channel.Id, remoteEP);
                            }

                            // Send ACK
                            KChannel acceptChannel = this.waitAcceptChannels[remoteConn];
                            if (acceptChannel != null)
                            {
                                byte[] ack = new byte[9];
                                ack.WriteTo(0, KcpProtocalType.ACK);
                                ack.WriteTo(1, acceptChannel.LocalConn);
                                ack.WriteTo(5, acceptChannel.RemoteConn);
                                this.udpClient.Send(ack, ack.Length, remoteEP);
                            }
                            break;
                        }
                        case KcpProtocalType.ACK:
                        {
                            if (data.Length != 9) break;

                            uint remoteConn = BitConverter.ToUInt32(data, 1);
                            uint localConn = BitConverter.ToUInt32(data, 5);

                            if (this.localConnChannels.TryGetValue(localConn, out KChannel kChannel))
                            {
                                kChannel.RemoteConn = remoteConn;
                                kChannel.HandleConnect(this.timeNow);
                                kChannel.IsConnected = true;
                            }
                            break;
                        }
                        case KcpProtocalType.FIN:
                        {
                            if (data.Length != 13) break;

                            uint localConn = BitConverter.ToUInt32(data, 1);
                            uint remoteConn = BitConverter.ToUInt32(data, 5);
                            int error = BitConverter.ToInt32(data, 9);

                            if (this.localConnChannels.TryGetValue(localConn, out KChannel kChannel))
                            {
                                if (kChannel.RemoteConn == remoteConn)
                                {
                                    Log.Debug($"KService recv FIN: {localConn} {remoteConn} {error}");
                                    kChannel.OnError(error);
                                }
                            }
                            break;
                        }
                        case KcpProtocalType.MSG:
                        {
                            if (data.Length < 9) break;

                            uint localConn = BitConverter.ToUInt32(data, 1);
                            uint remoteConn = BitConverter.ToUInt32(data, 5);

                            if (this.localConnChannels.TryGetValue(localConn, out KChannel kChannel))
                            {
                                if (kChannel.RemoteConn == remoteConn)
                                {
                                    if (!kChannel.IsConnected)
                                    {
                                        kChannel.IsConnected = true;
                                        this.waitAcceptChannels.Remove(kChannel.RemoteConn);
                                    }
                                    kChannel.HandleRecv(data, 5, data.Length - 5);
                                }
                            }
                            break;
                        }
                    }
                }
                catch (Exception e)
                {
                    Log.Error($"KService Recv error: {flag}\n{e}");
                }
            }
        }

        private void UpdateChannel()
        {
            this.cacheIds.Clear();
            this.cacheIds.AddRange(this.updateIds);
            this.updateIds.Clear();

            foreach (long id in this.cacheIds)
            {
                if (this.localConnChannels.TryGetValue(id, out KChannel channel))
                {
                    if (!channel.IsDisposed)
                    {
                        channel.UpdateKcp(this.timeNow, this.kcpBuffer);
                    }
                }
            }
            this.cacheIds.Clear();
        }

        public void AddToUpdate(long id)
        {
            this.updateIds.Add(id);
        }

        public override void Dispose()
        {
            base.Dispose();

            foreach (long id in new List<long>(this.localConnChannels.Keys))
            {
                this.Remove(id);
            }

            this.udpClient?.Close();
        }
    }
}
