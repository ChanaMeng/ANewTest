using SDClub.Core;
using SDClub.UIFrameWork;

namespace SDClub.HotfixView
{
    // Button 组件系统 - 注册点击事件等
    [EntitySystem]
    public class ButtonAwakeSystem : AwakeSystem<ButtonComponent>
    {
        protected override void Awake(ButtonComponent self)
        {
            if (self.Button != null)
            {
                self.Button.onClick.AddListener(() => self.OnClick?.Invoke());
            }
        }
    }

    [EntitySystem]
    public class ButtonDestroySystem : DestroySystem<ButtonComponent>
    {
        protected override void Destroy(ButtonComponent self)
        {
            if (self.Button != null)
            {
                self.Button.onClick.RemoveAllListeners();
            }
            self.OnClick = null;
        }
    }
}
