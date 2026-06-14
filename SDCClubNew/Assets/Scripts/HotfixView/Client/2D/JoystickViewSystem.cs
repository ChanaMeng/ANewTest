using SDClub.Core;
using UnityEngine;
using UnityEngine.UI;

namespace SDClub.HotfixView.Client
{
    [EntitySystem]
    public class JoystickViewAwakeSystem : AwakeSystem<JoystickComponent>
    {
        protected override void Awake(JoystickComponent self)
        {
            self.Config = new JoystickConfig
            {
                BasePosition = Vector2.zero,
                MaxRadius = 100f,
                IsFixed = true
            };
            self.Horizontal = 0f;
            self.Vertical = 0f;
            self.Direction = Vector2.zero;
            self.Magnitude = 0f;
            self.IsDragging = false;
            self.ActiveFingerId = -1;
        }
    }

    [EntitySystem]
    public class JoystickViewUpdateSystem : UpdateSystem<JoystickComponent>
    {
        protected override void Update(JoystickComponent self)
        {
            if (self.IsDragging && self.ActiveFingerId < 0)
            {
                self.IsDragging = false;
                self.Horizontal = 0f;
                self.Vertical = 0f;
                self.Direction = Vector2.zero;
                self.Magnitude = 0f;
                EventSystem.Instance.Publish(self.IScene as Scene, new OnJoystickDragEnd());
                return;
            }

            for (int i = 0; i < Input.touchCount; i++)
            {
                Touch touch = Input.GetTouch(i);

                if (self.IsDragging)
                {
                    if (touch.fingerId == self.ActiveFingerId)
                    {
                        if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
                        {
                            self.IsDragging = false;
                            self.ActiveFingerId = -1;
                            self.Horizontal = 0f;
                            self.Vertical = 0f;
                            self.Direction = Vector2.zero;
                            self.Magnitude = 0f;
                            EventSystem.Instance.Publish(self.IScene as Scene, new OnJoystickDragEnd());
                            return;
                        }
                        else
                        {
                            UpdateJoystick(self, touch.position);
                        }
                    }
                }
                else
                {
                    if (touch.phase == TouchPhase.Began)
                    {
                        if (IsInJoystickArea(self, touch.position))
                        {
                            self.IsDragging = true;
                            self.ActiveFingerId = touch.fingerId;
                            self.Config.BasePosition = self.Config.IsFixed ? self.Config.BasePosition : touch.position;
                            UpdateJoystick(self, touch.position);
                            EventSystem.Instance.Publish(self.IScene as Scene, new OnJoystickDragStart { Position = touch.position });
                        }
                    }
                }
            }
        }

        private static bool IsInJoystickArea(JoystickComponent self, Vector2 touchPos)
        {
            float radius = self.Config.MaxRadius * 1.5f;
            return Vector2.Distance(touchPos, self.Config.BasePosition) <= radius;
        }

        private static void UpdateJoystick(JoystickComponent self, Vector2 touchPos)
        {
            Vector2 offset = touchPos - self.Config.BasePosition;
            float dist = offset.magnitude;

            if (dist > self.Config.MaxRadius)
            {
                offset = offset.normalized * self.Config.MaxRadius;
                dist = self.Config.MaxRadius;
            }

            float normalized = dist / self.Config.MaxRadius;
            self.Direction = dist > 0.01f ? offset.normalized : Vector2.zero;
            self.Horizontal = self.Direction.x * normalized;
            self.Vertical = self.Direction.y * normalized;
            self.Magnitude = normalized;

            EventSystem.Instance.Publish(self.IScene as Scene, new OnJoystickDrag
            {
                Horizontal = self.Horizontal,
                Vertical = self.Vertical,
                Direction = self.Direction,
                Magnitude = self.Magnitude
            });
        }
    }

    [EntitySystem]
    public class JoystickViewDestroySystem : DestroySystem<JoystickComponent>
    {
        protected override void Destroy(JoystickComponent self)
        {
            self.IsDragging = false;
            self.ActiveFingerId = -1;
        }
    }
}
