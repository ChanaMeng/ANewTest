using System;

namespace SDClub.Core
{
    public abstract class ATimer : IDisposable
    {
        public long Id { get; set; }

        public long StartFrame { get; set; }

        public long Interval { get; set; }

        public bool IsRepeated { get; set; }

        public bool IsDisposed { get; private set; }

        public abstract void Handle();

        public virtual void Dispose()
        {
            this.IsDisposed = true;
        }
    }
}
