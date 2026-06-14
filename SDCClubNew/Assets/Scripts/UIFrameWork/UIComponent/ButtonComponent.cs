using System;
using SDClub.Core;
using UnityEngine.UI;

namespace SDClub.UIFrameWork
{
    public class ButtonComponent : Entity, IAwake, IDestroy
    {
        public Button Button { get; set; }
        public Action OnClick { get; set; }
    }
}
