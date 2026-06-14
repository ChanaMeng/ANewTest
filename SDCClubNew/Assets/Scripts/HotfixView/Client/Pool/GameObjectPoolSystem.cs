using System.Collections.Generic;
using SDClub.Core;
using UnityEngine;

namespace SDClub.HotfixView
{
    public interface IPoolable
    {
        void OnSpawn();
        void OnDespawn();
    }

    public class GameObjectPoolSystem : Singleton<GameObjectPoolSystem>, ISingletonAwake
    {
        private readonly Dictionary<GameObject, Queue<GameObject>> pool =
            new Dictionary<GameObject, Queue<GameObject>>();

        private readonly Dictionary<GameObject, GameObject> prefabLookup =
            new Dictionary<GameObject, GameObject>();

        private Transform poolRoot;

        public void Awake()
        {
            var go = new GameObject("[GameObjectPool]");
            GameObject.DontDestroyOnLoad(go);
            this.poolRoot = go.transform;
        }

        public GameObject Spawn(GameObject prefab, Vector3 position = default,
            Quaternion rotation = default, Transform parent = null)
        {
            Queue<GameObject> queue = GetQueue(prefab);
            GameObject obj;

            if (queue.Count > 0)
            {
                obj = queue.Dequeue();
                if (obj == null)
                {
                    obj = GameObject.Instantiate(prefab);
                }
                else
                {
                    obj.SetActive(true);
                }
            }
            else
            {
                obj = GameObject.Instantiate(prefab);
            }

            obj.transform.SetParent(parent ?? this.poolRoot);
            obj.transform.SetPositionAndRotation(position, rotation == default ? Quaternion.identity : rotation);
            this.prefabLookup[obj] = prefab;

            var poolables = obj.GetComponents<IPoolable>();
            foreach (var p in poolables)
            {
                p.OnSpawn();
            }

            return obj;
        }

        public T Spawn<T>(T prefab, Vector3 position = default,
            Quaternion rotation = default, Transform parent = null) where T : Component
        {
            return Spawn(prefab.gameObject, position, rotation, parent).GetComponent<T>();
        }

        public void Despawn(GameObject obj)
        {
            if (obj == null) return;

            var poolables = obj.GetComponents<IPoolable>();
            foreach (var p in poolables)
            {
                p.OnDespawn();
            }

            obj.SetActive(false);
            obj.transform.SetParent(this.poolRoot);

            if (this.prefabLookup.TryGetValue(obj, out var prefab))
            {
                Queue<GameObject> queue = GetQueue(prefab);
                queue.Enqueue(obj);
            }
            else
            {
                GameObject.Destroy(obj);
            }
        }

        public void Prewarm(GameObject prefab, int count)
        {
            Queue<GameObject> queue = GetQueue(prefab);

            for (int i = 0; i < count; i++)
            {
                var obj = GameObject.Instantiate(prefab, this.poolRoot);
                obj.SetActive(false);
                this.prefabLookup[obj] = prefab;
                queue.Enqueue(obj);
            }
        }

        public void ClearPool(GameObject prefab)
        {
            if (!this.pool.TryGetValue(prefab, out var queue))
            {
                return;
            }

            while (queue.Count > 0)
            {
                var obj = queue.Dequeue();
                if (obj != null)
                {
                    this.prefabLookup.Remove(obj);
                    GameObject.Destroy(obj);
                }
            }

            this.pool.Remove(prefab);
        }

        public void ClearAll()
        {
            foreach (var kv in this.pool)
            {
                while (kv.Value.Count > 0)
                {
                    var obj = kv.Value.Dequeue();
                    if (obj != null)
                    {
                        this.prefabLookup.Remove(obj);
                        GameObject.Destroy(obj);
                    }
                }
            }

            this.pool.Clear();
            this.prefabLookup.Clear();
        }

        private Queue<GameObject> GetQueue(GameObject prefab)
        {
            if (!this.pool.TryGetValue(prefab, out var queue))
            {
                queue = new Queue<GameObject>();
                this.pool[prefab] = queue;
            }

            return queue;
        }

        public override void Dispose()
        {
            ClearAll();
            if (this.poolRoot != null)
            {
                GameObject.Destroy(this.poolRoot.gameObject);
            }

            base.Dispose();
        }
    }
}
