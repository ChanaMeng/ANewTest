using System.Collections.Generic;
using UnityEngine;

namespace SDClub.Core
{
    [ComponentOf(typeof(Scene))]
    public class TouchInputComponent : Entity, IAwake, IUpdate, IDestroy
    {
        public List<TouchData> ActiveTouches { get; private set; } = new List<TouchData>();
        public List<TouchData> EndedTouches { get; private set; } = new List<TouchData>();
        public Dictionary<int, TouchData> TrackedTouches { get; private set; } = new Dictionary<int, TouchData>();

        // internal state
        public int ActiveFingerId { get; set; } = -1;

        // configuration
        public float DoubleTapThreshold { get; set; } = 0.3f;
        public float LongPressDuration { get; set; } = 0.5f;
        public float SwipeThreshold { get; set; } = 50f;
    }

    public struct TouchData
    {
        public int FingerId;
        public Vector2 Position;
        public Vector2 DeltaPosition;
        public TouchPhase Phase;
        public float Duration;
        public float StartTime;
        public Vector2 StartPosition;
        public bool IsDoubleClick;
    }

    // Gesture events
    public struct OnTap
    {
        public Vector2 Position;
    }

    public struct OnDoubleTap
    {
        public Vector2 Position;
    }

    public struct OnSwipe
    {
        public Vector2 Start;
        public Vector2 End;
    }

    public struct OnLongPress
    {
        public Vector2 Position;
    }
}
