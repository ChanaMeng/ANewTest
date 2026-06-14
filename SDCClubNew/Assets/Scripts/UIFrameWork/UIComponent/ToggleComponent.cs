using System;
using SDClub.Core;
using UnityEngine.UI;

namespace SDClub.UIFrameWork
{
    public class ToggleComponent : Entity, IAwake, IDestroy
    {
        public Toggle Toggle { get; set; }
        public Action<bool> OnValueChanged { get; set; }
        public bool IsOn { get => Toggle?.isOn ?? false; set { if (Toggle) Toggle.isOn = value; } }
    }
}
