using System;
using SDClub.Core;
using UnityEngine.UI;

namespace SDClub.UIFrameWork
{
    public class SliderComponent : Entity, IAwake, IDestroy
    {
        public Slider Slider { get; set; }
        public Action<float> OnValueChanged { get; set; }
        public float Value { get => Slider?.value ?? 0; set { if (Slider) Slider.value = value; } }
    }
}
