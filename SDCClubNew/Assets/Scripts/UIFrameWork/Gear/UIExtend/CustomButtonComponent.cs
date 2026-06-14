using System;
using SDClub.Core;

namespace SDClub.UIFrameWork
{
    // 自定义按钮 (支持按下/抬起/长按回调)
    public class CustomButtonComponent : Entity, IAwake, IDestroy
    {
        public CustomButton Button { get; set; }
        public Action OnClick { get; set; }
        public Action OnPointerDown { get; set; }
        public Action OnPointerUp { get; set; }
    }
}
