using UnityEngine;

namespace SDClub.UIFrameWork
{
    public static class GameObjectExtensions
    {
        /// 在子节点中递归查找
        public static Transform FindRecursive(this Transform parent, string name)
        {
            if (parent.name == name) return parent;
            
            for (int i = 0; i < parent.childCount; i++)
            {
                var found = FindRecursive(parent.GetChild(i), name);
                if (found != null) return found;
            }
            return null;
        }
        
        public static T FindComponent<T>(this GameObject go, string path) where T : Component
        {
            var child = go.transform.FindRecursive(path);
            return child?.GetComponent<T>();
        }
        
        /// 获取或添加组件
        public static T GetOrAddComponent<T>(this GameObject go) where T : Component
        {
            var comp = go.GetComponent<T>();
            return comp ?? go.AddComponent<T>();
        }
        
        /// 设置父节点并重置
        public static void SetParentAndReset(this Transform child, Transform parent)
        {
            child.SetParent(parent);
            child.localPosition = Vector3.zero;
            child.localRotation = Quaternion.identity;
            child.localScale = Vector3.one;
        }
    }
}
