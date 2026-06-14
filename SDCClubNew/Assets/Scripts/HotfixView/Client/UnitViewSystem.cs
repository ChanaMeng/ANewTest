using SDClub.Core;
using SDClub.ModelView;
using UnityEngine;

namespace SDClub.HotfixView
{
    // 单位视图更新系统
    [EntitySystem]
    public class UnitViewUpdateSystem : UpdateSystem<UnitViewComponent>
    {
        protected override void Update(UnitViewComponent self)
        {
            // 每帧更新视图
        }
    }

    [EntitySystem]
    public class UnitViewAwakeSystem : AwakeSystem<UnitViewComponent>
    {
        protected override void Awake(UnitViewComponent self)
        {
            Log.Debug("UnitViewComponent Awake");
        }
    }

    [EntitySystem]
    public class UnitViewDestroySystem : DestroySystem<UnitViewComponent>
    {
        protected override void Destroy(UnitViewComponent self)
        {
            if (self.GameObject != null)
            {
                GameObject.Destroy(self.GameObject);
            }
        }
    }
}
