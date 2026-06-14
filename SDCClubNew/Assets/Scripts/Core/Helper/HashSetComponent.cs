using System;
using System.Collections.Generic;

namespace SDClub.Core
{
    public class HashSetComponent<T> : HashSet<T>, IDisposable, IPool
    {
        public bool IsFromPool { get; set; }

        public static HashSetComponent<T> Create()
        {
            var set = ObjectPool.Instance.Fetch<HashSetComponent<T>>();
            set.Clear();
            return set;
        }

        public void Dispose()
        {
            this.Clear();
            ObjectPool.Instance.Recycle(this);
        }
    }
}
