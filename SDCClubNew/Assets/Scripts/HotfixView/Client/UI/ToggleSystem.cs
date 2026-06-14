using SDClub.Core;
using SDClub.UIFrameWork;

namespace SDClub.HotfixView
{
    [EntitySystem]
    public class ToggleAwakeSystem : AwakeSystem<ToggleComponent>
    {
        protected override void Awake(ToggleComponent self)
        {
            if (self.Toggle != null)
            {
                self.Toggle.onValueChanged.AddListener((val) => self.OnValueChanged?.Invoke(val));
            }
        }
    }

    [EntitySystem]
    public class ToggleDestroySystem : DestroySystem<ToggleComponent>
    {
        protected override void Destroy(ToggleComponent self)
        {
            if (self.Toggle != null)
            {
                self.Toggle.onValueChanged.RemoveAllListeners();
            }
            self.OnValueChanged = null;
        }
    }
}
