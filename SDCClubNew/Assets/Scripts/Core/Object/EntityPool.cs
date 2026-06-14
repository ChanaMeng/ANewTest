using System;
using System.Collections.Generic;

namespace SDClub.Core
{
    public class EntityPool : Singleton<EntityPool>, ISingletonAwake
    {
        private Dictionary<long, Pool> entityPool;
        private const int DefaultCapacity = 3000;

        public void Awake()
        {
            entityPool = new Dictionary<long, Pool>();
        }

        public T Fetch<T>(long id, bool isFromPool = true) where T : Entity
        {
            if (!isFromPool)
            {
                return Activator.CreateInstance<T>();
            }

            Pool pool = GetPool(typeof(T), id);
            object obj = pool.Get();
            if (obj is IPool p)
            {
                p.IsFromPool = true;
            }
            return obj as T;
        }

        public void Recycle<T>(T entity, long id) where T : Entity
        {
            if (entity is IPool p)
            {
                if (!p.IsFromPool)
                {
                    return;
                }

                // 防止多次入池
                p.IsFromPool = false;
            }

            Pool pool = GetPool(entity.GetType(), id);
            pool.Return(entity);
        }

        private Pool GetPool(Type type, long id)
        {
            if (!entityPool.TryGetValue(id, out var pool))
            {
                pool = new Pool(type, DefaultCapacity);
                entityPool[id] = pool;
            }
            return pool;
        }

        /// <summary>
        /// 释放所有未使用的对象
        /// </summary>
        public void ReleaseUnused()
        {
            foreach (var kv in entityPool)
            {
                kv.Value.Clear();
            }
        }

        private class Pool
        {
            private readonly Type _objectType;
            private readonly int _maxCapacity;
            private int _numItems;
            private readonly Queue<object> _items = new();
            private object FastItem;

            public Pool(Type objectType, int maxCapacity)
            {
                _objectType = objectType;
                _maxCapacity = maxCapacity;
            }

            public object Get()
            {
                if (FastItem != null)
                {
                    object item = FastItem;
                    FastItem = null;
                    return item;
                }

                if (_items.Count > 0)
                {
                    _numItems--;
                    return _items.Dequeue();
                }

                return Activator.CreateInstance(_objectType);
            }

            public void Return(object obj)
            {
                if (FastItem == null)
                {
                    FastItem = obj;
                    return;
                }

                if (_numItems < _maxCapacity)
                {
                    _items.Enqueue(obj);
                    _numItems++;
                }
            }

            public void Clear()
            {
                FastItem = null;
                _items.Clear();
                _numItems = 0;
            }
        }
    }
}
