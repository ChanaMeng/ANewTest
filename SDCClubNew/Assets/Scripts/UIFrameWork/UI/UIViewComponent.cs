using SDClub.Core;
using UnityEngine;

namespace SDClub.UIFrameWork
{
    public class UIViewComponent : Entity, IAwake<string>
    {
        public string BundlePath { get; set; }
        public GameObject UIPrefab { get; set; }
    }
}
