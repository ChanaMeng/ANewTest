using System;
using System.Net;

namespace SDClub.Core
{
    public abstract class AService : IDisposable
    {
        public Action<long, IPEndPoint> AcceptCallback;
        public Action<long, byte[]> ReadCallback;
        public Action<long, int> ErrorCallback;

        public long Id { get; set; }

        public abstract AChannel Create(IPEndPoint ipEndPoint);

        public abstract AChannel Connect(IPEndPoint ipEndPoint);

        public abstract void Send(long channelId, byte[] data);

        public abstract void Remove(long channelId);

        public abstract void Update();

        public virtual void Dispose()
        {
        }
    }
}
