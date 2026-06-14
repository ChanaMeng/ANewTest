using SDClub.Core;
using SDClub.UIFrameWork;

namespace SDClub.HotfixView
{
    [EntitySystem]
    public class SliderAwakeSystem : AwakeSystem<SliderComponent>
    {
        protected override void Awake(SliderComponent self)
        {
            if (self.Slider != null)
            {
                self.Slider.onValueChanged.AddListener((val) => self.OnValueChanged?.Invoke(val));
            }
        }
    }

    [EntitySystem]
    public class SliderDestroySystem : DestroySystem<SliderComponent>
    {
        protected override void Destroy(SliderComponent self)
        {
            if (self.Slider != null)
            {
                self.Slider.onValueChanged.RemoveAllListeners();
            }
            self.OnValueChanged = null;
        }
    }
}
