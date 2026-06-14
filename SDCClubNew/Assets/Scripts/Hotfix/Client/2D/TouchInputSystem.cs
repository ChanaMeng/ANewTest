using SDClub.Core;
using UnityEngine;
using System.Collections.Generic;

namespace SDClub.Hotfix.Client
{
    [EntitySystem]
    public class TouchInputAwakeSystem : AwakeSystem<TouchInputComponent>
    {
        protected override void Awake(TouchInputComponent self)
        {
            self.ActiveTouches.Clear();
            self.EndedTouches.Clear();
            self.TrackedTouches.Clear();
            self.ActiveFingerId = -1;
        }
    }

    [EntitySystem]
    public class TouchInputUpdateSystem : UpdateSystem<TouchInputComponent>
    {
        protected override void Update(TouchInputComponent self)
        {
            self.ActiveTouches.Clear();
            self.EndedTouches.Clear();

            for (int i = 0; i < Input.touchCount; i++)
            {
                Touch touch = Input.GetTouch(i);

                TouchData data = new TouchData
                {
                    FingerId = touch.fingerId,
                    Position = touch.position,
                    DeltaPosition = touch.deltaPosition,
                    Phase = touch.phase,
                    Duration = 0f,
                    StartTime = 0f,
                    StartPosition = touch.position,
                    IsDoubleClick = false
                };

                if (self.TrackedTouches.TryGetValue(touch.fingerId, out var tracked))
                {
                    data.StartTime = tracked.StartTime;
                    data.StartPosition = tracked.StartPosition;
                    data.Duration = Time.time - tracked.StartTime;
                }
                else
                {
                    data.StartTime = Time.time;
                    data.StartPosition = touch.position;
                    self.TrackedTouches[touch.fingerId] = data;
                }

                self.ActiveTouches.Add(data);

                switch (touch.phase)
                {
                    case TouchPhase.Began:
                        HandleTouchBegan(self, data);
                        break;
                    case TouchPhase.Moved:
                        HandleTouchMoved(self, data);
                        break;
                    case TouchPhase.Ended:
                        HandleTouchEnded(self, data);
                        self.TrackedTouches.Remove(touch.fingerId);
                        break;
                    case TouchPhase.Canceled:
                        self.TrackedTouches.Remove(touch.fingerId);
                        break;
                }
            }
        }

        private void HandleTouchBegan(TouchInputComponent self, TouchData data)
        {
            self.ActiveFingerId = data.FingerId;
        }

        private float lastTapTime;
        private Vector2 lastTapPosition;
        private const float DoubleTapDistance = 50f;

        private void HandleTouchMoved(TouchInputComponent self, TouchData data)
        {
            // Swipe detection - publish on end for accuracy
        }

        private void HandleTouchEnded(TouchInputComponent self, TouchData data)
        {
            self.EndedTouches.Add(data);
            self.ActiveFingerId = -1;

            // Long press detection
            if (data.Duration >= self.LongPressDuration)
            {
                EventSystem.Instance.Publish(self.IScene as Scene, new OnLongPress { Position = data.Position });
                return;
            }

            // Swipe detection
            float swipeDist = Vector2.Distance(data.StartPosition, data.Position);
            if (swipeDist >= self.SwipeThreshold)
            {
                EventSystem.Instance.Publish(self.IScene as Scene, new OnSwipe
                {
                    Start = data.StartPosition,
                    End = data.Position
                });
                return;
            }

            // Double tap detection
            float timeSinceLastTap = Time.time - lastTapTime;
            float distFromLastTap = Vector2.Distance(lastTapPosition, data.Position);

            if (timeSinceLastTap <= self.DoubleTapThreshold && distFromLastTap <= DoubleTapDistance)
            {
                EventSystem.Instance.Publish(self.IScene as Scene, new OnDoubleTap { Position = data.Position });
                lastTapTime = 0f;
                return;
            }

            // Single tap
            EventSystem.Instance.Publish(self.IScene as Scene, new OnTap { Position = data.Position });
            lastTapTime = Time.time;
            lastTapPosition = data.Position;
        }
    }

    [EntitySystem]
    public class TouchInputDestroySystem : DestroySystem<TouchInputComponent>
    {
        protected override void Destroy(TouchInputComponent self)
        {
            self.ActiveTouches?.Clear();
            self.EndedTouches?.Clear();
            self.TrackedTouches?.Clear();
        }
    }
}
