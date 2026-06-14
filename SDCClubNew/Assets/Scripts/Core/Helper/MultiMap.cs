using System.Collections.Generic;

namespace SDClub.Core
{
    public class MultiMap<TKey, TValue> : Dictionary<TKey, List<TValue>>
    {
        public new List<TValue> this[TKey t]
        {
            get
            {
                if (!this.TryGetValue(t, out List<TValue> list))
                {
                    list = new List<TValue>();
                }
                return list;
            }
        }

        public void Add(TKey t, TValue k)
        {
            if (!this.TryGetValue(t, out List<TValue> list))
            {
                list = new List<TValue>();
                base[t] = list;
            }
            list.Add(k);
        }

        public bool Remove(TKey t, TValue k)
        {
            if (!this.TryGetValue(t, out List<TValue> list))
            {
                return false;
            }
            if (!list.Remove(k))
            {
                return false;
            }
            if (list.Count == 0)
            {
                this.Remove(t);
            }
            return true;
        }

        public bool Contains(TKey t, TValue k)
        {
            if (!this.TryGetValue(t, out List<TValue> list))
            {
                return false;
            }
            return list.Contains(k);
        }

        public new int Count
        {
            get
            {
                int count = 0;
                foreach (var kv in this)
                {
                    count += kv.Value.Count;
                }
                return count;
            }
        }
    }
}
