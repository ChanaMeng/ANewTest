using SDClub.Core;
using SDClub.ModelView;
using UnityEngine;

namespace SDClub.HotfixView
{
    // 相机系统
    [EntitySystem]
    public class CameraUpdateSystem : UpdateSystem<CameraComponent>
    {
        protected override void Update(CameraComponent self)
        {
            // 相机跟随逻辑
        }
    }
}
