using System;
using System.IO;
using System.Net;

namespace SDClub.Core
{
    public enum ChannelType
    {
        Connect,
        Accept,
    }

    public abstract class AChannel : IDisposable
    {
        public long Id;

        public ChannelType ChannelType { get; protected set; }

        public int Error { get; set; }

        public IPEndPoint RemoteAddress { get; set; }

        public bool IsDisposed
        {
            get { return this.Id == 0; }
        }

        public MemoryStream Stream { get; protected set; }

        public abstract void Start();

        public abstract void Send(byte[] data);

        public abstract void Dispose();
    }
}
