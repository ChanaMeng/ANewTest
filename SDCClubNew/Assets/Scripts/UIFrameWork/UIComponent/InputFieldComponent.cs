using System;
using SDClub.Core;
using UnityEngine.UI;

namespace SDClub.UIFrameWork
{
    public class InputFieldComponent : Entity, IAwake, IDestroy
    {
        public InputField InputField { get; set; }
        public Action<string> OnValueChanged { get; set; }
        public string Text => InputField?.text;
    }
}
