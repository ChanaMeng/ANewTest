using SDClub.Core;
using UnityEngine;

namespace SDClub.UIFrameWork
{
    public class CanvasGroupComponent : Entity, IAwake
    {
        public CanvasGroup CanvasGroup { get; set; }
        public float Alpha { get => CanvasGroup?.alpha ?? 1f; set { if (CanvasGroup) CanvasGroup.alpha = value; } }
    }
}
