using SDClub.Core;
using UnityEngine;

namespace SDClub.ModelView
{
    public class UnitViewComponent : Entity, IAwake, IDestroy, IUpdate
    {
        public GameObject GameObject { get; set; }
        public Transform Transform => GameObject?.transform;
        public Vector3 Position
        {
            get => Transform?.position ?? Vector3.zero;
            set { if (Transform) Transform.position = value; }
        }
    }
}
