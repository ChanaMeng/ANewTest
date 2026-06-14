using System;
using System.ComponentModel;

namespace SDClub.Core
{
    public abstract class Object
    {
    }

    public abstract class DisposeObject : Object, IDisposable, ISupportInitialize
    {
        public virtual void Dispose()
        {
        }

        public virtual void BeginInit()
        {
        }

        public virtual void EndInit()
        {
        }
    }

    public interface IPool
    {
        bool IsFromPool { get; set; }
    }
}
