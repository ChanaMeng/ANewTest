using System;
using System.Collections.Generic;

namespace SDClub.Core
{
    public class ListComponent<T> : List<T>, IDisposable, IPool
    {
        public bool IsFromPool { get; set; }

        public static ListComponent<T> Create()
        {
            var list = ObjectPool.Instance.Fetch<ListComponent<T>>();
            list.Clear();
            return list;
        }

        public void Dispose()
        {
            this.Clear();
            ObjectPool.Instance.Recycle(this);
        }
    }
}
