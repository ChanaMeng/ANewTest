using System;
using System.Collections.Generic;
using UnityEngine;

namespace SDClub.Loader
{
    /// <summary>
    /// 引用收集器，用于在 Prefab 上序列化 GameObject/Component 引用列表
    /// 可用于 UI 绑定等场景
    /// </summary>
    public class ReferenceCollector : MonoBehaviour
    {
        /// <summary>
        /// 序列化的引用数据
        /// </summary>
        [Serializable]
        public class ReferenceData
        {
            public string Key;
            public GameObject Value;
        }

        /// <summary>
        /// 序列化的引用列表
        /// </summary>
        public List<ReferenceData> References = new();

        /// <summary>
        /// 通过 Key 获取 GameObject 引用
        /// </summary>
        public GameObject Get(string key)
        {
            foreach (var data in this.References)
            {
                if (data.Key == key)
                {
                    return data.Value;
                }
            }
            return null;
        }

        /// <summary>
        /// 通过 Key 获取指定类型的 Component 引用
        /// </summary>
        public T Get<T>(string key) where T : Component
        {
            var go = Get(key);
            if (go == null)
            {
                return null;
            }
            return go.GetComponent<T>();
        }
    }
}
