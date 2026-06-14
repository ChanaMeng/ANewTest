using UnityEngine;

namespace SDClub.Core
{
    [ComponentOf(typeof(Scene))]
    public class JoystickComponent : Entity, IAwake, IUpdate, IDestroy
    {
        public JoystickConfig Config;
        public float Horizontal { get; set; }
        public float Vertical { get; set; }
        public Vector2 Direction { get; set; }
        public float Magnitude { get; set; }
        public bool IsDragging { get; set; }
        public int ActiveFingerId { get; set; } = -1;
    }

    public struct JoystickConfig
    {
        public Vector2 BasePosition;
        public float MaxRadius;
        public bool IsFixed;
    }

    // Joystick events
    public struct OnJoystickDragStart
    {
        public Vector2 Position;
    }

    public struct OnJoystickDrag
    {
        public float Horizontal;
        public float Vertical;
        public Vector2 Direction;
        public float Magnitude;
    }

    public struct OnJoystickDragEnd
    {
    }
}
