using SDClub.Core;
using SDClub.UIFrameWork;

namespace SDClub.HotfixView
{
    [EntitySystem]
    public class InputAwakeSystem : AwakeSystem<InputFieldComponent>
    {
        protected override void Awake(InputFieldComponent self)
        {
            if (self.InputField != null)
            {
                self.InputField.onValueChanged.AddListener((val) => self.OnValueChanged?.Invoke(val));
            }
        }
    }

    [EntitySystem]
    public class InputDestroySystem : DestroySystem<InputFieldComponent>
    {
        protected override void Destroy(InputFieldComponent self)
        {
            if (self.InputField != null)
            {
                self.InputField.onValueChanged.RemoveAllListeners();
            }
            self.OnValueChanged = null;
        }
    }
}
