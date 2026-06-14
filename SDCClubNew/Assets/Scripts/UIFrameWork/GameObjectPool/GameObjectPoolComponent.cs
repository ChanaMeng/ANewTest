using System.Collections.Generic;
using SDClub.Core;
using UnityEngine;

namespace SDClub.UIFrameWork
{
    public class GameObjectPoolComponent : Entity, IAwake, IDestroy
    {
        // 按 Prefab 路径管理池
        private readonly Dictionary<string, Queue<GameObject>> pool = new();
        private Transform poolRoot;
        
        public void SetPoolRoot(Transform root)
        {
            poolRoot = root;
        }
        
        public GameObject Fetch(string prefabPath)
        {
            if (pool.TryGetValue(prefabPath, out var queue) && queue.Count > 0)
            {
                var obj = queue.Dequeue();
                obj.SetActive(true);
                return obj;
            }
            return null; // 需要外部资源加载
        }
        
        public void Recycle(string prefabPath, GameObject obj)
        {
            obj.SetActive(false);
            obj.transform.SetParent(poolRoot);
            
            if (!pool.ContainsKey(prefabPath))
                pool[prefabPath] = new Queue<GameObject>();
            pool[prefabPath].Enqueue(obj);
        }
        
        public void Clear()
        {
            foreach (var queue in pool.Values)
            {
                while (queue.Count > 0)
                    GameObject.Destroy(queue.Dequeue());
            }
            pool.Clear();
        }
        
        public void Clear(string prefabPath)
        {
            if (pool.TryGetValue(prefabPath, out var queue))
            {
                while (queue.Count > 0)
                    GameObject.Destroy(queue.Dequeue());
                pool.Remove(prefabPath);
            }
        }
    }
}
