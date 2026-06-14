using SDClub.Core;
using UnityEngine;

namespace SDClub.Loader
{
    /// <summary>
    /// 全局组件，挂在 Scene Entity 上，持有 Global/Unit/UI 层级的 Transform 引用
    /// 通过 scene.AddComponent&lt;GlobalComponent&gt;() 添加到 Scene
    /// </summary>
    public class GlobalComponent : Entity, IAwake
    {
        /// <summary>
        /// 全局根节点 (DontDestroyOnLoad)
        /// </summary>
        public Transform Global;

        /// <summary>
        /// 单位根节点
        /// </summary>
        public Transform Unit;

        /// <summary>
        /// UI 根节点
        /// </summary>
        public Transform UI;
    }
}
