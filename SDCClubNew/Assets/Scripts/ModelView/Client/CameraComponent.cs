using SDClub.Core;
using UnityEngine;

namespace SDClub.ModelView
{
    public class CameraComponent : Entity, IAwake, IUpdate
    {
        public Camera Camera { get; set; }
        public float FieldOfView { get => Camera?.fieldOfView ?? 60; set { if (Camera) Camera.fieldOfView = value; } }
    }
}
